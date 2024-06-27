using ArcGIS.Desktop.Mapping;
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
using ArcGIS.Core.Geometry;

namespace Rose_AGP
{
    /// <summary>
    /// Interaction logic for MovingStatistics.xaml
    /// </summary>
    public partial class MovingStatistics : Window
    {
        const string strTitle = "Fiosrachadh";
 
        public FeatureLayer InputLayer { get; set; }
        public RasterLineamentAnalysis _analysis { get; set; }
        private int _XBlocks { get; set; }
        private int _YBlocks { get; set; }
        private int TotalBlocks { get; set; }
        public SpatialReference thisSpatRef { get; set; }
        private string WindowTitle { get; set; }
        public string inputDatabasePath { get; set; }
        public string outputDatabasePath { get; set; }
        public MovingStatistics()
        {
            InitializeComponent();
        }

        private void BtnAccept_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnYMinus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnYAdd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnXMinus_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnXAdd_Click(object sender, RoutedEventArgs e)
        {

        }

        private void RadGrid_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RadPoly_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void RadPoint_Checked(object sender, RoutedEventArgs e)
        {

        }
    }
}
