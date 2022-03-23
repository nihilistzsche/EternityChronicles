// DataManager.cs in EternityChronicles/XDL
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

using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml.Linq;

namespace XDL
{
    public class DataManager
    {
        private readonly Dictionary<string, CustomLoadingContext> _customLoaders = new();

        private readonly Dictionary<string, Dictionary<string, string>> _tagToAttributeReference = new();

        private readonly Dictionary<string, string> _tagToKeyReference = new();

        public void RegisterTagForKey(string tag, string key)
        {
            _tagToKeyReference[tag] = key;
        }

        public void RegisterAttributesForTag(string tag, params (string key, string value)[] attributeToKeyPairs)
        {
            var attributeRefs = new Dictionary<string, string>();

            foreach (var (key, value) in attributeToKeyPairs) attributeRefs[key] = value;

            _tagToAttributeReference[tag] = attributeRefs;
        }

        public void RegisterTagForCustomLoading<T>(string tag, string key, object context)
            where T : ICustomLoader, new()
        {
            _customLoaders[tag] = new CustomLoadingContext { Loader = new T(), Context = context, Key = key };
        }

        public void LoadFromPath<T>(string path, T obj) where T : class
        {
            if (obj == null) return;

            using var fs = new FileStream(path, FileMode.Open);
            var doc = XDocument.Load(fs);

            foreach (var tag in _tagToKeyReference.Keys)
            {
                var elem = doc.Element(tag);

                if (elem == null)
                    continue;

                var ty = typeof(T);
                var fi = ty.GetField(_tagToKeyReference[tag],
                                     BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                fi?.SetValue(obj, elem.Value);
            }

            foreach (var tag in _tagToAttributeReference.Keys)
            {
                if (tag == null) continue;

                var elem = doc.Element(tag);

                if (doc.Root?.Name.LocalName == tag) elem = doc.Root;

                if (elem == null)
                    continue;

                foreach (var attribute in _tagToAttributeReference[tag].Keys)
                {
                    var attr = elem.Attribute(attribute);
                    if (attr == null)
                        continue;

                    var ty = typeof(T);
                    var fi = ty.GetField(_tagToAttributeReference[tag][attribute],
                                         BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                    fi?.SetValue(obj, attr.Value);
                }
            }

            foreach (var tag in _customLoaders.Keys)
            {
                if (tag == null) continue;

                var elem = doc.Element(tag);

                if (doc.Root?.Name.LocalName == tag) elem = doc.Root;

                if (elem == null) continue;

                var cl = _customLoaders[tag];

                var loadedObject = cl.Loader.CustomLoadObject(elem, cl.Context);

                var ty = typeof(T);
                var fi = ty.GetField(cl.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

                fi?.SetValue(obj, loadedObject);
            }
        }

        private class CustomLoadingContext
        {
            public ICustomLoader Loader { get; set; }

            public object Context { get; set; }

            public string Key { get; set; }
        }
    }
}