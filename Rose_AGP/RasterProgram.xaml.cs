using ArcGIS.Desktop.Mapping;
using Rose_AGP.Enum;
using ArcGIS.Core.Geometry;
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
    /// Interaction logic for RasterProgram.xaml
    /// </summary>
    public partial class RasterProgram : Window
    {
        const string strTitle = "Fiosrachadh";
        public FeatureLayer InputLayer { get; set; }
        public RasterLineamentAnalysis _analysis { get; set; }
        public SpatialReference thisSpatRef { get; set; }
        public string inputDatabase { get; set; }
        public string outputDatabase { get; set; }
        private string WindowTitle { get; set; }

        public RasterProgram()
        {
            InitializeComponent();
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TxtTo_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }

        private void TxtFrom_SelectionChanged(object sender, RoutedEventArgs e)
        {

        }
    }
}
