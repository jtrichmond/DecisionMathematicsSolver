using System;
using System.Collections.Generic;
using Exceptions_Library;

namespace Sorting_Library
{
    public enum DataType
    {
        Character,
        String,
        Number
    }
    // Determines what data type the sort items should have. Type of ISortItem determined by SortingArray
    //In general, number will mean double

    //all setter methods booleans so that they return false if assignment fails.
    public interface ISortingArray<T> where T : IComparable
    {
        DataType GetDataType(); // DataType managed at this level
        ISortItem<T>[] GetArray();
        ISortItem<T> GetItem(int index);
        bool SetItem(int index, ISortItem<T> item);
        ISortingArray<T> Concatenate(ISortingArray<T> array); //merging 2 arrays into a new one
        ISortingArray<T> Append(ISortItem<T> item);
        ISortingArray<T> QuickSort(ISortingArray<T> array, bool ascending); //functions to allow recursion
        ISortingArray<T> BubbleSort(ISortingArray<T> array, bool ascending);
        string GetResultsString();
    }

    public interface ISortItem<T>
        where T : IComparable
    // means that only one array interface is needed
    // rather than specific ones for named and unnamed items
    {
        T GetValue();
        bool SetValue(T value);
        bool IsNamed();
        string GetName();
        bool IsGreaterThan(ISortItem<T> item);
    }

