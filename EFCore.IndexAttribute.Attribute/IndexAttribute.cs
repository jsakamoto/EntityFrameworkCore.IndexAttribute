using System;
using System.ComponentModel;
using Toolbelt.ComponentModel.DataAnnotations.Schema.Internals;

namespace Toolbelt.ComponentModel.DataAnnotations.Schema
{
    /// <summary>
    /// [deprecated] Please use [IndexColumn] attribute with "using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;".
    /// </summary>
    [Obsolete("Please use [IndexColumn] attribute with \"using Toolbelt.ComponentModel.DataAnnotations.Schema.V5;\".")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public class IndexAttribute : Attribute, INameAndOrder
    {
        /// <summary>
        /// Gets the index name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a number that determines the column ordering for multi-column indexes. This will be -1 if no column order has been specified.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Gets or sets a value to indicate whether to define a unique index.
        /// </summary>
        public bool IsUnique { get; set; }

        /// <summary>
        /// Gets or sets a value to indicate whether to define a cluster index.
        /// </summary>
        public bool IsClustered { get; set; }

        /// <summary>
        /// Gets or sets an array of name of properties for inclusion into this index.
        /// </summary>
        public string[] Includes { get; set; } = new string[] { };

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
