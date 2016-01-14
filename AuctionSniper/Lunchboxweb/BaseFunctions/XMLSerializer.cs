using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using DAS.Domain;

namespace Lunchboxweb.BaseFunctions
{
    public class XMLSerializer
    {

        public static SortableBindingList<T> Deserialize<T>(string filePath) where T : new()
        {
            var ds = new SortableBindingList<T>();
            if (File.Exists(filePath))
            {
                using (Stream loadstream = new FileStream(filePath, FileMode.Open))
                {
                    var serializer = new XmlSerializer(typeof(SortableBindingList<T>));
                    ds = (SortableBindingList<T>)serializer.Deserialize(loadstream);
                }

            }

            return ds;
        }

        public static void Serialize(string filepath, object p)
        {
            var doc = new XmlDocument();
            var x = new XmlSerializer(p.GetType());
            var sb = new StringBuilder();

            using (var writer = new StringWriter(sb))
            {
                x.Serialize(writer, p);
            }

            doc.LoadXml(sb.ToString());
            doc.Save(filepath);
        }

    }
}
