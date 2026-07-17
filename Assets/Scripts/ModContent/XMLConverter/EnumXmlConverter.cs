using System;
using System.Collections.Generic;

namespace ModContent.XmlConverter
{
    /// <summary>
    /// Converter cho enum types. Hỗ trợ cả enum thường và [Flags] enum.
    ///
    /// Format serialize:
    /// - Enum thường: "ValueA"
    /// - Flags enum: "ValueA|ValueB" (pipe-separated)
    ///
    /// Format deserialize:
    /// - Hỗ trợ cả tên giá trị và số nguyên
    /// - Flags enum: "ValueA|ValueB" hoặc "3" (numeric)
    /// </summary>
    public sealed class EnumXmlConverter : XmlConverter
    {
        public override string toXML(object value)
        {
            if (value == null) return "";

            if (value is Enum enumValue)
            {
                Type enumType = enumValue.GetType();

                // Flags enum: "ValueA|ValueB"
                if (enumType.IsDefined(typeof(FlagsAttribute), false))
                {
                    long numericValue = Convert.ToInt64(enumValue);
                    var values = Enum.GetValues(enumType);
                    var selected = new List<string>();

                    foreach (var val in values)
                    {
                        long numVal = Convert.ToInt64(val);
                        // Bỏ qua giá trị 0 (None) và kiểm tra flag đã set
                        if (numVal != 0 && (numericValue & numVal) == numVal)
                            selected.Add(val.ToString());
                    }

                    return selected.Count > 0 ? string.Join("|", selected) : "None";
                }

                // Normal enum: "ValueA"
                return enumValue.ToString();
            }

            return value.ToString();
        }

        public override object fromXML(string xml)
        {
            // Không dùng trực tiếp - dùng DeserializeEnum<T> generic method
            return null;
        }

        public override bool CanConvert(Type type)
        {
            return type.IsEnum;
        }

        public override bool canRead => false; // Dùng DeserializeEnum<T> thay vì fromXML
        public override bool canWrite => true;
    }
}
