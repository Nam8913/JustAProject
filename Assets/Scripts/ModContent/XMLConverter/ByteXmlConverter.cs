namespace ModContent.XmlConverter
{
    public sealed class ByteXmlConverter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value == null) return "0";
            if (value is byte b)
            {
                return b.ToString();
            }
            throw new System.Exception($"Value is not of type byte: {value}");
        }

        public override object fromXML(string xml)
        {
            if (byte.TryParse(xml, out byte result))
            {
                return result;
            }
            throw new System.Exception($"Invalid byte value: {xml}");
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(byte);
        }
    }
}
