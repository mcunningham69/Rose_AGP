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

namespace Rose_AGP.Raster
{
    internal class RasterBatch : Button
    {
        protected async override void OnClick()
        {
            Module1 module = new Module1();
            await module.BatchRun(Enum.RoseType.Other, Enum.RoseGeom.Other, false);
            module = null;
        }

        protected override void OnUpdate()
        {
            this.Enabled = true;
        }
    }
}
