using System;
using Traficante.TSQL.Evaluator.Visitors;

namespace Traficante.TSQL.Parser.Nodes
{
    public class TypeNode : Node
    {

        public TypeNode(string name, long? size = null)
        {
            Name = name;
            Size = size;
            Id = $"{nameof(TypeNode)}{Name}{Size}";
        }

        public string Name { get; }
        public long? Size { get; }

        public override Type ReturnType
        {
            get
            {
                switch (Name.ToLower())
                {
                    case "tinyint": return typeof(byte?);
                    case "smallint": return typeof(short?);
                    case "int": return typeof(int?);
                    case "bigint": return typeof(long?);
                    case "real": return typeof(float?);
                    case "float": return typeof(double?);
                    case "decimal": return typeof(decimal?);
                    case "bit": return typeof(bool?);
                    case "datetime": return typeof(DateTimeOffset?);
                    case "varchar": return typeof(string);
                    case "nvarchar": return typeof(string);
                    case "sysname": return typeof(string);
                    case "binary": return typeof(byte[]);
                    case "varbinary": return typeof(byte[]);
                    case "char": return typeof(string);
                    case "nchar": return typeof(string);
                    case "smalldatetime": return typeof(DateTimeOffset?);
                    case "date": return typeof(DateTimeOffset?);
                    case "time": return typeof(DateTimeOffset?);
                    case "datetimeoffset": return typeof(DateTimeOffset?);
                    case "datetime2": return typeof(DateTimeOffset?);
                    case "numeric": return typeof(decimal?);
                    case "money": return typeof(decimal?);
                    case "smallmoney": return typeof(decimal?);
                    case "timestamp": return typeof(DateTimeOffset?);
                    case "uniqueidentifier": return typeof(Guid?);
                    case "ntext": return typeof(string);
                    case "text": return typeof(string);
                    default: return typeof(void);
                }
            }
        }
      
        public override string Id { get; }

        public override void Accept(IExpressionVisitor visitor)
        {
            visitor.Visit(this);
        }

        public override string ToString()
        {
            return Size.HasValue ? $"{Name}({Size})" : $"{Name}";
        }
    }
}
