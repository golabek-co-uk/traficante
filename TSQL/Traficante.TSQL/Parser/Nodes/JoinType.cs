namespace Traficante.TSQL.Parser.Nodes
{
    public enum JoinType
    {
        Inner,
        OuterLeft,
        OuterRight
    }

    public enum JoinOperator
    {
        Loop,
        Hash
    }
}