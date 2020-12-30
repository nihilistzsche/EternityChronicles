using System.IO;
using System.Xml.Linq;
using DragonMUD.Data.Character;
using DMRace = DragonMUD.Data.Race;
using XDL;
using MDK.Master;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace EternityChronicles.Core.Data
{
    public class Race : DMRace,IDataSchema<Race>,IMDKDataSchema
    {        
        public static bool ForcedBootstrap => File.Exists("forcedracebootstrap");

        [BsonId]
        public ObjectId ID { get; set; }
        
        public string DataType => "races";
        
        public string Description { get; set; }
        
        public string GetKeyForTag(string tag)
        {
            switch (tag)
            {
                case "description":
                    return "Description";
                case "statbonuses":
                    return "Bonuses";
                default:
                    return null;
            }
        }

        public string GetKeyForAttribute(string attribute, string tag)
        {
            if (tag != "race") return null;
            switch (attribute)
            {
                    case "name":
                        return "Name";
                    case "abbr":
                        return "Abbreviation";
                    default:
                        return null;
            }
        }

        public DataLoadHandler GetLoadHandlerForKey(string key)
        {
            if (key == "Bonuses")
                return o => Stat.LoadFromTemplateWithRootElement(o as XElement, Stat.StatLoadType.Race);
            return null;
        }

        public string CollectionName => "Races";
    }
}