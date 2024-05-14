using ArcGIS.Core.Data;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Desktop.Editing;
using Rose_AGP.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ArcGIS.Core.Geometry;
using System.IO;
using System.Windows.Media;
using ArcGIS.Core.Internal.Geometry;

namespace Rose_AGP
{
    public static class VectorFunctions
    {
        public static string ProgressMessage { get; set; }

        public static async Task<FeatureClass> CreatePolyFeatClass(string suffix, string rasterName, bool bExists, string databasePath,
            FeatureLayer InputLayer, RasterLineamentAnalysis _rasterType, SpatialReference thisSpatRef, string FClassType, bool bMovStats)
        {
            VectorFunctions.ProgressMessage = "Creating Layer...";
            await VectorFunctions.Progressor_NonCancelable();

            //TODO - check if exists
            //Check if exists
            bExists = await FeatureClassQuery.FeatureclassExists(databasePath, rasterName + suffix);

            FeatureClass RasterAsPoly = null;

            if (!bExists)
            {
                RasterAsPoly = await FeatureClassManagement.CreateOutputFeatureClass(Path.Combine(databasePath, rasterName + suffix),
                    rasterName + suffix, databasePath, InputLayer, _rasterType, thisSpatRef, FClassType, bMovStats);
            }
            else  //If featureclass exists, checks whether to overwrite or update. Will update if detects CellID field
            {
                RasterAsPoly = await FeatureClassManagement.SetOutputFeatureclass(databasePath, rasterName + suffix);

            }

            if (RasterAsPoly == null)
                return null;

            return RasterAsPoly;
        }

        public static async Task<FeatureClass> CreateRoseNetFeatClass(string suffix, string roseName, bool bExists,
            string databasePath, FeatureLayer InputLayer, bool bStatistics, RoseGeom roseGeom, bool bFish,
            SpatialReference thisSpatRef)
        {
            VectorFunctions.ProgressMessage = "Creating Layer...";
            await VectorFunctions.Progressor_NonCancelable();

            FeatureClass RoseFC = null;

            if (!bExists)
            {
                RoseFC = await FeatureClassManagement.CreateOutputFeatureClass(Path.Combine(databasePath, roseName + suffix),
                    roseName + suffix, databasePath, InputLayer, bStatistics, roseGeom.ToString(), bFish, thisSpatRef);
            }
            else  //If featureclass exists, checks whether to overwrite or update. Will update if detects CellID field
            {
                RoseFC = await FeatureClassManagement.SetOutputFeatureclass(databasePath, roseName + suffix);

            }

            if (RoseFC == null)
                return null;

            return RoseFC;
        }

        public static async Task<string> SaveRasterToPolygon(FeatureClass OuputFeatureClass, SpatialReference mySpatialRef,
            FlapParameters _parameters, RasterLineamentAnalysis _rasterAnalysis)
        {
            VectorFunctions.ProgressMessage = "Saving polygons...";
            await VectorFunctions.Progressor_NonCancelable();

            bool bEntropy = false;

            string rasterType = "";
            switch (_rasterAnalysis)
            {
                case RasterLineamentAnalysis.DensityLength:
                    rasterType = "Density Length";
                    break;
                case RasterLineamentAnalysis.DensityFrequency:
                    rasterType = "Density Frequency";
                    break;
                case RasterLineamentAnalysis.GroupMeansFrequency:
                    rasterType = "Group Mean Frequency";
                    break;
                case RasterLineamentAnalysis.GroupMeansLength:
                    rasterType = "Group Mean Length";
                    break;
                case RasterLineamentAnalysis.RelativeEntropy:
                    rasterType = "Relative Entropy";
                    bEntropy = true;
                    break;
                case RasterLineamentAnalysis.GroupDominanceLength:
                    rasterType = "Group Dominance Length";
                    break;
                case RasterLineamentAnalysis.GroupDominanceFrequency:
                    rasterType = "Group Dominance Frequency";
                    break;
                default:
                    break;
            }


            string message = String.Empty;
            bool creationResult = false;

            await QueuedTask.Run(() =>
            {
                EditOperation editOperation1 = new EditOperation();
                EditOperation editOperation = editOperation1;
                editOperation.Name = string.Format("Create features in layer {0}", rasterType + " Layer"); //TODO
                editOperation.SelectNewFeatures = true;

                editOperation.Callback(async context =>
                {
                    FeatureClassDefinition _definition = OuputFeatureClass.GetDefinition();

                    foreach (FlapParameter parameter in _parameters.flapParameters)
                    {
                        if (parameter.CreateCell)
                        {
                            if (parameter.LenAzi.Count > 0)
                            {
                                MapPointBuilderEx ex = new MapPointBuilderEx(mySpatialRef);

                                using (RowBuffer rowBuffer = OuputFeatureClass.CreateRowBuffer())
                                {
                                    MapPoint pt1 = MapPointBuilderEx.CreateMapPoint(parameter.XMin, parameter.YMin, mySpatialRef);
                                    MapPoint pt2 = MapPointBuilderEx.CreateMapPoint(parameter.XMin, parameter.YMax, mySpatialRef);
                                    MapPoint pt3 = MapPointBuilderEx.CreateMapPoint(parameter.XMax, parameter.YMax, mySpatialRef);
                                    MapPoint pt4 = MapPointBuilderEx.CreateMapPoint(parameter.XMax, parameter.YMin, mySpatialRef);


                                    List<MapPoint> list = new List<MapPoint>
                                {
                                    pt1,
                                    pt2,
                                    pt3,
                                    pt4
                                };

                                    Polygon poly = PolygonBuilderEx.CreatePolygon(list);
                                    rowBuffer[_definition.GetShapeField()] = poly;

                                    int checkField = -1;

                                    checkField = _definition.FindField("CellID");
                                    if (checkField > -1)
                                        rowBuffer["CellID"] = parameter.CellID;

                                    checkField = _definition.FindField("Count");
                                    if (checkField > -1)
                                        rowBuffer["Count"] = parameter.Count;

                                    checkField = _definition.FindField("Value");
                                    if (checkField > -1)
                                        rowBuffer["Value"] = parameter.GridValue;

                                    //checkField = _definition.FindField("Mean");
                                    //if (checkField > -1)
                                    //    rowBuffer["Mean"] = parameter.MeanValue;

                                    //checkField = _definition.FindField("Min");
                                    //if (checkField > -1)
                                    //    rowBuffer["Min"] = parameter.MinValue;

                                    //checkField = _definition.FindField("Max");
                                    //if (checkField > -1)
                                    //    rowBuffer["Max"] = parameter.MaxValue;

                                    //checkField = _definition.FindField("Std");
                                    //if (checkField > -1)
                                    //    rowBuffer["Std"] = parameter.StdValue;

                                    //checkField = _definition.FindField("Sum");
                                    //if (checkField > -1)
                                    //    rowBuffer["Sum"] = parameter.SumValue;

                                    checkField = _definition.FindField("Search");
                                    if (checkField > -1)
                                        rowBuffer["Search"] = parameter.SearchCount;

                                    checkField = _definition.FindField("RasType");
                                    if (checkField > -1)
                                        rowBuffer["RasType"] = rasterType;

                                    checkField = _definition.FindField("Comment");
                                    if (checkField > -1)
                                        rowBuffer["Comment"] = "Calculated on: " + DateTime.Now.ToLongDateString();

                                    checkField = _definition.FindField("Cellsize");
                                    if (checkField > -1)
                                        rowBuffer["Cellsize"] = _parameters.SubCellsize;

                                    checkField = _definition.FindField("Interval");
                                    if (checkField > -1)
                                        rowBuffer["Interval"] = _parameters.Interval;

                                    checkField = _definition.FindField("XBlock");
                                    if (checkField > -1)
                                        rowBuffer["XBlock"] = _parameters.XBlocks;

                                    checkField = _definition.FindField("YBlock");
                                    if (checkField > -1)
                                        rowBuffer["YBlock"] = _parameters.YBlocks;

                                    checkField = _definition.FindField("TotalBlock");
                                    if (checkField > -1)
                                        rowBuffer["TotalBlock"] = _parameters.TotalBlocks;

                                    checkField = _definition.FindField("Area");
                                    if (checkField > -1)
                                        rowBuffer["Area"] = _parameters.SearchWindow;

                                    if (!bEntropy)
                                    {
                                        checkField = _definition.FindField("AngleF");
                                        if (checkField > -1)
                                            rowBuffer["AngleF"] = _parameters.RangeFrom;

                                        checkField = _definition.FindField("AngleT");
                                        if (checkField > -1)
                                            rowBuffer["AngleT"] = _parameters.RangeTo;
                                    }

                                    using (Feature feature = OuputFeatureClass.CreateRow(rowBuffer))
                                    {
                                        //To Indicate that the attribute table has to be updated
                                        context.Invalidate(feature);
                                    }
                                }

                            }
                        }
                    }

                }, OuputFeatureClass);

                try
                {
                    creationResult = editOperation.Execute();
                    if (!creationResult) message = editOperation.ErrorMessage;

                    Project.Current.SaveEditsAsync();

                }
                catch (Exception exObj)
                {
                    message = exObj.Message;

                }
                finally
                {

                }

            });

            return message;

        }

