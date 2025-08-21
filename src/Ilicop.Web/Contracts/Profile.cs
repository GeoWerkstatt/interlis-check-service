using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Geowerkstatt.Ilicop.Web.Contracts
{
    /// <summary>
    /// A validation profile that is used for validating INTERLIS data.
    /// </summary>
    public record Profile
    {
        /// <summary>
        /// Id of the profile.
        /// </summary>
        [Required]
        public string Id { get; init; } = string.Empty;

        /// <summary>
        /// List of the profile's title in different languages.
        /// </summary>
        public List<LocalisedText> Titles { get; init; } = new();
    }
}
