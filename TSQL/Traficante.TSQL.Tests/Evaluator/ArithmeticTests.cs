using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.TSQL.Evaluator.Tests.Core.Schema;

namespace Traficante.TSQL.Evaluator.Tests.Core
{
    [TestClass]
    public class ArithmeticTests : TestBase
    {
        [TestMethod]
        public void MultiDividingTest()
        {
            TestMethodTemplate<int?>("256 / 8 / 8", 4);
        }

        [TestMethod]
        public void DivisionWithAdditionTest()
        {
            TestMethodTemplate<int?>("256 + 256 / 2", 384);
        }

        [TestMethod]
        public void DivisionWithAdditionHigherPriorityTest()
        {
            TestMethodTemplate<int?>("(256 + 256) / 2", 256);
        }

        [TestMethod]
        public void DivisionWithMultiplicationTest()
        {
            TestMethodTemplate<int?>("2 * 3 / 2", 3);
        }

        [TestMethod]
        public void DivisionWithSubtraction1Test()
        {
            TestMethodTemplate<int?>("128 / 64 - 2", 0);
        }

        [TestMethod]
        public void DivisionWithSubtraction2Test()
        {
            TestMethodTemplate<int?>("128 - 64 / 2", 96);
        }

        [TestMethod]
        public void MultiSubtractionTest()
        {
            TestMethodTemplate<int?>("256 - 128 - 128", 0);
        }

        [TestMethod]
        public void SubtractionWithAdditionTest()
        {
            TestMethodTemplate<int?>("256 + 128 - 128", 256);
        }

        [TestMethod]
        public void SubtractionWithUnaryMinusTest()
        {
            TestMethodTemplate<int?>("1 - -1", 2);
        }

        [TestMethod]
        public void SubtractionWithUnaryMinusExpressionTest()
        {
            TestMethodTemplate<int?>("1 - -(1 + 2)", 4);
        }

        [TestMethod]
        public void SubtractionWithUnaryMinusAndAdditionTest()
        {
            TestMethodTemplate<int?>("1 - -1 + 2", 4);
        }

        [TestMethod]
        public void SubtractionWithUnaryMinusAndSubtractionTest()
        {
            TestMethodTemplate<int?>("1 - (-1) - 2", 0);
        }

        [TestMethod]
        public void AdditionWithUnaryMinusAndAdditionTest()
        {
            TestMethodTemplate<int?>("1 + -1 + 2", 2);
        }

        [TestMethod]
        public void ModuloExpressionWithMultiplicationTest()
        {
            TestMethodTemplate<int?>("8 % 3 * 2", 4);
        }

        [TestMethod]
        public void ModuloExpressionWithSubtractionTest()
        {
            TestMethodTemplate<int?>("8 % 3 - 2", 0);
        }

        [TestMethod]
        public void ModuloExpressionWithAdditionTest()
        {
            TestMethodTemplate<int?>("8 % 3 + 2", 4);
        }

        [TestMethod]
        public void MultiModuloExpressionTest()
        {
            TestMethodTemplate<int?>("5 % 4 % 6", 1);
        }

        [TestMethod]
        public void ComplexArithmeticExpression1Test()
        {
            TestMethodTemplate<int?>("1 + 2 * 3 * ( 7 * 8 ) - ( 45 - 10 )", 302);
        }

        [TestMethod]
        public void CaseWhenArithmeticExpressionTest()
        {
            TestMethodTemplate<int?>("1 + (case when 2 > 1 then 1 else 0 end) - 1", 1);
            TestMethodTemplate<int?>("1 + (case when 2 < 1 then 0 else 1 end) - 1", 1);
        }
    }
}