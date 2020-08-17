using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.TSQL.Evaluator.Visitors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Traficante.TSQL.Evaluator.Helpers;

namespace Traficante.TSQL.Tests.Evaluator.Helpers
{
    [TestClass]
    public class AnonymousTypeBuilderTests
    {
        [TestMethod]
        public void Equals_SameFieldsValues_ReturnsTrue()
        {
            AnonymousTypeBuilder sut = new AnonymousTypeBuilder();
            Type t = sut.CreateAnonymousType(new[] { 
                ("Name", typeof(string)), 
                ("Age", typeof(int?)), 
                ("Hight", typeof(decimal?)),
                ("Weight", typeof(int?))
            });

            var object1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Name").SetValue(object1, "Daniel");
            t.GetField("Age").SetValue(object1, (int?)20);
            t.GetField("Hight").SetValue(object1, (decimal?)6.1);

            var object2 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Name").SetValue(object2, "Daniel");
            t.GetField("Age").SetValue(object2, (int?)20);
            t.GetField("Hight").SetValue(object2, (decimal?)6.1);

            Assert.IsTrue(object1.Equals(object2));
        }

        [TestMethod]
        public void Equals_DifferentFieldsValues_ReturnsFalse()
        {
            AnonymousTypeBuilder sut = new AnonymousTypeBuilder();
            Type t = sut.CreateAnonymousType(new[] {
                ("Name", typeof(string)),
                ("Age", typeof(int?)),
                ("Hight", typeof(decimal?)),
                ("Weight", typeof(int?))
            });

            var object1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Name").SetValue(object1, "Daniel");
            t.GetField("Age").SetValue(object1, (int?)20);
            t.GetField("Hight").SetValue(object1, (decimal?)6.1);

            var object2 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Name").SetValue(object2, "John");
            t.GetField("Age").SetValue(object2, (int?)20);
            t.GetField("Hight").SetValue(object2, (decimal?)6.1);

            Assert.IsFalse(object1.Equals(object2));
        }

        [TestMethod]
        public void Equals_FieldValueIsNull_ReturnsFalse()
        {
            AnonymousTypeBuilder sut = new AnonymousTypeBuilder();
            Type t = sut.CreateAnonymousType(new[] { ("Name", typeof(string)) });

            var object1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Name").SetValue(object1, null);

            var object2 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Name").SetValue(object2, "John");

            Assert.IsFalse(object1.Equals(object2));
        }

        [TestMethod]
        public void GetHashCode_FieldValueIsNull_ReturnsHash()
        {
            AnonymousTypeBuilder sut = new AnonymousTypeBuilder();
            Type t = sut.CreateAnonymousType(new[] { ("Name", typeof(string)) });

            var object1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Name").SetValue(object1, null);
            
            Assert.AreEqual(17 * 23 + 0, object1.GetHashCode());

        }

        [TestMethod]
        public void GetHashCode_FieldValueIsInt_ReturnsHash()
        {
            AnonymousTypeBuilder sut = new AnonymousTypeBuilder();
            Type t = sut.CreateAnonymousType(new[] { ("Age", typeof(int)) });

            var object1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Age").SetValue(object1, 20);

            //int hash = 17
            //hash = hash * 23 + Age.GetHashCode();
            
            Assert.AreEqual(17 * 23 + 20.GetHashCode(), object1.GetHashCode());
        }

        [TestMethod]
        public void GetHashCode_FieldValueIsNulableInt_ReturnsHash()
        {
            AnonymousTypeBuilder sut = new AnonymousTypeBuilder();
            Type t = sut.CreateAnonymousType(new[] { ("Age", typeof(int?)) });

            var object1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Age").SetValue(object1, 20);

            //int hash = 17
            //hash = hash * 23 + Age.GetHashCode();

            Assert.AreEqual(17 * 23 + 20.GetHashCode(), object1.GetHashCode());
        }

