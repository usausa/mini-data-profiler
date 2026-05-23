namespace MiniDataProfiler;

using System.Data;
using System.Data.Common;

internal sealed class ProfileDbBatchCommand : DbBatchCommand
{
    private readonly DbBatchCommand cmd;

    #pragma warning disable CA2100
    public override string CommandText
    {
        get => cmd.CommandText;
        set => cmd.CommandText = value;
    }
    #pragma warning restore CA2100

    public override CommandType CommandType
    {
        get => cmd.CommandType;
        set => cmd.CommandType = value;
    }

    public override int RecordsAffected => cmd.RecordsAffected;

    protected override DbParameterCollection DbParameterCollection => cmd.Parameters;

    public ProfileDbBatchCommand(DbBatchCommand cmd)
    {
        this.cmd = cmd;
    }
}
