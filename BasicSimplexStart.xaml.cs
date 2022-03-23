using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Simplex_Library;

namespace WPF_code
{
    /// <summary>
    /// Interaction logic for BasicSimplexStart.xaml
    /// </summary>
    public partial class BasicSimplexStart : Window
    {
        private int NumberOfConstraints { get; set; }
        private const char SplitCharacter = ' ';
        public BasicSimplexStart(int numberOfConstraints)
        {
            InitializeComponent();
            NumberOfConstraints = numberOfConstraints;
        }

        private void MainMenuButton_Click(object sender, RoutedEventArgs e)
        {
            new MainWindow().Show();
            Close();
        }

        private void ContinueButton_Click(object sender, RoutedEventArgs e)
        {
            string errorMessage = "";
            string objectiveVariable = "";
            if (SimplexFactory.TrySplitNames(VariableNameTextBox.Text, SplitCharacter, out string[] varNames) is false)
            {
                errorMessage += "The variable names could not be separated! \n";
            }
            if (SimplexFactory.TrySplitNames(BasicVariablesTextBox.Text, SplitCharacter, out string[] basicVars) is false)
            {
                errorMessage += "The basic variable names could not be separated! \n";
            }
            else if (basicVars.GetLength(0) != NumberOfConstraints)
            {
                errorMessage +=
                    $"{basicVars.GetLength(0)} basic variables were given when there should have been {NumberOfConstraints}! \n";
            }
            else
            {
                foreach (string name in basicVars)
                {
                    if (varNames.Contains(name) is false)
                    {
                        errorMessage += $"{name} was not a given variable! \n";
                    }
                }
            }
            if (varNames.Contains(ObjectiveTextBox.Text))
            {
                errorMessage += $"{ObjectiveTextBox.Text} was given as a variable! \n";
            }
            else
            {
                objectiveVariable = ObjectiveTextBox.Text;
            }

            if (errorMessage != "")
            {
                MessageBox.Show(this, errorMessage, Title = "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            string[] rowVariables = new string[basicVars.GetLength(0) + 1];
            //Last element holds data about the objective row
            basicVars.CopyTo(rowVariables, 0);
            rowVariables[basicVars.GetLength(0)] = objectiveVariable;
            RowType[] rowTypes = new RowType[rowVariables.GetLength(0)];
            for (int i = 0; i < basicVars.GetLength(0); i++)
            {
                rowTypes[i] = RowType.Variable;
            }

            rowTypes[basicVars.GetLength(0)] = RowType.PrimaryObjective;

            List<ArraySimplexRowInput> inputList = SimplexFactory.MakeEmptyInputListFromArrays(
                varNames, rowVariables, rowTypes);
            
            new TableauInputWindow(inputList).Show();
            Close();

        }
    }
}