        [TestMethod]
        public void GetHashCode_FieldValueIsNulableIntAndIsNull_ReturnsHash()
        {
            int? a = null;
            var h = a.GetHashCode();
            AnonymousTypeBuilder sut = new AnonymousTypeBuilder();
            Type t = sut.CreateAnonymousType(new[] { ("Age", typeof(int?)) });

            var object1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Age").SetValue(object1, null);

            //int hash = 17
            //hash = hash * 23 + Age.GetHashCode();

            Assert.AreEqual(17 * 23 + ((int?)null).GetHashCode(), object1.GetHashCode());
        }

        [TestMethod]
        public void GetHashCode_FieldValueIsString_ReturnsHash()
        {
            AnonymousTypeBuilder sut = new AnonymousTypeBuilder();
            Type t = sut.CreateAnonymousType(new[] { ("Name", typeof(string)) });

            var object1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Name").SetValue(object1, "daniel");

            //int hash = 17
            //hash = hash * 23 + Name.GetHashCode()

            Assert.AreEqual(17 * 23 + "daniel".GetHashCode(), object1.GetHashCode());
        }

        [TestMethod]
        public void GetHashCode_CombplexObject_ReturnsHash()
        {
            AnonymousTypeBuilder sut = new AnonymousTypeBuilder();
            Type t = sut.CreateAnonymousType(new[] {
                ("Id", typeof(int?)),
                ("Name", typeof(string)),
                ("Age", typeof(short?)),
                ("Address", typeof(string)),
                ("Created", typeof(DateTime?))
            });

            var object1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Id").SetValue(object1, (int?)123);
            t.GetField("Name").SetValue(object1, "daniel");
            t.GetField("Age").SetValue(object1, (short?)30);
            t.GetField("Address").SetValue(object1, "London");
            t.GetField("Created").SetValue(object1, new DateTime(2019, 4, 12));

            int expectedhash = 17;
            expectedhash = expectedhash * 23 + ((int?)123).GetHashCode();
            expectedhash = expectedhash * 23 + "daniel".GetHashCode();
            expectedhash = expectedhash * 23 + ((short?)30).GetHashCode();
            expectedhash = expectedhash * 23 + "London".GetHashCode();
            expectedhash = expectedhash * 23 + ((DateTime?)new DateTime(2019, 4, 12)).GetHashCode();
            
            Assert.AreEqual(expectedhash, object1.GetHashCode());
        }

        [TestMethod]
        public void Assert_AreEqual()
        {
            AnonymousTypeBuilder sut = new AnonymousTypeBuilder();
            Type t = sut.CreateAnonymousType(new[] {
                ("Substr(Name, 0, 2)", typeof(System.String))
            });

            var obj1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Substr(Name, 0, 2)").SetValue(obj1, "AA");

            var obj2 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Substr(Name, 0, 2)").SetValue(obj2, "AA");


            Assert.AreEqual(obj1, obj2);
        }

        [TestMethod]
        public void EqualityCompare_ImplementsInterface()
        {
            AnonymousTypeBuilder sut = new AnonymousTypeBuilder();
            Type t = sut.CreateAnonymousType(new[] {
                ("Id", typeof(int?)),
                ("Name", typeof(string))
            });

            Type t_comparer = sut.CreateEqualityComparerForType(t, new string[] { "Name" });

            var implementsIEqualityComparer = typeof(IEqualityComparer<>).MakeGenericType(t).IsAssignableFrom(t_comparer);
            Assert.IsTrue(implementsIEqualityComparer);
        }

