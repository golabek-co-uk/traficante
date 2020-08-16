using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using Traficante.TSQL.Parser.Lexing;
using Traficante.TSQL.Parser.Tokens;

namespace Traficante.TSQL.Tests.Parser
{
    [TestClass]
    public class LexerTests
    {
        [TestMethod]
        public void CheckEmptyString_ShouldReturnWordToken()
        {
            var lexer = new Lexer("''");

            var token = lexer.Current();

            Assert.AreEqual(TokenType.None, token.TokenType);

            token = lexer.Next();

            Assert.AreEqual(TokenType.Word, token.TokenType);
            Assert.AreEqual(string.Empty, token.Value);
        }

        [TestMethod]
        public void CheckTestString_ShouldReturnWordToken()
        {
            var lexer = new Lexer("'test'");

            var token = lexer.Current();

            Assert.AreEqual(TokenType.None, token.TokenType);

            token = lexer.Next();

            Assert.AreEqual(TokenType.Word, token.TokenType);
            Assert.AreEqual("test", token.Value);
        }

        [TestMethod]
        public void GetLineAndColumn()
        {
            var lexer = new Lexer("SELECT * FROME\r\nXYZ x\r\nWHERE x.Id = 1");

            Assert.AreEqual('S', lexer.Input[0]);
            Assert.AreEqual(1, lexer.GetLocation(0).LineNumber);
            Assert.AreEqual(1, lexer.GetLocation(0).ColumnNumber);

            Assert.AreEqual('*', lexer.Input[7]);
            Assert.AreEqual(1, lexer.GetLocation(7).LineNumber);
            Assert.AreEqual(8, lexer.GetLocation(7).ColumnNumber);

            Assert.AreEqual('X', lexer.Input[16]);
            Assert.AreEqual(2, lexer.GetLocation(16).LineNumber);
            Assert.AreEqual(1, lexer.GetLocation(16).ColumnNumber);

            Assert.AreEqual('x', lexer.Input[20]);
            Assert.AreEqual(2, lexer.GetLocation(20).LineNumber);
            Assert.AreEqual(5, lexer.GetLocation(20).ColumnNumber);

            Assert.AreEqual('W', lexer.Input[23]);
            Assert.AreEqual(3, lexer.GetLocation(23).LineNumber);
            Assert.AreEqual(1, lexer.GetLocation(23).ColumnNumber);

            Assert.AreEqual('1', lexer.Input[36]);
            Assert.AreEqual(3, lexer.GetLocation(36).LineNumber);
            Assert.AreEqual(14, lexer.GetLocation(36).ColumnNumber);

            lexer = new Lexer("SELECT * FROME\nXYZ x\nWHERE x.Id = 1");

            Assert.AreEqual('S', lexer.Input[0]);
            Assert.AreEqual(1, lexer.GetLocation(0).LineNumber);
            Assert.AreEqual(1, lexer.GetLocation(0).ColumnNumber);

            Assert.AreEqual('*', lexer.Input[7]);
            Assert.AreEqual(1, lexer.GetLocation(7).LineNumber);
            Assert.AreEqual(8, lexer.GetLocation(7).ColumnNumber);

            Assert.AreEqual('X', lexer.Input[15]);
            Assert.AreEqual(2, lexer.GetLocation(15).LineNumber);
            Assert.AreEqual(1, lexer.GetLocation(15).ColumnNumber);

            Assert.AreEqual('x', lexer.Input[19]);
            Assert.AreEqual(2, lexer.GetLocation(19).LineNumber);
            Assert.AreEqual(5, lexer.GetLocation(19).ColumnNumber);

            Assert.AreEqual('W', lexer.Input[21]);
            Assert.AreEqual(3, lexer.GetLocation(21).LineNumber);
            Assert.AreEqual(1, lexer.GetLocation(21).ColumnNumber);

            Assert.AreEqual('1', lexer.Input[34]);
            Assert.AreEqual(3, lexer.GetLocation(34).LineNumber);
            Assert.AreEqual(14, lexer.GetLocation(34).ColumnNumber);
        }
    }
}
