using System;
using System.Windows;
using System.Windows.Controls;
using Exceptions_Library;
using Graph_Library;

namespace WPF_code
{
    /// <summary>
    /// Interaction logic for NetworkStartWindow.xaml
    /// </summary>
    
    

    public partial class NetworkStartWindow : Window
    {
        //combo boxes must only contain combo box items
        public NetworkStartWindow()
        {
            InitializeComponent();
        }
        
        private GraphAlgorithm ChosenAlgorithm { get; set; }
        private bool SelectedGraphAlgorithm { get; set; }

        private bool IsDirected { get; set; }
        private bool SelectedDirected { get; set; }
        
        private const char NameSplitCharacter = ' ';

        private void AlgorithmComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)AlgorithmComboBox.SelectedItem;
            switch (item.Content)
            {
                case "Dijkstra's Shortest Path":
                    ChosenAlgorithm = GraphAlgorithm.Dijkstra;
                    SelectedGraphAlgorithm = true;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new();
            mainWindow.Show();
            Close();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            string errorDialogue = "";
            string[] nodeNames = {};
            if (SelectedGraphAlgorithm is false)
            {
                errorDialogue += "You have not selected a graph algorithm! \n";
            }
            if (!int.TryParse(NodeNumberTextBox.Text, out int numberOfNodes))
            {
                errorDialogue += "You have not selected an integer number of nodes! \n";
            }
            else if (numberOfNodes <= 0)
            {
                errorDialogue += "You have not selected a positive number of nodes! \n";
            }
            if (NodeNamesTextBox.Text.Contains(NameSplitCharacter))
            {
                nodeNames = NodeNamesTextBox.Text.Split(NameSplitCharacter);
                if (nodeNames.GetLength(0) != numberOfNodes)
                {
                    errorDialogue += "The number of names does not equal the entered number of nodes! \n";
                }
            }
            else
            {
                errorDialogue += "There are no names to split! \n";
            }
            if (SelectedDirected is false)
            {
                errorDialogue += "You have not selected whether the graph is directed! \n";
            }
            

            if (errorDialogue != "")
            {
                MessageBox.Show(this, "You cannot create a graph yet \n" + errorDialogue, 
                    Title = "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            //execution moves to new window
            new NetworkInputWindow(nodeNames, IsDirected, ChosenAlgorithm).Show();
            Close();
        }

        private void DirectedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)DirectedComboBox.SelectedItem;
            switch(item.Content)
            {
                case "Directed":
                    IsDirected = SelectedDirected = true;
                    break;
                case "Undirected":
                    IsDirected = false;
                    SelectedDirected = true;
                    break;
                default:
                    throw new NotAnOptionException();
            }
        }
    }
}
