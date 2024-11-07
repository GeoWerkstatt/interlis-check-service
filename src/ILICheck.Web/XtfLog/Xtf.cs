using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace ILICheck.Web.XtfLog
{
    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.interlis.ch/INTERLIS2.3")]
    [XmlRoot(ElementName = "TRANSFER", Namespace = "http://www.interlis.ch/INTERLIS2.3", IsNullable = false)]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1649:File name should match first type name", Justification = "Accepted for xtf log XML classes.")]
    public class Transfer
    {
        [XmlElement("DATASECTION")]
        public Datasection Datasection { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.interlis.ch/INTERLIS2.3")]
    public class Datasection
    {
        [XmlElement("IliVErrors.ErrorLog")]
        public ErrorLogBasket ErrorLogBasket { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.interlis.ch/INTERLIS2.3")]
    public class ErrorLogBasket
    {
        [XmlAttribute("BID")]
        public string Bid { get; set; }

        [XmlElement("IliVErrors.ErrorLog.Error")]
        public List<LogError> Errors { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.interlis.ch/INTERLIS2.3")]
    public class LogError
    {
        [XmlAttribute("TID")]
        public string Tid { get; set; }
        public string Message { get; set; }
        public string Type { get; set; }
        public string ObjTag { get; set; }
        public string DataSource { get; set; }
        public int? Line { get; set; }
        public Geometry Geometry { get; set; }
        public string TechDetails { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.interlis.ch/INTERLIS2.3")]
    public class Geometry
    {
        [XmlElement("COORD")]
        public Coord Coord { get; set; }
    }

    [Serializable]
    [XmlType(AnonymousType = true, Namespace = "http://www.interlis.ch/INTERLIS2.3")]
    public class Coord
    {
        public decimal C1 { get; set; }
        public decimal C2 { get; set; }
    }
}
