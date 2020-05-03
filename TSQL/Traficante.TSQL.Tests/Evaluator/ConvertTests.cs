using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using CsvHelper;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Traficante.TSQL.Evaluator.Tests.Core.Schema;
using Traficante.TSQL.Tests;

namespace Traficante.TSQL.Evaluator.Tests.Core
{
    [TestClass]
    public class ConvertTests : TestBase
    {
        [TestMethod]
        public void Select_Function_Cast()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddFunction<string, bool>("SERVERPROPERTY", x => true);

            var result = sut.RunAndReturnTable("SELECT CAST(SERVERPROPERTY(N'IsHadrEnabled') AS bit) AS [IsHadrEnabled]");
            Assert.IsNotNull(result);
            Assert.AreEqual("IsHadrEnabled", result.Columns.First().ColumnName);
            Assert.AreEqual(true, result[0][0]);
        }

        [TestMethod]
        public void StringToDate()
        {
            TSQLEngine sut = new TSQLEngine();
            var result = sut.RunAndReturnTable("SELECT CONVERT(datetime, '2009-07-16 08:28:01') AS [date]");
            Assert.IsNotNull(result);
            Assert.AreEqual("date", result.Columns.First().ColumnName);
            Assert.AreEqual(new DateTimeOffset(2009, 07, 16, 08, 28, 01, 0, TimeSpan.Zero), result[0][0]);
        }

