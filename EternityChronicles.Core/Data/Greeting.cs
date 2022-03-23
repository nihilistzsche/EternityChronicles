// Greeting.cs in EternityChronicles/EternityChronicles.Core
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