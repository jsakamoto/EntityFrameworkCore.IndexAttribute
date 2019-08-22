using System;
using Toolbelt.ComponentModel.DataAnnotations.Schema.Internals;

namespace Toolbelt.ComponentModel.DataAnnotations.Schema
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PrimaryKeyAttribute : Attribute, INameAndOrder
    {
        /// <summary>
        /// Gets the name of this primary key.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a number that determines the column ordering for multi-column indexes. This will be -1 if no column order has been specified.
        /// </summary>
        public int Order { get; }

        /// <summary>
        /// Gets or sets a value to indicate whether to define a cluster index.
        /// </summary>
        public bool IsClustered { get; set; }

        /// <summary>
        /// Initializes a new PrimaryKeyAttribute instance for a primary key that will be named by convention and has no column order specified.
        /// </summary>
        public PrimaryKeyAttribute() : this("", -1)
        {
        }

        /// <summary>
        /// Initializes a new PrimaryKeyAttribute instance for a primary key with the given name and has no column order specified.
        /// </summary>
        /// <param name="name">The primary key name.</param>
        public PrimaryKeyAttribute(string name) : this(name, -1)
        {
        }

        /// <summary>
        /// Initializes a new PrimaryKeyAttribute instance for a primary key with the given name and column order.
        /// </summary>
        /// <param name="name">The primary key name.</param>
        /// <param name="order">A number which will be used to determine column ordering for multi-column primary keys.</param>
        public PrimaryKeyAttribute(string name, int order)
        {
            this.Name = name;
            this.Order = order;
        }
    }
}
