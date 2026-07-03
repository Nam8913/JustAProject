using System.Collections.Generic;
using UnityEngine;

public abstract class UIWindow : MonoBehaviour
{
    [SerializeField] protected CanvasGroup canvasGroup;
    [SerializeField] private readonly List<UIWindow> childPopups = new();

    public void RegisterChild(UIWindow child) => childPopups.Add(child);

    public virtual void Show()
    {
        gameObject.SetActive(true);
        SetInteractable(true);
        OnShown();
    }

    public virtual void Hide()
    {
        childPopups.ForEach(child => child.Hide());// Đóng tất cả các child popups khi window bị đóng
        childPopups.Clear(); // Xóa danh sách child popups sau khi đóng
        
        SetInteractable(false);
        OnHidden();
        gameObject.SetActive(false);
    }

    void CheckCanvasGroup()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                Debug.LogWarning($"CanvasGroup is not assigned and not found on {gameObject.name}. Please assign it in the inspector or add a CanvasGroup component.");
            }
        }
    }

    // Thêm hàm này
    private void SetInteractable(bool value)
    {
        CheckCanvasGroup();
        canvasGroup.interactable = value;
        canvasGroup.blocksRaycasts = value;
    }

    protected virtual void OnShown() { }
    protected virtual void OnHidden() { }
}