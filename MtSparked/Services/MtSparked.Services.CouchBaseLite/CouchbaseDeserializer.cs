using System;
using System.Reflection;
using Newtonsoft.Json;

using MtSparked.Interop.Models;
using Couchbase.Lite;

namespace MtSparked.Services.CouchBaseLite {
    public static class CouchbaseDeserializer {

        public static T FromResult<T>(IDictionaryObject result) where T : Model => (T)FromResult(typeof(T), result);

        public static object FromResult(Type type, IDictionaryObject result) { 
            object builtObject = Activator.CreateInstance(type);
            foreach (PropertyInfo info in type.GetProperties()) {
                if (info.GetIndexParameters().Length > 0 || info.IsDefined(typeof(JsonIgnoreAttribute))) {
                    continue;
                }
                string name = info.Name;
                JsonPropertyAttribute jsonProperty = info.GetCustomAttribute<JsonPropertyAttribute>();
                if (!(jsonProperty is null) && !(jsonProperty.PropertyName is null)) {
                    name = jsonProperty.PropertyName;
                }
                object value = null;
                // TODO: Handle IEnumerables/Arrays and Dictionaries
                if (info.PropertyType == typeof(bool)) {
                    value = result.GetBoolean(info.Name);
                } else if (info.PropertyType == typeof(DateTimeOffset)) {
                    value = result.GetDate(info.Name);
                } else if (info.PropertyType == typeof(double)) {
                    value = result.GetDouble(info.Name);
                } else if (info.PropertyType == typeof(float)) {
                    value = result.GetFloat(info.Name);
                } else if (info.PropertyType == typeof(int)) {
                    value = result.GetInt(info.Name);
                } else if (info.PropertyType == typeof(long)) {
                    value = result.GetLong(info.Name);
                } else if (info.PropertyType == typeof(string)) {
                    value = result.GetString(info.Name);
                } else { 
                    IDictionaryObject recursive = result.GetDictionary(name);
                    if(!(recursive is null)) {
                        value = CouchbaseDeserializer.FromResult(info.PropertyType, recursive);
                    }
                }
                info.SetValue(builtObject, value);
            }
            return builtObject;
        }

    }
}
