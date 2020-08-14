namespace Toolbelt.ComponentModel.DataAnnotations.Schema.V5
{
#pragma warning disable CS0618 // Type or member is obsolete
    public class PrimaryKeyAttribute : Toolbelt.ComponentModel.DataAnnotations.Schema.PrimaryKeyAttribute
#pragma warning restore CS0618 // Type or member is obsolete
    {
        /// <summary>
        /// Initializes a new PrimaryKeyAttribute instance for a primary key that will be named by convention and has no column order specified.
        /// </summary>
        public PrimaryKeyAttribute() : base("", -1)
        {
        }

        /// <summary>
        /// Initializes a new PrimaryKeyAttribute instance for a primary key with the given name and has no column order specified.
        /// </summary>
        /// <param name="name">The primary key name.</param>
        public PrimaryKeyAttribute(string name) : base(name, -1)
        {
        }

        /// <summary>
        /// Initializes a new PrimaryKeyAttribute instance for a primary key with the given name and column order.
        /// </summary>
        /// <param name="name">The primary key name.</param>
        /// <param name="order">A number which will be used to determine column ordering for multi-column primary keys.</param>
        public PrimaryKeyAttribute(string name, int order) : base(name, order)
        {
        }
    }
}
