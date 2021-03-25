using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Linq;
using System.Diagnostics;

namespace XSDParser
{
    public class SchemaBasedParser
    {
        public void ParserFile(string[] dependencyFilePath, string targetFilePath, string rootObject)
        {
            SchemaSet = new XmlSchemaSet();
            Schema = null;
            foreach (var filePath in dependencyFilePath)
            {
                using (FileStream fs = File.OpenRead(filePath))
                {
                    var schema = XmlSchema.Read(fs, null);
                    SchemaSet.Add(schema);

                }
            }
            using (FileStream fs = File.OpenRead(targetFilePath))
            {
                Schema = XmlSchema.Read(fs, null);
                SchemaSet.Add(Schema);

            }
            SchemaSet.Compile();


            MainContainer = new ElementDecalaration()
            {
                TypeName = "namespace",
                NeedPrefix=false,
                Name = NameConverter(Schema.Namespaces.ToArray().Where(ele => ele.Namespace.Equals(Schema.TargetNamespace)).First().Name),
                ElementType = ElementDecalaration.XmlElementTypeEnum.Root,
            };
            //searching for element~~~
            var types = SchemaSet.GlobalTypes[new XmlQualifiedName(rootObject, Schema.TargetNamespace)];
            HandlingTypes(MainContainer, types);


        }
        public string GetCode()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(@"
using System;
using System.Collections.Generic;
using System.Xml.Serialization;");
            GetResult(MainContainer,null, (line) => stringBuilder.AppendLine(line));
            return stringBuilder.ToString();
        }
        void GetResult(ElementDecalaration container,ElementDecalaration parentContainer, Action<string> appendLineAction)
        {
            StringBuilder line = new StringBuilder();
            ElementDecalaration oldContainer = container;
            //need to specified in container at top level
            bool topLevel = false;
            if (MainContainer.SubElements.Where(ele => ele.Value.Equals(container)).Count() != 0)
            {
                topLevel = true;
            }
            else
            {

                if (container.ElementType == ElementDecalaration.XmlElementTypeEnum.Element)
                {
                    var elementFiltered = MainContainer.SubElements.Select(ele => ele.Value).Where(ele => ele.Name.Equals(container.TypeName)).FirstOrDefault();
                    if (elementFiltered != null && elementFiltered.ElementType == ElementDecalaration.XmlElementTypeEnum.Array)
                    {
                        //use the array definitions
                        container = elementFiltered;
                    }
                }
            }
            bool useAttribute = true;
            switch (container.ElementType)
            {
                case ElementDecalaration.XmlElementTypeEnum.Element:
                    line.Append("[XmlElement");
                    break;
                case ElementDecalaration.XmlElementTypeEnum.Array:
                    if(!topLevel)
                    {
                        line.Append("[XmlArray");
                    }                  
                    else
                    {
                        useAttribute = false;
                    }
                    break;
                case ElementDecalaration.XmlElementTypeEnum.ArrayItem:
                    line.Append("[XmlArrayItem");
                    break;
                case ElementDecalaration.XmlElementTypeEnum.XmlText:
                    line.Append("[XmlText");
                    break;
                default:
                    useAttribute = false;
                    break;
            }
            if(useAttribute)
            {
                if (oldContainer.Attributes.Count > 0 && oldContainer.ElementType != ElementDecalaration.XmlElementTypeEnum.NotXmlElement)
                {
                    line.Append("(");
                    foreach (var attribute in oldContainer.Attributes)
                    {
                        line.Append($"{attribute.Key}={attribute.Value},");
                    }
                    line.Remove(line.Length - 1, 1);
                    line.Append(")");
                }
                switch (container.ElementType)
                {
                    case ElementDecalaration.XmlElementTypeEnum.Element:
                        line.Append("]");
                        break;
                    case ElementDecalaration.XmlElementTypeEnum.Array:
                        line.Append("]");
                        break;
                    case ElementDecalaration.XmlElementTypeEnum.ArrayItem:
                        line.Append("]");
                        break;
                    case ElementDecalaration.XmlElementTypeEnum.XmlText:
                        line.Append("]");
                        break;
                }
                if (line.Length > 0)
                {
                    appendLineAction(line.ToString());
                    //handling object
                    line.Clear();
                }
            }         
            if (container.NeedPrefix)
            {
                line.Append("public ");
            }
            switch (container.ElementType)
            {
                case ElementDecalaration.XmlElementTypeEnum.NotXmlElement:
                    int[][] array;
                    
                    line.Append($"{container.TypeName} {container.Name}");
                    string inheritInfo = string.Join(",", container.InheritInfoList);
                    if(!string.IsNullOrEmpty(inheritInfo))
                    {
                        line.Append($": {inheritInfo}");
                    }
                    appendLineAction(line.ToString());
                    appendLineAction("{");
                    foreach (var subElement in container.SubElements)
                    {
                        GetResult(subElement.Value,container, (newLine)=>appendLineAction("\t"+ newLine));
                    }
                    appendLineAction("}");
                    break;
                case ElementDecalaration.XmlElementTypeEnum.Element:
                    string containerName = container.Name;
                    //avoid name duplicate in class
                    if(container.Name.Equals(parentContainer.Name))
                    {
                        containerName += "Value";
                    }
                    line.Append($"{container.TypeName} {containerName}" +"{get;set;}");
                    appendLineAction(line.ToString());
                    line.Clear();
                    break;
                case ElementDecalaration.XmlElementTypeEnum.Array:
                    if(!topLevel)
                    {
                        foreach (var subElement in container.SubElements.Where(ele=>ele.Value.ElementType.Equals(ElementDecalaration.XmlElementTypeEnum.ArrayItem)))
                        {
                            GetResult(subElement.Value,container, appendLineAction);
                        }
                        
                        line.AppendFormat("{0} {1}", oldContainer.TypeName, oldContainer.Name);
                        line.Append("{get;set;}");
                        appendLineAction(line.ToString());
                        line.Clear();
                    }
                    else
                    {
                        //try to find the type
                        var subElementTypeFilter = container.SubElements.Select(ele => ele.Value.TypeName).Distinct();
                        string type;
                        if (subElementTypeFilter.Count() > 1)
                        {
                            //multi type, need to declare as a object array
                            type = "object";
                        }
                        else
                        {
                            //single type, need to declare as a type array
                            type = subElementTypeFilter.First();
                        }
                        line.AppendFormat("class {1}:List<{0}>  ", type, container.Name);
                        appendLineAction(line.ToString());
                        line.Clear();
                        appendLineAction("{");
                        foreach (var subElement in container.SubElements.Where(ele => !ele.Value.ElementType.Equals(ElementDecalaration.XmlElementTypeEnum.ArrayItem)))
                        {
                            GetResult(subElement.Value,container, (addLine) => { appendLineAction("\t" + addLine); });
                        }
                        appendLineAction("}");


                    }                   

                    
                    break;
                case ElementDecalaration.XmlElementTypeEnum.ArrayItem:
                    break;
                case ElementDecalaration.XmlElementTypeEnum.Enum:
                    break;
                default:
                    line.Append($"{container.TypeName} {container.Name} "+"{get;set;}");
                    appendLineAction(line.ToString());
                    line.Clear();
                    break;
            }



        }
        XmlSchemaSet SchemaSet { get; set; }
        XmlSchema Schema { get; set; }
        ElementDecalaration MainContainer { get; set; }
        String head = "";
        void HandlingAnnotations(ElementDecalaration container, XmlSchemaAnnotated annotatedType)
        {
            if (annotatedType.Annotation != null && annotatedType.Annotation.Items.Count != 0)
            {
                StringBuilder comment = new StringBuilder();
                //add summary                            
                foreach (var annotation in annotatedType.Annotation.Items)
                {
                    switch (annotation)
                    {
                        case XmlSchemaDocumentation documentation:
                            foreach (var markUpItem in documentation.Markup)
                            {
                                comment.AppendLine(markUpItem.Value);
                            }
                            break;
                        case XmlSchemaAppInfo info:
                            foreach (var markUpItem in info.Markup)
                            {
                                comment.AppendLine("appinfo:" + markUpItem.Value);
                            }
                            break;
                        default:
                            Trace.WriteLine($"Currently not recongnized annotation type : {annotation} at line {annotation.LineNumber}");
                            break;
                    }
                }
                string commentString = comment.ToString();
                if (!string.IsNullOrEmpty(container.Comment))
                {
                    if (!container.Comment.Contains(commentString))
                    {
                        container.Comment += "\n" + commentString;
                    }
                }
            }
        }
        void HandlingSchemaType(ElementDecalaration elementDecalaration, XmlSchemaType schemaTypeDeclaration)
        {



        }
        void HandlingTypes(ElementDecalaration container, XmlSchemaObject xmlSchemaObject)
        {
            head = head + "\t";
            Trace.WriteLine(head + $"{xmlSchemaObject}");
            //get type name            
            switch (xmlSchemaObject)
            {
                case XmlSchemaAttributeGroup attributeGroupItem:
                    {
                        if (container.ElementType == ElementDecalaration.XmlElementTypeEnum.Array)
                        {
                            //raise error
                        }
                        foreach (var item in attributeGroupItem.Attributes)
                        {
                            HandlingTypes(container, item);
                        }
                    }
                    break;
                case XmlSchemaAttribute attributeDecalaration:
                    {

                    }
                    break;
                case XmlSchemaType schemaTypeDeclaration:
                    {


                        string typeRawName;
                        if (string.IsNullOrEmpty(schemaTypeDeclaration.Name))
                        {
                            typeRawName = ((schemaTypeDeclaration.Parent.GetType().GetProperty("Name").GetValue(schemaTypeDeclaration.Parent).ToString()));
                        }
                        else
                        {
                            typeRawName = (schemaTypeDeclaration.Name);
                        }
                        Trace.WriteLine(head + $"{typeRawName}");
                        if (container.SubElements.ContainsKey(typeRawName))
                        {

                        }
                        else
                        {
                            ElementDecalaration elementDecalaration = new ElementDecalaration();
                            elementDecalaration.Name = NameConverter(typeRawName);
                            elementDecalaration.ElementType = ElementDecalaration.XmlElementTypeEnum.NotXmlElement;
                            elementDecalaration.Attributes["ElementName"] =$"\"{typeRawName}\"";
                            elementDecalaration.TypeName = "class";
                            //handling annotations
                            HandlingAnnotations(elementDecalaration, schemaTypeDeclaration);
                            container.SubElements[typeRawName] = elementDecalaration;
                            //add inherit info
                            if (schemaTypeDeclaration.BaseXmlSchemaType != null)
                            {
                                if (schemaTypeDeclaration.BaseXmlSchemaType.Name != null)
                                {
                                    if (!MainContainer.SubElements.ContainsKey(schemaTypeDeclaration.BaseXmlSchemaType.Name))
                                    {
                                        HandlingTypes(MainContainer, schemaTypeDeclaration.BaseXmlSchemaType);
                                    }
                                    elementDecalaration.InheritInfoList.Add(NameConverter(schemaTypeDeclaration.BaseXmlSchemaType.Name));
                                }
                                else
                                {

                                    //consider base type
                                    string inheritType = "";
                                    switch(schemaTypeDeclaration.BaseXmlSchemaType.QualifiedName.Name.ToLower())
                                    {

                                        case "anytype":
                                            break;
                                        case "nmtoken":
                                            if (elementDecalaration.SubElements.ContainsKey("XmlNMToken"))
                                            {

                                            }
                                            else
                                            {
                                                elementDecalaration.SubElements["XmlNMToken"] = new ElementDecalaration()
                                                {
                                                    ElementType = ElementDecalaration.XmlElementTypeEnum.XmlText,
                                                    Name = "XmlNMToken",
                                                    TypeName = "string",
                                                };
                                            }
                                            break;
                                        default:
                                            inheritType = "Unknow";
                                            string type = schemaTypeDeclaration.BaseXmlSchemaType.QualifiedName.Name.ToLower();
                                            
                                            switch (type)
                                            {
                                                case "string":
                                                    break;
                                                case "double":
                                                    break;
                                                case "unsignedint":
                                                    type = "uint";
                                                    break;
                                                case "nmtoken":
                                                    type = "string";
                                                    break;
                                                case "nmtokens":
                                                    type = "string";
                                                    break;
                                                default:
                                                    break;
                                            }
                                            //add sub element,string
                                            if (elementDecalaration.SubElements.ContainsKey("XmlContent"))
                                            {

                                            }
                                            else
                                            {
                                                elementDecalaration.SubElements["XmlContent"] = new ElementDecalaration()
                                                {
                                                    ElementType = ElementDecalaration.XmlElementTypeEnum.XmlText,
                                                    Name = "XmlContent",
                                                    
                                                    TypeName = type,
                                                };
                                            }
                                            break;
                                    }
                                   
                                }


                            }
                            switch (schemaTypeDeclaration)
                            {
                                case XmlSchemaComplexType complexTypeDeclaration:
                                    {
                                        if (complexTypeDeclaration.Attributes != null)
                                        {
                                            //add attributes type
                                            foreach (var attributeItem in complexTypeDeclaration.Attributes)
                                            {
                                                switch (attributeItem)
                                                {
                                                    case XmlSchemaAttributeGroupRef attributeGroupRef:
                                                        {
                                                            //get group from ref                               
                                                            var attributeInstance = Schema.AttributeGroups[attributeGroupRef.RefName];
                                                            if (attributeInstance != null)
                                                            {
                                                                HandlingTypes(elementDecalaration, attributeInstance);
                                                            }
                                                        }
                                                        break;
                                                    default:
                                                        Trace.WriteLine($"Currently not attribute object type : {attributeItem} at line {attributeItem.LineNumber}");
                                                        break;
                                                }
                                            }
                                        }
                                        //avoid emptyobject
                                        if (complexTypeDeclaration.ContentTypeParticle != null && complexTypeDeclaration.ContentTypeParticle.GetType().IsPublic)
                                        {
                                            HandlingTypes(elementDecalaration, complexTypeDeclaration.ContentTypeParticle);
                                        }
                                        else
                                        {

                                        }
                                    }
                                    break;
                                case XmlSchemaSimpleType simpleTypeDecalaration:
                                    // no further handling
                                    break;
                                default:
                                    Trace.WriteLine($"Currently not recongnized object type : {xmlSchemaObject} at line {xmlSchemaObject.LineNumber}");
                                    break;
                            }                            
                        }
                    }
                    break;
                case XmlSchemaChoice xmlSchemaChoiceDeclaration:
                    {
                        //avoid recurrent choice
                        if (xmlSchemaChoiceDeclaration.Parent.Equals(xmlSchemaChoiceDeclaration))
                        {

                        }
                        if (xmlSchemaChoiceDeclaration.MaxOccurs == 1)
                        {
                            //not a array type
                        }
                        else
                        {
                            //array type
                            container.ElementType = ElementDecalaration.XmlElementTypeEnum.Array;
                            
                            

                        }
                        foreach (var choiceItem in xmlSchemaChoiceDeclaration.Items)
                        {
                            HandlingTypes(container, choiceItem);
                        }
                    }
                    break;
                case XmlSchemaElement xmlSchemaElementDecalaration:
                    {
                        string typeRawName = xmlSchemaElementDecalaration.ElementSchemaType.Name;
                        if (string.IsNullOrEmpty(typeRawName))
                        {
                            typeRawName = xmlSchemaElementDecalaration.Name;
                        }
                        Trace.WriteLine(head + $"{typeRawName}");
                        container.SubElements[xmlSchemaElementDecalaration.Name] = new ElementDecalaration()
                        {
                            Name = NameConverter(xmlSchemaElementDecalaration.Name),
                            TypeName = NameConverter(typeRawName),
                        };
                        container.SubElements[xmlSchemaElementDecalaration.Name].Attributes["ElementName"] = $"\"{xmlSchemaElementDecalaration.Name}\"";
                        if (container.ElementType == ElementDecalaration.XmlElementTypeEnum.Array)
                        {
                            container.SubElements[xmlSchemaElementDecalaration.Name].ElementType = ElementDecalaration.XmlElementTypeEnum.ArrayItem;
                            container.SubElements[xmlSchemaElementDecalaration.Name].Attributes["Type"] = $"typeof({container.SubElements[xmlSchemaElementDecalaration.Name].TypeName})";
                        }
                        else
                        {
                            container.SubElements[xmlSchemaElementDecalaration.Name].ElementType = ElementDecalaration.XmlElementTypeEnum.Element;
                        }
                        if (!MainContainer.SubElements.ContainsKey(typeRawName))
                        {
                            //create type append
                            HandlingTypes(MainContainer, xmlSchemaElementDecalaration.ElementSchemaType);
                            //if type is array
                            //if(MainContainer.SubElements[typeRawName].ElementType==ElementDecalaration.XmlElementTypeEnum.Array)
                            //{
                            //    //migrate to this one
                            //    container.SubElements[xmlSchemaElementDecalaration.Name] = MainContainer.SubElements[typeRawName];
                            //    //MainContainer.SubElements.Remove(typeRawName);
                            //    //TBD how to avoid multi 
                            //}
                        }
                    }

                    break;
                case XmlSchemaSequence xmlSchemaSequenceDecalaration:
                    {
                        if (xmlSchemaSequenceDecalaration.Items != null)
                        {
                            foreach (var choiceItem in xmlSchemaSequenceDecalaration.Items)
                            {
                                HandlingTypes(container, choiceItem);
                            }

                        }
                        else
                        {

                        }

                    }
                    break;
                default:
                    Trace.WriteLine($"Currently not recongnized object type : {xmlSchemaObject} at line {xmlSchemaObject.LineNumber}");
                    break;
            }
            head = head.Remove(head.Length - 1);
        }

