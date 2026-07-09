using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using UnityEngine;

namespace ModContent.XmlExtendedAttribute
{
    /// <summary>
    /// Registry quản lý tất cả XmlAttributeProcessors.
    /// Chịu trách nhiệm: đăng ký, tìm kiếm, và gọi processors khi deserialize.
    /// 
    /// Cách sử dụng:
    /// 1. Tạo registry: var registry = new XmlAttributeProcessorRegistry();
    /// 2. Register processors: registry.Register(new GameObjectTargetProcessor());
    /// 3. Gọi ProcessAttributes() khi deserialize XML.
    /// 
    /// Thread-safe: Không (chỉ dùng trong Unity main thread).
    /// </summary>
    public class XmlAttributeProcessorRegistry
    {
        private readonly List<XmlAttributeProcessor> _processors = new List<XmlAttributeProcessor>();
        private readonly Dictionary<string, XmlAttributeProcessor> _byAttributeName = new Dictionary<string, XmlAttributeProcessor>();
        private bool _sorted = true;

        /// <summary>
        /// Số lượng processors đã đăng ký.
        /// </summary>
        public int Count => _processors.Count;

        /// <summary>
        /// Đăng ký 1 processor mới.
        /// Nếu attribute name đã tồn tại, sẽ ghi đè (với log warning).
        /// </summary>
        /// <param name="processor">Processor cần đăng ký</param>
        public void Register(XmlAttributeProcessor processor)
        {
            if (processor == null)
            {
                Debug.LogError("[XmlAttributeRegistry] Cannot register null processor.");
                return;
            }

            string attributeName = processor.AttributeName;
            if (string.IsNullOrEmpty(attributeName))
            {
                Debug.LogError($"[XmlAttributeRegistry] Processor {processor.GetType().Name} has empty AttributeName.");
                return;
            }

            if (_byAttributeName.ContainsKey(attributeName))
            {
                Debug.LogWarning(
                    $"[XmlAttributeRegistry] Attribute '{attributeName}' already registered. " +
                    $"Replacing {_byAttributeName[attributeName].GetType().Name} with {processor.GetType().Name}.");
            }

            _byAttributeName[attributeName] = processor;
            _processors.Add(processor);
            _sorted = false;
        }

        /// <summary>
        /// Đăng ký nhiều processors cùng lúc.
        /// </summary>
        public void RegisterAll(IEnumerable<XmlAttributeProcessor> processors)
        {
            foreach (var processor in processors)
            {
                Register(processor);
            }
        }

