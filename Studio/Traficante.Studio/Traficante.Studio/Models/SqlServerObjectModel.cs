﻿using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Traficante.Studio.Services;

namespace Traficante.Studio.Models
{
    public class SqlServerObjectModel : ObjectModel
    {
        public string Name { get; set; }
        public bool IsConnected { get; set; }

        public SqlServerConnectionInfo ConnectionInfo { get; set; }

        public SqlServerObjectModel(SqlServerConnectionInfo connectionString)
        {
            ConnectionInfo = connectionString;
            Name = connectionString.Server;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() => new SqlServerService().GetDatabases(ConnectionInfo)))
                .SelectMany(x => x)
                .Select(x => new SqlServerDatabaseObjectModel(this, x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class SqlServerDatabaseObjectModel : ObjectModel
    {
        public string Name { get; set; }
        public SqlServerObjectModel SqlServer { get; }

        public SqlServerDatabaseObjectModel(SqlServerObjectModel sqlServer, string name)
        {
            SqlServer = sqlServer;
            Name = name;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() =>
                {
                    new SqlServerService().TryConnect(SqlServer.ConnectionInfo, Name);
                    return new object[] {
                        new SqlServerTablesObjectModel(this),
                        new SqlServerViewsObjectModel(this)
                    };
                }))
                .SelectMany(x => x)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class SqlServerTablesObjectModel : ObjectModel
    {
        public string Name { get; set; }
        public SqlServerDatabaseObjectModel Database { get; }

        public SqlServerTablesObjectModel(SqlServerDatabaseObjectModel database)
        {
            Database = database;
            Name = "Tables";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() => new SqlServerService().GetTables(Database.SqlServer.ConnectionInfo, Database.Name)))
                .SelectMany(x => x)
                .Select(x => new SqlServerTableObjectModel(Database, x.schema, x.name))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class SqlServerTableObjectModel : ObjectModel
    {
        public SqlServerDatabaseObjectModel Databse { get; }
        public string OnlyName { get; set; }
        public string OnlySchema { get; set; }
        public string Name { get; set; }

        public SqlServerTableObjectModel(SqlServerDatabaseObjectModel databse, string schema, string name)
        {
            Databse = databse;
            OnlyName = name;
            OnlySchema = schema;
            Name = schema + "." + name;
        }

        public override void LoadItems()
        {
        }
    }

    public class SqlServerViewsObjectModel : ObjectModel
    {
        public string Name { get; set; }
        public SqlServerDatabaseObjectModel Database { get; }

        public SqlServerViewsObjectModel(SqlServerDatabaseObjectModel database)
        {
            Database = database;
            Name = "Views";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() => new SqlServerService().GetViews(Database.SqlServer.ConnectionInfo, Database.Name)))
                .SelectMany(x => x)
                .Select(x => new SqlServerViewObjectModel(Database, x.schema, x.name))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class SqlServerViewObjectModel : ObjectModel
    {
        public SqlServerDatabaseObjectModel Databse { get; }
        public string OnlyName { get; set; }
        public string OnlySchema { get; set; }
        public string Name { get; set; }

        public SqlServerViewObjectModel(SqlServerDatabaseObjectModel databse, string schema, string name)
        {
            Databse = databse;
            OnlyName = name;
            OnlySchema = schema;
            Name = schema + "." + name;
        }

        public override void LoadItems()
        {
        }
    }

    public class SqlServerConnectionInfo : ReactiveObject
    {
        public string Server { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }

        private SqlServerAuthentication _authentication = SqlServerAuthentication.Windows;
        public SqlServerAuthentication Authentication
        {
            get => _authentication;
            set => this.RaiseAndSetIfChanged(ref _authentication, value);
        }

        public string ToConnectionString()
        {
            if (Authentication == SqlServerAuthentication.Windows)
                return $"Server={Server};Database=master;Trusted_Connection=True;";
            else
                return $"Server={Server};Database=master;User Id={UserId};Password={Password};";
        }
    }

    public enum SqlServerAuthentication : int
    {
        Windows = 0,
        SqlServer = 1
    }
}
