using ArcGIS.Core.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rose_AGP
{
    public static class CalcOrientation
    {
        #region Azimuth
        public static double ReturnOrient(ArcGIS.Core.Geometry.Polyline line)
        {
            double dblDirection = 0.0;

            var PointCol = ((Multipart)line).Points;

            double dblStartX = PointCol[0].X;
            double dblStartY = PointCol[0].Y;
            int pointCount = PointCol.Count;

            double dblEndX = PointCol[pointCount - 1].X;
            double dblEndY = PointCol[pointCount - 1].Y;

            double dblDx = dblEndX - dblStartX;
            double dblDy = dblEndY - dblStartY;

            if (dblDy == 0.0)
                return 90;

            double dblOrient = dblDx / dblDy;

            //get the inverse tangent
            double dblInverse = Math.Atan(dblOrient);

            //as Atan returns radians, convert to degress
            double dblDeg = dblInverse * (180 / Math.PI);

            if (dblDeg < 0)
                dblDirection = dblDeg + 180;
            else
                dblDirection = dblDeg;

            return dblDirection;

        }

        public static double ReturnLength(ArcGIS.Core.Geometry.LineSegment line)
        {
            double dblLength = 0.0;

            dblLength = line.Length;

            return dblLength;
        }

        public static double ReturnOrient(ArcGIS.Core.Geometry.LineSegment line)
        {
            double dblDirection = 0.0;

            var PointCol = line.StartPoint;

            double dblStartX = PointCol.X;
            double dblStartY = PointCol.Y;

            PointCol = line.EndPoint;

            double dblEndX = PointCol.X;
            double dblEndY = PointCol.Y;

            double dblDx = dblEndX - dblStartX;
            double dblDy = dblEndY - dblStartY;

            if (dblDy == 0.0)
                return 90;

            double dblOrient = dblDx / dblDy;

            //get the inverse tangent
            double dblInverse = Math.Atan(dblOrient);

            //as Atan returns radians, convert to degress
            double dblDeg = dblInverse * (180 / Math.PI);

            if (dblDeg < 0)
                dblDirection = dblDeg + 180;
            else
                dblDirection = dblDeg;

            return dblDirection;

        }
        #endregion
    }
}

