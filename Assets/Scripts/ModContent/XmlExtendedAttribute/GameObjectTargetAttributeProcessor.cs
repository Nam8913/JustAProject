using System.Xml;
using UnityEngine;

namespace ModContent.XmlExtendedAttribute
{
    /// <summary>
    /// Processor xử lý attribute "GameObjectTarget" trong XML.
    ///
    /// Hỗ trợ 2 format:
    /// 1. GameObjectTarget="TypeName"          → Auto-generate Id từ Id field
    /// 2. GameObjectTarget="TypeName:CustomId" → Dùng CustomId tùy chỉnh
    ///
    /// VD:
    ///   <Define Id="HumanDef" GameObjectTarget="Creature">
    ///     → Đăng ký mapping Id="HumanDef" → Type="Creature"
    ///
    ///   <Define Id="HumanDef" GameObjectTarget="Creature:Player1">
    ///     → Đăng ký mapping Id="Player1" → Type="Creature"
    /// </summary>
    public class GameObjectTargetAttributeProcessor : XmlAttributeProcessor
    {
        public override string AttributeName => "GameObjectTarget";
        public override int Priority => 10;

        /// <summary>
        /// Key trong State chứa Type đã parse.
        /// </summary>
        public const string StateKeyParsedType = "GameObjectTarget.ParsedType";

        /// <summary>
        /// Key trong State chứa Id đã parse.
        /// </summary>
        public const string StateKeyParsedId = "GameObjectTarget.ParsedId";

        public override bool CanProcess(XmlNode node, string value)
        {
            return !string.IsNullOrEmpty(value);
        }

        public override void Process(XmlAttributeProcessContext context)
        {
            string value = context.Value;
            string typeName;
            string id;

            // Hỗ trợ 2 format: "Type" hoặc "Type:Id"
            string[] parts = value.Split(':');
            if (parts.Length == 2)
            {
                // Format cũ: "Type:Id"
                typeName = parts[0].Trim();
                id = parts[1].Trim();
            }
            else if (parts.Length == 1)
            {
                // Format mới: chỉ "Type" → Id sẽ lấy từ node.Id sau
                typeName = parts[0].Trim();
                id = null; // Sẽ resolve trong PostProcess
            }
            else
            {
                context.LogError($"Invalid format: '{value}'. Expected 'Type' or 'Type:Id'.");
                return;
            }

            if (string.IsNullOrEmpty(typeName))
            {
                context.LogError($"Empty type name in value '{value}'.");
                return;
            }

            // Lưu vào state để PostProcess có thể đọc
            context.SetState(StateKeyParsedType, typeName);
            context.SetState(StateKeyParsedId, id);

            // Nếu có Id trực tiếp, đăng ký luôn
            if (!string.IsNullOrEmpty(id))
            {
                ThingHandler.AddThingMappingById(id, typeName);
                context.Log($"Registered mapping: Id='{id}' → Type='{typeName}'");
            }
            else
            {
                context.Log($"Type '{typeName}' resolved. Id will be set from node.Id field in PostProcess.");
            }
        }

        /// <summary>
        /// PostProcess: nếu Id chưa có, lấy từ child node "Id" của XmlNode.
        /// </summary>
        public override void PostProcess(XmlAttributeProcessContext context, object createdObject)
        {
            string typeName = context.GetState<string>(StateKeyParsedType);
            string id = context.GetState<string>(StateKeyParsedId);

            if (string.IsNullOrEmpty(typeName))
            {
                return;
            }

            // Nếu chưa có Id, thử lấy từ child node "Id"
            if (string.IsNullOrEmpty(id))
            {
                XmlNode idNode = context.Node.SelectSingleNode("Id");
                if (idNode != null && !string.IsNullOrEmpty(idNode.InnerText))
                {
                    id = idNode.InnerText.Trim();
                }
            }

            if (string.IsNullOrEmpty(id))
            {
                context.LogWarning($"No Id found for Type '{typeName}'. Skipping registration.");
                return;
            }

            // Đăng ký mapping
            ThingHandler.AddThingMappingById(id, typeName);
            context.SetState(StateKeyParsedId, id);

            context.Log($"Registered mapping (PostProcess): Id='{id}' → Type='{typeName}'");
        }
    }
}
