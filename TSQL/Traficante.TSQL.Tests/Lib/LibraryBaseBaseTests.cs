using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Traficante.TSQL.Plugins.Tests
{
    [TestClass]
    public class LibraryBaseBaseTests
    {
        private class EmptyLibrary : LibraryBase { }

        protected LibraryBase Library;

        [TestInitialize]
        public void Initialize()
        {
            Library = new EmptyLibrary();
        }
    }
}
