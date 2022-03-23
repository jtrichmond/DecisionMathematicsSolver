using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Graph_Library;

namespace WPF_code
{
    /// <summary>
    /// Interaction logic for DijkstraWindow.xaml
    /// </summary>
    public partial class DijkstraWindow : Window
    {
        private INetwork Network { get; set; }
        private string StartNode { get; set; } = "";
        private bool SelectedStart { get; set; }

        public DijkstraWindow(INetwork network)
        {
            InitializeComponent();
            Network = network;
            foreach (string nodeName in network.GetNodeNames())
            {
                ComboBoxItem item = new()
                {
                    Content = nodeName
                };
                StartNodeComboBox.Items.Add(item);
            }
            //produces options in drop down menu from nodes in the network
        }

        private void StartNodeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)StartNodeComboBox.SelectedItem;
            if (Network.GetNodeNames().Contains(item.Content) && item.Content is not null)
            {
                StartNode = (string) item.Content;
                SelectedStart = true;
            }
            else
            {
                throw new KeyNotFoundException("Node not found in graph");
            }
        }

        private void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedStart is false)
            {
                MessageBox.Show(this, "You have not selected a start node!",
                    Title = "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                (string[] Names, double[] Weights, string[] PreviousNodes) results =
                    Network.Dijkstra(StartNode);
                string output = "";
                for (int i = 0; i < results.Names.GetLength(0); i++)
                {
                    output += $"Node {i+1} is {results.Names[i]}: shortest weight is {results.Weights[i]}, " +
                        $"from {results.PreviousNodes[i]} \n";
                }
                MessageBox.Show(this, output);
            }
        }
    }
}
