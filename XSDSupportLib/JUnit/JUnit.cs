
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.Text;

namespace JUnit
{

    public abstract partial class _JUnitBaseType
    {
		
	}

	///<summary>
	///Documents:
	///Contains the results of exexuting a testsuite
	///
	///AppInfo:
	///</summary>
	[Description(Desc=@"Documents:
Contains the results of exexuting a testsuite

AppInfo:")]
	[XmlRoot(ElementName="testsuite",Namespace="")]
	public class Testsuite : _JUnitBaseType
	{
		[XmlText(Type=typeof(string))]
		public virtual string _XmlText {get;set;}

        ///<summary>
		///Documents:
		///Full class name of the test for non-aggregated testsuite documents. Class name without the package for aggregated testsuites documents
		///
		///AppInfo:
        ///</summary>
        [Description(Desc=@"Documents:
Full class name of the test for non-aggregated testsuite documents. Class name without the package for aggregated testsuites documents

AppInfo:")]
		[XmlAttribute(AttributeName="name",Namespace="")]
		public virtual Token Name {get;set;}

        ///<summary>
		///Documents:
		///when the test was executed. Timezone may not be specified.
		///
		///AppInfo:
        ///</summary>
        [Description(Desc=@"Documents:
when the test was executed. Timezone may not be specified.

AppInfo:")]
		[XmlAttribute(AttributeName="timestamp",Namespace="")]
		public virtual Datetime Timestamp {get;set;}

        ///<summary>
		///Documents:
		///Host on which the tests were executed. 'localhost' should be used if the hostname cannot be determined.
		///
		///AppInfo:
        ///</summary>
        [Description(Desc=@"Documents:
Host on which the tests were executed. 'localhost' should be used if the hostname cannot be determined.

AppInfo:")]
		[XmlAttribute(AttributeName="hostname",Namespace="")]
		public virtual Token Hostname {get;set;}

        ///<summary>
		///Documents:
		///The total number of tests in the suite
		///
		///AppInfo:
        ///</summary>
        [Description(Desc=@"Documents:
The total number of tests in the suite

AppInfo:")]
		[XmlAttribute(AttributeName="tests",Namespace="http://www.w3.org/2001/XMLSchema")]
		public virtual Long Tests {get;set;}

        ///<summary>
		///Documents:
		///The total number of tests in the suite that failed. A failure is a test which the code has explicitly failed by using the mechanisms for that purpose. e.g., via an assertEquals
		///
		///AppInfo:
        ///</summary>
        [Description(Desc=@"Documents:
The total number of tests in the suite that failed. A failure is a test which the code has explicitly failed by using the mechanisms for that purpose. e.g., via an assertEquals

AppInfo:")]
		[XmlAttribute(AttributeName="failures",Namespace="http://www.w3.org/2001/XMLSchema")]
		public virtual Long Failures {get;set;}

        ///<summary>
		///Documents:
		///The total number of tests in the suite that errorrd. An errored test is one that had an unanticipated problem. e.g., an unchecked throwable; or a problem with the implementation of the test.
		///
		///AppInfo:
        ///</summary>
        [Description(Desc=@"Documents:
The total number of tests in the suite that errorrd. An errored test is one that had an unanticipated problem. e.g., an unchecked throwable; or a problem with the implementation of the test.

AppInfo:")]
		[XmlAttribute(AttributeName="errors",Namespace="http://www.w3.org/2001/XMLSchema")]
		public virtual Long Errors {get;set;}

        ///<summary>
		///Documents:
		///Time taken (in seconds) to execute the tests in the suite
		///
		///AppInfo:
        ///</summary>
        [Description(Desc=@"Documents:
Time taken (in seconds) to execute the tests in the suite

AppInfo:")]
		[XmlAttribute(AttributeName="time",Namespace="http://www.w3.org/2001/XMLSchema")]
		public virtual Anysimpletype Time {get;set;}

        ///<summary>
		///Documents:
		///Properties (e.g., environment settings) set during test execution
		///
		///AppInfo:
        ///</summary>
        [Description(Desc=@"Documents:
Properties (e.g., environment settings) set during test execution

AppInfo:")]
		[XmlArray(ElementName="properties",Namespace="")]
		[XmlArrayItem(ElementName="property",Namespace="")]
		public virtual TestsuitePropertiesProperty[] Properties {get;set;}
		[XmlElement(ElementName="testcase",Namespace="")]
		public virtual TestsuiteTestcase[] Testcases {get;set;}

        ///<summary>
		///Documents:
		///Data that was written to standard out while the test was executed
		///
		///AppInfo:
        ///</summary>
        [Description(Desc=@"Documents:
Data that was written to standard out while the test was executed

AppInfo:")]
		[XmlElement(ElementName="system-out",Namespace="")]
		public virtual PreString SystemOut {get;set;}

        ///<summary>
		///Documents:
		///Data that was written to standard error while the test was executed
		///
		///AppInfo:
        ///</summary>
        [Description(Desc=@"Documents:
Data that was written to standard error while the test was executed

AppInfo:")]
		[XmlElement(ElementName="system-err",Namespace="")]
		public virtual PreString SystemErr {get;set;}

	}
	public class TestsuiteName : _JUnitBaseType
	{
		[XmlText(Type=typeof(string))]
		public virtual string _XmlText {get;set;}

	}
	public class Token : _JUnitBaseType
	{
		[XmlText(Type=typeof(string))]
		public virtual string _XmlText {get;set;}

	}
	public class Datetime : _JUnitBaseType
	{
		[XmlText(Type=typeof(string))]
		public virtual string _XmlText {get;set;}

	}
	public class TestsuiteHostname : _JUnitBaseType
	{
		[XmlText(Type=typeof(string))]
		public virtual string _XmlText {get;set;}

	}
	public class Long : _JUnitBaseType
	{
		[XmlText(Type=typeof(string))]
		public virtual string _XmlText {get;set;}

	}
	public class Anysimpletype : _JUnitBaseType
	{
		[XmlText(Type=typeof(string))]
		public virtual string _XmlText {get;set;}

	}
	public class TestsuitePropertiesProperty : _JUnitBaseType
	{
		[XmlText(Type=typeof(string))]
		public virtual string _XmlText {get;set;}
		[XmlAttribute(AttributeName="name",Namespace="")]
		public virtual Token Name {get;set;}
		[XmlAttribute(AttributeName="value",Namespace="http://www.w3.org/2001/XMLSchema")]
		public virtual Anysimpletype Value {get;set;}

	}
	public class TestsuitePropertiesPropertyName : _JUnitBaseType
	{
		[XmlText(Type=typeof(string))]
		public virtual string _XmlText {get;set;}

	}
	public class TestsuiteTestcase : _JUnitBaseType
	{
		[XmlText(Type=typeof(string))]
		public virtual string _XmlText {get;set;}

        ///<summary>
		///Documents:
		///Name of the test method
		///
		///AppInfo:
        ///</summary>
        [Description(Desc=@"Documents:
Name of the test method

AppInfo:")]
		[XmlAttribute(AttributeName="name",Namespace="http://www.w3.org/2001/XMLSchema")]
		public virtual Normalizedstring Name {get;set;}

        ///<summary>
		///Documents:
		///Full class name for the class the test method is in.
		///
		///AppInfo:
        ///</summary>
        [Description(Desc=@"Documents:
Full class name for the class the test method is in.

AppInfo:")]
		[XmlAttribute(AttributeName="classname",Namespace="http://www.w3.org/2001/XMLSchema")]
		public virtual Normalizedstring Classname {get;set;}

        ///<summary>
		///Documents:
		///Time taken (in seconds) to execute the test
		///
		///AppInfo:
        ///</summary>
        [Description(Desc=@"Documents:
Time taken (in seconds) to execute the test

AppInfo:")]
		[XmlAttribute(AttributeName="time",Namespace="http://www.w3.org/2001/XMLSchema")]
		public virtual Anysimpletype Time {get;set;}

        ///<summary>
		///Documents:
		///Indicates that the test errored.  An errored test is one that had an unanticipated problem. e.g., an unchecked throwable; or a problem with the implementation of the test. Contains as a text node relevant data for the error, e.g., a stack trace
		///
		///AppInfo:
        ///</summary>
        [Description(Desc=@"Documents:
Indicates that the test errored.  An errored test is one that had an unanticipated problem. e.g., an unchecked throwable; or a problem with the implementation of the test. Contains as a text node relevant data for the error, e.g., a stack trace

AppInfo:")]
		[XmlElement(ElementName="error",Namespace="")]
		public virtual TestsuiteTestcaseError Error {get;set;}

        ///<summary>
		///Documents:
		///Indicates that the test failed. A failure is a test which the code has explicitly failed by using the mechanisms for that purpose. e.g., via an assertEquals. Contains as a text node relevant data for the failure, e.g., a stack trace
		///
		///AppInfo:
        ///</summary>
        [Description(Desc=@"Documents:
Indicates that the test failed. A failure is a test which the code has explicitly failed by using the mechanisms for that purpose. e.g., via an assertEquals. Contains as a text node relevant data for the failure, e.g., a stack trace

AppInfo:")]
		[XmlElement(ElementName="failure",Namespace="")]
		public virtual TestsuiteTestcaseFailure Failure {get;set;}

	}
	public class Normalizedstring : _JUnitBaseType
	{
		[XmlText(Type=typeof(string))]
		public virtual string _XmlText {get;set;}

	}
	public class TestsuiteTestcaseError
	{
		[XmlText(Type=typeof(string))]
		public virtual string _XmlText {get;set;}

        public override string ToString()
        {
            return _XmlText.ToString();
        }

        ///<summary>
		///Documents:
		///The error message. e.g., if a java exception is thrown, the return value of getMessage()
		///
		///AppInfo:
        ///</summary>
        [Description(Desc=@"Documents:
The error message. e.g., if a java exception is thrown, the return value of getMessage()

AppInfo:")]
		[XmlAttribute(AttributeName="message",Namespace="http://www.w3.org/2001/XMLSchema")]
		public virtual Anysimpletype Message {get;set;}

        ///<summary>
		///Documents:
		///The type of error that occured. e.g., if a java execption is thrown the full class name of the exception.
		///
		///AppInfo:
        ///</summary>
        [Description(Desc=@"Documents:
The type of error that occured. e.g., if a java execption is thrown the full class name of the exception.

AppInfo:")]
		[XmlAttribute(AttributeName="type",Namespace="http://www.w3.org/2001/XMLSchema")]
		public virtual Anysimpletype Type {get;set;}

	}
	public class TestsuiteTestcaseFailure
	{
		[XmlText(Type=typeof(string))]
		public virtual string _XmlText {get;set;}

        public override string ToString()
        {
            return _XmlText.ToString();
        }

        ///<summary>
		///Documents:
		///The message specified in the assert
		///
		///AppInfo:
        ///</summary>
        [Description(Desc=@"Documents:
The message specified in the assert

AppInfo:")]
		[XmlAttribute(AttributeName="message",Namespace="http://www.w3.org/2001/XMLSchema")]
		public virtual Anysimpletype Message {get;set;}

        ///<summary>
		///Documents:
		///The type of the assert.
		///
		///AppInfo:
        ///</summary>
        [Description(Desc=@"Documents:
The type of the assert.

AppInfo:")]
		[XmlAttribute(AttributeName="type",Namespace="http://www.w3.org/2001/XMLSchema")]
		public virtual Anysimpletype Type {get;set;}

	}
	public class TestsuiteSystemOut : _JUnitBaseType
	{
		[XmlText(Type=typeof(string))]
		public virtual string _XmlText {get;set;}

	}
	public class PreString : _JUnitBaseType
	{
		[XmlText(Type=typeof(string))]
		public virtual string _XmlText {get;set;}

	}
	public class TestsuiteSystemErr : _JUnitBaseType
	{
		[XmlText(Type=typeof(string))]
		public virtual string _XmlText {get;set;}

	}
}
