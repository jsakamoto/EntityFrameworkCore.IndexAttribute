using Microsoft.EntityFrameworkCore;

namespace Toolbelt.ComponentModel.DataAnnotations
{
    public static class AttributedIndexBuilderSqlServerExtension
    {
        public static void BuildIndexesFromAnnotationsForSqlServer(this ModelBuilder modelBuilder)
        {
            modelBuilder.BuildIndexesFromAnnotations(
                postProcessForIndex: (builder, arg) =>
                {
                    builder.ForSqlServerIsClustered(arg.IsClustered);
                },
                postProcessForPrimaryKey: (builder, arg) =>
                {
                    builder.ForSqlServerIsClustered(arg.IsClustered);
                });
        }
    }
}
