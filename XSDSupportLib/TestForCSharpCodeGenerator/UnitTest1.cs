using System;
using System.IO;
using Xunit;
using System.Linq;
using System.Xml.Serialization;
using System.Text;
using System.Xml;
using CSharpCodeGenerator;
using System.Diagnostics;

namespace TestForCSharpCodeGenerator
{
    public class UnitTest1
    {
        [Fact]
        public void TestForBasicLoadingFiles()
        {
            CSharpCodeGenerator.Generator generator = new CSharpCodeGenerator.Generator();
            var res = generator.ParserFile(new string[] { @"C:\Personal\WorkSpaces\XSDtoCsharpCodeGenerator\namespace.xsd" }, @"C:\Personal\WorkSpaces\XSDtoCsharpCodeGenerator\AUTOSAR_4-3-0.xsd", "AUTOSAR");
            
            var code = generator.GenerateCode("AR430");
            File.WriteAllText(@"C:\Personal\WorkSpaces\XSDtoCsharpCodeGenerator\XSDSupportLib\TesterOutput\AR.cs", code);
            //XmlSerializer ser = new XmlSerializer(generator.GetType());
            //StringBuilder sb = new StringBuilder();
            //try
            //{
            //    using (XmlWriter writer = XmlWriter.Create(sb, new XmlWriterSettings() { Encoding = Encoding.UTF8, Indent = true }))
            //    {
            //        ser.Serialize(writer, generator);
            //    }
            //}
            //catch(Exception e)
            //{
            //    Debug.Write(e.Message + e.StackTrace);
            //}
            File.WriteAllText("loadCOnfigure.xml", code);
            //}
            //catch (Exception e)
            //{

            //}
        }
        [Fact]
        public void TestForCodeGenerating()
        {
            //Generator generator;
            //XmlSerializer ser = new XmlSerializer(typeof(Generator));
            //using (Stream reader = File.OpenRead("loadCOnfigure.xml"))
            //{
            //    generator = ser.Deserialize(reader) as Generator;
            //}

            //var code = generator.GenerateCode("AR430");
            //File.WriteAllText(@"C:\Personal\WorkSpaces\XSDtoCsharpCodeGenerator\XSDSupportLib\TesterOutput\AR.cs", code);
        }
    }
}
