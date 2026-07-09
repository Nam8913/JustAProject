using System.Xml;
using UnityEngine;

namespace ModContent.XmlExtendedAttribute
{
    /// <summary>
    /// Processor xử lý attribute "Condition" trong XML.
    /// 
    /// Format: Condition="Expression"
    /// VD: Condition="IsDevMode==true"
    ///     → Chỉ deserialize node này nếu condition thỏa mãn
    /// 
    /// Cho phép data-driven conditional loading.
    /// </summary>
    public class ConditionalAttributeProcessor : XmlAttributeProcessor
    {
        public override string AttributeName => "Condition";
        public override int Priority => -10; // Chạy rất sớm để có thể skip node

        /// <summary>
        /// Key trong State chứa kết quả condition evaluation.
        /// </summary>
        public const string StateKeyConditionResult = "Condition.Result";

        public override bool CanProcess(XmlNode node, string value)
        {
            // Luôn process để evaluate condition
            return true;
        }

        public override void Process(XmlAttributeProcessContext context)
        {
            string expression = context.Value;

            if (string.IsNullOrEmpty(expression))
            {
                context.SetState(StateKeyConditionResult, true);
                return;
            }

            // Simple condition evaluation (có thể mở rộng thành expression parser)
            bool result = EvaluateSimpleCondition(expression);

            context.SetState(StateKeyConditionResult, result);

            if (!result)
            {
                context.Log($"Condition '{expression}' evaluated to false. Node will be skipped.");
            }
        }

        /// <summary>
        /// Đánh giá condition đơn giản.
        /// Hiện tại hỗ trợ: "Key==Value", "Key!=Value", "Key>Value", "Key<Value"
        /// Có thể override để thêm expression parser phức tạp hơn.
        /// </summary>
        public virtual bool EvaluateSimpleCondition(string expression)
        {
            // TODO: Implement expression parser đầy đủ
            // Hiện tại chỉ hỗ trợ format đơn giản

            // Fix #6: Dùng string.Equals với OrdinalIgnoreCase thay vì ToLower()
            if (string.Equals(expression, "true", System.StringComparison.OrdinalIgnoreCase)) return true;
            if (string.Equals(expression, "false", System.StringComparison.OrdinalIgnoreCase)) return false;

            // Hỗ trợ so sánh đơn giản
            string[] operators = { "==", "!=", ">", "<", ">=", "<=" };
            foreach (var op in operators)
            {
                int opIndex = expression.IndexOf(op);
                if (opIndex > 0)
                {
                    string left = expression.Substring(0, opIndex).Trim();
                    string right = expression.Substring(opIndex + op.Length).Trim();

                    // Simple string comparison
                    return op switch
                    {
                        "==" => left == right,
                        "!=" => left != right,
                        ">" => string.Compare(left, right) > 0,
                        "<" => string.Compare(left, right) < 0,
                        ">=" => string.Compare(left, right) >= 0,
                        "<=" => string.Compare(left, right) <= 0,
                        _ => true
                    };
                }
            }

            // Nếu không parse được, coi như true
            Debug.LogWarning(
                $"[Conditional] Cannot parse expression '{expression}'. Assuming true.");
            return true;
        }
    }
}
