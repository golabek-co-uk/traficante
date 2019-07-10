using System;

namespace Traficante.TSQL.Plugins.Attributes
{
    public class InjectQueryStats : InjectTypeAttribute
    {
        public override Type InjectType => typeof(QueryStats);
    }
}