using System;
using TimHanewich.Dataverse;
using TimHanewich.Dataverse.AdvancedRead;
using TimHanewich.Dataverse.Helpers;
using TimHanewich.Dataverse.Metadata;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace TimHanewich.Dataverse.Humanization
{
    public static class HumanizationToolkit
    {
        public static async Task<JObject> HumanizeAsync(this DataverseService service, string setter, Guid id)
        {

            //Make a call to get the payload WITH the text values of the option sets
            HttpRequestMessage req = new HttpRequestMessage();
            req.Headers.Add("Authorization", "Bearer " + service.AccessToken);
            req.Method = HttpMethod.Get;
            req.RequestUri = new Uri(service.EnvironmentRequestUrl + setter + "(" + id.ToString() + ")");
            req.Headers.Add("Prefer", "odata.include-annotations=\"OData.Community.Display.V1.FormattedValue\""); //this is key for getting option set values as text

            //Request
            HttpClient hc = new HttpClient();
            HttpResponseMessage resp = await hc.SendAsync(req);
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Error while getting selected choice text for ");
            }

            //Parse
            string content = await resp.Content.ReadAsStringAsync();
            JObject RecordWithChoiceText = JObject.Parse(content);





            //Get metadata summaries - we will use these to jump from the setter name "logical collection name" or "entity set name" to the logical name"
            EntityMetadataSummary[] summaries = await service.GetEntityMetadataSummariesAsync();

            //Find the right one that matches the setter provided
            EntityMetadataSummary jumper = null;
            foreach (EntityMetadataSummary summary in summaries)
            {
                if (summary.EntitySetName == setter)
                {
                    jumper = summary;
                }
            }
            if (jumper == null)
            {
                throw new Exception("Unable to find logical name for table with EntitySetName '" + setter + "'");
            }

            //Get the entity metadata for this table
            EntityMetadata emeta = await service.GetEntityMetadataAsync(jumper.LogicalName);






            //Construct a record that we will return
            JObject ToReturn = new JObject();
            foreach (JProperty property in RecordWithChoiceText.Properties())
            {
                if (property.Value.Type != JTokenType.Null) //Only include fields with values
                {
                    foreach (AttributeMetadata ameta in emeta.Attributes) //Try to find the attribute metadata for this property. If we can't find it, move on (skip)!
                    {
                        if (ameta.LogicalName == property.Name) //property name matches attribute metadata "logical name"...
                        {
                            if (ameta.DisplayName != null) //It has a display name, so it is meant for public consumption (visible to users)
                            {
                                //Get property name
                                string NAME = ameta.DisplayName; //use the DISPLAY NAME of the column

                                //add it, depending on the column type
                                if (ameta.AttributeType == AttributeType.Picklist || ameta.AttributeType == AttributeType.Virtual) //option set (choice)
                                {
                                    //Find the text property in that payload
                                    foreach (JProperty prop in RecordWithChoiceText.Properties())
                                    {
                                        if (prop.Name.StartsWith(ameta.LogicalName + "@"))
                                        {
                                            ToReturn.Add(NAME, prop.Value.ToString());
                                        }
                                    }  
                                }
                                else if (ameta.AttributeType == AttributeType.DateTime)
                                {
                                    DateTime dt = DateTime.Parse(property.Value.ToString());
                                    ToReturn.Add(NAME, dt.ToString());
                                }
                                else
                                {
                                    ToReturn.Add(NAME, property.Value);
                                }
                            }
                        }
                    }
                }
                
            }
            
            //Get the metadata for this record
            return ToReturn;
        }
    }
}