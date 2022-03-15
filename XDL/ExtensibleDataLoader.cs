using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Xsl;
using CSLog;
using Dynamitey;

namespace XDL
{
    public class ExtensibleDataLoader
    {
        public static string CurrentFileName { get; internal set; }
    }

    public class ExtensibleDataLoader<T> : ExtensibleDataLoader where T : new()
    {
        public static List<T> LoadFile(string path, IDataSchema<T> schema)
        {
            var type = typeof(T);

            var objects = new List<T>();

            using (var fs = new FileStream(path, FileMode.Open))
            {
                var bytes = new byte[fs.Length];

                fs.Read(bytes, 0, bytes.Length);

                var contents = Encoding.UTF8.GetString(bytes).ReplaceAllVariables();

                var xdoc = XDocument.Parse(contents);

                var xsltMarkup = @"<?xml version=""1.0"" encoding=""ISO-8859-1""?>
                        <xsl:stylesheet version=""1.0"" xmlns:xsl=""http://www.w3.org/1999/XSL/Transform"">
                        <xsl:template match=""*"">
                        <xsl:copy>
                        <xsl:copy-of select=""@*""/>
                        <xsl:apply-templates/>
                        </xsl:copy>
                        </xsl:template>
                        <xsl:template match=""subdata"">
                        <xsl:apply-templates/>
                        </xsl:template>
                        </xsl:stylesheet>";

                var doc = new XDocument();

                using (var writer = doc.CreateWriter())
                {
                    var xslt = new XslCompiledTransform();
                    using (var sr = new StringReader(xsltMarkup))
                    {
                        xslt.Load(XmlReader.Create(sr));
                    }

                    xslt.Transform(xdoc.CreateReader(), writer);
                }

                var root = doc.Root;

                if (root?.Name.LocalName != "data")
                {
                    Log.LogMessage("xdl", LogLevel.Warning, "Invalid XDF data file.");
                    return null;
                }

                var typeAttr = root.Attribute("type")?.Value;

                if (typeAttr != schema.DataType)
                {
                    Log.LogMessage("xdl", LogLevel.Warning, "Given XDF file does not match given schema.");
                    return null;
                }

                foreach (var elem in root.Elements())
                {
                    var obj = new T();

                    foreach (var attr in elem.Attributes())
                    {
                        var key = schema.GetKeyForAttribute(attr.Name.LocalName, elem.Name.LocalName);

                        if (key == null)
                            continue;

                        var del = schema.GetLoadHandlerForKey(key);

                        if (del != null)
                        {
                            var ret = del(attr);

                            if (key == "self")
                                obj = (T)ret;
                            else
                                Dynamic.InvokeSet(obj, key, ret);
                        }
                        else
                        {
                            if (key == "self" && typeof(T) == typeof(string))
                                obj = (T)(object)attr.Value;
                            else if (key != "self")
                                Dynamic.InvokeSet(obj, key, attr.Value);
                            else
                                Log.LogMessage("xdl", LogLevel.Warning,
                                               "Tried to assign self key without load handler to non-string type.");
                        }
                    }

                    foreach (var child in elem.Elements())
                    {
                        var key = schema.GetKeyForTag(child.Name.LocalName);

                        if (key == null)
                            continue;

                        var del = schema.GetLoadHandlerForKey(key);

                        if (del != null)
                        {
                            var ret = del(child);

                            if (key == "self")
                                obj = (T)ret;
                            else
                                Dynamic.InvokeSet(obj, key, ret);
                        }
                        else
                        {
                            if (key == "self" && typeof(T) == typeof(string))
                                obj = (T)(object)child.Value;
                            else if (key != "self")
                                Dynamic.InvokeSet(obj, key, child.Value);
                            else
                                Log.LogMessage("xdl", LogLevel.Warning,
                                               "Tried to assign self key without load handler to non-string type.");
                        }

                        foreach (var attr in child.Attributes())
                        {
                            var akey = schema.GetKeyForAttribute(attr.Name.LocalName, elem.Name.LocalName);

                            if (akey == null)
                                continue;

                            var adel = schema.GetLoadHandlerForKey(akey);

                            if (adel != null)
                            {
                                var ret = adel(attr);

                                if (akey == "self")
                                    obj = (T)ret;
                                else
                                    Dynamic.InvokeSet(obj, akey, ret);
                            }
                            else
                            {
                                if (akey == "self" && typeof(T) == typeof(string))
                                    obj = (T)(object)attr.Value;
                                else if (akey != "self")
                                    Dynamic.InvokeSet(obj, akey, attr.Value);
                                else
                                    Log.LogMessage("xdl", LogLevel.Warning,
                                                   "Tried to assign self key without load handler to non-string type.");
                            }
                        }
                    }

                    objects.Add(obj);
                }
            }

            return objects;
        }
    }
}