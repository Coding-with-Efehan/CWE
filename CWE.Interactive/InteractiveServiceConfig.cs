namespace CWE.Interactive
{
    using System;

    /// <summary>
    /// Configuration for <see cref="InteractiveService"/>.
    /// </summary>
    public class InteractiveServiceConfig
    {
        /// <summary>
        /// Gets or sets the default timeout timespan.
        /// </summary>
        public TimeSpan DefaultTimeout { get; set; } = TimeSpan.FromSeconds(15);
    }
}
