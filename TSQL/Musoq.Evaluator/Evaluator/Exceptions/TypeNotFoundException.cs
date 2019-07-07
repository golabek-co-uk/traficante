using System;
using System.Collections.Generic;
using System.Text;

namespace Traficante.TSQL.Evaluator.Exceptions
{
    public class TypeNotFoundException : Exception
    {
        public TypeNotFoundException(string message)
            : base(message)
        {

        }
    }
}
