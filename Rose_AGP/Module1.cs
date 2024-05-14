using ArcGIS.Core.CIM;
using ArcGIS.Core.Data;
using ArcGIS.Core.Geometry;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Editing;
using ArcGIS.Desktop.Extensions;
using ArcGIS.Desktop.Framework;
using ArcGIS.Desktop.Framework.Contracts;
using ArcGIS.Desktop.Framework.Dialogs;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Layouts;
using ArcGIS.Desktop.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Rose_AGP.Enum;
using ArcGIS.Desktop.Core.Geoprocessing;

namespace Rose_AGP
{
    internal class Module1 : Module
    {
        //Properties
        private Datastore inputDatastore { get; set; }
        private string inputDatabasePath { get; set; }
        private string outputDatabasePath { get; set; }
        private FeatureLayer InputLayer { get; set; }
        public RoseType _RoseType { get; set; }
        public RoseGeom _RoseGeom { get; set; }

        private SpatialReference thisSpatRef { get; set; }

        private static Module1 _this = null;

        const string strTitle = "Fiosrachadh";

        /// <summary>
        /// Retrieve the singleton instance to this module here
        /// </summary>
        public static Module1 Current => _this ??= (Module1)FrameworkApplication.FindModule("Rose_AGP_Module");

        #region Overrides
        /// <summary>
        /// Called by Framework when ArcGIS Pro is closing
        /// </summary>
        /// <returns>False to prevent Pro from closing, otherwise True</returns>
        protected override bool CanUnload()
        {
            //TODO - add your business logic
            //return false to ~cancel~ Application close
            return true;
        }

        #endregion Overrides

        public async Task<string> BatchRun(RoseType analysisType, RoseGeom geomType, bool bRegional)
        {
            string layerCheck = "";

            outputDatabasePath = "";
            inputDatabasePath = "";

            layerCheck = await BatchRunChecks(geomType);

            if (layerCheck != "")
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(layerCheck, strTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return "Error";
            }
            else if (InputLayer == null)
            {
                return "Error";
            }
            else
            {
                if (inputDatastore == null)
                {
                    await SetDataStoreProperty();
                }
            }

            if (InputLayer == null)
                return "";

            // bool bCheck = await CheckSetProjection();
            string message = await CheckSetProjection("");

            if (message != "")
            {
                var question = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, strTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (question == MessageBoxResult.No)
                    return "";

            }

            switch (analysisType)
            {
                case RoseType.Other:
                    RasterBatchAnalysis _rasterBatch = new RasterBatchAnalysis
                    {
                        Title = "Lineament Analysis - Batch Run"
                    };
                    _rasterBatch.btnCreate.Content = "Batch Process";
                    _rasterBatch.InputLayer = InputLayer;
                    _rasterBatch.thisSpatRef = thisSpatRef;
                    _rasterBatch.inputDatabase = inputDatabasePath;
                    _rasterBatch.outputDatabase = outputDatabasePath;
                    _rasterBatch.Show();
                    break;

                case RoseType.Frequency:
                    if (geomType == RoseGeom.Line)
                        _RoseGeom = RoseGeom.Line;
                    else
                        _RoseGeom = RoseGeom.Point;
                    _RoseType = RoseType.Frequency;

                    CreateRoseDigram(bRegional);
                    break;

                case RoseType.Length:
                    _RoseGeom = RoseGeom.Line;
                    _RoseType = RoseType.Length;

                    CreateRoseDigram(bRegional);
                    break;
            }

            return "";
        }

