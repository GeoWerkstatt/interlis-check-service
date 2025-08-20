namespace Geowerkstatt.Ilicop.Web.Contracts
{
    public record LocalisedText
    {
        /// <summary>
        /// The language code as INTERLIS LanguageCode_ISO639_1 for the language the text is in (e.g., "en", "de").
        /// </summary>
        public string Language { get; init; } = string.Empty;

        /// <summary>
        /// The text in the specified language.
        /// </summary>
        public string Text { get; init; } = string.Empty;
    }
}
