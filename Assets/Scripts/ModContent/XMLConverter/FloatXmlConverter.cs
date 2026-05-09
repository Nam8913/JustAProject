namespace ModContent.XmlConverter
{
    public sealed class FloatXmlConverter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value == null) return "0";
            if (value is float f)
            {
                return f.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }
            throw new System.Exception($"Value is not of type float: {value}");
        }

        public override object fromXML(string xml)
        {
            if (float.TryParse(xml, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float result))
            {
                return result;
            }
            throw new System.Exception($"Invalid float value: {xml}");
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(float);
        }
    }
}
