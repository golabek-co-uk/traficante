using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Traficante.Studio.Models;
using Traficante.Studio.Services;

namespace Traficante.Studio.Tests.Models
{
    [TestClass]
    public class AppDataSerializerTests
    {
        [TestMethod]
        public void SerializeObjects()
        {
            AppDataSerializer sut = new AppDataSerializer();
            AppData appData = new AppData();
            appData.Objects.Add(new SqlServerObjectModel(new SqlServerConnectionModel{ Server = "server1.com" }) 
            {
                Name = "server1"
            });
            appData.Objects.Add(new MySqlObjectModel(new MySqlConnectionModel { Server = "server2.com" } )
            {
                Name = "server2"
            });
            var json =  sut.Serialize(appData);
            var appDataCopy = sut.Derialize(json);
            CollectionAssert.Equals(appData.Objects, appDataCopy.Objects);
        }
    }
}
