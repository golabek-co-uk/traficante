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
    public class VariableTests : TestBase
    {
        [TestMethod]
        public void Select_Variable()
        {
            Engine sut = new Engine();
            sut.SetVariable("@@VERSION", 123);

            var table = sut.Run("SELECT @@VERSION AS 'SQL Server Version'");

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual(123, table[0][0]);
        }

        [TestMethod]
        public void Set_Variable()
        {
            Engine sut = new Engine();
            sut.SetVariable("@MasterPath", "/path/to/resource");
            var result = sut.Run("SET @MasterPath = '/new/path/to/resource'");
            Assert.AreEqual("/new/path/to/resource", sut.GetVariable("@MasterPath").Value);
        }

        [TestMethod]
        public void Declare_Variable()
        {
            Engine sut = new Engine();
            var result = sut.Run("declare @MasterPath nvarchar(512)");
            Assert.IsNull(result);
            var variable = sut.GetVariable("@MasterPath");
            Assert.AreEqual("@MasterPath", variable.Name);
            Assert.AreEqual(typeof(string), variable.Type);
        }

        [TestMethod]
        public void Declare_Set_Variable()
        {
            Engine sut = new Engine();
            sut.AddFunction<string,string>(null, null, "SERVERPROPERTY", x => "Standard Edition");

            var result = sut.Run("DECLARE @edition sysname; SET @edition = SERVERPROPERTY(N'EDITION'); ");
            Assert.IsNull(result);
            var variable = sut.GetVariable("@MasterPath");
            Assert.AreEqual("@MasterPath", variable.Name);
            Assert.AreEqual(typeof(string), variable.Type);
        }

        [TestMethod]
        public void Declare_Cast_Set_Variable()
        {
            Engine sut = new Engine();
            sut.AddFunction<string, string>(null, null, "SERVERPROPERTY", x => "Standard Edition");

            var result = sut.Run("DECLARE @edition sysname; SET @edition = cast(SERVERPROPERTY(N'EDITION') as sysname); ");
            Assert.IsNull(result);
            var variable = sut.GetVariable("@MasterPath");
            Assert.AreEqual("@MasterPath", variable.Name);
            Assert.AreEqual(typeof(string), variable.Type);
        }
    }
}