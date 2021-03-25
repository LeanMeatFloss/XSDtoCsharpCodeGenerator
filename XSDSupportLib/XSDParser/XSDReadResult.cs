using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XSDParser
{
    public class AttributeClass
    {
        public string Name { get; set; }
        public Dictionary<string, string> ConfigurationParams { get; set; } = new Dictionary<string, string>();
    }

    public class XSDReadResult
    {
        public enum XSDInstanceType
        {
            ComplexType,
            Group,
            SimpleType,
            Element,
            AttributeGroup,
            Attribute,
            Extension,
        }
        public XSDInstanceType InstanceType { get; set; }
        public bool IsArray
        {
            get;
            set;
        }
        public string Comment { get; set; }
        public string Prefix { get; set; }
        public string TypeName { get; set; }
        public string QualifiedName { get; set; }
        public string Name { get; set; }
        public List<string> ArrayItemTypeList { get; set; } = new List<string>();
        public List<string> InheritInfo { get; set; } = new List<string>();
        public List<AttributeClass> HeaderAttributes { get; set; } = new List<AttributeClass>();
        public void UpdateHeaderAttributes(string headerName, Dictionary<string, string> attributeParams)
        {
            AttributeClass attribute = HeaderAttributes.Where(ele => ele.Name.Equals(headerName)).FirstOrDefault();
            if (attribute == null)
            {
                attribute = new AttributeClass() { Name = headerName };
                HeaderAttributes.Add(attribute);
            }
            foreach (var keyValue in attributeParams)
            {
                attribute.ConfigurationParams[keyValue.Key] = keyValue.Value;
            }
        }
        public AttributeClass GetOrAddAttribute(string attributeName)
        {
            AttributeClass attribute = HeaderAttributes.Where(ele => ele.Name.Equals(attributeName)).FirstOrDefault();
            if (attribute == null)
            {
                attribute = new AttributeClass() { Name = attributeName };
                HeaderAttributes.Add(attribute);
            }
            return attribute;
        }
        public void AppendRef(List<XSDReadResult> refList)
        {
            if (!RefSubElementsList.Contains(refList))
            {
                RefSubElementsList.Add(refList);
            }
        }
        public List<List<XSDReadResult>> RefSubElementsList { get; set; } = new List<List<XSDReadResult>>();
        public List<XSDReadResult> SubElement { get; set; } = new List<XSDReadResult>();
        public XSDReadResult GetElementOrCreate(string prefix, string type, string name,XSDInstanceType instanceType)
        {
            XSDReadResult filterResult = this.SubElement.Where(ele => ele.Name.Equals(name)&&ele.InstanceType.Equals(instanceType)).FirstOrDefault();
            if (filterResult == null)
            {
                filterResult = new XSDReadResult()
                {
                    Prefix = prefix,
                    TypeName = type,
                    Name = name,
                    InstanceType=instanceType,
                };
                this.SubElement.Add(filterResult);
            }
            return filterResult;
        }
    }
}
