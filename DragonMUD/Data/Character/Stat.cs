// Stat.cs in EternityChronicles/DragonMUD
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Dynamitey;
using XDL;

namespace DragonMUD.Data.Character
{
    public class Stat : BaseObject, ICustomLoader
    {
        [Flags]
        public enum StatCopySettings
        {
            None,
            Name,
            Value,
            Allocatable,
            Changeable,
            AllocationEngine = Allocatable | Changeable,
            AllExceptName = Value | AllocationEngine,
            All = Name | AllExceptName
        }

        public enum StatLoadType
        {
            Default,
            Race,
            Job,
            Allocation,
            Save
        }

        public Stat() : this(null, 0)
        {
        }

        public Stat(string name, int value) : this(name, null, value)
        {
        }

        public Stat(string name, string abbr) : this(name, abbr, 0)
        {
        }

        public Stat(string name, string abbr, int value)
        {
            Name = name;
            Abbreviation = abbr ?? name;
            Value = value;
            Children = new List<Stat>();
        }

        public int Value { get; set; }

        public string Name { get; set; }

        public string Abbreviation { get; set; }

        public List<Stat> Children { get; set; }

        public int Allocatable { get; set; }

        public bool Changeable { get; set; }

        public bool HasChildren => Children.Any();

        public new Stat this[string path]
        {
            get
            {
                var parts = path.Split('.').ToList();
                if (!parts.Any())
                    return this;

                var child = (from c in Children
                             where string.Equals(c.Name, parts[0], StringComparison.CurrentCultureIgnoreCase) ||
                                   string.Equals(c.Abbreviation, parts[0],
                                                 StringComparison.CurrentCultureIgnoreCase)
                             select c)
                    .FirstOrDefault();

                return child?[string.Join(".", parts.Skip(1))];
            }
            set
            {
                var parts = path.Split('.').ToList();
                if (!parts.Any())
                    CopyStat(value);

                var child = (from c in Children
                             where string.Equals(c.Name, parts[0], StringComparison.CurrentCultureIgnoreCase) ||
                                   string.Equals(c.Abbreviation, parts[0],
                                                 StringComparison.CurrentCultureIgnoreCase)
                             select c)
                    .FirstOrDefault();
                if (child == null)
                {
                    child = new Stat(parts[0], 0);
                    AddChild(child);
                }

                child[string.Join(".", parts.Skip(1))] = value;
            }
        }

        public object CustomLoadObject(XElement element, object context)
        {
            var type = (StatLoadType)context;
            return LoadFromTemplateWithRootElement(element, type);
        }

        public void AddChild(Stat child)
        {
            Children.Add(child);
        }

        public static Stat LoadFromTemplateAtPath(string path)
        {
            return LoadFromTemplateAtPath(path, StatLoadType.Default);
        }

        public static Stat LoadFromTemplateAtPath(string path, StatLoadType loadType)
        {
            using var template = new StreamReader(path);
            return LoadFromTemplateWithData(template.ReadToEnd(), loadType);
        }

        public static Stat LoadFromTemplateUsingXMLDocument(XDocument doc)
        {
            return LoadFromTemplateUsingXMLDocument(doc, StatLoadType.Default);
        }

        public static Stat LoadFromTemplateUsingXMLDocument(XDocument doc, StatLoadType loadType)
        {
            return LoadFromTemplateWithRootElement(doc.Root, loadType);
        }

        public static Stat LoadFromTemplateWithRootElement(XElement root)
        {
            return LoadFromTemplateWithRootElement(root, StatLoadType.Default);
        }