        private async Task<string> BatchRunChecks(RoseGeom geomType)
        {
            if (MapView.Active == null)
                return "Requires active map to continue";

            var layers = MapView.Active.Map.GetLayersAsFlattenedList().Select(l => l.Name);

            if (layers.Count() == 0)
                return "Add a POLYLINE or POINT layer to begin";

            var selectedLayers = MapView.Active.GetSelectedLayers();

            if (selectedLayers.Count() == 0)
                return "Select a POLYLINE or POINT layer to continue";
            else if (selectedLayers.Count() > 1)
                return "Select one layer only";

            Layer selectedLayer = selectedLayers[0] as Layer;

            if (selectedLayer is GroupLayer)
            {
                return "Grouped layers are currently not supported! Coming soon!!";
            }

            if (selectedLayer is FeatureLayer)
            {
                FeatureLayer featLayer = selectedLayer as FeatureLayer;

                //check if any features exist

                int nFeatures = 0;

                await QueuedTask.Run(() =>
                {
                    nFeatures = (int)featLayer.GetFeatureClass().GetCount();
                });

                if (nFeatures == 0)
                    return "There are no input features to process";

                if (geomType == RoseGeom.Other || geomType == RoseGeom.Line)
                {
                    if (featLayer.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline)
                    {
                        InputLayer = selectedLayer as FeatureLayer;
                        // return "";
                    }
                    else
                    {
                        return "Select layer with POLYLINE geometry";
                    }
                }
                else if (geomType == RoseGeom.CellOnly)
                {
                    if (featLayer.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline ||
                       featLayer.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPoint ||
                       featLayer.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryMultipoint)
                    {
                        InputLayer = selectedLayer as FeatureLayer;
                        //return "";
                    }
                    else
                    {
                        return "Select layer with POLYLINE or POINT geometry";
                    }
                }
                else
                {
                    if (featLayer.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPoint ||
                        featLayer.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryMultipoint)
                    {
                        InputLayer = selectedLayer as FeatureLayer;
                        //return "";
                    }
                    else
                    {
                        return "Select layer with POINT geometry";
                    }
                }
            }
            else
                return "Select Feature layer";

            await SetDataStoreProperty();

            return "";
        }

        private async void CreateRoseDigram(bool bRegional)
        {

            //Run actual program now that all prepared. Pass in Feature layer input and Rose Type    
            RunProgram mRun = new RunProgram
            {
                InputLayer = InputLayer,
                _RoseType = _RoseType,
                _RoseGeom = _RoseGeom

            };


            mRun.chkRegional.IsChecked = true;



            if (_RoseGeom == RoseGeom.Point)
            {
                mRun.cboDirection.Visibility = Visibility.Visible;
                mRun.chkAverage.IsEnabled = false;
                mRun.chkAverage.IsChecked = false;
                mRun.lblDirection.Visibility = Visibility.Visible;
                mRun.chkDirection.Visibility = Visibility.Visible;
                mRun.WindowTitle = "Rose Diagram - Frequency from Points using selected layer '" + InputLayer.Name.ToString() + "'";

            }
            else if (_RoseType == RoseType.Length)
            {
                mRun.WindowTitle = "Rose Diagram - Length weighted  using selected layer '" + InputLayer.Name.ToString() + "'";

            }
            else
                mRun.WindowTitle = "Rose Diagram - Frequency of occurrence  using selected layer '" + InputLayer.Name.ToString() + "'";

            if (!bRegional)
            {
                mRun.txtCell.Visibility = Visibility.Visible;
                mRun.lblSubcell.Visibility = Visibility.Visible;
                mRun.chkRegional.IsChecked = false;
                mRun.lblInfo.Visibility = Visibility.Visible;
                mRun.btnCreate.Content = "Create Rose Plots";
            }

            mRun.inputDatabasePath = inputDatabasePath;
            mRun.outputDatabasePath = outputDatabasePath;
            mRun.thisSpatRef = thisSpatRef;
            mRun.Show();

        }


        #region Spatial reference
        private async Task<bool> CheckSetProjection()
        {
            bool bCheck = true;

            thisSpatRef = await FeatureClassQuery.GetSpatialReferenceProp(InputLayer);

            if (thisSpatRef.IsGeographic || thisSpatRef.IsUnknown)
            {
                thisSpatRef = await FeatureClassQuery.GetSpatialReferenceProp();
                bCheck = false;

                if (thisSpatRef.IsGeographic)
                {
                    thisSpatRef = await FeatureClassQuery.SetSpatialReferenceDefault();
                }
            }
            else if (!thisSpatRef.IsProjected)
            {

            }

            return bCheck;
        }

