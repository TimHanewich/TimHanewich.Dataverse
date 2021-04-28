using System;

namespace TimHanewich.Cds.Metadata
{
    public class EntityMetadataSummary
    {
        public Guid MetadataId {get; set;}
        public string LogicalName {get; set;} //Used to request additional metadata
        public string SchemaName {get; set;}
        public string LogicalCollectionName {get; set;}
        public string CollectionSchemaName {get; set;}
        public string EntitySetName {get; set;}
    }
}