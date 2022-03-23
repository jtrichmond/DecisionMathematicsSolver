using System;
namespace Exceptions_Library
{
    //Library of all exceptions used in the project
    //Comments show project Exception was originally for
    //Constructors overridden to allow provision of custom error message

    //Sorting Library
    public class SortItemConstructionException : Exception
    {
        public SortItemConstructionException(string message) : base(message)
        {
        }
    }

    public class DataTypeException : Exception
    {
        public DataTypeException(string message) : base(message)
        {
        }
    }

    public class SortingArrayConstructionException : Exception
    {
        public SortingArrayConstructionException(string message) : base(message)
        {
        }
    }

    public class SortingArrayCombiningException : Exception
    {
        public SortingArrayCombiningException(string message) : base(message)
        {
        }
    }

    public class SortException : Exception
    {
        public SortException(string message) : base(message)
        {
        }
    }

    public class SplitException : Exception
    {
        public SplitException(string message) : base(message)
        {
        }
    }

    public class ParseException : Exception
    {
        public ParseException(string message) : base(message)
        {
        }
    }

    //Graph classes 
    public class NetworkConstructionException : Exception
    {
        public NetworkConstructionException(string message) : base(message)
        { }
    }

    public class NetworkConversionException : Exception
    {
        public NetworkConversionException(string message) : base(message)
        { }
    }



    //Simplex classes
    public class SimplexRowConstructionException : Exception
    {
        public SimplexRowConstructionException(string message) : base(message)
        { }
    }

    public class SimplexTableauConstructionException : Exception
    {
        public SimplexTableauConstructionException(string message) : base(message)
        { }
    }

    public class InvalidSimplexMethodException : Exception
    {
        public InvalidSimplexMethodException(string message) : base(message)
        { }
    }

    public class NoObjectiveRowException : Exception
    {
        public NoObjectiveRowException(string message) : base(message)
        { } //unlikely to be thrown as constructor requires an objective row, but used in Simplex regardless
    }

    public class NoPositiveThetaException : Exception
    {
        public NoPositiveThetaException(string message) : base(message)
        { }
    }

    public class BasicVariableException : Exception
    {
        public BasicVariableException(string message) : base(message)
        { }
    }

    public class RowDivisionException : Exception
    {
        public RowDivisionException(string message) : base(message)
        { }
    }

    public class RowSummationException : Exception
    {
        public RowSummationException(string message) : base(message)
        { }
    }
    
    //Window menu logic
    public class NotAnOptionException : Exception
    {
        public override string Message => "Option chosen was not a recognised option";

        public NotAnOptionException() : base()
        {}
        public NotAnOptionException(string message) : base(message)
        {}
    }
}