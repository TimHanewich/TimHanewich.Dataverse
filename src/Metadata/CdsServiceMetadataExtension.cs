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

                //Collection Display Name
                s.DisplayCollectionName = GetLocalizedLabel(oo, "DisplayCollectionName");

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



        # region "one to many relationships"

        //Get one to many relationships where this entity is pointing to any other entity ("entities this entity points to")
        public static async Task<OneToManyRelationship[]> GetOneToManyRelationshipsByReferencingEntityAsync(this CdsService service, string entity_logical_name)
        {
            string url = service.ReadEnvironmentRequestUrl() + "RelationshipDefinitions/Microsoft.Dynamics.CRM.OneToManyRelationshipMetadata?$filter=ReferencingEntity eq '" + entity_logical_name + "'";
            HttpClient hc = new HttpClient();
            HttpRequestMessage req = new HttpRequestMessage();
            req.RequestUri = new Uri(url);
            req.Method = HttpMethod.Get;
            req.Headers.Add("Authorization", "Bearer " + service.ReadAccessToken());
            HttpResponseMessage resp = await hc.SendAsync(req);
            string content = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Failure while trying to find one to many relationships by referencing entity '" + entity_logical_name + "': " + content);
            }
            JObject jo = JObject.Parse(content);

            return ParseOneToManyRelationships(jo);
        }

        //Get one to many relationships where this entity is being pointed at by any other entity ("entities that point at this entity")
        public static async Task<OneToManyRelationship[]> GetOneToManyRelationshipsByReferencedEntityAsync(this CdsService service, string entity_logical_name)
        {
            string url = service.ReadEnvironmentRequestUrl() + "RelationshipDefinitions/Microsoft.Dynamics.CRM.OneToManyRelationshipMetadata?$filter=ReferencedEntity eq '" + entity_logical_name + "'";
            HttpClient hc = new HttpClient();
            HttpRequestMessage req = new HttpRequestMessage();
            req.RequestUri = new Uri(url);
            req.Method = HttpMethod.Get;
            req.Headers.Add("Authorization", "Bearer " + service.ReadAccessToken());
            HttpResponseMessage resp = await hc.SendAsync(req);
            string content = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Failure while trying to find one to many relationships by referencing entity '" + entity_logical_name + "': " + content);
            }
            JObject jo = JObject.Parse(content);

            return ParseOneToManyRelationships(jo);
        }

        private static OneToManyRelationship[] ParseOneToManyRelationships(JObject api_response)
        {
            JProperty prop_value = api_response.Property("value");
            if (prop_value == null)
            {
                throw new Exception("Unable to parse OneToMany relationships from API response");
            }
            JArray value = (JArray)prop_value.Value;
            List<OneToManyRelationship> ToReturn = new List<OneToManyRelationship>();
            foreach (JObject jo in value)
            {
                OneToManyRelationship rel = new OneToManyRelationship();

                //ReferencedAttribute
                JProperty prop_ReferencedAttribute = jo.Property("ReferencedAttribute");
                if (prop_ReferencedAttribute != null)
                {
                    rel.ReferencedAttribute = prop_ReferencedAttribute.Value.ToString();
                }

                //ReferencedEntity
                JProperty prop_ReferencedEntity = jo.Property("ReferencedEntity");
                if (prop_ReferencedEntity != null)
                {
                    rel.ReferencedEntity = prop_ReferencedEntity.Value.ToString();
                }

                //ReferencingAttribute
                JProperty prop_ReferencingAttribute = jo.Property("ReferencingAttribute");
                if (prop_ReferencedEntity != null)
                {
                    rel.ReferencingAttribute = prop_ReferencingAttribute.Value.ToString();
                }

                //ReferencingEntity
                JProperty prop_ReferencingEntity = jo.Property("ReferencingEntity");
                if (prop_ReferencedEntity != null)
                {
                    rel.ReferencingEntity = prop_ReferencingEntity.Value.ToString();
                }

                //IsCustomAttribute
                JProperty prop_IsCustomRelationship = jo.Property("IsCustomRelationship");
                if (prop_IsCustomRelationship != null)
                {
                    rel.IsCustomRelationship = Convert.ToBoolean(prop_IsCustomRelationship.Value.ToString());
                }

                ToReturn.Add(rel);
            }
            return ToReturn.ToArray();
        }

        # endregion

        # region "many to many relationships"

        public static async Task<ManyToManyRelationship[]> GetManyToManyRelationshipsAsync(this CdsService service, string entity_logical_name)
        {
            string url = service.ReadEnvironmentRequestUrl() + "RelationshipDefinitions/Microsoft.Dynamics.CRM.ManyToManyRelationshipMetadata?$filter=Entity1LogicalName eq '" + entity_logical_name + "' or Entity2LogicalName eq '" + entity_logical_name + "'";
            HttpClient hc = new HttpClient();
            HttpRequestMessage req = new HttpRequestMessage();
            req.RequestUri = new Uri(url);
            req.Method = HttpMethod.Get;
            req.Headers.Add("Authorization", "Bearer " + service.ReadAccessToken());
            HttpResponseMessage resp = await hc.SendAsync(req);
            string content = await resp.Content.ReadAsStringAsync();
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                throw new Exception("Failure while trying to find one to many relationships by referencing entity '" + entity_logical_name + "': " + content);
            }
            JObject jo = JObject.Parse(content);

            //Parse
            JProperty prop_value = jo.Property("value");
            if (prop_value == null)
            {
                throw new Exception("Unable to parse ManyToMany relationships from API response");
            }
            JArray value = (JArray)prop_value.Value;
            List<ManyToManyRelationship> ToReturn = new List<ManyToManyRelationship>();
            foreach (JObject j in value)
            {
                ManyToManyRelationship rel = new ManyToManyRelationship();

                //Entity1LogicalName
                JProperty prop_Entity1LogicalName = j.Property("Entity1LogicalName");
                if (prop_Entity1LogicalName != null)
                {
                    rel.Entity1LogicalName = prop_Entity1LogicalName.Value.ToString();
                }


                //Entity2LogicalName
                JProperty prop_Entity2LogicalName = j.Property("Entity2LogicalName");
                if (prop_Entity2LogicalName != null)
                {
                    rel.Entity2LogicalName = prop_Entity2LogicalName.Value.ToString();
                }

                //IntersectEntityName
                JProperty prop_IntersectEntityName = j.Property("IntersectEntityName");
                if (prop_IntersectEntityName != null)
                {
                    rel.IntersectEntityName = prop_IntersectEntityName.Value.ToString();
                }

                //Entity1IntersectAttribute
                JProperty prop_Entity1IntersectAttribute = j.Property("Entity1IntersectAttribute");
                if (prop_Entity1IntersectAttribute != null)
                {
                    rel.Entity1IntersectAttribute = prop_Entity1IntersectAttribute.Value.ToString();
                }

                //Entity2IntersectAttribute
                JProperty prop_Entity2IntersectAttribute = j.Property("Entity2IntersectAttribute");
                if (prop_Entity2IntersectAttribute != null)
                {
                    rel.Entity2IntersectAttribute = prop_Entity2IntersectAttribute.Value.ToString();
                }

                //IsCustomAttribute
                JProperty prop_IsCustomRelationship = jo.Property("IsCustomRelationship");
                if (prop_IsCustomRelationship != null)
                {
                    rel.IsCustomRelationship = Convert.ToBoolean(prop_IsCustomRelationship.Value.ToString());
                }

                ToReturn.Add(rel);
            }
            return ToReturn.ToArray();
        }

        # endregion


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