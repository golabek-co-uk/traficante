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
            TSQLEngine sut = new TSQLEngine();
            sut.SetVariable("@@VERSION", 123);

            var table = sut.RunAndReturnTable("SELECT @@VERSION AS 'SQL Server Version'");

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual(123, table[0][0]);
        }

        [TestMethod]
        public void Set_Variable()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.SetVariable("@MasterPath", "/path/to/resource");
            var result = sut.RunAndReturnTable("SET @MasterPath = '/new/path/to/resource'");
            Assert.AreEqual("/new/path/to/resource", sut.GetVariable("@MasterPath").Value);
        }

        [TestMethod]
        public void Declare_Variable()
        {
            TSQLEngine sut = new TSQLEngine();
            var result = sut.RunAndReturnTable("declare @MasterPath nvarchar(512)");
            Assert.IsNull(result);
            var variable = sut.GetVariable("@MasterPath");
            Assert.AreEqual("@MasterPath", variable.Name);
            Assert.AreEqual(typeof(string), variable.Type);
        }

        [TestMethod]
        public void Declare_SetString()
        {
            TSQLEngine sut = new TSQLEngine();
            var result = sut.RunAndReturnTable("declare @MasterPath nvarchar(512); SET @MasterPath = '/new/path/to/resource'");
            Assert.IsNull(result);
            var variable = sut.GetVariable("@MasterPath");
            Assert.AreEqual("@MasterPath", variable.Name);
            Assert.AreEqual("/new/path/to/resource", variable.Value);
        }

        [TestMethod]
        public void Declare_SetFunction()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddFunction<string,string>("SERVERPROPERTY", x => "Standard Edition");

            var result = sut.RunAndReturnTable("DECLARE @edition sysname; SET @edition = SERVERPROPERTY(N'EDITION'); ");
            Assert.IsNull(result);
            var variable = sut.GetVariable("@edition");
            Assert.AreEqual("@edition", variable.Name);
            Assert.AreEqual("Standard Edition", variable.Value);
        }

        [TestMethod]
        public void Declare_SetCast()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddFunction<string, string>("SERVERPROPERTY", x => "Standard Edition");

            var result = sut.RunAndReturnTable("DECLARE @edition sysname; SET @edition = cast(SERVERPROPERTY(N'EDITION') as sysname); ");
            Assert.IsNull(result);
            var variable = sut.GetVariable("@edition");
            Assert.AreEqual("@edition", variable.Name);
            Assert.AreEqual("Standard Edition", variable.Value);
        }
    }
}