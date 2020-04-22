using System;
using System.ComponentModel;

namespace Traficante.TSQL.Evaluator.Helpers
{
    static public class ObjectExtensions
    {
        static public string ToStringOrDefault(this object obj)
        {
            return obj?.ToString();
        }

        static public T ConvertOrDefault<T>(this object obj)
        {
            try
            {
                var converter = TypeDescriptor.GetConverter(typeof(T));
                if (converter != null)
                {
                    // Cast ConvertFromString(string text) : object to (T)
                    return (T)converter.ConvertFromString(obj?.ToString());
                }
                return default(T);
            }
            catch
            {
                return default(T);
            }
        }
        
    }
}