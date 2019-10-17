using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;
using Traficante.Studio.Models;
using Traficante.Studio.Services;

namespace Traficante.Studio
{
    public static class Interactions
    {
        public static readonly Interaction<SqlServerConnectionString, SqlServerConnectionString> ConnectToSqlServer = new Interaction<SqlServerConnectionString, SqlServerConnectionString>();
    }
}