        public static async Task<string> SaveRasterToPoint(FeatureClass OuputFeatureClass, SpatialReference mySpatialRef,
    FlapParameters _parameters, RasterLineamentAnalysis _rasterAnalysis)
        {
            VectorFunctions.ProgressMessage = "Saving points...";
            await VectorFunctions.Progressor_NonCancelable();

            bool bEntropy = false;

            string rasterType = "";
            switch (_rasterAnalysis)
            {
                case RasterLineamentAnalysis.DensityLength:
                    rasterType = "Density Length";
                    break;
                case RasterLineamentAnalysis.DensityFrequency:
                    rasterType = "Density Frequency";
                    break;
                case RasterLineamentAnalysis.GroupMeansFrequency:
                    rasterType = "Group Mean Frequency";
                    break;
                case RasterLineamentAnalysis.GroupMeansLength:
                    rasterType = "Group Mean Length";
                    break;
                case RasterLineamentAnalysis.RelativeEntropy:
                    rasterType = "Relative Entropy";
                    bEntropy = true;
                    break;
                case RasterLineamentAnalysis.GroupDominanceLength:
                    rasterType = "Group Dominance Length";
                    break;
                case RasterLineamentAnalysis.GroupDominanceFrequency:
                    rasterType = "Group Dominance Frequency";
                    break;
                default:
                    break;
            }


            string message = String.Empty;
            bool creationResult = false;

            await QueuedTask.Run(() =>
            {
                EditOperation editOperation1 = new EditOperation();
                EditOperation editOperation = editOperation1;
                editOperation.Name = string.Format("Create features in layer {0}", rasterType + " Layer"); //TODO
                editOperation.SelectNewFeatures = true;

                editOperation.Callback(context =>
                {
                    FeatureClassDefinition _definition = OuputFeatureClass.GetDefinition();

                    foreach (FlapParameter parameter in _parameters.flapParameters)
                    {
                        if (parameter.CreateCell)
                        {
                            if (parameter.LenAzi.Count > 0)
                            {
                                using (RowBuffer rowBuffer = OuputFeatureClass.CreateRowBuffer())
                                {
                                    MapPoint pt1 = MapPointBuilderEx.CreateMapPoint(parameter.CentreX, parameter.CentreY, mySpatialRef);

                                    rowBuffer[_definition.GetShapeField()] = pt1;

                                    int checkField = -1;

                                    checkField = _definition.FindField("CellID");
                                    if (checkField > -1)
                                        rowBuffer["CellID"] = parameter.CellID;

                                    checkField = _definition.FindField("Count");
                                    if (checkField > -1)
                                        rowBuffer["Count"] = parameter.Count;

                                    checkField = _definition.FindField("Value");
                                    if (checkField > -1)
                                        rowBuffer["Value"] = parameter.GridValue;

                                    checkField = _definition.FindField("RasType");
                                    if (checkField > -1)
                                        rowBuffer["RasType"] = rasterType;

                                    checkField = _definition.FindField("Comment");
                                    if (checkField > -1)
                                        rowBuffer["Comment"] = "Calculated on: " + DateTime.Now.ToLongDateString();

                                    checkField = _definition.FindField("Cellsize");
                                    if (checkField > -1)
                                        rowBuffer["Cellsize"] = _parameters.SubCellsize;

                                    checkField = _definition.FindField("Interval");
                                    if (checkField > -1)
                                        rowBuffer["Interval"] = _parameters.Interval;

                                    if (!bEntropy)
                                    {
                                        checkField = _definition.FindField("AngleF");
                                        if (checkField > -1)
                                            rowBuffer["AngleF"] = _parameters.RangeFrom;

                                        checkField = _definition.FindField("AngleT");
                                        if (checkField > -1)
                                            rowBuffer["AngleT"] = _parameters.RangeTo;
                                    }

                                    using (Feature feature = OuputFeatureClass.CreateRow(rowBuffer))
                                    {
                                        //To Indicate that the attribute table has to be updated
                                        context.Invalidate(feature);
                                    }
                                }

                            }
                        }
                    }

                }, OuputFeatureClass);

                try
                {
                    creationResult = editOperation.Execute();
                    if (!creationResult) message = editOperation.ErrorMessage;

                    Project.Current.SaveEditsAsync();

                }
                catch (Exception exObj)
                {
                    message = exObj.Message;

                }
                finally
                {

                }

            });

            return message;

        }

        public static async Task<string> SaveRoseToVectorFeatures(FeatureClass OuputFeatureClass, SpatialReference mySpatialRef,
            FlapParameters _parameters)
        {
            VectorFunctions.ProgressMessage = "Saving Rose plots...";
            await VectorFunctions.Progressor_NonCancelable();

            string message = String.Empty;
            bool creationResult = false;

            await QueuedTask.Run(() =>
            {
                EditOperation editOperation = new EditOperation
                {
                    Name = string.Format("Create features in layer {0}", "Rose Layer"), //TODO
                    SelectNewFeatures = true
                };
                editOperation.Callback(context =>
                {
                    FeatureClassDefinition _definition = OuputFeatureClass.GetDefinition();

                    foreach (FlapParameter parameter in _parameters.flapParameters)
                    {
                        if (parameter.CreateCell)
                        {
                            if (parameter.LenAzi.Count > 0)
                            {
                                int counter = parameter.Rose.RoseArrayBin.GetUpperBound(0);

                                for (int i = 0; i < counter + 1; i++)
                                {
                                    using (RowBuffer rowBuffer = OuputFeatureClass.CreateRowBuffer())
                                    {
                                        MapPoint pt1 = MapPointBuilderEx.CreateMapPoint(parameter.CentreX, parameter.CentreY);
                                        MapPoint pt2 = MapPointBuilderEx.CreateMapPoint(parameter.Rose.RoseArrayBin[i, 0] + parameter.CentreX, parameter.Rose.RoseArrayBin[i, 1] + parameter.CentreY);
                                        MapPoint pt3 = MapPointBuilderEx.CreateMapPoint(parameter.Rose.RoseArrayBin[i, 2] + parameter.CentreX, parameter.Rose.RoseArrayBin[i, 3] + parameter.CentreY);
                                        List<MapPoint> list = new List<MapPoint>
                                    {
                                        pt1,
                                        pt2,
                                        pt3
                                    };

                                        // use the builder constructor
                                        Polygon poly = null;
                                        using (PolygonBuilder pb = new PolygonBuilder(list))
                                        {
                                            pb.SpatialReference = mySpatialRef;
                                            poly = pb.ToGeometry();
                                            rowBuffer[_definition.GetShapeField()] = poly;
                                        }
                                        int checkField = -1;

                                        checkField = _definition.FindField("CellID");
                                        if (checkField > -1)
                                            rowBuffer["CellID"] = parameter.CellID;

                                        checkField = _definition.FindField("AvgAzi");
                                        if (checkField > -1)
                                            rowBuffer["AvgAzi"] = parameter.Rose.AvgAzi[i];

                                        checkField = _definition.FindField("MinAzi");
                                        if (checkField > -1)
                                            rowBuffer["MinAzi"] = parameter.Rose.MinAzi[i];

                                        checkField = _definition.FindField("MaxAzi");
                                        if (checkField > -1)
                                            rowBuffer["MaxAzi"] = parameter.Rose.MaxAzi[i];

                                        checkField = _definition.FindField("StdAzi");
                                        if (checkField > -1)
                                            rowBuffer["StdAzi"] = parameter.Rose.StdAzi[i];

                                        checkField = _definition.FindField("BinCount");
                                        if (checkField > -1)
                                            rowBuffer["BinCount"] = parameter.Rose.BinCount[i];

                                        checkField = _definition.FindField("BinAzi");
                                        if (checkField > -1)
                                            rowBuffer["BinAzi"] = parameter.Rose.BinRange[i].LowerRange.ToString() + " - " + parameter.Rose.BinRange[i].UpperRange.ToString();


                                        checkField = _definition.FindField("AvgLength");
                                        if (checkField > -1)
                                            rowBuffer["AvgLength"] = parameter.Rose.AvgLength[i];

                                        checkField = _definition.FindField("MinLength");
                                        if (checkField > -1)
                                            rowBuffer["MinLength"] = parameter.Rose.MinLength[i];

                                        checkField = _definition.FindField("MaxLength");
                                        if (checkField > -1)
                                            rowBuffer["MaxLength"] = parameter.Rose.MaxLength[i];

                                        checkField = _definition.FindField("StdLen");
                                        if (checkField > -1)
                                            rowBuffer["StdLen"] = parameter.Rose.StdLength[i];

                                        checkField = _definition.FindField("Created");
                                        if (checkField > -1)
                                            rowBuffer["Created"] = "Created on: " + DateTime.Now.ToShortDateString() + " at: " + DateTime.Now.ToShortTimeString();


                                        using (Feature feature = OuputFeatureClass.CreateRow(rowBuffer))
                                        {
                                            //To Indicate that the attribute table has to be updated
                                            context.Invalidate(feature);

                                        }
                                    }
                                }
                            }
                        }
                    }

                }, OuputFeatureClass);

                try
                {
                    creationResult = editOperation.Execute();
                    if (!creationResult) message = editOperation.ErrorMessage;

                    Project.Current.SaveEditsAsync();

                }
                catch (Exception exObj)
                {
                    message = exObj.Message;

                }
                finally
                {

                }

            });

            return message;

        }

