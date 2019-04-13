using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using System.Text;
using System.Threading;
using Xunit;

namespace JSONSerializer.Test
{
    public class UnitTest
    {
        #region Classes


        public class SimpleObject
        {
            public int a ;
            public int a1;
            public string b;
            public object c;
            private int d = 3;


            public int A { get; set; }
            public int A1 { get; set; }
            public string B { get; set; }
            public object C { get; set; }
            private int D { get; set; } = 3;
        }

        public class Person
        {
            public string Name { get; set; }

            public Home Home { get; set; }
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
            public ClassWithArray ClassWithArray { get; set; }
            public Person Person { get; set; }
        }

        #endregion

        #region Serialize tests
        [Fact]
        public void SerializeNullObjectTest()
        {
            using (var stream = new StreamWriter("Null.json", false, Encoding.Default))
            {
                Serializer.Serialize<object>(stream, null);
            }

            using (var stream = new StreamReader("Null.json"))
            {
                Assert.Equal("{\n}", stream.ReadToEnd());
            }
        }

        [Fact]
        public void SerializeIntObjectTest()
        {
            int test = 123;
            using (var stream = new StreamWriter("Int.json", false, Encoding.Default))
            {
                Serializer.Serialize<int>(stream, test, "test");
            }

            using (var stream = new StreamReader("Int.json"))
            {
                Assert.Equal("{\n\"test\": \"123\"\n}", stream.ReadToEnd());
            }
        }

        [Fact]
        public void SerializeStringObjectTest()
        {
            string test = "bla-bla-bla";
            using (var stream = new StreamWriter("String.json", false, Encoding.Default))
            {
                Serializer.Serialize<string>(stream, test, "test");
            }

            using (var stream = new StreamReader("String.json"))
            {
                Assert.Equal("{\n\"test\": \"bla-bla-bla\"\n}", stream.ReadToEnd());
            }
        }

        [Fact]
        public void SerializeSimpleObjectTest()
        {
            var obj = new SimpleObject {a = 1, b = "b", c = null, A = 1, B = "b", C = null};
            using (var stream = new StreamWriter("SimpleObject.json", false, Encoding.Default))
            {
                Serializer.Serialize(stream, obj);
            }

            using (var stream = new StreamReader("SimpleObject.json"))
            {
                Assert.Equal($@"{{
""a"": ""1"",
""a1"": ""0"",
""b"": ""b"",
""c"": {{
}},
""A"": ""1"",
""A1"": ""0"",
""B"": ""b"",
""C"": {{
}}
}}", stream.ReadToEnd());
            }
        }

        [Fact]
        public void SerializeWithNestedObjectTest()
        {
            var obj = new Person{Name = "Vlad", Home = new Home{Address = "Florida", numRooms = 10000}};
            using (var stream = new StreamWriter("ObjectWithNestedObject.json", false, Encoding.Default))
            {
                Serializer.Serialize(stream, obj);
            }

            using (var stream = new StreamReader("ObjectWithNestedObject.json"))
            {
                Assert.Equal($@"{{
""Name"": ""Vlad"",
""Home"": {{
""numRooms"": ""10000"",
""Address"": ""Florida""
}}
}}", stream.ReadToEnd());
            }
        }

        [Fact]
        public void SerializeWithArrayTest()
        {
            var obj = new ClassWithArray{Array = new List<string>{"1", "2", "3"}};
            using (var stream = new StreamWriter("ClassWithArray.json", false, Encoding.Default))
            {
                Serializer.Serialize(stream, obj);
            }

            using (var stream = new StreamReader("ClassWithArray.json"))
            {
                Assert.Equal($@"{{
""Array"": [
""1"",
""2"",
""3""
]
}}", stream.ReadToEnd());
            }
        }

        [Fact]
        public void SerializeComplexObjectTest()
        {
            var obj = new ComplexClass
            {
                ClassWithArray = new ClassWithArray { Array = new List<string> { "1", "2", "3" } },
                Person = new Person { Name = "Vlad", Home = new Home { Address = "Florida", numRooms = 10000 } }
            };
            using (var stream = new StreamWriter("ComplexClass.json", false, Encoding.Default))
            {
                Serializer.Serialize(stream, obj);
            }

            using (var stream = new StreamReader("ComplexClass.json"))
            {
                Assert.Equal($@"{{
""ClassWithArray"": {{
""Array"": [
""1"",
""2"",
""3""
]
}},
""Person"": {{
""Name"": ""Vlad"",
""Home"": {{
""numRooms"": ""10000"",
""Address"": ""Florida""
}}
}}
}}", stream.ReadToEnd());
            }
        }
        #endregion

        #region Deserialize tests
        [Fact]
        public void DeserializeSimpleObjectTest()
        {
            var obj = new SimpleObject();
            using (var stream = new StreamWriter("SimpleObject.json", false, Encoding.Default))
            {
                stream.Write(@"{{
""a"": ""1"",
""a1"": ""0"",
""b"": ""b"",
""c"": {{
}},
""A"": ""1"",
""A1"": ""0"",
""B"": ""b"",
""C"": {{
}}
}}");
            }

            using (var stream = new StreamReader("SimpleObject.json"))
            {
                Serializer.Deserialize(stream, obj);
            }
            Assert.Equal(1, obj.a);
            Assert.Equal(1, obj.A);
            Assert.Equal(null, obj.C);
        }

        [Fact]
        public void DeserializeObjectWithNestedObjectTest()
        {
            var obj = new Person{Home = new Home{Address = "Miami", numRooms = 20}, Name = "qwewqe"};
            using (var stream = new StreamWriter("ObjectWithNestedObject.json", false, Encoding.Default))
            {
                stream.Write($@"{{
""Name"": ""Vlad"",
""Home"": {{
""numRooms"": ""10000"",
""Address"": ""Florida""
}}
}}");
            }

            using (var stream = new StreamReader("ObjectWithNestedObject.json"))
            {
                Serializer.Deserialize(stream, obj);
            }
            Assert.Equal("Vlad", obj.Name);
            Assert.Equal(10000, obj.Home.numRooms);
            Assert.Equal("Florida", obj.Home.Address);
        }

        [Fact]
        public void DeserializeWithArrayTest()
        {
            var obj = new ClassWithArray();
            using (var stream = new StreamWriter("ClassWithArray.json", false, Encoding.Default))
            {
                stream.Write($@"{{
""Array"": [
""1"",
""2"",
""3""
]
}}");
            }

            using (var stream = new StreamReader("ClassWithArray.json"))
            {
                Serializer.Deserialize(stream, obj);
            }

            var list = new List<string> {"1", "2", "3"};
            Assert.Equal(list, obj.Array);
        }
        #endregion

    }
}
