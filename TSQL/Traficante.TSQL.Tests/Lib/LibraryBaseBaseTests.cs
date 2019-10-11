using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Traficante.TSQL.Lib.Tests
{
    [TestClass]
    public class LibraryBaseBaseTests
    {
        
        protected Library Library;

        [TestInitialize]
        public void Initialize()
        {
            Library = new Traficante.TSQL.Lib.Library();
        }
    }
}
