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
using System.Linq;

namespace TimHanewich.Dataverse.Humanization
{
    public static class HumanizationToolkit
    {
        public static async Task<JObject> HumanizeAsync(this DataverseService service, string entity_logical_name, Guid id, int depth = 0)
        {

            //Get the entity metadata for this table
            EntityMetadata emeta = await service.GetEntityMetadataAsync(entity_logical_name);





            //Make a call to get the payload WITH the text values of the option sets
            HttpRequestMessage req = new HttpRequestMessage();
            req.Headers.Add("Authorization", "Bearer " + service.AccessToken);
            req.Method = HttpMethod.Get;
            req.RequestUri = new Uri(service.EnvironmentRequestUrl + emeta.EntitySetName + "(" + id.ToString() + ")");
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


            //Create a "dictionary" of hummanized payloads that can be used to ensure we don't hummanize the same record several times, at least on this one level (save bandwidth, compute time)
            Dictionary<Guid, JObject> HummanizedDict = new Dictionary<Guid, JObject>();


            //Construct a record that we will return
            JObject ToReturn = new JObject();
            foreach (JProperty property in RecordWithChoiceText.Properties())
            {
                if (property.Value.Type != JTokenType.Null) //Only include fields with values
                {
                    
                    //Is it a lookup?
                    if (property.Name.StartsWith("_") && property.Name.EndsWith("_value")) //it is a lookup
                    {
                        foreach (AttributeMetadata ameta in emeta.Attributes)
                        {
                            if (ameta.LogicalName != null && ameta.LogicalName != "" && property.Name.Contains(ameta.LogicalName))
                            {
                                if (depth > 0)
                                {

                                    //Do we have it in the dictionary?
                                    JObject HummanizedRelatedRecord = null;
                                    foreach (KeyValuePair<Guid, JObject> kvp in HummanizedDict)
                                    {
                                        if (kvp.Key == Guid.Parse(property.Value.ToString()))
                                        {
                                            HummanizedRelatedRecord = kvp.Value;
                                        }
                                    }

                                    //If we did not retrieve it from the dictionary, retrieve it from Dataverse
                                    if (HummanizedRelatedRecord == null)
                                    {
                                        HummanizedRelatedRecord = await service.HumanizeAsync(ameta.Targets[0], Guid.Parse(property.Value.ToString()), depth - 1);
                                        HummanizedDict.Add(Guid.Parse(property.Value.ToString()), HummanizedRelatedRecord); //Add it to the dictionary for further use next time, if needed
                                    }

                                    //Add it
                                    ToReturn.Add(ameta.DisplayName, HummanizedRelatedRecord);
                                }
                            }
                        }
                    }
                    else //Everything that is NOT a lookup
                    {
                        foreach (AttributeMetadata ameta in emeta.Attributes) //Try to find the attribute metadata for this property. If we can't find it, move on (skip)!
                        {
                            if (ameta.DisplayName != null) //It has a display name, so it is meant for public consumption (visible to users)
                            {
                                if (ameta.LogicalName == property.Name) //property name matches attribute metadata "logical name"...
                                {
                                    //Is it not one of the automatically disqualified metadata fields?
                                    List<string> ExcludeFields = new List<string>();
                                    ExcludeFields.Add("statecode");
                                    ExcludeFields.Add("statuscode");
                                    ExcludeFields.Add("timezoneruleversionnumber");
                                    ExcludeFields.Add("versionnumber");
                                    if (ExcludeFields.Contains(ameta.LogicalName) == false)
                                    {
                                        if (ameta.AttributeType != AttributeType.Uniqueidentifier)
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
                    }
                }
            }
            
            //Get the metadata for this record
            return ToReturn;
        }
    }
}