using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core.Geoprocessing;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using Rose_AGP.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Geometry;

namespace Rose_AGP
{
    public static class FeatureClassManagement
    {

        public static async Task<FeatureClass> CreateOutputFeatureClass(string filePath, string roseName, string databasePath,
            FeatureLayer InputLayer, bool bStatistics, string RoseGeom, bool bFish, SpatialReference thisSpatRef)
        {
            string fullOutputSpec = "";

            if (filePath != "")
                fullOutputSpec = filePath;

            List<object> arguments = new List<object>
            {
                // store the results in the default geodatabase
                //CoreModule.CurrentProject.DefaultGeodatabasePath,
                databasePath,
                // name of the feature class
                roseName,
                // type of geometry
                "POLYGON",
                // no template
                "",
                // m values
                "DISABLED",
                // no z values
                "DISABLED"
              };

            arguments.Add(thisSpatRef);

            System.Threading.CancellationTokenSource _cts = new System.Threading.CancellationTokenSource();
            var args = Geoprocessing.MakeValueArray(arguments.ToArray());

            var result = await Geoprocessing.ExecuteToolAsync("CreateFeatureclass_management", args, null, _cts.Token, (event_name, o) =>  // implement delegate and handle events, o is message object.
            {

            }, GPExecuteToolFlags.None);

            List<RoseFields> roseFields = new List<RoseFields>
            {
                new RoseFields { FieldName = "CellID", FieldAlias = "Cell ID", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 }
            };

            bool bCheck = true;

            bCheck = await ExecuteAddFieldTool(fullOutputSpec, roseFields);


            if (bStatistics)
            {
                if (bFish)
                    bCheck = await AddFishFields(RoseGeom, fullOutputSpec);
                else
                    bCheck = await AddRoseFields(RoseGeom, fullOutputSpec);

            }

            //TODO 
            return await SetOutputFeatureclass(databasePath, roseName);

        }

        public static async Task<FeatureClass> CreateOutputFeatureClass(string filePath, string rasterPolyName,
            string databasePath, FeatureLayer InputLayer, RasterLineamentAnalysis _rasterType, SpatialReference thisSpatRef, string FClassType,
            bool bMovStats)
        {
            //FClassType either "POINT" or "POLYGON"
            string fullOutputSpec = "";

            if (filePath != "")
                fullOutputSpec = filePath;

            List<object> arguments = new List<object>
            {
                // store the results in the default geodatabase
                databasePath,
                // name of the feature class
                rasterPolyName,
                // type of geometry
                FClassType,
                // no template
                "",
                // m values
                "DISABLED",
                // no z values
                "DISABLED"
              };

            arguments.Add(thisSpatRef);

            System.Threading.CancellationTokenSource _cts = new System.Threading.CancellationTokenSource();
            var args = Geoprocessing.MakeValueArray(arguments.ToArray());

            IGPResult result = await Geoprocessing.ExecuteToolAsync("CreateFeatureclass_management", Geoprocessing.MakeValueArray(arguments.ToArray()), null, _cts.Token, (event_name, o) =>  // implement delegate and handle events, o is message object.
            {

            }, GPExecuteToolFlags.None);


            bool bCheck = true;

            if (bMovStats)
                bCheck = await AddMovStatsFields(fullOutputSpec, _rasterType);
            else
                bCheck = await AddRasterFields(fullOutputSpec, _rasterType);

            //TODO 
            return await SetOutputFeatureclass(databasePath, rasterPolyName);

        }

        public static async Task<FeatureClass> SetOutputFeatureclass(string databasePath, string roseName)
        {
            FeatureClass OutputFeatureclass = null;

            await QueuedTask.Run(() => {

                Geodatabase geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(databasePath)));
                FeatureClass _outputFeatureclass = geodatabase.OpenDataset<FeatureClass>(roseName);
                {
                    OutputFeatureclass = _outputFeatureclass;
                }

            });

