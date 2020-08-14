using System.ComponentModel.DataAnnotations.Schema;
using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;

namespace EntityFrameworkCore.IndexAttributeTest.Models
{
    public class SNSAccount
    {
        [PrimaryKey(IsClustered = false)]
        public int Id { get; set; }

        public int PersonId { get; set; }

        [ForeignKey(nameof(PersonId))]
        public virtual Person Person { get; set; }

        [IndexColumn]
        [IndexColumn("Ix_Provider_and_Account", 1, IsUnique = true, IsClustered = true)]
        public SNSProviders Provider { get; set; }

        [IndexColumn("Ix_Provider_and_Account", 2, IsUnique = true, IsClustered = true)]
        public string AccountName { get; set; }
    }
}
