using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace JUnit
{
	public abstract partial class _JUnitBaseType
	{
		static Dictionary<Type, XmlSerializer> _serializerDict { get; set; } = new Dictionary<Type, XmlSerializer>();
		public string GetXmlOutput()
		{
			StringBuilder sb = new StringBuilder();

			try
			{
				using (XmlWriter writer = XmlWriter.Create(sb, new XmlWriterSettings() { Encoding = Encoding.UTF8, Indent = true }))
				{
					_serializerDict[this.GetType()].Serialize(writer, this);
				}
			}
			catch (Exception e)
			{
				Trace.WriteLine(e.Message + e.StackTrace);
			}
			return sb.ToString();
		}
		public _JUnitBaseType()
        {
			if (!_serializerDict.ContainsKey(this.GetType()))
			{
				_serializerDict[this.GetType()] = new XmlSerializer(this.GetType());
			}
		}
	}
	public enum XmlSpace
	{
		[XmlEnum("none")]
		None,
		[XmlEnum("default")]
		Default,
		[XmlEnum("preserve")]
		Preserve,
	}
	[AttributeUsage(AttributeTargets.All)]
	public sealed class DescriptionAttribute : Attribute
	{
		public DescriptionAttribute()
		{

		}
		public string Desc { get; set; }
	}

}
