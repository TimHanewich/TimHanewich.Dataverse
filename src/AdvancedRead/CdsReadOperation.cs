using System;
using System.Collections.Generic;

namespace TimHanewich.Cds.AdvancedRead
{
    public class CdsReadOperation : TableSelection
    {
        public Guid? RecordId {get; set;}
        public TableSelection Expand {get; set;}

        //Filters
        private List<CdsReadFilter> _Filters;
        public CdsReadFilter[] Filters
        {
            get
            {
                return _Filters.ToArray();
            }
        }

        //Sorting
        private List<CdsReadOrder> _Ordering;
        public CdsReadOrder[] Ordering
        {
            get
            {
                return _Ordering.ToArray();
            }
        }

        public CdsReadOperation()
        {
            RecordId = null;
            _Filters = new List<CdsReadFilter>();
            _Ordering = new List<CdsReadOrder>();
        }

        #region "Filters operations"

        public void AddFilter(CdsReadFilter filter)
        {
            _Filters.Add(filter);
        }

        public void RemoveFilter(CdsReadFilter filter)
        {
            _Filters.Remove(filter);
        }

        public void ClearFilters()
        {
            _Filters.Clear();
        }

        #endregion

        #region "Ordering"

        public void AddOrdering(CdsReadOrder order)
        {
            _Ordering.Add(order);
        }

        public void RemoveOrdering(CdsReadOrder order)
        {
            _Ordering.Remove(order);
        }

        public void ClearOrdering()
        {
            _Ordering.Clear();
        }

        #endregion

        public string ToUrlExtension()
        {
            if (TableIdentifier == null)
            {
                throw new Exception("Trying to construct query URL extension without setting the 'TableIdentifier' property first. This is required.");
            }

            //Is this for a single record or is this for an array of records? it cannot be both
            //Therefore, the user should only have specified the record id OR the filters.
            if (RecordId.HasValue && Filters.Length > 0)
            {
                throw new Exception("You specified both a single record to retrieve (by ID) and one or more filters to use. You can only do one of these (request a single record or multiple).");
            }

            //Start with the table name
            string ToReturn = TableIdentifier;
            
            //Add the next characters
            if (RecordId.HasValue) //If it is for 1 record only.
            {
                ToReturn = ToReturn + "(" + RecordId.ToString() + ")";
            }

            //Now, just add any statements
            List<string> ParamsToAdd = new List<string>();

            //Filter statement
            string FilterStmt = PrepareFilterStatement();
            if (FilterStmt != null)
            {
                ParamsToAdd.Add(FilterStmt);
            }

            //Ordring statement
            string OrderStmt = PrepareOrderStatement();
            if (OrderStmt != null)
            {
                ParamsToAdd.Add(OrderStmt);
            }


            //Append a select statement if one exists
            string SelectStmt = PrepareSelectStatement();
            if (SelectStmt != null)
            {
                ParamsToAdd.Add(SelectStmt);
            }

            //Append an expand if one exists
            string ExpandStmt = PrepareExpandStatement();
            if (ExpandStmt != null)
            {
                ParamsToAdd.Add(ExpandStmt);
            }

            //Add them
            ToReturn = ToReturn + UrlParamSeperator(ParamsToAdd.ToArray());

            return ToReturn;
        }

        #region "UTILITY"

        private string PrepareSelectStatement()
        {
            string ToReturn = null;

            if (SelectColumns.Length > 0)
            {
                ToReturn = "$select=";
                foreach (string s in SelectColumns)
                {
                    ToReturn = ToReturn + s + ",";
                }
                ToReturn = ToReturn.Substring(0, ToReturn.Length - 1);
            }

            return ToReturn;
        }

        private string PrepareFilterStatement()
        {
            string ToReturn = null;

            if (Filters.Length > 0)
            {
                ToReturn = "$filter=";
                foreach (CdsReadFilter filter in Filters)
                {
                    ToReturn = ToReturn + filter.ToString() + " ";
                }
                ToReturn = ToReturn.Substring(0, ToReturn.Length - 1);
            }

            return ToReturn;
        }

        private string UrlParamSeperator(string[] param_and_values)
        {
            string ToReturn = "";

            if (param_and_values.Length > 0)
            {
                ToReturn = "?";
                foreach (string s in param_and_values)
                {
                    ToReturn = ToReturn + s + "&";
                }
                ToReturn = ToReturn.Substring(0, ToReturn.Length - 1); //Remove the last "&" symbol.
            }

            return ToReturn;
        }

        private string PrepareExpandStatement()
        {
            string ToReturn = null;

            if (Expand != null)
            {
                ToReturn = ToReturn + "$expand=" + Expand.TableIdentifier;

                //Prepare columns to select (if any)
                if (Expand.SelectColumns.Length > 0)
                {
                    ToReturn = ToReturn + "($select=";
                    foreach (string s in Expand.SelectColumns)
                    {
                        ToReturn = ToReturn + s + ",";
                    }
                    ToReturn = ToReturn.Substring(0, ToReturn.Length - 1); //Remove the last comma
                    ToReturn = ToReturn + ")";
                }
            }

            return ToReturn;
        }

        private string PrepareOrderStatement()
        {
            string ToReturn = null;

            if (Ordering.Length > 0)
            {
                ToReturn = "$orderby=";
                foreach (CdsReadOrder order in Ordering)
                {
                    ToReturn = ToReturn + order.ColumnName + " ";
                    if (order.Direction == OrderDirection.Ascending)
                    {
                        ToReturn = ToReturn + "asc";
                    }
                    else if (order.Direction == OrderDirection.Descending)
                    {
                        ToReturn = ToReturn + "desc";
                    }

                    //Place a comma at the end
                    ToReturn = ToReturn + ",";
                }
                ToReturn = ToReturn.Substring(0, ToReturn.Length - 1); //Remove the last trailing comma
            }

            return ToReturn;
        }

        #endregion
    }
}