        private async Task<string> CheckSetProjection(string strMessage)
        {
            strMessage = "";

            thisSpatRef = await FeatureClassQuery.GetSpatialReferenceProp(InputLayer);

            if (thisSpatRef.IsGeographic || thisSpatRef.IsUnknown)
            {

                if (thisSpatRef.IsGeographic)
                {
                    strMessage = "Unprojected data. This works much quicker with projected data. Continue with unprojected?. Continue?";
                }
                else if (thisSpatRef.IsUnknown)
                {
                    strMessage = "Spatial reference unknown. Continue?";
                }
            }
            else if (!thisSpatRef.IsProjected)
            {

            }

            return strMessage;
        }
        #endregion


        #region Fishnet
        public async void FishnetClass(bool bRegional)
        {
            try
            {
                string message = "";
                message = await BatchRunChecks(RoseGeom.CellOnly);

                if (message != "")
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(message, strTitle, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                string fishName = await CheckFilename();

                if (fishName == "")
                    return;
                else if (fishName == "SDE not supported")
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Saving to 'Enterprise geodatabase' " +
                        "is currently not supported, sorry!", strTitle, MessageBoxButton.OK, MessageBoxImage.Hand);
                    return;
                }

                //set correct output path
                string databasePath = inputDatabasePath; //default
                if (outputDatabasePath != "")
                    databasePath = outputDatabasePath;

                RoseFactoryPreview _factory = new RoseFactoryPreview(Enum.RoseLineamentAnalysis.Fishnet);

                Envelope CustomEnvelope = await FeatureClassQuery.ReturnExtent(InputLayer); //the default extent

                if (InputLayer.ShapeType == ArcGIS.Core.CIM.esriGeometryType.esriGeometryPolyline)
                    _RoseGeom = RoseGeom.Line;
                else
                    _RoseGeom = RoseGeom.Point;

                if (InputLayer == null)
                    return;

                bool bCheck = await CheckSetProjection();

                if (!bCheck)
                {
                    var question = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Unprojected Layer! This works much quicker with projected " +
                        "data. Continue with unprojected?", strTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (question == MessageBoxResult.No)
                        return;

                }

                int subCellsize = 0;

                if (!bRegional)
                {
                    subCellsize = await SubCellFishnet();

                    if (subCellsize == 0)
                        return;
                }

                bool bCount = false ? true : InputLayer.SelectionCount > 0;

                if (bCount)
                {
                    CustomEnvelope = await FeatureClassQuery.ExtentFromSelectectedInput(InputLayer);
                }
                else
                {
                    CustomEnvelope = await FeatureClassQuery.ReturnExtent(InputLayer); //the default extent
                }

                FlapParameters _parameters = await _factory.PrepareInputForProcessing(InputLayer, CustomEnvelope, subCellsize, 0,
                    bCount, _RoseGeom, bRegional, "");

                bool bExists = await FeatureClassQuery.FeatureclassExists(databasePath, fishName + "_fish");
                FeatureClass fishFC = await _factory.CreateFeatureClass("_fish", fishName, bExists,
                    databasePath, InputLayer, false, _RoseGeom, thisSpatRef);

                await _factory.SaveAsVectorFeatures(fishFC, thisSpatRef, _parameters, true);

                await AddLayerToMap(fishName + "_fish");

                return;

            }
            catch (Exception ex)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Message, strTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

        }