        public static Stat LoadFromTemplateWithRootElement(XElement root, StatLoadType loadType)
        {
            var main = new Stat("main", 0);
            if (root == null)
                return main;

            var statCollection = root.Descendants("statcollection");
            var xElements = statCollection as XElement[] ?? statCollection.ToArray();
            if (!xElements.Any() && loadType != StatLoadType.Race)
                return main;

            var mainElementA = xElements.Where(elem => elem.Attribute("statname")?.Value == "main");

            var mainElement = mainElementA.FirstOrDefault() ?? root;
            if (loadType == StatLoadType.Allocation)
            {
                var attribute = mainElement.Attribute("alloc");
                if (attribute != null)
                    main.Allocatable = int.Parse(attribute.Value);
            }

            var attributeToLookFor = loadType switch
                                     {
                                         StatLoadType.Default => "value",
                                         StatLoadType.Race => "bonus",
                                         StatLoadType.Job => "klassreq",
                                         StatLoadType.Allocation => "alloc",
                                         StatLoadType.Save => "value",
                                         _ => throw new ArgumentOutOfRangeException(nameof(loadType), loadType, null)
                                     };

            Stat GetStat(XElement elem)
            {
                var nameAttribute = elem.Attribute("statname");
                if (nameAttribute == null)
                    return null;

                var statName = nameAttribute.Value;
                var statAbbr = statName;

                var abbrAttribute = elem.Attribute("abbr");
                if (abbrAttribute != null)
                    statAbbr = abbrAttribute.Value;

                var _statvalue = 0;

                var valueAttribute = elem.Attribute(attributeToLookFor);
                if (valueAttribute != null)
                    _statvalue = int.Parse(valueAttribute.Value);

                var stat = new Stat(statName, statAbbr, 0);
                if (loadType == StatLoadType.Allocation)
                {
                    stat.Allocatable = _statvalue;
                    var changeableAttribute = elem.Attribute("changeable");
                    if (changeableAttribute != null)
                        stat.Changeable = bool.Parse(changeableAttribute.Value);
                }
                else
                {
                    stat.Value = _statvalue;
                }

                return stat;
            }

            void LoopStat(Stat parent, IEnumerable<XElement> enumerator)
            {
                foreach (var statCollectionElement in enumerator)
                {
                    var stat = GetStat(statCollectionElement);
                    var directDescendants = statCollectionElement.Descendants("stat");
                    var collectionDescendants = statCollectionElement.Descendants("statcollection");
                    foreach (var descendant in directDescendants) stat.AddChild(GetStat(descendant));

                    var descendants = collectionDescendants as XElement[] ?? collectionDescendants.ToArray();
                    if (descendants.Any())
                        LoopStat(stat, descendants);
                    parent.AddChild(stat);
                }
            }

            var statchildren = mainElement.Descendants("stat");
            var collectionchildren = mainElement.Descendants("statcollection");

            LoopStat(main, collectionchildren);

            foreach (var mainStatChild in statchildren) main.AddChild(GetStat(mainStatChild));

            return main;
        }

        public static Stat LoadFromTemplateWithData(string data)
        {
            return LoadFromTemplateWithData(data, StatLoadType.Default);
        }

        public static Stat LoadFromTemplateWithData(string data, StatLoadType loadType)
        {
            var doc = new XDocument(data);
            if (doc != null)
                return LoadFromTemplateUsingXMLDocument(doc, loadType);
            return null;
        }

        public XElement SaveToXML()
        {
            var mainName = HasChildren ? "statcollection" : "stat";
            var mainElement = new XElement(mainName);
            var nameAttribute = new XAttribute("statname", Name);
            var abbreviationAttribute = new XAttribute("abbr", Abbreviation);
            var valueAttribute = new XAttribute("value", Value);
            mainElement.Add(nameAttribute, abbreviationAttribute, valueAttribute);
            if (!HasChildren) return mainElement;
            foreach (var child in Children) mainElement.Add(child.SaveToXML());

            return mainElement;
        }

        public void CopyStat(Stat stat)
        {
            CopyStat(stat, StatCopySettings.AllExceptName);
        }

        public void CopyStat(Stat stat, StatCopySettings settings)
        {
            if (stat == null)
                return;

            if ((settings & StatCopySettings.Name) != StatCopySettings.None)
            {
                Name = stat.Name;
                Abbreviation = stat.Abbreviation;
            }

            if ((settings & StatCopySettings.Value) != StatCopySettings.None) Value = stat.Value;
            if ((settings & StatCopySettings.Changeable) != StatCopySettings.None) Changeable = stat.Changeable;
            if ((settings & StatCopySettings.Allocatable) != StatCopySettings.None) Allocatable = stat.Allocatable;
            if (!stat.HasChildren) return;
            foreach (var child in stat.Children)
            {
                if (child.Name == null || child.Abbreviation == null)
                    continue;

                var mychild = this[child.Name];
                var toAdd = false;
                if (mychild == null)
                {
                    mychild = Dynamic.InvokeConstructor(child.GetType());
                    toAdd = true;
                }

                mychild.CopyStat(child, settings);
                if (toAdd)
                    AddChild(mychild);
            }
        }
    }
}