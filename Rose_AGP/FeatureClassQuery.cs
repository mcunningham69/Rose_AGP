using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rose_AGP
{
    public static class FeatureClassQuery
    {
        public static async Task<bool> FeatureclassExists(string databasePath, string roseName)
        {
            bool bCheck = true;

            await QueuedTask.Run(() =>
            {
                try
                {
                    Geodatabase geodatabase = new Geodatabase(new FileGeodatabaseConnectionPath(new Uri(databasePath)));

                    FeatureClassDefinition featureClassDefinition = geodatabase.GetDefinition<FeatureClassDefinition>(roseName);
                    featureClassDefinition.Dispose();
                    bCheck = true;
                }
                catch
                {
                    bCheck = false;
                }



            });

            return bCheck;

        }

        public static async Task<int> ReturnNoFeatures(FeatureClass featClass)
        {
            int nId = 0;

            await QueuedTask.Run(() =>
            {

                nId = (int)featClass.GetCount();

            });

            return nId;
        }

        public static async Task<int> ReturnNoFeatures(FeatureClass featClass, QueryFilter _query)
        {
            int nId = 0;

            await QueuedTask.Run(() =>
            {

                nId = (int)featClass.GetCount(_query);

            });

            return nId;
        }

        public static async Task<int> ReturnCellIdFromAttributes(FeatureClass featClass)
        {
            int nId = 0;

            List<int> cellIDs = new List<int>();

            await QueuedTask.Run(() =>
            {

                FeatureClassDefinition _definition = featClass.GetDefinition();

                //get field index of TD for adding value to attribute table
                int nField = _definition.FindField("CellID");


                RowCursor rowCursor = featClass.Search(null, true);
                {
                    while (rowCursor.MoveNext())
                    {
                        Feature feat = (Feature)rowCursor.Current;

                        var temp = feat["CellID"];

                        cellIDs.Add((int)temp);
                    }
                }
            });

            nId = cellIDs.LastOrDefault();
            return nId;
        }

        public static async Task<Envelope> ReturnExtent(FeatureLayer layer)
        {
            Envelope CellsizeExtent = null;

            await QueuedTask.Run(() =>
            {
                FeatureClass featClass = layer.GetFeatureClass();
                CellsizeExtent = featClass.GetExtent();

            });

            return CellsizeExtent; ;
        }

        public static async Task<IReadOnlyList<Field>> GetFieldNames(FeatureLayer layer)
        {
            IReadOnlyList<Field> _fields = null;

            await QueuedTask.Run(() =>
            {
                FeatureClassDefinition _definition = layer.GetFeatureClass().GetDefinition();

                _fields = _definition.GetFields();

            });

            return _fields;
        }

        //works on a selection instead of the whole input layer
        public static async Task<Envelope> ExtentFromSelectectedInput(FeatureLayer InputLayer)
        {
            if (InputLayer.SelectionCount == 0)
                return null;

            Envelope env = null;

            await QueuedTask.Run(() =>
            {
                SpatialReference mySpatRef = FeatureClassQuery.GetSpatialReferenceProp(InputLayer.GetFeatureClass()).Result;

                using (RowCursor rowCursor = InputLayer.GetSelection().Search(null, false))
                {
                    List<double> easting = new List<double>();
                    List<double> northing = new List<double>();

                    while (rowCursor.MoveNext())
                    {
                        using (Feature feature = (Feature)rowCursor.Current)
                        {
                            easting.Add(feature.GetShape().Extent.XMin);
                            easting.Add(feature.GetShape().Extent.XMax);

                            northing.Add(feature.GetShape().Extent.YMin);
                            northing.Add(feature.GetShape().Extent.YMax);
                        }
                    }

                    double dblXMin = easting.Min();
                    double dblYMin = northing.Min();
                    double dblXMax = easting.Max();
                    double dblYMax = northing.Max();

                    MapPoint minPt = MapPointBuilderEx.CreateMapPoint(dblXMin, dblYMin, mySpatRef);
                    MapPoint maxPt = MapPointBuilderEx.CreateMapPoint(dblXMax, dblYMax, mySpatRef);

                    env = EnvelopeBuilderEx.CreateEnvelope(minPt, maxPt);
                }
            });

            return env;
        }

        /// <summary>
        /// Get Spatial Reference from feature layer
        /// </summary>
        /// <param name="layer"></param>
        /// <returns></returns>
        public static async Task<SpatialReference> GetSpatialReferenceProp(FeatureLayer layer)
        {
            if (layer == null)
                return null;

            SpatialReference thisSpatRef = null;

            await QueuedTask.Run(() =>
            {
                FeatureClass featClass = layer.GetFeatureClass();
                FeatureClassDefinition _definition = featClass.GetDefinition();
                thisSpatRef = _definition.GetSpatialReference();

            });

            return thisSpatRef;
        }

        /// <summary>
        /// Get Spatial Reference from feature class
        /// </summary>
        /// <param name="featClass"></param>
        /// <returns></returns>
        public static async Task<SpatialReference> GetSpatialReferenceProp(FeatureClass featClass)
        {
            SpatialReference thisSpatRef = null;

            await QueuedTask.Run(() =>
            {

                FeatureClassDefinition _definition = featClass.GetDefinition();
                thisSpatRef = _definition.GetSpatialReference();

            });

            return thisSpatRef;
        }

        /// <summary>
        /// Get spatial reference from Active Map
        /// </summary>
        /// <returns></returns>
        public static async Task<SpatialReference> GetSpatialReferenceProp()
        {
            SpatialReference thisSpatRef = null;

            await QueuedTask.Run(() =>
            {
                thisSpatRef = MapView.Active.Map.GetDefinition().SpatialReference;

            });

            return thisSpatRef;
        }

        public static async Task<SpatialReference> SetSpatialReferenceDefault()
        {
            SpatialReference thisSpatRef = null;

            await QueuedTask.Run(() =>
            {
                thisSpatRef = SpatialReferences.WebMercator;

            });

            return thisSpatRef;
        }

        public static async Task<Envelope> ProjectFromWGS84(Envelope extent)
        {
            Geometry projPoly = null;

            await QueuedTask.Run(() =>
            {
                List<MapPoint> pts = new List<MapPoint>();
                pts.Add(MapPointBuilderEx.CreateMapPoint(extent.XMin, extent.YMin, extent.SpatialReference));
                pts.Add(MapPointBuilderEx.CreateMapPoint(extent.XMin, extent.YMax, extent.SpatialReference));
                pts.Add(MapPointBuilderEx.CreateMapPoint(extent.XMax, extent.YMax, extent.SpatialReference));
                pts.Add(MapPointBuilderEx.CreateMapPoint(extent.XMax, extent.YMin, extent.SpatialReference));

                Polygon polygon = PolygonBuilderEx.CreatePolygon(pts);

                // ensure it is simple
                bool isSimple = GeometryEngine.Instance.IsSimpleAsFeature(polygon);

                //Project to Active Map
                SpatialReference outputReference = MapView.Active.Map.GetDefinition().SpatialReference;

                if (!outputReference.IsGeographic) //project to same as active map
                {
                    projPoly = GeometryEngine.Instance.Project(polygon, outputReference);

                }
                else //create project to webmercator
                {
                    projPoly = GeometryEngine.Instance.Project(polygon, SpatialReferences.WebMercator);
                }
            });



            return projPoly.Extent;
        }

        public static async Task<Polygon> ProjectPolygonToWGS84(Polygon poly)
        {
            Geometry projPoly = null;

            await QueuedTask.Run(() =>
            {
                // ensure it is simple
                bool isSimple = GeometryEngine.Instance.IsSimpleAsFeature(poly);

                projPoly = GeometryEngine.Instance.Project(poly, SpatialReferences.WGS84);

            });

            return projPoly as Polygon;
        }

        public static async Task<Polygon> ProjectPolygonGeneric(Polygon poly, SpatialReference spatRef)
        {
            Geometry projPoly = null;

            await QueuedTask.Run(() =>
            {
                // ensure it is simple
                bool isSimple = GeometryEngine.Instance.IsSimpleAsFeature(poly);

                projPoly = GeometryEngine.Instance.Project(poly, spatRef);

            });

            return projPoly as Polygon;
        }

        public static async Task<Polyline> ProjectPolylineFromWGS84(Polyline line)
        {
            Geometry projLine = null;

            await QueuedTask.Run(() =>
            {
                // ensure it is simple
                bool isSimple = GeometryEngine.Instance.IsSimpleAsFeature(line);

                //Project to Active Map
                SpatialReference outputReference = MapView.Active.Map.GetDefinition().SpatialReference;

                if (!outputReference.IsGeographic) //project to same as active map
                {
                    projLine = GeometryEngine.Instance.Project(line, outputReference);

                }
                else //create project to webmercator
                {
                    projLine = GeometryEngine.Instance.Project(line, SpatialReferences.WebMercator);
                }

            });

            return projLine as Polyline;
        }
    }
}
