using System;

namespace TimHanewich.Cds.Metadata
{
    public class OneToManyRelationship
    {

        //The entity that is being pointed to (the "one" in the relationship)
        public string ReferencedAttribute {get; set;} //The primary key (will always be a GUID) id that is being stored as a foreign key in the pointing table
        public string ReferencedEntity {get; set;} //Entity logical name

        //The entity that is "pointing" at another table (the "many" in the relationship)
        public string ReferencingAttribute {get; set;} //The property it is using to "point" the foreign key, containing the primary key of the table it is pointing to
        public string ReferencingEntity {get; set;} //Entity logical name

        public bool IsCustomRelationship {get; set;}
    
    }
}