using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading;
using CsvHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.TSQL.Evaluator.Tests.Core.Schema;
using Traficante.TSQL.Tests;

namespace Traficante.TSQL.Evaluator.Tests.Core
{
    [TestClass]
    public class TextFunctionsTests : TestBase
    {

        [TestMethod]
        public void LikeOperatorTest()
        {
            var query = "select Name from #A.Entities() where Name like '%AA%'";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("ABCAACBA"), 
                        new BasicEntity("AAeqwgQEW"), 
                        new BasicEntity("XXX"),
                        new BasicEntity("dadsqqAA")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(3, table.Count);
            Assert.AreEqual("ABCAACBA", table[0].Values[0]);
            Assert.AreEqual("AAeqwgQEW", table[1].Values[0]);
            Assert.AreEqual("dadsqqAA", table[2].Values[0]);
        }

        [TestMethod]
        public void NotLikeOperatorTest()
        {
            var query = "select Name from #A.Entities() where Name not like '%AA%'";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("ABCAACBA"), new BasicEntity("AAeqwgQEW"), new BasicEntity("XXX"),
                        new BasicEntity("dadsqqAA")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("XXX", table[0].Values[0]);
        }

        [TestMethod]
        public void RLikeOperatorTest()
        {
            var query = @"select Name from #A.Entities() where Name rlike '^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$'";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("12@hostname.com"),
                        new BasicEntity("ma@hostname.comcom"),
                        new BasicEntity("david.jones@proseware.com"),
                        new BasicEntity("ma@hostname.com")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(3, table.Count);
            Assert.AreEqual("12@hostname.com", table[0].Values[0]);
            Assert.AreEqual("david.jones@proseware.com", table[1].Values[0]);
            Assert.AreEqual("ma@hostname.com", table[2].Values[0]);
        }

