namespace ModContent.XmlConverter
{
    public sealed class UInt32XmlConverter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value == null) return "0";
            if (value is uint i)
            {
                return i.ToString();
            }
            throw new System.Exception($"Value is not of type uint: {value}");
        }

        public override object fromXML(string xml)
        {
            if (uint.TryParse(xml, out uint result))
            {
                return result;
            }
            throw new System.Exception($"Invalid uint value: {xml}");
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(uint);
        }
    }
}
