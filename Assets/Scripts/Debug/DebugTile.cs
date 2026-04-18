using UnityEngine;

public class DebugTile : MonoBehaviour
{
    public Tile tile;
    public bool isWalkable;
    public float moveCost;

    void Start()
    {
        SpriteRenderer renderer = this.GetComponent<SpriteRenderer>();

        if (isWalkable)
        {
            renderer.color = Color.white;
        }
        else
        {
            renderer.color = Color.red;
        }

        if(moveCost > 0f)
        {
            float value = moveCost * 255f;
            renderer.color = new Color(value/255f, value/255f, value/255f);
        }
    }
}
