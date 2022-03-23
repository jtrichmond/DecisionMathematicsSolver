using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using Sorting_Library;
using Exceptions_Library;

namespace WPF_code
{
    /// <summary>
    /// Interaction logic for SortWindow.xaml
    /// </summary>
    public partial class SortWindow : Window
    {
        public SortWindow()
        {
            InitializeComponent();
        }
        //Combo boxes must only contain combo box items
        //properties holding user options, and flags for whether the user has
        //selected an option
        private DataType ChosenDataType { get; set; } 
        private bool HasSelectedType { get; set; }
        private bool ChosenAscending { get; set; } 
        private bool HasSelectedAscending { get; set; }
        private bool ChosenQuickSort { get; set; } 
        private bool HasSelectedQuickSort { get; set; }
        private bool ChosenNamed { get; set; } 
        private bool HasSelectedNamed { get; set; }
        private const char NameSeparator = ':'; //could use data binding to change window display in future
        private const char ItemSeparator = ' ';


        private void DataTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)DataTypeComboBox.SelectedItem;
            Trace.WriteLine(item.Content);
            switch (item.Content)
            {
                case "Number":
                    ChosenDataType = DataType.Number;
                    HasSelectedType = true;
                    break;
                case "String":
                    ChosenDataType = DataType.String;
                    HasSelectedType = true;
                    break;
                case "Character":
                    ChosenDataType = DataType.Character;
                    HasSelectedType = true;
                    break;
                default:
                    throw new DataTypeException("Invalid data type");
            }
        }

        private void SortOrderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)SortOrderComboBox.SelectedItem;
            Trace.WriteLine(item.Content);
            switch (item.Content)
            {
                case "Ascending":
                    ChosenAscending = true;
                    HasSelectedAscending = true;
                    break;

                case "Descending":
                    ChosenAscending= false;
                    HasSelectedAscending = true;
                    break;
                default:
                    throw new NotAnOptionException();
            }
        }

        private void SortTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)SortTypeComboBox.SelectedItem;
            Trace.WriteLine(item.Content);
            switch (item.Content)
            {
                case "Quick Sort":
                    ChosenQuickSort = true;
                    HasSelectedQuickSort = true;
                    break;

                case "Bubble Sort":
                    ChosenQuickSort = false;
                    HasSelectedQuickSort = true;
                    break;

                default:
                    throw new NotImplementedException();
                //have not implemented other sorts
            }

        }

        private void NamedComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)NamedComboBox.SelectedItem;
            Trace.WriteLine(item.Content);
            switch (item.Content)
            {
                case "Yes":
                    ChosenNamed = true;
                    HasSelectedNamed = true;
                    break;

                case "No":
                    ChosenNamed = false;
                    HasSelectedNamed = true;
                    break;

                default :
                    throw new NotAnOptionException();
            }
        }

        private void SortButton_Click(object sender, RoutedEventArgs e)
        {
            Trace.WriteLine("DataType " + ChosenDataType);
            Trace.WriteLine("Ascending " + ChosenAscending);
            Trace.WriteLine("Quick sort " + ChosenQuickSort);
            Trace.WriteLine("Named " + ChosenNamed);
            //gives information to debugging panel
            
            string errorDialogue = "";
            string rawString = ItemsTextBox.Text;
            if (HasSelectedType is false)
                errorDialogue += "You have not selected a data type! \n";
            if (HasSelectedAscending is false)
                errorDialogue += "You have not selected a sort order! \n";
            if (HasSelectedQuickSort is false)
                errorDialogue += "You have not selected a sort type! \n";
            if (HasSelectedNamed is false)
                errorDialogue += "You have not selected whether there is data associated with the values to be sorted! \n";
            if (errorDialogue is not "")
            {
                MessageBox.Show(this, "You cannot sort yet \n" + errorDialogue, Title = "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return; //halts method execution
            }

            string[] rawStringArray;
            try
            {
                rawStringArray = SortFactory.SplitItems(rawString, ItemSeparator);
            }
            catch (SplitException)
            {
                MessageBox.Show(this, "There are no items to sort!", Title = "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }


            dynamic[] valuesArray;
            dynamic sortingArray; //type will depend on construction
 
            if (ChosenNamed) 
            {
                string[] namesArray;
                try
                {
                    (string[] Names, string[] Values) namesAndValues = SortFactory.SplitNamesAndValues(rawStringArray, NameSeparator);
                    namesArray = namesAndValues.Names;
                    valuesArray = SortFactory.ParseArrayToType(namesAndValues.Values, ChosenDataType);
                }
                catch (SplitException)
                {
                    MessageBox.Show(this, "There is no additional data to separate from the values", Title = "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                catch(ParseException)
                {
                    MessageBox.Show(this, "The values entered are not of the same data type as the type selected", 
                        Title = "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    sortingArray = SortFactory.MakeNamedArray(namesArray,valuesArray, ChosenDataType);
                }
                catch (SortingArrayConstructionException error)
                {
                    MessageBox.Show(this, error.Message, Title = "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            else //unnamed chosen
            {
                try
                {
                    valuesArray = SortFactory.ParseArrayToType(rawStringArray, ChosenDataType);
                }
                catch (ParseException)
                {
                    MessageBox.Show(this, "The values entered are not of the same data type as the type selected", 
                        Title = "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                try
                {
                    sortingArray = SortFactory.MakeUnnamedArray(valuesArray, ChosenDataType);
                }
                catch (SortingArrayConstructionException error)
                {
                    MessageBox.Show(this, error.Message, Title = "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            //extension: sort results window
            switch (ChosenDataType)
            {
                case DataType.Character:
                    DisplayCharacterSortResults(sortingArray);
                    break;
                case DataType.String:
                    DisplayStringSortResults(sortingArray);
                    break;
                case DataType.Number:
                    DisplayNumberSortResults(sortingArray);
                    break;
            }
            
        }

        //separate routines as different data types for resultsArray,
        //variable type declaration cannot depend on factors at runtime
        private void DisplayCharacterSortResults(dynamic sortingArray)
        {
            ISortingArray<char> resultsArray;
            try
            {
                resultsArray = (ISortingArray<char>) sortingArray;
            }
            catch (InvalidCastException e)
            {
                MessageBox.Show(this, e.Message, Title = "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string result = ChosenQuickSort 
                ? resultsArray.QuickSort(resultsArray, ChosenAscending).GetResultsString() 
                : resultsArray.BubbleSort(resultsArray, ChosenAscending).GetResultsString();

            DisplayResultMessageBox(result);
        }

        private void DisplayStringSortResults(dynamic sortingArray)
        {
            ISortingArray<string> resultsArray;
            try
            {
                resultsArray = (ISortingArray<string>)sortingArray;
            }
            catch (InvalidCastException e)
            {
                MessageBox.Show(this, e.Message, Title = "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string result = ChosenQuickSort 
                ? resultsArray.QuickSort(resultsArray, ChosenAscending).GetResultsString() 
                : resultsArray.BubbleSort(resultsArray, ChosenAscending).GetResultsString();

            DisplayResultMessageBox(result);
        }

        private void DisplayNumberSortResults(dynamic sortingArray)
        {
            ISortingArray<double> resultsArray;
            try
            {
                resultsArray = (ISortingArray<double>)sortingArray;
            }
            catch (InvalidCastException e)
            {
                MessageBox.Show(this, e.Message, Title = "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string result = ChosenQuickSort 
                ? resultsArray.QuickSort(resultsArray, ChosenAscending).GetResultsString() 
                : resultsArray.BubbleSort(resultsArray, ChosenAscending).GetResultsString();

            DisplayResultMessageBox(result);
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mainWindow = new();
            mainWindow.Show();
            Close();
        }

        private void DisplayResultMessageBox(string result)
        {
            MessageBox.Show(this, result, Title = "Result", MessageBoxButton.OK);
        }
    }
}