        /// <summary>
        /// Xóa processor theo attribute name.
        /// </summary>
        public bool Unregister(string attributeName)
        {
            if (_byAttributeName.TryGetValue(attributeName, out var processor))
            {
                _byAttributeName.Remove(attributeName);
                _processors.Remove(processor);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Lấy processor theo attribute name.
        /// </summary>
        public XmlAttributeProcessor GetProcessor(string attributeName)
        {
            _byAttributeName.TryGetValue(attributeName, out var processor);
            return processor;
        }

        /// <summary>
        /// Kiểm tra attribute name có processor không.
        /// </summary>
        public bool HasProcessor(string attributeName)
        {
            return _byAttributeName.ContainsKey(attributeName);
        }

        /// <summary>
        /// Xử lý tất cả attributes trong XmlNode.
        /// Gọi processor tương ứng cho mỗi attribute.
        /// </summary>
        /// <param name="node">XmlNode cần xử lý attributes</param>
        /// <param name="settings">XmlConverterSettings hiện tại</param>
        public void ProcessAttributes(XmlNode node, XmlConverterSettings settings)
        {
            ProcessAttributes(node, settings, new Dictionary<string, object>());
        }

        /// <summary>
        /// Xử lý tất cả attributes với state được truyền từ bên ngoài.
        /// </summary>
        /// <param name="node">XmlNode cần xử lý attributes</param>
        /// <param name="settings">XmlConverterSettings hiện tại</param>
        /// <param name="state">State chia sẻ (processor có thể đọc/ghi)</param>
        public void ProcessAttributes(XmlNode node, XmlConverterSettings settings, Dictionary<string, object> state)
        {
            if (node?.Attributes == null || node.Attributes.Count == 0)
            {
                return;
            }

            EnsureSorted();

            foreach (XmlAttribute attr in node.Attributes)
            {
                if (_byAttributeName.TryGetValue(attr.Name, out var processor))
                {
                    try
                    {
                        // Kiểm tra điều kiện trước khi process
                        if (!processor.CanProcess(node, attr.Value))
                        {
                            continue;
                        }

                        // Tạo context và process
                        var context = new XmlAttributeProcessContext(
                            node,
                            attr.Value,
                            settings,
                            state,
                            attr.Name);

                        processor.Process(context);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(
                            $"[XmlAttributeRegistry] Error processing attribute '{attr.Name}' " +
                            $"with processor {processor.GetType().Name}: {ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
        }

        /// <summary>
        /// Xử lý POST-PROCESSING cho tất cả processors đã chạy trước đó.
        /// Gọi SAU KHI object đã được tạo xong.
        /// </summary>
        /// <param name="node">XmlNode chứa attribute</param>
        /// <param name="settings">XmlConverterSettings hiện tại</param>
        /// <param name="state">State từ quá trình Process trước đó</param>
        /// <param name="createdObject">Object vừa được tạo (có thể null nếu primitive)</param>
        public void PostProcessAll(XmlNode node, XmlConverterSettings settings,
            Dictionary<string, object> state, object createdObject)
        {
            if (node?.Attributes == null || node.Attributes.Count == 0)
            {
                return;
            }

            EnsureSorted();

            foreach (XmlAttribute attr in node.Attributes)
            {
                if (_byAttributeName.TryGetValue(attr.Name, out var processor))
                {
                    try
                    {
                        if (!processor.CanProcess(node, attr.Value))
                        {
                            continue;
                        }

                        var context = new XmlAttributeProcessContext(
                            node, attr.Value, settings, state, attr.Name);

                        processor.PostProcess(context, createdObject);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError(
                            $"[XmlAttributeRegistry] Error in PostProcess for '{attr.Name}': " +
                            $"{ex.Message}\n{ex.StackTrace}");
                    }
                }
            }
        }

        /// <summary>
        /// Xử lý 1 attribute cụ thể.
        /// </summary>
        public bool ProcessSingleAttribute(XmlNode node, string attributeName, string value, XmlConverterSettings settings)
        {
            if (!_byAttributeName.TryGetValue(attributeName, out var processor))
            {
                return false;
            }

            try
            {
                if (!processor.CanProcess(node, value))
                {
                    return false;
                }

                var state = new Dictionary<string, object>();
                var context = new XmlAttributeProcessContext(
                    node,
                    value,
                    settings,
                    state,
                    attributeName);

                processor.Process(context);
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError(
                    $"[XmlAttributeRegistry] Error processing attribute '{attributeName}': {ex.Message}\n{ex.StackTrace}");
                return false;
            }
        }

        /// <summary>
        /// Lấy danh sách tất cả attribute names đã đăng ký.
        /// </summary>
        public IEnumerable<string> GetAllAttributeNames()
        {
            return _byAttributeName.Keys;
        }

        /// <summary>
        /// Lấy danh sách tất cả processors, sắp xếp theo Priority.
        /// </summary>
        public IReadOnlyList<XmlAttributeProcessor> GetAllProcessors()
        {
            EnsureSorted();
            return _processors.AsReadOnly();
        }

        /// <summary>
        /// Xóa tất cả processors.
        /// </summary>
        public void Clear()
        {
            _processors.Clear();
            _byAttributeName.Clear();
        }

        private void EnsureSorted()
        {
            if (!_sorted)
            {
                _processors.Sort((a, b) => a.Priority.CompareTo(b.Priority));
                _sorted = true;
            }
        }
    }
}
