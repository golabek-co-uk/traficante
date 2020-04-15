using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Traficante.TSQL.Parser;
using Traficante.TSQL.Parser.Lexing;

namespace Traficante.TSQL.Tests.Parser
{
    [TestClass]
    public class ParserTests
    {
        //[TestMethod]
        //public void CheckReorderedQueryWithJoin_ShouldConstructQuery()
        //{
        //    var query = "from #some.a() s1 inner join #some.b() s2 on s1.col = s2.col where s1.col2 = '1' group by s2.col3 select s1.col4, s2.col4 skip 1 take 1";

        //    var lexer = new Lexer(query, true);
        //    var parser = new TSQL.Parser.Parser(lexer);

        //    var root = parser.ComposeAll();
        //}
    }
}
