using System;

namespace TimHanewich.Cds.Metadata
{
    public class ManyToManyRelationship
    {

        public string Entity1LogicalName {get; set;} 
        public string Entity2LogicalName {get; set;} 

        public string IntersectEntityName {get; set;} //Logical name of the entity that ties the two together
        public string Entity1IntersectAttribute {get; set;} //The attribute of entity 1 that the intersect is using (it's primary key most likely)
        public string Entity2IntersectAttribute {get; set;} //The attribute of entity 2 that the intersect is using (it's primary key most likely)
    
    }
}