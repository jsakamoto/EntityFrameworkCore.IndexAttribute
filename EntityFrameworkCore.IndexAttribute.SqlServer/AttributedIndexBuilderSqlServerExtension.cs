using System;
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
                    builder.ForSqlServerIsClustered(arg.IsClustered);
                },
                postProcessForPrimaryKey: (builder, arg) =>
                {
                    builder.ForSqlServerIsClustered(arg.IsClustered);
                },
                configure: options =>
                {
                    configure?.Invoke(options);
                    options.SuppressNotSupportedException.IsClustered = true;
                });
        }
    }
}