    public abstract class SortItem<T> : ISortItem<T>
        where T : IComparable 
    {
        protected T Value;

        //constructor
        protected SortItem(T value) //constructor accessible within subtypes
        {
            string errorMessage = "";
            if (this.SetValue(value) == false)
            {
                errorMessage = errorMessage + "Bad Value: " + value + "\n";
            }

            if (errorMessage != "")
            {
                throw new SortItemConstructionException("SortItem construction failed. \n" + errorMessage);
            }
        }

        // functional methods

        public virtual T GetValue()
        {
            return this.Value;
        }

        public virtual bool SetValue(T value)
        {
            if (value != null)
            {
                this.Value = value;
                return true;
            }
            else
            {
                return false;
            }
        }

        public abstract bool IsNamed();
        public abstract string GetName();

        public virtual bool IsGreaterThan(ISortItem<T> item) // for comparisons, abstracting data type
        {
            switch (this.GetValue())
            {
                case string:
                case char:
                    StringComparer comparer = StringComparer.InvariantCultureIgnoreCase;
                    // ignores capitals
                    if (comparer.Compare(this.GetValue(), item.GetValue()) <= 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                case double: //can add other number types here if needed
                case int: //int
                case float: //float
                case long: //long
                case decimal:
                    if (this.GetValue().CompareTo(item.GetValue()) <= 0)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                default:
                    throw new DataTypeException("DataType not recognised for comparison");
                //Datatype deliberately constricted to prevent use outside of that designed for
            }
        }
    }

    public sealed class UnnamedItem<T> : SortItem<T>
        where T : IComparable // single data item, type checked in array not in item
    {
        //Constructor
        public UnnamedItem(T value) : base(value)
        {
            //uses parent constructor
        }

        //debugging methods
        public override bool Equals(object obj)
        {
            if (obj is null or not UnnamedItem<T>)
            {
                return false;
            }

            UnnamedItem<T> trial = (UnnamedItem<T>) obj;
            if (this.Value.Equals(trial.GetValue()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return "Value: " + this.Value.ToString();
        }

        //functional methods
        public override bool IsNamed()
        {
            return false; //never named
        }

        public override string GetName()
        {
            return "";
        }
    }


    public sealed class NamedItem<T> : SortItem<T> where T : IComparable // data item with name associated
    {
        private string _name;

        //Constructor
        public NamedItem(string name, T value) : base(value)
        {
            string errorMessage = "";
            if (this.SetName(name) == false)
            {
                errorMessage += "Bad name: " + name + "\n";
            }

            if (errorMessage != "")
            {
                throw new SortItemConstructionException("NamedItem construction failed. \n" + errorMessage);
            }
        }

        //debugging methods
        public override string ToString()
        {
            string returnString = ("Name: " + this._name + " Value: " + this.Value.ToString());
            return returnString;
        }

        public override bool Equals(object obj)
        {
            if (obj is null or not NamedItem<T>)
            {
                return false;
            }

            NamedItem<T> trial = (NamedItem<T>) obj;
            return (this._name == trial.GetName()) && (this.Value.Equals(trial.GetValue()));
        }

        //functional methods

        private bool SetName(string name) // name should not be changed after instantiation
        {
            if (name != null)
            {
                this._name = name;
                return true;
            }
            else
            {
                return false;
            }
        }

        public override bool IsNamed()
        {
            return true;
        }

        public override string GetName()
        {
            return _name;
        }
    }

    public class SortingArray<T> : ISortingArray<T> where T : IComparable
    {
        private ISortItem<T>[] _array; 
        private DataType _dataType;

        // Constructors

        private SortingArray(ISortItem<T>[] array, DataType dataType) //constructor for when list pre-made
        {
            string errorMessage = "";
            if (this.SetArray(array) == false)
            {
                errorMessage = errorMessage + "Bad input array: " + array + "\n";
            }

            if (this.SetDataType(dataType) == false)
            {
                errorMessage = errorMessage + "Bad data type: " + dataType + "\n";
            }

            if (errorMessage != "")
            {
                throw new SortingArrayConstructionException("SortingArray construction failed \n" + errorMessage);
            }
        }

        private SortingArray(List<ISortItem<T>> list, DataType dataType) : this(list.ToArray(), dataType)
        {
        } //for quick sort

        //these may not work if the dynamic array includes ISortItem, as the normal constructor may not work
        public SortingArray(dynamic[] rawArray, DataType dataType) : 
            this(new ISortItem<T>[rawArray.GetLength(0)], dataType)
        {
            string errorMessage = "";
            for (int i = 0; i < rawArray.GetLength(0); i++)
            {
                try
                {
                    ISortItem<T> item = new UnnamedItem<T>(rawArray[i]);
                    if (this.SetItem(i, item) == false)
                    {
                        errorMessage = errorMessage + $"Bad value: {rawArray[i]}" + "\n";
                    }
                }
                catch (SortItemConstructionException e)
                {
                    errorMessage += e.Message;
                }
            }

            if (errorMessage != "")
            {
                throw new SortingArrayConstructionException("SortingArray construction failed:" + errorMessage);
            }
        }

        public SortingArray(string[] rawNameArray, dynamic[] rawValueArray, DataType dataType) : this(
            new ISortItem<T>[rawNameArray.GetLength(0)], dataType)
        {
            string errorMessage = "";
            if (rawNameArray.GetLength(0) != rawValueArray.GetLength(0))
            {
                errorMessage += "Array lengths do not match";
            }
            else
            {
                for (int i = 0; i < rawNameArray.GetLength(0); i++)
                {
                    try
                    {
                        NamedItem<T> item = new NamedItem<T>(rawNameArray[i], rawValueArray[i]);
                        if (this.SetItem(i, item) == false)
                            errorMessage += $"Bad pair: {rawNameArray[i]}, {rawValueArray[i]}";
                    }
                    catch (SortItemConstructionException e)
                    {
                        errorMessage += e.Message;
                    }
                }
            }

            if (errorMessage != "")
            {
                throw new SortingArrayConstructionException("SortingArray construction failed \n" + errorMessage);
            }
        }

        //debugging methods
        public override string ToString()
        {
            string dataTypeString;

            switch (this._dataType)
            {
                case DataType.Character:
                    dataTypeString = "Character";
                    break;
                case DataType.String:
                    dataTypeString = "String";
                    break;
                case DataType.Number:
                    dataTypeString = "Number";
                    break;
                default:
                    throw new DataTypeException("No data type");
            }

            string[] stringArray = new string[this.GetArray().GetLength(0)];
            int i = 0;
            foreach (var item in this.GetArray())
            {
                stringArray[i] = item.ToString();
                i += 1;
            }

            string concatenatedArray = string.Join("; ", stringArray);
            return ("Array(" + concatenatedArray + ") DataType: " + dataTypeString);
        }

        public override bool Equals(object obj)
        {
            if (obj is null or not SortingArray<T>)
            {
                return false;
            }

            SortingArray<T> trial = (SortingArray<T>) obj;
            if ((this._array == trial.GetArray()) && (this._dataType == trial.GetDataType()))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        //functional methods
        public string GetResultsString()
        {
            string[] stringArray = new string[this.GetArray().GetLength(0)];
            int i = 0;
            foreach (var item in this.GetArray())
            {
                stringArray[i] = "";
                if (item.IsNamed())
                    stringArray[i] += item.GetName() + ": ";
                stringArray[i] += item.GetValue().ToString();
                i += 1;
            }

            string concatenatedArray = string.Join(", ", stringArray);
            return concatenatedArray;
        }

        public DataType GetDataType()
        {
            return this._dataType;
        }

        public ISortItem<T>[] GetArray()
        {
            return this._array;
        }

        public ISortItem<T> GetItem(int index)
        {
            return this._array[index];
        }

        public bool SetItem(int index, ISortItem<T> item) //index from 0
        {
            bool valid = false;
            Type valueType = item.GetValue().GetType();
            switch (this._dataType)
            {
                case DataType.Character:
                    if (valueType == typeof(char))
                        valid = true;
                    break;
                case DataType.String:
                    if (valueType == typeof(string))
                        valid = true;
                    break;
                case DataType.Number:
                    if ((valueType == typeof(int)) || (valueType == typeof(double)))
                        valid = true;
                    break;
                default:
                    throw new DataTypeException("dataType not recognised");
            }

            if ((valid == false) || (index >= this._array.GetLength(0)) || (index < 0))
                return false; // may need to replace with exceptions detailing different errors
            this._array[index] = item;
            return true;
        }

        private bool SetArray(ISortItem<T>[] array)
            // should only be called by constructors
        {
            if (array != null)
                //don't need to check size as wont be called by anything other than constructor
            {
                this._array = array;
                return true;
            }
            else
            {
                return false;
            }
        }

        private bool SetDataType(DataType type)
        {
            switch (type)
            {
                case DataType.Character:
                case DataType.String:
                case DataType.Number:
                    this._dataType = type;
                    return true;
                default:
                    return false;
            }
        }

        public ISortingArray<T> Concatenate(ISortingArray<T> array)
        {
            if (this.GetDataType() != array.GetDataType())
            {
                throw new SortingArrayCombiningException("Tried to concatenate arrays of different types");
            }

            ISortItem<T>[] itemArray = new ISortItem<T>[this.GetArray().GetLength(0) + array.GetArray().GetLength(0)];
            this.GetArray().CopyTo(itemArray, 0);
            array.GetArray().CopyTo(itemArray, this.GetArray().GetLength(0));
            //copies the two arrays into one new array
            ISortingArray<T> returnArray = new SortingArray<T>(itemArray, array.GetDataType());
            return returnArray;
        }

        public ISortingArray<T> Append(ISortItem<T> item)
        {
            ISortItem<T>[] itemArray = new ISortItem<T>[this.GetArray().GetLength(0) + 1];
            this.GetArray().CopyTo(itemArray, 0);
            ISortingArray<T> returnArray = new SortingArray<T>(itemArray, this.GetDataType());
            if (returnArray.SetItem(returnArray.GetArray().GetLength(0) - 1, item) == false)
            {
                throw new SortingArrayCombiningException("Append failed: item was not of correct data type");
            }

            return returnArray;
        }

        public ISortingArray<T> BubbleSort(ISortingArray<T> array, bool ascending)
        {
            int arrayLength = array.GetArray().GetLength(0);
            bool swapped = true;
            int i = 0;
            while ((swapped) && (i < arrayLength))
            {
                swapped = false;
                for (int j = 0; j <= arrayLength - i - 2; j++)
                {
                    if (!(array.GetItem(j).IsGreaterThan(array.GetItem(j + 1)) ^
                          ascending))
                        //will switch if both are true or both are false
                    {
                        ISortItem<T> temporaryStore = array.GetItem(j);
                        if (array.SetItem(j, array.GetItem(j + 1)) == false)
                        {
                            throw new SortException($"Failed to swap items {temporaryStore}, {array.GetItem(j + 1)}");
                        }

                        if (array.SetItem(j + 1, temporaryStore) == false)
                        {
                            throw new SortException($"Failed to swap items {temporaryStore}, {array.GetItem(j + 1)}");
                        }

                        swapped = true;
                    }
                }

                i += 1;
            }

            return array;
        }

        public ISortingArray<T> QuickSort(ISortingArray<T> array, bool ascending)
        {
            if (array.GetArray().GetLength(0) == 1) //base case
            {
                return array;
            }

            double arrayLength = array.GetArray().GetLength(0);
            int pivotIndex = (int) Math.Round(arrayLength / 2, 0, MidpointRounding.AwayFromZero) - 1;
            //-1 because indexed from 0
            ISortItem<T> pivot = array.GetItem(pivotIndex);
            List<ISortItem<T>> lessThanItems = new();
            List<ISortItem<T>> greaterThanItems = new();
            bool itemsInLessThan = false; //flag for items added to lessThanItems
            bool itemsInGreaterThan = false; //flag for items added to greaterThanItems

            for (int i = 0; i < array.GetArray().GetLength(0); i++)
                // cannot use foreach and .Equals for identifying pivot as overriden, may have duplicate values. 
            {
                if (i == pivotIndex)
                {
                    continue; //item should not be put in either list
                }

                if (array.GetItem(i).IsGreaterThan(pivot))
                {
                    greaterThanItems.Add(array.GetItem(i));
                    itemsInGreaterThan = true;
                }
                else
                {
                    lessThanItems.Add(array.GetItem(i));
                    itemsInLessThan = true;
                }
            }

            ISortingArray<T> lessThanArray = new SortingArray<T>(lessThanItems, array.GetDataType());
            ISortingArray<T> greaterThanArray = new SortingArray<T>(greaterThanItems, array.GetDataType());
            if (itemsInLessThan)
            {
                lessThanArray = lessThanArray.QuickSort(lessThanArray, ascending); // recursive call
            }

            if (itemsInGreaterThan)
            {
                greaterThanArray = greaterThanArray.QuickSort(greaterThanArray, ascending); //recursive call
            }

            ISortingArray<T> returnArray;
            if (ascending)
            {
                returnArray = new SortingArray<T>(lessThanArray.GetArray(), lessThanArray.GetDataType());
                returnArray = returnArray.Append(pivot);
                returnArray = returnArray.Concatenate(greaterThanArray);
            }
            else
            {
                returnArray = new SortingArray<T>(greaterThanArray.GetArray(), greaterThanArray.GetDataType());
                returnArray = returnArray.Append(pivot);
                returnArray = returnArray.Concatenate(lessThanArray);
            }
            //combines lists

            return returnArray;
        }
    }

    public static class SortFactory
    {
        public static string[] SplitItems(string rawString, char itemSeparator)
        {
            if (!rawString.Contains(itemSeparator))
                throw new SplitException("No items to split");
            //cannot sort only one item so one item lists from user not acceptable
            return rawString.Split(itemSeparator);
        }

        public static dynamic[] ParseArrayToType(string[] rawValueArray, DataType dataType)
        {
            dynamic[] returnArray = new dynamic[rawValueArray.GetLength(0)];
            for (int i = 0; i < rawValueArray.GetLength(0); i++)
            {
                bool successfulParseFlag = false;

                switch (dataType)
                {
                    case DataType.Character:
                        if (char.TryParse(rawValueArray[i], out char character))
                        {
                            successfulParseFlag = true;
                            returnArray[i] = character;
                        }

                        break;
                    case DataType.Number:
                        if (double.TryParse(rawValueArray[i], out double number))
                        {
                            successfulParseFlag = true;
                            returnArray[i] = number;
                        }

                        break;

                    case DataType.String:
                        return (dynamic) rawValueArray;

                    default:
                        throw new DataTypeException("Data type not recognised");
                }

                if (!successfulParseFlag)
                {
                    throw new ParseException($"Object {returnArray[i]} could not be parsed to {dataType}");
                }
            }

            return returnArray;
        }

        public static (string[], string[]) SplitNamesAndValues(string[] rawStringArray, char nameSeparator)
        {
            string[] nameArray = new string[rawStringArray.GetLength(0)];
            string[] rawValueArray = new string[rawStringArray.GetLength(0)];

            for (int i = 0; i < rawStringArray.GetLength(0); i++)
            {
                if (!rawStringArray[i].Contains(nameSeparator))
                    throw new SplitException("No name/value split");
                string[] pairArray = rawStringArray[i].Split(nameSeparator);
                nameArray[i] = pairArray[0];
                rawValueArray[i] = pairArray[1];
            }

            return (nameArray, rawValueArray);
        }

        public static dynamic MakeNamedArray(string[] nameArray, dynamic[] valueArray, DataType dataType) 
            //different from constructor as needs to assign generic type too
        {
            switch (dataType)
            {
                case DataType.Character:
                    return new SortingArray<char>(nameArray, valueArray, dataType);
                case DataType.String:
                    return new SortingArray<string>(nameArray, valueArray, dataType);
                case DataType.Number:
                    return new SortingArray<double>(nameArray, valueArray, dataType);
                default:
                    throw new DataTypeException("DataType not recognised");
            }
        }

        public static dynamic MakeUnnamedArray(dynamic[] valueArray, DataType dataType)
        {
            //different constructor call than for named arrays
            switch (dataType)
            {
                case DataType.Character:
                    return new SortingArray<char>(valueArray, dataType);
                case DataType.String:
                    return new SortingArray<string>(valueArray, dataType);
                case DataType.Number:
                    return new SortingArray<double>(valueArray, dataType);
                default:
                    throw new DataTypeException("DataType not recognised");
            }
        }
    }
}