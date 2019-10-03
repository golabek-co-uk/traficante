using System;
using System.Collections.Generic;
using System.Text;
using Traficante.TSQL.Plugins.Attributes;

namespace Traficante.TSQL.Plugins
{
    public partial class LibraryBase
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
