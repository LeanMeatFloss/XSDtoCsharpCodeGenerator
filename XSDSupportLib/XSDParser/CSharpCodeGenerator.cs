using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Schema;

namespace XSDParser
{
    public class CSharpCodeGenerator
    {
        XmlSchemaSet SchemaSet { get; set; }
        XmlSchema Schema { get; set; }
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
            //searching for element~~~
            var types = SchemaSet.GlobalTypes[new XmlQualifiedName(rootObject, Schema.TargetNamespace)];
            //HandlingTypes(MainContainer, types);
            List<string> lines = new List<string>();
        }
        static string NameConverter(string name)
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
        List<string> HandlingTypes(XmlSchemaObject targetObject)
        {
            switch(targetObject)
            {
                case XmlSchemaAttributeGroup attributeGroupItem:
                    {
                        foreach (var item in attributeGroupItem.Attributes)
                        {
                            HandlingTypes(item);
                        }
                    }
                    break;
                case XmlSchemaType schemaTypeDeclaration:
                    
                    break;
            }
            return null;
        }
    }
}