        public static async Task<string> SaveFishnetVectorFeatures(FeatureClass OuputFeatureClass, SpatialReference mySpatialRef,
            FlapParameters _parameters, bool bExtentOnly)
        {

            VectorFunctions.ProgressMessage = "Saving Fishnet...";
            await VectorFunctions.Progressor_NonCancelable();

            string message = String.Empty;
            bool creationResult = false;

            await QueuedTask.Run(() =>
            {
                EditOperation editOperation = new EditOperation
                {
                    Name = string.Format("Create features in layer {0}", "Fishnet Layer"), //TODO
                    SelectNewFeatures = true
                };

                editOperation.Callback(context =>
                {
                    FeatureClassDefinition _definition = OuputFeatureClass.GetDefinition();

                    foreach (FlapParameter parameter in _parameters.flapParameters)
                    {
                        if (parameter.CreateCell)
                        {
                            if (parameter.LenAzi.Count > 0 || bExtentOnly)
                            {
                                using (RowBuffer rowBuffer = OuputFeatureClass.CreateRowBuffer())
                                {
                                    MapPoint pt1 = MapPointBuilderEx.CreateMapPoint(parameter.XMin, parameter.YMin);
                                    MapPoint pt2 = MapPointBuilderEx.CreateMapPoint(parameter.XMin, parameter.YMax);
                                    MapPoint pt3 = MapPointBuilderEx.CreateMapPoint(parameter.XMax, parameter.YMax);
                                    MapPoint pt4 = MapPointBuilderEx.CreateMapPoint(parameter.XMax, parameter.YMin);

                                    List<MapPoint> list = new List<MapPoint>
                                {
                                    pt1,
                                    pt2,
                                    pt3,
                                    pt4
                                };

                                    // use the builder constructor
                                    Polygon poly = null;
                                    using (PolygonBuilder pb = new PolygonBuilder(list))
                                    {
                                        pb.SpatialReference = mySpatialRef;
                                        poly = pb.ToGeometry();
                                        rowBuffer[_definition.GetShapeField()] = poly;
                                    }
                                    int checkField = -1;

                                    checkField = _definition.FindField("CellID");
                                    if (checkField > -1)
                                        rowBuffer["CellID"] = parameter.CellID;

                                    checkField = _definition.FindField("AvgAzi");
                                    if (checkField > -1)
                                        rowBuffer["AvgAzi"] = parameter.FishStats.AziAvg;

                                    checkField = _definition.FindField("MinAzi");
                                    if (checkField > -1)
                                        rowBuffer["MinAzi"] = parameter.FishStats.AziMin;

                                    checkField = _definition.FindField("MaxAzi");
                                    if (checkField > -1)
                                        rowBuffer["MaxAzi"] = parameter.FishStats.AziMax;

                                    checkField = _definition.FindField("StdAzi");
                                    if (checkField > -1)
                                        rowBuffer["StdAzi"] = parameter.FishStats.AziStd;

                                    checkField = _definition.FindField("Count");
                                    if (checkField > -1)
                                        rowBuffer["Count"] = parameter.FishStats.Count;

                                    checkField = _definition.FindField("RoseType");
                                    if (checkField > -1)
                                        rowBuffer["RoseType"] = _parameters.RoseType;

                                    checkField = _definition.FindField("Comment");
                                    if (checkField > -1)
                                        rowBuffer["Comment"] = "Fishnet created on: " + DateTime.Now.ToShortDateString() + " at: " +
                                                DateTime.Now.ToShortTimeString();

                                    checkField = _definition.FindField("AvgLength");
                                    if (checkField > -1)
                                        rowBuffer["AvgLength"] = parameter.FishStats.LenAvg;

                                    checkField = _definition.FindField("MinLength");
                                    if (checkField > -1)
                                        rowBuffer["MinLength"] = parameter.FishStats.LenMin;

                                    checkField = _definition.FindField("MaxLength");
                                    if (checkField > -1)
                                        rowBuffer["MaxLength"] = parameter.FishStats.LenMax;

                                    checkField = _definition.FindField("SumLen");
                                    if (checkField > -1)
                                        rowBuffer["SumLen"] = parameter.FishStats.TotalLength;

                                    checkField = _definition.FindField("StdLen");
                                    if (checkField > -1)
                                        rowBuffer["StdLen"] = parameter.FishStats.LenStd;

                                    using (Feature feature = OuputFeatureClass.CreateRow(rowBuffer))
                                    {
                                        //To Indicate that the attribute table has to be updated
                                        context.Invalidate(feature);
                                    }
                                }

                            }
                        }
                    }

                }, OuputFeatureClass);

                try
                {
                    creationResult = editOperation.Execute();
                    if (!creationResult) message = editOperation.ErrorMessage;

                    Project.Current.SaveEditsAsync();

                }
                catch (Exception exObj)
                {
                    message = exObj.Message;

                }
                finally
                {

                }

            });

            return message;

        }

        public static async Task<FlapParameters> PrepareInputForProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize,
    int nInterval, int rangeFrom, int rangeTo, RoseGeom roseGeom, string fieldName)
        {
            VectorFunctions.ProgressMessage = "Processing cells...";
            await VectorFunctions.Progressor_NonCancelable();

            bool bGeographics = false;
            SpatialReference originalSpatial = null;

            int cellID = 1;

            FlapParameters _parameters = new FlapParameters
            {
                SubCellsize = subCellsize,
                Interval = nInterval,
                SelectedFeatures = false,
                RangeFrom = rangeFrom,
                RangeTo = rangeTo,
                flapParameters = new List<FlapParameter>()
            };

            #region Projection
            SpatialReference mySpatRef = null;

            await QueuedTask.Run(() =>
            {
                FeatureClass featClass = InputLayer.GetFeatureClass();
                FeatureClassDefinition _definition = featClass.GetDefinition();
                mySpatRef = _definition.GetSpatialReference();

            });

            originalSpatial = mySpatRef;

            //check if geographic or unprojected
            if (!mySpatRef.IsProjected)
            {
                if (mySpatRef.IsGeographic)
                {
                    bGeographics = true;
                    customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
                    mySpatRef = customEnvelope.SpatialReference;
                }

             //   if (!mySpatRef.IsUnknown)
              //  {
                   // originalSpatial = mySpatRef;

             //       customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
             //       mySpatRef = customEnvelope.SpatialReference;
                   // bGeographics = true;
               //}
               // else
               // {
                    //mySpatRef = await FeatureClassQuery.GetSpatialReferenceProp(); //assume same as active view

                    //if (mySpatRef.IsGeographic)
                    //{
                    //    originalSpatial = mySpatRef;

                    //    customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
                    //    mySpatRef = customEnvelope.SpatialReference;
                    //    bGeographics = true;
                    //}
                //}
            }

            _parameters.SetProperties(customEnvelope);
            #endregion

            double dblMinX = customEnvelope.XMin;
            double dblMaxY = customEnvelope.YMax;
            double dblMaxX = dblMinX + subCellsize;
            double dblMinY = dblMaxY - subCellsize;

            double dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
            double dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);

