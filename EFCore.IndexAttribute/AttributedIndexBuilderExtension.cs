using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Toolbelt.ComponentModel.DataAnnotations.Schema;
using Toolbelt.ComponentModel.DataAnnotations.Schema.Internals;
using Toolbelt.EntityFrameworkCore.Metadata.Builders;
using IndexAttribute = Toolbelt.ComponentModel.DataAnnotations.Schema.IndexAttribute;
using PrimaryKeyAttribute = Toolbelt.ComponentModel.DataAnnotations.Schema.PrimaryKeyAttribute;

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

            public bool[] IsDescending { get; }

#pragma warning disable CS0618 // Type or member is obsolete
            public IndexBuilderArgument(IndexAttribute indexAttr, AnnotatedProperty<IndexAttribute>[] columns)
#pragma warning restore CS0618 // Type or member is obsolete
            {
                this.PropertyNames = columns.Select(column => column.Name).ToArray();
                this.IndexName = indexAttr.Name;
                this.IsUnique = indexAttr.IsUnique;
                this.IsClustered = indexAttr.IsClustered;
                this.Includes = indexAttr.Includes ?? new string[0];
                this.IsDescending = columns.Select(column => column.Attribute is Schema.V5.IndexColumnAttribute indexColumnAttr && indexColumnAttr.IsDescending).ToArray();
            }

#pragma warning disable CS0618 // Type or member is obsolete
            public IndexBuilderArgument(PrimaryKeyAttribute primaryKeyAttr, AnnotatedProperty<PrimaryKeyAttribute>[] columns)
#pragma warning restore CS0618 // Type or member is obsolete
            {
                this.PropertyNames = columns.Select(column => column.Name).ToArray();
                this.IndexName = primaryKeyAttr.Name;
                this.IsClustered = primaryKeyAttr.IsClustered;
                this.IsDescending = new bool[columns.Length];
            }
        }

        /// <summary>
        /// IndexBuilder.IsDescending(bool[]), which exists only in EF Core 7.0 or later. This library is compiled against EF Core 3.x, so it is called through reflection.
        /// </summary>
        private static readonly MethodInfo IsDescendingMethod = typeof(IndexBuilder).GetMethod("IsDescending", new[] { typeof(bool[]) });

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

#pragma warning disable CS0618 // Type or member is obsolete
            AnnotationBasedModelBuilder.Build<IndexAttribute, IndexBuilderArgument>(
                modelBuilder,
                (props) => CreateBuilderArguments(props, (attr, propNames) => new IndexBuilderArgument(attr, propNames)),
                (b1, b2, arg) => BuildIndex(options, b1, b2, arg, postProcessForIndex));
            AnnotationBasedModelBuilder.Build<PrimaryKeyAttribute, IndexBuilderArgument>(
                modelBuilder,
                (props) => CreateBuilderArguments(props, (attr, propNames) => new IndexBuilderArgument(attr, propNames)),
                (b1, b2, arg) => BuildPrimaryKey(b1, b2, arg, postProcessForPrimaryKey));
#pragma warning restore CS0618 // Type or member is obsolete
        }

        private static IndexBuilderArgument[] CreateBuilderArguments<TAttr>(
            AnnotatedProperty<TAttr>[] annotatedProperties,
            Func<TAttr, AnnotatedProperty<TAttr>[], IndexBuilderArgument> createBuilderArgInstance
        )
            where TAttr : Attribute, INameAndOrder
        {
            var unnamedIndexArgs = annotatedProperties
                .Where(prop => prop.Attribute.Name == "")
                .Select(prop => createBuilderArgInstance(prop.Attribute, new[] { prop }));

            var namedIndexArgs = annotatedProperties
                .Where(prop => prop.Attribute.Name != "")
                .GroupBy(prop => prop.Attribute.Name)
                .Select(g => createBuilderArgInstance(
                    g.First().Attribute,
                    g.OrderBy(item => item.Attribute.Order).ToArray())
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

            var hasDescending = builderArg.IsDescending.Any(isDescending => isDescending);
            if (!options.SuppressNotSupportedException.IsDescending && hasDescending && IsDescendingMethod == null)
                throw new NotSupportedException(
                    "\"IsDescending=true\" of [IndexColumn] attribute requires EF Core 7.0 or later.\n" +
                    "You can also suppress this exception by calling like \"BuildIndexesFromAnnotations(options => options.SuppressNotSupportedException.IsDescending = true)\". Then the index is created in ascending order.");

            var indexBuilder = builder1?.HasIndex(builderArg.PropertyNames) ?? builder2.HasIndex(builderArg.PropertyNames);
            indexBuilder.IsUnique(builderArg.IsUnique);
            if (builderArg.IndexName != "")
            {
                indexBuilder.HasName(builderArg.IndexName);
            }
            if (hasDescending && IsDescendingMethod != null)
            {
                IsDescendingMethod.Invoke(indexBuilder, new object[] { builderArg.IsDescending });
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