using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Toolbelt.ComponentModel.DataAnnotations.Schema;
using Toolbelt.ComponentModel.DataAnnotations.Schema.Internals;
using Toolbelt.EntityFrameworkCore.Metadata.Builders;

namespace Toolbelt.ComponentModel.DataAnnotations
{
    public static class AttributedIndexBuilderExtension
    {
        internal class IndexBuilderArgument
        {
            public string[] PropertyNames { get; }

            public string IndexName { get; }

            public bool IsUnique { get; }

            public bool IsClustered { get; }

            public string[] Includes { get; }

            public IndexBuilderArgument(IndexAttribute indexAttr, params string[] propertyNames)
            {
                this.PropertyNames = propertyNames;
                this.IndexName = indexAttr.Name;
                this.IsUnique = indexAttr.IsUnique;
                this.IsClustered = indexAttr.IsClustered;
                this.Includes = indexAttr.Includes ?? new string[0];
            }

            public IndexBuilderArgument(PrimaryKeyAttribute primaryKeyAttr, params string[] propertyNames)
            {
                this.PropertyNames = propertyNames;
                this.IndexName = primaryKeyAttr.Name;
                this.IsClustered = primaryKeyAttr.IsClustered;
            }
        }

        public static void BuildIndexesFromAnnotations(this ModelBuilder modelBuilder)
        {
            modelBuilder.BuildIndexesFromAnnotations(
                postProcessForIndex: null,
                postProcessForPrimaryKey: null,
                configure: null);
        }

        public static void BuildIndexesFromAnnotations(this ModelBuilder modelBuilder, Action<AttributedIndexBuilderOptions> configure)
        {
            modelBuilder.BuildIndexesFromAnnotations(
                postProcessForIndex: null,
                postProcessForPrimaryKey: null,
                configure: configure);
        }

        internal static void BuildIndexesFromAnnotations(
            this ModelBuilder modelBuilder,
            Action<IndexBuilder, IndexBuilderArgument> postProcessForIndex,
            Action<KeyBuilder, IndexBuilderArgument> postProcessForPrimaryKey,
            Action<AttributedIndexBuilderOptions> configure
        )
        {
            var options = new AttributedIndexBuilderOptions();
            configure?.Invoke(options);

            AnnotationBasedModelBuilder.Build<IndexAttribute, IndexBuilderArgument>(
                modelBuilder,
                (props) => CreateBuilderArguments(props, (attr, propNames) => new IndexBuilderArgument(attr, propNames)),
                (b1, b2, arg) => BuildIndex(options, b1, b2, arg, postProcessForIndex));
            AnnotationBasedModelBuilder.Build<PrimaryKeyAttribute, IndexBuilderArgument>(
                modelBuilder,
                (props) => CreateBuilderArguments(props, (attr, propNames) => new IndexBuilderArgument(attr, propNames)),
                (b1, b2, arg) => BuildPrimaryKey(b1, b2, arg, postProcessForPrimaryKey));
        }

        private static IndexBuilderArgument[] CreateBuilderArguments<TAttr>(
            AnnotatedProperty<TAttr>[] annotatedProperties,
            Func<TAttr, string[], IndexBuilderArgument> createBuilderArgInstance
        )
            where TAttr : Attribute, INameAndOrder
        {
            var unnamedIndexArgs = annotatedProperties
                .Where(prop => prop.Attribute.Name == "")
                .Select(prop => createBuilderArgInstance(prop.Attribute, new[] { prop.Name }));

            var namedIndexArgs = annotatedProperties
                .Where(prop => prop.Attribute.Name != "")
                .GroupBy(prop => prop.Attribute.Name)
                .Select(g => createBuilderArgInstance(
                    g.First().Attribute,
                    g.OrderBy(item => item.Attribute.Order).Select(item => item.Name).ToArray())
                );

            var indexBuilderArgs = unnamedIndexArgs.Concat(namedIndexArgs).ToArray();
            return indexBuilderArgs;
        }

        private static void BuildIndex(
            AttributedIndexBuilderOptions options,
            EntityTypeBuilder builder1,
            OwnedNavigationBuilder builder2,
            IndexBuilderArgument builderArg,
            Action<IndexBuilder, IndexBuilderArgument> postProcess)
        {
            if (!options.SuppressNotSupportedException.IsClustered && builderArg.IsClustered)
                throw new NotSupportedException(
                    "\"IsClustered=true\" of [Index] attribute is not supported.\n" +
                    "If you want to use \"IsClustered=true\", you need to call \"BuildIndexesFromAnnotationsForSqlServer()\" (in the Toolbelt.EntityFrameworkCore.IndexAttribute.SqlServer package) instead of \"BuildIndexesFromAnnotations()\", for a SQL Server connection.\n" +
                    "You can also suppress this exception by calling like \"BuildIndexesFromAnnotations(options => options.SupressUnsupportedException.IsClustered = true)\"");

            if (!options.SuppressNotSupportedException.Includes && (builderArg.Includes ?? new string[0]).Any())
                throw new NotSupportedException(
                    "\"Includes\" of [Index] attribute is not supported.\n" +
                    "If you want to use \"Includes\", you need to call \"BuildIndexesFromAnnotationsForSqlServer()\" (in the Toolbelt.EntityFrameworkCore.IndexAttribute.SqlServer package) instead of \"BuildIndexesFromAnnotations()\", for a SQL Server connection.\n" +
                    "You can also suppress this exception by calling like \"BuildIndexesFromAnnotations(options => options.SupressUnsupportedException.Includes = true)\"");

            var indexBuilder = builder1?.HasIndex(builderArg.PropertyNames) ?? builder2.HasIndex(builderArg.PropertyNames);
            indexBuilder.IsUnique(builderArg.IsUnique);
            if (builderArg.IndexName != "")
            {
                indexBuilder.HasName(builderArg.IndexName);
            }
            postProcess?.Invoke(indexBuilder, builderArg);
        }

        private static void BuildPrimaryKey(EntityTypeBuilder builder1, OwnedNavigationBuilder builder2, IndexBuilderArgument builderArg, Action<KeyBuilder, IndexBuilderArgument> postProcess)
        {
            if (builder1 == null) throw new NotSupportedException("Annotate primary key to owned entity isn't supported. If you want to do it, you have to implement it by Fluent API in DbContext.OnModelCreating() with EF Core v.2.2 or after.");

            var keyBuilder = builder1.HasKey(builderArg.PropertyNames);
            if (builderArg.IndexName != "")
            {
                keyBuilder.HasName(builderArg.IndexName);
            }
            postProcess?.Invoke(keyBuilder, builderArg);
        }
    }
}