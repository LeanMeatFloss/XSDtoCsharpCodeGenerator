using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace CSharpCodeGenerator
{

    public class Generator
    {
        [XmlIgnore]
        XmlSchemaSet SchemaSet { get; set; }
        [XmlIgnore]
        XmlSchema Schema { get; set; }
        public TypeDefineRef ParserFile(string[] dependencyFilePath, string targetFilePath, string rootObject)
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
            //searching for element~~~
            var types = SchemaSet.GlobalTypes[new XmlQualifiedName(rootObject, Schema.TargetNamespace)];
            //var root = new ;
            Root = new TypeDefineRef()
            {
                XmlNodeType = XmlNodeType.XmlRoot,
            };
            HandlingTypes(SchemaSet.GlobalElements.Values.OfType<XmlSchemaObject>().FirstOrDefault(), Root);



            //first calculate the base type referenced Types
            var hasChildList = TypeDefineCollection.Values.Where(ele => ele.InheritInfomation.Count > 0);
            var errorList = TypeDefineCollection.Values.Where(ele => ele.InheritInfomation.Count > 1);
            if (errorList.Count() > 0)
            {

            }
            var baseTypeList = hasChildList.SelectMany(ele => ele.InheritInfomation).Distinct();
            foreach (var baseTypeName in baseTypeList)
            {
                TypeDefineInformation baseTypeInformation = baseTypeName;
                foreach (var property in baseTypeInformation.ChildProperties)
                {
                    if (string.IsNullOrEmpty(property.AbstractionLevel))
                    {
                        property.AbstractionLevel = "virtual";
                    }
                }
                //gathering all the types related to the base types
                var derivedList = hasChildList.Where(ele => ele.InheritInfomation.Contains(baseTypeName));
                foreach (var item in derivedList)
                {
                    foreach (var property in baseTypeInformation.ChildProperties)
                    {
                        var filteredProperty = item.ChildProperties.Where(ele => ele.Name.Equals(property.Name)).FirstOrDefault();
                        if (filteredProperty != null)
                        {
                            filteredProperty.AbstractionLevel = "override";
                        }
                    }
                }
            }
            //handling type definitions for choice or array
            //select the item that 
            var filteredPotentialList = TypeDefineCollection
                .SelectMany(ele => ele.Value.ChildProperties)
                .Where(child => child.MaxCount > 1
                    || child.IsMaxUnlimited
                    );
            foreach (var filterItem in filteredPotentialList)
            {
                //if the item has multiple name match to same type
                //handling the items in the filter Item
                filterItem.IsArray = true;
            }

            return Root;
        }
        public void GenerateCodeForCSharp(string NameSpace)
        {
            //create codes for every
            StringBuilder stringBuilder = new StringBuilder();

        }
        [XmlIgnore]
        public TypeDefineRef Root { get; set; }
        public enum XmlNodeType
        {
            XmlRoot,
            XmlEnum,
            XmlElement,
            XmlAttribute
        }
        public class TypeDefineRef
        {
            public bool IsArray { get; set; }
            public string NameSpace { get; set; }
            public TypeDefineInformation ParentType { get; set; }
            public string AllowWhiteSpace { get; set; }
            public string MaxLength { get; set; }
            public string RegularExpression { get; set; }
            public string AppInfo { get; set; }
            public string Summary { get; set; }
            public string Name { get; set; }
            public XmlNodeType XmlNodeType { get; set; } = XmlNodeType.XmlElement;
            public TypeDefineInformation RefType { get; set; }
            public bool IsMaxUnlimited { get; set; }
            public int MaxCount { get; set; } = 0;
            public bool IsMinUnlimited { get; set; }
            public int MinCount { get; set; } = 0;
            public string AbstractionLevel { get; set; }
        }
        public class KeyValueElement<T>
        {
            public string Key { get; set; }
            public T Value { get; set; }
        }
        public List<KeyValueElement<TypeDefineInformation>> TypeDefineCollectionList
        {
            get
            {
                return TypeDefineCollection.Select(ele => new KeyValueElement<TypeDefineInformation>()
                {
                    Key = ele.Key,
                    Value = ele.Value
                }).ToList();
            }
            set
            {
                foreach (var item in value)
                {
                    TypeDefineCollection[item.Key] = item.Value;
                }
            }
        }
        [XmlIgnore]
        public Dictionary<string, TypeDefineInformation> TypeDefineCollection { get; } = new Dictionary<string, TypeDefineInformation>()
        {
            {"string",new TypeDefineInformation(){IsXmlBasicType=true,Name="string"}},
            {"NMTOKEN", new TypeDefineInformation(){IsXmlBasicType=true,Name="string"}},
            {"NMTOKENS", new TypeDefineInformation(){IsXmlBasicType=true,Name="string"}},
            {"double", new TypeDefineInformation(){IsXmlBasicType=true,Name="double"}},
            {"unsignedInt", new TypeDefineInformation(){IsXmlBasicType=true,Name="uint"}},
            {"space", new TypeDefineInformation(){IsXmlBasicType=true,Name="XmlSpace"}},
        };
        public TypeDefineInformation GetOrAddItem(string Name)
        {
            TypeDefineInformation childSelected = TypeDefineCollection.Values.Where(ele => ele.Name.Equals(Name)).FirstOrDefault();
            if (childSelected == null)
            {
                childSelected = new TypeDefineInformation()
                {
                    Name = Name,
                };
                TypeDefineCollection[Name] = childSelected;
                return childSelected;
            }
            else
            {
                return childSelected;
            }
        }
        public enum TypeBaseEnum
        {
            Class,
            Enum,
            //Array,            
        }
        public class TypeDefineInformation
        {
            public string Summary { get; set; }
            public string AppInfo { get; set; }
            public XmlSchemaType XmlRef { get; set; }
            public TypeBaseEnum TypeBase { get; set; } = TypeBaseEnum.Class;
            public bool IsXmlBasicType { get; set; }
            //public Dictionary<string, Dictionary<string, string>> AttributeConfigurations { get; set; }
            public string Name { get; set; }
            public List<TypeDefineInformation> InheritInfomation { get; } = new List<TypeDefineInformation>();
            public List<TypeDefineRef> ChildProperties { get; set; } = new List<TypeDefineRef>();
            public TypeDefineRef GetNewChild()
            {

                var res = new TypeDefineRef() { ParentType = this };

                ChildProperties.Add(res);
                return res;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="NameSpace"></param>
        /// <returns></returns>
        public string GenerateCode(string NameSpace)
        {
            StringBuilder stringBuilder = new StringBuilder(@"
using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
");
            stringBuilder.AppendLine($"namespace {NameSpace}");
            stringBuilder.AppendLine("{");
            //            stringBuilder.AppendLine(@"
            //    public abstract class XmlNodeBase
            //    {
            //        public virtual XmlNodeNameEnum XmlNodeName {get;set;}
            //    }
            //    public enum XmlNodeNameEnum
            //    {
            //");
            //            //generate all potential value for xml node
            //            List<string> XmlNodeNameList = TypeDefineCollection.Values
            //                .Where(ele => !ele.IsXmlBasicType)
            //                .Where(ele => ele.TypeBase != TypeBaseEnum.Enum)
            //                .Where(ele => ele.TypeBase != TypeBaseEnum.Array)
            //                .Select(ele => ele.Name)
            //                .Concat(TypeDefineCollection
            //                    .SelectMany(ele => ele.Value.ChildProperties)
            //                    .Where(ele => ele.XmlNodeType != XmlNodeType.XmlEnum)
            //                    .Where(ele => !ele.RefType.IsXmlBasicType)
            //                    .Where(ele => ele.RefType.TypeBase != TypeBaseEnum.Array && ele.RefType.TypeBase != TypeBaseEnum.Choice)
            //                    .Select(ele => ele.Name)
            //                    )
            //                .Distinct().ToList();
            //            foreach (var nameNode in XmlNodeNameList)
            //            {
            //                stringBuilder.AppendLine($"\t\t[XmlEnum(\"{nameNode}\")]");
            //                stringBuilder.AppendLine($"\t\t{NameConverter(nameNode)},");
            //            }
            //            stringBuilder.AppendLine("\t}");

            //handling array item or choice item
            //add content
            foreach (var item in TypeDefineCollection)
            {
                if (item.Value.IsXmlBasicType)
                {
                    continue;
                }
                if (item.Value.ChildProperties.Count == 1 && item.Value.ChildProperties[0].IsArray)
                {
                    //child are all array
                    continue;
                }
                //add content for root
                if (Root.RefType.Equals(item.Value))
                {
                    if (!string.IsNullOrEmpty(Root.Summary) || !string.IsNullOrEmpty(Root.AppInfo))
                    {
                        string comment = "Documents:\n" + Root.Summary + "\nAppInfo:\n" + Root.AppInfo;
                        comment = "\t\t///" + string.Join("\n\t///", comment.Split(new char[] { '\n' }));
                        //add summary
                        stringBuilder.AppendFormat(@"
    ///<summary>
{0}
    ///</summary>
", comment);
                    }
                    //add root summary
                    stringBuilder.AppendLine($"\t[XmlRoot(ElementName=\"{Root.Name}\",Namespace=\"{Root.NameSpace}\")]");
                    
                }
                if (item.Value.TypeBase == TypeBaseEnum.Enum)
                {
                    stringBuilder.Append("\tpublic enum ");
                }
                else// if (item.Value.TypeBase == TypeBaseEnum.Class)
                {
                    stringBuilder.Append("\tpublic class ");
                }
                //else
                //{
                //    continue;
                //}
                //add name
                stringBuilder.Append($"{NameConverter(item.Value.Name)}");
                //add inherit info
                if (item.Value.InheritInfomation
                    .Where(ele => ele.TypeBase == TypeBaseEnum.Enum || ele.IsXmlBasicType)
                    .Count() != 0
                    )
                {
                    //use value instead of the inherit info
                    stringBuilder.Append("\n\t{\n");
                    //add xmlText to the child
                    stringBuilder.AppendFormat("\t\t[XmlText(Type=typeof({0}))]\n", item.Value.InheritInfomation[0].IsXmlBasicType ? item.Value.InheritInfomation[0].Name : NameConverter(item.Value.InheritInfomation[0].Name));
                    stringBuilder.AppendLine($"\t\tpublic {(item.Value.InheritInfomation[0].IsXmlBasicType ? item.Value.InheritInfomation[0].Name : NameConverter(item.Value.InheritInfomation[0].Name))} _XmlText {{get;set;}}");
                }
                else if (item.Value.InheritInfomation.Count != 0)
                {
                    stringBuilder.Append(" : " + string.Join(",", item.Value.InheritInfomation.Select(ele => NameConverter(ele.Name))));

                    stringBuilder.Append("\n\t{\n");
                }
                else
                {
                    stringBuilder.Append("\n\t{\n");
                }

                foreach (var subItem in item.Value.ChildProperties)
                {
                    string TypeName = "";
                    //handling enum
                    if (!string.IsNullOrEmpty(subItem.Summary) || !string.IsNullOrEmpty(subItem.AppInfo))
                    {
                        string comment = "Documents:\n" + subItem.Summary + "\nAppInfo:\n" + subItem.AppInfo;
                        comment = "\t\t///" + string.Join("\n\t\t///", comment.Split(new char[] { '\n' }));
                        //add summary
                        stringBuilder.AppendFormat(@"
        ///<summary>
{0}
        ///</summary>
", comment);
                    }
                    //if child item are single array
                    if (subItem.XmlNodeType == XmlNodeType.XmlElement && subItem.RefType.ChildProperties.Count == 1 && subItem.RefType.ChildProperties[0].IsArray)
                    {
                        //handling enum
                        if (!string.IsNullOrEmpty(subItem.RefType.ChildProperties[0].Summary) || !string.IsNullOrEmpty(subItem.RefType.ChildProperties[0].AppInfo))
                        {
                            string comment = "Documents:\n" + subItem.RefType.ChildProperties[0].Summary + "\nAppInfo:\n" + subItem.RefType.ChildProperties[0].AppInfo;
                            comment = "\t\t///" + string.Join("\n\t\t///", comment.Split(new char[] { '\n' }));
                            //add summary
                            stringBuilder.AppendFormat(@"
        ///<summary>
{0}
        ///</summary>
", comment);
                        }
                        //map to a array instead of elements
                        stringBuilder.AppendFormat("\t\t[XmlArray(ElementName=\"{0}\",Namespace={1})]\n", subItem.Name, $"\"{subItem.NameSpace}\"");
                        stringBuilder.AppendFormat("\t\t[XmlArrayItem(ElementName=\"{0}\",Namespace={1})]\n", subItem.RefType.ChildProperties[0].Name, $"\"{subItem.NameSpace}\"");
                        TypeName = NameConverter(subItem.RefType.ChildProperties[0].RefType.Name) + "[]";
                    }
                    else
                    {
                        //get the xml header
                        switch (subItem.XmlNodeType)
                        {
                            case XmlNodeType.XmlAttribute:
                                stringBuilder.AppendFormat("\t\t[XmlAttribute(AttributeName=\"{0}\",Namespace={1})]\n", subItem.Name, $"\"{subItem.NameSpace}\"");
                                if (subItem.RefType.IsXmlBasicType)
                                {
                                    TypeName = (subItem.RefType.Name);
                                }
                                else
                                {
                                    TypeName = NameConverter(subItem.RefType.Name);
                                }
                                break;
                            case XmlNodeType.XmlEnum:
                                stringBuilder.AppendFormat("\t\t[XmlEnum(Name=\"{0}\")]\n", subItem.Name);
                                break;
                            case XmlNodeType.XmlRoot:
                                stringBuilder.AppendFormat("\t\t[XmlRoot(Name=\"{0}\",Namespace={1})]\n", subItem.Name, $"\"{subItem.NameSpace}\"");
                                TypeName = NameConverter(subItem.RefType.Name);
                                break;
                            case XmlNodeType.XmlElement:
                                {
                                    //need to check whether is array or element
                                    if (subItem.RefType.IsXmlBasicType)
                                    {
                                        TypeName = (subItem.RefType.Name);
                                    }
                                    else
                                    {
                                        TypeName = NameConverter(subItem.RefType.Name);
                                    }
                                    if (subItem.IsArray)
                                    {
                                        TypeName += "[]";
                                    }
                                    stringBuilder.AppendFormat("\t\t[XmlElement(ElementName=\"{0}\",Namespace={1})]\n", subItem.Name, $"\"{subItem.NameSpace}\"");
                                }
                                break;
                        }
                    }
                    //get the element type from list
                    if (subItem.XmlNodeType == XmlNodeType.XmlEnum)
                    {
                        stringBuilder.AppendLine($"\t\t{NameConverter(subItem.Name)},");
                    }
                    else
                    {
                        string elementName = NameConverter(subItem.Name);

                        if (TypeName.Equals(elementName) || elementName.Equals(NameConverter(item.Value.Name)) || item.Value.ChildProperties.Select(ele => ele.Name).Count(ele => ele.Equals(subItem.Name)) > 1)
                        {
                            switch (subItem.XmlNodeType)
                            {
                                //case XmlNodeType.XmlAttribute:
                                //    elementName = elementName+"_Attr";
                                //    break;
                                case XmlNodeType.XmlElement:
                                    elementName = "_" + elementName;
                                    break;
                                    //case XmlNodeType.XmlEnum:
                                    //    elementName = elementName + "_Enum";
                                    //    break;
                            }

                        }
                        if (subItem.IsArray)
                        {
                            elementName += "s";
                        }
                        if (string.IsNullOrEmpty(subItem.AbstractionLevel))
                        {
                            stringBuilder.Append($"\t\tpublic {TypeName} {elementName} {{get;set;}}");
                        }
                        else
                        {
                            stringBuilder.Append($"\t\tpublic {subItem.AbstractionLevel} {TypeName} {elementName} {{get;set;}}");
                        }
                        if(subItem.MinCount==1&&subItem.MaxCount==1&&!subItem.IsMinUnlimited&&!subItem.IsArray&&subItem.ParentType.TypeBase!=TypeBaseEnum.Enum)
                        {
                            //add type
                            stringBuilder.Append($" = new {TypeName} ();");
                        }
                        stringBuilder.Append("\n");
                    }

                }
                stringBuilder.Append("\n\t}\n");
            }
            stringBuilder.AppendLine("}");
            return stringBuilder.ToString();
        }
        void HandlingCodeGeneration(Action<string> AppendLine)
        {

        }
        public virtual string NameConverter(string name)
        {
            //string name=xmlSchemaObject.Name;
            //return null;
            string res = string.Join(
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
            if (new char[] { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0' }.Contains(res.First()))
            {
                res = "_" + res;
            }
            return res;
        }
        void handlingAnnotation(XmlSchemaAnnotated annotatedItem, TypeDefineRef currentDefine)
        {
            if (annotatedItem.Annotation == null)
            {
                return;
            }
            foreach (var annotationItem in annotatedItem.Annotation.Items)
            {

                if (annotationItem is XmlSchemaDocumentation schemaDocumentation)
                {
                    //add summary
                    currentDefine.Summary += string.Join("\n", schemaDocumentation.Markup.Select(ele => ele.Value));
                }
                if (annotationItem is XmlSchemaAppInfo appInfo)
                {
                    //add summary
                    currentDefine.AppInfo += string.Join("\n", appInfo.Markup.Select(ele => ele.Value));
                }
            }
        }
        void HandlingTypes(XmlSchemaObject targetObject, TypeDefineRef currentDefine)
        {
            if (targetObject == null)
            {
                return;
            }
            switch (targetObject)
            {
                case XmlSchemaAttributeGroup attributeGroupItem:
                    {
                        foreach (var item in attributeGroupItem.Attributes)
                        {
                            HandlingTypes(item, currentDefine);
                        }
                    }
                    break;
                case XmlSchemaType schemaTypeDeclaration:
                    handlingAnnotation(schemaTypeDeclaration, currentDefine);
                    //set typename
                    //get typefrom collection

                    if (schemaTypeDeclaration.Name == null)
                    {
                        if (schemaTypeDeclaration.QualifiedName.Name.Equals("anyType"))
                        {
                            break;
                        }
                        else if (string.IsNullOrEmpty(schemaTypeDeclaration.QualifiedName.Name))
                        {
                            //empty name happens in when sub classes
                            //create a sub container for the reference from parent
                            if (string.IsNullOrEmpty(currentDefine.Name))
                            {

                            }
                            string typeNameNeedToSearch = currentDefine.ParentType.Name + "--" + currentDefine.Name;
                            if (TypeDefineCollection.ContainsKey(typeNameNeedToSearch))
                            {
                                //end process
                                //check the definitions
                                if (currentDefine.RefType == null)
                                {
                                    currentDefine.RefType = TypeDefineCollection[typeNameNeedToSearch];
                                }
                                break;
                            }
                            else
                            {
                                //create to process
                                currentDefine.RefType = GetOrAddItem(typeNameNeedToSearch);
                                currentDefine.RefType.XmlRef = schemaTypeDeclaration;
                            }

                        }
                        else
                        {
                            //need to further check
                        }
                    }
                    else if (TypeDefineCollection.ContainsKey(schemaTypeDeclaration.Name))
                    {
                        //check the definitions
                        if (currentDefine.RefType == null)
                        {
                            currentDefine.RefType = TypeDefineCollection[schemaTypeDeclaration.Name];
                        }
                        //end process
                        break;
                    }
                    else
                    {

                    }
                    Debug.WriteLine($"{currentDefine.RefType?.Name ?? currentDefine.Name}");
                    //switch complex type
                    switch (schemaTypeDeclaration)
                    {
                        case XmlSchemaComplexType complexType:
                            if (currentDefine.RefType == null)
                                currentDefine.RefType = GetOrAddItem(schemaTypeDeclaration.Name);
                            //handling uses attributes
                            foreach (XmlQualifiedName attributeItemName in complexType.AttributeUses.Names)
                            {
                                XmlSchemaAttribute itemType = complexType.AttributeUses[attributeItemName] as XmlSchemaAttribute;

                                //get schemaType from container
                                //var attributeGet = SchemaSet.GlobalAttributes[itemType.SchemaTypeName];
                                var typeGet = itemType.AttributeSchemaType;// SchemaSet.GlobalTypes[itemType.SchemaTypeName];
                                if (itemType.AttributeSchemaType == null)
                                {

                                }
                                var typeRef = currentDefine.RefType.GetNewChild();
                                typeRef.Name = itemType.Name;
                                typeRef.NameSpace = itemType.QualifiedName.Namespace;
                                if (string.IsNullOrEmpty(typeRef.NameSpace))
                                {
                                    typeRef.NameSpace = itemType.SchemaTypeName.Namespace;
                                }
                                typeRef.XmlNodeType = XmlNodeType.XmlAttribute;
                                //handling annotations
                                handlingAnnotation(itemType, typeRef);
                                if (string.IsNullOrEmpty(itemType.Name))
                                {
                                    typeRef.Name = itemType.RefName.Name;
                                    //set to ref type
                                    if (!TypeDefineCollection.ContainsKey(typeRef.Name))
                                    {

                                    }
                                    else
                                    {
                                        typeRef.RefType = TypeDefineCollection[typeRef.Name];
                                        break;
                                    }
                                }
                                //create properties based on the base type
                                HandlingTypes(typeGet, typeRef);

                            }
                            //currently not handling attributes

                            //handling particles
                            HandlingTypes(complexType.ContentTypeParticle, currentDefine);
                            //handling basic type


                            if (!complexType.BaseXmlSchemaType.QualifiedName.Name.Equals("anyType"))
                            {
                                var baseType = new TypeDefineRef();
                                HandlingTypes(complexType.BaseXmlSchemaType, baseType);
                                currentDefine.RefType.InheritInfomation.Add(baseType.RefType);
                            }

                            break;
                        case XmlSchemaSimpleType simpleType:
                            //check content restriction
                            //simple type only have content attribute additionally
                            if (!string.IsNullOrEmpty(simpleType.Name))
                            {

                            }
                            else
                            {
                                currentDefine.NameSpace = simpleType.QualifiedName.Namespace;
                            }
                            handlingAnnotation(simpleType.Content, currentDefine);
                            switch (simpleType.Content)
                            {
                                case XmlSchemaSimpleTypeRestriction simpleTypeRestriction:
                                    //nead handling base type and enumeration value from facts
                                    //handling base type
                                    //if the base type has more divations
                                    if (simpleTypeRestriction.Facets.OfType<XmlSchemaEnumerationFacet>().Count() > 0)
                                    {
                                        //enum type, need create a new container
                                        currentDefine.RefType = GetOrAddItem(simpleType.Name);
                                    }
                                    else if (TypeDefineCollection.ContainsKey(simpleTypeRestriction.BaseTypeName.Name))
                                    {
                                        //set ref to base type
                                        currentDefine.RefType = TypeDefineCollection[simpleTypeRestriction.BaseTypeName.Name];


                                    }
                                    else
                                    {
                                        //add definitions
                                        currentDefine.RefType = GetOrAddItem(simpleTypeRestriction.BaseTypeName.Name);
                                    }
                                    //handling enum posissble
                                    if (simpleTypeRestriction.Facets.Count != 0)
                                    {
                                        foreach (var facet in simpleTypeRestriction.Facets)
                                        {
                                            switch (facet)
                                            {
                                                case XmlSchemaPatternFacet patternFacet:
                                                    //add pattern
                                                    currentDefine.RegularExpression = patternFacet.Value;
                                                    break;
                                                case XmlSchemaEnumerationFacet enumFacet:
                                                    currentDefine.RefType.TypeBase = TypeBaseEnum.Enum;
                                                    var enumChild = currentDefine.RefType.GetNewChild();
                                                    enumChild.XmlNodeType = XmlNodeType.XmlEnum;
                                                    enumChild.Name = enumFacet.Value;
                                                    break;
                                                case XmlSchemaMaxLengthFacet maxLengthFacet:
                                                    currentDefine.MaxLength = maxLengthFacet.Value;
                                                    break;
                                                case XmlSchemaWhiteSpaceFacet whiteSpaceFacet:
                                                    currentDefine.AllowWhiteSpace = whiteSpaceFacet.Value;
                                                    break;
                                                default:
                                                    break;
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }
                            break;
                        default:
                            break;
                    }

                    break;
                case XmlSchemaParticle particle:
                    //handling schematype
                    {
                        switch (particle)
                        {
                            case XmlSchemaElement schemaElement:
                                Debug.WriteLine($"{schemaElement.Name}");
                                //element has name
                                currentDefine.Name = schemaElement.QualifiedName.Name;
                                currentDefine.NameSpace = schemaElement.QualifiedName.Namespace;
                                HandlingTypes(schemaElement.ElementSchemaType, currentDefine);
                                break;
                            case XmlSchemaGroupBase groupBase:
                                //handling items
                                foreach (var item in groupBase.Items)
                                {
                                    switch (item)
                                    {
                                        case XmlSchemaChoice choiceItem:
                                            HandlingTypes(choiceItem, currentDefine);
                                            break;
                                        case XmlSchemaElement schemaItem:
                                            //create child container
                                            //
                                            var typeRef = currentDefine.RefType.GetNewChild();
                                            HandlingTypes(schemaItem, typeRef);
                                            break;
                                        case XmlSchemaSequence sequenceItem:
                                            HandlingTypes(sequenceItem, currentDefine);
                                            break;
                                        default:
                                            break;
                                    }
                                }
                                switch (groupBase)
                                {
                                    case XmlSchemaSequence sequence:

                                        break;
                                    case XmlSchemaChoice choice:
                                        break;
                                    default:
                                        break;
                                }
                                break;

                            default:
                                if (particle.GetType().Name.EndsWith("EmptyParticle"))
                                {

                                }
                                else
                                {

                                }
                                break;
                        }
                        
                        foreach (var item in currentDefine.RefType.ChildProperties.Where(ele => ele.XmlNodeType != XmlNodeType.XmlAttribute))
                        {
                            if ("unbounded".Equals(particle.MaxOccursString))
                            {
                                item.IsMaxUnlimited = true;
                            }
                            else
                            {
                                item.MaxCount = (int)particle.MaxOccurs;
                            }
                            if ("unbounded".Equals(particle.MinOccursString))
                            {
                                item.IsMinUnlimited = true;
                            }
                            else
                            {
                                item.MinCount = (int)particle.MinOccurs;
                            }
                        }
                    }
                    break;
                default:
                    break;

            }
        }
    }
}
