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
            req.RequestUri = new Uri(url);
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
                s.Name = oo.Property("name").Value.ToString();
                s.UrlExtension = oo.Property("url").Value.ToString();
                ToReturn.Add(s);
            }

            return ToReturn.ToArray();
        }
    
        public static async Task<EntityMetadata> GetEntityMetadataAsync(this CdsService service, string entity_logical_name)
        {
            string url = service.ReadEnvironmentRequestUrl() + "EntityDefinitions(LogicalName='" + entity_logical_name + "')?$expand=Attributes";

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
                throw new Exception("Request for metadata for entity '" + entity_logical_name + "' failed with code " + resp.StatusCode.ToString() + ". Msg: " + content);
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

        #endregion
    }
}