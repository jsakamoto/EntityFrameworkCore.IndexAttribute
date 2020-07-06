using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace Toolbelt.ComponentModel.DataAnnotations
{
    public static class AttributedIndexBuilderSqlServerExtension
    {
        public static void BuildIndexesFromAnnotationsForSqlServer(this ModelBuilder modelBuilder)
        {
            modelBuilder.BuildIndexesFromAnnotationsForSqlServer(configure: null);
        }

        public static void BuildIndexesFromAnnotationsForSqlServer(this ModelBuilder modelBuilder, Action<AttributedIndexBuilderOptions> configure)
        {
            modelBuilder.BuildIndexesFromAnnotations(
                postProcessForIndex: (builder, arg) =>
                {
                    builder.IsClustered(arg.IsClustered);
                    if (arg.Includes != null && arg.Includes.Any())
                        builder.IncludeProperties(arg.Includes);
                },
                postProcessForPrimaryKey: (builder, arg) =>
                {
                    builder.IsClustered(arg.IsClustered);
                },
                configure: options =>
                {
                    configure?.Invoke(options);
                    options.SuppressNotSupportedException.IsClustered = true;
                    options.SuppressNotSupportedException.Includes = true;
                });
        }
    }
}