        [TestMethod]
        public void EqualityCompare_GetHashCode_CombplexObject_ReturnsHash()
        {
            AnonymousTypeBuilder sut = new AnonymousTypeBuilder();
            Type t = sut.CreateAnonymousType(new[] {
                ("Id", typeof(int?)),
                ("Name", typeof(string)),
                ("Age", typeof(short?)),
                ("Address", typeof(string)),
                ("Created", typeof(DateTime?))
            });

            var object1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Id").SetValue(object1, (int?)123);
            t.GetField("Name").SetValue(object1, "daniel");
            t.GetField("Age").SetValue(object1, (short?)30);
            t.GetField("Address").SetValue(object1, "London");
            t.GetField("Created").SetValue(object1, new DateTime(2019, 4, 12));

            Type t_comparer = sut.CreateEqualityComparerForType(t, new string[] { "Id", "Name", "Age", "Address", "Created" });
            var comparer = t_comparer.GetConstructors()[0].Invoke(new object[0]);
            var getHashCode = t_comparer.GetMethods().Single(x => x.Name == "GetHashCode" && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == t);
            var hashCode = getHashCode.Invoke(comparer, new[] { object1 });

            int expectedhash = 17;
            expectedhash = expectedhash * 23 + ((int?)123).GetHashCode();
            expectedhash = expectedhash * 23 + "daniel".GetHashCode();
            expectedhash = expectedhash * 23 + ((short?)30).GetHashCode();
            expectedhash = expectedhash * 23 + "London".GetHashCode();
            expectedhash = expectedhash * 23 + ((DateTime?)new DateTime(2019, 4, 12)).GetHashCode();

            Assert.AreEqual(expectedhash, hashCode);
        }

        [TestMethod]
        public void EqualityCompare_GetHashCode_SameObjectsReturnsSameHash()
        {
            AnonymousTypeBuilder sut = new AnonymousTypeBuilder();
            Type t = sut.CreateAnonymousType(new[] {
                ("Id", typeof(int)),
                ("Name", typeof(string)),
                ("Age", typeof(int?)),
                ("Hight", typeof(decimal?)),
                ("Weight", typeof(int?))
            });
        
            var obj1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Id").SetValue(obj1, (int)1);
            t.GetField("Name").SetValue(obj1, "daniel");
            t.GetField("Age").SetValue(obj1, 20);

            var obj2 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Id").SetValue(obj2, (int)2);
            t.GetField("Name").SetValue(obj2, "daniel");
            t.GetField("Age").SetValue(obj2, 20);

            Type t_comparer = sut.CreateEqualityComparerForType(t, new string[] { "Name", "Age", "Hight", "Weight" });
            var comparer = t_comparer.GetConstructors()[0].Invoke(new object[0]);
            var getHashCode = t_comparer.GetMethods().Single(x => x.Name == "GetHashCode" && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == t);

            var hashCodeOfObj1 = getHashCode.Invoke(comparer, new[] { obj1 });
            var hashCodeOfObj2 = getHashCode.Invoke(comparer, new[] { obj2 });
            Assert.AreEqual(hashCodeOfObj1, hashCodeOfObj2);
        }

        [TestMethod]
        public void EqualityCompare_GetHashCode_DifferentObjectsReturnsDifferentHash()
        {
            AnonymousTypeBuilder sut = new AnonymousTypeBuilder();
            Type t = sut.CreateAnonymousType(new[] {
                ("Id", typeof(int)),
                ("Name", typeof(string)),
                ("Age", typeof(int?)),
                ("Hight", typeof(decimal?)),
                ("Weight", typeof(int?))
            });

            var obj1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Id").SetValue(obj1, (int)1);
            t.GetField("Name").SetValue(obj1, "daniel");

            var obj2 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Id").SetValue(obj2, (int)2);
            t.GetField("Name").SetValue(obj2, "john");

            Type t_comparer = sut.CreateEqualityComparerForType(t, new string[] { "Name" });
            var comparer = t_comparer.GetConstructors()[0].Invoke(new object[0]);
            var getHashCode = t_comparer.GetMethods().Single(x => x.Name == "GetHashCode" && x.GetParameters().Length == 1 && x.GetParameters()[0].ParameterType == t);

            var hashCodeOfObj1 = getHashCode.Invoke(comparer, new[] { obj1 });
            var hashCodeOfObj2 = getHashCode.Invoke(comparer, new[] { obj2 });
            Assert.AreNotEqual(hashCodeOfObj1, hashCodeOfObj2);
        }

