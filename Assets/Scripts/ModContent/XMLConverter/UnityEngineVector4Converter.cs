namespace ModContent.XmlConverter
{
    public sealed class UnityEngineVector4Converter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value == null) return "0";
            if (value is UnityEngine.Vector4 v)
            {
                return $"{v.x}, {v.y}, {v.z}, {v.w}";
            }
            throw new System.Exception($"Value is not of type Vector4: {value}");
        }

        public override object fromXML(string xml)
        {
            string[] parts = xml.Split(',');
            if (parts.Length == 4 &&
                float.TryParse(parts[0].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float x) &&
                float.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float y) &&
                float.TryParse(parts[2].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float z) &&
                float.TryParse(parts[3].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float w))
            {
                return new UnityEngine.Vector4(x, y, z, w);
            }
            throw new System.Exception($"Invalid Vector4 value: {xml}");
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(UnityEngine.Vector4);
        }
    }
}
