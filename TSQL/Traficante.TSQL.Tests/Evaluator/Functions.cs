using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.TSQL.Evaluator.Tests.Core.Schema;
using Traficante.TSQL.Tests;

namespace Traficante.TSQL.Evaluator.Tests.Core
{
    [TestClass]
    public class FunctionsTests : TestBase
    {
        [TestMethod]
        public void Select_Function_Cast()
        {
            Engine sut = new Engine();
            sut.AddFunction<string, bool>(null, null, "SERVERPROPERTY", x => true);

            var result = sut.Run("SELECT CAST(SERVERPROPERTY(N'IsHadrEnabled') AS bit) AS [IsHadrEnabled]");
            Assert.IsNotNull(result);
            Assert.AreEqual("IsHadrEnabled", result.Columns.First().ColumnName);
            Assert.AreEqual(true, result[0][0]);
        }

        [TestMethod]
        public void Select_ISNULL()
        {
            Engine sut = new Engine();
            sut.SetVariable("@alwayson", (int?)null);

            var result = sut.Run("SELECT ISNULL(@alwayson, -1) AS [AlwaysOn]");
            Assert.IsNotNull(result);
            Assert.AreEqual("AlwaysOn", result.Columns.First().ColumnName);
            Assert.AreEqual(-1, result[0][0]);
        }

        [TestMethod]
        public void Select_Function_WithDatabaseAndSchema()
        {
            Engine sut = new Engine();
            sut.AddFunction<bool>("msdb", "dbo", "fn_syspolicy_is_automation_enabled", () => true);

            var result = sut.Run("SELECT msdb.dbo.fn_syspolicy_is_automation_enabled()");
            Assert.IsNotNull(result);
            Assert.AreEqual("IsHadrEnabled", result.Columns.First().ColumnName);
            Assert.AreEqual(true, result[0][0]);
        }


        [TestMethod]
        public void Execute()
        {
            Engine sut = new Engine();
            sut.SetVariable("@alwayson", (int?)null);
            sut.SetVariable("@@SERVICENAME", "Traficante");
            sut.AddFunction<string, string, int>("master", "dbo", "xp_qv", (x, y) => 5);

            var result = sut.Run("EXECUTE @alwayson = master.dbo.xp_qv N'3641190370', @@SERVICENAME;");
            var alwayson = sut.GetVariable("@alwayson");
            Assert.IsNotNull(alwayson);
            Assert.AreEqual(5, alwayson.Value);
        }
    }
}