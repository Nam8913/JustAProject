namespace ModContent.XmlConverter
{
    public sealed class StringXmlConverter : XmlConverter
    {
        public override string toXML(object value)
        {
            return value?.ToString() ?? "";
        }

        public override object fromXML(string xml)
        {
            return xml;
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(string);
        }
    }
}
