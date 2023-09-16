using System;

namespace TimHanewich.Dataverse
{
    public class DataverseToolkit
    {
        // If you have a property called "cra0f_homepen" that is a lookup to another table, you can not use this exact name to do things like filters on the table.
        // When you query data, it will look like "_cra0f_homepen_value". So this is what you need to use for things like filtering.
        public static string LookupPropertyToLookupReference(string property_name)
        {
            return "_" + property_name + "_value";
        }
    }
}