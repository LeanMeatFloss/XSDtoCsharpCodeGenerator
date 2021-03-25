using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml;
using System.Linq;
using System.Xml.Serialization;
using Xunit;
using System.Collections;

namespace TestForXSDParser
{
    public class UnitTest1
    {
        [Fact]

        public void Test1()
        {

            var res = XSDParser.Reader.LoadFile(@"C:\Personal\WorkSpaces\XSDtoCsharpCodeGenerator\AUTOSAR_4-3-0.xsd");
            var code = res.OutputCode();
            File.WriteAllText(@"C:\Personal\WorkSpaces\XSDtoCsharpCodeGenerator\XSDSupportLib\TesterOutput\AR.cs", code);
        }
        [Fact]
        public void Test2()
        {

            XSDParser.SchemaBasedParser parser = new XSDParser.SchemaBasedParser();
            try
            {
                parser.ParserFile(new string[] { @"C:\Personal\WorkSpaces\XSDtoCsharpCodeGenerator\namespace.xsd" }, @"C:\Personal\WorkSpaces\XSDtoCsharpCodeGenerator\AUTOSAR_4-3-0.xsd", "AUTOSAR");
            }
            catch (Exception e)
            {

            }
            File.WriteAllText(@"C:\Personal\WorkSpaces\XSDtoCsharpCodeGenerator\XSDSupportLib\TesterOutput\AR.cs", parser.GetCode());

        }
        [Fact]
        public void Test3()
        {
            XmlSerializer ser = new XmlSerializer(typeof(Test));
            StringBuilder sb = new StringBuilder();

            using (XmlWriter writer = XmlWriter.Create(sb, new XmlWriterSettings() { Encoding = Encoding.UTF8, Indent = true }))
            {
                ser.Serialize(writer, new Test()
                {
                    hellos = new string[] { "231231", "2222", "2321312312" },
                    ChoiceTestArray = new ChoiceTest[]{
                        new ChoiceTest()
                        {
                            ChoiceString=ChoiceEnum.a1,
                            Value="23343434sss"
                        },
                        new ChoiceTest()
                        {
                            ChoiceString=ChoiceEnum.a2,
                            Value="23345553434sss"
                        },
                        new ChoiceTest()
                        {
                            ChoiceString=ChoiceEnum.b1,
                            Value="23343434sss"
                        },
                    }
                });
            }
            Trace.WriteLine(sb.ToString());
            using (TextReader reader = new StringReader(sb.ToString()))
            {
                var res = ser.Deserialize(reader) as Test;
            }

        }
        [Fact]
        public void TestForListSeri()
        {
            //var instance = new SwComponentDocumentation();
            //var info = new Chapter()
            //{
            //    ShortName = new Identifier() { XmlContent = "231231" },
            //};
            //var content = new MsrQueryP1();
            //content.MsrQueryResultP1 = new TopicContent();
            //content.MsrQueryResultP1.Add(new MultiLanguageParagraph());
            //instance.SwFeatureDef = info;

            //XmlSerializer ser = new XmlSerializer(instance.GetType());
            //StringBuilder sb = new StringBuilder();

            //using (XmlWriter writer = XmlWriter.Create(sb, new XmlWriterSettings() { Encoding = Encoding.UTF8, Indent = true }))
            //{
            //    ser.Serialize(writer, instance);
            //}
            //Trace.WriteLine(sb.ToString());
            //using (TextReader reader = new StringReader(sb.ToString()))
            //{
            //    var res = ser.Deserialize(reader);
            //}
        }
        public class ChoiceTest
        {

            public ChoiceEnum ChoiceString { get; set; }
            public string Value { get; set; }
        }
        public enum ChoiceEnum
        {
            a1,
            a2,
            b1,
            b2,
        }
        public class Test
        {
            [XmlText]
            public string Hello3 { get; set; } = "1212";
            [XmlElement("test")]
            public string ELELE { get; set; } = "2323232";
            [XmlArray()]
            public string[] hellos { get; set; }
            [XmlElement("a1", typeof(ChoiceTest))]
            [XmlElement("a2", typeof(ChoiceTest))]
            [XmlElement("b1", typeof(ChoiceTest))]
            [XmlElement("b2", typeof(ChoiceTest))]
            [XmlChoiceIdentifier("ChoiceString")]
            public ChoiceTest[] ChoiceTestArray { get; set; }
            [XmlIgnore]
            public ChoiceEnum[] ChoiceString
            {
                get => ChoiceTestArray.Select(ele => ele.ChoiceString).ToArray(); set
                {
                    for (int i = 0; i < value.Length; i++)
                    {
                        ChoiceTestArray[i].ChoiceString = value[i];
                    }
                }
            }
        }
        
    }

}
