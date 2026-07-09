using System.Globalization;

namespace ModContent.XmlConverter
{
    public sealed class UnityEngineVector2Converter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value == null) return "0";
            if (value is UnityEngine.Vector2 v)
            {
                return $"{v.x.ToString(CultureInfo.InvariantCulture)}, {v.y.ToString(CultureInfo.InvariantCulture)}";
            }
            throw new System.Exception($"Value is not of type Vector2: {value}");
        }

        public override object fromXML(string xml)
        {
            string[] parts = xml.Split(',');
            if (parts.Length == 2 &&
                float.TryParse(parts[0].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                float.TryParse(parts[1].Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out float y))
            {
                return new UnityEngine.Vector2(x, y);
            }
            throw new System.Exception($"Invalid Vector2 value: {xml}");
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(UnityEngine.Vector2);
        }
    }

    public sealed class UnityEngineVector2IntConverter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value == null) return "0";
            if (value is UnityEngine.Vector2Int v)
            {
                return $"{v.x}, {v.y}";
            }
            throw new System.Exception($"Value is not of type Vector2Int: {value}");
        }

        public override object fromXML(string xml)
        {
            string[] parts = xml.Split(',');
            if (parts.Length == 2 &&
                int.TryParse(parts[0].Trim(), out int x) &&
                int.TryParse(parts[1].Trim(), out int y))
            {
                return new UnityEngine.Vector2Int(x, y);
            }
            throw new System.Exception($"Invalid Vector2Int value: {xml}");
        }

        public override bool CanConvert(System.Type type)
        {
            return type == typeof(UnityEngine.Vector2Int);
        }
    }
}
