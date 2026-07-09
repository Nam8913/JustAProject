namespace ModContent.XmlConverter
{
    public sealed class UInt64XmlConverter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value == null) return "0";
            if (value is ulong i)
            {
                return i.ToString();
            }
            throw new System.Exception($"Value is not of type ulong: {value}");
        }

        public override object fromXML(string xml)
        {
            if (ulong.TryParse(xml, out ulong result))
            {
                return result;
            }
            throw new System.Exception($"Invalid ulong value: {xml}");
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(ulong);
        }
    }
}
