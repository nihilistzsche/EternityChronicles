// Race.cs in EternityChronicles/EternityChronicles.Core
// 
// Copyright (C) 2022 Michael Tindal (nihilistzsche AT gmail DOT com)
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//    http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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