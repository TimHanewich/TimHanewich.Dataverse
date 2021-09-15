using System;
using System.Collections.Generic;

namespace TimHanewich.Cds.AdvancedRead
{
    public class CdsReadConfiguration
    {
        public string Table {get; set;}

        //Column selection
        private List<string> _SelectColumns;
        public string[] SelectColumns
        {
            get
            {
                return _SelectColumns.ToArray();
            }
        }

        public void AddColumn(string column_name)
        {
            _SelectColumns.Add(column_name);
        }

        public void RemoveColumn(string column_name)
        {
            _SelectColumns.Remove(column_name);
        }

        public void ClearColumns()
        {
            _SelectColumns.Clear();
        }

        public CdsReadConfiguration()
        {
            _SelectColumns = new List<string>();
        }
    }
}