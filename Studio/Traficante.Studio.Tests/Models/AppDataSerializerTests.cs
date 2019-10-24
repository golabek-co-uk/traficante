using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Traficante.Studio.Models;

namespace Traficante.Studio.Tests.Models
{
    [TestClass]
    public class AppDataSerializerTests
    {
        [TestMethod]
        public void SerializeObjects()
        {
            AppDataSerializer sut = new AppDataSerializer();
            var items = new ObservableCollection<ObjectModel>();
            items.Add(new SqlServerObjectModel(new SqlServerConnectionInfo{ Server = "server1.com" }) 
            {
                Name = "server1"
            });
            items.Add(new MySqlObjectModel(new MySqlConnectionInfo { Server = "server2.com" } )
            {
                Name = "server2"
            });
            var json =  sut.SerializeObjects(items);
            var itemsCopy = sut.DerializeObjects(json);
            CollectionAssert.Equals(items, itemsCopy);
        }
    }
}
