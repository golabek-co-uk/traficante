using System;

namespace Traficante.TSQL.Tests
{
    public class Person
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTimeOffset HiredDate { get; set; }
    }
}
