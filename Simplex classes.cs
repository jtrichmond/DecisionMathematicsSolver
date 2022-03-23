using Exceptions_Library;
using System.Diagnostics;

//currently only works for basic simplex, big-M would require a new structure and two-stage requires separate method
namespace Simplex_Library
{
    public enum RowType 
    {
        Variable,
        PrimaryObjective,
        SecondaryObjective
    }

    public enum SimplexMethod
        //for selecting method to use
    {
        Basic,
        TwoStage,
        BigM
    }

    public interface ITableau
    {
        ITableau BasicSimplex();
        string GetValuesMessage();
    }


    internal class SimplexRow
    {
        private string _basicVariable;
        private Dictionary<string, double> _rowData;
        private double _rowValue;
        internal RowType RowType { get; private set; }

        public SimplexRow(string basicVariable, Dictionary<string, double> rowData, double rowValue, RowType rowType)
        {
            string errorMessage = "";
            _rowData = rowData;
            SetRowValue(rowValue);
            this.RowType = rowType;
            if (!this.SetBasicVariable(basicVariable))
            {
                errorMessage += $"Failed to set basic variable as {basicVariable} \n";
            }
            if (errorMessage != "")
            {
                throw new SimplexRowConstructionException(errorMessage);
            }
        }

        public SimplexRow(string basicVariable, string[] variableNames, 
            double[] variableValues, double rowValue, RowType rowType)
        {
            //dictionary instantiated in constructor so that class is composed of the dictionary
            this._rowData = new Dictionary<string, double>();
            string errorMessage = "";

            if (variableNames.GetLength(0) != variableValues.GetLength(0))
                errorMessage += $"Variable arrays are not of the same length: " +
                    $"names array has {variableNames.GetLength(0)} items," +
                    $" values array has {variableValues.GetLength(0)}.";
            
            //error thrown before dictionary construction so that custom error message shown only,
            //rather than errors thrown during construction as well.
            if (errorMessage != "")
                throw new SimplexRowConstructionException("SimplexRow Construction failed: \n " + errorMessage);

            for (int i = 0; i < variableNames.GetLength(0); i++)
            {
                this.SetColumnValue(variableNames[i], variableValues[i]);
            }
            
            this.SetRowValue(rowValue); //no checks needed, argument cannot be null and all values accepted
                                        //(infinities cannot be entered by user)
            this.RowType = rowType; //no checks needed, cannot be null or a wrong value as enum

            if (!this.SetBasicVariable(basicVariable))
                errorMessage += $"Failed to set the basic variable as {basicVariable}";
            
            
            if (errorMessage != "")
                throw new SimplexRowConstructionException("SimplexRow Construction failed: \n " + errorMessage);
        }
        
        //debugging methods
        public override bool Equals(object? obj)
        {
            if (obj is null or not SimplexRow)
                return false;

            SimplexRow trial = (SimplexRow) obj;

            if (this.GetBasicVariable() != trial.GetBasicVariable())
                return false;

            string[] thisVariables = this.GetVariables();
            string[] trialVariables = trial.GetVariables();

            if (thisVariables.GetLength(0) != trialVariables.GetLength(0))
                return false;

            foreach (string variable in thisVariables)
            {
                if (!trialVariables.Contains(variable))
                    return false;

                if (this.GetColumnValue(variable) - trial.GetColumnValue(variable) != 0)
                    return false;
            }

            if (this.GetRowValue() - trial.GetRowValue() != 0)
                return false;

            return true;
        }

        public override string ToString()
        {
            string returnString = "";

            returnString += "Basic Variable: " + _basicVariable + "\n";

            foreach (var keyValuePair in this._rowData)
                returnString += $"Column Variable: {keyValuePair.Key} Value: {keyValuePair.Value} \n";

            returnString += $"RowValue {this._rowValue} ";
            returnString += $"RowType {this.RowType}";
            return returnString;
        }
        
        //functional methods

