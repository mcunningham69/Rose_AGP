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
    internal class RasterMovingStatistics_button1 : Button
    {
        protected async override void OnClick()
        {
            Module1 module = new Module1();
            await module.MovingStatisticsModule(RasterLineamentAnalysis.RelativeEntropy);
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

    internal class RasterMovingStatistics_button2 : Button
    {
        protected async override void OnClick()
        {
            Module1 module = new Module1();
            await module.MovingStatisticsModule(RasterLineamentAnalysis.DensityFrequency);
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

    internal class RasterMovingStatistics_button3 : Button
    {
        protected async override void OnClick()
        {
            Module1 module = new Module1();
            await module.MovingStatisticsModule(RasterLineamentAnalysis.DensityLength);
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

    internal class RasterMovingStatistics_button4 : Button
    {
        protected async override void OnClick()
        {
            Module1 module = new Module1();
            await module.MovingStatisticsModule(RasterLineamentAnalysis.GroupDominanceFrequency);
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

    internal class RasterMovingStatistics_button5 : Button
    {
        protected async override void OnClick()
        {
            Module1 module = new Module1();
            await module.MovingStatisticsModule(RasterLineamentAnalysis.GroupDominanceLength);
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

    internal class RasterMovingStatistics_button6 : Button
    {
        protected async override void OnClick()
        {
            Module1 module = new Module1();
            await module.MovingStatisticsModule(RasterLineamentAnalysis.GroupMeansFrequency);
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

    internal class RasterMovingStatistics_button7 : Button
    {
        protected async override void OnClick()
        {
            Module1 module = new Module1();
            await module.MovingStatisticsModule(RasterLineamentAnalysis.GroupMeansLength);
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
