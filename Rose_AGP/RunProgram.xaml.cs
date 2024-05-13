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
using ArcGIS.Core.Data;
using ArcGIS.Desktop.Catalog;
using ArcGIS.Desktop.Core;
using ArcGIS.Desktop.Framework.Threading.Tasks;

namespace Rose_AGP
{
    /// <summary>
    /// Interaction logic for RunProgram.xaml
    /// </summary>
    public partial class RunProgram : Window
    {

        #region Properties

        public RoseType _RoseType { get; set; }
        public RoseGeom _RoseGeom { get; set; }
        public FeatureLayer InputLayer { get; set; }
        public string WindowTitle { get; set; }
        private string DirectionField { get; set; }
        private Envelope CustomEnvelope { get; set; }
        private bool bSelection { get; set; }
        private string OutputName { get; set; }
        public string inputDatabasePath { get; set; }
        public string outputDatabasePath { get; set; }
        public SpatialReference thisSpatRef { get; set; }
        #endregion

        const string strTitle = "Fiosrachadh";
        RoseDiagramParameters _rose = new RoseDiagramParameters();

        private RoseFactoryPreview _Rosefactory { get; set; }
        private FlapParameters _parameters { get; set; }


        public RunProgram()
        {
            InitializeComponent();
        }

        private void CboDirection_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DirectionField = cboDirection.SelectedItem.ToString();
        }

        private void CboDirection_SourceUpdated(object sender, DataTransferEventArgs e)
        {

        }