        internal bool SetBasicVariable(string basicVariableName)
        {
            if (basicVariableName == this._basicVariable) //basic variable should be changed by method
                return false;
            if (!this._rowData.ContainsKey(basicVariableName) && 
                (this.RowType is not RowType.PrimaryObjective or RowType.SecondaryObjective))
                return false; //basic variable is not one of the variables in the dictionary
                              //and the row is not an objective function, so is invalid

            this._basicVariable = basicVariableName;
            return true;
        }

        internal void SetColumnValue(string variable, double value)
        {
            this._rowData[variable] = value; 
        }

        internal void SetRowValue(double value) 
        {
            this._rowValue = value;
        }

        internal double CalculateTheta(string variable)
        {
            double columnValue = this._rowData[variable];
            if (columnValue == 0)
                return double.PositiveInfinity; //would be a divide by 0

            return this._rowValue / columnValue;
        }
        
        internal bool ContainsNegativeValues()
        {
            foreach (double value in this._rowData.Values)
            {
                if (value < 0)
                    return true;
            }

            return false;
        }

        internal string GetMostNegativeEntry()
        {
            string mostNegativeVariable = "";
            double mostNegativeValue = 0;

            foreach (var keyValuePair in this._rowData)
            {
                if (keyValuePair.Value < mostNegativeValue)
                {
                    mostNegativeVariable = keyValuePair.Key;
                    mostNegativeValue = keyValuePair.Value;
                }
                    
            }

            return mostNegativeVariable; //only called when there are negative values, inside the simplex method,
                                         //therefore will not return empty string.
        }

        internal string GetBasicVariable()
        {
            return this._basicVariable;
        }

        internal double GetColumnValue(string variable)
        {
            return this._rowData[variable]; //will throw exception if variable not in row
            //this should not be handled, as this method can only be called inside simplex algorithms
            //If it is thrown, a correction should be made
        }

        internal double GetRowValue()
        {
            return this._rowValue;
        }

        internal string[] GetVariables()
        {
            return this._rowData.Keys.ToArray();
            //returns all variable names in the dictionary
        }
    }

    public class SimplexTableau : ITableau
    {
        private SimplexRow[] TableauData { get; set; }
        private int NumberOfRows => this.TableauData.GetLength(0);
        //property uses function to return number of items in private attribute
        
        //Constructors
        public SimplexTableau(ArraySimplexRowInput[] inputRows)
        {
            string errorMessage = "";
            TableauData = new SimplexRow[inputRows.GetLength(0)];
            for (int i = 0; i < inputRows.GetLength(0); i++)
            {
                bool validDictFlag = true;
                Dictionary<string, double> columnValues = new Dictionary<string, double>();
                for (int j = 0; j < inputRows[i].ColumnValues.GetLength(0) -1; j++) 
                    // -1 because last element reserved for row value
                {
                    if (double.TryParse(inputRows[i].ColumnValues[j], out double value))
                    {
                        columnValues.Add(inputRows[i].ColumnNames[j], value);
                    }
                    else
                    {
                        errorMessage += $"{inputRows[i].ColumnValues[j]} is not a number, " +
                            $"so cannot be set as the entry for {inputRows[i].ColumnNames[j]} " +
                            $"in the {inputRows[i].BasicVariable} row \n";
                        validDictFlag = false;
                    }
                }
                if (double.TryParse(inputRows[i].ColumnValues[^1], out double rowValue) && validDictFlag)
                {
                    try
                    {
                        TableauData[i] = new SimplexRow(inputRows[i].BasicVariable, columnValues, rowValue, inputRows[i].RowType);
                    }
                    catch (SimplexRowConstructionException e)
                    {
                        errorMessage += e.Message + "\n";
                    }
                }
                else if (validDictFlag)
                {
                    errorMessage += $"{inputRows[i].ColumnValues[^1]} is not a number, " +
                        $"so cannot be set as the row value for the {inputRows[i].BasicVariable} row\n";
                }
                //^1 is the last element, as ^ allows indexing from the back of an array
            }

            if (errorMessage != "")
                throw new SimplexTableauConstructionException("Simplex tableau construction failed: " + errorMessage);
        }

