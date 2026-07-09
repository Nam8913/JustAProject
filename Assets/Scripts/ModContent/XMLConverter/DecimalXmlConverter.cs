namespace ModContent.XmlConverter
{
    public sealed class DecimalXmlConverter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value == null) return "0";
            if (value is decimal i)
            {
                return i.ToString();
            }
            throw new System.Exception($"Value is not of type decimal: {value}");
        }

        public override object fromXML(string xml)
        {
            if (decimal.TryParse(xml, out decimal result))
            {
                return result;
            }
            throw new System.Exception($"Invalid decimal value: {xml}");
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(decimal);
        }
    }
}
