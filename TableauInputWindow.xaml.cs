using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Simplex_Library;
using Exceptions_Library;

namespace WPF_code
{
    /// <summary>
    /// Interaction logic for TableauInputWindow.xaml
    /// </summary>
    public partial class TableauInputWindow : Window
    {
        public TableauInputWindow(List<ArraySimplexRowInput> inputList)
        {
            InitializeComponent();
            //Inspired by Stack Overflow (2011)
            Input = new ObservableCollection<ArraySimplexRowInput>(inputList);
            string[] variables = inputList[0].ColumnNames;
            DataGridTextColumn column = new DataGridTextColumn()
            {
                Header = "Basic Variable",
                Binding = new Binding("BasicVariable"),
                IsReadOnly = true
            };
            //Binds to BasicVariable property of ArraySimplexRowInput
            InputGrid.Columns.Add(column);
            for (int i = 0; i < variables.GetLength(0); i++)
            {
                column = new DataGridTextColumn()
                {
                    Header = variables[i],
                    Binding = new Binding("ColumnValues[" + i + "]")
                    {
                        Mode = BindingMode.TwoWay
                    },
                    IsReadOnly = false
                };
                //produces columns with the names of the variables
                //and bound to the elements in the ColumnValue array
                
                InputGrid.Columns.Add(column);
            }

            InputGrid.ItemsSource = Input;
        }
        
        private ObservableCollection<ArraySimplexRowInput> Input { get; set; }

        private void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var row in Input)
            {
                Trace.WriteLine(row.ToString());
                //information to debugger
            }
            ArraySimplexRowInput[] inputRows = Input.ToArray();
            ITableau tableau;
            try
            {
                tableau = new SimplexTableau(inputRows);
            }
            catch (SimplexTableauConstructionException error)
            {
                MessageBox.Show(this, error.Message, Title="Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                tableau = tableau.BasicSimplex();
                //Implementing the other simplex methods would require a switch here.
            }
            catch (NoPositiveThetaException)
            {
                MessageBox.Show(this, "The algorithm produced rows that had no finite, positive theta value. " +
                    "Ensure the columns of basic variables have 1s only in the corresponding rows",
                    Title = "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            
            MessageBox.Show(this, tableau.GetValuesMessage(), Title = "Success!", MessageBoxButton.OK);
        }
    }
}
