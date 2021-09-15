using System;

namespace TimHanewich.Cds.AdvancedRead
{
    public class CdsReadFilter
    {
        public string ColumnName {get; set;}
        public ComparisonOperator Operator {get; set;}
        
        //Value to compare to
        private string _Value;
        public string Value
        {
            get
            {
                return _Value;
            }
        }

        public CdsReadFilter()
        {
            ColumnName = null;
            _Value = null;
        }
    
        public void SetValue(string value)
        {
            _Value = "'" + value + "'";
        }

        public void SetValue(float value)
        {
            _Value = value.ToString();
        }

        public void SetValue(int value)
        {
            _Value = value.ToString();
        }
    
        public override string ToString()
        {
            if (ColumnName == null)
            {
                throw new Exception("Unable to convert CDS Read Filter to string. Column name was null.");
            }
            if (Value == null)
            {
                throw new Exception("Unable to convert CDS Read Filter to string. Value was null.");
            }

            return ColumnName + " " + OperatorToString(Operator) + " " + Value;
        }

        #region "Utility Functions"

        private string OperatorToString(ComparisonOperator op)
        {
            switch (op)
            {
                case ComparisonOperator.Equals:
                    return "eq";
                case ComparisonOperator.GreaterThan:
                    return "gt";
                case ComparisonOperator.LessThan:
                    return "lt";
                case ComparisonOperator.NotEqualTo:
                    return "ne";
                case ComparisonOperator.GreaterThanOrEqualTo:
                    return "ge";
                case ComparisonOperator.LessThanOrEqualTo:
                    return "le";
                default:
                    throw new Exception("String operator unknown for '" + op.ToString() + "'");
            }
        }

        #endregion
    }
}