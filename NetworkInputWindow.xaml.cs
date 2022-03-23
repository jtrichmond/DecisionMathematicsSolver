using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Graph_Library;
using Exceptions_Library;

namespace WPF_code
{
    /// <summary>
    /// Interaction logic for NetworkInputWindow.xaml
    /// </summary>
    public partial class NetworkInputWindow : Window
    {
        
        public NetworkInputWindow(string[] nodeNames, bool isDirected, GraphAlgorithm algorithm)
        {
            InitializeComponent();
            NodeNames = nodeNames;
            IsDirected = isDirected;
            Algorithm = algorithm;
            //code inspired by StackOverflow (2011)
            Input = new ObservableCollection<ArrayNetworkRowInput>();
            //Observable collection class implements interface ensuring properties are updated
            //when a user changes them (Microsoft, 2022)
            DataGridTextColumn column = new()
            {
                Header = "Nodes",
                Binding = new Binding("NodeName"),
                IsReadOnly = true
            };
            //Column values bind to the NodeName property in ArrayNetworkRowInput objects
            InputGrid.Columns.Add(column);
            for (int i = 0; i < NodeNames.GetLength(0); i++)
            {
                Input.Add(new ArrayNetworkRowInput()
                    {NodeName = NodeNames[i], Values = new string[nodeNames.GetLength(0)]});
                column = new DataGridTextColumn
                {
                    Header = NodeNames[i],
                    Binding = new Binding("Values[" + i + "]")
                    {
                        Mode = BindingMode.TwoWay //data in program changes GUI, and GUI changes program
                    }
                    //Produces columns bound to elements in the Values array property
                };
                
                InputGrid.Columns.Add(column);
                //Adds the column to the DataGrid control
            }
            
            InputGrid.ItemsSource = Input;
            //the InputGrid control sources the data for its columns from Input
        }
        
        private ObservableCollection<ArrayNetworkRowInput> Input { get; set; }
        private string[] NodeNames { get; set; }
        private bool IsDirected { get; set; }
        private GraphAlgorithm Algorithm { get; set; }
        private const char SplitCharacter = ',';

        private void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            ArrayNetworkRowInput[] rowInputs = Input.ToArray();
            INetwork network;
            try
            {
                (string[] Names, IList<double>[,] Weights) nodeNamesAndWeights = 
                    NetworkFactory.ExtractFromInput(rowInputs, SplitCharacter);
                network = new Network(nodeNamesAndWeights.Names, nodeNamesAndWeights.Weights, IsDirected);
            }
            catch (Exception error)
            {
                if (error is NetworkConversionException or NetworkConstructionException)
                {
                    MessageBox.Show(this, error.Message, Title = "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                else throw; //"Don't feel obliged to catch every exception that your code might throw" (Miles, 2018, p. 72)
            }

            switch (Algorithm)
            {
                case GraphAlgorithm.Dijkstra:
                    new DijkstraWindow(network).Show();
                    Close();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
