namespace ModContent.XmlConverter
{
    public sealed class ShortXmlConverter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value == null) return "0";
            if (value is short s)
            {
                return s.ToString();
            }
            throw new System.Exception($"Value is not of type short: {value}");
        }

        public override object fromXML(string xml)
        {
            if (short.TryParse(xml, out short result))
            {
                return result;
            }
            throw new System.Exception($"Invalid short value: {xml}");
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(short);
        }
    }
}
