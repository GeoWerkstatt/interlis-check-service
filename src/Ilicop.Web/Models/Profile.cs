using System.Collections.Generic;

namespace Geowerkstatt.Ilicop.Models
{
    /// <summary>
    /// A validation profile that is used for validating INTERLIS data.
    /// </summary>
    public record Profile
    {
        /// <summary>
        /// Id of the profile.
        /// </summary>
        public string Id { get; init; } = string.Empty;

        /// <summary>
        /// List of the profile's title in different languages.
        /// </summary>
        public List<LocalisedText> LocalisedTitles { get; init; } = new();
    }
}