        [TestMethod]
        public void EqualityCompare_Equals_SameObjectsAreEquals()
        {
            AnonymousTypeBuilder sut = new AnonymousTypeBuilder();
            Type t = sut.CreateAnonymousType(new[] {
                ("Id", typeof(int?)),
                ("Name", typeof(string)),
                ("Age", typeof(int?)),
                ("Hight", typeof(decimal?)),
                ("Weight", typeof(int?)),
            });

            var obj1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Id").SetValue(obj1, 1);
            t.GetField("Name").SetValue(obj1, "daniel");
            t.GetField("Age").SetValue(obj1, 20);
            t.GetField("Hight").SetValue(obj1, (decimal)6.1);

            var obj2 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Id").SetValue(obj2, 2);
            t.GetField("Name").SetValue(obj2, "daniel");
            t.GetField("Age").SetValue(obj2, 20);
            t.GetField("Hight").SetValue(obj2, (decimal)6.1);

            Type t_comparer = sut.CreateEqualityComparerForType(t, new string[] { "Name", "Age", "Hight", "Weight" });
            var comparer = t_comparer.GetConstructors()[0].Invoke(new object[0]);
            var equals = t_comparer.GetMethods().Single(x => x.Name == "Equals" && x.GetParameters().Length == 2);

            var areEquals = (bool)equals.Invoke(comparer, new[] { obj1, obj2 });
            Assert.IsTrue(areEquals);
        }

        [TestMethod]
        public void EqualityCompare_Equals_DifferentObjectsAreDifferen()
        {
            AnonymousTypeBuilder sut = new AnonymousTypeBuilder();
            Type t = sut.CreateAnonymousType(new[] {
                ("Id", typeof(int?)),
                ("Name", typeof(string)),
                ("Age", typeof(int?)),
                ("Hight", typeof(decimal?)),
                ("Weight", typeof(int?)),
            });

            var obj1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Id").SetValue(obj1, (int)1);
            t.GetField("Name").SetValue(obj1, "daniel");
            t.GetField("Age").SetValue(obj1, 20);
            t.GetField("Hight").SetValue(obj1, (decimal)6.1);

            var obj2 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Id").SetValue(obj2, (int)2);
            t.GetField("Name").SetValue(obj2, "john");
            t.GetField("Age").SetValue(obj2, null);
            t.GetField("Hight").SetValue(obj2, (decimal)6.1);

            Type t_comparer = sut.CreateEqualityComparerForType(t, new string[] { "Id","Name","Age", "Hight", "Weight" });
            var comparer = t_comparer.GetConstructors()[0].Invoke(new object[0]);
            var equals = t_comparer.GetMethods().Single(x => x.Name == "Equals" && x.GetParameters().Length == 2);

            var areEquals = (bool)equals.Invoke(comparer, new[] { obj1, obj2 });
            Assert.IsFalse(areEquals);
        }

        [TestMethod]
        public void CreateWrapperTypeFor()
        {
            AnonymousTypeBuilder sut = new AnonymousTypeBuilder();
            Type type = sut.CreateAnonymousType(new[] {
                ("Id", typeof(int)),
                ("Name", typeof(string))
            });
            var obj = type.GetConstructors()[0].Invoke(new object[0]);
            type.GetField("Id").SetValue(obj, (int)123);
            type.GetField("Name").SetValue(obj, "daniel");

            Type wrapperType = sut.CreateWrapperTypeFor(type);
            var wrapperObj = wrapperType.GetConstructors()[0].Invoke(new object[0]);
            wrapperType
                .GetFields()
                .FirstOrDefault(x => x.Name == "_inner")
                .SetValue(wrapperObj, obj);

            var id = (int)wrapperType
                .GetProperty("Id")
                .GetValue(wrapperObj);
            var name = (string)wrapperType
                .GetProperty("Name")
                .GetValue(wrapperObj);

            Assert.AreEqual(123, id);
            Assert.AreEqual("daniel", name);

        }
    }
}
