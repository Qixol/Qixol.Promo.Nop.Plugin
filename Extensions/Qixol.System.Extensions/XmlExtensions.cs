using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace Qixol.System.Extensions
{
    public static class XmlExtensions
    {
        public static T ToObject<T>(this string xmlString, Type[] additionalTypes = null)
        {
            object objectFromXml = new object();
            
            if (string.IsNullOrEmpty(xmlString))
            {
                return default(T);
            }

            try
            {

                XmlSerializer xmlSerializer = new XmlSerializer(typeof(T), additionalTypes);
                StringReader stringReader = new StringReader(xmlString);
                XmlTextReader xmlTextReader = new XmlTextReader(stringReader);
                objectFromXml = xmlSerializer.Deserialize(xmlTextReader);
                xmlTextReader.Close();
                stringReader.Close();
            }
            catch
            {
                return default(T);
            }

            return (T)objectFromXml;
        }

        public static string ToXmlString(this object xmlObject, Type[] additionalTypes = null)
        {
            string returnString = null;

            if (xmlObject == null)
                return returnString;

            try
            {
                XmlSerializer xmlSerializer = new XmlSerializer(xmlObject.GetType(), additionalTypes);
                MemoryStream memoryStream = new MemoryStream();
                StreamWriter streamWriter = new StreamWriter(memoryStream);
                xmlSerializer.Serialize(streamWriter, xmlObject);

                UTF8Encoding encoding = new UTF8Encoding();
                returnString = encoding.GetString(memoryStream.ToArray());
                memoryStream.Close();
                streamWriter.Close();
            }
            catch
            {
                return returnString;
            }

            return returnString;
        }
    }
}