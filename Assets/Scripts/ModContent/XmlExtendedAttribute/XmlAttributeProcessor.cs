using System.Xml;

namespace ModContent.XmlExtendedAttribute
{
    /// <summary>
    /// Base class cho tất cả XML attribute processors.
    /// Mỗi processor xử lý 1 loại attribute cụ thể trong XML.
    ///
    /// Lifecycle:
    /// 1. Process() - chạy TRƯỚC khi deserialize child nodes
    /// 2. PostProcess() - chạy SAU KHI object đã được tạo xong
    ///
    /// Cách sử dụng:
    /// 1. Tạo class kế thừa XmlAttributeProcessor
    /// 2. Override AttributeName (tên attribute trong XML)
    /// 3. Override Process() để xử lý trước
    /// 4. Override PostProcess() nếu cần xử lý sau khi object tạo xong
    /// 5. Register vào XmlAttributeProcessorRegistry
    /// </summary>
    public abstract class XmlAttributeProcessor
    {
        /// <summary>
        /// Tên attribute trong XML mà processor này xử lý.
        /// Phải là duy nhất trong registry.
        /// </summary>
        public abstract string AttributeName { get; }

        /// <summary>
        /// Thứ tự xử lý (thấp hơn = xử lý trước).
        /// </summary>
        public virtual int Priority => 0;

        /// <summary>
        /// Kiểm tra xem processor có thể xử lý attribute này không.
        /// </summary>
        public virtual bool CanProcess(XmlNode node, string value)
        {
            return true;
        }

        /// <summary>
        /// Xử lý attribute TRƯỚC khi deserialize child nodes.
        /// Dùng để: parse attribute, validate, set state, đăng ký mappings.
        /// </summary>
        public abstract void Process(XmlAttributeProcessContext context);

        /// <summary>
        /// Xử lý POST-PROCESSING SAU KHI object đã được tạo xong.
        /// Dùng để: modify object, attach components, validate final state.
        ///
        /// VD: GameObjectTargetProcessor có thể set name cho GameObject ở đây.
        ///
        /// Mặc định: không làm gì.
        /// </summary>
        /// <param name="context">Context chứa thông tin attribute</param>
        /// <param name="createdObject">Object vừa được tạo từ XML (có thể null nếu primitive type)</param>
        public virtual void PostProcess(XmlAttributeProcessContext context, object createdObject)
        {
            // Default: no-op
        }
    }
}
