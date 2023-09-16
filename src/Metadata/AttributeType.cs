using System;

namespace TimHanewich.Dataverse.Metadata
{
    public enum AttributeType
    {
        Other = 0,
        String = 1,
        Money = 2,
        Integer = 3,
        Lookup = 4,
        Boolean = 5,
        DateTime = 6,
        Memo = 7,
        Decimal = 8,
        Customer = 9,
        Virtual = 10, //An image
        Picklist = 11,
        Double = 12,
        BigInt = 13,
        EntityName = 14,
        State = 15,
        Owner = 16,
        Uniqueidentifier = 17,
        Status = 18
    }
}