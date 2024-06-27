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
using Microsoft.VisualBasic;

namespace Rose_AGP
{
    /// <summary>
    /// Interaction logic for RasterProgram.xaml
    /// </summary>
    public partial class RasterProgram : Window
    {
        #region
        const string strTitle = "Fiosrachadh";
        public FeatureLayer InputLayer { get; set; }
        public RasterLineamentAnalysis _analysis { get; set; }
        public SpatialReference thisSpatRef { get; set; }
        public string inputDatabase { get; set; }
        public string outputDatabase { get; set; }
        private string WindowTitle { get; set; }

        #endregion

    

        public RasterProgram()
        {
            InitializeComponent();
            WindowTitle = this.Title;
        }

        private void BtnCreate_Click(object sender, RoutedEventArgs e)
        {
            this.IsEnabled = false;

            try
            {
                RasterAnalysis rasterAnalysis = new RasterAnalysis()
                {
                    SelectedFeatures = (bool)chkSelected.IsChecked,
                    bRasterOnly = (bool)radGrid.IsChecked,
                    bPolygon = (bool)radPoly.IsChecked,
                    bMovingStatistics = false,
                    InputLayer = InputLayer,
                    RasterName = txtRaster.Text,
                    _rasterProgram = this,
                    _LineamentAnalysis = _analysis,
                    thisSpatRef = thisSpatRef,
                    inputDatabasePath = inputDatabase,
                    outputDatabasePath = outputDatabase
                };

                int nInterval = rasterAnalysis.ErrorCheckingValues(txtInterval.Text);

                if (nInterval == 0)
                {
                    MessageBox.Show("Error with 'interval' data type", strTitle, MessageBoxButton.OK, MessageBoxImage.Hand);
                    return;
                }
                else
                    rasterAnalysis.Interval = nInterval; //TODO further checks when setting property

                int nCellsize = rasterAnalysis.ErrorCheckingValues(txtCell.Text);

                if (nCellsize == 0)
                {
                    MessageBox.Show("Error with 'cellsize' data type", strTitle, MessageBoxButton.OK, MessageBoxImage.Hand);
                    return;
                }
                else
                    rasterAnalysis.Cellsize = nCellsize; //TODO further checks when setting property

                int rangeFrom = 0;
                int rangeTo = 0;

                if (_analysis != RasterLineamentAnalysis.RelativeEntropy)
                {
                    rangeFrom = rasterAnalysis.ErrorCheckingValues(txtFrom.Text);

                    if (rangeFrom == -1)
                    {
                        MessageBox.Show("Error with 'Range From' data type", strTitle, MessageBoxButton.OK, MessageBoxImage.Hand);
                        return;
                    }

                    rangeTo = rasterAnalysis.ErrorCheckingValues(txtTo.Text);

                    if (rangeTo == -1)
                    {
                        MessageBox.Show("Error with 'Range To' data type", strTitle, MessageBoxButton.OK, MessageBoxImage.Hand);
                        return;
                    }
                }
                rasterAnalysis.RangeFrom = rangeFrom;
                rasterAnalysis.RangeTo = rangeTo;

                string message = "";
                //string message = await rasterAnalysis.CreateRasterGrid(true);

                if (message != "")
                {
                    MessageBox.Show(message, strTitle, MessageBoxButton.OK, MessageBoxImage.Hand);
                    return;
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, strTitle, MessageBoxButton.OK, MessageBoxImage.Hand);
            }
            finally
            {
                this.Title = WindowTitle;
                this.IsEnabled = true;
            }

        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void TxtTo_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateIntervalFromRange();

        }

        private void TxtFrom_SelectionChanged(object sender, RoutedEventArgs e)
        {
            UpdateIntervalFromRange();

        }

        private void UpdateIntervalFromRange()
        {
            if (txtTo != null && txtFrom != null)
            {
                if (txtFrom.Text != "" && txtTo.Text != "")
                {
                    if (Information.IsNumeric(txtFrom.Text))
                    {
                        if (Information.IsNumeric(txtTo.Text))
                        {
                            int from = Convert.ToInt16(txtFrom.Text);
                            int to = Convert.ToInt16(txtTo.Text);

                            if (from >= 175)
                            {
                                txtFrom.Text = 0.ToString();
                                MessageBox.Show("Value must be less than 175", strTitle, MessageBoxButton.OK, MessageBoxImage.Hand);
                                return;
                            }

                            if (to > 180)
                            {
                                txtFrom.Text = 10.ToString();
                                MessageBox.Show("Value must be less than 180", strTitle, MessageBoxButton.OK, MessageBoxImage.Hand);
                                return;
                            }

                            if (to > from)
                                txtInterval.Text = (to - from).ToString();
                        }
                    }
                }
            }
        }

    }
}
