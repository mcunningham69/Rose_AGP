using ArcGIS.Core.Data;
using ArcGIS.Desktop.Mapping;
using Rose_AGP.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Geometry;
using Rose_AGP.Raster;
using Microsoft.VisualBasic;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Core.Data.Raster;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using System.Windows;

namespace Rose_AGP
{

    public class RasterAnalysis
    {
        public const string strTitle = "SRK Consulting";
        public const string default_pixel_type = "8_BIT_UNSIGNED";
        public string inputDatabasePath { get; set; }
        public string outputDatabasePath { get; set; }
        public FeatureLayer InputLayer { get; set; }
        public RasterLineamentAnalysis _LineamentAnalysis { get; set; }
        public Envelope CustomEnvelope { get; set; }
        public FeatureClass RasterAsPoly { get; set; }
        public string RasterName { get; set; }
        public bool SelectedFeatures { get; set; }

        #region Input Integers
        //TODO check values before setting property values
        private int _interval;
        public int Interval
        {
            get { return _interval; }
            set
            {
                if (180 % value == 0)
                    _interval = value;
                else
                    throw new Exception("Interval must be divisible into 180");
            }
        }

        private int _cellsize;
        public int Cellsize
        {
            get { return _cellsize; }

            set
            {

                _cellsize = value;
            }
        }

        private int _rangefrom;
        public int RangeFrom
        {
            get { return _rangefrom; }

            set
            {
                if (_LineamentAnalysis != RasterLineamentAnalysis.RelativeEntropy)
                {
                    if (value <= 175)
                    {

                        if (RangeTo > value)
                            _rangefrom = value;
                        else
                            throw new Exception("Range From cannot be greater than Range To");
                    }
                    else
                        throw new Exception("Range cannot be greater than 175");
                }
                else

                    _rangefrom = value;

            }
        }

        private int _rangeto;
        public int RangeTo
        {
            get { return _rangeto; }

            set
            {
                if (_LineamentAnalysis != RasterLineamentAnalysis.RelativeEntropy)
                {
                    if (value > 5)
                    {
                        if (value > RangeFrom)
                        {
                            _rangeto = value;
                        }
                        else
                            throw new Exception("Range To cannot be less than Range From");
                    }
                    else
                        throw new Exception("Range cannot be less than 5");
                }
                else _rangeto = value;

            }
        }
        #endregion

        #region Raster Analysis
        public async Task<string> CreateRasterGrid(bool setupSettings)
        {
            string DatabasePath = await SetupEnvironment();

            bool bLayer = false;

            if (setupSettings)
            {
                bLayer = await RasterExists();

                if (bLayer)
                    return "Grid already exsits";
            }

            await Progressor_NonCancelable();

            string suffix = "";

            switch (_LineamentAnalysis)
            {
                case RasterLineamentAnalysis.RelativeEntropy:
                    suffix = "_ent";
                    bLayer = await RelativeEntropy(suffix, default_pixel_type, true, DatabasePath); //no range values required
                    break;

                case RasterLineamentAnalysis.DensityLength:
                    suffix = "_cdl";
                    bLayer = await DensityLength(suffix, default_pixel_type, true, DatabasePath);
                    break;

                case RasterLineamentAnalysis.DensityFrequency:
                    suffix = "_cdf";
                    bLayer = await DensityFrequency(suffix, default_pixel_type, true, DatabasePath);
                    break;

                case RasterLineamentAnalysis.GroupDominanceFrequency:
                    suffix = "_gdf";
                    bLayer = await GroupDominanceFrequency(suffix, default_pixel_type, true, DatabasePath);
                    break;

                case RasterLineamentAnalysis.GroupDominanceLength:
                    suffix = "_gdl";
                    bLayer = await GroupDominanceLength(suffix, default_pixel_type, true, DatabasePath);
                    break;

                case RasterLineamentAnalysis.GroupMeansFrequency:
                    suffix = "_gmf";
                    bLayer = await GroupMeansFrequency(suffix, default_pixel_type, true, DatabasePath);
                    break;

                case RasterLineamentAnalysis.GroupMeansLength:
                    suffix = "_gml";
                    bLayer = await GroupMeansLength(suffix, default_pixel_type, true, DatabasePath);
                    break;

                default:
                    throw new Exception("Not yet implemented");
            }

            if (bLayer)
            {
                AddRasterLayerToMap(RasterName + suffix + "r");

                if (!bRasterOnly)
                {
                    AddLayerToMap(RasterName + suffix + "f");
                }
            }

            return "";

        }

        public async Task<string> SetupEnvironment()
        {
            //check if input and output database are the same
            string DatabasePath = inputDatabasePath;
            if (outputDatabasePath != "")
                DatabasePath = outputDatabasePath;

            //set up custom extent
            CustomEnvelope = await FeatureClassQuery.ReturnExtent(InputLayer); //the default extent

            //check for selected features
            if (SelectedFeatures)
            {
                bool bCount = false ? true : InputLayer.SelectionCount > 0;

                if (bCount)
                {
                    SelectedFeatures = true;
                    CustomEnvelope = await FeatureClassQuery.ExtentFromSelectectedInput(InputLayer);
                }
                else
                    SelectedFeatures = false; //turn it false despite the checkbox if there are no features selected
            }

            return DatabasePath;
        }

        private async Task<bool> RasterExists()
        {
            string strTemp = System.Environment.GetEnvironmentVariable("temp");

            //check if raster already exists
            bool bExist = await RasterClassManagement.CheckIfTempRasterExists(strTemp, RasterName + "_temp.tif"); //checks for temp grid first

            if (bExist)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Raster layer already exists", strTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return true;
            }

            return false;
        }

        #region Operations
        public async virtual Task<bool> RelativeEntropy(string suffix, string pixel_type, bool bInt, string DatabasePath)
        {
            string strLayer = "Relative Entropy";

            //check if raster already exists
            bool bExist = await RasterClassManagement.CheckIfRasterExists(DatabasePath, RasterName + suffix + "r"); //checks for temp grid first

            //check if already exists
            if (bExist)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(strLayer + " layer already exists!",
                    strTitle, MessageBoxButton.OK, MessageBoxImage.Hand);
                return false;
            }

