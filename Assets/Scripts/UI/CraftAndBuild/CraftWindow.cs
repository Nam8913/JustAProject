using UnityEngine;

public class CraftWindow : MonoBehaviour
{
    public void ToggleSelf()
    {
        this.gameObject.SetActive(!this.gameObject.activeSelf);
    }
}
