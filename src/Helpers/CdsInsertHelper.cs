using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TimHanewich.Cds.Helpers
{
    public class CdsInsertHelper
    {
        private List<PropertyValuePair> PVPs;

        public CdsInsertHelper()
        {
            PVPs = new List<PropertyValuePair>();
        }

        public void AddProperty(string property, string value)
        {
            PropertyValuePair pvp = new PropertyValuePair();
            pvp.Property = property;
            pvp.Value = value;
            pvp.IsLookup = false;
            pvp.LookupEntitySetter = null;
            PVPs.Add(pvp);
        }

        public void AddLookupProperty(string property, string lookup_entity_setter, string value)
        {
            PropertyValuePair pvp = new PropertyValuePair();
            pvp.Property = property;
            pvp.Value = value;
            pvp.IsLookup = true;
            pvp.LookupEntitySetter = lookup_entity_setter;
            PVPs.Add(pvp);
        }
    
        public JObject ToJObject()
        {
            JObject ToReturn = new JObject();
            foreach (PropertyValuePair pvp in PVPs)
            {

                if (pvp.IsLookup == false) //Normal properties
                {
                    ToReturn.Add(pvp.Property, pvp.Value);
                }
                else if (pvp.IsLookup == true) //Lookup properties
                {
                    ToReturn.Add(pvp.Property + "@odata.bind", pvp.LookupEntitySetter + "(" + pvp.Value + ")");
                }
            }

            return ToReturn;
        }

        public string ToPayloadString()
        {
            return ToJObject().ToString();
        }
    }
}