// using UnityEngine;

// /// <summary>
// /// Ghost entity - chỉ có visual, không có logic.
// /// Dùng để preview building trước khi đặt thật.
// /// Không add collider → không block placement check.
// /// </summary>
// public class BlueprintEntity : MonoBehaviour
// {
//     public Define sourceDefine;
//     public bool isValidPlacement = true;


//     private Define def;
//     private string labelName;
//     private string Id;

//     public void SetupBlueprint(Define define, Sprite sprite)
//     {
//         sourceDefine = define;
//         this.def = define;
//         this.Id = define.Id;
//         this.labelName = define.label;

//         // Chỉ thêm SpriteRenderer, KHÔNG thêm collider
//         SpriteRenderer renderer = GetComponent<SpriteRenderer>();
//         if (renderer == null)
//             renderer = gameObject.AddComponent<SpriteRenderer>();

//         renderer.sprite = sprite;
//         renderer.color = new Color(1f, 1f, 1f, 0.5f); // Semi-transparent
//         renderer.sortingLayerName = "Ghost";
//         renderer.sortingOrder = 10;
//     }

//     public void SetValidity(bool valid)
//     {
//         isValidPlacement = valid;
//         SpriteRenderer renderer = GetComponent<SpriteRenderer>();
//         if (renderer != null)
//         {
//             renderer.color = valid
//                 ? new Color(0f, 1f, 0f, 0.5f)  // Xanh = hợp lệ
//                 : new Color(1f, 0f, 0f, 0.5f);  // Đỏ = không hợp lệ
//         }
//     }
// }
