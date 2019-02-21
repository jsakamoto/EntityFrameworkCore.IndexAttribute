using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Toolbelt.ComponentModel.DataAnnotations.Schema;
using Toolbelt.EntityFrameworkCore.Metadata.Builders;

namespace Toolbelt.ComponentModel.DataAnnotations
{
    public static class AttributedIndexBuilderExtension
    {
        private class IndexBuilderArgument
        {
            public string[] PropertyNames { get; }

            public string IndexName { get; }

            public bool IsUnique { get; }

            public IndexBuilderArgument(IndexAttribute indexAttr, params string[] propertyNames)
            {
                this.PropertyNames = propertyNames;
                this.IndexName = indexAttr.Name;
                this.IsUnique = indexAttr.IsUnique;
            }
        }

        public static void BuildIndexesFromAnnotations(this ModelBuilder modelBuilder)
        {
            AnnotationBasedModelBuilder.Build<IndexAttribute, IndexBuilderArgument>(
                modelBuilder,
                CreateBuilderArguments,
                Build);
        }

        private static IndexBuilderArgument[] CreateBuilderArguments(AnnotatedProperty<IndexAttribute>[] annotatedProperties)
        {
            var unnamedIndexArgs = annotatedProperties
                .Where(prop => prop.Attribute.Name == "")
                .Select(prop => new IndexBuilderArgument(prop.Attribute, prop.Name));

            var namedIndexArgs = annotatedProperties
                .Where(prop => prop.Attribute.Name != "")
                .GroupBy(prop => prop.Attribute.Name)
                .Select(g => new IndexBuilderArgument(
                    g.First().Attribute,
                    g.OrderBy(item => item.Attribute.Order).Select(item => item.Name).ToArray())
                );

            var indexBuilderArgs = unnamedIndexArgs.Concat(namedIndexArgs).ToArray();
            return indexBuilderArgs;
        }

        private static void Build(EntityTypeBuilder builder1, ReferenceOwnershipBuilder builder2, IndexBuilderArgument builderArg)
        {
            var indexBuilder = builder1?.HasIndex(builderArg.PropertyNames) ?? builder2.HasIndex(builderArg.PropertyNames);
            indexBuilder.IsUnique(builderArg.IsUnique);
            if (builderArg.IndexName != "")
            {
                indexBuilder.HasName(builderArg.IndexName);
            }
        }
    }
}