        public SimplexTableau(string[] basicVariables, string[] variableNames, 
            double[,] rowDataArray, double[] rowValues, RowType[] rowTypes)
        {
            //basicVariables should include objective function(s), variableNames should not
            //This constructor could have been used if a network needed to be constructed without
            //ArraySimplexRowInputs
            string errorMessage = "";
            int[] rowDataDimensions = {rowDataArray.GetLength(0), rowDataArray.GetLength(1)}; 
            //to avoid repeated .GetLength calls
            
            if (variableNames.GetLength(0) != rowDataDimensions[1])
                errorMessage += $"Number of variables, {variableNames.GetLength(0)}, " +
                    $"does not match the number of columns in the data table, {rowDataDimensions[1]} \n";

            if (basicVariables.GetLength(0) != rowDataDimensions[0])
                errorMessage += $"Number of basic variables, {basicVariables.GetLength(0)}, " +
                    $"differs from number of rows, {rowDataDimensions[0]} \n";

            if (rowValues.GetLength(0) != rowDataDimensions[0])
                errorMessage += $"Number of row values, {rowValues.GetLength(0)}, " +
                    $"does not equal the number of rows, {rowDataDimensions[0]} \n";

            if (rowTypes.GetLength(0) != rowDataDimensions[0])
                errorMessage += $"Number of row types, {rowTypes.GetLength(0)}, " +
                    $"does not match the number of rows, {rowDataDimensions[0]} \n";

            uint primaryRowCounter = 0;
            uint secondaryRowCounter = 0;
            foreach (RowType rowType in rowTypes)
            {
                if (rowType == RowType.PrimaryObjective)
                    primaryRowCounter += 1;
                else if (rowType == RowType.SecondaryObjective)
                    secondaryRowCounter += 1;
            }

            if (primaryRowCounter != 1)
                errorMessage += $"Incorrect number of objective functions: {primaryRowCounter} \n";

            if (secondaryRowCounter > 1)
                errorMessage += $"Too many secondary objective functions: {secondaryRowCounter} \n";
                //Allows 0 or 1 secondary objective rows, thereby allowing all methods

            for (int i = 0; i < basicVariables.GetLength(0); i++)
            {
                if (!(variableNames.Contains(basicVariables[i]) || 
                    rowTypes[i] is RowType.PrimaryObjective or RowType.SecondaryObjective))
                    errorMessage += $"String {basicVariables[i]} is not a variable given, " +
                        $"so cannot be a basic variable for a variable row \n";
            }

            this.TableauData = new SimplexRow[rowDataDimensions[0]];

            for (int i = 0; i < rowDataDimensions[0]; i++)
            {
                double[] singleRowArray = Enumerable.Range(0, rowDataDimensions[1]).Select(x => rowDataArray[i, x]).ToArray();
                //produces a 1D array from a row of a 2D array
                try
                {
                    this.TableauData[i] = new SimplexRow(basicVariables[i], variableNames, 
                        singleRowArray, rowValues[i], rowTypes[i]);
                }
                catch (SimplexRowConstructionException e)
                {
                    errorMessage += e.Message + "\n";
                }
            }
            
            if (errorMessage != "")
                throw new SimplexTableauConstructionException("SimplexTableau construction failed: \n" + errorMessage);
        }
        
        //debugging methods
        public override bool Equals(object? obj)
        {
            if (obj is null or not SimplexTableau)
                return false;

            SimplexTableau trial = (SimplexTableau) obj;

            if (this.NumberOfRows != trial.NumberOfRows)
                return false;

            for (uint i = 0; i < this.NumberOfRows; i++)
            {
                if (!this.TableauData[i].Equals(trial.TableauData[i]))
                    return false;
            }

            return true;
        }

        public override string ToString()
        {
            string returnString = "";

            foreach (SimplexRow row in this.TableauData)
                returnString += "Row: " + row + "\n";

            return returnString;
        }
        
        //functional methods

