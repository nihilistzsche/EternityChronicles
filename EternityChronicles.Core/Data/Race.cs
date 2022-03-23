// Race.cs in EternityChronicles/EternityChronicles.Core
// 
// Copyright (C) 2022 Michael Tindal (nihilistzsche AT gmail DOT com)
// 
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with this program.  If not, see <http://www.gnu.org/licenses/>.

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
                       _ => null
                   };
        }

        public string GetKeyForAttribute(string attribute, string tag)
        {
            if (tag != "race") return null;
            return attribute switch
                   {
                       "name" => "Name",
                       "abbr" => "Abbreviation",
                       _ => null
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