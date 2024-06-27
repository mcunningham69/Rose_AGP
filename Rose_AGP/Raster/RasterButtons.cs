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
using Rose_AGP.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Rose_AGP.Raster
{
    internal class RasterButtons_button1 : Button
    {
        protected async override void OnClick()
        {
            Module1 module = new Module1();
            await module.RasterModule(RasterLineamentAnalysis.RelativeEntropy);
            module = null;
        }

        protected override void OnUpdate()
        {
            bool enableState = true;
            bool criteria = true;

            if (enableState)
                this.Enabled = true;
            else
                this.Enabled = false;

            if (criteria)
                this.DisabledTooltip = "Select a polyline layer";
        }
    }

    internal class RasterButtons_button2 : Button
    {
        protected async override void OnClick()
        {
            Module1 module = new Module1();
            module = new Module1();
            await module.RasterModule(RasterLineamentAnalysis.GroupDominanceLength);
            module = null;
        }

        protected override void OnUpdate()
        {
            bool enableState = true;
            bool criteria = true;

            if (enableState)
                this.Enabled = true;
            else
                this.Enabled = false;

            if (criteria)
                this.DisabledTooltip = "Select a polyline layer";
        }
    }

    internal class RasterButtons_button3 : Button
    {
        protected async override void OnClick()
        {
            Module1 module = new Module1();
            await module.RasterModule(RasterLineamentAnalysis.GroupDominanceFrequency);
            module = null;
        }

        protected override void OnUpdate()
        {
            bool enableState = true;
            bool criteria = true;

            if (enableState)
                this.Enabled = true;
            else
                this.Enabled = false;

            if (criteria)
                this.DisabledTooltip = "Select a polyline layer";
        }
    }

    internal class RasterButtons_button4 : Button
    {
        protected async override void OnClick()
        {
            Module1 module = new Module1();
            await module.RasterModule(RasterLineamentAnalysis.GroupMeansLength);
            module = null;
        }

        protected override void OnUpdate()
        {
            bool enableState = true;
            bool criteria = true;

            if (enableState)
                this.Enabled = true;
            else
                this.Enabled = false;

            if (criteria)
                this.DisabledTooltip = "Select a polyline layer";
        }
    }

    internal class RasterButtons_button5 : Button
    {
        protected async override void OnClick()
        {
            Module1 module = new Module1();
            await module.RasterModule(RasterLineamentAnalysis.GroupMeansFrequency);
            module = null;
        }

        protected override void OnUpdate()
        {
            bool enableState = true;
            bool criteria = true;

            if (enableState)
                this.Enabled = true;
            else
                this.Enabled = false;

            if (criteria)
                this.DisabledTooltip = "Select a polyline layer";
        }
    }

    internal class RasterButtons_button6 : Button
    {
        protected async override void OnClick()
        {
            Module1 module = new Module1();
            await module.RasterModule(RasterLineamentAnalysis.DensityLength);
            module = null;
        }

        protected override void OnUpdate()
        {
            bool enableState = true;
            bool criteria = true;

            if (enableState)
                this.Enabled = true;
            else
                this.Enabled = false;

            if (criteria)
                this.DisabledTooltip = "Select a polyline layer";
        }
    }

    internal class RasterButtons_button7 : Button
    {
        protected async override void OnClick()
        {
            Module1 module = new Module1();
            await module.RasterModule(RasterLineamentAnalysis.DensityFrequency);
            module = null;
        }

        protected override void OnUpdate()
        {
            bool enableState = true;
            bool criteria = true;

            if (enableState)
                this.Enabled = true;
            else
                this.Enabled = false;

            if (criteria)
                this.DisabledTooltip = "Select a polyline layer";
        }
    }

}
