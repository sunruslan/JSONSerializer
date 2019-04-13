using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace JSONSerializer
{
    public static class Serializer
    {
        public static void Serialize<T>(StreamWriter stream, T obj, string name="")
        {
            if (stream == null) throw new ArgumentNullException();
            if (obj != null && IsTypeSimple(obj.GetType()))
            {
                stream.Write(StartJsonObject);
                WriteSimpleObject(stream, obj, name);
                stream.Write("\n"+EndJsonObject);
            }
            else
            {
                WriteObject(stream, obj);
            }
        }

        public static T Deserialize<T>(StreamReader stream, T obj)
        {
            if (stream == null) throw new ArgumentNullException();
            if (obj == null) obj = default(T);
            if (obj == null) return default(T);
            string line;
            while (!stream.EndOfStream)
            {
                line = stream.ReadLine();
                if (!line.Contains('\"')) continue;
                string name = String.Concat(line.Skip(1).TakeWhile(c => c != '\"'));
                string value = String.Concat(line.Skip(name.Length + 5).TakeWhile(c => c != '\"'));
                var field = obj.GetType().GetField(name);
                if (field != null)
                {
                    if (IsTypeSimple(field.FieldType)) field.SetValue(obj, Convert.ChangeType(value, field.FieldType));
                    else if (field.GetValue(obj) is ICollection) ReadArray(stream, field.GetValue(obj) as IList);
                    else Deserialize(stream, field.GetValue(obj));
                }
                var property = obj.GetType().GetProperty(name);
                if (property != null)
                {
                    if (IsTypeSimple(property.PropertyType))
                        property.SetValue(obj, Convert.ChangeType(value, property.PropertyType));
                    else if (property.GetValue(obj) is ICollection)
                        ReadArray(stream, property.GetValue(obj) as IList);
                    else Deserialize(stream, property.GetValue(obj));
                }
            }
            return obj;
        }
        
        private static void WriteObject<T>(StreamWriter stream, T obj)
        {
            stream.Write(StartJsonObject);

            if (obj != null)
            {
                var lastField = obj.GetType().GetFields().LastOrDefault(f => f.IsPublic);
                var lastProperty = obj.GetType().GetProperties().LastOrDefault(p => p.CanRead);
                foreach (var field in obj.GetType().GetFields())
                {
                    if (field.IsPublic)
                    {
                        if (IsTypeSimple(field.FieldType)) WriteSimpleObject(stream, field.GetValue(obj), field.Name);
                        else if (field.GetValue(obj) is ICollection) WriteArray(stream, field.GetValue(obj) as ICollection, field.Name);
                        else
                        {
                            stream.Write($"\"{field.Name}\": ");
                            WriteObject(stream, field.GetValue(obj));
                        }
                        stream.Write(lastProperty != null || !field.Equals(lastField) ? JsonElementsSeparator : "\n");
                    }
                }
                foreach (var property in obj.GetType().GetProperties())
                {
                    if (property.CanRead)
                    {
                        if (IsTypeSimple(property.PropertyType)) WriteSimpleObject(stream, property.GetValue(obj), property.Name);
                        else if (property.GetValue(obj) is ICollection) WriteArray(stream, property.GetValue(obj) as ICollection, property.Name);
                        else
                        {
                            stream.Write($"\"{property.Name}\": ");
                            WriteObject(stream, property.GetValue(obj));
                        }
                        stream.Write(lastProperty.Equals(property) ? "\n" : JsonElementsSeparator);
                    }
                }
            }
            stream.Write(EndJsonObject);
        }

        private static void WriteArray(StreamWriter stream, ICollection arr, string name)
        {
            stream.Write($"\"{name}\": ");
            stream.Write(StartJsonArray);
            var count = arr.Count;
            foreach (var element in arr)
            {
                WriteArrayElement(stream, element);
                --count;
                stream.Write(count > 0 ? ",\n" : "\n");
            }
            stream.Write(EndJsonArray);
        }
        
        private static void WriteArrayElement<T>(StreamWriter stream, T obj)
        {
            stream.Write($"\"{obj}\"");
        }
        
        private static void WriteSimpleObject<T>(StreamWriter stream, T obj, string name)
        {
            stream.Write($"\"{name}\": \"{obj}\"");
        }

        private static bool IsTypeSimple(Type type) => type.IsPrimitive || type == typeof(string);
        
        private static void ReadArray(StreamReader stream, IList arr)
        {
            if (arr == null) return;
            string line = stream.ReadLine();
            while (line != "]")
            {
                string value = String.Concat(line.Skip(1).TakeWhile(c => c != '\"'));
                var type = arr.GetType().GetElementType();
                arr.Add(Convert.ChangeType(value, typeof(object)));
                line = stream.ReadLine();
            }
        }

        private const string StartJsonObject = "{\n";
        private const string EndJsonObject = "}";
        private const string StartJsonArray = "[\n";
        private const string EndJsonArray = "]";
        private const string JsonElementsSeparator = ",\n";
    }
}