            await QueuedTask.Run(() =>
            {
                for (int y = 0; y < _parameters.NoOfRows; y++)
                {
                    for (int x = 0; x < _parameters.NoOfColumns; x++)
                    {
                        _parameters.flapParameters.Add(new FlapParameter
                        {
                            CentreX = dblCentreX,
                            CentreY = dblCentreY,
                            CreateCell = true,  //default
                            ExtentArea = subCellsize * subCellsize,
                            ExtentHeight = subCellsize,
                            ExtentWidth = subCellsize,
                            XMin = dblMinX,
                            XMax = dblMaxX,
                            YMin = dblMinY,
                            YMax = dblMaxY,
                            CellID = cellID

                        });

                        //Build polygon
                        MapPoint pt1 = MapPointBuilderEx.CreateMapPoint(dblMinX, dblMinY, mySpatRef);
                        MapPoint pt2 = MapPointBuilderEx.CreateMapPoint(dblMinX, dblMaxY, mySpatRef);
                        MapPoint pt3 = MapPointBuilderEx.CreateMapPoint(dblMaxX, dblMaxY, mySpatRef);
                        MapPoint pt4 = MapPointBuilderEx.CreateMapPoint(dblMaxX, dblMinY, mySpatRef);

                        List<MapPoint> list = new List<MapPoint>
                        {
                            pt1,
                            pt2,
                            pt3,
                            pt4
                        };

                        Polygon spoly = null;
                        using (PolygonBuilder pb = new PolygonBuilder(list))
                        {
                            pb.SpatialReference = mySpatRef;
                            spoly = pb.ToGeometry();
                        }

                        if (bGeographics) //unproject
                            //poly = FeatureClassQuery.ProjectPolygonToWGS84(poly).Result;
                            spoly = FeatureClassQuery.ProjectPolygonGeneric(spoly, originalSpatial).Result;

                        QueryFilter _querySquare = new SpatialQueryFilter
                        {
                            FilterGeometry = spoly,
                            SpatialRelationship = SpatialRelationship.Intersects,
                        };

                        int featCount = 0;
                        featCount = FeatureClassQuery.ReturnNoFeatures(InputLayer.GetFeatureClass(), _querySquare).Result; //see if any features

                        _parameters.flapParameters.Last().CellID = cellID;

                        if (featCount > 0)
                        {
                            _parameters.flapParameters.Last().CreateCell = true;
                            _parameters.flapParameters.Last().Count = featCount;

                            //clip lines or select points
                            using (RowCursor rowCursor = InputLayer.GetFeatureClass().Search(_querySquare, false))
                            {
                                if (roseGeom != RoseGeom.Point)  //LINES
                                {
                                    while (rowCursor.MoveNext())
                                    {
                                        using (Feature feature = (Feature)rowCursor.Current)
                                        {
                                            ArcGIS.Core.Geometry.Polyline polyline = feature.GetShape() as ArcGIS.Core.Geometry.Polyline;
                                            ArcGIS.Core.Geometry.Polyline calcLine = null;

                                            bool bClip = GeometryEngine.Instance.Crosses(polyline, spoly.Extent);

                                            if (bClip)
                                            {
                                                calcLine = GeometryEngine.Instance.Clip(polyline, spoly.Extent) as ArcGIS.Core.Geometry.Polyline;

                                            }
                                            else
                                            {
                                                calcLine = feature.GetShape() as ArcGIS.Core.Geometry.Polyline;
                                            }

                                            //if (bGeographics)
                                            //{
                                            //    calcLine = FeatureClassQuery.ProjectPolylineFromWGS84(calcLine).Result;
                                            //}

                                            _parameters.flapParameters.Last().LenAzi.Add(new FreqLen
                                            {
                                                Azimuth = CalcOrientation.ReturnOrient(calcLine),
                                                Length = calcLine.Length
                                            });
                                        }
                                    }
                                }
                                else  //POINTS
                                {
                                    while (rowCursor.MoveNext())
                                    {
                                        using (Feature feature = (Feature)rowCursor.Current)
                                        {
                                            _parameters.flapParameters.Last().LenAzi.Add(new FreqLen
                                            {
                                                Azimuth = Convert.ToDouble(feature[fieldName]),
                                                Length = 0
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _parameters.flapParameters.Last().CreateCell = false;
                        }

                        cellID++;

                        //CHANGE to X
                        dblMinX = dblMinX + subCellsize;
                        dblMaxX = dblMaxX + subCellsize;

                        dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
                        dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);
                    }

                    //Reset after changing Y
                    dblMinX = Math.Floor(Math.Floor(customEnvelope.XMin));
                    dblMaxX = dblMinX + subCellsize;

                    dblMinY = dblMinY - subCellsize;
                    dblMaxY = dblMaxY - subCellsize;

                    dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
                    dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);
                }
            });

            return _parameters;
        }

        public static async Task<FlapParameters> PrepareInputForProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize,
int nInterval, int rangeFrom, int rangeTo, RoseGeom roseGeom, string fieldName, bool bSelection)
        {
            VectorFunctions.ProgressMessage = "Processing cells...";
            await VectorFunctions.Progressor_NonCancelable();

            bool bGeographics = false;

            SpatialReference originalSpatial = null;

            int cellID = 1;

            FlapParameters _parameters = new FlapParameters
            {
                SubCellsize = subCellsize,
                Interval = nInterval,
                SelectedFeatures = bSelection,
                RangeFrom = rangeFrom,
                RangeTo = rangeTo,
                flapParameters = new List<FlapParameter>()
            };

            #region Projection
            SpatialReference mySpatRef = null;

            await QueuedTask.Run(() =>
            {
                FeatureClass featClass = InputLayer.GetFeatureClass();
                FeatureClassDefinition _definition = featClass.GetDefinition();
                mySpatRef = _definition.GetSpatialReference();

            });

            //check if geographic or unprojected
            //if (!mySpatRef.IsProjected)
            //{
            //    if (!mySpatRef.IsUnknown)
            //    {
            //        originalSpatial = mySpatRef;

            //        customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
            //        mySpatRef = customEnvelope.SpatialReference;
            //        bGeographics = true;
            //    }
            //    else
            //    {
            //        mySpatRef = await FeatureClassQuery.GetSpatialReferenceProp(); //assume same as active view

            //        if (mySpatRef.IsGeographic)
            //        {
            //            originalSpatial = mySpatRef;

            //            customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
            //            mySpatRef = customEnvelope.SpatialReference;
            //            bGeographics = true;
            //        }
            //    }
            //}

            if (!mySpatRef.IsProjected)
            {
                if (mySpatRef.IsGeographic)
                {
                     bGeographics = true;
                    customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
                    mySpatRef = customEnvelope.SpatialReference;
                }
            }

            _parameters.SetProperties(customEnvelope);
            #endregion

            double dblMinX = Math.Floor(customEnvelope.XMin);
            double dblMaxY = Math.Floor(customEnvelope.YMax);
            double dblMaxX = dblMinX + subCellsize;
            double dblMinY = dblMaxY - subCellsize;

            double dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
            double dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);

            await QueuedTask.Run(() =>
            {
                for (int y = 0; y < _parameters.NoOfRows; y++)
                {
                    for (int x = 0; x < _parameters.NoOfColumns; x++)
                    {
                        _parameters.flapParameters.Add(new FlapParameter
                        {
                            CentreX = dblCentreX,
                            CentreY = dblCentreY,
                            CreateCell = true,  //default
                            ExtentArea = subCellsize * subCellsize,
                            ExtentHeight = subCellsize,
                            ExtentWidth = subCellsize,
                            XMin = dblMinX,
                            XMax = dblMaxX,
                            YMin = dblMinY,
                            YMax = dblMaxY,
                            CellID = cellID

                        });

                        //Build polygon
                        MapPoint pt1 = MapPointBuilderEx.CreateMapPoint(dblMinX, dblMinY, mySpatRef);
                        MapPoint pt2 = MapPointBuilderEx.CreateMapPoint(dblMinX, dblMaxY, mySpatRef);
                        MapPoint pt3 = MapPointBuilderEx.CreateMapPoint(dblMaxX, dblMaxY, mySpatRef);
                        MapPoint pt4 = MapPointBuilderEx.CreateMapPoint(dblMaxX, dblMinY, mySpatRef);

                        List<MapPoint> list = new List<MapPoint>
                        {
                            pt1,
                            pt2,
                            pt3,
                            pt4
                        };

                        Polygon spoly = null;
                        using (PolygonBuilder pb = new PolygonBuilder(list))
                        {
                            pb.SpatialReference = mySpatRef;
                            spoly = pb.ToGeometry();
                        }

                        if (bGeographics) //unproject
                        //                  // poly = FeatureClassQuery.ProjectPolygonToWGS84(poly).Result;
                            spoly = FeatureClassQuery.ProjectPolygonGeneric(spoly, originalSpatial).Result;

                        QueryFilter _querySquare = new SpatialQueryFilter
                        {
                            FilterGeometry = spoly,
                            SpatialRelationship = SpatialRelationship.Intersects,
                        };

                        Selection selQuery = InputLayer.GetSelection();

                        selQuery.Select(_querySquare);

                        int featCount = (int)selQuery.Select(_querySquare).GetCount();

                        if (featCount > 0)
                        {
                            _parameters.flapParameters.Last().CellID = cellID;
                            _parameters.flapParameters.Last().CreateCell = true;
                            _parameters.flapParameters.Last().Count = featCount;
                            cellID++;

                            //clip lines or select points
                            using (RowCursor rowCursor = selQuery.Search(_querySquare, false))
                            {
                                if (roseGeom != RoseGeom.Point)  //LINES
                                {
                                    while (rowCursor.MoveNext())
                                    {
                                        using (Feature feature = (Feature)rowCursor.Current)
                                        {
                                            ArcGIS.Core.Geometry.Polyline polyline = feature.GetShape() as ArcGIS.Core.Geometry.Polyline;
                                            ArcGIS.Core.Geometry.Polyline calcLine = null;

                                            bool bClip = GeometryEngine.Instance.Crosses(polyline, spoly.Extent);

                                            if (bClip)
                                            {
                                                calcLine = GeometryEngine.Instance.Clip(polyline, spoly.Extent) as ArcGIS.Core.Geometry.Polyline;

                                            }
                                            else
                                            {
                                                calcLine = feature.GetShape() as ArcGIS.Core.Geometry.Polyline;
                                            }

                                            //if (bGeographics)
                                            //{
                                            //    calcLine = FeatureClassQuery.ProjectPolylineFromWGS84(calcLine).Result;
                                            //}

                                            _parameters.flapParameters.Last().LenAzi.Add(new FreqLen
                                            {
                                                Azimuth = CalcOrientation.ReturnOrient(calcLine),
                                                Length = calcLine.Length
                                            });
                                        }
                                    }
                                }
                                else  //POINTS
                                {
                                    while (rowCursor.MoveNext())
                                    {
                                        using (Feature feature = (Feature)rowCursor.Current)
                                        {
                                            _parameters.flapParameters.Last().LenAzi.Add(new FreqLen
                                            {
                                                Azimuth = Convert.ToDouble(feature[fieldName]),
                                                Length = 0
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _parameters.flapParameters.Last().CreateCell = false;
                        }



                        //CHANGE to X
                        dblMinX = dblMinX + subCellsize;
                        dblMaxX = dblMaxX + subCellsize;

                        dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
                        dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);
                    }

                    //Reset after changing Y
                    dblMinX = Math.Floor(Math.Floor(customEnvelope.XMin));
                    dblMaxX = dblMinX + subCellsize;

                    dblMinY = dblMinY - subCellsize;
                    dblMaxY = dblMaxY - subCellsize;

                    dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
                    dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);
                }
            });

            return _parameters;
        }

        public static async Task<FlapParameters> PrepareMovingStatisticsProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize,
int nInterval, int rangeFrom, int rangeTo, RoseGeom roseGeom, string fieldName, int XBlocks, int YBlocks, int TotalBlocks)
        {
            VectorFunctions.ProgressMessage = "Processing cells...";
            await VectorFunctions.Progressor_NonCancelable();

            bool bGeographics = false;
            SpatialReference originalSpatial = null;

            int cellID = 1;

            FlapParameters _parameters = new FlapParameters
            {
                SubCellsize = subCellsize,
                Interval = nInterval,
                SelectedFeatures = false,
                RangeFrom = rangeFrom,
                RangeTo = rangeTo,
                XBlocks = XBlocks,
                YBlocks = YBlocks,
                TotalBlocks = TotalBlocks,
                flapParameters = new List<FlapParameter>()
            };

            #region Projection
            SpatialReference mySpatRef = null;

            await QueuedTask.Run(() =>
            {
                FeatureClass featClass = InputLayer.GetFeatureClass();
                FeatureClassDefinition _definition = featClass.GetDefinition();
                mySpatRef = _definition.GetSpatialReference();

            });

            //check if geographic or unprojected
            if (!mySpatRef.IsProjected)
            {
                if (!mySpatRef.IsUnknown)
                {
                    originalSpatial = mySpatRef;

                    customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
                    mySpatRef = customEnvelope.SpatialReference;
                    bGeographics = true;
                }
                else
                {
                    mySpatRef = await FeatureClassQuery.GetSpatialReferenceProp(); //assume same as active view

                    if (mySpatRef.IsGeographic)
                    {
                        originalSpatial = mySpatRef;

                        customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
                        mySpatRef = customEnvelope.SpatialReference;
                        bGeographics = true;
                    }
                }
            }

            _parameters.SetProperties(customEnvelope);
            #endregion

            //blocks on either side of target + target
            double searchX = 1.0;
            double searchY = 1.0;

            if (TotalBlocks > 1)
            {
                searchX = (XBlocks * 2) + 1;
                searchY = (YBlocks * 2) + 1;
            }

            //search window extent
            double dblSearchCellX = searchX * subCellsize;
            double dblSearchCellY = searchY * subCellsize;

            //check against extent
            double dblWidthCheck = customEnvelope.Width / 2;
            double dblHeightCheck = customEnvelope.Height / 2;

            if (dblSearchCellX > dblWidthCheck)
                throw new Exception("Too many blocks in the X direction"); //TODO custom exception
            else if (dblSearchCellY > dblHeightCheck)
                throw new Exception("Too many blocks in the Y direction"); //TODO custom exception

            //Extent of target block - for storing values to create geometry later
            double dblMinX = Math.Floor(customEnvelope.XMin);
            double dblMaxY = Math.Floor(customEnvelope.YMax);
            double dblMaxX = dblMinX + subCellsize;
            double dblMinY = dblMaxY - subCellsize;

            double dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
            double dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);

            //divide search by 2
            double dblXExtent = 0.0;
            double dblYExtent = 0.0;

            dblXExtent = dblSearchCellX / 2;
            dblYExtent = dblSearchCellY / 2;

            //Extent of search block - for selecting features only
            double dblSearchMinX = dblCentreX - dblXExtent;
            double dblSearchMinY = dblCentreY - dblYExtent;
            double dblSearchMaxX = dblCentreX + dblXExtent;
            double dblSearchMaxY = dblCentreY + dblYExtent;

            await QueuedTask.Run(() =>
            {
                for (int y = 0; y < _parameters.NoOfRows; y++)
                {
                    for (int x = 0; x < _parameters.NoOfColumns; x++)
                    {
                        _parameters.flapParameters.Add(new FlapParameter
                        {
                            CentreX = dblCentreX,
                            CentreY = dblCentreY,
                            CreateCell = true,  //default
                            ExtentArea = subCellsize * subCellsize,
                            ExtentHeight = subCellsize,
                            ExtentWidth = subCellsize,
                            XMin = dblMinX,
                            XMax = dblMaxX,
                            YMin = dblMinY,
                            YMax = dblMaxY,
                            CellID = cellID

                        });

                        //Build search polygon
                        MapPoint spt1 = MapPointBuilderEx.CreateMapPoint(dblSearchMinX, dblSearchMinY, mySpatRef);
                        MapPoint spt2 = MapPointBuilderEx.CreateMapPoint(dblSearchMinX, dblSearchMaxY, mySpatRef);
                        MapPoint spt3 = MapPointBuilderEx.CreateMapPoint(dblSearchMaxX, dblSearchMaxY, mySpatRef);
                        MapPoint spt4 = MapPointBuilderEx.CreateMapPoint(dblSearchMaxX, dblSearchMinY, mySpatRef);

                        List<MapPoint> slist = new List<MapPoint>
                        {
                            spt1,
                            spt2,
                            spt3,
                            spt4
                        };

                        Polygon spoly = null;
                        using (PolygonBuilder spb = new PolygonBuilder(slist))
                        {
                            spb.SpatialReference = mySpatRef;
                            spoly = spb.ToGeometry();
                            _parameters.SearchWindow = spoly.Area;
                        }

                        if (bGeographics) //unproject
                            spoly = FeatureClassQuery.ProjectPolygonGeneric(spoly, originalSpatial).Result;

                        QueryFilter _squerySquare = new SpatialQueryFilter
                        {
                            FilterGeometry = spoly,
                            SpatialRelationship = SpatialRelationship.Intersects,
                        };

                        int featCount = 0;
                        featCount = FeatureClassQuery.ReturnNoFeatures(InputLayer.GetFeatureClass(), _squerySquare).Result; //see if any features

                        _parameters.flapParameters.Last().CellID = cellID;

                        if (featCount > 0)
                        {
                            _parameters.flapParameters.Last().CreateCell = true;
                            _parameters.flapParameters.Last().SearchCount = featCount;

                            //Check normal polygon
                            if (TotalBlocks > 1)
                            {
                                //build normal polygon
                                MapPoint npt1 = MapPointBuilderEx.CreateMapPoint(dblMinX, dblMinY, mySpatRef);
                                MapPoint npt2 = MapPointBuilderEx.CreateMapPoint(dblMinX, dblMaxY, mySpatRef);
                                MapPoint npt3 = MapPointBuilderEx.CreateMapPoint(dblMaxX, dblMaxY, mySpatRef);
                                MapPoint npt4 = MapPointBuilderEx.CreateMapPoint(dblMaxX, dblMinY, mySpatRef);

                                List<MapPoint> nlist = new List<MapPoint>
                                {
                                    npt1,
                                    npt2,
                                    npt3,
                                    npt4
                                };

                                Polygon npoly = null;
                                using (PolygonBuilder npb = new PolygonBuilder(nlist))
                                {
                                    npb.SpatialReference = mySpatRef;
                                    npoly = npb.ToGeometry();
                                }

                                if (bGeographics) //unproject
                                    npoly = FeatureClassQuery.ProjectPolygonGeneric(npoly, originalSpatial).Result;

                                QueryFilter _nquerySquare = new SpatialQueryFilter
                                {
                                    FilterGeometry = npoly,
                                    SpatialRelationship = SpatialRelationship.Intersects,
                                };

                                int nfeatCount = 0;
                                nfeatCount = FeatureClassQuery.ReturnNoFeatures(InputLayer.GetFeatureClass(), _nquerySquare).Result; //see if any features

                                if (nfeatCount > 0)
                                    _parameters.flapParameters.Last().Count = nfeatCount;
                                else
                                    _parameters.flapParameters.Last().Count = 0;
                            }
                            else
                                _parameters.flapParameters.Last().Count = featCount;

                            //clip lines or select points
                            using (RowCursor rowCursor = InputLayer.GetFeatureClass().Search(_squerySquare, false))
                            {
                                if (roseGeom != RoseGeom.Point)  //LINES
                                {
                                    while (rowCursor.MoveNext())
                                    {
                                        using (Feature feature = (Feature)rowCursor.Current)
                                        {
                                            ArcGIS.Core.Geometry.Polyline polyline = feature.GetShape() as ArcGIS.Core.Geometry.Polyline;
                                            ArcGIS.Core.Geometry.Polyline calcLine = null;

                                            bool bClip = GeometryEngine.Instance.Crosses(polyline, spoly.Extent);

                                            if (bClip)
                                            {
                                                calcLine = GeometryEngine.Instance.Clip(polyline, spoly.Extent) as ArcGIS.Core.Geometry.Polyline;

                                            }
                                            else
                                            {
                                                calcLine = feature.GetShape() as ArcGIS.Core.Geometry.Polyline;
                                            }

                                            if (bGeographics)
                                            {
                                                calcLine = FeatureClassQuery.ProjectPolylineFromWGS84(calcLine).Result;
                                            }

                                            _parameters.flapParameters.Last().LenAzi.Add(new FreqLen
                                            {
                                                Azimuth = CalcOrientation.ReturnOrient(calcLine),
                                                Length = calcLine.Length
                                            });
                                        }
                                    }
                                }
                                else  //POINTS
                                {
                                    while (rowCursor.MoveNext())
                                    {
                                        using (Feature feature = (Feature)rowCursor.Current)
                                        {
                                            _parameters.flapParameters.Last().LenAzi.Add(new FreqLen
                                            {
                                                Azimuth = Convert.ToDouble(feature[fieldName]),
                                                Length = 0
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _parameters.flapParameters.Last().CreateCell = false;
                        }

                        cellID++;

                        //CHANGE to X
                        dblMinX = dblMinX + subCellsize;
                        dblMaxX = dblMaxX + subCellsize;

                        dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
                        dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);

                        dblSearchMinX = dblCentreX - dblXExtent;
                        dblSearchMaxX = dblCentreX + dblXExtent;

                    }

                    //Reset after changing Y
                    dblMinX = Math.Floor(Math.Floor(customEnvelope.XMin));
                    dblMaxX = dblMinX + subCellsize;

                    dblMinY = dblMinY - subCellsize;
                    dblMaxY = dblMaxY - subCellsize;

                    dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
                    dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);

                    dblSearchMinY = dblCentreY - dblYExtent;
                    dblSearchMaxY = dblCentreY + dblYExtent;
                }
            });

            return _parameters;
        }

        public static async Task<FlapParameters> PrepareMovingStatisticsProcessing(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize,
int nInterval, int rangeFrom, int rangeTo, RoseGeom roseGeom, string fieldName, bool bSelection, int XBlocks, int YBlocks, int TotalBlocks)
        {
            VectorFunctions.ProgressMessage = "Processing cells...";
            await VectorFunctions.Progressor_NonCancelable();

            bool bGeographics = false;

            SpatialReference originalSpatial = null;

            int cellID = 1;

            FlapParameters _parameters = new FlapParameters
            {
                SubCellsize = subCellsize,
                Interval = nInterval,
                SelectedFeatures = false,
                RangeFrom = rangeFrom,
                RangeTo = rangeTo,
                XBlocks = XBlocks,
                YBlocks = YBlocks,
                TotalBlocks = TotalBlocks,
                flapParameters = new List<FlapParameter>()
            };

            #region Projection
            SpatialReference mySpatRef = null;

            await QueuedTask.Run(() =>
            {
                FeatureClass featClass = InputLayer.GetFeatureClass();
                FeatureClassDefinition _definition = featClass.GetDefinition();
                mySpatRef = _definition.GetSpatialReference();

            });

            //check if geographic or unprojected
            if (!mySpatRef.IsProjected)
            {
                if (!mySpatRef.IsUnknown)
                {
                    originalSpatial = mySpatRef;

                    customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
                    mySpatRef = customEnvelope.SpatialReference;
                    bGeographics = true;
                }
                else
                {
                    mySpatRef = await FeatureClassQuery.GetSpatialReferenceProp(); //assume same as active view

                    if (mySpatRef.IsGeographic)
                    {
                        originalSpatial = mySpatRef;

                        customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
                        mySpatRef = customEnvelope.SpatialReference;
                        bGeographics = true;
                    }
                }
            }

            _parameters.SetProperties(customEnvelope);
            #endregion

            //blocks on either side of target + target
            double searchX = 1.0;
            double searchY = 1.0;

            if (TotalBlocks > 1)
            {
                searchX = (XBlocks * 2) + 1;
                searchY = (YBlocks * 2) + 1;
            }

            //search window extent
            double dblSearchCellX = searchX * subCellsize;
            double dblSearchCellY = searchY * subCellsize;

            //check against extent
            double dblWidthCheck = customEnvelope.Width / 2;
            double dblHeightCheck = customEnvelope.Height / 2;

            if (dblSearchCellX > dblWidthCheck)
                throw new Exception("Too many blocks in the X direction"); //TODO custom exception
            else if (dblSearchCellY > dblHeightCheck)
                throw new Exception("Too many blocks in the Y direction"); //TODO custom exception

            double dblMinX = Math.Floor(customEnvelope.XMin);
            double dblMaxY = Math.Floor(customEnvelope.YMax);
            double dblMaxX = dblMinX + subCellsize;
            double dblMinY = dblMaxY - subCellsize;

            double dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
            double dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);

            //divide search by 2
            double dblXExtent = 0.0;
            double dblYExtent = 0.0;

            dblXExtent = dblSearchCellX / 2;
            dblYExtent = dblSearchCellY / 2;

            //Extent of search block - for selecting features only
            double dblSearchMinX = dblCentreX - dblXExtent;
            double dblSearchMinY = dblCentreY - dblYExtent;
            double dblSearchMaxX = dblCentreX + dblXExtent;
            double dblSearchMaxY = dblCentreY + dblYExtent;

            await QueuedTask.Run(() =>
            {
                for (int y = 0; y < _parameters.NoOfRows; y++)
                {
                    for (int x = 0; x < _parameters.NoOfColumns; x++)
                    {
                        _parameters.flapParameters.Add(new FlapParameter
                        {
                            CentreX = dblCentreX,
                            CentreY = dblCentreY,
                            CreateCell = true,  //default
                            ExtentArea = subCellsize * subCellsize,
                            ExtentHeight = subCellsize,
                            ExtentWidth = subCellsize,
                            XMin = dblMinX,
                            XMax = dblMaxX,
                            YMin = dblMinY,
                            YMax = dblMaxY,
                            CellID = cellID

                        });

                        //Build search polygon
                        MapPoint spt1 = MapPointBuilderEx.CreateMapPoint(dblSearchMinX, dblSearchMinY, mySpatRef);
                        MapPoint spt2 = MapPointBuilderEx.CreateMapPoint(dblSearchMinX, dblSearchMaxY, mySpatRef);
                        MapPoint spt3 = MapPointBuilderEx.CreateMapPoint(dblSearchMaxX, dblSearchMaxY, mySpatRef);
                        MapPoint spt4 = MapPointBuilderEx.CreateMapPoint(dblSearchMaxX, dblSearchMinY, mySpatRef);

                        List<MapPoint> slist = new List<MapPoint>
                        {
                            spt1,
                            spt2,
                            spt3,
                            spt4
                        };

                        Polygon spoly = null;
                        using (PolygonBuilder spb = new PolygonBuilder(slist))
                        {
                            spb.SpatialReference = mySpatRef;
                            spoly = spb.ToGeometry();
                            _parameters.SearchWindow = spoly.Area;
                        }

                        if (bGeographics) //unproject
                            spoly = FeatureClassQuery.ProjectPolygonGeneric(spoly, originalSpatial).Result;

                        QueryFilter _squerySquare = new SpatialQueryFilter
                        {
                            FilterGeometry = spoly,
                            SpatialRelationship = SpatialRelationship.Intersects,
                        };

                        Selection searchSelQuery = InputLayer.GetSelection();
                        Selection normalSelQuery = InputLayer.GetSelection();

                        searchSelQuery.Select(_squerySquare);

                        int featCount = (int)searchSelQuery.Select(_squerySquare).GetCount();

                        if (featCount > 0)
                        {
                            _parameters.flapParameters.Last().CellID = cellID;
                            _parameters.flapParameters.Last().CreateCell = true;
                            _parameters.flapParameters.Last().SearchCount = featCount;
                            cellID++;

                            //Check normal polygon
                            if (TotalBlocks > 1)
                            {
                                //build normal polygon
                                MapPoint npt1 = MapPointBuilderEx.CreateMapPoint(dblMinX, dblMinY, mySpatRef);
                                MapPoint npt2 = MapPointBuilderEx.CreateMapPoint(dblMinX, dblMaxY, mySpatRef);
                                MapPoint npt3 = MapPointBuilderEx.CreateMapPoint(dblMaxX, dblMaxY, mySpatRef);
                                MapPoint npt4 = MapPointBuilderEx.CreateMapPoint(dblMaxX, dblMinY, mySpatRef);

                                List<MapPoint> nlist = new List<MapPoint>
                                {
                                    npt1,
                                    npt2,
                                    npt3,
                                    npt4
                                };

                                Polygon npoly = null;
                                using (PolygonBuilder npb = new PolygonBuilder(nlist))
                                {
                                    npb.SpatialReference = mySpatRef;
                                    npoly = npb.ToGeometry();
                                }

                                if (bGeographics) //unproject
                                    npoly = FeatureClassQuery.ProjectPolygonGeneric(npoly, originalSpatial).Result;

                                QueryFilter _nquerySquare = new SpatialQueryFilter
                                {
                                    FilterGeometry = npoly,
                                    SpatialRelationship = SpatialRelationship.Intersects,
                                };

                                normalSelQuery.Select(_nquerySquare);

                                int nfeatCount = 0;

                                if (nfeatCount > 0)
                                    _parameters.flapParameters.Last().Count = nfeatCount;
                                else
                                    _parameters.flapParameters.Last().Count = 0;
                            }
                            else
                                _parameters.flapParameters.Last().Count = featCount;

                            //clip lines or select points
                            using (RowCursor rowCursor = searchSelQuery.Search(_squerySquare, false))
                            {
                                if (roseGeom != RoseGeom.Point)  //LINES
                                {
                                    while (rowCursor.MoveNext())
                                    {
                                        using (Feature feature = (Feature)rowCursor.Current)
                                        {
                                            ArcGIS.Core.Geometry.Polyline polyline = feature.GetShape() as ArcGIS.Core.Geometry.Polyline;
                                            ArcGIS.Core.Geometry.Polyline calcLine = null;

                                            bool bClip = GeometryEngine.Instance.Crosses(polyline, spoly.Extent);

                                            if (bClip)
                                            {

                                                calcLine = GeometryEngine.Instance.Clip(polyline, spoly.Extent) as ArcGIS.Core.Geometry.Polyline;

                                            }
                                            else
                                            {
                                                calcLine = feature.GetShape() as ArcGIS.Core.Geometry.Polyline;
                                            }

                                            if (bGeographics)
                                            {
                                                calcLine = FeatureClassQuery.ProjectPolylineFromWGS84(calcLine).Result;
                                            }

                                            _parameters.flapParameters.Last().LenAzi.Add(new FreqLen
                                            {
                                                Azimuth = CalcOrientation.ReturnOrient(calcLine),
                                                Length = calcLine.Length
                                            });
                                        }
                                    }
                                }
                                else  //POINTS
                                {
                                    while (rowCursor.MoveNext())
                                    {
                                        using (Feature feature = (Feature)rowCursor.Current)
                                        {
                                            _parameters.flapParameters.Last().LenAzi.Add(new FreqLen
                                            {
                                                Azimuth = Convert.ToDouble(feature[fieldName]),
                                                Length = 0
                                            });
                                        }
                                    }
                                }
                            }
                        }
                        else
                        {
                            _parameters.flapParameters.Last().CreateCell = false;
                        }



                        //CHANGE to X
                        dblMinX = dblMinX + subCellsize;
                        dblMaxX = dblMaxX + subCellsize;

                        dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
                        dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);

                        dblSearchMinX = dblCentreX - dblXExtent;
                        dblSearchMaxX = dblCentreX + dblXExtent;
                    }

                    //Reset after changing Y
                    dblMinX = Math.Floor(Math.Floor(customEnvelope.XMin));
                    dblMaxX = dblMinX + subCellsize;

                    dblMinY = dblMinY - subCellsize;
                    dblMaxY = dblMaxY - subCellsize;

                    dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
                    dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);

                    dblSearchMinY = dblCentreY - dblYExtent;
                    dblSearchMaxY = dblCentreY + dblYExtent;
                }
            });

            return _parameters;
        }

        #region Regional
        public static async Task<FlapParameters> PrepareInputForRegionalProcessing(FeatureLayer InputLayer, Envelope customEnvelope,
    int nInterval, RoseGeom roseGeom, string fieldName)
        {
            VectorFunctions.ProgressMessage = "Processing cells...";
            await VectorFunctions.Progressor_NonCancelable();

            bool bGeographics = false;

            int cellID = 1;

            FlapParameters regionalRose = new FlapParameters
            {
                Interval = nInterval,
                SelectedFeatures = false,
                flapParameters = new List<FlapParameter>()
            };


            #region Projection
            SpatialReference mySpatRef = null;

            await QueuedTask.Run(() =>
            {
                FeatureClass featClass = InputLayer.GetFeatureClass();
                FeatureClassDefinition _definition = featClass.GetDefinition();
                mySpatRef = _definition.GetSpatialReference();

            });

            ////check if geographic or unprojected
            //if (!mySpatRef.IsProjected)
            //{
            //    if (!mySpatRef.IsUnknown)
            //    {
            //        customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
            //        mySpatRef = customEnvelope.SpatialReference;
            //        bGeographics = true;
            //    }
            //    else
            //    {
            //        mySpatRef = await FeatureClassQuery.GetSpatialReferenceProp(); //assume same as active view

            //        if (mySpatRef.IsGeographic)
            //        {
            //            customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
            //            mySpatRef = customEnvelope.SpatialReference;
            //            bGeographics = true;
            //        }
            //    }
            //}

            if (mySpatRef.IsGeographic)
                bGeographics = true;

            #endregion
            double dblMinX = 0.0;
            double dblMaxX = 0.0;
            double dblMinY = 0.0;
            double dblMaxY = 0.0;

            //if (!bGeographics)
            //{
            //    //TOTAL extent
            //    dblMinX = Math.Floor(customEnvelope.XMin);
            //    dblMaxY = Math.Floor(customEnvelope.YMax) + 1;

            //    dblMaxX = Math.Floor(customEnvelope.XMax) + 1;
            //    dblMinY = Math.Floor(customEnvelope.YMin);
           // }
           // else
           //{
                //TOTAL extent
                dblMinX = customEnvelope.XMin;
                dblMaxY = customEnvelope.YMax;

                dblMaxX = customEnvelope.XMax;
                dblMinY = customEnvelope.YMin;
            //}

            double dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
            double dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);

            CancelableProgressorSource cps = new CancelableProgressorSource("Calculating lines", "Cancelled");

            await QueuedTask.Run(() =>
            {
                regionalRose.flapParameters.Add(new FlapParameter
                {
                    CentreX = dblCentreX,
                    CentreY = dblCentreY,
                    CreateCell = true,  //default
                    ExtentArea = customEnvelope.Area,
                    ExtentHeight = dblMaxY - dblMinY,
                    ExtentWidth = dblMaxX - dblMinX,
                    XMin = dblMinX,
                    XMax = dblMaxX,
                    YMin = dblMinY,
                    YMax = dblMaxY,
                    CellID = cellID

                });

                cps.Progressor.Max = (uint)FeatureClassQuery.ReturnNoFeatures(InputLayer.GetFeatureClass()).Result;

                FeatureClass featClass = InputLayer.GetFeatureClass();

                using (RowCursor rowCursor = featClass.Search(null, false))
                {
                    while (rowCursor.MoveNext() && !cps.Progressor.CancellationToken.IsCancellationRequested)
                    {
                        if (roseGeom == RoseGeom.Line)
                        {
                            using (Feature feature = (Feature)rowCursor.Current)
                            {
                                ArcGIS.Core.Geometry.Polyline polyline = feature.GetShape() as ArcGIS.Core.Geometry.Polyline;

                                //Project if input in geographics
                                if (bGeographics)
                                    polyline = FeatureClassQuery.ProjectPolylineFromWGS84(polyline).Result;

                                ReadOnlyPartCollection lineParts = polyline.Parts;

                                IEnumerator<ReadOnlySegmentCollection> segments = lineParts.GetEnumerator();
                                while (segments.MoveNext())
                                {
                                    ReadOnlySegmentCollection seg = segments.Current;
                                    foreach (Segment s in seg)
                                    {
                                        regionalRose.flapParameters.Last().LenAzi.Add(new FreqLen
                                        {
                                            Azimuth = CalcOrientation.ReturnOrient(s as ArcGIS.Core.Geometry.LineSegment),
                                            Length = CalcOrientation.ReturnLength(s as ArcGIS.Core.Geometry.LineSegment)
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            using (Feature feature = (Feature)rowCursor.Current)
                            {

                                regionalRose.flapParameters.Last().LenAzi.Add(new FreqLen
                                {
                                    Azimuth = Convert.ToDouble(feature[fieldName]),
                                    Length = 0
                                });
                            }
                        }
                        cps.Progressor.Value += 1;
                        cps.Progressor.Status = "Status " + cps.Progressor.Value;
                    }

                }

            }, cps.Progressor);

            return regionalRose;
        }

        public static async Task<FlapParameters> PrepareInputForRegionalProcessing(FeatureLayer InputLayer, Envelope customEnvelope,
int nInterval, RoseGeom roseGeom, string fieldName, bool bSelection)
        {
            VectorFunctions.ProgressMessage = "Processing cells...";
            await VectorFunctions.Progressor_NonCancelable();

            bool bGeographics = false;

            int cellID = 1;

            FlapParameters regionalRose = new FlapParameters
            {
                Interval = nInterval,
                SelectedFeatures = bSelection,
                flapParameters = new List<FlapParameter>()
            };


            #region Projection
            SpatialReference mySpatRef = null;

            await QueuedTask.Run(() =>
            {
                FeatureClass featClass = InputLayer.GetFeatureClass();
                FeatureClassDefinition _definition = featClass.GetDefinition();
                mySpatRef = _definition.GetSpatialReference();

            });

            //check if geographic or unprojected
            if (!mySpatRef.IsProjected)
            {
                if (!mySpatRef.IsUnknown)
                {
                    customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
                    mySpatRef = customEnvelope.SpatialReference;
                    bGeographics = true;
                }
                else
                {
                    mySpatRef = await FeatureClassQuery.GetSpatialReferenceProp(); //assume same as active view

                    if (mySpatRef.IsGeographic)
                    {
                        customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
                        mySpatRef = customEnvelope.SpatialReference;
                        bGeographics = true;
                    }
                }
            }
            #endregion

            //TOTAL extent
            double dblMinX = Math.Floor(customEnvelope.XMin);
            double dblMaxY = Math.Floor(customEnvelope.YMax) + 1;

            double dblMaxX = Math.Floor(customEnvelope.XMax) + 1;
            double dblMinY = Math.Floor(customEnvelope.YMin);

            double dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
            double dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);

            CancelableProgressorSource cps = new CancelableProgressorSource("Calculating lines", "Cancelled");

            await QueuedTask.Run(() =>
            {
                regionalRose.flapParameters.Add(new FlapParameter
                {
                    CentreX = dblCentreX,
                    CentreY = dblCentreY,
                    CreateCell = true,  //default
                    ExtentArea = customEnvelope.Area,
                    ExtentHeight = dblMaxY - dblMinY,
                    ExtentWidth = dblMaxX - dblMinX,
                    XMin = dblMinX,
                    XMax = dblMaxX,
                    YMin = dblMinY,
                    YMax = dblMaxY,
                    CellID = cellID

                });

                cps.Progressor.Max = (uint)FeatureClassQuery.ReturnNoFeatures(InputLayer.GetFeatureClass()).Result;

                using (RowCursor rowCursor = InputLayer.GetSelection().Search(null, false))
                {
                    while (rowCursor.MoveNext() && !cps.Progressor.CancellationToken.IsCancellationRequested)
                    {
                        if (roseGeom == RoseGeom.Line)
                        {
                            using (Feature feature = (Feature)rowCursor.Current)
                            {
                                ArcGIS.Core.Geometry.Polyline polyline = feature.GetShape() as ArcGIS.Core.Geometry.Polyline;

                                //Project if input in geographics
                                if (bGeographics)
                                    polyline = FeatureClassQuery.ProjectPolylineFromWGS84(polyline).Result;

                                ReadOnlyPartCollection lineParts = polyline.Parts;

                                IEnumerator<ReadOnlySegmentCollection> segments = lineParts.GetEnumerator();
                                while (segments.MoveNext())
                                {
                                    ReadOnlySegmentCollection seg = segments.Current;
                                    foreach (Segment s in seg)
                                    {
                                        regionalRose.flapParameters.Last().LenAzi.Add(new FreqLen
                                        {
                                            Azimuth = CalcOrientation.ReturnOrient(s as ArcGIS.Core.Geometry.LineSegment),
                                            Length = CalcOrientation.ReturnLength(s as ArcGIS.Core.Geometry.LineSegment)
                                        });
                                    }
                                }
                            }
                        }
                        else
                        {
                            using (Feature feature = (Feature)rowCursor.Current)
                            {

                                regionalRose.flapParameters.Last().LenAzi.Add(new FreqLen
                                {
                                    Azimuth = Convert.ToDouble(feature[fieldName]),
                                    Length = 0
                                });
                            }
                        }
                        cps.Progressor.Value += 1;
                        cps.Progressor.Status = "Status " + cps.Progressor.Value;
                    }

                }

            }, cps.Progressor);

            return regionalRose;
        }

        public static async Task<FlapParameters> PrepareInputForRegionalFishnet(FeatureLayer InputLayer, Envelope customEnvelope,
int nInterval, RoseGeom roseGeom, string fieldName)
        {
            VectorFunctions.ProgressMessage = "Processing Fishnet...";
            await VectorFunctions.Progressor_NonCancelable();

            int cellID = 1;

            FlapParameters regionalRose = new FlapParameters
            {
                Interval = nInterval,
                SelectedFeatures = false,
                flapParameters = new List<FlapParameter>()
            };

            #region Projection
            SpatialReference mySpatRef = null;

            await QueuedTask.Run(() =>
            {
                FeatureClass featClass = InputLayer.GetFeatureClass();
                FeatureClassDefinition _definition = featClass.GetDefinition();
                mySpatRef = _definition.GetSpatialReference();

            });

            //check if geographic or unprojected
            if (!mySpatRef.IsProjected)
            {
                if (!mySpatRef.IsUnknown)
                {
                    customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
                    mySpatRef = customEnvelope.SpatialReference;
                }
                else
                {
                    mySpatRef = await FeatureClassQuery.GetSpatialReferenceProp(); //assume same as active view

                    if (mySpatRef.IsGeographic)
                    {
                        customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
                        mySpatRef = customEnvelope.SpatialReference;
                    }
                }
            }
            #endregion

            //TOTAL extent
            double dblMinX = Math.Floor(customEnvelope.XMin);
            double dblMaxY = Math.Floor(customEnvelope.YMax) + 1;

            double dblMaxX = Math.Floor(customEnvelope.XMax) + 1;
            double dblMinY = Math.Floor(customEnvelope.YMin);

            double dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
            double dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);

            CancelableProgressorSource cps = new CancelableProgressorSource("Calculating Cells", "Cancelled");

            await QueuedTask.Run(() =>
            {
                regionalRose.flapParameters.Add(new FlapParameter
                {
                    CentreX = dblCentreX,
                    CentreY = dblCentreY,
                    CreateCell = true,  //default
                    ExtentArea = customEnvelope.Area,
                    ExtentHeight = dblMaxY - dblMinY,
                    ExtentWidth = dblMaxX - dblMinX,
                    XMin = dblMinX,
                    XMax = dblMaxX,
                    YMin = dblMinY,
                    YMax = dblMaxY,
                    CellID = cellID

                });
            });

            return regionalRose;
        }
        #endregion

        #region Fishnet
        public static async Task<FlapParameters> PrepareInputForCellFishnet(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize,
int nInterval, int rangeFrom, int rangeTo, RoseGeom roseGeom, string fieldName)
        {
            VectorFunctions.ProgressMessage = "Processing Fishnet...";
            await VectorFunctions.Progressor_NonCancelable();

            bool bGeographics = false;
            SpatialReference originalSpatial = null;

            int cellID = 1;

            FlapParameters _parameters = new FlapParameters
            {
                SubCellsize = subCellsize,
                Interval = nInterval,
                SelectedFeatures = false,
                RangeFrom = rangeFrom,
                RangeTo = rangeTo,
                flapParameters = new List<FlapParameter>()
            };

            #region Projection
            SpatialReference mySpatRef = null;

            await QueuedTask.Run(() =>
            {
                FeatureClass featClass = InputLayer.GetFeatureClass();
                FeatureClassDefinition _definition = featClass.GetDefinition();
                mySpatRef = _definition.GetSpatialReference();

            });

            //check if geographic or unprojected
            if (!mySpatRef.IsProjected)
            {
                if (!mySpatRef.IsUnknown)
                {
                    originalSpatial = mySpatRef;

                    customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
                    mySpatRef = customEnvelope.SpatialReference;
                    bGeographics = true;
                }
                else
                {
                    mySpatRef = await FeatureClassQuery.GetSpatialReferenceProp(); //assume same as active view

                    if (mySpatRef.IsGeographic)
                    {
                        originalSpatial = mySpatRef;

                        customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
                        mySpatRef = customEnvelope.SpatialReference;
                        bGeographics = true;
                    }
                }
            }

            _parameters.SetProperties(customEnvelope);
            #endregion

            double dblMinX = Math.Floor(customEnvelope.XMin);
            double dblMaxY = Math.Floor(customEnvelope.YMax);
            double dblMaxX = dblMinX + subCellsize;
            double dblMinY = dblMaxY - subCellsize;

            double dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
            double dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);

            await QueuedTask.Run(() =>
            {
                for (int y = 0; y < _parameters.NoOfRows; y++)
                {
                    for (int x = 0; x < _parameters.NoOfColumns; x++)
                    {
                        _parameters.flapParameters.Add(new FlapParameter
                        {
                            CentreX = dblCentreX,
                            CentreY = dblCentreY,
                            CreateCell = true,  //default
                            ExtentArea = subCellsize * subCellsize,
                            ExtentHeight = subCellsize,
                            ExtentWidth = subCellsize,
                            XMin = dblMinX,
                            XMax = dblMaxX,
                            YMin = dblMinY,
                            YMax = dblMaxY,
                            CellID = cellID

                        });

                        //Build polygon
                        MapPoint pt1 = MapPointBuilderEx.CreateMapPoint(dblMinX, dblMinY, mySpatRef);
                        MapPoint pt2 = MapPointBuilderEx.CreateMapPoint(dblMinX, dblMaxY, mySpatRef);
                        MapPoint pt3 = MapPointBuilderEx.CreateMapPoint(dblMaxX, dblMaxY, mySpatRef);
                        MapPoint pt4 = MapPointBuilderEx.CreateMapPoint(dblMaxX, dblMinY, mySpatRef);

                        List<MapPoint> list = new List<MapPoint>
                        {
                            pt1,
                            pt2,
                            pt3,
                            pt4
                        };

                        Polygon poly = null;
                        using (PolygonBuilder pb = new PolygonBuilder(list))
                        {
                            pb.SpatialReference = mySpatRef;
                            poly = pb.ToGeometry();
                        }

                        if (bGeographics) //unproject
                            poly = FeatureClassQuery.ProjectPolygonGeneric(poly, originalSpatial).Result;

                        QueryFilter _querySquare = new SpatialQueryFilter
                        {
                            FilterGeometry = poly,
                            SpatialRelationship = SpatialRelationship.Intersects,
                        };

                        int featCount = 0;
                        featCount = FeatureClassQuery.ReturnNoFeatures(InputLayer.GetFeatureClass(), _querySquare).Result; //see if any features

                        _parameters.flapParameters.Last().CellID = cellID;

                        if (featCount > 0)
                        {
                            _parameters.flapParameters.Last().CreateCell = true;
                            _parameters.flapParameters.Last().Count = featCount;
                        }
                        else
                        {
                            _parameters.flapParameters.Last().CreateCell = false;
                        }

                        cellID++;

                        //CHANGE to X
                        dblMinX = dblMinX + subCellsize;
                        dblMaxX = dblMaxX + subCellsize;

                        dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
                        dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);
                    }

                    //Reset after changing Y
                    dblMinX = Math.Floor(Math.Floor(customEnvelope.XMin));
                    dblMaxX = dblMinX + subCellsize;

                    dblMinY = dblMinY - subCellsize;
                    dblMaxY = dblMaxY - subCellsize;

                    dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
                    dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);
                }
            });

            return _parameters;
        }

        public static async Task<FlapParameters> PrepareInputForCellFishnet(FeatureLayer InputLayer, Envelope customEnvelope, int subCellsize,
int nInterval, int rangeFrom, int rangeTo, RoseGeom roseGeom, string fieldName, bool bSelected)
        {
            VectorFunctions.ProgressMessage = "Processing Fishnet...";
            await VectorFunctions.Progressor_NonCancelable();

            bool bGeographics = false;
            SpatialReference originalSpatial = null;

            int cellID = 1;

            FlapParameters _parameters = new FlapParameters
            {
                SubCellsize = subCellsize,
                Interval = nInterval,
                SelectedFeatures = true,
                RangeFrom = rangeFrom,
                RangeTo = rangeTo,
                flapParameters = new List<FlapParameter>()
            };

            #region Projection
            SpatialReference mySpatRef = null;

            await QueuedTask.Run(() =>
            {
                FeatureClass featClass = InputLayer.GetFeatureClass();
                FeatureClassDefinition _definition = featClass.GetDefinition();
                mySpatRef = _definition.GetSpatialReference();

            });

            //check if geographic or unprojected
            if (!mySpatRef.IsProjected)
            {
                if (!mySpatRef.IsUnknown)
                {
                    originalSpatial = mySpatRef;

                    customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
                    mySpatRef = customEnvelope.SpatialReference;
                    bGeographics = true;
                }
                else
                {
                    mySpatRef = await FeatureClassQuery.GetSpatialReferenceProp(); //assume same as active view

                    if (mySpatRef.IsGeographic)
                    {
                        originalSpatial = mySpatRef;

                        customEnvelope = await FeatureClassQuery.ProjectFromWGS84(customEnvelope);
                        mySpatRef = customEnvelope.SpatialReference;
                        bGeographics = true;
                    }
                }
            }

            _parameters.SetProperties(customEnvelope);
            #endregion

            double dblMinX = Math.Floor(customEnvelope.XMin);
            double dblMaxY = Math.Floor(customEnvelope.YMax);
            double dblMaxX = dblMinX + subCellsize;
            double dblMinY = dblMaxY - subCellsize;

            double dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
            double dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);

            await QueuedTask.Run(() =>
            {
                for (int y = 0; y < _parameters.NoOfRows; y++)
                {
                    for (int x = 0; x < _parameters.NoOfColumns; x++)
                    {
                        _parameters.flapParameters.Add(new FlapParameter
                        {
                            CentreX = dblCentreX,
                            CentreY = dblCentreY,
                            CreateCell = true,  //default
                            ExtentArea = subCellsize * subCellsize,
                            ExtentHeight = subCellsize,
                            ExtentWidth = subCellsize,
                            XMin = dblMinX,
                            XMax = dblMaxX,
                            YMin = dblMinY,
                            YMax = dblMaxY,
                            CellID = cellID

                        });

                        //Build polygon
                        MapPoint pt1 = MapPointBuilderEx.CreateMapPoint(dblMinX, dblMinY, mySpatRef);
                        MapPoint pt2 = MapPointBuilderEx.CreateMapPoint(dblMinX, dblMaxY, mySpatRef);
                        MapPoint pt3 = MapPointBuilderEx.CreateMapPoint(dblMaxX, dblMaxY, mySpatRef);
                        MapPoint pt4 = MapPointBuilderEx.CreateMapPoint(dblMaxX, dblMinY, mySpatRef);

                        List<MapPoint> list = new List<MapPoint>
                        {
                            pt1,
                            pt2,
                            pt3,
                            pt4
                        };

                        Polygon poly = null;
                        using (PolygonBuilder pb = new PolygonBuilder(list))
                        {
                            pb.SpatialReference = mySpatRef;
                            poly = pb.ToGeometry();
                        }

                        if (bGeographics) //unproject
                            poly = FeatureClassQuery.ProjectPolygonGeneric(poly, originalSpatial).Result;

                        QueryFilter _querySquare = new SpatialQueryFilter
                        {
                            FilterGeometry = poly,
                            SpatialRelationship = SpatialRelationship.Intersects,
                        };

                        Selection selQuery = InputLayer.GetSelection();

                        selQuery.Select(_querySquare);


                        int featCount = (int)selQuery.Select(_querySquare).GetCount();

                        if (featCount > 0)
                        {
                            _parameters.flapParameters.Last().CellID = cellID;
                            _parameters.flapParameters.Last().CreateCell = true;
                            _parameters.flapParameters.Last().Count = featCount;

                            cellID++;
                        }
                        else
                        {
                            _parameters.flapParameters.Last().CreateCell = false;
                        }

                        //CHANGE to X
                        dblMinX = dblMinX + subCellsize;
                        dblMaxX = dblMaxX + subCellsize;

                        dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
                        dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);
                    }

                    //Reset after changing Y
                    dblMinX = Math.Floor(Math.Floor(customEnvelope.XMin));
                    dblMaxX = dblMinX + subCellsize;

                    dblMinY = dblMinY - subCellsize;
                    dblMaxY = dblMaxY - subCellsize;

                    dblCentreX = dblMinX + ((dblMaxX - dblMinX) / 2);
                    dblCentreY = dblMinY + ((dblMaxY - dblMinY) / 2);
                }
            });

            return _parameters;
        }
        #endregion

        public static async Task Progressor_NonCancelable()
        {
            ArcGIS.Desktop.Framework.Threading.Tasks.ProgressorSource ps = new ArcGIS.Desktop.Framework.Threading.Tasks.ProgressorSource(ProgressMessage, false);

            int numSecondsDelay = 3;
            //If you run this in the DEBUGGER you will NOT see the dialog
            await QueuedTask.Run(() => Task.Delay(numSecondsDelay * 1000).Wait(), ps.Progressor);
        }


    }
}
