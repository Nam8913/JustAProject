using ModContent;
using UnityEngine;

public class TallThing : Thing
{
    // Một tallthing phải có ít nhất 2 sprite , 1 cái để hiện thị root và 1 cái để hiện thị phần trên của tallthing
    [SerializeField]
    private Sprite[] sprites = new Sprite[2];
    [SerializeField]
    private GameObject topPart;
    [SerializeField]
    private GameObject rootPart;
    [SerializeField]
    private Vector2 anchor;

    private static Vector3 GetShiftToAnchor(Sprite sprite, Vector2 anchorPixel)
    {
        if (sprite == null)
        {
            return Vector3.zero;
        }

        Vector2 offsetPixels = sprite.pivot - anchorPixel;
        return new Vector3(offsetPixels.x / sprite.pixelsPerUnit, offsetPixels.y / sprite.pixelsPerUnit, 0f);
    }

    private static Vector3 GetShiftToAnchorNormalized(Sprite sprite, Vector2 anchorNormalized)
    {
        if (sprite == null)
        {
            return Vector3.zero;
        }

        Vector2 normalizedAnchor = new Vector2(
            Mathf.Clamp01(anchorNormalized.x),
            Mathf.Clamp01(anchorNormalized.y));
        Vector2 anchorPixel = Vector2.Scale(sprite.rect.size, normalizedAnchor);
        return GetShiftToAnchor(sprite, anchorPixel);
    }

    public override void Start()
    {
        base.Start();

        rbg2d.gravityScale = 0f;

        // điểm neo cho ảnh tree hiện tại đang dùng 
        // TODO: need to make this anchor configurable in future
        anchor = new Vector2(0.472718f, 0.5139963f);

        topPart = new GameObject("TopPart");
        topPart.transform.SetParent(transform, false);

        rootPart = new GameObject("RootPart");
        rootPart.transform.SetParent(transform, false);

        Vector2 rootAnchorNormalized = anchor != Vector2.zero
            ? anchor
            : new Vector2(0.5f, 0f);
        Vector3 shift = GetShiftToAnchorNormalized(sprites[0], rootAnchorNormalized);

        rootPart.transform.localPosition = shift;
        topPart.transform.localPosition = shift;

        SpriteRenderer rootRender = rootPart.AddComponent<SpriteRenderer>();
        rootRender.sprite = sprites[0];

        SpriteRenderer topRender = topPart.AddComponent<SpriteRenderer>();
        topRender.sprite = sprites[1];
        
        CircleCollider2D collider = gameObject.GetComponent<CircleCollider2D>();
        collider.radius = 0.4f;
    }

    public override void FixedUpdate()
    {
        
    }

    public override void Setup()
    {
        if(GraphicData.Is<MultiGraphicData>(def.graphicData, out MultiGraphicData multiGraphicData))
        {
            int count = multiGraphicData.metaData.Count;
            if(count >= 2)
            {
                if(count >= sprites.Length)
                {
                    sprites = new Sprite[count];
                }
                for(int i = 0; i < count; i++)
                {
                    var metaData = multiGraphicData.metaData[i];
                    string path = $"{metaData.path}"+$"{(string.IsNullOrEmpty(metaData.extraId) ? "" : $"_{metaData.extraId}")}";
                    var sprite = LocalPackAssets.GetAsset<Sprite>(path);
                    sprites[i] = sprite;
                }
            }
            else
            {
                Debug.LogError($"TallThing with ID: {Id} must have at least 2 sprites in its MultiGraphicData.");
            }
        }
        else
        {
            Debug.LogError($"TallThing with ID: {Id} must have MultiGraphicData for its graphic data.");
        }

        this.gameObject.AddComponent<CircleCollider2D>();
        this.gameObject.AddComponent<Rigidbody2D>();
        this.gameObject.AddComponent<SpriteRenderer>();
        
        
    }
}
