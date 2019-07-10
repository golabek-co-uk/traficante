using System;

namespace Traficante.TSQL.Plugins.Attributes
{
    public class InjectGroupAttribute : InjectTypeAttribute
    {
        public override Type InjectType => typeof(Group);
    }
}