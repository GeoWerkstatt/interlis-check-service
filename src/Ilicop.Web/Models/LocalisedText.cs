namespace Geowerkstatt.Ilicop.Models
{
    public record LocalisedText
    {
        /// <summary>
        /// The language code (e.g., "en", "de") for the language the text is in.
        /// </summary>
        public string Language { get; init; } = string.Empty;

        /// <summary>
        /// The text in the specified language.
        /// </summary>
        public string Text { get; init; } = string.Empty;
    }
}