            _rasterProgram.Title = "Calculating " + strLayer;

            if (CustomEnvelope == null)
                CustomEnvelope = await FeatureClassQuery.ReturnExtent(InputLayer);

            if (RasterPreview == null)
                RasterPreview = new RasterFactoryPreview(RasterLineamentAnalysis.RelativeEntropy);
            else
                RasterPreview.SetRasterAnalysisDataset(RasterLineamentAnalysis.RelativeEntropy);

            FlapParameters flapParameters = await RasterPreview.PrepareInputForProcessing(InputLayer,
                CustomEnvelope, Cellsize, Interval, SelectedFeatures, RangeFrom, RangeTo, Enum.RoseGeom.Other, false, "");

            RasterPreview.CalculateGridValues(flapParameters);

            _rasterProgram.Title = "Creating Output";

            await RasterPreview.CreateRasterDataset(pixel_type, RasterName + "_temp", Cellsize.ToString(), thisSpatRef, CustomEnvelope); //template

            string strPath = Environment.GetEnvironmentVariable("temp");

            RasterDataset _rasterDS = await RasterPreview.OpenRasterDataset(strPath, RasterName + "_temp.tif", true, InputLayer);

            await RasterPreview.SaveToNewRaster(_rasterDS, RasterName + suffix + "r", Cellsize, thisSpatRef,
                CustomEnvelope, InputLayer);

            _rasterDS.Dispose();

            RasterDataset _rasterSave = await RasterPreview.OpenRasterDataset(DatabasePath, RasterName + suffix + "r", false, InputLayer);

            RasterPreview.WriteRasterValuesToGrid(_rasterSave, flapParameters, bInt);

            if (!bRasterOnly)
            {
                string FClassType = "POLYGON";
                if (!bPolygon)
                    FClassType = "POINT";

                RasterAsPoly = await RasterPreview.CreatePolyFeatClass(suffix + "f", RasterName, bExist,
                    DatabasePath, InputLayer, thisSpatRef, FClassType, false);

                _rasterProgram.Title = "Saving Output";

                string message = await RasterPreview.SaveAsVectorFeatures(RasterAsPoly, thisSpatRef, flapParameters, FClassType);
            }

