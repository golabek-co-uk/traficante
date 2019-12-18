using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Traficante.TSQL.Lib;

namespace Traficante.TSQL.Schema.Managers
{
    public class MethodsAggregator
    {
        private readonly MethodsManager _methsManager;

        public MethodsAggregator(MethodsManager methsManager)//, PropertiesManager propsManager)
        {
            _methsManager = methsManager;
        }

        public MethodInfo ResolveMethod(string name, Type[] args)
        {
            return _methsManager.GetMethod(name, args);
        }
    }
}