        private async Task<int> SubCellFishnet()
        {
            string strCellsize = "0";

            FishnetSize cellsize = new FishnetSize();

            cellsize.ShowDialog();

            if (cellsize.DialogResult.HasValue)
            {
                if (cellsize.Cellsize != null)
                {
                    strCellsize = cellsize.Cellsize;
                }
                else
                    return 0;
            }
            else if (cellsize.Cellsize == "")
                return 0;

            //Check value is valid as we know it is numeric but not if it is integer or decimal

            int subCellsize = 0;
            int number;
            bool result = Int32.TryParse(strCellsize, out number);
            if (result)
            {
                subCellsize = Convert.ToInt32(strCellsize);
            }
            else
            {
                subCellsize = Convert.ToInt32(Math.Floor(Convert.ToDouble(strCellsize)));
            }

            return subCellsize;
        }
        #endregion

        #region Manage database
        public void SetDefaultDatabase()
        {
            OpenItemDialog openDialog = new OpenItemDialog()
            {
                Title = "Set Output Database",
                Filter = ItemFilters.Geodatabases,
                //DefaultExt = ".gdb",
                InitialLocation = CoreModule.CurrentProject.DefaultGeodatabasePath
            };

            string filePath = "";
            if (openDialog.ShowDialog() == true)
            {
                QueuedTask.Run(() =>
                {
                    filePath = "";
                    foreach (Item i in openDialog.Items)
                    {
                        filePath = i.PhysicalPath;
                    }

                    if (CoreModule.CurrentProject.DefaultGeodatabasePath != filePath)
                    {
                        CoreModule.CurrentProject.SetDefaultGeoDatabasePath(filePath);
                        outputDatabasePath = filePath;
                    }

                });
            }
        }

        public async void CreateDefaultDatabase()
        {
            SaveItemDialog saveDialog = new SaveItemDialog()
            {
                Title = "Create File Geoatabase",
                Filter = ItemFilters.Geodatabases,
                DefaultExt = ".gdb",
                InitialLocation = Environment.GetEnvironmentVariable("home") + "\\" // CoreModule.CurrentProject.DefaultGeodatabasePath

            };

            outputDatabasePath = "";
            string workspace = "";

            if (saveDialog.ShowDialog() == true)
            {
                await QueuedTask.Run(() =>
                {
                    string filePath = saveDialog.FilePath;

                    int last = filePath.LastIndexOf("\\");

                    workspace = filePath.Substring(0, last);

                    outputDatabasePath = filePath.Substring(last + 1, filePath.Length - (workspace.Length + 1));

                    List<object> arguments = new List<object>
                {
                // store the results in the workspace
                workspace,

                outputDatabasePath,
                };

                    IGPResult result = Geoprocessing.ExecuteToolAsync("CreateFileGDB_management", Geoprocessing.MakeValueArray(arguments.ToArray())).Result;

                    Geoprocessing.ShowMessageBox(result.Messages, "New Geoatabase", GPMessageBoxStyle.Default, strTitle);

                    if (result.ErrorMessages.Count() == 0)
                    {
                        CoreModule.CurrentProject.SetDefaultGeoDatabasePath(workspace + "\\" + outputDatabasePath);
                    }

                });
            }

        }

        #endregion

        #region Miscellaneous
        private async Task<bool> AddLayerToMap(string fileName)
        {
            string databasePath = inputDatabasePath;
            if (outputDatabasePath != "")
                databasePath = outputDatabasePath;

            var layers = MapView.Active.Map.GetLayersAsFlattenedList().Where(l => l.Name == fileName).Count();

            if (layers > 0) //then already added
                return true;

            string url = databasePath + "\\" + fileName;  //FeatureClass of a FileGeodatabase

            Uri uri = new Uri(url);
            await QueuedTask.Run(() => LayerFactory.Instance.CreateLayer(uri, MapView.Active.Map));

            return true;
        }

