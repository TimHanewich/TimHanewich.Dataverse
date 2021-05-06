using System;

namespace TimHanewich.Cds.Metadata
{
    public class Choice
    {
        public string Name {get; set;}
        public string DisplayName {get; set;}
        public Guid MetadataId {get; set;}
        public ChoiceOption[] Options {get; set;}
    }
}