using System;

namespace TimHanewich.Cds.Helpers
{
    public class PropertyValuePair
    {
        public string Property {get; set;}
        public string Value {get; set;}

        //Lookup related
        public bool IsLookup {get; set;}
        public string LookupEntitySetter {get; set;}
    }
}