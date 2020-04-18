namespace MSharp.Framework
{
    using System;
    using MSharp.Framework.Data;

    public static partial class Database
    {
        public enum AggregateFunction { Max, Min, Sum, Average }

        // TODO: Consider defining IDatabase<TEntity> and use here for full DI support.
        //  public static Database<TEntity> Of<TEntity>() where TEntity : IEntity => new Database<TEntity>();

        public static DatabaseQuery<TEntity> Of<TEntity>() where TEntity : IEntity => new DatabaseQuery<TEntity>();

        public static DatabaseQuery Of(Type type) => new DatabaseQuery(type);
    }

    //public partial class Database<TEntity> where TEntity : IEntity
    //{
    //    // TODO: Move all other database methods here. 
    //    // Then make the static Database class just delegate the calls to these ones for backwards compatibility.
    //}
}