using System.IO;
using System.Xml.Linq;
using DragonMUD.Data.Character;
using MDK.Master;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using XDL;
using DMRace = DragonMUD.Data.Race;

namespace EternityChronicles.Core.Data
{
    public class Race : DMRace, IDataSchema<Race>, IMDKDataSchema
    {
        public static bool ForcedBootstrap => File.Exists("forcedracebootstrap");

        public string Description { get; set; }

        public string DataType => "races";

        public string GetKeyForTag(string tag)
        {
            return tag switch
            {
                "description" => "Description",
                "statbonuses" => "Bonuses",
                _             => null
            };
        }

        public string GetKeyForAttribute(string attribute, string tag)
        {
            if (tag != "race") return null;
            return attribute switch
            {
                "name" => "Name",
                "abbr" => "Abbreviation",
                _      => null
            };
        }

        public DataLoadHandler GetLoadHandlerForKey(string key)
        {
            if (key == "Bonuses")
                return o => Stat.LoadFromTemplateWithRootElement(o as XElement, Stat.StatLoadType.Race);
            return null;
        }

        [BsonId] public ObjectId ID { get; set; }

        public string CollectionName => "Races";
    }
}