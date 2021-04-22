using System;

namespace TimHanewich.Cds.Metadata
{
    public class AttributeMetadata
    {
        public Guid MetadataId {get; set;}
        public string AttributeType {get; set;}
        public int ColumnNumber {get; set;}
        public Version IntroducedVersion {get; set;}
        public bool IsManaged {get; set;}
        public string LogicalName {get; set;}
        public string SchemaName {get; set;}
        public string Format {get; set;}
        public int MaxLength {get; set;}
        public int DatabaseLength {get; set;}
        public string Description {get; set;}
        public string DisplayName {get; set;}
        public bool IsAuditEnabled {get; set;}
        public bool IsCustomizable {get; set;}
    }
}