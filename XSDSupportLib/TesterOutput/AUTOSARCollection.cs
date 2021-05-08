using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;

namespace AR430
{
    public class AUTOSARCollection
    {
        public Dictionary<string, Autosar> Collection { get; set; } = new Dictionary<string, Autosar>();
        public string HandlerPath { get; private set; }
        private object HandlerObj { get; set; } = new object();
        public AUTOSARCollection StartHandler(string handlerPath)
        {
            Trace.WriteLine("Start handler");
            while (!Monitor.TryEnter(HandlerObj, 100000))
            {
                Trace.WriteLine("Wait beforehandler end.");
            }
            Trace.WriteLine($"Start handle {handlerPath}");
            HandlerPath = handlerPath;
            return this;
        }
        public static AUTOSARCollection LoadFile(string[] fileLocArray)
        {
            AUTOSARCollection collection = new AUTOSARCollection();
            foreach (var file in fileLocArray)
            {
                try
                {
                    Autosar.GetInstance(file, out Autosar package);
                    collection.Collection[file] = package;
                }
                catch (Exception e)
                {
                    throw new Exception($"Error happen in {file}", e);
                }

            }
            return collection;
        }
        public static T GetConcatElementFromArrayByName<T>(T[] collectionArray,string name) where T:_AR430BaseType
        {
            if(_AR430BaseType._IsIdentify(typeof(T)))
            {
                return ConcatItem(collectionArray.Where(ele => ele.ToString().Equals(name)).ToArray()) as T;
            }
            else
            {
                return null;
            }
        }

        public static object ConcatItem(object[] inputArray)
        {
            if(inputArray==null||inputArray.Length==0)
            {
                return null;
            }
            object concatResult = Activator.CreateInstance(inputArray[0].GetType());
            PropertyInfo[] propertyInfos = concatResult.GetType().GetProperties();
            foreach (PropertyInfo propertyinfo in propertyInfos.Where(prop => prop.GetCustomAttribute<XmlIgnoreAttribute>() == null && prop.CanWrite))
            {

                if (propertyinfo.PropertyType.IsSubclassOf(typeof(_AR430BaseType)))
                {
                    //continue concat
                    //Type arrayItemType = propertyinfo.PropertyType;
                    object[] subConcatRes = inputArray.Select(item => propertyinfo.GetValue(item)).Where(val => val != null).ToArray();
                    if (subConcatRes.Length == 0)
                    {
                        continue;
                    }
                    //Array arr = Array.CreateInstance(arrayItemType, subConcatRes.Length);
                    //Array.Copy(subConcatRes, arr, arr.Length);
                    var res = ConcatItem(subConcatRes);
                    propertyinfo.SetValue(concatResult, res);
                }
                else if(propertyinfo.PropertyType.IsArray)
                {
                    if(propertyinfo.PropertyType.GetElementType().IsSubclassOf(typeof(_AR430BaseType))&& _AR430BaseType._IsIdentify(propertyinfo.PropertyType.GetElementType()))
                    {
                        Type arrayItemType = propertyinfo.PropertyType.GetElementType();
                        
                        object[] subConcatRes = inputArray.Concat(new[] { concatResult })
                            .Select(item => propertyinfo.GetValue(item))
                            .Where(val => val != null)
                            .SelectMany(val=>val as object[])
                            .GroupBy(val => val.ToString())
                            .Select(group => ConcatItem(group.ToArray())).ToArray();
                        Array arr = Array.CreateInstance(arrayItemType, subConcatRes.Length);
                        Array.Copy(subConcatRes, arr, arr.Length);
                        //group by item name
                        propertyinfo.SetValue(concatResult, arr);
                    }
                    else
                    {
                        //none identify type
                        Type arrayItemType = propertyinfo.PropertyType.GetElementType();
                        object[] subConcatRes = inputArray.Concat(new[] { concatResult })
                            .Select(item => propertyinfo.GetValue(item))
                            .Where(val => val != null)
                            .SelectMany(val => val as object[])
                            .ToArray();
                        Array arr = Array.CreateInstance(arrayItemType, subConcatRes.Length);
                        Array.Copy(subConcatRes, arr, arr.Length);
                        propertyinfo.SetValue(concatResult, arr);
                    }
                }
                else
                {
                   
                    foreach(var item in inputArray)
                    {
                        //simple overwrite 
                        //try to assign value
                        object val = propertyinfo.GetValue(item);
                        if (propertyinfo.GetValue(concatResult) != null&& val!=null&& !propertyinfo.Name.Equals("Uuid")&& !propertyinfo.Name.Equals("S")&& !propertyinfo.Name.Equals("T"))
                        {
                            Trace.WriteLine($"Overwrite info at {concatResult.ToString()}.{propertyinfo.Name} to {val.ToString()}\n\tat {(item as _AR430BaseType)?.GetAutosarPath()}\n\tat {(item as _AR430BaseType)?._ArxmlFilePath}");
                        }
                        if(val!=null)
                        {
                            propertyinfo.SetValue(concatResult, val);
                        }
                        
                    }
                    
                }
            }
            return concatResult;
        }
        public object GetConcatResultByPath(string autoSarPath)
        {
            GetElementsByPath(autoSarPath, out _AR430BaseType[] elementArray);
            return ConcatItem(elementArray);

        }
        public void GetElementsByPath<T>(string autoSarPath, out T[] elementArray) where T : _AR430BaseType
        {
            elementArray = Collection.Values.SelectMany(ele =>
            {
                ele.GetElementArrayByType(out T[] subArray);
                return subArray;
            }).Where(ele => autoSarPath.Equals(ele.GetAutosarPath())).ToArray();
        }
        public void GetElementArrayByType<T>(out T[] array) where T : _AR430BaseType
        {
            array = Collection.Values.SelectMany(ele =>
              {
                  ele.GetElementArrayByType(out T[] subArray);
                  return subArray;
              }).ToArray();
        }
        public void EndHandler()
        {
            Trace.WriteLine("Handler end.");
            Monitor.Exit(HandlerObj);
        }
    }
}
