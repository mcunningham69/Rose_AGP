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
        public RasterFactoryPreview RasterPreview { get; set; }
        public SpatialReference thisSpatRef { get; set; }
        public bool bMovingStatistics { get; set; }
        public bool bRasterOnly { get; set; }
        public bool bPolygon { get; set; }
        public RasterProgram _rasterProgram { get; set; }
        public string progressMessage { get; set; }


        
    }
}
