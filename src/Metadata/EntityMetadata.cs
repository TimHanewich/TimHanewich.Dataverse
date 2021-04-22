using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

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
        public AttributeMetadata[] Attributes {get; set;}

        public EntityMetadata()
        {
            
        }

        public static EntityMetadata ParseFromApiJson(string json)
        {
            JObject jo = JObject.Parse(json);

            EntityMetadata ToReturn = new EntityMetadata();

            ToReturn.MetadataId = Guid.Parse(jo.Property("MetadataId").Value.ToString());
            ToReturn.IsBpfEntity = Convert.ToBoolean(jo.Property("IsBPFEntity").Value.ToString());
            ToReturn.IsCustomEntity = Convert.ToBoolean(jo.Property("IsCustomEntity").Value.ToString());
            ToReturn.IsQuickCreateEnabled = Convert.ToBoolean(jo.Property("IsQuickCreateEnabled").Value.ToString());
            ToReturn.LogicalName = jo.Property("LogicalName").Value.ToString();
            ToReturn.ObjectTypeCode = Convert.ToInt32(jo.Property("ObjectTypeCode").Value.ToString());
            ToReturn.SchemaName = jo.Property("SchemaName").Value.ToString();

            //Introduced version
            JProperty prop_IV = jo.Property("IntroducedVersion");
            if (prop_IV != null)
            {
                if (prop_IV.Value.Type != JTokenType.Null)
                {
                    string ivs = prop_IV.Value.ToString();
                    Version v = new Version(ivs);
                    ToReturn.IntroducedVersion = v;
                }
            }

            ToReturn.EntityColor = jo.Property("EntityColor").Value.ToString();
            ToReturn.LogicalCollectionName = jo.Property("LogicalCollectionName").Value.ToString();
            ToReturn.CollectionSchemaName = jo.Property("CollectionSchemaName").Value.ToString();
            ToReturn.EntitySetName = jo.Property("EntitySetName").Value.ToString();
            ToReturn.IsPrivate = Convert.ToBoolean(jo.Property("IsPrivate").Value.ToString());
            ToReturn.CreatedOn = DateTime.Parse(jo.Property("CreatedOn").Value.ToString());
            ToReturn.ModifiedOn = DateTime.Parse(jo.Property("ModifiedOn").Value.ToString());
            ToReturn.Description = new EntityMetadata().GetLocalizedLabel(jo, "Description");
            ToReturn.DisplayCollectioName = new EntityMetadata().GetLocalizedLabel(jo, "DisplayCollectionName");
            ToReturn.DisplayName = new EntityMetadata().GetLocalizedLabel(jo, "DisplayName");
            ToReturn.IsAuditEnabled = new EntityMetadata().GetNestedBoolean(jo, "IsAuditEnabled");
            ToReturn.IsCustomizable = new EntityMetadata().GetNestedBoolean(jo, "IsCustomizable");
            ToReturn.IsDuplicateDetectionEnabled = new EntityMetadata().GetNestedBoolean(jo, "IsDuplicateDetectionEnabled");
            ToReturn.CanCreateAttributes = new EntityMetadata().GetNestedBoolean(jo, "CanCreateAttributes");

            return ToReturn;
        }

        #region "Utility"

        private string GetLocalizedLabel(JObject master, string property_name)
        {
            JProperty prop = master.Property(property_name);
            if (prop == null)
            {
                return null;
            }
            if (prop.Value.Type == JTokenType.Null)
            {
                return null;
            }

            //Get the localized label
            JObject thisprop = JObject.Parse(prop.Value.ToString());
            JProperty prop_localizedlabel = thisprop.Property("LocalizedLabel");
            if (prop_localizedlabel == null)
            {
                return null;
            }
            if (prop_localizedlabel.Value.Type == JTokenType.Null)
            {
                return null;
            }

            //Get the label
            JObject obj_localizedlabel = JObject.Parse(prop_localizedlabel.Value.ToString());
            JProperty prop_label = obj_localizedlabel.Property("Label");
            if (prop_label == null)
            {
                return null;
            }
            if (prop_label.Value.Type == JTokenType.Null)
            {
                return null;
            }
            return prop_label.Value.ToString();
        }

        private bool GetNestedBoolean(JObject master, string property_name)
        {
            //for properties that look like this:
            // "CanCreateAttributes": {
            //         "Value": true,
            //         "CanBeChanged": false,
            //         "ManagedPropertyLogicalName": "cancreateattributes"
            //     }

            JProperty prop = master.Property(property_name);
            JObject asobj = JObject.Parse(prop.Value.ToString());
            bool ToReturn = Convert.ToBoolean(asobj.Property("Value").Value.ToString());
            return ToReturn;
        }

        #endregion
    }
}