        private bool SumRow(int pivotRowIndex, int subjectRowIndex, double multiplier)
        {
            //sums the values in the pivot row, multiplied by the multiplier
            //to the values in the subject row
            if (pivotRowIndex < 0 || subjectRowIndex < 0)
                return false;
            string[] variables = this.TableauData[pivotRowIndex].GetVariables();

            foreach (string variable in variables)
            {
                double newColumnValue = this.TableauData[pivotRowIndex].GetColumnValue(variable) * multiplier 
                    + TableauData[subjectRowIndex].GetColumnValue(variable);
                TableauData[subjectRowIndex].SetColumnValue(variable, newColumnValue);
            }

            TableauData[subjectRowIndex].SetRowValue(TableauData[subjectRowIndex].GetRowValue() 
                + TableauData[pivotRowIndex].GetRowValue() * multiplier);
            return true;
        }

        private bool DivideRow(int subjectRowIndex, double divider)
        {
            if (subjectRowIndex < 0)
                return false;
            SimplexRow row = TableauData[subjectRowIndex];
            string[] variables = row.GetVariables();

            foreach (string variable in variables)
            {
                double newColumnValue = row.GetColumnValue(variable) / divider;
                row.SetColumnValue(variable, newColumnValue);
            }

            double newRowValue = row.GetRowValue() / divider;
            TableauData[subjectRowIndex].SetRowValue(newRowValue);

            return true;
        }
        

        public ITableau BasicSimplex() //changes current object
        {
            //quotations from Susie, et al. (2017)
            SimplexRow? objectiveRow = null;
            //check that there are no SecondaryObjective rows
            foreach (SimplexRow row in TableauData)
            {
                if (row.RowType == RowType.PrimaryObjective)
                    objectiveRow = row;

                if (row.RowType == RowType.SecondaryObjective)
                    throw new InvalidSimplexMethodException(
                        "Tableau contains secondary objective functions, which are usable only in Two Stage Simplex");
            }

            if (objectiveRow is null)
                throw new NoObjectiveRowException("No objective row.");
            Trace.WriteLine($"Objective Row is {objectiveRow}");

            while (objectiveRow.ContainsNegativeValues())
            {
                Trace.WriteLine($"Tableau is {this}");
                string pivotColumn = objectiveRow.GetMostNegativeEntry(); 
                //'look along the objective row for the most negative entry: this indicates the pivot column'
                Trace.WriteLine($"Pivot variable is {pivotColumn}");
                double leastPositiveTheta = double.PositiveInfinity;
                //so that all finite values are less than it
                int pivotRow = -1; //default value
                for (int i = 0; i < this.NumberOfRows; i++)
                {
                    if (this.TableauData[i].RowType != RowType.Variable)
                        continue;
                    //skips objective rows
                    
                    double rowTheta = this.TableauData[i].CalculateTheta(pivotColumn); 
                    //'Calculate the theta values, for each of the constraint rows...'
                    if (rowTheta < leastPositiveTheta && rowTheta > 0)
                    {
                        leastPositiveTheta = rowTheta; 
                        pivotRow = i;
                    } //'select the row with the smallest, positive theta to become the pivot row'
                }

                if (pivotRow == -1)
                    throw new NoPositiveThetaException("No row had a finite, positive theta value");
                
                Trace.WriteLine($"pivotRow is {pivotRow}");

                double pivot = this.TableauData[pivotRow].GetColumnValue(pivotColumn); 
                //'The element in the pivot row and pivot column is the pivot'
                Trace.WriteLine($"Pivot Value is {pivot}");

                if (!this.DivideRow(pivotRow, pivot))
                    throw new RowDivisionException($"Failed to divide row {TableauData[pivotRow]} by {pivot}");
                
                if (!this.TableauData[pivotRow].SetBasicVariable(pivotColumn))
                    throw new BasicVariableException(
                        $"Could not set basic variable of row {this.TableauData[pivotRow]} as {pivotColumn}");
                //'Divide the row found in step 5 [pivot row] by the pivot,
                //and change the basic variable at the start of the column to the variable at the top of the pivot column.
                //This is now the pivot row'

                for (int i = 0; i < this.NumberOfRows; i++)
                {
                    if (i == pivotRow)
                        continue;
                    double multiple = TableauData[i].GetColumnValue(pivotColumn) *-1;
                    if (!SumRow(pivotRow, i, multiple))
                        throw new RowSummationException(
                            $"Failed to subtract {multiple} lots of row {TableauData[pivotRow]} from row {TableauData[i]}");
                }
                //'Use the pivot row to eliminate the pivot's variable from the other rows.
                //This means that the pivot column now contains one 1 and zeroes
            }

            return this; //returns the amended tableau.
        }

