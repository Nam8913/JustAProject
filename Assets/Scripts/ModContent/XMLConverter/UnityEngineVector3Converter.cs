namespace ModContent.XmlConverter
{
    public sealed class UnityEngineVector3Converter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value == null) return "0";
            if (value is UnityEngine.Vector3 v)
            {
                return $"{v.x}, {v.y}, {v.z}";
            }
            throw new System.Exception($"Value is not of type Vector3: {value}");
        }

        public override object fromXML(string xml)
        {
            string[] parts = xml.Split(',');
            if (parts.Length == 3 &&
                float.TryParse(parts[0].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float x) &&
                float.TryParse(parts[1].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float y) &&
                float.TryParse(parts[2].Trim(), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float z))
            {
                return new UnityEngine.Vector3(x, y, z);
            }
            throw new System.Exception($"Invalid Vector3 value: {xml}");
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(UnityEngine.Vector3);
        }
    }

    public sealed class UnityEngineVector3IntConverter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value == null) return "0";
            if (value is UnityEngine.Vector3Int v)
            {
                return $"{v.x}, {v.y}, {v.z}";
            }
            throw new System.Exception($"Value is not of type Vector3Int: {value}");
        }

        public override object fromXML(string xml)
        {
            string[] parts = xml.Split(',');
            if (parts.Length == 3 &&
                int.TryParse(parts[0].Trim(), out int x) &&
                int.TryParse(parts[1].Trim(), out int y) &&
                int.TryParse(parts[2].Trim(), out int z))
            {
                return new UnityEngine.Vector3Int(x, y, z);
            }
            throw new System.Exception($"Invalid Vector3Int value: {xml}");
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(UnityEngine.Vector3Int);
        }
    }
}
