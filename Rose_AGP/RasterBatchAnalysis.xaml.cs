using ArcGIS.Desktop.Mapping;
using ArcGIS.Core.Geometry;
using Rose_AGP.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Rose_AGP
{
    /// <summary>
    /// Interaction logic for RasterBatchAnalysis.xaml
    /// </summary>
    public partial class RasterBatchAnalysis : Window
    {
        #region Properties and Variables
        RoseDiagramParameters _rose = new RoseDiagramParameters();

        const string strTitle = "Fiosrachadh";
        const string default_pixel_type = "8_BIT_UNSIGNED";

        private bool GroupLayers { get; set; }
        public FeatureLayer InputLayer { get; set; }
        private string progressMessage { get; set; }
        public SpatialReference thisSpatRef { get; set; }
        public Enum.RoseGeom _RoseGeom { get; set; }
        public Enum.RoseType _RoseType { get; set; }
        private string WindowTitle { get; set; }
        public string inputDatabase { get; set; }
        public string outputDatabase { get; set; }
        public RasterLineamentAnalysis _analysis { get; set; }
        #endregion
        public RasterBatchAnalysis()
        {
            InitializeComponent();
        }

        private void TxtName_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void ChkGroup_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void TxtFrom_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void TxtCell_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnRefresh_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void TxtInterval_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void TxtTo_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
