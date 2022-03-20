// Greeting.cs in EternityChronicles/EternityChronicles.Core
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

using XDL;

namespace EternityChronicles.Core.Data
{
    public class Greeting : IDataSchema<Greeting>
    {
        private string _text;

        // ReSharper disable once ConvertToAutoProperty
        public string Text
        {
            get => _text;

            set => _text = value;
        }

        public string DataType => "greeting";

        public string GetKeyForTag(string tag)
        {
            return tag == "text" ? "_text" : null;
        }

        public string GetKeyForAttribute(string attribute, string tag)
        {
            return null;
        }

        public DataLoadHandler GetLoadHandlerForKey(string key)
        {
            return null;
        }
    }
}