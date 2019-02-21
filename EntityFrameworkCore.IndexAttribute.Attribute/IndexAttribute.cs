using System;

namespace Toolbelt.ComponentModel.DataAnnotations.Schema
{
    /// <summary>
    /// Represents an attribute that is placed on a property to indicate that the database column to which the property is mapped has an index.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class IndexAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the index name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets a number that determines the column ordering for multi-column indexes. This will be -1 if no column order has been specified.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Gets or sets a value to indicate whether to define a unique index.
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        /// Initializes a new IndexAttribute instance for an index that will be named by convention and has no column order, uniqueness specified.
        /// </summary>
        public IndexAttribute() : this("", -1)
        {
        }

        /// <summary>
        /// Initializes a new IndexAttribute instance for an index with the given name and has no column order, uniqueness specified.
        /// </summary>
        /// <param name="name">The index name.</param>
        public IndexAttribute(string name) : this(name, -1)
        {
        }

        /// <summary>
        /// Initializes a new IndexAttribute instance for an index with the given name and column order, but with no uniqueness specified.
        /// </summary>
        /// <param name="name">The index name.</param>
        /// <param name="order">A number which will be used to determine column ordering for multi-column indexes.</param>
        public IndexAttribute(string name, int order)
        {
            this.Name = name;
            this.Order = order;
        }
    }
}
