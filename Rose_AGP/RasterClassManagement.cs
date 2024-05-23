using ArcGIS.Core.Data.Raster;
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Framework.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rose_AGP
{
    public static class RasterClassManagement
    {
        public static async Task<bool> CheckIfRasterExists(string rasterPath, string rasterName)
        {
            bool bCheck = true;

            RasterDataset rasterTemplate = null;

            // string geodb = CoreModule.CurrentProject.DefaultGeodatabasePath;

            await QueuedTask.Run(() =>
            {
                try
                {
                    // Create a FileGeodatabaseConnectionPath using the path to the gdb. Note: This can be a path to a .sde file.
                    FileGeodatabaseConnectionPath geodatabaseConnectionPath = new FileGeodatabaseConnectionPath(new Uri(rasterPath));
                    // Create a new Geodatabase object using the FileGeodatabaseConnectionPath.

                    Geodatabase geodatabase = new Geodatabase(geodatabaseConnectionPath);

                    // Open the raster dataset.
                    rasterTemplate = geodatabase.OpenDataset<RasterDataset>(rasterName);
                }
                catch (Exception ex)
                {
                    bCheck = false;
                }
            });

            if (rasterTemplate != null)
                return true;


            return bCheck;
        }

        public static async Task<bool> CheckIfTempRasterExists(string rasterPath, string rasterName)
        {
            bool bCheck = true;

            RasterDataset rasterTemplate = null;

            await QueuedTask.Run(() =>
            {
                try
                {
                    FileSystemConnectionPath connectionPath = new FileSystemConnectionPath(new System.Uri(rasterPath), FileSystemDatastoreType.Raster);
                    // Create a new FileSystemDatastore using the FileSystemConnectionPath.
                    FileSystemDatastore dataStore = new FileSystemDatastore(connectionPath);
                    // Open the raster dataset.
                    rasterTemplate = dataStore.OpenDataset<RasterDataset>(rasterName);
                }
                catch (Exception ex)
                {
                    bCheck = false;
                }
            });

            if (rasterTemplate != null)
                return true;


            return bCheck;
        }

    }

    public class PixelType
    {
        public string OneBitUS { get; set; }
        public string TwoBitUS { get; set; }
        public string FourBitUS { get; set; }
        public string EightBitUS { get; set; }
        public string EightBitS { get; set; }
        public string SixteenBitUS { get; set; }
        public string SixteenBitS { get; set; }
        public string ThirtytwoBitUS { get; set; }
        public string ThirtytwoBitS { get; set; }
        public string ThirtytwoBitF { get; set; }
        public string SixtyfourBitD { get; set; }

        public PixelType()
        {
            OneBitUS = "1_BIT_UNSIGNED";
            TwoBitUS = "2_BIT_UNSIGNED";
            FourBitUS = "4_BIT_UNSIGNED";
            EightBitUS = "8_BIT_UNSIGNED";
            EightBitS = "8_BIT_SIGNED";
            SixteenBitUS = "16_BIT_UNSIGNED";
            SixteenBitS = "16_BIT_SIGNED";
            ThirtytwoBitUS = "32_BIT_UNSIGNED";
            ThirtytwoBitS = "32_BIT_SIGNED";
            ThirtytwoBitF = "32_BIT_FLOAT";
            SixtyfourBitD = "64_BIT";
        }
    }

}
