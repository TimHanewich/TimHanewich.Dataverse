using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Net;
using System.Text;
using System.Collections.Generic;
using TimHanewich.Dataverse.AdvancedRead;


namespace TimHanewich.Dataverse
{
    public class DataverseService
    {
        private string _EnvironmentRequestUrl; //This should be something like https://org_name.crm.dynamics.com/api/data/v9.0/
        private string _AccessToken;

        #region "Constructors"

        public DataverseService()
        {

        }

        /// <summary>
        /// Initializes a new DataverseService
        /// </summary>
        /// <param name="environment_url">The URL to your Dataverse environment. (i.e. 'https://org7671dd37.crm.dynamics.com/')</param>
        public DataverseService(string environment_url, string access_token)
        {
            //Get the environment URL to use (trim the trailing slash)
            string EnvUrlToUse = environment_url;
            if (EnvUrlToUse.Substring(EnvUrlToUse.Length - 1, 1) == "/")
            {
                EnvUrlToUse = EnvUrlToUse.Substring(0, EnvUrlToUse.Length - 1);
            }

            //Append the dataverse API extensions
            _EnvironmentRequestUrl = EnvUrlToUse + "/api/data/v9.0/";

            //If the provided environment url doesn't start with an https
            if (_EnvironmentRequestUrl.Substring(0, "https://".Length).ToLower() != "https://")
            {
                _EnvironmentRequestUrl = "https://" + _EnvironmentRequestUrl;
            }

            _AccessToken = access_token;
        }

        #endregion

        #region "Reads"

        //Read single record
        public async Task<JObject> ReadAsync(string setter, Guid id)
        {
            //Construct the query endpoint
            string query_ep = setter + "(" + id.ToString() + ")";

            //Construct the request
            HttpRequestMessage req = new HttpRequestMessage();
            req.Method = HttpMethod.Get;
            req.RequestUri = new Uri(_EnvironmentRequestUrl + query_ep);
            req.Headers.Add("Authorization", "Bearer " + _AccessToken);

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
        
        //Read all records
        public async Task<JArray> ReadAsync(string setter)
        {
            //construct the request
            HttpRequestMessage req = new HttpRequestMessage();
            req.Method = HttpMethod.Get;
            req.RequestUri = new Uri(_EnvironmentRequestUrl + setter);
            req.Headers.Add("Authorization", "Bearer " + _AccessToken);

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
            JArray ToReturn = new JArray();
            foreach (JObject sjo in jo["value"])
            {
                ToReturn.Add(sjo);
            }

            //Return it
            return ToReturn;
        }
    
        //Complex read (filtering, top, ordering, etc)
        public async Task<JArray> ReadAsync(DataverseReadOperation operation)
        {
            string ToRequestTo = _EnvironmentRequestUrl + operation.ToUrlExtension();
            HttpRequestMessage req = PrepareRequestMsg();
            req.RequestUri = new Uri(ToRequestTo);
            req.Method = HttpMethod.Get;

            //Call
            HttpClient hc = new HttpClient();
            HttpResponseMessage resp = await hc.SendAsync(req);
            string bodystr = await resp.Content.ReadAsStringAsync();
            JObject body = JObject.Parse(bodystr);

            //Get and return
            JArray ToReturn = new JArray();

            if (resp.StatusCode == HttpStatusCode.OK)
            {
                JProperty prop = body.Property("value");
                if (prop != null)
                {
                    JArray ja = JArray.Parse(prop.Value.ToString());
                    foreach (JObject jo in ja)
                    {
                        ToReturn.Add(jo);
                    }
                }
                else
                {
                    ToReturn.Add(body);
                }
            }
            else
            {
                throw new Exception("Response from dataverse API: " + resp.StatusCode.ToString() + " - " + bodystr);
            }

            return ToReturn;
        }

        //Provide an odata portion of the URL, i.e. EntityDefinitions?$select=LogicalName,LogicalCollectionName,DisplayName,IsCustomEntity&$expand=Attributes($select=AttributeType,LogicalName,DisplayName,IsCustomAttribute)&$filter=IsCustomEntity eq true
        public async Task<JArray> ReadODataAsync(string url_odata_portion)
        {
            string ToRequestTo = _EnvironmentRequestUrl + url_odata_portion;
            HttpRequestMessage req = PrepareRequestMsg();
            req.RequestUri = new Uri(ToRequestTo);
            req.Method = HttpMethod.Get;

            //Call
            HttpClient hc = new HttpClient();
            HttpResponseMessage resp = await hc.SendAsync(req);
            string bodystr = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("The Dataverse API returned '" + resp.StatusCode.ToString() + "' during an attempted read operation. Message: " + bodystr);
            }

            
            JObject body = JObject.Parse(bodystr);
            JProperty prop_value = body.Property("value");
            if (prop_value != null)
            {
                JArray ToReturn = (JArray)prop_value.Value;
                return ToReturn;
            }
            else
            {
                throw new Exception("Unable to find value property (payload) in returned response from the Dataverse API");
            }
        }

