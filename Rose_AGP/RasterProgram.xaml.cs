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
