using System;

namespace Toolbelt.ComponentModel.DataAnnotations.Schema.V5
{
    /// <summary>
    /// Represents an attribute that is placed on a property to indicate that the database column to which the property is mapped has an index.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
#pragma warning disable CS0618 // Type or member is obsolete
    public class IndexColumnAttribute : IndexAttribute
#pragma warning restore CS0618 // Type or member is obsolete
    {
        /// <summary>
        /// Initializes a new IndexAttribute instance for an index that will be named by convention and has no column order, uniqueness specified.
        /// </summary>
        public IndexColumnAttribute()
        {
        }

        /// <summary>
        /// Initializes a new IndexAttribute instance for an index with the given name and has no column order, uniqueness specified.
        /// </summary>
        /// <param name="name">The index name.</param>
        public IndexColumnAttribute(string name) : base(name, -1)
        {
        }

        /// <summary>
        /// Initializes a new IndexAttribute instance for an index with the given name and column order, but with no uniqueness specified.
        /// </summary>
        /// <param name="name">The index name.</param>
        /// <param name="order">A number which will be used to determine column ordering for multi-column indexes.</param>
        public IndexColumnAttribute(string name, int order) : base(name, order)
        {
        }
    }
}
