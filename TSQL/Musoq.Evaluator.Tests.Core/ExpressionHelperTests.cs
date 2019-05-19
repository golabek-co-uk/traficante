using Microsoft.VisualStudio.TestTools.UnitTesting;
using Musoq.Evaluator.Visitors;
using System;
using System.Collections.Generic;
using System.Text;

namespace Musoq.Evaluator.Tests.Core
{
    [TestClass]
    public class ExpressionHelperTests
    {
        [TestMethod]
        public void Equals_SameFieldValue_ReturnsTrue()
        {
            ExpressionHelper sut = new ExpressionHelper();
            Type t = sut.CreateAnonymousType(new[] { ("Name", typeof(string)) });

            var object1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Name").SetValue(object1, "Daniel");

            var object2 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Name").SetValue(object2, "Daniel");

            Assert.IsTrue(object1.Equals(object2));
        }

        [TestMethod]
        public void Equals_DifferentFieldValue_ReturnsFalse()
        {
            ExpressionHelper sut = new ExpressionHelper();
            Type t = sut.CreateAnonymousType(new[] { ("Name", typeof(string)) });

            var object1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Name").SetValue(object1, "Daniel");

            var object2 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Name").SetValue(object2, "John");

            Assert.IsFalse(object1.Equals(object2));
        }

        [TestMethod]
        public void Equals_FieldValueIsNull_ReturnsFalse()
        {
            ExpressionHelper sut = new ExpressionHelper();
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
            ExpressionHelper sut = new ExpressionHelper();
            Type t = sut.CreateAnonymousType(new[] { ("Name", typeof(string)) });

            var object1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Name").SetValue(object1, null);
            
            Assert.AreEqual(17, object1.GetHashCode());
        }

        [TestMethod]
        public void GetHashCode_FieldValueIsInt_ReturnsHash()
        {
           ExpressionHelper sut = new ExpressionHelper();
            Type t = sut.CreateAnonymousType(new[] { ("Age", typeof(int)) });

            var object1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Age").SetValue(object1, 20);

            //int hash = 17
            //hash = hash * 23 + Age.GetHashCode();
            
            Assert.AreEqual(17 * 23 + 20.GetHashCode(), object1.GetHashCode());
        }

        [TestMethod]
        public void GetHashCode_FieldValueIsString_ReturnsHash()
        {
            ExpressionHelper sut = new ExpressionHelper();
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
            ExpressionHelper sut = new ExpressionHelper();
            Type t = sut.CreateAnonymousType(new[] {
                ("Id", typeof(int)),
                ("Name", typeof(string)),
                ("Age", typeof(short)),
                ("Address", typeof(string)),
                ("Created", typeof(DateTime))
            });

            var object1 = t.GetConstructors()[0].Invoke(new object[0]);
            t.GetField("Id").SetValue(object1, (int)123);
            t.GetField("Name").SetValue(object1, "daniel");
            t.GetField("Age").SetValue(object1, (short)30);
            t.GetField("Address").SetValue(object1, "London");
            t.GetField("Created").SetValue(object1, new DateTime(2019,4,12));

            int expectedhash = 17;
            expectedhash = expectedhash * 23 + 123.GetHashCode();
            expectedhash = expectedhash * 23 + "daniel".GetHashCode();
            expectedhash = expectedhash * 23 + ((short)30).GetHashCode();
            expectedhash = expectedhash * 23 + "London".GetHashCode();
            expectedhash = expectedhash * 23 + new DateTime(2019, 4, 12).GetHashCode();
            
            Assert.AreEqual(expectedhash, object1.GetHashCode());
        }
    }
}
