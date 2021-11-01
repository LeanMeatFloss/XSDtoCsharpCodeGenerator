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
    public class GeneratorUnitTests
    {
        [Fact]
        public void TestForBasicLoadingFiles()
        {
            CSharpCodeGenerator.Generator generator = new CSharpCodeGenerator.Generator();
            var res = generator.ParserFile(
                new string[] { @"C:\Personal\WorkSpaces\GItHub\XSDtoCsharpCodeGenerator\namespace.xsd" },
                new string[] { "http://autosar.org/schema/r4.0",},
                @"C:\Personal\WorkSpaces\GItHub\XSDtoCsharpCodeGenerator\AUTOSAR_4-3-0.xsd",
                 "http://autosar.org/schema/r4.0 AUTOSAR_4-3-0.xsd",
                "AUTOSAR");
            var code = generator.GenerateCode("AR430");
            File.WriteAllText(@"C:\Personal\WorkSpaces\GItHub\XSDtoCsharpCodeGenerator\XSDSupportLib\TesterOutput\AR.cs", code);
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
            //File.WriteAllText("loadCOnfigure.xml", code);
            //}
            //catch (Exception e)
            //{

            //}
        }
        [Fact]
        public void TestForGenerateJUnit()
        {
            CSharpCodeGenerator.Generator generator = new CSharpCodeGenerator.Generator();
            var res = generator.ParserFile(
                new string[] {},
                new string[] {},
                @"C:\Personal\WorkSpaces\GItHub\XSDtoCsharpCodeGenerator\XSDSupportLib\TestForXSDParser\JUnit.xsd",
                 "",
                "testsuites");
            var code = generator.GenerateCode("JUnit");
            File.WriteAllText(@"C:\Personal\WorkSpaces\GItHub\XSDtoCsharpCodeGenerator\XSDSupportLib\TesterOutput\JUnit.cs", code);
        }
        
    }
}
