using System;
using System.Windows;
using System.Windows.Controls;
using Simplex_Library;

namespace WPF_code
{
    /// <summary>
    /// Interaction logic for SimplexStartWindow.xaml
    /// </summary>
    public partial class SimplexStartWindow : Window
    {
        private SimplexMethod Method { get; set; }
        private bool SelectedMethod { get; set; }
        public SimplexStartWindow()
        {
            InitializeComponent();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            string errorMessage = "";
            if (SelectedMethod is false)
            {
                errorMessage += "You have not selected a simplex method!\n";
            }
            if (int.TryParse(ConstraintTextBox.Text, out int numberOfConstraints) is false)
            {
                errorMessage += "You have not entered an integer number of constraints!\n";
            }
            else if (numberOfConstraints < 1)
            {
                errorMessage += "You have not entered a positive number of constraints!\n";
            }

            if (errorMessage != "")
            {
                MessageBox.Show(this, errorMessage, Title = "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                switch (Method)
                {
                    case SimplexMethod.Basic:
                        new BasicSimplexStart(numberOfConstraints).Show();
                        Close();
                        break;
                    default:
                        throw new NotImplementedException();
                }
            }
        }

        private void MethodComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBoxItem item = (ComboBoxItem)MethodComboBox.SelectedItem;
            switch (item.Content)
            {
                case "Basic":
                    Method = SimplexMethod.Basic;
                    SelectedMethod = true;
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
    }
}
