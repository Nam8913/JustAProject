namespace ModContent.XmlConverter
{
    public sealed class UInt16XmlConverter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value == null) return "0";
            if (value is ushort i)
            {
                return i.ToString();
            }
            throw new System.Exception($"Value is not of type ushort: {value}");
        }

        public override object fromXML(string xml)
        {
            if (ushort.TryParse(xml, out ushort result))
            {
                return result;
            }
            throw new System.Exception($"Invalid ushort value: {xml}");
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(ushort);
        }
    }
}
