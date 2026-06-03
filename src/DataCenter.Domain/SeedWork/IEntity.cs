namespace DataCenter.Domain.SeedWork;

/// <summary>
/// 实体接口
/// </summary>
public interface IEntity
{
}

/// <summary>
/// 实体基类
/// </summary>
/// <typeparam name="TKey"></typeparam>
public abstract class Entity<TKey> : IEntity where TKey : struct
{
    TKey _id;

    /// <summary>
    /// 实体标识
    /// </summary>
    public virtual TKey Id
    {
        get => _id;
        protected set => _id = value;
    }
}
/// <summary>
/// 聚合根接口
/// </summary>
public interface IAggregateRoot : IEntity
{
}

/// <summary>
/// 聚合根基类
/// </summary>
/// <typeparam name="TKey"></typeparam>
public abstract class AggregateRoot<TKey> : Entity<TKey>, IAggregateRoot where TKey : struct
{
}

/// <summary>
/// 软删除接口
/// </summary>

public interface ISoftDelete
{
    /// <summary>
    /// 是否已删除
    /// </summary>
    bool IsDeleted { get; set; }
}