using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace TimHanewich.Dataverse.Metadata
{
    public class Choice
    {
        public string Name {get; set;}
        public string DisplayName {get; set;}
        public Guid MetadataId {get; set;}
        public ChoiceOption[] Options {get; set;}

        public static Choice ParseJsonFromApi(string json)
        {
            Choice ToReturn = new Choice();
            JObject jo = JObject.Parse(json);

            ToReturn.Name = jo.Property("Name").Value.ToString();
            ToReturn.DisplayName = CdsServiceMetadataExtension.GetLocalizedLabel(jo, "DisplayName");
            ToReturn.MetadataId = Guid.Parse(jo.Property("MetadataId").Value.ToString());
            
            //Options
            List<ChoiceOption> Options = new List<ChoiceOption>();
            JProperty prop_Options = jo.Property("Options");
            if (prop_Options != null)
            {
                JArray ja = JArray.Parse(prop_Options.Value.ToString());
                foreach (JObject sjo in ja)
                {
                    ChoiceOption co = ChoiceOption.ParseJsonFromApi(sjo.ToString());
                    Options.Add(co);
                }
            }
            ToReturn.Options = Options.ToArray();
            

            return ToReturn;
        }
    } 
}