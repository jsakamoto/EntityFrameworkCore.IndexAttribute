namespace Toolbelt.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Options for building Indexes from annotations.
    /// </summary>
    public class AttributedIndexBuilderOptions
    {
        /// <summary>
        /// Flags that indicates unsupported features.
        /// </summary>
        public class NotSupportedFeaturesFlag
        {
            /// <summary>
            /// "IsClustered" porperty of [Index] attribute.
            /// </summary>
            public bool IsClustered { get; set; }

            /// <summary>
            /// "Includes" porperty of [Index] attribute.
            /// </summary>
            public bool Includes { get; set; }
        }

        /// <summary>
        /// Sets or gets the flags to suppress "NotSupportedException" for each unsupported feature.
        /// </summary>
        public NotSupportedFeaturesFlag SuppressNotSupportedException { get; } = new NotSupportedFeaturesFlag();
    }
}
