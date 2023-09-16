using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TimHanewich.Dataverse.Metadata
{
    public class AttributeMetadata
    {
        public Guid MetadataId {get; set;}
        public AttributeType AttributeType {get; set;}
        public string AttributeOf {get; set;}
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
        public AttributeRequireLevel RequireLevel {get; set;}

        //String-specific values
        //These will only be populated if the attribute derives from a string field in CDS
        public FormatType? Format {get; set;}
        public int MaxLength {get; set;}
        public int DatabaseLength {get; set;}

        //Lookup-specific values
        // These will only be populated if this attribute is a lookup value
        // If this attribute is NOT a lookup value, it will be null
        public string[] Targets {get; set;}

        public static AttributeMetadata ParseFromApiJson(string json)
        {
            JObject jo = JObject.Parse(json);
            AttributeMetadata ToReturn = new AttributeMetadata();

            ToReturn.MetadataId = Guid.Parse(jo.Property("MetadataId").Value.ToString());
            ToReturn.AttributeType = DataverseServiceMetadataExtension.AttributeTypeApiStringToEnum(jo.Property("AttributeType").Value.ToString());
            ToReturn.ColumnNumber = Convert.ToInt32(jo.Property("ColumnNumber").Value.ToString());
            ToReturn.IntroducedVersion = new Version(jo.Property("IntroducedVersion").Value.ToString());
            ToReturn.IsManaged = Convert.ToBoolean(jo.Property("IsManaged").Value.ToString());
            ToReturn.LogicalName = jo.Property("LogicalName").Value.ToString();
            ToReturn.SchemaName = jo.Property("SchemaName").Value.ToString();
            ToReturn.Description = DataverseServiceMetadataExtension.GetLocalizedLabel(jo, "Description");
            ToReturn.DisplayName = DataverseServiceMetadataExtension.GetLocalizedLabel(jo, "DisplayName");
            ToReturn.IsAuditEnabled = DataverseServiceMetadataExtension.GetNestedBoolean(jo, "IsAuditEnabled");
            ToReturn.IsCustomizable = DataverseServiceMetadataExtension.GetNestedBoolean(jo, "IsCustomizable");
            ToReturn.IsCustomAttribute = Convert.ToBoolean(jo.Property("IsCustomAttribute").Value.ToString());

            //Require level
            JProperty prop_RequiredLevel = jo.Property("RequiredLevel");
            if (prop_RequiredLevel != null)
            {
                JObject obj_RequiredLevel = JObject.Parse(prop_RequiredLevel.Value.ToString());
                string requiredlvl = obj_RequiredLevel.Property("Value").Value.ToString();
                if (requiredlvl == "None")
                {
                    ToReturn.RequireLevel = AttributeRequireLevel.None;
                }
                else if (requiredlvl == "Recommended")
                {
                    ToReturn.RequireLevel = AttributeRequireLevel.Recommended;
                }
                else if (requiredlvl == "ApplicationRequired")
                {
                    ToReturn.RequireLevel = AttributeRequireLevel.ApplicationRequired;
                }
                else //This should never happen
                {
                    ToReturn.RequireLevel = AttributeRequireLevel.None;
                }
            }


            //String-related fields
            JProperty prop_Format = jo.Property("Format");
            if (prop_Format != null)
            {
                ToReturn.Format = DataverseServiceMetadataExtension.FormatApiStringToEnum(jo.Property("Format").Value.ToString());
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
                StringFormat ToAssign = TimHanewich.Dataverse.Metadata.StringFormat.Other;
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

            
            // Targets property (for lookups, which table it points to)
            JProperty prop_Targets = jo.Property("Targets");
            if (prop_Targets != null)
            {
                if (prop_Targets.Value.Type != JTokenType.Null)
                {
                    if (prop_Targets.Value.Type != JTokenType.None)
                    {
                        string[] targets = JsonConvert.DeserializeObject<string[]>(prop_Targets.Value.ToString()); // It will be an array of strings, with each string representing the logical name of the entity.
                        ToReturn.Targets = targets;
                    }
                }
            }

            //AttributeOf
            JProperty prop_AttributeOf = jo.Property("AttributeOf");
            if (prop_AttributeOf != null)
            {
                if (prop_AttributeOf.Value.Type != JTokenType.Null)
                {
                    ToReturn.AttributeOf = prop_AttributeOf.Value.ToString();
                }
                else
                {
                    ToReturn.AttributeOf = null;
                }
            }
            else
            {
                ToReturn.AttributeOf = null;
            }

            
            return ToReturn;
        }
    }
}