            return OutputFeatureclass;

        }

        public static async Task<bool> ExecuteAddFieldTool(string fullSpec, List<RoseFields> roseFields, bool isNullable = true)
        {
            try
            {
                return await QueuedTask.Run(() =>
                {
                    foreach (RoseFields _field in roseFields)
                    {
                        string fieldname = _field.FieldName;
                        string fieldAlias = _field.FieldAlias;

                        KeyValuePair<string, string> field = new KeyValuePair<string, string>(fieldname, fieldAlias);

                        var parameters = Geoprocessing.MakeValueArray(fullSpec, field.Key, _field.FieldType.ToUpper(), null, null,
                            _field.FieldLength, field.Value, isNullable ? "NULABLE" : "NON_NULLABLE");

                        var cts = new System.Threading.CancellationTokenSource();

                        var results = Geoprocessing.ExecuteToolAsync("management.AddField", parameters, null, cts.Token,
                            (eventName, o) =>
                            {
                                System.Diagnostics.Debug.WriteLine($@"GP event: {eventName}");
                            }, GPExecuteToolFlags.None);
                    }
                    return true;
                });
            }
            catch (Exception ex)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.ToString());
                return false;
            }
        }

        public static async Task<bool> ExecuteAddRasterField(string fullSpec, List<RasterFields> rasterFields, bool isNullable = true)
        {

            try
            {
                return await QueuedTask.Run(() =>
                {
                    foreach (RasterFields _field in rasterFields)
                    {
                        string fieldname = _field.FieldName;
                        string fieldAlias = _field.FieldAlias;

                        KeyValuePair<string, string> field = new KeyValuePair<string, string>(fieldname, fieldAlias);

                        var parameters = Geoprocessing.MakeValueArray(fullSpec, field.Key, _field.FieldType.ToUpper(), null, null,
                            _field.FieldLength, field.Value, isNullable ? "NULABLE" : "NON_NULLABLE");

                        var cts = new System.Threading.CancellationTokenSource();

                        var results = Geoprocessing.ExecuteToolAsync("management.AddField", parameters, null, cts.Token,
                            (eventName, o) =>
                            {
                                System.Diagnostics.Debug.WriteLine($@"GP event: {eventName}");
                            }, GPExecuteToolFlags.None);
                    }
                    return true;
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                return false;
            }
        }

        public static async Task<bool> ExecuteAddMovStatsField(string fullSpec, List<MovingStatsFields> movStatsFields, bool isNullable = true)
        {

            try
            {
                return await QueuedTask.Run(() =>
                {
                    foreach (MovingStatsFields _field in movStatsFields)
                    {
                        string fieldname = _field.FieldName;
                        string fieldAlias = _field.FieldAlias;

                        KeyValuePair<string, string> field = new KeyValuePair<string, string>(fieldname, fieldAlias);

                        var parameters = Geoprocessing.MakeValueArray(fullSpec, field.Key, _field.FieldType.ToUpper(), null, null,
                            _field.FieldLength, field.Value, isNullable ? "NULABLE" : "NON_NULLABLE");

                        var cts = new System.Threading.CancellationTokenSource();

                        var results = Geoprocessing.ExecuteToolAsync("management.AddField", parameters, null, cts.Token,
                            (eventName, o) =>
                            {
                                System.Diagnostics.Debug.WriteLine($@"GP event: {eventName}");
                            }, GPExecuteToolFlags.None);
                    }
                    return true;
                });
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());
                return false;
            }
        }

        public static async Task<Datastore> GetDataStore(FeatureLayer featLayer)
        {
            Datastore dataStore = null;
            try
            {
                await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() =>
                {

                    dataStore = featLayer.GetFeatureClass().GetDatastore();

                });
            }
            catch (Exception ex)
            {
                return null;
            }

            return dataStore;
        }

        private static async Task<bool> AddRoseFields(string RoseGeom, string fullOutputSpec)
        {

            //Rose fields
            List<RoseFields> roseFields = new List<RoseFields>
            {
                new RoseFields { FieldName = "CellID", FieldAlias = "Cell ID", FieldType = "integer", FieldLength = 0, FieldPrecision = 0 },
                new RoseFields { FieldName = "BinAzi", FieldAlias = "BinRange", FieldType = "TEXT", FieldLength = 20, FieldPrecision = 0 },
                new RoseFields { FieldName = "BinCount", FieldAlias = "Bin Count", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 },
                new RoseFields { FieldName = "MinAzi", FieldAlias = "Minimum Azi", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 4 },
                new RoseFields { FieldName = "MaxAzi", FieldAlias = "Maximum Azi", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 4 },
                new RoseFields { FieldName = "AvgAzi", FieldAlias = "Average Azi", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 4 },
                new RoseFields { FieldName = "StdAzi", FieldAlias = "Standard Deviation", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 4 },
                new RoseFields { FieldName = "Created", FieldAlias = "Date and time", FieldType = "TEXT", FieldLength = 50, FieldPrecision = 0 }
            };

            if (RoseGeom == "Line")
            {
                roseFields.Add(new RoseFields { FieldName = "MinLength", FieldAlias = "Minimum Length", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 4 });
                roseFields.Add(new RoseFields { FieldName = "MaxLength", FieldAlias = "Maximum Length", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 4 });
                roseFields.Add(new RoseFields { FieldName = "AvgLength", FieldAlias = "Average Length", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 4 });
                roseFields.Add(new RoseFields { FieldName = "StdLen", FieldAlias = "Std. Length", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 4 });

            }

            try
            {

                return await ExecuteAddFieldTool(fullOutputSpec, roseFields);

            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());

                return false;
            }
        }

        private static async Task<bool> AddFishFields(string RoseGeom, string fullOutputSpec)
        {
            //Rose fields
            List<RoseFields> roseFields = new List<RoseFields>
            {
                new RoseFields { FieldName = "CellID", FieldAlias = "Cell ID", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 },
                new RoseFields { FieldName = "Count", FieldAlias = "Count of Features", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 },
                new RoseFields { FieldName = "MinAzi", FieldAlias = "Minimum Azi", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 4 },
                new RoseFields { FieldName = "MaxAzi", FieldAlias = "Maximum Azi", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 4 },
                new RoseFields { FieldName = "AvgAzi", FieldAlias = "Average Azi", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 4 },
                new RoseFields { FieldName = "StdAzi", FieldAlias = "Standard Deviation", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 4 }
            };

            if (RoseGeom == "Line")
            {
                roseFields.Add(new RoseFields { FieldName = "MinLength", FieldAlias = "Minimum Length", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 4 });
                roseFields.Add(new RoseFields { FieldName = "MaxLength", FieldAlias = "Maximum Length", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 4 });
                roseFields.Add(new RoseFields { FieldName = "AvgLength", FieldAlias = "Average Length", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 4 });
                roseFields.Add(new RoseFields { FieldName = "StdLen", FieldAlias = "Std. Length", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 4 });
                roseFields.Add(new RoseFields { FieldName = "SumLen", FieldAlias = "Total Length", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 4 });

            }
            roseFields.Add(new RoseFields { FieldName = "RoseType", FieldAlias = "Rose Type", FieldType = "TEXT", FieldLength = 30, FieldPrecision = 0 });
            roseFields.Add(new RoseFields { FieldName = "Comment", FieldAlias = "Fish summary", FieldType = "TEXT", FieldLength = 50, FieldPrecision = 0 });


            try
            {

                return await ExecuteAddFieldTool(fullOutputSpec, roseFields);

            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());

                return false;
            }
        }

        private static async Task<bool> AddRasterFields(string fullOutputSpec, RasterLineamentAnalysis _rasterType)
        {
            //Raster fields
            List<RasterFields> rasterFields = new List<RasterFields>
            {
                new RasterFields { FieldName = "CellID", FieldAlias = "Cell ID", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 },
                new RasterFields { FieldName = "Value", FieldAlias = "Computed value", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 9 },
                new RasterFields { FieldName = "Cellsize", FieldAlias = "Cell size", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 },
                new RasterFields { FieldName = "Interval", FieldAlias = "Interval size", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 },
                new RasterFields { FieldName = "Count", FieldAlias = "Features in Cell", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 },
                new RasterFields { FieldName = "RasType", FieldAlias = "Analysis Type", FieldType = "TEXT", FieldLength = 30, FieldPrecision = 0 },
                new RasterFields { FieldName = "Comment", FieldAlias = "Raster Analsyis", FieldType = "TEXT", FieldLength = 50, FieldPrecision = 0 }
            };

            if (_rasterType != RasterLineamentAnalysis.RelativeEntropy)
            {
                rasterFields.Add(new RasterFields { FieldName = "AngleF", FieldAlias = "Azimuth from", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 });
                rasterFields.Add(new RasterFields { FieldName = "AngleT", FieldAlias = "Azimuth to", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 });
            }

            try
            {
                return await ExecuteAddRasterField(fullOutputSpec, rasterFields);

            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());

                return false;
            }
        }


        private static async Task<bool> AddMovStatsFields(string fullOutputSpec, RasterLineamentAnalysis _rasterType)
        {
            //Raster fields
            List<MovingStatsFields> movStatsFields = new List<MovingStatsFields>
            {
                new MovingStatsFields { FieldName = "CellID", FieldAlias = "Cell ID", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 },
                new MovingStatsFields { FieldName = "Value", FieldAlias = "Grid value", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 9 },
                //new MovingStatsFields { FieldName = "Mean", FieldAlias = "Average value", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 9 },
                //new MovingStatsFields { FieldName = "Min", FieldAlias = "Minimum value", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 9 },
                //new MovingStatsFields { FieldName = "Max", FieldAlias = "Maximum value", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 9 },
                //new MovingStatsFields { FieldName = "Std", FieldAlias = "Standard Deviation", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 9 },
                //new MovingStatsFields { FieldName = "Sum", FieldAlias = "Total value", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 9 },
                new MovingStatsFields { FieldName = "Cellsize", FieldAlias = "Cell size", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 },
                new MovingStatsFields { FieldName = "Interval", FieldAlias = "Interval size", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 },
                new MovingStatsFields { FieldName = "Count", FieldAlias = "Features in Cell", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 },
                new MovingStatsFields { FieldName = "Search", FieldAlias = "Features in Search", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 },
                new MovingStatsFields { FieldName = "RasType", FieldAlias = "Analysis Type", FieldType = "TEXT", FieldLength = 30, FieldPrecision = 0 },
                new MovingStatsFields { FieldName = "XBlock", FieldAlias = "XBlock", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 },
                new MovingStatsFields { FieldName = "YBlock", FieldAlias = "YBlock", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 },
                new MovingStatsFields { FieldName = "TotalBlock", FieldAlias = "Total Blocks", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 },
                new MovingStatsFields { FieldName = "Area", FieldAlias = "Search Area", FieldType = "DOUBLE", FieldLength = 0, FieldPrecision = 9 },
                new MovingStatsFields { FieldName = "Comment", FieldAlias = "Raster Analsyis", FieldType = "TEXT", FieldLength = 50, FieldPrecision = 0 }
            };

            if (_rasterType != RasterLineamentAnalysis.RelativeEntropy)
            {
                movStatsFields.Add(new MovingStatsFields { FieldName = "AngleF", FieldAlias = "Azimuth from", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 });
                movStatsFields.Add(new MovingStatsFields { FieldName = "AngleT", FieldAlias = "Azimuth to", FieldType = "INTEGER", FieldLength = 0, FieldPrecision = 0 });
            }

            try
            {
                return await ExecuteAddMovStatsField(fullOutputSpec, movStatsFields);

            }

            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.ToString());

                return false;
            }
        }

    }
}
