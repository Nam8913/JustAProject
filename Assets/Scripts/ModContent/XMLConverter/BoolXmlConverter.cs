namespace ModContent.XmlConverter
{
    public sealed class BoolXmlConverter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value == null) return "false";
            if (value is bool b)
            {
                return b.ToString().ToLowerInvariant();
            }
            throw new System.Exception($"Value is not of type bool: {value}");
        }

        public override object fromXML(string xml)
        {
            if (bool.TryParse(xml, out bool result))
            {
                return result;
            }
            throw new System.Exception($"Invalid bool value: {xml}");
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(bool);
        }
    }
}
