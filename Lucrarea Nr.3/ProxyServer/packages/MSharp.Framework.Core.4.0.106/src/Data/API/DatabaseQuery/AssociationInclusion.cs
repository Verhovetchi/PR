namespace MSharp.Framework.Data
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;

    public class AssociationInclusion
    {
        public PropertyInfo Association { get; set; }

        List<string> IncludedNestedAssociations = new List<string>();

        public static AssociationInclusion Create(PropertyInfo association) =>
            new AssociationInclusion { Association = association };

        public void IncludeNestedAssociation(string nestedAssociation)
            => IncludedNestedAssociations.Add(nestedAssociation);

        public void LoadAssociations(DatabaseQuery query, IEnumerable<IEntity> mainObjects)
        {
            if (mainObjects.None()) return;

            var associatedObjects = LoadTheAssociatedObjects(query);

            var groupedObjects = GroupTheMainObjects(mainObjects);

            var cachedField = query.EntityType.GetField("cached" + Association.Name,
                BindingFlags.NonPublic | BindingFlags.Instance);
            if (cachedField == null) return;

            foreach (var associatedObject in associatedObjects)
            {
                var group = groupedObjects.GetOrDefault(associatedObject.GetId());
                if (group == null)
                {
                    if (query.PageSize.HasValue) continue;

                    throw new Exception("Database include binding failed.\n" +
                        "The loaded associated " + associatedObject.GetType().Name + " with id " + associatedObject.GetId() + "\n" + "is not referenced by any " + Association.DeclaringType.Name + "object!\n\n" +
                        "All associated " + Association.Name + " Ids are:\n" +
                        groupedObjects.Select(x => x.Key).ToLinesString());
                }

                foreach (var mainEntity in group)
                    BindToCachedField(cachedField, associatedObject, mainEntity);
            }
        }

        void BindToCachedField(FieldInfo cachedField, IEntity associatedObject, IEntity mainEntity)
        {
            var cachedRef = cachedField.GetValue(mainEntity);

            var bindMethod = cachedRef.GetType().GetMethod("Bind",
                BindingFlags.NonPublic | BindingFlags.Instance);

            bindMethod?.Invoke(cachedRef, new[] { associatedObject });
        }

        Dictionary<object, IEntity[]> GroupTheMainObjects(IEnumerable<IEntity> mainObjects)
        {
            var idProperty = Association.DeclaringType.GetProperty(Association.Name + "Id");

            var groupedResult = mainObjects.GroupBy(item => idProperty.GetValue(item))
                .Except(x => ReferenceEquals(x.Key, null))
           .ToDictionary(i => i.Key, i => i.ToArray());
            return groupedResult;
        }

        IEnumerable<IEntity> LoadTheAssociatedObjects(DatabaseQuery query)
        {
            var nestedQuery = Database.Of(Association.PropertyType);
            var provider = nestedQuery.Provider;

            return nestedQuery
                       .Where(provider.GetAssociationInclusionCriteria(query, Association))
                       .Include(IncludedNestedAssociations)
                       .GetList();
        }
    }
}