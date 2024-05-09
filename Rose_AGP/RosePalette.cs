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


namespace Rose_AGP
{
    internal class RosePalette_button1 : Button
    {
        protected async override void OnClick()
        {
            Module1 module = new Module1();
            await module.BatchRun(Enum.RoseType.Length, Enum.RoseGeom.Line, true);
            // module = null;
        }
        protected override void OnUpdate()
        {
            this.Enabled = true;
        }
    }

    internal class RosePalette_button2 : Button
    {
        protected async override void OnClick()
        {
            Module1 module = new Module1();
            await module.BatchRun(Enum.RoseType.Frequency, Enum.RoseGeom.Line, true);
            
            // module = null;
        }
        protected override void OnUpdate()
        {
            this.Enabled = true;
        }
    }

    internal class RosePalette_button3 : Button
    {
        protected async override void OnClick()
        {
            Module1 module = new Module1();
            await module.BatchRun(Enum.RoseType.Frequency, Enum.RoseGeom.Point, true);
            module = null;
        }
        protected override void OnUpdate()
        {
            this.Enabled = true;
        }
    }

}
