using System;

namespace TimHanewich.Dataverse.Metadata
{
    public class EntityMetadataSummary
    {
        public Guid MetadataId {get; set;}
        public string DisplayName {get; set;}
        public string DisplayCollectionName {get; set;} //plural display name
        public string LogicalName {get; set;} //Used to request additional metadata
        public string SchemaName {get; set;}
        public string LogicalCollectionName {get; set;}
        public string CollectionSchemaName {get; set;}
        public string EntitySetName {get; set;}
        public bool IsCustomEntity {get; set;}
    }
}