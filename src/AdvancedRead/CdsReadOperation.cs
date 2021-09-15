using System;
using System.Collections.Generic;

namespace TimHanewich.Cds.AdvancedRead
{
    public class CdsReadOperation : CdsReadConfiguration
    {
        public Guid? RecordId {get; set;}
        public CdsReadConfiguration Exand {get; set;}

        //Filters
        private List<CdsReadFilter> _Filters;
        public CdsReadFilter[] Filters
        {
            get
            {
                return _Filters.ToArray();
            }
        }

        public CdsReadOperation()
        {
            RecordId = null;
            _Filters = new List<CdsReadFilter>();
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


    }
}