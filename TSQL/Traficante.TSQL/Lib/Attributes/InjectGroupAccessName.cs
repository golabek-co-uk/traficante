using System;

namespace Traficante.TSQL.Plugins.Attributes
{
    public class InjectGroupAccessName : InjectTypeAttribute
    {
        public override Type InjectType => typeof(string);
    }
}