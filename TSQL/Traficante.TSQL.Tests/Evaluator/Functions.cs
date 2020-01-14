using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
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
            sut.AddFunction<string, bool>("SERVERPROPERTY", x => true);

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
            sut.AddFunction<bool>("fn_syspolicy_is_automation_enabled", new string[2] { "msdb", "dbo" }, () => true);

            var result = sut.Run("SELECT msdb.dbo.fn_syspolicy_is_automation_enabled()");
            Assert.IsNotNull(result);
            Assert.AreEqual("msdb.dbo.fn_syspolicy_is_automation_enabled()", result.Columns.First().ColumnName);
            Assert.AreEqual(true, result[0][0]);
        }


        [TestMethod]
        public void Execute_FunctionWithArguments_AssigneResultToVariable()
        {
            Engine sut = new Engine();
            sut.SetVariable("@alwayson", (int?)null);
            sut.SetVariable("@@SERVICENAME", "Traficante");
            sut.AddFunction<string, string, int>("xp_qv", new string[2]{"master", "dbo"}, (x, y) => 5);

            var result = sut.Run("EXECUTE @alwayson = master.dbo.xp_qv N'3641190370', @@SERVICENAME;");
            var alwayson = sut.GetVariable("@alwayson");
            Assert.IsNotNull(alwayson);
            Assert.AreEqual(5, alwayson.Value);
        }

        static bool Execute_FunctionWithArguments_Flag = false;
        [TestMethod]
        public void Execute_FunctionWithArguments()
        {
            Engine sut = new Engine();
            
            sut.SetVariable("@@SERVICENAME", "Traficante");
            sut.AddFunction<string, string, int>("xp_qv", new string[2] { "master", "dbo" }, (x, y) => 
            {
                Execute_FunctionWithArguments_Flag = true;
                return default(int);
            });

            sut.Run("EXECUTE master.dbo.xp_qv N'3641190370', @@SERVICENAME;");
            Assert.IsTrue(Execute_FunctionWithArguments_Flag);
        }

        [TestMethod]
        public void Select_From_FunctionWithoutArguments()
        {
            Engine sut = new Engine();
            sut.AddFunction("get_entities", () =>
            {
                return new[]
                    {
                        new BasicEntity("may"),
                        new BasicEntity("june")
                    }.AsEnumerable();
            });

            var table = sut.Run("select * from get_entities()");
            
            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("may", table[0][0]);
            Assert.AreEqual("june", table[1][0]);
        }

        [TestMethod]
        public void SelectOneColumn_From_FunctionWithoutArguments()
        {
            Engine sut = new Engine();
            sut.AddFunction("get_entities", () =>
            {
                return new[]
                    {
                        new BasicEntity("may"),
                        new BasicEntity("june")
                    }.AsEnumerable();
            });

            var table = sut.Run("select Name from get_entities()");

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("may", table[0][0]);
            Assert.AreEqual("june", table[1][0]);
        }

        [TestMethod]
        public void Select_From_FunctionArguments()
        {
            Engine sut = new Engine();
            sut.AddFunction("get_entities", (int a, string b) =>
            {
                return new[]
                    {
                        new BasicEntity(a.ToString()),
                        new BasicEntity(b)
                    }.AsEnumerable();
            });

            var table = sut.Run("select * from get_entities(3, 'june')");

            Assert.AreEqual(2, table.Count);
            Assert.AreEqual("3", table[0][0]);
            Assert.AreEqual("june", table[1][0]);
        }
    }
}