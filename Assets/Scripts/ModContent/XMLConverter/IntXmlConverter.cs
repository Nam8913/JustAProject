namespace ModContent.XmlConverter
{
    public sealed class IntXmlConverter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value == null) return "0";
            if (value is int i)
            {
                return i.ToString();
            }
            throw new System.Exception($"Value is not of type int: {value}");
        }

        public override object fromXML(string xml)
        {
            if (int.TryParse(xml, out int result))
            {
                return result;
            }
            throw new System.Exception($"Invalid int value: {xml}");
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(int);
        }
    }
}
