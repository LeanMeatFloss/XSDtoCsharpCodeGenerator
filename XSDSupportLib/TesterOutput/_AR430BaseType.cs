using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Linq;
using System.IO;
using System.Xml.Serialization;
using System.ComponentModel;
using System.Security.Cryptography;
using System.Xml;
using System.Diagnostics;
using System.Threading;
namespace AR430
{
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
    public sealed class DescriptionAttribute:Attribute
    {
        public DescriptionAttribute()
        {
           
        }
        public string Desc { get; set; }
    }
    public abstract partial class _AR430BaseType : INotifyPropertyChanged
    {
        static Dictionary<Type, XmlSerializer> _serializerDict { get; set; } = new Dictionary<Type, XmlSerializer>();
        static MD5 _md5 { get; set; } = MD5.Create();
        public _AR430BaseType()
        {
            if (!_serializerDict.ContainsKey(this.GetType()))
            {
                _serializerDict[this.GetType()] = new XmlSerializer(this.GetType());
            }
            //if has property T, or S
            PropertyInfo timeStampProperty = this.GetType().GetProperty("T");
            PropertyInfo checkSumProperty = this.GetType().GetProperty("S");
            PropertyInfo uuidProperty = this.GetType().GetProperty("Uuid");
            if (checkSumProperty != null && checkSumProperty.PropertyType.Equals(typeof(string)) && timeStampProperty != null && timeStampProperty.PropertyType.Equals(typeof(string)))
            {
                this.PropertyChanged += handlingPropertyChanged;
            }
            if(uuidProperty!=null&&uuidProperty.PropertyType.Equals(typeof(string)))
            {
                string guid = Guid.NewGuid().ToString();
                uuidProperty.SetValue(this, guid);
            }
            
        }
        private static void handlingPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (_initLock.CurrentReadCount==0&&sender.GetType().GetProperty(e.PropertyName).GetCustomAttribute<XmlIgnoreAttribute>()!=null&&!e.PropertyName.Equals("S") && !e.PropertyName.Equals("T")&&_initLock.TryEnterWriteLock(1)&&sender is _AR430BaseType baseType )
            {
                try
                {
                    //if has property T, or S
                    PropertyInfo timeStampProperty = baseType.GetType().GetProperty("T");
                    PropertyInfo checkSumProperty = baseType.GetType().GetProperty("S");
                    if (checkSumProperty.PropertyType.Equals(typeof(string)) && timeStampProperty.PropertyType.Equals(typeof(string)))
                    {
                        //add timestamp or find upper element to add time stamp
                        string timeStamp = DateTime.Now.ToString("yyyy-MM-ddTHH:mm:ssZ");
                        //check sum use the property changed
                        checkSumProperty.SetValue(baseType, "");
                        var hash = BitConverter.ToString(_md5.ComputeHash(Encoding.UTF8.GetBytes(baseType.GetXmlOutput())));
                        timeStampProperty.SetValue(baseType, timeStamp);
                        checkSumProperty.SetValue(baseType, hash);
                    }
                }
                catch(Exception ex)
                {

                }
                finally
                {
                    _initLock.ExitWriteLock();
                }              
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;
        public virtual bool ShouldSerializeSpace()
        {
            PropertyInfo spaceProperty = this.GetType().GetProperty("Space");
            if(spaceProperty!=null&&spaceProperty.PropertyType.Equals(typeof(XmlSpace))&&spaceProperty.GetValue(this)!=null)
            {
                return true;
            }
            return false;
        }

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
        static ReaderWriterLockSlim _initLock { get; set; } = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
        [XmlIgnore]
        public string _ArxmlFilePath { get; set; }
        public static void GetInstance<T>(string filePath, out T instance) where T : _AR430BaseType
        {
            _initLock.EnterReadLock();
            //handling parent 
            void handlingParent(_AR430BaseType item)
            {
                foreach (var filteredItem in GetNoneEmptyBaseItems(item))
                {
                    filteredItem._AutosarParent = item;
                    filteredItem._ArxmlFilePath = filePath;
                    handlingParent(filteredItem);
                }
            }
            XmlSerializer ser = new XmlSerializer(typeof(T));
            using (Stream reader = File.OpenRead(filePath))
            {
                instance = ser.Deserialize(reader) as T;
                instance._ArxmlFilePath = filePath;
                handlingParent(instance);
            }
            _initLock.ExitReadLock();

        }
        public virtual _AR430BaseType GetElementByPath(string path)
        {
            _AR430BaseType curr = this;
            if(string.IsNullOrEmpty(path))
            {
                return null;
            }
            foreach (var item in path.Split(new string[] { "/" }, StringSplitOptions.RemoveEmptyEntries))
            {
                curr = curr[item];
                if (curr == null)
                {
                    return null;
                }
            }
            return curr;
        }
        public virtual _AR430BaseType this[string Name]
        {
            get
            {
                //first search its properties that contains identify
                IEnumerable<_AR430BaseType> getIdenItem(object input)
                {
                    return input.GetType()
                        .GetProperties()
                        .Where(prop => prop.GetIndexParameters().Length == 0)
                        .Where(prop => prop.GetValue(input) != null)
                        .Where(prop => prop.PropertyType.IsArray)
                        .Select(prop => prop.GetValue(input) as object[])
                        .SelectMany(item => item.Select(ele => ele as _AR430BaseType))
                        .Where(Item => Item != null)
                        .Where(item => item.GetType().GetProperties().Select(subProp => subProp.PropertyType).Contains(typeof(Identifier)))
                        .Concat(input.GetType()
                            .GetProperties()
                            .Where(prop => prop.GetIndexParameters().Length == 0)
                            .Where(prop => prop.GetValue(input) != null)
                            .Where(prop => !prop.GetType().IsArray)
                            .Select(prop => prop.GetValue(input) as _AR430BaseType)
                            .Where(item => item != null)
                            .Where(item => !item.GetType().GetProperties().Select(subProp => subProp.PropertyType).Contains(typeof(Identifier)))
                            .SelectMany(item => getIdenItem(item))
                        )
                        .Concat(input.GetType()
                            .GetProperties()
                            .Where(prop => prop.GetIndexParameters().Length == 0)
                            .Where(prop => prop.GetValue(input) != null)
                            .Where(prop => !prop.GetType().IsArray)
                            .Select(prop => prop.GetValue(input) as _AR430BaseType)
                            .Where(item => item != null)
                            .Where(item => item.GetType().GetProperties().Select(subProp => subProp.PropertyType).Contains(typeof(Identifier)))
                        );
                }
                var res = getIdenItem(this).ToList();
                return res
                    .Where(ele => ((ele.GetType()
                        .GetProperties()
                        .Where(prop =>prop.PropertyType.Equals(typeof(Identifier))).First().GetValue(ele)?.ToString()?.Equals(Name))??false)
                            ).FirstOrDefault();

            }
        }
        public virtual string GetAutosarPath()
        {
            _AR430BaseType curr = this;
            StringBuilder path = new StringBuilder();
            do
            {
                if (curr.GetType().GetProperties().Select(prop => prop.PropertyType).Contains(typeof(Identifier)))
                {
                    path.Insert(0, "/" + curr.GetType().GetProperties().Where(ele => ele.PropertyType.Equals(typeof(Identifier))).First().GetValue(curr)?.ToString());
                    curr = curr._AutosarParent;
                }
                else if (path.Length == 0)
                {
                    return null;
                }
                else
                {
                    curr = curr._AutosarParent;
                }
            }
            while (curr != null);
            return path.ToString();
        }
        public static IEnumerable<_AR430BaseType> GetNoneEmptyBaseItems(_AR430BaseType item)
        {
            var propertyValList = item.GetType()
                    .GetProperties()
                    .Where(ele => !ele.PropertyType.IsArray && ele.PropertyType.IsSubclassOf(typeof(_AR430BaseType)))
                    .Select(ele => ele.GetValue(item) as _AR430BaseType)
                    .Where(val => val != null);
            var propertyArrayValList = item.GetType()
                .GetProperties()
                .Where(ele => ele.PropertyType.IsArray && ele.PropertyType.GetElementType().IsSubclassOf(typeof(_AR430BaseType)))
                .Where(ele => ele.GetValue(item) != null)
                .SelectMany(ele => ele.GetValue(item) as _AR430BaseType[])
                .Where(val => val != null);
            return propertyValList.Concat(propertyArrayValList);
        }
        public virtual void GetElementArrayByType<T>(out T[] array)where T:_AR430BaseType
        {
            IEnumerable<T> handleProperties(_AR430BaseType item)
            {
                var list = GetNoneEmptyBaseItems(item).ToArray();
                return list.Where(ele => ele.GetType().Equals(typeof(T))||ele.GetType().IsSubclassOf(typeof(T)))
                    .Select(ele=>ele as T).Concat(list.SelectMany(subItem => handleProperties(subItem)));
            }
            array = handleProperties(this).ToArray();
        }
        [XmlIgnore]
        public virtual _AR430BaseType _AutosarParent { get; set; }
        public override string ToString()
        {

            PropertyInfo idenProp = this.GetType().GetProperties().Where(prop => prop.GetIndexParameters().Length == 0).Where(prop => prop.PropertyType.Equals(typeof(Identifier))).FirstOrDefault();
            PropertyInfo _xmlTextProp = this.GetType().GetProperties().Where(prop => prop.GetIndexParameters().Length == 0).Where(prop => prop.Name.Equals("_XmlText")).FirstOrDefault();
            if (idenProp != null && idenProp.GetValue(this) != null)
            {
                return idenProp.GetValue(this).ToString();
            }
            else if (_xmlTextProp != null && _xmlTextProp.GetValue(this) != null)
            {
                return _xmlTextProp.GetValue(this).ToString();
            }
            else
            {
                return base.ToString();
            }
        }
        public static bool _IsIdentify(Type type)
        {
            return type.GetProperties().Select(prop => prop.PropertyType).Contains(typeof(Identifier));
        }
    }
}
