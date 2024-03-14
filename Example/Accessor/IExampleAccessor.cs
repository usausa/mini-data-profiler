namespace Example.Accessor;

using System.Data.Common;

using Example.Models;

using Smart.Data.Accessor.Attributes;
using Smart.Data.Accessor.Builders;

[DataAccessor]
public interface IExampleAccessor
{
    [Execute]
    void Create();

    [Insert]
    void Insert(DataEntity entity);

    [Insert]
    void Insert(DbTransaction tx, DataEntity entity);

    [Query]
    List<DataEntity> QueryDataList(string? type = null, string? order = null);
}
