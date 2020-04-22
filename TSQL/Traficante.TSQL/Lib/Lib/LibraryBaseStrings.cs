using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using Traficante.TSQL.Evaluator;
using Traficante.TSQL.Lib.Attributes;

namespace Traficante.TSQL.Lib
{
    public partial class Library
    {
        private readonly Soundex _soundex = new Soundex();

        [BindableMethod]
        public string Substr(string value, int? index, int? length)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            if (length < 1)
                return string.Empty;

            var valueLastIndex = value.Length - 1;
            var computedLastIndex = index + (length - 1);

            if (valueLastIndex < computedLastIndex)
                length = ((value.Length - 1) - index) + 1;

            return value.Substring(index.Value, length.Value);
        }

        [BindableMethod]
        public string Substr(string value, int? length)
        {
            return Substr(value, 0, length);
        }

        [BindableMethod]
        public string Concat(params string[] strings)
        {
            var concatedStrings = new StringBuilder();

            foreach (var value in strings)
                concatedStrings.Append(value);

            return concatedStrings.ToString();
        }

        [BindableMethod]
        public string Concat(params object[] objects)
        {
            var concatedStrings = new StringBuilder();

            foreach (var value in objects)
                concatedStrings.Append(value);

            return concatedStrings.ToString();
        }

        [BindableMethod]
        public string CONCAT_WS(params string[] strings)
        {
            if (strings.Length > 1)
                return string.Join(strings[0], strings.Skip(1));
            else
                return null;
        }

        [BindableMethod]
        public string CONCAT_WS(params object[] objects)
        {
            if (objects.Length > 1)
                return string.Join(objects[0].ToString(), objects.Skip(1).Select(x => x.ToString()));
            else
                return null;
        }

        [BindableMethod]
        public bool? Contains(string content, string what)
        {
            if (string.IsNullOrEmpty(content) || string.IsNullOrEmpty(what))
                return false;

            return CultureInfo.CurrentCulture.CompareInfo.IndexOf(content, what, CompareOptions.IgnoreCase) >= 0;
        }

        [BindableMethod]
        public int? IndexOf(string value, string text)
        {
            return value.IndexOf(text, StringComparison.OrdinalIgnoreCase);
        }

        [BindableMethod]
        public string Soundex(string value)
        {
            return _soundex.For(value);
        }

        [BindableMethod]
        public string ToUpperInvariant(string value)
        {
            return value.ToUpperInvariant();
        }

        [BindableMethod]
        public string ToLowerInvariant(string value)
        {
            return value.ToLowerInvariant();
        }

        [BindableMethod]
        public byte? ASCII(char? value)
        {
            if (!value.HasValue) return null;
            return Encoding.ASCII.GetBytes(new char[1] { value.Value })[0];
        }

        [BindableMethod]
        public byte? ASCII(string value)
        {
            if (value == null || value.Length == 0) return null;
            return Encoding.ASCII.GetBytes(new char[1] { value[0] })[0];
        }

        [BindableMethod]
        public char? CHAR(byte? value)
        {
            if (!value.HasValue) return null;
            return Encoding.ASCII.GetChars(new byte[1] { value.Value })[0];
        }

        [BindableMethod]
        public char? NCHAR(byte? value)
        {
            if (!value.HasValue) return null;
            return Encoding.Unicode.GetChars(new byte[1] { value.Value })[0];
        }

        [BindableMethod]
        public byte? UNICODE(string value)
        {
            if (value == null || value.Length == 0) return null;
            return Encoding.Unicode.GetBytes(new char[1] { value[0] })[0];
        }


        [BindableMethod]
        public int? CHARINDEX(string substring, string s)
        {
            if (s == null || substring == null) return 0;
            return s.IndexOf(substring) + 1;
        }

        [BindableMethod]
        public int? CHARINDEX(string substring, string s, int? start)
        {
            if (s == null || substring == null) return 0;
            if (start.HasValue)
                return s.IndexOf(substring, start.Value) + 1;
            else
                return s.IndexOf(substring) + 1;
        }

        [BindableMethod]
        public int? DATALENGTH(string value)
        {
            if (value == null) return null;
            return value.Length;
        }

        [BindableMethod]
        public string Format(DateTime? value, string format)
        {
            if (!value.HasValue) return null;
            return value.Value.ToString(format);
        }

        [BindableMethod]
        public string Format(DateTime? value, string format, string culture)
        {
            if (!value.HasValue) return null;
            return value.Value.ToString(format, CultureInfo.CreateSpecificCulture(culture));
        }

        [BindableMethod]
        public string Format(DateTimeOffset? value, string format)
        {
            if (!value.HasValue) return null;
            return value.Value.ToString(format);
        }

