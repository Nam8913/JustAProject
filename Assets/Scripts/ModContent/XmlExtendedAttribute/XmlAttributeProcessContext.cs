using System.Collections.Generic;
using System.Xml;
using UnityEngine;

namespace ModContent.XmlExtendedAttribute
{
    /// <summary>
    /// Context được truyền vào XmlAttributeProcessor.Process().
    /// Chứa tất cả thông tin cần thiết để xử lý attribute.
    /// </summary>
    public class XmlAttributeProcessContext
    {
        /// <summary>
        /// XmlNode đang được xử lý.
        /// Dùng để truy cập parent node, sibling nodes, v.v.
        /// </summary>
        public XmlNode Node { get; }

        /// <summary>
        /// Giá trị của attribute đang được xử lý.
        /// </summary>
        public string Value { get; }

        /// <summary>
        /// XmlConverterSettings hiện tại.
        /// Dùng để truy cập converters, other settings.
        /// </summary>
        public XmlConverterSettings Settings { get; }

        /// <summary>
        /// State chia sẻ giữa tất cả processors trong cùng 1 lần deserialize.
        /// Cho phép processors truyền dữ liệu cho nhau.
        /// 
        /// VD: Processor A lưu "parsedType" → Processor B đọc "parsedType" để xử lý tiếp.
        /// </summary>
        public Dictionary<string, object> State { get; }

        /// <summary>
        /// Tên attribute đang được xử lý.
        /// </summary>
        public string AttributeName { get; }

        /// <summary>
        /// Log message mức Info.
        /// </summary>
        public void Log(string message)
        {
            Debug.Log($"[XmlAttribute] {AttributeName}: {message}");
        }

        /// <summary>
        /// Log message mức Warning.
        /// </summary>
        public void LogWarning(string message)
        {
            Debug.LogWarning($"[XmlAttribute] {AttributeName}: {message}");
        }

        /// <summary>
        /// Log message mức Error.
        /// </summary>
        public void LogError(string message)
        {
            Debug.LogError($"[XmlAttribute] {AttributeName}: {message}");
        }

        /// <summary>
        /// Lưu giá trị vào state để processor khác có thể đọc.
        /// </summary>
        public void SetState(string key, object value)
        {
            State[key] = value;
        }

        /// <summary>
        /// Đọc giá trị từ state. Trả về default nếu không tìm thấy.
        /// </summary>
        public T GetState<T>(string key, T defaultValue = default)
        {
            if (State.TryGetValue(key, out object value) && value is T typedValue)
            {
                return typedValue;
            }
            return defaultValue;
        }

        /// <summary>
        /// Kiểm tra state có chứa key không.
        /// </summary>
        public bool HasState(string key)
        {
            return State.ContainsKey(key);
        }

        public XmlAttributeProcessContext(
            XmlNode node,
            string value,
            XmlConverterSettings settings,
            Dictionary<string, object> state,
            string attributeName)
        {
            Node = node;
            Value = value;
            Settings = settings;
            State = state;
            AttributeName = attributeName;
        }
    }
}