        #endregion
        
        public async Task CreateAsync(string setter, JObject json)
        {
            //Construct the request
            HttpRequestMessage req = new HttpRequestMessage();
            req.Method = HttpMethod.Post;
            req.RequestUri = new Uri(_EnvironmentRequestUrl + setter);
            req.Headers.Add("Authorization", "Bearer " + _AccessToken);
            req.Content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");

            //Make the request
            HttpClient hc = new HttpClient();
            HttpResponseMessage resp = await hc.SendAsync(req);
            string cont = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode != HttpStatusCode.NoContent)
            {
                throw new Exception("Unable to create new record of type '" + setter + "'. Content: " + cont);
            }
        }

        public async Task DeleteAsync(string setter, Guid id)
        {
            //Construct the endpoint
            string ep = setter + "(" + id.ToString() + ")";

            //Construct the reuqest
            HttpRequestMessage req = new HttpRequestMessage();
            req.Method = HttpMethod.Delete;
            req.RequestUri = new Uri(_EnvironmentRequestUrl + ep);
            req.Headers.Add("Authorization", "Bearer " + _AccessToken);
            
            //Make the request
            HttpClient hc = new HttpClient();
            HttpResponseMessage msg = await hc.SendAsync(req);
            if (msg.StatusCode != HttpStatusCode.NoContent)
            {
                string ermsg = await msg.Content.ReadAsStringAsync();
                throw new Exception("Request to delete record '" + id + "' of type '" + setter + "' failed with the following message: " + ermsg);
            }
        }
    
        public async Task UpdateAsync(string setter, Guid id, JObject json)
        {
            //Construct the endpoint
            string ep = _EnvironmentRequestUrl + setter + "(" + id + ")";

            //Construct the request
            HttpRequestMessage req = new HttpRequestMessage();
            req.Method = new HttpMethod("PATCH");
            req.RequestUri = new Uri(ep);
            req.Headers.Add("Authorization", "Bearer " + _AccessToken);
            req.Content = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            
            //Make the request
            HttpClient hc = new HttpClient();
            HttpResponseMessage msg = await hc.SendAsync(req);
            if (msg.StatusCode != HttpStatusCode.NoContent)
            {
                string cont = await msg.Content.ReadAsStringAsync();
                throw new Exception("The update record request of type '" + setter + "' and ID '" + id + "' failed. Message content: " + cont);
            }
        }  
        
        public string EnvironmentRequestUrl
        {
            get
            {
                return _EnvironmentRequestUrl;
            }
        }

        public string AccessToken
        {
            get
            {
                return _AccessToken;
            }
        }
    
        public HttpRequestMessage PrepareRequestMsg()
        {
            //Construct the request
            HttpRequestMessage req = new HttpRequestMessage();
            req.Headers.Add("Authorization", "Bearer " + _AccessToken);
            return req;
        }
    }
}