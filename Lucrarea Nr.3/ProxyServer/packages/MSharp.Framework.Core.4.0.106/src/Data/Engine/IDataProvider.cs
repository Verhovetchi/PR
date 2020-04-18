namespace MSharp.Framework.Data
{
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    public interface IDataProvider
    {
        IEntity Get(object objectID);
        void Save(IEntity record);
        void Delete(IEntity record);

        IEnumerable<IEntity> GetList(DatabaseQuery query);

        object Aggregate(Type type, Database.AggregateFunction function, string propertyName, IEnumerable<ICriterion> conditions, params QueryOption[] options);


        /// <summary>
        /// Reads the many to many relation and returns the IDs of the associated objects.
        /// </summary>
        IEnumerable<string> ReadManyToManyRelation(IEntity instance, string property);

        IDictionary<string, Tuple<string, string>> GetUpdatedValues(IEntity original, IEntity updated);

        int ExecuteNonQuery(string command);
        object ExecuteScalar(string command);

        bool SupportValidationBypassing();

        void BulkInsert(IEntity[] entities, int batchSize);
        void BulkUpdate(IEntity[] entities, int batchSize);

        string ConnectionString { get; set; }
        string ConnectionStringKey { get; set; }

        DirectDatabaseCriterion GetAssociationInclusionCriteria(DatabaseQuery query, PropertyInfo association);

        Type EntityType { get; }


        string MapColumn(string propertyName);

        int Count(DatabaseQuery query);

        object Aggregate(DatabaseQuery query, QueryAggregateFunction function, string propertyName);

        string MapSubquery(string path, string parent);

        string GenerateSelectCommand(DatabaseQuery iquery, string fields);
    }
}