using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace ILICheck.Web.XtfLog
{
    public static class XtfLogParser
    {
        public static IList<LogError> Parse(TextReader xtfLogReader)
        {
            using var xmlReader = XmlReader.Create(xtfLogReader);
            var serializer = new XmlSerializer(typeof(Transfer));
            var transfer = (Transfer)serializer.Deserialize(xmlReader);
            return transfer.Datasection.ErrorLogBasket.Errors;
        }
    }
}
