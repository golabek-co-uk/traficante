using System.Text;
using Traficante.TSQL.Lib.Attributes;

namespace Traficante.TSQL.Lib
{
    public partial class Library
    {
        [BindableMethod]
        public object IsNull(object obj, object ifNull)
        {
            if (obj == null)
                return ifNull;
            return obj;
        }
    }


}