        [BindableMethod]
        public string Format(DateTimeOffset? value, string format, string culture)
        {
            if (!value.HasValue) return null;
            return value.Value.ToString(format, CultureInfo.CreateSpecificCulture(culture));
        }

        [BindableMethod]
        public string Left(string value, int? numberOfChars)
        {
            if (value == null || !numberOfChars.HasValue) return null;
            if (value.Length <= numberOfChars)
                return value;
            return value.Remove(numberOfChars.Value);
        }

        [BindableMethod]
        public string Right(string value, int? numberOfChars)
        {
            if (value == null || !numberOfChars.HasValue) return null;
            if (value.Length < numberOfChars)
                return value;
            return value.Substring(value.Length - numberOfChars.Value);
        }

        [BindableMethod]
        public int? Len(string value)
        {
            if (value == null) return null;
            return value.Length;
        }

        [BindableMethod]
        public string LOWER(string value)
        {
            if (value == null) return null;
            return value.ToLower();
        }

        [BindableMethod]
        public string UPPER(string value)
        {
            if (value == null) return null;
            return value.ToUpper();
        }

        [BindableMethod]
        public string LTRIM(string value)
        {
            if (value == null) return null;
            return value.TrimStart();
        }

        [BindableMethod]
        public string RTRIM(string value)
        {
            if (value == null) return null;
            return value.TrimEnd();
        }

        [BindableMethod]
        public string TRIM(string value)
        {
            if (value == null) return null;
            return value.Trim();
        }

        [BindableMethod]
        public int? PATINDEX(string pattern, string value)
        {
            if (value == null || pattern == null) return null;
            return new Operators().LikeIndex(value.ToLower(), pattern.ToLower()).Value + 1;
        }

        [BindableMethod]
        public string QUOTENAME(string value)
        {
            if (value == null) return null;
            return $"[{value}]";
        }

        [BindableMethod]
        public string QUOTENAME(string value, char? quoteChar)
        {
            if (value == null || !quoteChar.HasValue) return null;
            return $"{quoteChar}{value}{quoteChar}";
        }

        [BindableMethod]
        public string QUOTENAME(string value, string quoteChar)
        {
            if (value == null || quoteChar == null) return null;
            return $"{quoteChar}{value}{quoteChar}";
        }

        [BindableMethod]
        public string REPLACE(string value, string oldValue, string newValue)
        {
            if (value == null || oldValue == null || newValue == null) return null;
            return Regex.Replace(value, oldValue, newValue, RegexOptions.IgnoreCase);
        }

        [BindableMethod]
        public string REPLICATE(string value, int? times)
        {
            if (value == null || !times.HasValue) return null;
            return string.Concat(Enumerable.Repeat(value, times.Value));
        }

        [BindableMethod]
        public string REVERSE(string value)
        {
            if (value == null) return null;
            char[] charArray = value.ToCharArray();
            Array.Reverse(charArray);
            return new string(charArray);
        }

        [BindableMethod]
        public string SPACE(int? times)
        {
            if (!times.HasValue) return null;
            return string.Concat(Enumerable.Repeat(" ", times.Value));
        }

        [BindableMethod]
        public string STR(int? value)
        {
            if (!value.HasValue) return null;
            return value.ToString();
        }

        [BindableMethod]
        public string STR(long? value)
        {
            if (!value.HasValue) return null;
            return value.ToString();
        }

        [BindableMethod]
        public string STR(short? value)
        {
            if (!value.HasValue) return null;
            return value.ToString();
        }

        [BindableMethod]
        public string SUBSTRING(string value, int? start, int? lenght)
        {
            if (value == null || !start.HasValue || !lenght.HasValue) return null;
            start--;
            if (value.Length <= start) return null;
            if (lenght == 0) return null;
            if (value.Length >= start + lenght)
                return value.Substring(start.Value, lenght.Value);
            else
                return value.Substring(start.Value);
        }

        [BindableMethod]
        public string STUFF(string value, int? start, int? lenght, string newValue)
        {
            if (value == null || newValue == null || !start.HasValue || !lenght.HasValue) return null;
            start--;
            if (value.Length <= start) return null;
            if (lenght < 0) return null;
            if (value.Length >= start + lenght)
                value = value.Remove(start.Value, lenght.Value);
            else
                value = value.Remove(start.Value);
            return value.Insert(start.Value, newValue);
        }
        

        [BindableMethod]
        public string TRANSLATE(string value, string characters, string translations)
        {
            if (value == null || characters == null || translations == null) return null;

            var sb = new StringBuilder(value);
            for(int i = 0; i < characters.Length && i < translations.Length; i++)
                sb.Replace(characters[i], translations[i]);
            return sb.ToString(); ;
        }
        
    }
}
