using System;
using System.Threading.Tasks;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TimHanewich.Cds
{
    public class CdsAuthenticator
    {
        //Inputs
        public Guid ClientId {get; set;}
        public string Resource {get; set;}
        public string Username {get; set;}
        public string Password {get; set;}

        //Access token
        public string AccessToken {get; set;}
        public DateTime AccessTokenReceivedUtc {get; set;}
        public DateTime AccessTokenExpiresUtc {get; set;}

        public async Task GetAccessTokenAsync()
        {
            //Check inputs are all filled in
            if (ClientId == Guid.Empty)
            {
                throw new Exception("Client ID has not been specified.");
            }
            if (Resource == null || Resource == "")
            {
                throw new Exception("Resource has not been specified.");
            }
            if (Username == null)
            {
                throw new Exception("Username is null!");
            }
            if (Password == null)
            {
                throw new Exception("Password is null!");
            }

            //Set up the request
            HttpRequestMessage req = new HttpRequestMessage();
            req.Method = HttpMethod.Post;
            req.RequestUri = new Uri("https://login.windows.net/common/oauth2/token");

            List<KeyValuePair<string, string>> KVPs = new List<KeyValuePair<string, string>>();
            KVPs.Add(new KeyValuePair<string, string>("grant_type", "password"));
            KVPs.Add(new KeyValuePair<string, string>("client_id", ClientId.ToString()));
            KVPs.Add(new KeyValuePair<string, string>("resource", Resource));
            KVPs.Add(new KeyValuePair<string, string>("username", Username));
            KVPs.Add(new KeyValuePair<string, string>("password", Password));

            //Assemble body
            FormUrlEncodedContent fuec = new FormUrlEncodedContent(KVPs.ToArray());
            string body = await fuec.ReadAsStringAsync();
            req.Content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");

            //Make the call
            HttpClient hc = new HttpClient();
            HttpResponseMessage msg = await hc.SendAsync(req);
            string content = await msg.Content.ReadAsStringAsync();
            if (msg.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Attempt to authenticate using username and password failed with code '" + msg.StatusCode.ToString() + "'. Content: " + content);
            }

            //Extract the content we want
            JObject jo = JObject.Parse(content);
            AccessTokenReceivedUtc = DateTime.UtcNow;

            //Expires in
            JProperty prop_expires_in = jo.Property("expires_in");
            if (prop_expires_in != null)
            {
                if (prop_expires_in.Value.Type != JTokenType.Null)
                {
                    int ExpiresInSeconds = Convert.ToInt32(prop_expires_in.Value.ToString());
                    AccessTokenExpiresUtc = DateTime.UtcNow.AddSeconds(ExpiresInSeconds);
                }
            }

            //Expires in - if the payload also includes the property "expires_on", this is the property that defines specifically when it expires, expressed as a unix time stamp (# of seconds since January 1, 1970)
            //If this is provided, overwrite the calculation from abover
            JProperty prop_expires_on = jo.Property("expires_on");
            if (prop_expires_on != null)
            {
                if (prop_expires_on.Value.Type != JTokenType.Null)
                {
                    int NumberOfSeconds = Convert.ToInt32(prop_expires_on.Value.ToString());
                    DateTime Foundation = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
                    DateTime ExpiresAtFixed = Foundation.AddSeconds(NumberOfSeconds);
                    AccessTokenExpiresUtc = ExpiresAtFixed;
                }
            }

            //Get the access token itself
            JProperty prop_access_token = jo.Property("access_token");
            if (prop_access_token != null)
            {
                if (prop_access_token.Value.Type != JTokenType.Null)
                {
                    AccessToken = prop_access_token.Value.ToString();
                }
            }
        }

    }
}