        private void BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private async void btnRunRose(object sender, RoutedEventArgs e)
        {
            await Progressor_NonCancelable();

            this.Title = "Creating Regional Rose Plot";

            if ((bool)chkSelected.IsChecked)
            {
                bool bCount = false ? true : InputLayer.SelectionCount > 0;

                if (bCount)
                {
                    bSelection = true;
                    CustomEnvelope = await FeatureClassQuery.ExtentFromSelectectedInput(InputLayer);

                }
                else
                {
                    bSelection = false;
                    CustomEnvelope = await FeatureClassQuery.ReturnExtent(InputLayer); //the default extent
                }
            }

            try
            {
                this.IsEnabled = false;

                string strMessage = "";
                if (!(bool)chkRegional.IsChecked)
                {
                    strMessage = await RunSubroseProgram();
                    return;
                }
                else
                {
                    strMessage = await RunRegionalRoseProgram();
                }

                if (strMessage != "")
                    ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(strMessage, strTitle, MessageBoxButton.OK, MessageBoxImage.Information);

            }
            catch (Exception ex)
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show(ex.Message, strTitle, MessageBoxButton.OK, MessageBoxImage.Information);

            }
            finally
            {
                this.Title = WindowTitle;
                this.IsEnabled = true;
            }
        }

        public async void FieldnamesForPointLayer()
        {
            IReadOnlyList<Field> _fields = await FeatureClassQuery.GetFieldNames(InputLayer);

            cboDirection.Text = "Select Direction";

            foreach (Field field in _fields)
            {
                if (field.Name != "OBJECTID" || field.Name != "Shape")
                {
                    if (field.FieldType == FieldType.Double || field.FieldType == FieldType.Integer || field.FieldType == FieldType.Single || field.FieldType == FieldType.SmallInteger)
                    {
                        // cboDip.Items.Add(field.Name.ToString());
                        cboDirection.Items.Add(field.Name.ToString());
                    }
                }
            }

        }

        public async Task Progressor_NonCancelable()
        {
            ArcGIS.Desktop.Framework.Threading.Tasks.ProgressorSource ps = new ArcGIS.Desktop.Framework.Threading.Tasks.ProgressorSource("It's coming...", false);

            int numSecondsDelay = 3;
            //If you run this in the DEBUGGER you will NOT see the dialog
            await ArcGIS.Desktop.Framework.Threading.Tasks.QueuedTask.Run(() => Task.Delay(numSecondsDelay * 1000).Wait(), ps.Progressor);
        }

        private async Task<SaveItemDialog> SaveDialog()
        {
            SaveItemDialog saveDialog = new SaveItemDialog()
            {
                Title = "Save Rose Plot As...",
                Filter = ItemFilters.FeatureClasses_All,
                OverwritePrompt = true,
                InitialLocation = CoreModule.CurrentProject.DefaultGeodatabasePath

            };

            return saveDialog;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.Title = WindowTitle;

            if (_RoseGeom == RoseGeom.Point)
            {
                FieldnamesForPointLayer();

            }
        }


        private async Task<string> RunSubroseProgram()
        {
            if (_RoseGeom == RoseGeom.Point)
            {
                SetupForPointProcessing();
            }

            string databasePath = inputDatabasePath;
            if (outputDatabasePath != "")
                databasePath = outputDatabasePath;
            else if (inputDatabasePath == "")
                databasePath = CoreModule.CurrentProject.DefaultGeodatabasePath;

            string message = "";

            #region checking input values for interval and cellsize
            int nInterval = 0;
            message = _rose.IntervalErrorChecking(txtInterval.Text);

            if (message == "")
            {
                nInterval = Convert.ToInt16(txtInterval.Text);

            }
            else
                return message;

            int Cellsize = 0;
            message = _rose.SubcellErrorChecking(txtCell.Text);

            if (message == "")
            {
                Cellsize = Convert.ToInt32(txtCell.Text);

            }
            else
                return message;
            #endregion

            if (_Rosefactory == null)
                _Rosefactory = new RoseFactoryPreview(Enum.RoseLineamentAnalysis.RoseCells);
            else
                _Rosefactory.SetAnalysisDataset(Enum.RoseLineamentAnalysis.RoseCells);

            this.Title = "Calculating Rose Plots";

            _parameters = null;

            _parameters = await _Rosefactory.PrepareInputForProcessing(InputLayer, CustomEnvelope, Cellsize, nInterval, bSelection, _RoseGeom, false, DirectionField);

            if (_RoseType == RoseType.Length)
                _parameters.RoseType = "LENGTH";
            else
                _parameters.RoseType = "FREQUENCY";

            _parameters.Interval = nInterval;
            _parameters.SelectedFeatures = bSelection;
            _parameters.flapParameters[0].CellID = 1;
            _Rosefactory.CalculateVectorValues(_parameters, (bool)chkStatistics.IsChecked, _RoseType, (bool)chkDirection.IsChecked);

            this.Title = "Saving Rose Plots";

            message = await CheckFilename();


            bool bExist = await FeatureClassQuery.FeatureclassExists(databasePath, OutputName + "_rose");

            if (bExist)
            {
                int cellID = await UpdateCellID();
                if (cellID > 0)
                    _parameters.flapParameters[0].CellID = cellID + 1;
            }

            FeatureClass roseFC = await _Rosefactory.CreateFeatureClass("_rose", OutputName, bExist, databasePath,
                InputLayer, (bool)chkStatistics.IsChecked, _RoseGeom, thisSpatRef);

            message = await _Rosefactory.SaveAsVectorFeatures(roseFC, thisSpatRef, _parameters, false);

            if ((bool)chkCells.IsChecked)
            {
                this.Title = "Saving Fishnet Extent";

                message = await CreateFishnet(bExist, thisSpatRef);
            }

            if ((bool)chkCells.IsChecked)
                await AddLayerToMap(OutputName + "_fish");

            await AddLayerToMap(OutputName + "_rose");

            return "Rose Completed";

        }

        private async Task<string> RunRegionalRoseProgram()
        {
            string databasePath = inputDatabasePath;
            if (outputDatabasePath != "")
                databasePath = outputDatabasePath;
            else if (inputDatabasePath == "")
                databasePath = CoreModule.CurrentProject.DefaultGeodatabasePath;

            if (_RoseGeom == RoseGeom.Point)
            {
                SetupForPointProcessing();
            }

            string message = "";

            int nInterval = 0;
            message = _rose.IntervalErrorChecking(txtInterval.Text);

            if (message == "")
            {
                nInterval = Convert.ToInt16(txtInterval.Text);
            }
            else
                return message;



            if (_Rosefactory == null)
                _Rosefactory = new RoseFactoryPreview(Enum.RoseLineamentAnalysis.RoseRegional);
            else
                _Rosefactory.SetAnalysisDataset(Enum.RoseLineamentAnalysis.RoseRegional);

            this.Title = "Calculating Rose Plot";

            _parameters = null;
            _parameters = await _Rosefactory.PrepareInputForProcessing(InputLayer, CustomEnvelope, 0, nInterval, bSelection, _RoseGeom, true, DirectionField);

            if (_RoseType == RoseType.Length)
                _parameters.RoseType = "LENGTH";
            else
                _parameters.RoseType = "FREQUENCY";

            _parameters.SelectedFeatures = bSelection;
            _parameters.flapParameters[0].CellID = 1;
            _Rosefactory.CalculateVectorValues(_parameters, (bool)chkStatistics.IsChecked, _RoseType, (bool)chkDirection.IsChecked);

            this.Title = "Saving Rose Plot";

            message = await CheckFilename();

            bool bExist = await FeatureClassQuery.FeatureclassExists(databasePath, OutputName + "_rose");
            if (bExist)
            {
                int cellID = await UpdateCellID();
                if (cellID > 0)
                    _parameters.flapParameters[0].CellID = cellID + 1;
            }

            FeatureClass roseFC = await _Rosefactory.CreateFeatureClass("_rose", OutputName, bExist, databasePath, InputLayer, (bool)chkStatistics.IsChecked, _RoseGeom, thisSpatRef);

            message = await _Rosefactory.SaveAsVectorFeatures(roseFC, thisSpatRef, _parameters, false);

            if ((bool)chkCells.IsChecked)
            {
                this.Title = "Saving Fishnet Extent";

                message = await CreateFishnet(bExist, thisSpatRef);
            }

            if ((bool)chkCells.IsChecked)
                await AddLayerToMap(OutputName + "_fish");

            await AddLayerToMap(OutputName + "_rose");

            return "Rose Completed";
        }

        private async Task<int> UpdateCellID()
        {
            string databasePath = inputDatabasePath;
            if (outputDatabasePath != "")
                databasePath = outputDatabasePath;
            else if (inputDatabasePath == "")
                databasePath = CoreModule.CurrentProject.DefaultGeodatabasePath;

            //get FeatureClass to check if CellID field exists
            FeatureClass checkFC = await FeatureClassManagement.SetOutputFeatureclass(databasePath, OutputName + "_rose");
            bool bField = false;

            await QueuedTask.Run(() =>
            {
                FeatureClassDefinition _definition = checkFC.GetDefinition();
                //get field index of TD for adding value to attribute table
                int nField = _definition.FindField("CellID");

                if (nField == -1)
                {
                    bField = false;
                }
                else
                    bField = true;
            });

            if (bField)
            {
                int featCount = 0;
                featCount = await FeatureClassQuery.ReturnNoFeatures(checkFC); //see if any features
                if (featCount > 0)
                {
                    var _nRoses = await FeatureClassQuery.ReturnCellIdFromAttributes(checkFC);

                    return _nRoses;
                }
            }


            return 0;

        }

        private async Task<string> CreateFishnet(bool bExist, SpatialReference thisSpatRef)
        {
            string databasePath = inputDatabasePath;
            if (outputDatabasePath != "")
                databasePath = outputDatabasePath;
            else if (inputDatabasePath == "")
                databasePath = CoreModule.CurrentProject.DefaultGeodatabasePath;

            _Rosefactory.SetAnalysisDataset(Enum.RoseLineamentAnalysis.Fishnet);

            _Rosefactory.CalculateVectorValues(_parameters, (bool)chkStatistics.IsChecked, _RoseType, false);

            FeatureClass fishFC = await _Rosefactory.CreateFeatureClass("_fish", OutputName, bExist, databasePath,
                InputLayer, (bool)chkStatistics.IsChecked, _RoseGeom, thisSpatRef);

            return await _Rosefactory.SaveAsVectorFeatures(fishFC, thisSpatRef, _parameters, false);
        }

        private async Task<string> CheckFilename()
        {
            if (txtVector.Text == "")
            {
                SaveItemDialog saveDialog = await SaveDialog();

                if (saveDialog.ShowDialog() == true)
                {
                    if (saveDialog.FilePath == "")
                        return "Empty path";

                    string filePath = saveDialog.FilePath;
                    int index = filePath.IndexOf(".gdb");
                    inputDatabasePath = filePath.Substring(0, index + 4);
                    int start = inputDatabasePath.Length + 1;
                    int length = filePath.Length;
                    int nameLength = length - start;

                    OutputName = filePath.Substring(start, nameLength);

                    await CheckForSpecialCharacters(OutputName);

                }
                else
                {
                    OutputName = "";
                    return "-99";
                }
            }
            else
            {
                await CheckForSpecialCharacters(txtVector.Text);
            }

            txtVector.Text = OutputName;

            return "";
        }

        private async Task<string> CheckForSpecialCharacters(string name)
        {

            string validName = CheckStrings.Checker(name, strTitle);

            if (validName == "")
            {
                return "";
            }
            else
            {
                validName = CheckStrings.CheckFirstLetter(validName, strTitle);
                if (validName == "")
                {
                    return "";
                }
                else
                    OutputName = validName; //update property
            }

            return "";
        }

        private void SetupForPointProcessing()
        {
            if (cboDirection.SelectedItem != null)
                DirectionField = cboDirection.SelectedItem.ToString();
            else
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Select Direction Field From List", strTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            if (cboDirection.SelectedItem.ToString() == "")
            {
                ArcGIS.Desktop.Framework.Dialogs.MessageBox.Show("Select Direction Field", strTitle, MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

        }


        private async Task<bool> AddLayerToMap(string roseName)
        {
            //Check if layer already added
            bool bLayer = LayerExistsInTOC(roseName);

            if (bLayer)
                return true;

            string databasePath = inputDatabasePath;

            if (outputDatabasePath != "")
                databasePath = outputDatabasePath;

            string url = databasePath + "\\" + roseName;  //FeatureClass of a FileGeodatabase

            Uri uri = new Uri(url);
            await QueuedTask.Run(() => LayerFactory.Instance.CreateLayer(uri, MapView.Active.Map));

            return true;
        }


        private bool LayerExistsInTOC(string roseName)
        {
            var layers = MapView.Active.Map.GetLayersAsFlattenedList().Where(l => l.Name == roseName).Count();

            if (layers == 0)
                return false;
            else
                return true;

        }

        private void chkSelected_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
