using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace TimHanewich.Dataverse.Metadata
{
    public class ChoiceOption
    {
        public long Value {get; set;}
        public string Label {get; set;}

        public static ChoiceOption ParseJsonFromApi(string json)
        {
            ChoiceOption ToReturn = new ChoiceOption();
            JObject jo = JObject.Parse(json);
            
            ToReturn.Value = Convert.ToInt64(jo.Property("Value").Value.ToString());
            ToReturn.Label = CdsServiceMetadataExtension.GetLocalizedLabel(jo, "Label");

            return ToReturn;
        }
    }
}