        private async Task<string> CheckFilename()
        {
            string fishName = "";

            SaveItemDialog saveDialog = new SaveItemDialog()
            {
                Title = "Save Fishnet Extent As...",
                Filter = ItemFilters.FeatureClasses_All,
                OverwritePrompt = true,
                InitialLocation = CoreModule.CurrentProject.DefaultGeodatabasePath
            };

            if (saveDialog.ShowDialog() == true)
            {
                if (saveDialog.FilePath == "")
                    return "Empty path";

                string filePath = saveDialog.FilePath;
                int index = filePath.IndexOf(".gdb");

                if (index != -1)
                {


                    outputDatabasePath = filePath.Substring(0, index + 4);
                    int start = outputDatabasePath.Length + 1;
                    int length = filePath.Length;
                    int nameLength = length - start;

                    fishName = filePath.Substring(start, nameLength);

                    await CheckForSpecialCharacters(fishName);
                }
                else
                {
                    fishName = "SDE not supported";
                }

            }
            else
            {
                fishName = "";
            }

            return fishName;
        }

        private async Task<string> CheckForSpecialCharacters(string name)
        {
            string validName = CheckStrings.Checker(name, strTitle);

            if (validName == "")
            {
                return "";
            }
            else
            {
                validName = CheckStrings.CheckFirstLetter(validName, strTitle);
                if (validName == "")
                {
                    return "";
                }

            }

            return validName;
        }

        private async Task<bool> SetDataStoreProperty()
        {
            await QueuedTask.Run(() =>
            {
                var table = InputLayer.GetTable();
                inputDatastore = table.GetDatastore();
                var workspaceNameDef = inputDatastore.GetConnectionString();
                inputDatabasePath = workspaceNameDef.Split('=')[1];
            });

            outputDatabasePath = CoreModule.CurrentProject.DefaultGeodatabasePath;

            if (inputDatabasePath == "")
                return false;
            else
                return true;
        }

        #endregion

