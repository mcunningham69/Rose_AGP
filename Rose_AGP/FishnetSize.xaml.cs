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
    /// Interaction logic for FishnetSize.xaml
    /// </summary>
    public partial class FishnetSize : Window
    {
        public string Cellsize { get; set; }
        const string strTitle = "Fiosrachadh";

        public FishnetSize()
        {
            InitializeComponent();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void BtnField_Click(object sender, RoutedEventArgs e)
        {
            Cellsize = txtCell.Text;

            if (Cellsize == "")
            {
                var exitCell = MessageBox.Show("Nothing has been entered. Do you want to re-enter a value?", strTitle,
                    MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (exitCell == MessageBoxResult.Yes)
                    return;
            }
            else
            {
                //Check value is valid number
                RoseDiagramParameters mTemp = new RoseDiagramParameters();
                string message = mTemp.SubcellErrorChecking(Cellsize);

                if (message != "")
                {
                    MessageBox.Show(message, strTitle, MessageBoxButton.OK,MessageBoxImage.Information);
                    return;
                }
            }

            this.Close();
        }
    }
}
