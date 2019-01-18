using System.ComponentModel.DataAnnotations.Schema;
using Toolbelt.ComponentModel.DataAnnotations.Schema;

namespace EntityFrameworkCore.IndexAttributeTest.Models
{
    public class SNSAccount
    {
        public int Id { get; set; }

        public int PersonId { get; set; }

        [ForeignKey(nameof(PersonId))]
        public virtual Person Person { get; set; }

        [Index]
        [Index("Ix_Provider_and_Account", 1, IsUnique = true)]
        public SNSProviders Provider { get; set; }

        [Index("Ix_Provider_and_Account", 2, IsUnique = true)]
        public string AccountName { get; set; }
    }
}