        [TestMethod]
        public void ExactStringToDate()
        {
            TSQLEngine sut = new TSQLEngine();
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 15, 31, 00, 00, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, 'May  1 2020  3:31PM', 0) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 09, 12, 00, 00, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, 'May  1 2020  9:12AM',100) AS [date]")[0][0]);
            Assert.AreEqual(
            new DateTimeOffset(2020, 05, 01, 00, 00, 00, 00, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '05/01/20', 1) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 00, 00, 00, 00, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '05/01/2020', 101) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 00, 00, 00, 00, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '20.05.01', 2) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 00, 00, 00, 00, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '2020.05.01', 102) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 00, 00, 00, 0, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '01/05/20', 3) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 00, 00, 00, 0, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '01/05/2020', 103) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 00, 00, 00, 0, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '01.05.20', 4) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 00, 00, 00, 0, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '01.05.2020', 104) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 00, 00, 00, 0, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '01-05-20', 5) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 00, 00, 00, 0, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '01-05-2020', 105) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 00, 00, 00, 0, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '01 May 20', 6) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 00, 00, 00, 0, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '01 May 2020', 106) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 00, 00, 00, 0, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, 'May 01, 20', 7) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 00, 00, 00, 0, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, 'May 01, 2020', 107) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 15, 37, 56, 0, TimeSpan.Zero).TimeOfDay,
                ((DateTimeOffset)sut.RunAndReturnTable("SELECT CONVERT(datetime, '15:37:56', 108) AS [date]")[0][0]).TimeOfDay);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 15, 38, 40, 677, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, 'May  1 2020  3:38:40:677PM', 109) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 00, 00, 00, 0, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '05-01-20', 10) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 00, 00, 00, 0, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '05-01-2020', 110) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 00, 00, 00, 0, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '20/05/01', 11) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 00, 00, 00, 0, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '2020/05/01', 111) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 00, 00, 00, 0, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '200501', 12) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 00, 00, 00, 0, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '20200501', 112) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 16, 04, 04, 673, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '01 May 2020 16:04:04:673', 113) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 16, 04, 41, 200, TimeSpan.Zero).TimeOfDay,
                ((DateTimeOffset)sut.RunAndReturnTable("SELECT CONVERT(datetime, '16:04:41:200', 114) AS [date]")[0][0]).TimeOfDay);
            Assert.AreEqual(
                new DateTimeOffset(2009, 07, 16, 08, 28, 01, 0, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '2009-07-16 08:28:01', 120) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 16, 05, 18, 160, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '2020-05-01 16:05:18.160', 121) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 16, 12, 28, 473, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '2020-05-01T16:12:28.473', 126) AS [date]")[0][0]);
            Assert.AreEqual(
                new DateTimeOffset(2020, 05, 01, 16, 13, 40, 890, TimeSpan.Zero),
                sut.RunAndReturnTable("SELECT CONVERT(datetime, '2020-05-01T16:13:40.890', 127) AS [date]")[0][0]);
        }

        [TestMethod]
        public void NullStringToExactDate()
        {
            TSQLEngine sut = new TSQLEngine();
            var result = sut.RunAndReturnTable("SELECT CONVERT(datetime, NULL, 100) AS [date]");
            Assert.IsNotNull(result);
            Assert.AreEqual("date", result.Columns.First().ColumnName);
            Assert.AreEqual(null, result[0][0]);
        }

        [TestMethod]
        [ExpectedException(typeof(TSQLException), "Cannot convert 'XXX 1 2020 9:12AM' to datetime")]
        public void StringToExactDate_BadString_ThrowError()
        {
            TSQLEngine sut = new TSQLEngine();
            var result = sut.RunAndReturnTable("SELECT CONVERT(datetime, 'XXX  1 2020  9:12AM', 100) AS [date]");
            Assert.IsNotNull(result);
            Assert.AreEqual("date", result.Columns.First().ColumnName);
            Assert.AreEqual(new DateTimeOffset(2020, 05, 01, 09, 12, 00, 00, TimeSpan.Zero), result[0][0]);
        }

        [TestMethod]
        [ExpectedException(typeof(TSQLException), "'9876 is not a valid style when converting datetime to string'")]
        public void StringToExactDate_BadStyle_ThrowError()
        {
            TSQLEngine sut = new TSQLEngine();
            var result = sut.RunAndReturnTable("SELECT CONVERT(datetime, 'May  1 2020  9:12AM', 9876) AS [date]");
            Assert.IsNotNull(result);
            Assert.AreEqual("date", result.Columns.First().ColumnName);
            Assert.AreEqual(new DateTimeOffset(2020, 05, 01, 09, 12, 00, 00, TimeSpan.Zero), result[0][0]);
        }

        [TestMethod]
        public void DateToString()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddFunction<DateTimeOffset?>("GetTestDate",() => new DateTimeOffset(2020, 05, 01, 09, 12, 00, 00, TimeSpan.Zero));
            var result = sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate()) AS [date]");
            Assert.IsNotNull(result);
            Assert.AreEqual("date", result.Columns.First().ColumnName);
            Assert.AreEqual("May  1 2020  9:12AM", result[0][0]);
        }

        [TestMethod]
        public void DateToExactString()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddFunction<DateTimeOffset?>("GetTestDate", () => new DateTimeOffset(2020, 05, 01, 16, 1, 6, 710, TimeSpan.Zero));
            Assert.AreEqual("May  1 2020  4:01PM", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 0) AS [date]")[0][0]);
            Assert.AreEqual("05/01/20", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 1) AS [date]")[0][0]);
            Assert.AreEqual("20.05.01", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 2) AS [date]")[0][0]);
            Assert.AreEqual("01/05/20", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 3) AS [date]")[0][0]);
            Assert.AreEqual("01.05.20", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 4) AS [date]")[0][0]);
            Assert.AreEqual("01-05-20", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 5) AS [date]")[0][0]);
            Assert.AreEqual("01 May 20", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 6) AS [date]")[0][0]);
            Assert.AreEqual("May 01, 20", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 7) AS [date]")[0][0]);
            Assert.AreEqual("16:01:06", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 8) AS [date]")[0][0]);
            Assert.AreEqual("May  1 2020  4:01:06:710PM", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 9) AS [date]")[0][0]);
            Assert.AreEqual("05-01-20", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 10) AS [date]")[0][0]);
            Assert.AreEqual("20/05/01", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 11) AS [date]")[0][0]);
            Assert.AreEqual("200501", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 12) AS [date]")[0][0]);
            Assert.AreEqual("01 May 2020 16:01:06:710", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 13) AS [date]")[0][0]);
            Assert.AreEqual("16:01:06:710", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 14) AS [date]")[0][0]);
            Assert.AreEqual("2020-05-01 16:01:06", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 20) AS [date]")[0][0]);
            Assert.AreEqual("2020-05-01 16:01:06.710", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 21) AS [date]")[0][0]);
            Assert.AreEqual("05/01/20  4:01:06 PM", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 22) AS [date]")[0][0]);
            Assert.AreEqual("2020-05-01", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 23) AS [date]")[0][0]);
            Assert.AreEqual("16:01:06", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 24) AS [date]")[0][0]);
            Assert.AreEqual("2020-05-01 16:01:06.710", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 25) AS [date]")[0][0]);
            Assert.AreEqual("May  1 2020  4:01PM", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 100) AS [date]")[0][0]);
            Assert.AreEqual("05/01/2020", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 101) AS [date]")[0][0]);
            Assert.AreEqual("2020.05.01", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 102) AS [date]")[0][0]);
            Assert.AreEqual("01/05/2020", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 103) AS [date]")[0][0]);
            Assert.AreEqual("01.05.2020", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 104) AS [date]")[0][0]);
            Assert.AreEqual("01-05-2020", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 105) AS [date]")[0][0]);
            Assert.AreEqual("01 May 2020", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 106) AS [date]")[0][0]);
            Assert.AreEqual("May 01, 2020", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 107) AS [date]")[0][0]);
            Assert.AreEqual("16:01:06", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 108) AS [date]")[0][0]);
            Assert.AreEqual("May  1 2020  4:01:06:710PM", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 109) AS [date]")[0][0]);
            Assert.AreEqual("05-01-2020", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 110) AS [date]")[0][0]);
            Assert.AreEqual("2020/05/01", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 111) AS [date]")[0][0]);
            Assert.AreEqual("20200501", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 112) AS [date]")[0][0]);
            Assert.AreEqual("01 May 2020 16:01:06:710", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 113) AS [date]")[0][0]);
            Assert.AreEqual("16:01:06:710", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 114) AS [date]")[0][0]);
            Assert.AreEqual("2020-05-01 16:01:06", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 120) AS [date]")[0][0]);
            Assert.AreEqual("2020-05-01 16:01:06.710", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 121) AS [date]")[0][0]);
            Assert.AreEqual("2020-05-01T16:01:06.710", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 126) AS [date]")[0][0]);
            Assert.AreEqual("2020-05-01T16:01:06.710", sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 127) AS [date]")[0][0]);
        }


        [TestMethod]
        [ExpectedException(typeof(TSQLException), "")]
        public void DateToExactString_BadStyle_ThrowException()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddFunction<DateTimeOffset?>("GetTestDate", () => new DateTimeOffset(2020, 05, 01, 09, 12, 00, 00, TimeSpan.Zero));
            var result = sut.RunAndReturnTable("SELECT CONVERT(varchar, GetTestDate(), 9876) AS [date]");
            Assert.IsNotNull(result);
            Assert.AreEqual("date", result.Columns.First().ColumnName);
            Assert.AreEqual("20.05.01", result[0][0]);
        }

        [TestMethod]
        public void NullDateToExactString()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddFunction<DateTimeOffset?>("GetTestDate", () => null);
            var result = sut.RunAndReturnTable("SELECT CONVERT(varchar, null, 2) AS [date]");
            Assert.IsNotNull(result);
            Assert.AreEqual("date", result.Columns.First().ColumnName);
            Assert.AreEqual(null, result[0][0]);
        }

        [TestMethod]
        public void DateTimeOffsetToDate()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddFunction<DateTimeOffset?>("GetTestDate", () => new DateTimeOffset(2020, 05, 01, 09, 12, 00, 00, TimeSpan.Zero));
            var result = sut.RunAndReturnTable("SELECT CONVERT(datetime, GetTestDate()) AS [date]");
            Assert.IsNotNull(result);
            Assert.AreEqual("date", result.Columns.First().ColumnName);
            Assert.AreEqual(new DateTimeOffset(2020, 05, 01, 09, 12, 00, 00, TimeSpan.Zero), result[0][0]);
        }

        [TestMethod]
        public void DateTimeToDate()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.AddFunction<DateTime?>("GetTestDate", () => new DateTime(2020, 05, 01, 09, 12, 00, 00));
            var result = sut.RunAndReturnTable("SELECT CONVERT(datetime, GetTestDate()) AS [date]");
            Assert.IsNotNull(result);
            Assert.AreEqual("date", result.Columns.First().ColumnName);
            Assert.AreEqual(new DateTimeOffset(new DateTime(2020, 05, 01, 09, 12, 00, 00)), result[0][0]);
        }

        [TestMethod]
        public void NumberToBool()
        {
            TSQLEngine sut = new TSQLEngine();
            Assert.AreEqual(false, sut.RunAndReturnTable("SELECT CONVERT(bit, 0) AS [value]")[0][0]);
            Assert.AreEqual(true, sut.RunAndReturnTable("SELECT CONVERT(bit, 1) AS [value]")[0][0]);
            Assert.AreEqual(true, sut.RunAndReturnTable("SELECT CONVERT(bit, 5) AS [value]")[0][0]);
            Assert.AreEqual(false, sut.RunAndReturnTable("SELECT CONVERT(bit, 0.0) AS [value]")[0][0]);
            Assert.AreEqual(true, sut.RunAndReturnTable("SELECT CONVERT(bit, 0.01) AS [value]")[0][0]);
        }

        [TestMethod]
        public void StringToBool()
        {
            TSQLEngine sut = new TSQLEngine();
            Assert.AreEqual(true, sut.RunAndReturnTable("SELECT CONVERT(bit, 'True') AS [value]")[0][0]);
            Assert.AreEqual(false, sut.RunAndReturnTable("SELECT CONVERT(bit, 'False') AS [value]")[0][0]);
            Assert.AreEqual(false, sut.RunAndReturnTable("SELECT CONVERT(bit, '0') AS [value]")[0][0]);
            Assert.AreEqual(true, sut.RunAndReturnTable("SELECT CONVERT(bit, '1') AS [value]")[0][0]);
            Assert.AreEqual(true, sut.RunAndReturnTable("SELECT CONVERT(bit, '5') AS [value]")[0][0]);
            Assert.AreEqual(false, sut.RunAndReturnTable("SELECT CONVERT(bit, '0.0') AS [value]")[0][0]);
            Assert.AreEqual(true, sut.RunAndReturnTable("SELECT CONVERT(bit, '0.01') AS [value]")[0][0]);
            Assert.AreEqual(false, sut.RunAndReturnTable("SELECT CONVERT(bit, '') AS [value]")[0][0]);
            Assert.AreEqual(false, sut.RunAndReturnTable("SELECT CONVERT(bit, ' ') AS [value]")[0][0]);
            Assert.AreEqual(null, sut.RunAndReturnTable("SELECT CONVERT(bit, NULL) AS [value]")[0][0]);
        }

        [TestMethod]
        public void DateToBool()
        {
            TSQLEngine sut = new TSQLEngine();
            Assert.AreEqual(true, sut.RunAndReturnTable("SELECT CONVERT(bit, GetDate()) AS [value]")[0][0]);
        }

        [TestMethod]
        public void BoolToBool()
        {
            TSQLEngine sut = new TSQLEngine();
            Assert.AreEqual(true, sut.RunAndReturnTable("SELECT CONVERT(bit, CONVERT(bit, 1)) AS [value]")[0][0]);
            Assert.AreEqual(false, sut.RunAndReturnTable("SELECT CONVERT(bit, CONVERT(bit, 0)) AS [value]")[0][0]);
        }

        [TestMethod]
        [ExpectedException(typeof(TSQLException))]
        public void StringToBool_BadString_ThrowException()
        {
            TSQLEngine sut = new TSQLEngine();
            sut.RunAndReturnTable("SELECT CONVERT(bit, 'asdf') AS [value]");
        }
    }
}