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
    }
}