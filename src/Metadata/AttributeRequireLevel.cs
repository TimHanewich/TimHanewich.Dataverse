using System;

namespace TimHanewich.Dataverse.Metadata
{
    public enum AttributeRequireLevel
    {
        None = 0, //Not required
        Recommended = 1, //Recommended
        ApplicationRequired = 2, //Required (needs to be submitted as part of a new CDS insert operation)
    }
}