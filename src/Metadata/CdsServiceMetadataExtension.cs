using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TimHanewich.Cds.Metadata
{
    public static class CdsServiceMetadataExtension
    {
        public static async Task<EntityMetadataSummary[]> GetEntityMetadataSummariesAsync(this CdsService service)
        {
            string url = service.ReadEnvironmentRequestUrl();
            
            //Prepare request
            HttpRequestMessage req = new HttpRequestMessage();
            req.Method = HttpMethod.Get;
            req.RequestUri = new Uri(url + "EntityDefinitions?$select=LogicalName,SchemaName,LogicalCollectionName,CollectionSchemaName,EntitySetName, DisplayName,IsCustomEntity");
            req.Headers.Add("Authorization", "Bearer " + service.ReadAccessToken());

            //Make the request
            HttpClient hc = new HttpClient();
            HttpResponseMessage resp = await hc.SendAsync(req);
            string content = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Request for all entity metadata summaries failed with code " + resp.StatusCode.ToString() + ". Body: " + content);
            }

            //Get the objects
            JObject jo = JObject.Parse(content);
            JProperty prop_value = jo.Property("value");
            JArray ja_values = JArray.Parse(prop_value.Value.ToString());
            List<EntityMetadataSummary> ToReturn = new List<EntityMetadataSummary>();
            foreach (JObject oo in ja_values)
            {
                EntityMetadataSummary s = new EntityMetadataSummary();
                s.MetadataId = Guid.Parse(oo.Property("MetadataId").Value.ToString());
                s.LogicalName = oo.Property("LogicalName").Value.ToString();
                s.SchemaName = oo.Property("SchemaName").Value.ToString();
                s.LogicalCollectionName = oo.Property("LogicalCollectionName").Value.ToString();
                s.CollectionSchemaName = oo.Property("CollectionSchemaName").Value.ToString();
                s.EntitySetName = oo.Property("EntitySetName").Value.ToString();

                //Display name (if available)
                s.DisplayName = GetLocalizedLabel(oo, "DisplayName");

                //Is custom
                s.IsCustomEntity = Convert.ToBoolean(oo.Property("IsCustomEntity").Value.ToString());
                
                ToReturn.Add(s);
            }

            return ToReturn.ToArray();
        }
    
        public static async Task<EntityMetadata> GetEntityMetadataAsync(this CdsService service, string entity_logical_name)
        {
            string url = service.ReadEnvironmentRequestUrl() + "EntityDefinitions(LogicalName='" + entity_logical_name + "')?$expand=Attributes";
            EntityMetadata ToReturn = await service.GetEntityMetadataFromRequestUrlAsync(url);
            return ToReturn;
        }

        public static async Task<EntityMetadata> GetEntityMetadataAsync(this CdsService service, Guid metadata_id)
        {
            string url = service.ReadEnvironmentRequestUrl() + "EntityDefinitions(" + metadata_id.ToString() + ")?$expand=Attributes";
            EntityMetadata ToReturn = await service.GetEntityMetadataFromRequestUrlAsync(url);
            return ToReturn;
        }

        public static async Task<Choice[]> GetAllChoiceMetadataAsync(this CdsService service)
        {
            string url = service.ReadEnvironmentRequestUrl() + "GlobalOptionSetDefinitions";
            HttpClient hc = new HttpClient();
            HttpRequestMessage req = new HttpRequestMessage();
            req.RequestUri = new Uri(url);
            req.Method = HttpMethod.Get;
            req.Headers.Add("Authorization", "Bearer " + service.ReadAccessToken());
            HttpResponseMessage resp = await hc.SendAsync(req);
            string content = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Failure while trying to access choice metadata. Returned '" + resp.StatusCode.ToString() + "'. Msg: " + content);
            }
            JObject jo = JObject.Parse(content);
            JArray ja = JArray.Parse(jo.Property("value").Value.ToString());
            
            List<Choice> ToReturn = new List<Choice>();
            foreach (JObject joc in ja)
            {
                Choice c = Choice.ParseJsonFromApi(joc.ToString());
                ToReturn.Add(c);
            }
            return ToReturn.ToArray();
        }

        //If you find an attribute of a table that uses a picklist, this will let you find the logical name of the global option set that that attribute points to
        public static async Task<string> FindPicklistGlobalOptionSetAsync(this CdsService service, string entity_logical_name, Guid attribute_id)
        {
            string url = service.ReadEnvironmentRequestUrl() + "EntityDefinitions(LogicalName='" + entity_logical_name + "')/Attributes(" + attribute_id.ToString() + ")/Microsoft.Dynamics.CRM.PicklistAttributeMetadata?$expand=OptionSet";
            HttpClient hc = new HttpClient();
            HttpRequestMessage req = new HttpRequestMessage();
            req.RequestUri = new Uri(url);
            req.Method = HttpMethod.Get;
            req.Headers.Add("Authorization", "Bearer " + service.ReadAccessToken());
            HttpResponseMessage resp = await hc.SendAsync(req);
            string content = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Failure while trying to find global option set used in entity '" + entity_logical_name + "' in attribute '" + attribute_id.ToString() + "'. Returned '" + resp.StatusCode.ToString() + "'. Msg: " + content);
            }
            JObject jo = JObject.Parse(content);

            //Get the OptionSet property
            JProperty prop_OptionSet = jo.Property("OptionSet");
            if (prop_OptionSet != null)
            {
                JObject OptionSet = JObject.Parse(prop_OptionSet.Value.ToString());
                JProperty prop_Name = OptionSet.Property("Name");
                if (prop_Name != null)
                {
                    return prop_Name.Value.ToString();
                }
                else
                {
                    throw new Exception("Failure while trying to find global option set used in entity '" + entity_logical_name + "' in attribute '" + attribute_id.ToString() + "': OptionSet property did not have a 'Name' property.");
                }
            }
            else
            {
                throw new Exception("Failure while trying to find global option set used in entity '" + entity_logical_name + "' in attribute '" + attribute_id.ToString() + "': OptionSet property was not returned in the payload.");
            }
        }





        //Used for entity metadata
        private static async Task<EntityMetadata> GetEntityMetadataFromRequestUrlAsync(this CdsService service, string url)
        {
            //Prepare the request
            HttpRequestMessage req = new HttpRequestMessage();
            req.Method = HttpMethod.Get;
            req.Headers.Add("Authorization", "Bearer " + service.ReadAccessToken());
            req.RequestUri = new Uri(url);
            
            //Make the request
            HttpClient hc = new HttpClient();
            HttpResponseMessage resp = await hc.SendAsync(req);
            string content = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Request for metadata for entity failed with code " + resp.StatusCode.ToString() + ". Msg: " + content);
            }

            EntityMetadata ToReturn = EntityMetadata.ParseFromApiJson(content);
            return ToReturn;
        }

        #region "Utility"

        public static string GetLocalizedLabel(JObject master, string property_name)
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
            JProperty prop_localizedlabel = thisprop.Property("LocalizedLabels");
            if (prop_localizedlabel == null)
            {
                return null; 
            }
            if (prop_localizedlabel.Value.Type == JTokenType.Null)
            {
                return null;
            }

            //Get the first in the array
            JArray ja = JArray.Parse(prop_localizedlabel.Value.ToString());
            if (ja.Count == 0)
            {
                return null;
            }

            //Get the label
            JObject obj_localizedlabel = JObject.Parse(ja[0].ToString());
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

        public static bool GetNestedBoolean(JObject master, string property_name)
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

        public static AttributeType AttributeTypeApiStringToEnum(string att)
        {
            AttributeType ToReturn = AttributeType.Other;

            foreach (AttributeType val in Enum.GetValues(typeof(AttributeType)))
            {
                if (val.ToString().ToLower() == att.ToLower())
                {
                    ToReturn = val;
                }
            }

            return ToReturn;
        }

        public static FormatType FormatApiStringToEnum(string format)
        {
            FormatType ToReturn = FormatType.Other;

            foreach (FormatType ft in Enum.GetValues(typeof(FormatType)))
            {
                if (format.ToLower() == ft.ToString().ToLower())
                {
                    ToReturn = ft;
                }
            }

            return ToReturn;
        }

        #endregion
    }
}