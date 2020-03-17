﻿using ReactiveUI;
using System;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Traficante.Connect.Connectors;
using Traficante.Studio.Services;

namespace Traficante.Studio.Models
{
    public class SqliteObjectModel : ObjectModel
    {
        [DataMember]
        public SqliteConnectionModel ConnectionInfo { get; set; }
        public override string Name
        {
            get { return this.ConnectionInfo.Alias; }
            set { }
        }

        public SqliteObjectModel()
        {
            ConnectionInfo = new SqliteConnectionModel();
        }

        public SqliteObjectModel(SqliteConnectionModel connectionString)
        {
            ConnectionInfo = connectionString;
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() =>
                {
                    new SqliteConnector(ConnectionInfo.ToConectorConfig()).TryConnect();
                    return new object[] {
                        new SqliteTablesObjectModel(this),
                        new SqliteViewsObjectModel(this)
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

    public class SqliteTablesObjectModel : ObjectModel
    {
        public SqliteObjectModel Database { get; }

        public SqliteTablesObjectModel(SqliteObjectModel database)
        {
            Database = database;
            Name = "Tables";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() => new SqliteConnector(Database.ConnectionInfo.ToConectorConfig()).GetTables()))
                .SelectMany(x => x)
                .Select(x => new SqliteTableObjectModel(Database, x.schema, x.name))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }

    public class SqliteTableObjectModel : ObjectModel, IObjectPath, IObjectFields
    {
        public SqliteObjectModel Databse { get; }
        //public string OnlyName { get; set; }
        //public string OnlySchema { get; set; }

        public SqliteTableObjectModel(SqliteObjectModel databse, string schema, string name)
        {
            Databse = databse;
            //OnlyName = name;
            //OnlySchema = schema;
            Name = name;
        }

        public override void LoadItems()
        {
        }

        public string[] GetObjectPath()
        {
            return new string[] { Databse.Name, Name };
        }

        public string[] GetObjectFields()
        {
            return new string[0];
        }
    }

    public class SqliteViewsObjectModel : ObjectModel
    {
        public SqliteObjectModel Database { get; }

        public SqliteViewsObjectModel(SqliteObjectModel database)
        {
            Database = database;
            Name = "Views";
        }

        public override void LoadItems()
        {
            Observable
                .FromAsync(() => new TaskFactory().StartNew(() => new SqliteConnector(Database.ConnectionInfo.ToConectorConfig()).GetViews()))
                .SelectMany(x => x)
                .Select(x => new SqliteViewObjectModel(Database, x.schema, x.name))
                .ObserveOn(RxApp.MainThreadScheduler)
                .Catch<object, Exception>(ex =>
                {
                    Interactions.Exceptions.Handle(ex).Subscribe();
                    return Observable.Empty<object>();
                })
                .Subscribe(x => Items.Add(x));
        }
    }
    public class SqliteViewObjectModel : ObjectModel, IObjectPath, IObjectFields
    {
        public SqliteObjectModel Databse { get; }
        public string OnlyName { get; set; }
        public string OnlySchema { get; set; }

        public SqliteViewObjectModel(SqliteObjectModel databse, string schema, string name)
        {
            Databse = databse;
            OnlyName = name;
            OnlySchema = schema;
            Name = name;
        }

        public override void LoadItems()
        {
        }

        public string[] GetObjectPath()
        {
            return new string[] { Databse.Name, OnlySchema, OnlyName };
        }

        public string[] GetObjectFields()
        {
            return new string[0];
        }
    }


    [DataContract]
    public class SqliteConnectionModel : ReactiveObject
    {
        [DataMember]
        public string Alias { get; set; }

        [DataMember]
        public string Database { get; set; }
        
        public SqliteConnectorConfig ToConectorConfig()
        {
            return new SqliteConnectorConfig()
            {
                Alias = this.Alias,
                Database = this.Database,
            };
        }
    }

}