        public string GetValuesMessage()
        {
            SimplexRow? objectiveRow = null;
            string[] basicVariables = new string[NumberOfRows];
            int i = 0;
            foreach (SimplexRow row in TableauData)
            {
                if (row.RowType == RowType.PrimaryObjective)
                    objectiveRow = row;
                basicVariables[i] = row.GetBasicVariable();
                i++;
            }
            
            if (objectiveRow is null)
                throw new NoObjectiveRowException("No objective row.");


            string returnString = $"Objective function {objectiveRow.GetBasicVariable()} " +
                $"has value {objectiveRow.GetRowValue()} when: ";
            
            foreach (string variable in objectiveRow.GetVariables())
            {
                if (!basicVariables.Contains(variable))
                    returnString += $"{variable} is 0";
                else
                {
                    IEnumerable<SimplexRow> variableRowEnum =
                        from row in TableauData
                        where row.GetBasicVariable() == variable
                        select row;
                    //selects rows with the same basic variable

                    SimplexRow[] variableRowArray = variableRowEnum.ToArray();
                    if (variableRowArray.GetLength(0) == 1)
                        returnString += $"{variable} is {variableRowArray[0].GetRowValue()}";
                    else
                        throw new BasicVariableException("Multiple rows with the same basic variable");
                }
                returnString += " / ";
            }

            return returnString;
        }
    }

    public struct ArraySimplexRowInput
    {
        public string BasicVariable { get; init; }
        public string[] ColumnNames { get; init; }
        public string[] ColumnValues { get; set; }
        public RowType RowType { get; init; }

        public override string ToString()
        {
            string returnString = $"BasicVariable = {BasicVariable} \n";
            for (int i = 0; i < ColumnNames.GetLength(0); i++)
            {
                returnString += ColumnNames[i] + " = " + ColumnValues[i] + "\n";
            }
            returnString += $"RowType is {RowType}";
            return returnString;
        }
    }

    public static class SimplexFactory
    {
        public static bool TrySplitNames(string names, char splitCharacter, out string[] namesArray)
        {
            if (names.Contains(splitCharacter) is false)
            {
                namesArray = Array.Empty<string>(); 
                return false;
            }
            else
            {
                namesArray = names.Split(splitCharacter);
                return true;
            }
        }

        public static List<ArraySimplexRowInput> MakeEmptyInputListFromArrays
            (string[] variables, string[] rowVariables, RowType[] rowTypes)
            //objective variables should be included in row variables
        {
            if (rowVariables.GetLength(0) != rowTypes.GetLength(0))
            {
                throw new IndexOutOfRangeException($"There are {rowVariables.GetLength(0)} Basic Variables, " +
                    $"and {rowTypes.GetLength(0)} Row Types.");
            }
            List<ArraySimplexRowInput> returnList = new();

            for (int i = 0; i < rowVariables.GetLength(0); i++)
            {
                ArraySimplexRowInput newRow = new ArraySimplexRowInput()
                {
                    BasicVariable = rowVariables[i],
                    ColumnNames = new string[variables.GetLength(0) +1],
                    ColumnValues = new string[variables.GetLength(0) +1], //+1 for rowValue
                    RowType = rowTypes[i]
                };
                variables.CopyTo(newRow.ColumnNames, 0);
                for (int j = 0; j < variables.GetLength(0); j++)
                {
                    if (newRow.BasicVariable == newRow.ColumnNames[j])
                    {
                        newRow.ColumnValues[j] = "1";
                    }
                    else
                        newRow.ColumnValues[j] = "0";
                }

                newRow.ColumnNames[variables.GetLength(0)] = "Value"; //will be last element
                newRow.ColumnValues[variables.GetLength(0)] = "0"; 
                returnList.Add(newRow);
            }
            return returnList;
        }
    }
}
