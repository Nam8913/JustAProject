namespace ModContent.XmlConverter
{
    public sealed class SByteXmlConverter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value == null) return "0";
            if (value is sbyte i)
            {
                return i.ToString();
            }
            throw new System.Exception($"Value is not of type sbyte: {value}");
        }

        public override object fromXML(string xml)
        {
            if (sbyte.TryParse(xml, out sbyte result))
            {
                return result;
            }
            throw new System.Exception($"Invalid sbyte value: {xml}");
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(sbyte);
        }
    }
}
