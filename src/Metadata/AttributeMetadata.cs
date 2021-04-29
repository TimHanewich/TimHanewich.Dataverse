using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TimHanewich.Cds.Metadata
{
    public class AttributeMetadata
    {
        public Guid MetadataId {get; set;}
        public AttributeType AttributeType {get; set;}
        public int ColumnNumber {get; set;}
        public Version IntroducedVersion {get; set;}
        public bool IsManaged {get; set;}
        public string LogicalName {get; set;}
        public string SchemaName {get; set;} 
        public string Description {get; set;}
        public string DisplayName {get; set;}
        public bool IsAuditEnabled {get; set;}
        public bool IsCustomizable {get; set;}
        public bool IsCustomAttribute {get; set;}
        public StringFormat? StringFormat {get; set;}

        //String-specific values
        //These will only be populated if the attribute derives from a string field in CDS
        public FormatType? Format {get; set;}
        public int MaxLength {get; set;}
        public int DatabaseLength {get; set;}

        public static AttributeMetadata ParseFromApiJson(string json)
        {
            JObject jo = JObject.Parse(json);
            AttributeMetadata ToReturn = new AttributeMetadata();

            ToReturn.MetadataId = Guid.Parse(jo.Property("MetadataId").Value.ToString());
            ToReturn.AttributeType = CdsServiceMetadataExtension.AttributeTypeApiStringToEnum(jo.Property("AttributeType").Value.ToString());
            ToReturn.ColumnNumber = Convert.ToInt32(jo.Property("ColumnNumber").Value.ToString());
            ToReturn.IntroducedVersion = new Version(jo.Property("IntroducedVersion").Value.ToString());
            ToReturn.IsManaged = Convert.ToBoolean(jo.Property("IsManaged").Value.ToString());
            ToReturn.LogicalName = jo.Property("LogicalName").Value.ToString();
            ToReturn.SchemaName = jo.Property("SchemaName").Value.ToString();
            ToReturn.Description = CdsServiceMetadataExtension.GetLocalizedLabel(jo, "Description");
            ToReturn.DisplayName = CdsServiceMetadataExtension.GetLocalizedLabel(jo, "DisplayName");
            ToReturn.IsAuditEnabled = CdsServiceMetadataExtension.GetNestedBoolean(jo, "IsAuditEnabled");
            ToReturn.IsCustomizable = CdsServiceMetadataExtension.GetNestedBoolean(jo, "IsCustomizable");
            ToReturn.IsCustomAttribute = Convert.ToBoolean(jo.Property("IsCustomAttribute").Value.ToString());

            //String-related fields
            JProperty prop_Format = jo.Property("Format");
            if (prop_Format != null)
            {
                ToReturn.Format = CdsServiceMetadataExtension.FormatApiStringToEnum(jo.Property("Format").Value.ToString());
            }
            else
            {
                ToReturn.Format = null;
            }
            JProperty prop_MaxLength = jo.Property("MaxLength");
            if (prop_MaxLength != null)
            {
                ToReturn.MaxLength = Convert.ToInt32(jo.Property("MaxLength").Value.ToString());
            }
            JProperty prop_DatabaseLength = jo.Property("DatabaseLength");
            if (prop_DatabaseLength != null)
            {
                ToReturn.DatabaseLength = Convert.ToInt32(jo.Property("DatabaseLength").Value.ToString());
            }

            //String format (under the "FormatName" property, like this):
            //"FormatName":{"Value":"Phone"}
            JProperty prop_FormatName = jo.Property("FormatName");
            if (prop_FormatName != null)
            {
                JObject obj_FormatName = JObject.Parse(jo.Property("FormatName").Value.ToString());
                JProperty prop_Value = obj_FormatName.Property("Value");
                string propformat = prop_Value.Value.ToString();
                
                //Auto assign
                StringFormat ToAssign = TimHanewich.Cds.Metadata.StringFormat.Other;
                foreach (StringFormat sf in Enum.GetValues(typeof(StringFormat)))
                {
                    if (sf.ToString().ToLower() == propformat.ToLower())
                    {
                        ToAssign = sf;
                    }
                }
                ToReturn.StringFormat = ToAssign;
            }
            else
            {
                ToReturn.StringFormat = null;
            }
            
            return ToReturn;
        }
    }
}