        class ElementDecalaration
        {
            public enum XmlElementTypeEnum
            {
                NotXmlElement,
                Root,
                XmlText,
                Element,
                Attribute,
                Array,
                ArrayItem,
                ChoiceItem,
                Enum
            }
            public string ElementName { get; set; }
            public string Comment { get; set; }
            public bool NeedPrefix { get; set; } = true;
            public List<string> InheritInfoList { get; set; } = new List<string>();
            public string TypeName { get; set; }
            public XmlElementTypeEnum ElementType { get; set; }
            public Dictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();
            public string Name { get; set; }
            public Dictionary<string, ElementDecalaration> SubElements { get; set; } = new Dictionary<string, ElementDecalaration>();
            public int MaxNumber { get; set; } = 1;
            public int MinNumber { get; set; } = 0;
        }
        class AttributeItem
        {
            public string AttributeName { get; set; }
            public Dictionary<string, string> AttributeParams { get; set; } = new Dictionary<string, string>();
        }
        string NameConverter(string name)
        {
            //string name=xmlSchemaObject.Name;
            //return null;
            return string.Join(
                "",
                name//.Replace("xsd:", "")
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
                        .Select(ele => $"{ele.First().ToString().ToUpper()}{ele.Substring(1).ToLower()}")
                        );
                }));
        }
    }
}
