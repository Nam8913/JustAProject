using System.Linq;

namespace ModContent.XmlConverter
{
    public sealed class TypeXmlConverter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value is System.Type type)
            {
                return type.AssemblyQualifiedName ?? type.FullName ?? type.Name;
            }

            return value?.ToString() ?? "";
        }

        public override object fromXML(string xml)
        {
            if (string.IsNullOrWhiteSpace(xml))
            {
                return null;
            }

            var type = System.Type.GetType(xml);
            if (type != null)
            {
                return type;
            }

            return TypeUtils.GetAllTypes().FirstOrDefault(t =>
                t.FullName == xml ||
                t.AssemblyQualifiedName == xml ||
                t.Name == xml);
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(System.Type);
        }
    }
}
