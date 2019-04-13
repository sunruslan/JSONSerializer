using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace JSONSerializer
{
    public class Program
    {
        public static void Main(string[] args)
        {

            object obj = new ClassWithArray{Array = new List<string>{"1", "2", "3"}};
            using (var stream = new StreamWriter("ClassWithArray.json"))
            {
                Serializer.Serialize(stream, obj);
            }
            //foreach (var property in typeof(ClassWithArray).GetProperties())
            //{
            //    foreach (var str in property.GetValue(obj) as IEnumerable)
            //    {
            //        Console.WriteLine(str);
            //    }


            //}
        }

    }

    public class SimpleObject
    {
        public int a ;
        public int a1;
        public string b ;
        public object c;
        private int d;


        public int A { get; set; } = 1;
        public int A1 { get; set; }
        public string B { get; set; } = "b";
        public object C { get; set; } = null;
        private int D { get; set; } = 3;
    }

    public class Person
    {
        public string Name { get; set; }

        public Home Home { get; set; } = new Home();
    }

    public class Home
    {
        public string Address { get; set; }
        public int numRooms = 99;
    }

    public class ClassWithArray
    {
        public ICollection Array { get; set; } = new List<string>();
    }

    public class ComplexClass
    {
        public ClassWithArray ClassWithArray { get; set; } = new ClassWithArray();
        public Person Person { get; set; } = new Person();
    }

}