        [TestMethod]
        public void NotRLikeOperatorTest()
        {
            var query = @"select Name from #A.Entities() where Name not rlike '^([\w-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([\w-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$'";
            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A",
                    new[]
                    {
                        new BasicEntity("12@hostname.com"),
                        new BasicEntity("ma@hostname.comcom"),
                        new BasicEntity("david.jones@proseware.com"),
                        new BasicEntity("ma@hostname.com")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(1, table.Columns.Count());
            Assert.AreEqual("Name", table.Columns.ElementAt(0).ColumnName);
            Assert.AreEqual(typeof(string), table.Columns.ElementAt(0).ColumnType);

            Assert.AreEqual(1, table.Count);
            Assert.AreEqual("ma@hostname.comcom", table[0].Values[0]);
        }


        [TestMethod]
        public void ConcateWithPlus()
        {
            var query = @"select 'a' + ' ' + 'b' from #A.entities()";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("A")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("a b", table[0][0]);
        }

        [TestMethod]
        public void MatchWithRegexTest()
        {
            var query = @"select Match('\d{7}', Name) from #A.entities()";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>>
            {
                {
                    "#A", new[]
                    {
                        new BasicEntity("3213213")
                    }
                }
            };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(true, table[0][0]);
        }

        [TestMethod]
        public void ASCII()
        {
            var query = @"select ASCII('Alfreds Futterkiste')";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual((byte)65, table[0][0]);
        }

        [TestMethod]
        public void CHAR()
        {
            var query = @"SELECT CHAR(65) AS CodeToCharacter;";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual('A', table[0][0]);
        }

        [TestMethod]
        public void NCHAR()
        {
            var query = @"SELECT NCHAR(65) AS CodeToCharacter;";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual('A', table[0][0]);
        }

        [TestMethod]
        public void CONCAT_WS()
        {
            var query = @"SELECT CONCAT_WS('.', 'www', 'W3Schools', 'com')";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("www.W3Schools.com", table[0][0]);
        }

        [TestMethod]
        public void DATALENGTH()
        {
            var query = @"SELECT DATALENGTH('W3Schools.com')";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(13, table[0][0]);
        }

        [TestMethod]
        public void FORMAT()
        {
            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");

            var query = @"
               DECLARE @d DATETIME = '12/01/2018';
               SELECT FORMAT(@d, 'd', 'en-US') AS 'US English Result',
               FORMAT(@d, 'd', 'no') AS 'Norwegian Result',
               FORMAT(@d, 'd', 'zu') AS 'Zulu Result';
";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("12/1/2018", table[0][0]);
            Assert.AreEqual("01.12.2018", table[0][1]);
            Assert.AreEqual("12/1/2018", table[0][2]);
        }

        [TestMethod]
        public void LEFT()
        {
            var query = @"SELECT LEFT('SQL Tutorial', 3), LEFT('SQL Tutorial', 11), LEFT('SQL Tutorial', 12), LEFT('SQL Tutorial', 13)";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("SQL", table[0][0]);
            Assert.AreEqual("SQL Tutoria", table[0][1]);
            Assert.AreEqual("SQL Tutorial", table[0][2]);
            Assert.AreEqual("SQL Tutorial", table[0][3]);
        }

        [TestMethod]
        public void LEN()
        {
            var query = @"SELECT LEN('W3Schools.com')";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(13, table[0][0]);
        }

        [TestMethod]
        public void PATINDEX()
        {
            var query = @"SELECT PATINDEX('%schools%', 'W3Schools.com'), PATINDEX('w3sch%', 'W3Schools.com'), PATINDEX('xxx%', 'W3Schools.com');";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual(3, table[0][0]);
            Assert.AreEqual(1, table[0][1]);
            Assert.AreEqual(0, table[0][2]);
        }

        [TestMethod]
        public void QUOTENAME()
        {
            var query = @" SELECT QUOTENAME('abcdef'),QUOTENAME('abcdef','-');";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("[abcdef]", table[0][0]);
            Assert.AreEqual("-abcdef-", table[0][1]);
        }

        [TestMethod]
        public void REPLACE()
        {
            var query = @" SELECT REPLACE('SQL Tutorial', 'T', 'M');";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("SQL MuMorial", table[0][0]);
        }

        [TestMethod]
        public void REPLICATE()
        {
            var query = @"SELECT REPLICATE('SQL Tutorial', 5);";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("SQL TutorialSQL TutorialSQL TutorialSQL TutorialSQL Tutorial", table[0][0]);
        }

        [TestMethod]
        public void REVERSE()
        {
            var query = @"SELECT REVERSE('SQL Tutorial');";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("lairotuT LQS", table[0][0]);
        }

        [TestMethod]
        public void RIGHT()
        {
            var query = @"SELECT RIGHT('SQL Tutorial', 3), RIGHT('SQL Tutorial', 30)";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("ial", table[0][0]);
            Assert.AreEqual("SQL Tutorial", table[0][1]);
        }

        [TestMethod]
        public void RTRIM()
        {
            var query = @"SELECT RTRIM('SQL Tutorial     ') AS RightTrimmedString;";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("SQL Tutorial", table[0][0]);
        }

        [TestMethod]
        public void SPACE()
        {
            var query = @"SELECT SPACE(10);";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("          ", table[0][0]);
        }

        [TestMethod]
        public void STR()
        {
            var query = @"SELECT STR(185);";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("185", table[0][0]);
        }

        [TestMethod]
        public void SUBSTRING()
        {
            var query = @"SELECT SUBSTRING('SQL Tutorial', 1, 3), SUBSTRING('SQL Tutorial', 1, 333), SUBSTRING('SQL Tutorial', 13, 1), SUBSTRING('SQL Tutorial', 12, 0);";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("SQL", table[0][0]);
            Assert.AreEqual("SQL Tutorial", table[0][1]);
            Assert.AreEqual(null, table[0][2]);
            Assert.AreEqual(null, table[0][3]);
        }

        [TestMethod]
        public void UNICODE()
        {
            var query = @"SELECT UNICODE('Atlanta');";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual((byte?)65, table[0][0]);
        }

        [TestMethod]
        public void TRANSLATE()
        {
            var query = @"SELECT TRANSLATE('3*[2+1]/{8-4}', '[]{}', '()()'), TRANSLATE('Monday', 'Monday', 'Sunday')";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("3*(2+1)/(8-4)", table[0][0]);
            Assert.AreEqual("Sunday", table[0][1]);
        }

        [TestMethod]
        public void STUFF()
        {
            var query = @"SELECT STUFF('SQL Tutorial!', 13, 1, ' is fun!'), STUFF('SQL Tutorial!', 14, 1, ' is fun!'), STUFF('SQL Tutorial!', 13, 3, ' is fun!'), STUFF('SQL Tutorial!', 13, 0, ' is fun!'), STUFF('SQL Tutorial!', 13, -1, ' is fun!');";

            var sources = new Dictionary<string, IEnumerable<BasicEntity>> { };

            var vm = CreateAndRunVirtualMachine(query, sources);
            var table = vm.Run();

            Assert.AreEqual("SQL Tutorial is fun!", table[0][0]);
            Assert.AreEqual(null, table[0][1]);
            Assert.AreEqual("SQL Tutorial is fun!", table[0][2]);
            Assert.AreEqual("SQL Tutorial is fun!!", table[0][3]);
        }
    }
}