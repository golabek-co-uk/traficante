using Musoq.Schema;
using Musoq.Schema.DataSources;

namespace Musoq.Evaluator.Tests.Core.Schema
{
    public class BasicEntityTable : ITable
    {
        public BasicEntityTable()
        {
            Columns = new IColumn[]
            {
                new Column(nameof(BasicEntity.Name), 10,
                    typeof(BasicEntity).GetProperty(nameof(BasicEntity.Name)).PropertyType),
                new Column(nameof(BasicEntity.City), 11,
                    typeof(BasicEntity).GetProperty(nameof(BasicEntity.City)).PropertyType),
                new Column(nameof(BasicEntity.Country), 12,
                    typeof(BasicEntity).GetProperty(nameof(BasicEntity.Country)).PropertyType),
                new Column(nameof(BasicEntity.Population), 13,
                    typeof(BasicEntity).GetProperty(nameof(BasicEntity.Population)).PropertyType),
                new Column(nameof(BasicEntity.Self), 14,
                    typeof(BasicEntity).GetProperty(nameof(BasicEntity.Self)).PropertyType),
                new Column(nameof(BasicEntity.Money), 15,
                    typeof(BasicEntity).GetProperty(nameof(BasicEntity.Money)).PropertyType),
                new Column(nameof(BasicEntity.Month), 16,
                    typeof(BasicEntity).GetProperty(nameof(BasicEntity.Month)).PropertyType),
                new Column(nameof(BasicEntity.Time), 17,
                    typeof(BasicEntity).GetProperty(nameof(BasicEntity.Time)).PropertyType),
                new Column(nameof(BasicEntity.Id), 18,
                    typeof(BasicEntity).GetProperty(nameof(BasicEntity.Id)).PropertyType),
                new Column(nameof(BasicEntity.NullableValue), 19,
                    typeof(BasicEntity).GetProperty(nameof(BasicEntity.NullableValue)).PropertyType)
            };
        }

        public IColumn[] Columns { get; }
    }
}