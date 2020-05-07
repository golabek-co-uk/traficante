using System;
using System.ComponentModel;
using Traficante.TSQL.Lib;

namespace Traficante.TSQL.Evaluator.Helpers
{
    static public class ObjectExtensions
    {
        private static Library _library = new Library();

        static public string ToStringOrDefault(this object value)
        {
            return value?.ToString();
        }

        static public T ConvertOrDefault<T>(this object value)
        {
            try
            {
                return (T)_library.Convert(typeof(T), value);
            }
            catch
            {
                return default(T);
            }
        }
        
    }
}