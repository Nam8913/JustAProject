namespace ModContent.XmlConverter
{
    public sealed class LongXmlConverter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value == null) return "0";
            if (value is long l)
            {
                return l.ToString();
            }
            throw new System.Exception($"Value is not of type long: {value}");
        }

        public override object fromXML(string xml)
        {
            if (long.TryParse(xml, out long result))
            {
                return result;
            }
            throw new System.Exception($"Invalid long value: {xml}");
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(long);
        }
    }
}
