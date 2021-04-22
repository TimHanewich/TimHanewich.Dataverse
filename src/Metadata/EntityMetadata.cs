using System;

namespace TimHanewich.Cds.Metadata
{
    public class EntityMetadata
    {
        public Guid MetadataId {get; set;}
        public bool IsBpfEntity {get; set;}
        public bool IsCustomEntity {get; set;}
        public bool IsQuickCreateEnabled {get; set;}
        public string LogicalName {get; set;}
        public int ObjectTypeCode {get; set;}
        public string SchemaName {get; set;}
        public Version IntroducedVersion {get; set;}
        public string EntityColor {get; set;}
        public string LogicalCollectionName {get; set;}
        public string CollectionSchemaName {get; set;}
        public string EntitySetName {get; set;}
        public bool IsPrivate {get; set;}
        public DateTime CreatedOn {get; set;}
        public DateTime ModifiedOn {get; set;}
        public string Description {get; set;}
        public string DisplayCollectioName {get; set;} //Plural name
        public string DisplayName {get; set;}
        public bool IsAuditEnabled {get; set;}
        public bool IsCustomizable {get; set;}
        public bool IsDuplicateDetectionEnabled {get; set;}
        public bool CanCreateAttributes {get; set;}

        public EntityMetadata()
        {
            
        }
    }
}