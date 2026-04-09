using UnityEngine;

public class DefineThing : MonoBehaviour
{
    [SerializeField]
    protected string labelName;
    [SerializeField]
    [TextArea(3, 10)]
    protected string labelDescription;

    public virtual void ConfigError()
    {
        if (string.IsNullOrEmpty(labelName))
        {
            Debug.LogError($"Label name is missing for {this.GetType().Name}");
        }
    }
}