        #region Raster
        public async Task<string> RasterModule(RasterLineamentAnalysis analysisType)
        {
            outputDatabasePath = "";
            inputDatabasePath = "";

            RasterProgram _rasterProgram = new RasterProgram();

            try
            {
                string layerCheck = await BatchRunChecks(RoseGeom.Other);

                if (layerCheck != "")
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(layerCheck, strTitle, MessageBoxButton.OK, MessageBoxImage.Stop);
                    return "";
                }
                else
                {
                    //setup input Datastore
                    //setup path

                    if (inputDatastore == null)
                    {
                        await SetDataStoreProperty();
                    }

                    switch (analysisType)
                    {
                        case Enum.RasterLineamentAnalysis.RelativeEntropy:
                            _rasterProgram.Title = "Raster Lineament Analysis - Relative Entropy";
                            _rasterProgram.btnCreate.Content = "Compute Relative Entropy";
                            _rasterProgram._analysis = RasterLineamentAnalysis.RelativeEntropy;
                            break;
                        case Enum.RasterLineamentAnalysis.DensityLength:
                            _rasterProgram.Title = "Raster Lineament Analysis - Density Length";
                            _rasterProgram.btnCreate.Content = "Compute Density Length";
                            _rasterProgram.lblFrom.Visibility = Visibility.Visible;
                            _rasterProgram.lblTo.Visibility = Visibility.Visible;
                            _rasterProgram.lblLen.Visibility = Visibility.Visible;
                            _rasterProgram.txtFrom.Visibility = Visibility.Visible;
                            _rasterProgram.txtTo.Visibility = Visibility.Visible;
                            _rasterProgram.txtInterval.Visibility = Visibility.Hidden;
                            _rasterProgram.lblInterval.Visibility = Visibility.Hidden;
                            _rasterProgram._analysis = RasterLineamentAnalysis.DensityLength;
                            break;
                        case Enum.RasterLineamentAnalysis.DensityFrequency:
                            _rasterProgram.Title = "Raster Lineament Analysis - Density Frequency";
                            _rasterProgram.btnCreate.Content = "Compute Density Frequency";
                            _rasterProgram.lblFrom.Visibility = Visibility.Visible;
                            _rasterProgram.lblTo.Visibility = Visibility.Visible;
                            _rasterProgram.lblLen.Visibility = Visibility.Visible;
                            _rasterProgram.txtFrom.Visibility = Visibility.Visible;
                            _rasterProgram.txtTo.Visibility = Visibility.Visible;
                            _rasterProgram.txtInterval.Visibility = Visibility.Hidden;
                            _rasterProgram.lblInterval.Visibility = Visibility.Hidden;
                            _rasterProgram._analysis = RasterLineamentAnalysis.DensityFrequency;
                            break;
                        case Enum.RasterLineamentAnalysis.GroupDominanceFrequency:
                            _rasterProgram.Title = "Raster Lineament Analysis - Group Dominance Frequency";
                            _rasterProgram.btnCreate.Content = "Compute Group Dominance Frequency";
                            _rasterProgram.lblFrom.Visibility = Visibility.Visible;
                            _rasterProgram.lblTo.Visibility = Visibility.Visible;
                            _rasterProgram.txtFrom.Visibility = Visibility.Visible;
                            _rasterProgram.txtTo.Visibility = Visibility.Visible;
                            _rasterProgram.lblLen.Visibility = Visibility.Visible;
                            _rasterProgram.txtInterval.Visibility = Visibility.Hidden;
                            _rasterProgram.lblInterval.Visibility = Visibility.Hidden;
                            _rasterProgram._analysis = RasterLineamentAnalysis.GroupDominanceFrequency;
                            break;
                        case Enum.RasterLineamentAnalysis.GroupDominanceLength:
                            _rasterProgram.Title = "Raster Lineament Analysis - Group Dominance Length";
                            _rasterProgram.btnCreate.Content = "Compute Group Dominance Length";
                            _rasterProgram.lblFrom.Visibility = Visibility.Visible;
                            _rasterProgram.lblTo.Visibility = Visibility.Visible;
                            _rasterProgram.txtFrom.Visibility = Visibility.Visible;
                            _rasterProgram.txtTo.Visibility = Visibility.Visible;
                            _rasterProgram.lblLen.Visibility = Visibility.Visible;
                            _rasterProgram.txtInterval.Visibility = Visibility.Hidden;
                            _rasterProgram.lblInterval.Visibility = Visibility.Hidden;
                            _rasterProgram._analysis = RasterLineamentAnalysis.GroupDominanceLength;
                            break;
                        case Enum.RasterLineamentAnalysis.GroupMeansFrequency:
                            _rasterProgram.Title = "Raster Lineament Analysis - Group Mean Frequency";
                            _rasterProgram.btnCreate.Content = "Compute Group Mean Frequency";
                            _rasterProgram.lblFrom.Visibility = Visibility.Visible;
                            _rasterProgram.lblTo.Visibility = Visibility.Visible;
                            _rasterProgram.txtFrom.Visibility = Visibility.Visible;
                            _rasterProgram.txtTo.Visibility = Visibility.Visible;
                            _rasterProgram.lblLen.Visibility = Visibility.Visible;
                            _rasterProgram.txtInterval.Visibility = Visibility.Hidden;
                            _rasterProgram.lblInterval.Visibility = Visibility.Hidden;
                            _rasterProgram._analysis = RasterLineamentAnalysis.GroupMeansFrequency;
                            break;
                        case Enum.RasterLineamentAnalysis.GroupMeansLength:
                            _rasterProgram.Title = "Raster Lineament Analysis - Group Mean Length";
                            _rasterProgram.btnCreate.Content = "Compute Group Mean Length";
                            _rasterProgram.lblFrom.Visibility = Visibility.Visible;
                            _rasterProgram.lblTo.Visibility = Visibility.Visible;
                            _rasterProgram.txtFrom.Visibility = Visibility.Visible;
                            _rasterProgram.txtTo.Visibility = Visibility.Visible;
                            _rasterProgram.lblLen.Visibility = Visibility.Visible;
                            _rasterProgram.txtInterval.Visibility = Visibility.Hidden;
                            _rasterProgram.lblInterval.Visibility = Visibility.Hidden;
                            _rasterProgram._analysis = RasterLineamentAnalysis.GroupMeansLength;
                            break;
                        default:
                            break;
                    }

                    if (InputLayer == null)
                        return "";


                    _rasterProgram.inputDatabase = inputDatabasePath;
                    _rasterProgram.outputDatabase = outputDatabasePath;

                    bool bCheck = await CheckSetProjection();

                    if (!bCheck)
                    {
                        var question = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Unprojected data. This works much quicker with projected " +
                            "data. Continue with unprojected?", strTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (question == MessageBoxResult.No)
                            return "";

                    }

                    _rasterProgram.thisSpatRef = thisSpatRef;
                    _rasterProgram.InputLayer = InputLayer;
                    _rasterProgram.Show();
                }
            }
            catch (Exception ex)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Message, strTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return ex.Message;
            }

            return "";
        }

        public async Task<string> MovingStatisticsModule(RasterLineamentAnalysis analysisType)
        {
            outputDatabasePath = "";
            inputDatabasePath = "";

            MovingStatistics _rasterProgram = new MovingStatistics();

            try
            {
                string layerCheck = await BatchRunChecks(RoseGeom.Other);

                if (layerCheck != "")
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(layerCheck, strTitle, MessageBoxButton.OK, MessageBoxImage.Stop);
                    return "";
                }
                else
                {
                    switch (analysisType)
                    {
                        case Enum.RasterLineamentAnalysis.RelativeEntropy:
                            _rasterProgram.Title = "Raster Lineament Analysis - Relative Entropy";
                            _rasterProgram.btnAccept.Content = "Compute Relative Entropy";
                            _rasterProgram._analysis = RasterLineamentAnalysis.RelativeEntropy;
                            _rasterProgram.txtFrom.IsEnabled = false;
                            _rasterProgram.txtTo.IsEnabled = false;
                            break;
                        case Enum.RasterLineamentAnalysis.DensityLength:
                            _rasterProgram.Title = "Raster Lineament Analysis - Density Length";
                            _rasterProgram.btnAccept.Content = "Compute Density Length";
                            _rasterProgram.lblFrom.Visibility = Visibility.Visible;
                            _rasterProgram.lblTo.Visibility = Visibility.Visible;
                            _rasterProgram.lblLen.Visibility = Visibility.Visible;
                            _rasterProgram.txtFrom.Visibility = Visibility.Visible;
                            _rasterProgram.txtTo.Visibility = Visibility.Visible;
                            _rasterProgram.txtInterval.Visibility = Visibility.Hidden;
                            _rasterProgram.lblInterval.Visibility = Visibility.Hidden;
                            _rasterProgram._analysis = RasterLineamentAnalysis.DensityLength;
                            break;
                        case Enum.RasterLineamentAnalysis.DensityFrequency:
                            _rasterProgram.Title = "Raster Lineament Analysis - Density Frequency";
                            _rasterProgram.btnAccept.Content = "Compute Density Frequency";
                            _rasterProgram.lblFrom.Visibility = Visibility.Visible;
                            _rasterProgram.lblTo.Visibility = Visibility.Visible;
                            _rasterProgram.lblLen.Visibility = Visibility.Visible;
                            _rasterProgram.txtFrom.Visibility = Visibility.Visible;
                            _rasterProgram.txtTo.Visibility = Visibility.Visible;
                            _rasterProgram.txtInterval.Visibility = Visibility.Hidden;
                            _rasterProgram.lblInterval.Visibility = Visibility.Hidden;
                            _rasterProgram._analysis = RasterLineamentAnalysis.DensityFrequency;
                            break;
                        case Enum.RasterLineamentAnalysis.GroupDominanceFrequency:
                            _rasterProgram.Title = "Raster Lineament Analysis - Group Dominance Frequency";
                            _rasterProgram.btnAccept.Content = "Compute Group Dominance Frequency";
                            _rasterProgram.lblFrom.Visibility = Visibility.Visible;
                            _rasterProgram.lblTo.Visibility = Visibility.Visible;
                            _rasterProgram.txtFrom.Visibility = Visibility.Visible;
                            _rasterProgram.txtTo.Visibility = Visibility.Visible;
                            _rasterProgram.lblLen.Visibility = Visibility.Visible;
                            _rasterProgram.txtInterval.Visibility = Visibility.Hidden;
                            _rasterProgram.lblInterval.Visibility = Visibility.Hidden;
                            _rasterProgram._analysis = RasterLineamentAnalysis.GroupDominanceFrequency;
                            break;
                        case Enum.RasterLineamentAnalysis.GroupDominanceLength:
                            _rasterProgram.Title = "Raster Lineament Analysis - Group Dominance Length";
                            _rasterProgram.btnAccept.Content = "Compute Group Dominance Length";
                            _rasterProgram.lblFrom.Visibility = Visibility.Visible;
                            _rasterProgram.lblTo.Visibility = Visibility.Visible;
                            _rasterProgram.txtFrom.Visibility = Visibility.Visible;
                            _rasterProgram.txtTo.Visibility = Visibility.Visible;
                            _rasterProgram.lblLen.Visibility = Visibility.Visible;
                            _rasterProgram.txtInterval.Visibility = Visibility.Hidden;
                            _rasterProgram.lblInterval.Visibility = Visibility.Hidden;
                            _rasterProgram._analysis = RasterLineamentAnalysis.GroupDominanceLength;
                            break;
                        case Enum.RasterLineamentAnalysis.GroupMeansFrequency:
                            _rasterProgram.Title = "Raster Lineament Analysis - Group Mean Frequency";
                            _rasterProgram.btnAccept.Content = "Compute Group Mean Frequency";
                            _rasterProgram.lblFrom.Visibility = Visibility.Visible;
                            _rasterProgram.lblTo.Visibility = Visibility.Visible;
                            _rasterProgram.txtFrom.Visibility = Visibility.Visible;
                            _rasterProgram.txtTo.Visibility = Visibility.Visible;
                            _rasterProgram.lblLen.Visibility = Visibility.Visible;
                            _rasterProgram.txtInterval.Visibility = Visibility.Hidden;
                            _rasterProgram.lblInterval.Visibility = Visibility.Hidden;
                            _rasterProgram._analysis = RasterLineamentAnalysis.GroupMeansFrequency;
                            break;
                        case Enum.RasterLineamentAnalysis.GroupMeansLength:
                            _rasterProgram.Title = "Raster Lineament Analysis - Group Mean Length";
                            _rasterProgram.btnAccept.Content = "Compute Group Mean Length";
                            _rasterProgram.lblFrom.Visibility = Visibility.Visible;
                            _rasterProgram.lblTo.Visibility = Visibility.Visible;
                            _rasterProgram.txtFrom.Visibility = Visibility.Visible;
                            _rasterProgram.txtTo.Visibility = Visibility.Visible;
                            _rasterProgram.lblLen.Visibility = Visibility.Visible;
                            _rasterProgram.txtInterval.Visibility = Visibility.Hidden;
                            _rasterProgram.lblInterval.Visibility = Visibility.Hidden;
                            _rasterProgram._analysis = RasterLineamentAnalysis.GroupMeansLength;
                            break;
                        default:
                            break;
                    }

                    if (InputLayer == null)
                        return "";

                    _rasterProgram.inputDatabasePath = inputDatabasePath;
                    _rasterProgram.outputDatabasePath = outputDatabasePath;

                    bool bCheck = await CheckSetProjection();

                    if (!bCheck)
                    {
                        var question = ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Unprojected data. This works much quicker with projected " +
                            "data. Continue with unprojected?", strTitle, MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (question == MessageBoxResult.No)
                            return "";

                    }

                    _rasterProgram.thisSpatRef = thisSpatRef;
                    _rasterProgram.InputLayer = InputLayer;
                    _rasterProgram.Show();
                }
            }
            catch (Exception ex)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Message, strTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return ex.Message;
            }

            return "";
        }

        #endregion
    }
}
