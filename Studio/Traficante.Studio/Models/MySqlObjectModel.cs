using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Traficante.Studio.Services;

namespace Traficante.Studio.Models
{
    public class MySqlObjectModel : ObjectModel
    {
        public MySqlConnectionInfo ConnectionInfo { get; set; }

        public MySqlObjectModel(MySqlConnectionInfo connectionString)
        {
            ConnectionInfo = connectionString;
            Name = connectionString.Server;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() => new MySqlService().GetDatabases(ConnectionInfo)))
                .SelectMany(x => x)
                .Select(x => new MySqlDatabaseObjectModel(this, x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class MySqlDatabaseObjectModel : ObjectModel
    {
        public MySqlObjectModel Server { get; }

        public MySqlDatabaseObjectModel(MySqlObjectModel server, string name)
        {
            Server = server;
            Name = name;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() =>
                {
                    new MySqlService().TryConnect(Server.ConnectionInfo, Name);
                    return new object[] {
                        new MySqlTablesObjectModel(this),
                        new MySqlViewsObjectModel(this)
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

    public class MySqlTablesObjectModel : ObjectModel
    {
        public MySqlDatabaseObjectModel Database { get; }

        public MySqlTablesObjectModel(MySqlDatabaseObjectModel database)
        {
            Database = database;
            Name = "Tables";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() => new MySqlService().GetTables(Database.Server.ConnectionInfo, Database.Name)))
                .SelectMany(x => x)
                .Select(x => new MySqlTableObjectModel(Database, x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class MySqlTableObjectModel : ObjectModel
    {
        public MySqlDatabaseObjectModel Databse { get; }
        
        public MySqlTableObjectModel(MySqlDatabaseObjectModel databse, string name)
        {
            Databse = databse;
            Name = name;
        }

        public override void LoadItems()
        {
        }
    }

    public class MySqlViewsObjectModel : ObjectModel
    {
        public MySqlDatabaseObjectModel Database { get; }

        public MySqlViewsObjectModel(MySqlDatabaseObjectModel database)
        {
            Database = database;
            Name = "Views";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() => new MySqlService().GetViews(Database.Server.ConnectionInfo, Database.Name)))
                .SelectMany(x => x)
                .Select(x => new MySqlViewObjectModel(Database, x))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class MySqlViewObjectModel : ObjectModel
    {
        public MySqlDatabaseObjectModel Databse { get; }

        public MySqlViewObjectModel(MySqlDatabaseObjectModel databse, string name)
        {
            Databse = databse;
            Name = name;
        }

        public override void LoadItems()
        {
        }
    }

    public class MySqlConnectionInfo : ReactiveObject
    {
        public string Server { get; set; }
        public string UserId { get; set; }
        public string Password { get; set; }

        public string ToConnectionString()
        {
            return $"Server={Server};User Id={UserId};Password={Password};";
        }
    }
}
