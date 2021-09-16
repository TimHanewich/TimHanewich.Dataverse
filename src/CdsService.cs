using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Collections.Generic;
using TimHanewich.Cds.AdvancedRead;


namespace TimHanewich.Cds
{
    public class CdsService
    {
        private string EnvironmentRequestUrl; //This should be something like https://org_name.crm.dynamics.com/api/data/v9.0/
        
        private string AccessToken;

        #region "Constructors"

        public CdsService()
        {

        }

        public CdsService(string org_name, string access_token)
        {
            EnvironmentRequestUrl = "https://" + org_name + ".crm.dynamics.com/api/data/v9.0/";
            AccessToken = access_token;
        }



        #endregion

        public async Task<JObject> GetRecordAsync(string setter, string id)
        {
            //Construct the query endpoint
            string query_ep = setter + "(" + id + ")";

            //Construct the request
            HttpRequestMessage req = new HttpRequestMessage();
            req.Method = HttpMethod.Get;
            req.RequestUri = new Uri(EnvironmentRequestUrl + query_ep);
            req.Headers.Add("Authorization", "Bearer " + AccessToken);

            //Make the request
            HttpClient hc = new HttpClient();
            HttpResponseMessage resp = await hc.SendAsync(req);
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                string errcont = await resp.Content.ReadAsStringAsync();
                throw new Exception("Request for record failed with code " + resp.StatusCode.ToString() + ". Content: " + errcont);
            }
            string cont = await resp.Content.ReadAsStringAsync();

            //Return the content
            JObject ToReturn = JObject.Parse(cont);
            return ToReturn;
        }
        
        public async Task CreateRecordAsync(string setter, string object_json)
        {
            //Construct the request
            HttpRequestMessage req = new HttpRequestMessage();
            req.Method = HttpMethod.Post;
            req.RequestUri = new Uri(EnvironmentRequestUrl + setter);
            req.Headers.Add("Authorization", "Bearer " + AccessToken);
            req.Content = new StringContent(object_json, Encoding.UTF8, "application/json");

            //Make the request
            HttpClient hc = new HttpClient();
            HttpResponseMessage resp = await hc.SendAsync(req);
            string cont = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode != HttpStatusCode.NoContent)
            {
                throw new Exception("Unable to create new record of type '" + setter + "'. Content: " + cont);
            }
        }

        public async Task DeleteRecordAsync(string setter, string id)
        {
            //Construct the endpoint
            string ep = setter + "(" + id + ")";

            //Construct the reuqest
            HttpRequestMessage req = new HttpRequestMessage();
            req.Method = HttpMethod.Delete;
            req.RequestUri = new Uri(EnvironmentRequestUrl + ep);
            req.Headers.Add("Authorization", "Bearer " + AccessToken);
            
            //Make the request
            HttpClient hc = new HttpClient();
            HttpResponseMessage msg = await hc.SendAsync(req);
            if (msg.StatusCode != HttpStatusCode.NoContent)
            {
                string ermsg = await msg.Content.ReadAsStringAsync();
                throw new Exception("Request to delete record '" + id + "' of type '" + setter + "' failed with the following message: " + ermsg);
            }
        }
    
        public async Task<JObject[]> GetRecordsAsync(string setter)
        {
            //construct the request
            HttpRequestMessage req = new HttpRequestMessage();
            req.Method = HttpMethod.Get;
            req.RequestUri = new Uri(EnvironmentRequestUrl + setter);
            req.Headers.Add("Authorization", "Bearer " + AccessToken);

            //Make the request
            HttpClient hc = new HttpClient();
            HttpResponseMessage resp = await hc.SendAsync(req);
            string content = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Unable to retireve records of type '" + setter + "'. Error message: " + content);
            }

            //Get the responses
            JObject jo = JObject.Parse(content);
            if (jo["value"] == null)
            {
                throw new Exception("Returned payload with '" + setter + "' entities came back empty.");
            }
            List<JObject> ToReturn = new List<JObject>();
            foreach (JObject sjo in jo["value"])
            {
                ToReturn.Add(sjo);
            }

            //Return it
            return ToReturn.ToArray();
        }
    
        public async Task UpdateRecordAsync(string setter, string id, string object_json)
        {
            //Construct the endpoint
            string ep = EnvironmentRequestUrl + setter + "(" + id + ")";

            //Construct the request
            HttpRequestMessage req = new HttpRequestMessage();
            req.Method = new HttpMethod("PATCH");
            req.RequestUri = new Uri(ep);
            req.Headers.Add("Authorization", "Bearer " + AccessToken);
            req.Content = new StringContent(object_json, Encoding.UTF8, "application/json");
            
            //Make the request
            HttpClient hc = new HttpClient();
            HttpResponseMessage msg = await hc.SendAsync(req);
            if (msg.StatusCode != HttpStatusCode.NoContent)
            {
                string cont = await msg.Content.ReadAsStringAsync();
                throw new Exception("The update record request of type '" + setter + "' and ID '" + id + "' failed. Message content: " + cont);
            }
        }
    
        public async Task<JObject[]> ExecuteCdsReadOperationAsync(CdsReadOperation operation)
        {
            string ToRequestTo = EnvironmentRequestUrl + operation.ToUrlExtension();
            HttpRequestMessage req = PrepareRequestMsg();
            req.RequestUri = new Uri(ToRequestTo);
            req.Method = HttpMethod.Get;

            //Call
            HttpClient hc = new HttpClient();
            HttpResponseMessage resp = await hc.SendAsync(req);
            string bodystr = await resp.Content.ReadAsStringAsync();
            JObject body = JObject.Parse(bodystr);

            //Get and retunr
            List<JObject> ToReturn = new List<JObject>();

            if (resp.StatusCode == HttpStatusCode.OK)
            {
                bool BodyIsOneRecord = false;

                JProperty prop = body.Property("value");
                if (prop != null)
                {
                    if (prop.Type == JTokenType.Array)
                    {
                        BodyIsOneRecord = false;
                        foreach (JObject jo in prop)
                        {
                            ToReturn.Add(jo);
                        }
                    }
                    else
                    {
                        BodyIsOneRecord = true;
                    }
                }
                else
                {
                    BodyIsOneRecord = true;
                }

                if (BodyIsOneRecord)
                {
                    ToReturn.Add(body);
                }
            }
            else
            {
                throw new Exception("Response from dataverse API: " + resp.StatusCode.ToString() + " - " + bodystr);
            }

            return ToReturn.ToArray();
        }

        public string ReadEnvironmentRequestUrl()
        {
            return EnvironmentRequestUrl;
        }

        public string ReadAccessToken()
        {
            return AccessToken;
        }
    
    
        private HttpRequestMessage PrepareRequestMsg()
        {
            //Construct the request
            HttpRequestMessage req = new HttpRequestMessage();
            req.Headers.Add("Authorization", "Bearer " + AccessToken);
            return req;
        }
    }
}