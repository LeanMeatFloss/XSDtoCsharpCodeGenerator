using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;

namespace XSDParser
{
    public class StringSimple
    {

    }
    public class Reader
    {
        public static Reader LoadFile(string filePath)
        {
            XElement root = XElement.Load(filePath, LoadOptions.SetLineInfo);
            var attributes = root.Attributes();
            Reader reader = new Reader();
            XAttribute targetNameSpaceUrl = attributes.Where(attr => attr.Name.LocalName.Equals("targetNamespace")).First();
            reader.TargetNameSpace = attributes.Where(attr => !attr.Equals(targetNameSpaceUrl) && attr.Value.Equals(targetNameSpaceUrl.Value)).First();
            reader.XSDNameSpace = attributes.Where(attr => attr.Name.LocalName.Equals("xsd")).First();
            reader.ParserInfo = new XSDReadResult();
            reader.ParserInfo.Name = reader.TargetNameSpace.Name.LocalName;
            reader.ParserInfo.TypeName = "namespace";
            try
            {
                reader.HandlingNode(root, reader.ParserInfo);
            }
            catch
            {

            }


            return reader;
        }
        XAttribute TargetNameSpace { get; set; }
        XAttribute XSDNameSpace { get; set; }
        XSDReadResult ParserInfo { get; set; }
        public string OutputCode()
        {
            StringBuilder sBuilder = new StringBuilder(@"
using System;
using System.Xml;
using System.Xml.Serialization;
");
            OutputCodeChain(ParserInfo, (line) => sBuilder.AppendLine(line));
            return sBuilder.ToString();
        }
        bool TryAppendItem(List<XSDReadResult> container, IEnumerable<string> nameChain, XSDReadResult item)
        {
            if (nameChain.Count() == 1)
            {
                item.QualifiedName = nameChain.First();
                container.Add(item);
                return true;
            }
            else
            {
                var subContainer = container.Where(ele => ele.QualifiedName.Equals(nameChain.First())).FirstOrDefault();
                if (subContainer != null)
                {
                    return TryAppendItem(subContainer.SubElement, nameChain.Skip(1), item);
                }
                else
                {
                    return false;
                }
            }
        }
        public class CompareRawClass<T> : IEqualityComparer<T>
        {
            public CompareRawClass(Func<T, T, bool> equality, Func<T, int> getHash)
            {
                Equality = equality;
                GetHash = getHash;
            }
            Func<T, T, bool> Equality { get; set; }
            Func<T, int> GetHash { get; set; }
            public bool Equals(T x, T y)
            {
                return Equality(x, y);
            }

            public int GetHashCode(T obj)
            {
                return GetHash(obj);
            }
        }
        int CallLevel = 0;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="container"></param>
        /// <param name="appendLineAction"></param>
        public void OutputCodeChain(XSDReadResult container, Action<string> appendLineAction)
        {

            CallLevel++;
            string[] prefix = new string[CallLevel];
            for (int i = 0; i < CallLevel; i++)
            {
                prefix[i] = "\t";
            }

            Debug.WriteLine($"{string.Join("", prefix)}{container.Name}");
            //add comment
            if (!string.IsNullOrEmpty(container.Comment))
            {
                appendLineAction("///<summary>");
                foreach (var item in container.Comment.Split(new string[] { "\n" }, StringSplitOptions.RemoveEmptyEntries))
                {
                    appendLineAction("///" + item);
                }
                appendLineAction("///</summary>");
            }
            //add attribute
            foreach (var item in container.HeaderAttributes)
            {
                string attributeline = $"[{item.Name}({string.Join(",", item.ConfigurationParams.Select(ele => ele.Key + "=" + ele.Value))})]";
                appendLineAction(attributeline);
            }



            string proceedType = container.TypeName;
            string proceedName = container.Name;
            if (string.IsNullOrEmpty(proceedType))
            {
                appendLineAction($"{proceedName},");
            }
            else
            {
                string inheritInfo = "";
                if (container.InheritInfo.Count > 0)
                {
                    inheritInfo = ":" + string.Join(",", container.InheritInfo);
                }
                appendLineAction(string.Format(
                    "{0}{1}{2}",
                (string.IsNullOrEmpty(container.Prefix) ? "" : (container.Prefix + " ")),
                    ((string.IsNullOrEmpty(proceedType) ? "" : (proceedType + (container.IsArray ? "[]" : "") + " "))),
                    proceedName + inheritInfo));
            }

            var subElementList = container.RefSubElementsList.SelectMany(refList => refList).Concat(container.SubElement).GroupBy(item => item.Name).OrderBy(item => item.Key);
            if (subElementList.Count() > 0)
            {
                appendLineAction("{");
                foreach (var item in subElementList)
                {
                    //create a new composite for this
                    var newInput = new XSDReadResult()
                    {
                        Name = item.Key,
                        ArrayItemTypeList = item.SelectMany(ele => ele.ArrayItemTypeList).ToList(),
                        Comment = string.Join("\n", item.Select(ele => ele.Comment).Distinct().Where(ele => !string.IsNullOrEmpty(ele))),
                        HeaderAttributes = item.SelectMany(ele => ele.HeaderAttributes)
                            .Distinct(new CompareRawClass<AttributeClass>((in1, in2) => in1.Name.Equals(in2.Name), (in1) => in1.Name.GetHashCode()))
                            .ToList(),
                        InheritInfo = item.SelectMany(ele => ele.InheritInfo).Distinct().ToList(),
                        IsArray = item.Select(ele => ele.IsArray ? 1 : 0).Sum() > 0,
                        Prefix = item.OrderBy(ele => string.IsNullOrEmpty(ele.Prefix) ? 1 : 0).First().Prefix,
                        SubElement = item.SelectMany(ele => ele.SubElement).ToList(),
                        RefSubElementsList = item.Where(ele => ele.RefSubElementsList.Count > 0).SelectMany(ele => ele.RefSubElementsList).ToList(),
                        TypeName = item.OrderBy(ele => string.IsNullOrEmpty(ele.TypeName) ? 1 : 0).First().TypeName,
                    };
                    OutputCodeChain(newInput, (line) =>
                    {
                        appendLineAction("\t" + line);
                    });
                }
                appendLineAction("}");
            }
            else if (string.IsNullOrEmpty(container.TypeName))
            {
            }
            else if (container.TypeName.Equals("class"))
            {
                appendLineAction("{");
                appendLineAction("}");
            }

            else
            {
                appendLineAction("{");
                appendLineAction("\tget;");
                appendLineAction("\tset;");
                appendLineAction("}");
            }
            CallLevel--;
        }
        void HandlingNode(XElement node, XSDReadResult container)
        {
            try
            {
                HandlingNodeRaw(node, container);
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Error on dealing with xsd node {node.Name.LocalName} at line {((IXmlLineInfo)node).LineNumber}");
                throw e;
            }
        }
        void HandlingNodeRaw(XElement node, XSDReadResult container)
        {
            foreach (var element in node.Elements())
            {
                if (element.Name.Namespace.NamespaceName.Equals(XSDNameSpace.Value))
                {
                    switch (element.Name.LocalName)
                    {
                        case "group":
                            {
                                string typeName, name, rootType;
                                if (element.Attribute("ref") != null)
                                {
                                    //get type name
                                    typeName = GetNodeRef(element);
                                    //get type declaration
                                    var item = ParserInfo.GetElementOrCreate("public", "class", typeName, XSDReadResult.XSDInstanceType.Group);
                                    //connect sub elements to current
                                    container.AppendRef(item.SubElement);
                                }
                                else
                                {
                                    typeName = "class";
                                    rootType = "XmlRoot";
                                    name = GetNodeName(element);
                                    var item = container.GetElementOrCreate("public", typeName, name, XSDReadResult.XSDInstanceType.Group);
                                    if (element.Attribute("ref") == null)
                                    {
                                        item.GetOrAddAttribute(rootType).ConfigurationParams["ElementName"] = $"\"{element.Attribute("name").Value}\"";
                                    }

                                    //Sub element checking
                                    HandlingNode(element, item);
                                }

                            }
                            break;
                        case "annotation":
                            {
                                HandlingNode(element, container);
                            }
                            break;

                        case "documentation":
                            container.Comment = element.Value;
                            break;
                        case "appinfo":
                            switch (element.Attribute("source").Value)
                            {
                                case "tags":
                                    {
                                        string regularExpression = "mmt\\.qualifiedName=\\\"([0-9A-Z.a-z_]*)\\\"";
                                        Match matchRes = Regex.Match(element.Value, regularExpression);
                                        if (matchRes.Success)
                                        {
                                            container.QualifiedName = matchRes.Groups[1].Value;
                                        }
                                    }
                                    break;
                                case "stereotypes":
                                    {
                                        //container.InheritInfo.Add(element.Value);
                                    }
                                    break;
                            }
                            break;
                        case "simpleType":
                            {
                                //Get name
                                string name = GetNodeName(element);
                                string typeName = "class";
                                XSDReadResult item = container.GetElementOrCreate("public", typeName, name, XSDReadResult.XSDInstanceType.SimpleType);
                                item.GetOrAddAttribute("XmlRoot").ConfigurationParams["ElementName"] = $"\"{element.Attribute("name").Value}\"";
                                HandlingNode(element, item);
                            }
                            break;
                        case "restriction":
                            {
                                string typeName = GetNodeBase(element);
                                container.InheritInfo.Add(typeName);
                                HandlingNode(element, container);
                            }
                            break;
                        case "pattern":
                            {
                                //add comment
                                container.Comment += "\npattern:" + element.Attribute("value").Value + "\n";

                            }
                            break;
                        case "enumeration":
                            {
                                //this type should be a enum
                                container.TypeName = "enum";
                                container.InheritInfo.Clear();
                                XSDReadResult enumItem = new XSDReadResult()
                                {
                                    Name = NameValueConverter(element.Attribute("value").Value),
                                };
                                enumItem.GetOrAddAttribute("XmlEnum").ConfigurationParams["Name"] = $"\"{element.Attribute("value").Value}\"";
                                container.SubElement.Add(enumItem);
                            }
                            break;
                        case "sequence":
                            {
                                //string name = GetNodeName(element);
                                //string typeName = "class";                                
                                //var item = container.GetElementOrCreate("public", "class", name);
                                HandlingNode(element, container);
                            }
                            break;
                        case "element":
                            {
                                int maxOccurs = -1;
                                int minOccurs = -1;
                                if (element.Attribute("maxOccurs") != null)
                                {
                                    if (int.TryParse(element.Attribute("maxOccurs").Value, out int value))
                                    {
                                        maxOccurs = value;
                                    }
                                }
                                if (element.Attribute("minOccurs") != null)
                                {
                                    if (int.TryParse(element.Attribute("minOccurs").Value, out int value))
                                    {
                                        minOccurs = value;
                                    }
                                }
                                //Get name
                                string name = GetNodeName(element);
                                string typeName = "class";
                                if (element.Attribute("type") != null)
                                {
                                    typeName = GetNodeType(element);
                                }
                                else
                                {


                                }

                                if (container.IsArray)
                                {
                                    if (typeName.Equals("class"))
                                    {
                                        //class type need futher declaration
                                        //top level
                                        var item = ParserInfo.GetElementOrCreate("public", "class", name, XSDReadResult.XSDInstanceType.Element);
                                        item.UpdateHeaderAttributes("XmlRoot", new Dictionary<string, string>()
                                        {
                                            { "ElementName", $"\"{element.Attribute("name").Value}\"" }
                                        });
                                        HandlingNode(element, item);
                                        typeName = name;
                                    }
                                    container.HeaderAttributes.Add(new AttributeClass()
                                    {
                                        Name = "XmlArrayItem",
                                        ConfigurationParams = new Dictionary<string, string>()
                                    {
                                        {"ElementName",$"\"{element.Attribute("name").Value}\"" },
                                        {"Type",$"typeof({typeName})" }
                                    }
                                    });
                                    container.ArrayItemTypeList.Add(typeName);
                                }
                                else
                                {
                                    XSDReadResult item;
                                    //if it is not a array, need to detect which level
                                    if (container.Equals(ParserInfo))
                                    {
                                        //top level
                                        item = container.GetElementOrCreate("public", "class", name, XSDReadResult.XSDInstanceType.Element);
                                        item.UpdateHeaderAttributes("XmlRoot", new Dictionary<string, string>()
                                        {
                                            { "ElementName", $"\"{element.Attribute("name").Value}\"" }
                                        });
                                        HandlingNode(element, item);
                                    }
                                    else
                                    {
                                        //second level
                                        item = container.GetElementOrCreate("public", typeName, name, XSDReadResult.XSDInstanceType.Element);
                                        item.UpdateHeaderAttributes("XmlElement", new Dictionary<string, string>()
                                        {
                                            { "ElementName", $"\"{element.Attribute("name").Value}\"" }
                                        });
                                        HandlingNode(element, item);
                                        //if typename not specified aka equals class :
                                        // this element can be 1.array,2.inherit element
                                        if (typeName.Equals("class"))
                                        {
                                            if (item.IsArray)
                                            {
                                                // do nothing
                                            }
                                            else
                                            {
                                                //not a array,migrate to top level
                                                container.SubElement.Remove(item);
                                                ParserInfo.SubElement.Add(item);
                                            }
                                        }
                                    }

                                }
                            }
                            break;
                        case "complexType":
                            {
                                string typeName, name, rootType;
                                if (element.Attribute("ref") != null)
                                {
                                    //get type name
                                    typeName = GetNodeRef(element);
                                    //get type declaration
                                    var item = container.GetElementOrCreate("public", "class", typeName, XSDReadResult.XSDInstanceType.ComplexType);
                                    //connect sub elements to current
                                    container.AppendRef(item.SubElement);
                                }
                                else if (element.Attribute("name") != null)
                                {
                                    typeName = "class";
                                    rootType = "XmlRoot";
                                    name = GetNodeName(element);
                                    var item = container.GetElementOrCreate("public", typeName, name, XSDReadResult.XSDInstanceType.ComplexType);
                                    if (element.Attribute("ref") == null)
                                    {
                                        item.GetOrAddAttribute(rootType).ConfigurationParams["ElementName"] = $"\"{element.Attribute("name").Value}\"";
                                    }

                                    //Sub element checking
                                    HandlingNode(element, item);
                                }
                                else
                                {
                                    //Sub element checking
                                    HandlingNode(element, container);
                                }
                            }
                            break;
                        case "choice":
                            {
                                //if max Occurs>1
                                if (element.Attribute("maxOccurs") != null && int.TryParse(element.Attribute("maxOccurs").Value, out int maxOccurs) && maxOccurs == 1)
                                {

                                }
                                else
                                {
                                    var subElements = element.Elements().Select(ele => ele.Name.LocalName).Distinct();
                                    if(subElements.Count()==1)
                                    {
                                        switch(subElements.First())
                                        {
                                            case "element":
                                                container.IsArray = true;
                                                //change parent to array
                                                container.GetOrAddAttribute("XmlElement").Name = "XmlArray";
                                                break;
                                            case "group":
                                                //group reference type
                                                //continue handling
                                                break;
                                            case "choice":
                                                //choice reference type
                                                //continue handling
                                                break;
                                            default:
                                                Debug.WriteLine($"Not handling {subElements.First()} at line {((IXmlLineInfo)element).LineNumber} currently");
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        //multi elements
                                        foreach(var subElement in element.Elements())
                                        {
                                            switch(subElement.Name.LocalName)
                                            {
                                                case "element":
                                                    container.IsArray = true;
                                                    //change parent to array
                                                    container.GetOrAddAttribute("XmlElement").Name = "XmlArray";
                                                    break;
                                                case "group":
                                                    //group reference type
                                                    //continue handling
                                                    break;
                                                case "choice":
                                                    //choice reference type
                                                    //continue handling
                                                    break;
                                                default:
                                                    Debug.WriteLine($"Not handling combined choice object {subElements.First()} at line {((IXmlLineInfo)element).LineNumber} currently");
                                                    break;
                                            }
                                        }
                                        string info = string.Join(",", subElements);
                                        Debug.WriteLine($"Not handling combained {info} at line {((IXmlLineInfo)element).LineNumber} currently");
                                    }
                                    
                                }
                                HandlingNode(element, container);
                                if (container.IsArray && container.SubElement.Count == 0)
                                {
                                    if (container.ArrayItemTypeList.Count() == 1)
                                    {
                                        container.TypeName = container.ArrayItemTypeList.First();
                                    }
                                    else
                                    {
                                        container.TypeName = "object";
                                    }
                                }

                            }
                            break;
                        case "attribute":
                            {
                                string typeName, name, rootType;
                                if (element.Attribute("ref") != null)
                                {
                                    //get type name                                   
                                    typeName = GetNodeRef(element);
                                    name = typeName;
                                }
                                else
                                {
                                    //Get name
                                    name = GetNodeName(element);
                                    //Get type
                                    typeName = GetNodeType(element);
                                }

                                var item = container.GetElementOrCreate("public", typeName, name, XSDReadResult.XSDInstanceType.Attribute);
                                if (element.Attribute("ref") == null)
                                {
                                    item.UpdateHeaderAttributes("XmlAttribute", new Dictionary<string, string>() { { "AttributeName", $"\"{element.Attribute("name").Value}\"" } });
                                }

                                HandlingNode(element, item);
                            }
                            break;
                        case "simpleContent":
                            {
                                HandlingNode(element, container);
                            }
                            break;
                        case "extension":
                            {
                                string baseType = GetNodeBase(element);
                                //get new item in base
                                var item = ParserInfo.GetElementOrCreate("public", "class", container.Name, XSDReadResult.XSDInstanceType.Extension);
                                item.InheritInfo.Add(baseType);
                                //container.InheritInfo.Add(baseType);
                                HandlingNode(element, item);
                            }
                            break;
                        case "attributeGroup":
                            {

                                string typeName, name;
                                if (element.Attribute("ref") != null)
                                {
                                    //get type name
                                    typeName = GetNodeRef(element);
                                    //get type declaration
                                    var item = container.GetElementOrCreate("public", "class", typeName, XSDReadResult.XSDInstanceType.AttributeGroup);
                                    //connect sub elements to current
                                    container.AppendRef(item.SubElement);
                                }
                                else
                                {
                                    //Get name
                                    name = GetNodeName(element);
                                    var item = container.GetElementOrCreate("public", "class", name, XSDReadResult.XSDInstanceType.AttributeGroup);
                                    HandlingNode(element, item);
                                }
                            }
                            break;
                        default:
                            Trace.WriteLine($"Currently not dealing with xsd node {element.Name.LocalName} at line {((IXmlLineInfo)element).LineNumber}");
                            break;
                    }
                }
                else
                {

                }
            }
        }
        string GetNodeRef(XElement node)
        {
            string refString = node.Attribute("ref").Value;
            if (refString.StartsWith("xml:"))
            {
                refString.Replace(":", "-");
            }
            return NameValueConverter(node.Attribute("ref").Value.Replace($"{TargetNameSpace.Name.LocalName}:", ""));
        }
        string GetNodeBase(XElement node)
        {
            return NameValueConverter(node.Attribute("base").Value.Replace($"{TargetNameSpace.Name.LocalName}:", ""));
        }
        string GetNodeType(XElement node)
        {
            return NameValueConverter(node.Attribute("type").Value.Replace($"{TargetNameSpace.Name.LocalName}:", ""));
        }
        string GetNodeName(XElement node)
        {
            //handling appinfo
            return NameValueConverter(node.Attribute("name").Value);
        }
        string ConvertToHumpNaming(string Name)
        {
            return $"{Name.First().ToString().ToUpper()}{Name.Substring(1).ToLower()}";
        }
        string NameValueConverter(string Name)
        {
            return string.Join(
                "",
                Name.Replace("xsd:", "")
                .Split(
                        new string[] { "--" },
                        StringSplitOptions.RemoveEmptyEntries
                        )
                .Select(item =>
                {
                    return string
                    .Join(
                        "",
                        item
                        .Split(
                            new string[] { "-" },
                            StringSplitOptions.RemoveEmptyEntries
                            )
                        .Select(ConvertToHumpNaming)
                        );
                }));

        }
    }
}
