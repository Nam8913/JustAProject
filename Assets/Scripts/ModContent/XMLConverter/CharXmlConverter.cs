namespace ModContent.XmlConverter
{
    public sealed class CharXmlConverter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value == null) return "";
            if (value is char c)
            {
                return c.ToString();
            }
            throw new System.Exception($"Value is not of type char: {value}");
        }

        public override object fromXML(string xml)
        {
            if (xml.Length == 1)
            {
                return xml[0];
            }
            throw new System.Exception($"Invalid char value: {xml}");
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(char);
        }
    }
}
