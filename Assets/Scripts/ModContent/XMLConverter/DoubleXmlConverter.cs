namespace ModContent.XmlConverter
{
    public sealed class DoubleXmlConverter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value == null) return "0";
            if (value is double d)
            {
                return d.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            throw new System.Exception($"Value is not of type double: {value}");
        }

        public override object fromXML(string xml)
        {
            if (double.TryParse(xml, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out double result))
            {
                return result;
            }
            throw new System.Exception($"Invalid double value: {xml}");
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(double);
        }
    }
}
