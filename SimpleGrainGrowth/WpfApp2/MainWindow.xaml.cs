using GrainGrowth.Lib.builders;
using GrainGrowth.Lib.Enums;
using GrainGrowth.Lib.Extensions;
using GrainGrowth.Lib.Handlers;
using Grains.Lib.Enums;
using Grains.Library.Enums;
using Grains.Library.Extensions;
using Grains.Utilities.ImageHandler;
using Grains.Utilities.IOHandlers;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WpfApp2
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private Rectangle[,] array;
        private builder builder;
        private Color[] colorsArray;
        private bool[,] renderingArray;
        private int xDimension;
        private int yDimension;
        private int matrixIdsNumber;
        private readonly BackgroundWorker backgroundWorker;
        private readonly BackgroundWorker clearingBackgroundWorker;
        private readonly DispatcherTimer dispatcherTimer;
        private SimulationType currentChosenSimulation = SimulationType.CellularAutomata;
        private bool microstructureDisplayed;


        public MainWindow()
        {
            xDimension = 100;
            yDimension = 100;

            InitializeComponent();
            builder = new builder(xDimension, yDimension);
            colorsArray = new Color[10];
            array = new Rectangle[xDimension, yDimension];
            renderingArray = new bool[xDimension, yDimension];
            InitializeComponent();
            InitializeRectanglesOnCanvas(xDimension, yDimension);
            backgroundWorker = new BackgroundWorker();
            clearingBackgroundWorker = new BackgroundWorker();
            matrixIdsNumber = builder.MaxCellIdNumber;
            dispatcherTimer = new DispatcherTimer();
            dispatcherTimer.Tick += dispatcherTimer_Tick;
            dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 0, 50);

            backgroundWorker.DoWork += backgroundWorker_DoWork;
            backgroundWorker.RunWorkerCompleted += worker_RunWorkerCompleted;
            clearingBackgroundWorker.DoWork += clearingbackgroundWorker_DoWork;

            builder.StepAdding += UpdateCurrentRXStep;

            InitializeComboBoxes();
            MakeColorsGrains(0);

        }



        private async void addRandomGrainsButton_Click(object sender, RoutedEventArgs e)
        {
            await builder.AddRandomGrains(Convert.ToInt32(addRandomGrainsNumber.Text));

            InitializeColorsArray(builder.MaxCellIdNumber);

            RefreshFullArray();
        }

        private async void clearingbackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            await builder.Clear();
            await Dispatcher.BeginInvoke(DispatcherPriority.Normal, (Action)(() =>
            {
                this.colorsArray = new Color[0];
                InitializeColorsArray(0);
                ClearArray();
            }));

        }

        private void InitializeRectanglesOnCanvas(int width, int height)
        {
            double rectangeWidth = canvas.Width / width;
            double rectangeHeight = canvas.Height / height;

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    var rect = new Rectangle();
                    rect.Width = rectangeWidth;
                    rect.Height = rectangeHeight;

                    rect.Fill = new SolidColorBrush(Colors.Azure);

                    array[i, j] = rect;

                    Canvas.SetLeft(rect, i * rectangeWidth);
                    Canvas.SetTop(rect, j * rectangeHeight);

                    if (!CheckAccess())
                    {
                        Dispatcher.Invoke(() => canvas.Children.Add(rect));
                        return;
                    }

                    canvas.Children.Add(rect);
                }
            }
        }
        private void MakeColorsGrains(int value)
        {
            colorsArray = new Color[value + 2];
            var rand = new Random();
            colorsArray[0] = Colors.White;
            colorsArray[1] = Colors.Black;

            for (int i = 2; i < value + 2; i++)
            {
                var color = new Color();
                color.R = (byte)rand.Next(0, 255);
                color.G = (byte)rand.Next(0, 255);
                color.B = (byte)rand.Next(0, 255);
                color.A = 255;
                colorsArray[i] = color;
            }
        }

        private void worker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (matrixIdsNumber < builder.MaxCellIdNumber)
            {
                InitializeColorsArray(builder.MaxCellIdNumber - matrixIdsNumber, true);
                matrixIdsNumber = builder.MaxCellIdNumber;
            }
            RefreshFullArray();
        }

        private void RefreshFullArray()
        {
            for (int i = 0; i < xDimension; i++)
            {
                for (int j = 0; j < yDimension; j++)
                {
                    if (builder.Array[i, j] == 0)
                    {
                        continue;
                    }

                    if (array[i, j].Fill.ToString().Equals(colorsArray[builder.Array[i, j]].ToString()))
                    {
                        continue;
                    }

                    array[i, j].Fill = new SolidColorBrush(colorsArray[builder.Array[i, j]]);
                }
            }
        }


        private void dispatcherTimer_Tick(object sender, EventArgs e)
        {

            if (builder.TypesOfSimulation == SimulationType.SRXMonteCarlo && builder.CurrentStep >= builder.StepsLimit)
            {
                builder.TypesOfSimulation = currentChosenSimulation;
                startButton.IsEnabled = true;
                rxStart1.Content = "Start RX";
                dispatcherTimer.Stop();
            }

            if (!backgroundWorker.IsBusy)
            {
                backgroundWorker.RunWorkerAsync();
            }
        }




        private void addRandomGrainsNumber_TextChanged(object sender, TextChangedEventArgs e)
        {

        }


        private void startButton_Click_1(object sender, RoutedEventArgs e)
        {
            if (dispatcherTimer.IsEnabled)
            {
                startButton.Content = "Start";
                dispatcherTimer.Stop();
                rxStart.IsEnabled = true;
            }
            else
            {
                startButton.Content = "Stop";
                dispatcherTimer.Start();
                rxStart.IsEnabled = false;
            }
        }



        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }


        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            int x = 90;
            double j = 0.1;
            matrixIdsNumber = builder.MaxCellIdNumber;

            xTextBox.Dispatcher.Invoke(DispatcherPriority.Normal,
                (ThreadStart)delegate { x = Convert.ToInt32(this.incusionsNumberField.Text); });
            jgbValues.Dispatcher.Invoke(DispatcherPriority.Normal,
                (ThreadStart)delegate { j = double.Parse(jgbValues.Text, System.Globalization.CultureInfo.InvariantCulture); });

            builder.Step(x, j);
        }



        private void numberOfInclusionsField_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void ClearArray()
        {
            for (int i = 0; i < xDimension; i++)
            {
                for (int j = 0; j < yDimension; j++)
                {
                    array[i, j].Fill = new SolidColorBrush(colorsArray[0]);
                }
            }
        }
        private void exportTxt_Click(object sender, RoutedEventArgs e)
        {
            var savingHandler = new SaveHandler("Text files (*.txt)|*.txt|All files (*.*)|*.*");
            var path = savingHandler.GetPath();

            TextHandler.ExportToTextFile(builder.Array, xDimension, yDimension, path);
        }

        private void exportImage_Click(object sender, RoutedEventArgs e)
        {
            var savingHandler = new SaveHandler("Png files (*.png)|*.png|All files (*.*)|*.*");
            var path = savingHandler.GetPath();

            ImageHandler.ExportToImage(canvas, path);
        }

        private async void importTxt_Click(object sender, RoutedEventArgs e)
        {
            var openingHandler = new OpeningHandler("Text files (*.txt)|*.txt|All files (*.*)|*.*");
            var path = openingHandler.GetPath();

            TextHandler.ImportFromTextFile(builder.Array, xDimension, yDimension, path);

            await Task.Run(() =>
            {
                renderingArray = new bool[xDimension, yDimension];
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    ClearArray();
                    MakeColorsGrains(builder.Array.Max());
                    RefreshFullArray();
                }));
            });
        }

        private async void importImage_Click(object sender, RoutedEventArgs e)
        {
            var openingHandler = new OpeningHandler("Png files (*.png)|*.png|All files (*.*)|*.*");
            var path = openingHandler.GetPath();

            ImageHandler.ImportFromImage(builder.Array, xDimension, yDimension, 600, path);

            await Task.Run(() =>
            {
                renderingArray = new bool[xDimension, yDimension];
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    ClearArray();
                    MakeColorsGrains(builder.Array.Max());
                    RefreshFullArray();
                }));
            });
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private async void inslucionButton_Click(object sender, RoutedEventArgs e)
        {
            await builder.AddInclusions(Convert.ToInt32(incusionsNumberField.Text), Convert.ToInt32(inclusionsSizeField.Text),
             (Inclusions)inclusionsComboBox.SelectedItem);

            await Task.Run(() =>
            {
                renderingArray = new bool[xDimension, yDimension];
                Dispatcher.BeginInvoke((Action)(() =>
                {
                    Dispatcher.BeginInvoke((Action)(() =>
                    {
                        renderingArray = new bool[xDimension, yDimension];
                        RefreshFullArray();
                    }));
                }));
            });
        }

        private async void restartArray_Click(object sender, RoutedEventArgs e)
        {
            await builder.Clear();
            renderingArray = new bool[xDimension, yDimension];
            ClearArray();
        }


        // dualPhase and Substructure and Inclusions
        private void InitializeComboBoxes()
        {
            var inclusions = new List<Inclusions>
            {
                Inclusions.Circular,
                Inclusions.Square
            };

            inclusionsComboBox.ItemsSource = inclusions;
            inclusionsComboBox.SelectedIndex = 0;

            var substructures = new List<Substructures>
            {
               Substructures.Substructure,
               Substructures.DualPhase
            };

            substructuresComboBox.ItemsSource = substructures;
            substructuresComboBox.SelectedIndex = 0;
        }
        private void icnlusionsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void neighbourhoodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            builder.SetNeighbourhood(Neighbourhood.VonNeumann);
        }

        private void Moore_Checked(object sender, RoutedEventArgs e)
        {
            builder.SetNeighbourhood(Neighbourhood.VonNeumann);
        }

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            builder.SetNeighbourhood(Neighbourhood.Moore);
        }

        private void RadioButton_Checked_1(object sender, RoutedEventArgs e)
        {
            builder.SetNeighbourhood(Neighbourhood.VonNeumann);
        }

        private void RadioButton_Checked_2(object sender, RoutedEventArgs e)
        {
            builder.SetNeighbourhood(Neighbourhood.MooreExtented);
        }

        private void xTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (!backgroundWorker.IsBusy)
            {
                backgroundWorker.RunWorkerAsync();
            }
        }

        private async void MonteCarloGenerateBurron_Click(object sender, RoutedEventArgs e)
        {
            await RunLongTask(this.builder.GenerateMonteCarloArea(Convert.ToInt32(MonteCarloVariation.Text)),
                 () =>
                 {
                     InitializeColorsArray(Convert.ToInt32(MonteCarloVariation.Text));
                     RefreshFullArray();
                 });
        }

        private void InitializeColorsArray(int value, bool oneColorType = false)
        {
            var rand = new Random();

            if (colorsArray != null && colorsArray.Count() != 0)
            {
                if (this.builder.Array.Max() + 2 <= colorsArray.Length)
                {
                    return;
                }

                var oldColors = colorsArray;

                colorsArray = new Color[oldColors.Count() + value];

                for (int i = 0; i < oldColors.Count(); i++)
                {
                    colorsArray[i] = oldColors[i];
                }

                for (int i = oldColors.Count(); i < oldColors.Count() + value; i++)
                {
                    colorsArray[i] = GetRandomColor(rand, oneColorType);
                }

            }
            else
            {
                colorsArray = new Color[value + 2];
                colorsArray[0] = Colors.White;
                colorsArray[1] = Colors.Black;

                for (int i = 2; i < value + 2; i++)
                {
                    colorsArray[i] = GetRandomColor(rand, oneColorType);
                }
            }
        }


        private Color GetRandomColor(Random random, bool redColor = false)
        {
            var color = new Color();
            color.R = (byte)random.Next(0, 255);
            color.A = 255;

            if (redColor)
            {
                color.G = 0;
                color.B = 0;
            }
            else
            {
                color.G = (byte)random.Next(0, 255);
                color.B = (byte)random.Next(0, 255);
            }

            return color;
        }

        private async void EnergySpreadButton_Click(object sender, RoutedEventArgs e)
        {
            EnergySpreadType energyDistributionType;
            if ((bool)heterogenousRadioButton.IsChecked)
            {
                energyDistributionType = EnergySpreadType.Heterogoniczna;
            }
            else
            {
                energyDistributionType = EnergySpreadType.Homogeniczna;
            }
            await RunLongTask(this.builder.DistributeEnergy(energyDistributionType), () =>
            {
                if (!microstructureDisplayed)
                {
                    DisplayEnergyArray();
                }
                energyDistributionCheckBox.IsChecked = true;
            });
        }

        private async Task RunLongTask(Task longTask, Action afterCompleted)
        {
            var longOperation = longTask.ContinueWith((task) =>
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    afterCompleted();
                    loadingStackPanel.Visibility = Visibility.Collapsed;
                }));
            });


            await longOperation;
        }

        private void jgbValues_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (double.Parse(jgbValues.Text, System.Globalization.CultureInfo.InvariantCulture) < 0.1)
            {
                jgbValues.Text = "0.1";

            }
            if (double.Parse(jgbValues.Text, System.Globalization.CultureInfo.InvariantCulture) > 1)
            {
                jgbValues.Text = "1";

            }
        }

        private void UpdateCurrentRXStep(int stepNumber)
        {
            Dispatcher.Invoke((Action)(() => { this.numberOfSteps.Text = stepNumber.ToString(); }));
        }


        private void DisplayEnergyArray()
        {
            for (int i = 0; i < xDimension; i++)
            {
                for (int j = 0; j < yDimension; j++)
                {
                    var color = new Color();
                    color.R = (byte)(255 / (builder.Energy[i, j] + 1));
                    color.G = (byte)(255 / (builder.Energy[i, j] + 1));
                    color.B = (byte)(255 / (builder.Energy[i, j] + 1));
                    color.A = 255;
                    array[i, j].Fill = new SolidColorBrush(color);
                }
            }
        }

        private void EnergySpreadEnable_Checked(object sender, RoutedEventArgs e)
        {
            rxStart.IsEnabled = true;
        }

        private void RxButton_Click(object sender, RoutedEventArgs e)
        {
            if (dispatcherTimer.IsEnabled)
            {
                builder.TypesOfSimulation = currentChosenSimulation;
                startButton.IsEnabled = true;
                rxStart1.Content = "Start RX";
                dispatcherTimer.Stop();
            }
            else
            {
                builder.TypesOfSimulation = SimulationType.SRXMonteCarlo;
                startButton.IsEnabled = false;
                rxStart1.Content = "Stop RX";
                dispatcherTimer.Start();
            }
        }

        private void EnergySpreadCheck_Checked(object sender, RoutedEventArgs e)
        {
            rxStart1.IsEnabled = true;
        }

        private void EnergySpreadCheck_Unchecked(object sender, RoutedEventArgs e)
        {
            rxStart1.IsEnabled = false;
        }

        private async void clearEnergy_Click(object sender, RoutedEventArgs e)
        {
            await RunLongTask(builder.ClearEnergy(), () =>
            {
                if (!microstructureDisplayed)
                {
                    DisplayEnergyArray();
                }
                energyDistributionCheckBox.IsChecked = false;
            });
        }

        private void ShowHideEnergy_Click(object sender, RoutedEventArgs e)
        {
            if (microstructureDisplayed)
            {
                DisplayEnergyArray();
                this.showHideEnergy.Content = "Show microstructure";
              
            }
            else
            {
                RefreshFullArray();
                this.showHideEnergy.Content = "Show energy";
               
            }

            microstructureDisplayed = !microstructureDisplayed;
        }

        private async void makeBorder_Click(object sender, RoutedEventArgs e)
        {
            int thick = 1;
            await RunLongTask(builder.AddBorders(thick), (() =>
            {
                RefreshFullArray();

            }));
        }

        private async void clearBorderBackground_Click(object sender, RoutedEventArgs e)
        {
            await RunLongTask(builder.ClearAllButBorders(),
                () =>
                {
                    ClearArray();
                    RefreshFullArray();
                });
        }

        private async void RxNucleonsButton_Click(object sender, RoutedEventArgs e)
        {
            
            await RunLongTask(this.builder.AddRecrystalisedNucleons(), () => {
                InitializeColorsArray(builder.NucleArea, true);
                RefreshFullArray();
            });
        }

        private void homogenousRadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }


        private void heterogenousRadioButton_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void cellularAutomataSimluation_Checked(object sender, RoutedEventArgs e)
        {
            this.builder.TypesOfSimulation = SimulationType.CellularAutomata;
            currentChosenSimulation = SimulationType.CellularAutomata;
        }

        private void monteCarloSimluationChecked(object sender, RoutedEventArgs e)

        {
            this.builder.TypesOfSimulation = SimulationType.MonteCarlo;
            currentChosenSimulation = SimulationType.MonteCarlo;
        }

        private void startButton_Click(object sender, RoutedEventArgs e)
        {
            if (dispatcherTimer.IsEnabled)
            {
                startButton.Content = "Start";
                dispatcherTimer.Stop();
                rxStart1.IsEnabled = true;
            }
            else
            {
                startButton.Content = "Stop";
                dispatcherTimer.Start();
                rxStart1.IsEnabled = false;
            }
        }

        private void rxNucleonsSizeConst_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void constantRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            builder.TypeOfNucleation = NucleationModuleType.Constant;
            builder.NucleArea = Convert.ToInt32(this.rxConstantNucleonsTextBox.Text);
            rxStart1.IsEnabled = false;
        }

        private void dualPhasen_Checked_3(object sender, RoutedEventArgs e)
        {

        }

        private async void startSubstructersButton_Click_1(object sender, RoutedEventArgs e)
        {
            await RunLongTask(builder.CreateSubstructure((Substructures)substructuresComboBox.SelectedItem, Convert.ToInt32(substructerBox.Text)),
                () => {
                    ClearArray();
                    RefreshFullArray();
                });
        }

        private void substructerTextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {

        }

       
        private void substructuresComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}
