using System.Collections.Generic;
using System.Reflection;
using Traficante.TSQL.Schema;
using Traficante.TSQL.Schema.DataSources;
using Traficante.TSQL.Schema.Helpers;
using Traficante.TSQL.Schema.Managers;
using Traficante.TSQL.Schema.Reflection;

namespace Traficante.TSQL.Evaluator.Tables
{
    internal class TransitionSchema : BaseDatabase
    {
        private readonly ITable _table;

        public TransitionSchema(string name, ITable table)
            : base(name)
        {
            _table = table;
        }

        public override ITable GetTableByName(string schema, string name)
        {
            return _table;
        }
    }
}