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
                    //RasterBatchAnalysis _rasterBatch = new RasterBatchAnalysis
                    //{
                    //    Title = "Lineament Analysis - Batch Run"
                    //};
                    //_rasterBatch.btnCreate.Content = "Batch Process";
                    //_rasterBatch.InputLayer = InputLayer;
                    //_rasterBatch.thisSpatRef = thisSpatRef;
                    //_rasterBatch.inputDatabase = inputDatabasePath;
                    //_rasterBatch.outputDatabase = outputDatabasePath;
                    //_rasterBatch.Show();
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
    }
}