            return true;
        }
        public async virtual Task<bool> DensityLength(string suffix, string pixel_type, bool bInt, string DatabasePath)
        {
            //check if raster already exists
            bool bExist = await RasterClassManagement.CheckIfRasterExists(DatabasePath, RasterName + suffix + "r"); //checks for temp grid first

            string strLayer = "Density Length";

            if (bExist)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(strLayer + " layer already exists", strTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            _rasterProgram.Title = "Calculating " + strLayer;
            bool bCheck = true;

            if (RasterPreview == null)
                RasterPreview = new RasterFactoryPreview(RasterLineamentAnalysis.DensityLength);
            else
                RasterPreview.SetRasterAnalysisDataset(RasterLineamentAnalysis.DensityLength);

            FlapParameters flapParameters = await RasterPreview.PrepareInputForProcessing(InputLayer,
                CustomEnvelope, Cellsize, Interval, SelectedFeatures, RangeFrom, RangeTo, Enum.RoseGeom.Other, false, "");

            RasterPreview.CalculateGridValues(flapParameters);

            _rasterProgram.Title = "Creating Output";

            await RasterPreview.CreateRasterDataset(pixel_type, RasterName + "_temp", Cellsize.ToString(), thisSpatRef, CustomEnvelope); //template

            string strPath = Environment.GetEnvironmentVariable("temp");

            RasterDataset _rasterDS = await RasterPreview.OpenRasterDataset(strPath, RasterName + "_temp.tif", true, InputLayer);

            await RasterPreview.SaveToNewRaster(_rasterDS, RasterName + suffix + "r", Cellsize, thisSpatRef,
                CustomEnvelope, InputLayer);

            RasterDataset _rasterSave = await RasterPreview.OpenRasterDataset(DatabasePath, RasterName + suffix + "r", false, InputLayer);

            RasterPreview.WriteRasterValuesToGrid(_rasterSave, flapParameters, bInt);

            if (!bRasterOnly)
            {
                string FClassType = "POLYGON";
                if (!bPolygon)
                    FClassType = "POINT";

                RasterAsPoly = await RasterPreview.CreatePolyFeatClass(suffix + "f", RasterName, bExist,
                    DatabasePath, InputLayer, thisSpatRef, FClassType, false);

                _rasterProgram.Title = "Saving Output";

                string message = await RasterPreview.SaveAsVectorFeatures(RasterAsPoly, thisSpatRef, flapParameters, FClassType);
            }

            return bCheck;
        }
        public async virtual Task<bool> DensityFrequency(string suffix, string pixel_type, bool bInt, string DatabasePath)
        {
            bool bCheck = true;
            //check if raster already exists
            bool bExist = await RasterClassManagement.CheckIfRasterExists(DatabasePath, RasterName + suffix + "r"); //checks for temp grid first

            string strLayer = "Density Frequency";

            if (bExist)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(strLayer + " layer already exists", strTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            _rasterProgram.Title = "Calculating " + strLayer;

            if (CustomEnvelope == null)
                CustomEnvelope = await FeatureClassQuery.ReturnExtent(InputLayer);

            if (RasterPreview == null)
                RasterPreview = new RasterFactoryPreview(RasterLineamentAnalysis.DensityFrequency);
            else
                RasterPreview.SetRasterAnalysisDataset(RasterLineamentAnalysis.DensityFrequency);

            FlapParameters parameters = await RasterPreview.PrepareInputForProcessing(InputLayer,
               CustomEnvelope, Cellsize, Interval, SelectedFeatures, RangeFrom, RangeTo, Enum.RoseGeom.Other, false, "");

            RasterPreview.CalculateGridValues(parameters);

            bool bExists = false;

            _rasterProgram.Title = "Creating Output";

            await RasterPreview.CreateRasterDataset(pixel_type, RasterName + "_temp", Cellsize.ToString(), thisSpatRef, CustomEnvelope); //template

            string strPath = Environment.GetEnvironmentVariable("temp");

            RasterDataset _rasterDS = await RasterPreview.OpenRasterDataset(strPath, RasterName + "_temp.tif", true, InputLayer);

            await RasterPreview.SaveToNewRaster(_rasterDS, RasterName + suffix + "r", Cellsize, thisSpatRef,
                CustomEnvelope, InputLayer);

            RasterDataset _rasterSave = await RasterPreview.OpenRasterDataset(DatabasePath, RasterName + suffix + "r", false, InputLayer);

            RasterPreview.WriteRasterValuesToGrid(_rasterSave, parameters, bInt);

            if (!bRasterOnly)
            {
                string FClassType = "POLYGON";
                if (!bPolygon)
                    FClassType = "POINT";

                RasterAsPoly = await RasterPreview.CreatePolyFeatClass(suffix + "f", RasterName, bExists,
                    DatabasePath, InputLayer, thisSpatRef, FClassType, false);

                _rasterProgram.Title = "Saving Output";

                string message = await RasterPreview.SaveAsVectorFeatures(RasterAsPoly, thisSpatRef, parameters, FClassType);
            }

            return bCheck;
        }
        public async virtual Task<bool> GroupDominanceFrequency(string suffix, string pixel_type, bool bInt, string DatabasePath)
        {
            bool bCheck = true;
            //check if raster already exists
            bool bExist = await RasterClassManagement.CheckIfRasterExists(DatabasePath, RasterName + suffix + "r"); //checks for temp grid first

            string strLayer = "Group Dominance Frequency";

            if (bExist)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(strLayer + " layer already exists", strTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            _rasterProgram.Title = "Calculating " + strLayer;

            if (CustomEnvelope == null)
                CustomEnvelope = await FeatureClassQuery.ReturnExtent(InputLayer);

            if (RasterPreview == null)
                RasterPreview = new RasterFactoryPreview(RasterLineamentAnalysis.GroupDominanceFrequency);
            else
                RasterPreview.SetRasterAnalysisDataset(RasterLineamentAnalysis.GroupDominanceFrequency);

            FlapParameters parameters = await RasterPreview.PrepareInputForProcessing(InputLayer,
               CustomEnvelope, Cellsize, Interval, SelectedFeatures, RangeFrom, RangeTo, Enum.RoseGeom.Other, false, "");

            RasterPreview.CalculateGridValues(parameters);

            bool bExists = false;

            _rasterProgram.Title = "Creating Output";

            await RasterPreview.CreateRasterDataset(pixel_type, RasterName + "_temp", Cellsize.ToString(), thisSpatRef, CustomEnvelope); //template

            string strPath = Environment.GetEnvironmentVariable("temp");

            RasterDataset _rasterDS = await RasterPreview.OpenRasterDataset(strPath, RasterName + "_temp.tif", true, InputLayer);

            await RasterPreview.SaveToNewRaster(_rasterDS, RasterName + suffix + "r", Cellsize, thisSpatRef,
                CustomEnvelope, InputLayer);

            RasterDataset _rasterSave = await RasterPreview.OpenRasterDataset(DatabasePath, RasterName + suffix + "r", false, InputLayer);

            RasterPreview.WriteRasterValuesToGrid(_rasterSave, parameters, bInt);

            if (!bRasterOnly)
            {
                string FClassType = "POLYGON";
                if (!bPolygon)
                    FClassType = "POINT";

                RasterAsPoly = await RasterPreview.CreatePolyFeatClass(suffix + "f", RasterName, bExists,
                    DatabasePath, InputLayer, thisSpatRef, FClassType, false);

                _rasterProgram.Title = "Saving Output";

                string message = await RasterPreview.SaveAsVectorFeatures(RasterAsPoly, thisSpatRef, parameters, FClassType);
            }

            return bCheck;
        }
        public async virtual Task<bool> GroupDominanceLength(string suffix, string pixel_type, bool bInt, string DatabasePath)
        {
            bool bCheck = true;
            //check if raster already exists
            bool bExist = await RasterClassManagement.CheckIfRasterExists(DatabasePath, RasterName + suffix + "r"); //checks for temp grid first

            string strLayer = "Group Dominance Length";

            if (bExist)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(strLayer + " layer already exists", strTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            _rasterProgram.Title = "Calculating " + strLayer;

            if (CustomEnvelope == null)
                CustomEnvelope = await FeatureClassQuery.ReturnExtent(InputLayer);

            if (RasterPreview == null)
                RasterPreview = new RasterFactoryPreview(RasterLineamentAnalysis.GroupDominanceLength);
            else
                RasterPreview.SetRasterAnalysisDataset(RasterLineamentAnalysis.GroupDominanceLength);

            FlapParameters parameters = await RasterPreview.PrepareInputForProcessing(InputLayer,
               CustomEnvelope, Cellsize, Interval, SelectedFeatures, RangeFrom, RangeTo, Enum.RoseGeom.Other, false, "");

            RasterPreview.CalculateGridValues(parameters);

            bool bExists = false;

            _rasterProgram.Title = "Creating Output";

            await RasterPreview.CreateRasterDataset(pixel_type, RasterName + "_temp", Cellsize.ToString(), thisSpatRef, CustomEnvelope); //template

            string strPath = Environment.GetEnvironmentVariable("temp");

            RasterDataset _rasterDS = await RasterPreview.OpenRasterDataset(strPath, RasterName + "_temp.tif", true, InputLayer);

            await RasterPreview.SaveToNewRaster(_rasterDS, RasterName + suffix + "r", Cellsize, thisSpatRef,
                CustomEnvelope, InputLayer);

            RasterDataset _rasterSave = await RasterPreview.OpenRasterDataset(DatabasePath, RasterName + suffix + "r", false, InputLayer);

            RasterPreview.WriteRasterValuesToGrid(_rasterSave, parameters, bInt);

            if (!bRasterOnly)
            {
                string FClassType = "POLYGON";
                if (!bPolygon)
                    FClassType = "POINT";

                RasterAsPoly = await RasterPreview.CreatePolyFeatClass(suffix + "f", RasterName, bExists,
                    DatabasePath, InputLayer, thisSpatRef, FClassType, false);

                _rasterProgram.Title = "Saving Output";

                string message = await RasterPreview.SaveAsVectorFeatures(RasterAsPoly, thisSpatRef, parameters, FClassType);
            }

            return bCheck;
        }
        public async virtual Task<bool> GroupMeansFrequency(string suffix, string pixel_type, bool bInt, string DatabasePath)
        {
            bool bCheck = true;
            //check if raster already exists
            bool bExist = await RasterClassManagement.CheckIfRasterExists(DatabasePath, RasterName + suffix + "r"); //checks for temp grid first

            string strLayer = "Group Mean Frequency";

            if (bExist)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(strLayer + " layer already exists", strTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            _rasterProgram.Title = "Calculating " + strLayer;

            if (CustomEnvelope == null)
                CustomEnvelope = await FeatureClassQuery.ReturnExtent(InputLayer);

            if (RasterPreview == null)
                RasterPreview = new RasterFactoryPreview(RasterLineamentAnalysis.GroupMeansFrequency);
            else
                RasterPreview.SetRasterAnalysisDataset(RasterLineamentAnalysis.GroupMeansFrequency);

            FlapParameters parameters = await RasterPreview.PrepareInputForProcessing(InputLayer,
               CustomEnvelope, Cellsize, Interval, SelectedFeatures, RangeFrom, RangeTo, Enum.RoseGeom.Other, false, "");

            RasterPreview.CalculateGridValues(parameters);

            bool bExists = false;

            _rasterProgram.Title = "Creating Output";

            await RasterPreview.CreateRasterDataset(pixel_type, RasterName + "_temp", Cellsize.ToString(), thisSpatRef, CustomEnvelope); //template

            string strPath = Environment.GetEnvironmentVariable("temp");

            RasterDataset _rasterDS = await RasterPreview.OpenRasterDataset(strPath, RasterName + "_temp.tif", true, InputLayer);

            await RasterPreview.SaveToNewRaster(_rasterDS, RasterName + suffix + "r", Cellsize, thisSpatRef,
                CustomEnvelope, InputLayer);

            RasterDataset _rasterSave = await RasterPreview.OpenRasterDataset(DatabasePath, RasterName + suffix + "r", false, InputLayer);

            RasterPreview.WriteRasterValuesToGrid(_rasterSave, parameters, bInt);

            if (!bRasterOnly)
            {
                string FClassType = "POLYGON";
                if (!bPolygon)
                    FClassType = "POINT";

                RasterAsPoly = await RasterPreview.CreatePolyFeatClass(suffix + "f", RasterName, bExists,
                    DatabasePath, InputLayer, thisSpatRef, FClassType, false);

                _rasterProgram.Title = "Saving Output";

                string message = await RasterPreview.SaveAsVectorFeatures(RasterAsPoly, thisSpatRef, parameters, FClassType);
            }

            return bCheck;
        }
        public async virtual Task<bool> GroupMeansLength(string suffix, string pixel_type, bool bInt, string DatabasePath)
        {
            bool bCheck = true;

            //check if raster already exists
            bool bExist = await RasterClassManagement.CheckIfRasterExists(DatabasePath, RasterName + suffix + "r"); //checks for temp grid first

            string strLayer = "Group Mean Length";

            if (bExist)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(strLayer + " layer already exists", strTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }

            _rasterProgram.Title = "Calculating " + strLayer;

            if (CustomEnvelope == null)
                CustomEnvelope = await FeatureClassQuery.ReturnExtent(InputLayer);

            if (RasterPreview == null)
                RasterPreview = new RasterFactoryPreview(RasterLineamentAnalysis.GroupMeansLength);
            else
                RasterPreview.SetRasterAnalysisDataset(RasterLineamentAnalysis.GroupMeansLength);

            FlapParameters parameters = await RasterPreview.PrepareInputForProcessing(InputLayer,
               CustomEnvelope, Cellsize, Interval, SelectedFeatures, RangeFrom, RangeTo, Enum.RoseGeom.Other, false, "");

            RasterPreview.CalculateGridValues(parameters);

            bool bExists = false;

            _rasterProgram.Title = "Creating Output";

            await RasterPreview.CreateRasterDataset(pixel_type, RasterName + "_temp", Cellsize.ToString(), thisSpatRef, CustomEnvelope); //template

            string strPath = Environment.GetEnvironmentVariable("temp");

            RasterDataset _rasterDS = await RasterPreview.OpenRasterDataset(strPath, RasterName + "_temp.tif", true, InputLayer);

            await RasterPreview.SaveToNewRaster(_rasterDS, RasterName + suffix + "r", Cellsize, thisSpatRef,
                CustomEnvelope, InputLayer);

            RasterDataset _rasterSave = await RasterPreview.OpenRasterDataset(DatabasePath, RasterName + suffix + "r", false, InputLayer);

            RasterPreview.WriteRasterValuesToGrid(_rasterSave, parameters, bInt);

            if (!bRasterOnly)
            {
                string FClassType = "POLYGON";
                if (!bPolygon)
                    FClassType = "POINT";

                RasterAsPoly = await RasterPreview.CreatePolyFeatClass(suffix + "f", RasterName, bExists,
                    DatabasePath, InputLayer, thisSpatRef, FClassType, false);

                _rasterProgram.Title = "Saving Output";

                string message = await RasterPreview.SaveAsVectorFeatures(RasterAsPoly, thisSpatRef, parameters, FClassType);
            }

            return bCheck;
        }
        #endregion

        #endregion

        #region Dialogs
        private async Task<SaveItemDialog> SaveRasterDialog()
        {
            SaveItemDialog saveDialog = new SaveItemDialog()
            {
                Title = "Save Raster Statistical Grid...",
                Filter = ItemFilters.Rasters,
                OverwritePrompt = true,
                InitialLocation = CoreModule.CurrentProject.DefaultGeodatabasePath

            };

            return saveDialog;
        }

        private async Task<SaveItemDialog> SaveVectorDialog()
        {
            SaveItemDialog saveDialog = new SaveItemDialog()
            {
                Title = "Save Statistics As Vector...",
                Filter = ItemFilters.FeatureClasses_All,
                OverwritePrompt = true,
                InitialLocation = CoreModule.CurrentProject.DefaultGeodatabasePath

            };

            return saveDialog;
        }

        private async Task<OpenItemDialog> OpenDialog()
        {
            OpenItemDialog openDialog = new OpenItemDialog()
            {
                Title = "Open Raster Statistical Grid...",
                Filter = ItemFilters.Rasters,
                InitialLocation = CoreModule.CurrentProject.DefaultGeodatabasePath

            };

            if (openDialog.ShowDialog() == true)
            {
            }
            return openDialog;
        }
        #endregion

        #region Add Layers to Map
        public async void AddRasterLayerToMap(string rasterName)
        {
            var layers = MapView.Active.Map.GetLayersAsFlattenedList().Where(l => l.Name == rasterName).Count();

            if (layers > 0)
                return;

            string databasePath = inputDatabasePath;
            if (outputDatabasePath != "")
                databasePath = outputDatabasePath;

            Uri uri = null;
            string fullSpec = "";

            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
            {
                var table = InputLayer.GetTable();
                var dataStore = table.GetDatastore();
                uri = dataStore.GetPath();

                var workspaceNameDef = dataStore.GetConnectionString();
                var workspaceName = workspaceNameDef.Split('=')[1];

                fullSpec = System.IO.Path.Combine(workspaceName, rasterName);

            });

            await QueuedTask.Run(() => LayerFactory.Instance.CreateLayer(new Uri(fullSpec), MapView.Active.Map));

        }
        public async void AddLayerToMap(string layerName)
        {
            var layers = MapView.Active.Map.GetLayersAsFlattenedList().Where(l => l.Name == layerName).Count();

            if (layers > 0)
                return;

            string databasePath = inputDatabasePath;
            if (outputDatabasePath != "")
                databasePath = outputDatabasePath;

            string fullSpec;
            await QueuedTask.Run(() =>
            {

                fullSpec = System.IO.Path.Combine(databasePath, layerName);

                LayerFactory.Instance.CreateLayer(new Uri(fullSpec), MapView.Active.Map);

                var layer = MapView.Active.Map.GetLayersAsFlattenedList().Where(l => l.Name == layerName).FirstOrDefault();

                if (layer is FeatureLayer)
                {
                    FeatureLayer fLayer = layer as FeatureLayer;

                    fLayer.SetRenderer(fLayer.CreateRenderer(new SimpleRendererDefinition()
                    { }));

                    fLayer.SetTransparency(30);

                    fLayer.ClearDisplayCache();

                    layer.SetVisibility(false);
                }
            });
        }
        #endregion


        public RasterFactoryPreview RasterPreview { get; set; }
        public SpatialReference thisSpatRef { get; set; }
        public bool bMovingStatistics { get; set; }
        public bool bRasterOnly { get; set; }
        public bool bPolygon { get; set; }
        public RasterProgram _rasterProgram { get; set; }
        public string progressMessage { get; set; }


        public async Task Progressor_NonCancelable()
        {
            ArcGIS.Desktop.Framework.Threading.Tasks.ProgressorSource ps = new ArcGIS.Desktop.Framework.Threading.Tasks.ProgressorSource(progressMessage, false);

            int numSecondsDelay = 3;
            //If you run this in the DEBUGGER you will NOT see the dialog
            await QueuedTask.Run(() => Task.Delay(numSecondsDelay * 1000).Wait(), ps.Progressor);
        }

        public int ErrorCheckingValues(string _value) //TODO
        {
            int nValue = -1;

            if (_value == "")
                return nValue;

            if (Information.IsNothing(_value))
                return nValue;

            //TODO check for numeric
            if (Information.IsNumeric(_value))
            {
                //Check value is valid as we know it is numeric but not if it is integer or decimal

                bool result = Int32.TryParse(_value, out int number);

                if (result)
                {
                    nValue = Convert.ToInt32(_value);
                }
                else
                {
                    nValue = Convert.ToInt32(Math.Floor(Convert.ToDouble(_value)));
                }
            }
            else
                return nValue;

            return nValue;
        }



        public class MovingStatisticsRasterAnalysis : RasterAnalysis
        {
            public int XBlocks { get; set; }
            public int YBlocks { get; set; }
            public int TotalBlocks { get; set; }

            public MovingStatistics statisticsProgram { get; set; }

            #region Operations
            public async override Task<bool> RelativeEntropy(string suffix, string pixel_type, bool bInt, string DatabasePath)
            {
                string strLayer = "Relative Entropy";

                //check if raster already exists
                bool bExist = await RasterClassManagement.CheckIfRasterExists(DatabasePath, RasterName + suffix + "r"); //checks for temp grid first

                //check if already exists
                if (bExist)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(strLayer + " layer already exists!",
                        strTitle, MessageBoxButton.OK, MessageBoxImage.Hand);
                    return false;
                }

                statisticsProgram.Title = "Calculating " + strLayer;

                if (CustomEnvelope == null)
                    CustomEnvelope = await FeatureClassQuery.ReturnExtent(InputLayer);

                if (RasterPreview == null)
                    RasterPreview = new RasterFactoryPreview(RasterLineamentAnalysis.RelativeEntropy);
                else
                    RasterPreview.SetRasterAnalysisDataset(RasterLineamentAnalysis.RelativeEntropy);

                FlapParameters flapParameters = await RasterPreview.PrepareMovingStatisticsProcessing(InputLayer,
                    CustomEnvelope, Cellsize, Interval, SelectedFeatures, RangeFrom, RangeTo, Enum.RoseGeom.Other, false, "", XBlocks, YBlocks,
                    TotalBlocks);

                RasterPreview.CalculateGridValues(flapParameters);

                statisticsProgram.Title = "Creating Output";

                await RasterPreview.CreateRasterDataset(pixel_type, RasterName + "_temp", Cellsize.ToString(), thisSpatRef, CustomEnvelope); //template

                string strPath = Environment.GetEnvironmentVariable("temp");

                RasterDataset _rasterDS = await RasterPreview.OpenRasterDataset(strPath, RasterName + "_temp.tif", true, InputLayer);

                await RasterPreview.SaveToNewRaster(_rasterDS, RasterName + suffix + "r", Cellsize, thisSpatRef,
                    CustomEnvelope, InputLayer);

                _rasterDS.Dispose();

                RasterDataset _rasterSave = await RasterPreview.OpenRasterDataset(DatabasePath, RasterName + suffix + "r", false, InputLayer);

                RasterPreview.WriteRasterValuesToGrid(_rasterSave, flapParameters, bInt);

                if (!bRasterOnly)
                {
                    string FClassType = "POLYGON";
                    if (!bPolygon)
                        FClassType = "POINT";

                    RasterAsPoly = await RasterPreview.CreatePolyFeatClass(suffix + "f", RasterName, bExist,
                        DatabasePath, InputLayer, thisSpatRef, FClassType, true);

                    statisticsProgram.Title = "Saving Output";

                    string message = await RasterPreview.SaveAsVectorFeatures(RasterAsPoly, thisSpatRef, flapParameters, FClassType);
                }

                return true;
            }
            public async override Task<bool> DensityLength(string suffix, string pixel_type, bool bInt, string DatabasePath)
            {
                //check if raster already exists
                bool bExist = await RasterClassManagement.CheckIfRasterExists(DatabasePath, RasterName + suffix + "r"); //checks for temp grid first

                string strLayer = "Density Length";

                if (bExist)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(strLayer + " layer already exists", strTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

                statisticsProgram.Title = "Calculating " + strLayer;
                bool bCheck = true;

                if (RasterPreview == null)
                    RasterPreview = new RasterFactoryPreview(RasterLineamentAnalysis.DensityLength);
                else
                    RasterPreview.SetRasterAnalysisDataset(RasterLineamentAnalysis.DensityLength);

                FlapParameters flapParameters = await RasterPreview.PrepareMovingStatisticsProcessing(InputLayer,
                    CustomEnvelope, Cellsize, Interval, SelectedFeatures, RangeFrom, RangeTo, Enum.RoseGeom.Other, false, "", XBlocks, YBlocks,
                    TotalBlocks);

                RasterPreview.CalculateGridValues(flapParameters);

                statisticsProgram.Title = "Creating Output";

                await RasterPreview.CreateRasterDataset(pixel_type, RasterName + "_temp", Cellsize.ToString(), thisSpatRef, CustomEnvelope); //template

                string strPath = Environment.GetEnvironmentVariable("temp");

                RasterDataset _rasterDS = await RasterPreview.OpenRasterDataset(strPath, RasterName + "_temp.tif", true, InputLayer);

                await RasterPreview.SaveToNewRaster(_rasterDS, RasterName + suffix + "r", Cellsize, thisSpatRef,
                    CustomEnvelope, InputLayer);

                RasterDataset _rasterSave = await RasterPreview.OpenRasterDataset(DatabasePath, RasterName + suffix + "r", false, InputLayer);

                RasterPreview.WriteRasterValuesToGrid(_rasterSave, flapParameters, bInt);

                if (!bRasterOnly)
                {
                    string FClassType = "POLYGON";
                    if (!bPolygon)
                        FClassType = "POINT";

                    RasterAsPoly = await RasterPreview.CreatePolyFeatClass(suffix + "f", RasterName, bExist,
                        DatabasePath, InputLayer, thisSpatRef, FClassType, true);

                    statisticsProgram.Title = "Saving Output";

                    string message = await RasterPreview.SaveAsVectorFeatures(RasterAsPoly, thisSpatRef, flapParameters, FClassType);
                }

                return bCheck;
            }
            public async override Task<bool> DensityFrequency(string suffix, string pixel_type, bool bInt, string DatabasePath)
            {
                bool bCheck = true;
                //check if raster already exists
                bool bExist = await RasterClassManagement.CheckIfRasterExists(DatabasePath, RasterName + suffix + "r"); //checks for temp grid first

                string strLayer = "Density Frequency";

                if (bExist)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(strLayer + " layer already exists", strTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

                statisticsProgram.Title = "Calculating " + strLayer;

                if (CustomEnvelope == null)
                    CustomEnvelope = await FeatureClassQuery.ReturnExtent(InputLayer);

                if (RasterPreview == null)
                    RasterPreview = new RasterFactoryPreview(RasterLineamentAnalysis.DensityFrequency);
                else
                    RasterPreview.SetRasterAnalysisDataset(RasterLineamentAnalysis.DensityFrequency);

                FlapParameters flapParameters = await RasterPreview.PrepareMovingStatisticsProcessing(InputLayer,
                    CustomEnvelope, Cellsize, Interval, SelectedFeatures, RangeFrom, RangeTo, Enum.RoseGeom.Other, false, "", XBlocks, YBlocks,
                    TotalBlocks);

                RasterPreview.CalculateGridValues(flapParameters);

                bool bExists = false;

                statisticsProgram.Title = "Creating Output";

                await RasterPreview.CreateRasterDataset(pixel_type, RasterName + "_temp", Cellsize.ToString(), thisSpatRef, CustomEnvelope); //template

                string strPath = Environment.GetEnvironmentVariable("temp");

                RasterDataset _rasterDS = await RasterPreview.OpenRasterDataset(strPath, RasterName + "_temp.tif", true, InputLayer);

                await RasterPreview.SaveToNewRaster(_rasterDS, RasterName + suffix + "r", Cellsize, thisSpatRef,
                    CustomEnvelope, InputLayer);

                RasterDataset _rasterSave = await RasterPreview.OpenRasterDataset(DatabasePath, RasterName + suffix + "r", false, InputLayer);

                RasterPreview.WriteRasterValuesToGrid(_rasterSave, flapParameters, bInt);

                if (!bRasterOnly)
                {
                    string FClassType = "POLYGON";
                    if (!bPolygon)
                        FClassType = "POINT";

                    RasterAsPoly = await RasterPreview.CreatePolyFeatClass(suffix + "f", RasterName, bExists,
                        DatabasePath, InputLayer, thisSpatRef, FClassType, true);

                    statisticsProgram.Title = "Saving Output";

                    string message = await RasterPreview.SaveAsVectorFeatures(RasterAsPoly, thisSpatRef, flapParameters, FClassType);
                }

                return bCheck;
            }
            public async override Task<bool> GroupDominanceFrequency(string suffix, string pixel_type, bool bInt, string DatabasePath)
            {
                bool bCheck = true;
                //check if raster already exists
                bool bExist = await RasterClassManagement.CheckIfRasterExists(DatabasePath, RasterName + suffix + "r"); //checks for temp grid first

                string strLayer = "Group Dominance Frequency";

                if (bExist)
                {
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(strLayer + " layer already exists", strTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
                statisticsProgram.Title = "Calculating " + strLayer;

                if (CustomEnvelope == null)
                    CustomEnvelope = await FeatureClassQuery.ReturnExtent(InputLayer);

                if (RasterPreview == null)
                    RasterPreview = new RasterFactoryPreview(RasterLineamentAnalysis.GroupDominanceFrequency);
                else
                    RasterPreview.SetRasterAnalysisDataset(RasterLineamentAnalysis.GroupDominanceFrequency);

                FlapParameters flapParameters = await RasterPreview.PrepareMovingStatisticsProcessing(InputLayer,
                    CustomEnvelope, Cellsize, Interval, SelectedFeatures, RangeFrom, RangeTo, Enum.RoseGeom.Other, false, "", XBlocks, YBlocks,
                    TotalBlocks);

                RasterPreview.CalculateGridValues(flapParameters);

                bool bExists = false;

                statisticsProgram.Title = "Creating Output";

                await RasterPreview.CreateRasterDataset(pixel_type, RasterName + "_temp", Cellsize.ToString(), thisSpatRef, CustomEnvelope); //template

                string strPath = Environment.GetEnvironmentVariable("temp");

                RasterDataset _rasterDS = await RasterPreview.OpenRasterDataset(strPath, RasterName + "_temp.tif", true, InputLayer);

                await RasterPreview.SaveToNewRaster(_rasterDS, RasterName + suffix + "r", Cellsize, thisSpatRef,
                    CustomEnvelope, InputLayer);

                RasterDataset _rasterSave = await RasterPreview.OpenRasterDataset(DatabasePath, RasterName + suffix + "r", false, InputLayer);

                RasterPreview.WriteRasterValuesToGrid(_rasterSave, flapParameters, bInt);

                if (!bRasterOnly)
                {
                    string FClassType = "POLYGON";
                    if (!bPolygon)
                        FClassType = "POINT";

                    RasterAsPoly = await RasterPreview.CreatePolyFeatClass(suffix + "f", RasterName, bExists,
                        DatabasePath, InputLayer, thisSpatRef, FClassType, true);

                    statisticsProgram.Title = "Saving Output";

                    string message = await RasterPreview.SaveAsVectorFeatures(RasterAsPoly, thisSpatRef, flapParameters, FClassType);
                }

                return bCheck;
            }
            public async override Task<bool> GroupDominanceLength(string suffix, string pixel_type, bool bInt, string DatabasePath)
            {
                bool bCheck = true;
                //check if raster already exists
                bool bExist = await RasterClassManagement.CheckIfRasterExists(DatabasePath, RasterName + suffix + "r"); //checks for temp grid first

                string strLayer = "Group Dominance Length";

                if (bExist)
                {
                    MessageBox.Show(strLayer + " layer already exists", strTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

                statisticsProgram.Title = "Calculating " + strLayer;

                if (CustomEnvelope == null)
                    CustomEnvelope = await FeatureClassQuery.ReturnExtent(InputLayer);

                if (RasterPreview == null)
                    RasterPreview = new RasterFactoryPreview(RasterLineamentAnalysis.GroupDominanceLength);
                else
                    RasterPreview.SetRasterAnalysisDataset(RasterLineamentAnalysis.GroupDominanceLength);

                FlapParameters flapParameters = await RasterPreview.PrepareMovingStatisticsProcessing(InputLayer,
                    CustomEnvelope, Cellsize, Interval, SelectedFeatures, RangeFrom, RangeTo, Enum.RoseGeom.Other, false, "", XBlocks, YBlocks,
                    TotalBlocks);

                RasterPreview.CalculateGridValues(flapParameters);

                bool bExists = false;

                statisticsProgram.Title = "Creating Output";

                await RasterPreview.CreateRasterDataset(pixel_type, RasterName + "_temp", Cellsize.ToString(), thisSpatRef, CustomEnvelope); //template

                string strPath = Environment.GetEnvironmentVariable("temp");

                RasterDataset _rasterDS = await RasterPreview.OpenRasterDataset(strPath, RasterName + "_temp.tif", true, InputLayer);

                await RasterPreview.SaveToNewRaster(_rasterDS, RasterName + suffix + "r", Cellsize, thisSpatRef,
                    CustomEnvelope, InputLayer);

                RasterDataset _rasterSave = await RasterPreview.OpenRasterDataset(DatabasePath, RasterName + suffix + "r", false, InputLayer);

                RasterPreview.WriteRasterValuesToGrid(_rasterSave, flapParameters, bInt);

                if (!bRasterOnly)
                {
                    string FClassType = "POLYGON";
                    if (!bPolygon)
                        FClassType = "POINT";

                    RasterAsPoly = await RasterPreview.CreatePolyFeatClass(suffix + "f", RasterName, bExists,
                        DatabasePath, InputLayer, thisSpatRef, FClassType, true);

                    statisticsProgram.Title = "Saving Output";

                    string message = await RasterPreview.SaveAsVectorFeatures(RasterAsPoly, thisSpatRef, flapParameters, FClassType);
                }

                return bCheck;
            }
            public async override Task<bool> GroupMeansFrequency(string suffix, string pixel_type, bool bInt, string DatabasePath)
            {
                bool bCheck = true;
                //check if raster already exists
                bool bExist = await RasterClassManagement.CheckIfRasterExists(DatabasePath, RasterName + suffix + "r"); //checks for temp grid first

                string strLayer = "Group Mean Frequency";

                if (bExist)
                {
                    MessageBox.Show(strLayer + " layer already exists", strTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }
                statisticsProgram.Title = "Calculating " + strLayer;

                if (CustomEnvelope == null)
                    CustomEnvelope = await FeatureClassQuery.ReturnExtent(InputLayer);

                if (RasterPreview == null)
                    RasterPreview = new RasterFactoryPreview(RasterLineamentAnalysis.GroupMeansFrequency);
                else
                    RasterPreview.SetRasterAnalysisDataset(RasterLineamentAnalysis.GroupMeansFrequency);

                FlapParameters flapParameters = await RasterPreview.PrepareMovingStatisticsProcessing(InputLayer,
                    CustomEnvelope, Cellsize, Interval, SelectedFeatures, RangeFrom, RangeTo, Enum.RoseGeom.Other, false, "", XBlocks, YBlocks,
                    TotalBlocks);

                RasterPreview.CalculateGridValues(flapParameters);

                bool bExists = false;

                statisticsProgram.Title = "Creating Output";

                await RasterPreview.CreateRasterDataset(pixel_type, RasterName + "_temp", Cellsize.ToString(), thisSpatRef, CustomEnvelope); //template

                string strPath = Environment.GetEnvironmentVariable("temp");

                RasterDataset _rasterDS = await RasterPreview.OpenRasterDataset(strPath, RasterName + "_temp.tif", true, InputLayer);

                await RasterPreview.SaveToNewRaster(_rasterDS, RasterName + suffix + "r", Cellsize, thisSpatRef,
                    CustomEnvelope, InputLayer);

                RasterDataset _rasterSave = await RasterPreview.OpenRasterDataset(DatabasePath, RasterName + suffix + "r", false, InputLayer);

                RasterPreview.WriteRasterValuesToGrid(_rasterSave, flapParameters, bInt);

                if (!bRasterOnly)
                {
                    string FClassType = "POLYGON";
                    if (!bPolygon)
                        FClassType = "POINT";

                    RasterAsPoly = await RasterPreview.CreatePolyFeatClass(suffix + "f", RasterName, bExists,
                        DatabasePath, InputLayer, thisSpatRef, FClassType, true);

                    statisticsProgram.Title = "Saving Output";

                    string message = await RasterPreview.SaveAsVectorFeatures(RasterAsPoly, thisSpatRef, flapParameters, FClassType);
                }

                return bCheck;
            }
            public async override Task<bool> GroupMeansLength(string suffix, string pixel_type, bool bInt, string DatabasePath)
            {
                bool bCheck = true;

                //check if raster already exists
                bool bExist = await RasterClassManagement.CheckIfRasterExists(DatabasePath, RasterName + suffix + "r"); //checks for temp grid first

                string strLayer = "Group Mean Length";

                if (bExist)
                {
                    MessageBox.Show(strLayer + " layer already exists", strTitle, MessageBoxButton.OK, MessageBoxImage.Information);
                    return false;
                }

                statisticsProgram.Title = "Calculating " + strLayer;

                if (CustomEnvelope == null)
                    CustomEnvelope = await FeatureClassQuery.ReturnExtent(InputLayer);

                if (RasterPreview == null)
                    RasterPreview = new RasterFactoryPreview(RasterLineamentAnalysis.GroupMeansLength);
                else
                    RasterPreview.SetRasterAnalysisDataset(RasterLineamentAnalysis.GroupMeansLength);

                FlapParameters flapParameters = await RasterPreview.PrepareMovingStatisticsProcessing(InputLayer,
                    CustomEnvelope, Cellsize, Interval, SelectedFeatures, RangeFrom, RangeTo, Enum.RoseGeom.Other, false, "", XBlocks, YBlocks,
                    TotalBlocks);

                RasterPreview.CalculateGridValues(flapParameters);

                bool bExists = false;

                statisticsProgram.Title = "Creating Output";

                await RasterPreview.CreateRasterDataset(pixel_type, RasterName + "_temp", Cellsize.ToString(), thisSpatRef, CustomEnvelope); //template

                string strPath = Environment.GetEnvironmentVariable("temp");

                RasterDataset _rasterDS = await RasterPreview.OpenRasterDataset(strPath, RasterName + "_temp.tif", true, InputLayer);

                await RasterPreview.SaveToNewRaster(_rasterDS, RasterName + suffix + "r", Cellsize, thisSpatRef,
                    CustomEnvelope, InputLayer);

                RasterDataset _rasterSave = await RasterPreview.OpenRasterDataset(DatabasePath, RasterName + suffix + "r", false, InputLayer);

                RasterPreview.WriteRasterValuesToGrid(_rasterSave, flapParameters, bInt);

                if (!bRasterOnly)
                {
                    string FClassType = "POLYGON";
                    if (!bPolygon)
                        FClassType = "POINT";

                    RasterAsPoly = await RasterPreview.CreatePolyFeatClass(suffix + "f", RasterName, bExists,
                        DatabasePath, InputLayer, thisSpatRef, FClassType, true);

                    statisticsProgram.Title = "Saving Output";

                    string message = await RasterPreview.SaveAsVectorFeatures(RasterAsPoly, thisSpatRef, flapParameters, FClassType);
                }


                return bCheck;
            }
            #endregion
        }

    }
}
