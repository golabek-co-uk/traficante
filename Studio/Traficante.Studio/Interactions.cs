using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Reactive;
using System.Text;
using Traficante.Studio.Models;
using Traficante.Studio.Services;

namespace Traficante.Studio
{
    public static class Interactions
    {
        public static readonly Interaction<SqlServerObjectModel, SqlServerObjectModel> ConnectToSqlServer = new Interaction<SqlServerObjectModel, SqlServerObjectModel>();
        public static readonly Interaction<MySqlObjectModel, MySqlObjectModel> ConnectToMySql = new Interaction<MySqlObjectModel, MySqlObjectModel>();
        public static readonly Interaction<SqliteObjectModel, SqliteObjectModel> ConnectToSqlite = new Interaction<SqliteObjectModel, SqliteObjectModel>();
        public static readonly Interaction<ElasticSearchObjectModel, ElasticSearchObjectModel> ConnectToElasticSearch = new Interaction<ElasticSearchObjectModel, ElasticSearchObjectModel>();
        

        public static readonly Interaction<Unit, Unit> Exit = new Interaction<Unit, Unit>();
        public static readonly Interaction<Unit, Unit> Paste = new Interaction<Unit, Unit>();
        public static readonly Interaction<Unit, Unit> Copy = new Interaction<Unit, Unit>();
        public static readonly Interaction<Unit, Unit> CloseQuery = new Interaction<Unit, Unit>();
        public static readonly Interaction<Unit, Unit> SaveAllQuery = new Interaction<Unit, Unit>();
        public static readonly Interaction<Unit, Unit> SaveAsQuery = new Interaction<Unit, Unit>();
        public static readonly Interaction<Unit, Unit> SaveQuery = new Interaction<Unit, Unit>();
        public static readonly Interaction<Unit, Unit> OpenQuery = new Interaction<Unit, Unit>();
        public static readonly Interaction<Unit, Unit> NewQuery = new Interaction<Unit, Unit>();

        public static readonly Interaction<Exception, Unit> Exceptions = new Interaction<Exception, Unit>();
    }
}
