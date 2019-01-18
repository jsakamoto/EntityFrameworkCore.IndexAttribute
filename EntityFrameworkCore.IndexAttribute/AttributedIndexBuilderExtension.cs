using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace Toolbelt.ComponentModel.DataAnnotations
{
    public static class AttributedIndexBuilderExtension
    {
        private class IndexParam
        {
            public string IndexName { get; }

            public bool IsUnique { get; }

            public string[] PropertyNames { get; }

            public IndexParam(IndexAttribute indexAttr, params PropertyInfo[] properties)
            {
                this.IndexName = indexAttr.Name;
                this.IsUnique = indexAttr.IsUnique;
                this.PropertyNames = properties.Select(prop => prop.Name).ToArray();
            }
        }

        public static void BuildIndexesFromAnnotations(this ModelBuilder modelBuilder)
        {
            var entityTypes = modelBuilder.Model.GetEntityTypes();

            Parallel.ForEach(entityTypes, entityType =>
            {
                var indexParams = BuildIndexParams(entityType);
                if (indexParams.Length == 0) return;

                Action<IndexParam> createIndex = BuildCreateIndexAction(modelBuilder, entityType);

                lock (modelBuilder)
                {
                    foreach (var indexParam in indexParams)
                    {
                        createIndex(indexParam);
                    }
                }
            });
        }

        private static IndexParam[] BuildIndexParams(Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType)
        {
            var items = entityType.ClrType
                .GetProperties()
                .SelectMany(prop => Attribute.GetCustomAttributes(prop, typeof(IndexAttribute))
                    .Cast<IndexAttribute>()
                    .Select(index => new { prop, index })
                ).ToArray();

            var unnamedIndexParams = items
                .Where(item => item.index.Name == "")
                .Select(item => new IndexParam(item.index, item.prop));

            var namedIndexParams = items
                .Where(item => item.index.Name != "")
                .GroupBy(item => item.index.Name)
                .Select(g => new IndexParam(
                    g.First().index,
                    g.OrderBy(item => item.index.Order).Select(item => item.prop).ToArray())
                );

            var indexParams = unnamedIndexParams.Concat(namedIndexParams).ToArray();
            return indexParams;
        }

        private static Action<IndexParam> BuildCreateIndexAction(ModelBuilder modelBuilder, IMutableEntityType entityType)
        {
            if (!entityType.IsOwned())
            {
                return (IndexParam indexParam) => modelBuilder
                    .Entity(entityType.ClrType)
                    .HasIndex(indexParam.PropertyNames)
                    .Apply(indexParam);
            }
            else
            {
                return (IndexParam indexParam) =>
                {
                    Action<ReferenceOwnershipBuilder> buildAction = builder => builder
                        .HasIndex(indexParam.PropertyNames)
                        .Apply(indexParam);

                    modelBuilder.CreateIndexForOwnedType(entityType, buildAction);
                };
            }
        }

        private static void CreateIndexForOwnedType(this ModelBuilder modelBuilder, IEntityType owned, Action<ReferenceOwnershipBuilder> buildAction)
        {
            var owner = owned.DefiningEntityType;
            if (!owner.IsOwned())
            {
                modelBuilder.Entity(owner.ClrType)
                    .OwnsOne(owned.ClrType, owned.DefiningNavigationName, buildAction);
            }
            else
            {
                modelBuilder.CreateIndexForOwnedType(owner, builder =>
                    builder.OwnsOne(owned.ClrType, owned.DefiningNavigationName, buildAction));
            }
        }

        private static void Apply(this IndexBuilder indexBuilder, IndexParam indexParam)
        {
            indexBuilder.IsUnique(indexParam.IsUnique);
            if (indexParam.IndexName != "")
            {
                indexBuilder.HasName(indexParam.IndexName);
            }
        }
    }
}