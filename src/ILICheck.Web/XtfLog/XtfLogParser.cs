using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ILICheck.Web.XtfLog
{
    /// <summary>
    /// Provides a method to parse XTF log files.
    /// </summary>
    public static class XtfLogParser
    {
        /// <summary>
        /// Parses an XTF log file.
        /// </summary>
        /// <param name="xtfLogReader">Reader of an XTF log file.</param>
        /// <returns>The entries of the log basket.</returns>
        public static IList<LogError> Parse(TextReader xtfLogReader)
        {
            using var xmlReader = XmlReader.Create(xtfLogReader);
            var serializer = new XmlSerializer(typeof(Transfer));
            var transfer = (Transfer)serializer.Deserialize(xmlReader);
            return transfer.Datasection.ErrorLogBasket.Errors;
        }
    }
}
