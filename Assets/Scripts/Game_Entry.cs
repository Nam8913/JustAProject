using UnityEngine;
using UnityEngine.UI;

public class Game_Entry : Game
{
    public Canvas canvas {get; private set;}
    public CanvasScaler canvasScaler {get; private set;}
    public GraphicRaycaster graphicRaycaster {get; private set;} 

    public override void Start()
    {
        base.Start();
        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            canvas = canvasObj.GetComponent<Canvas>();
            canvasScaler = canvasObj.GetComponent<CanvasScaler>();
            graphicRaycaster = canvasObj.GetComponent<GraphicRaycaster>();
        }else
        {
            canvasObj = new GameObject("Canvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            graphicRaycaster = canvasObj.AddComponent<GraphicRaycaster>();
        }

        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            Resolution resolution = GameService.Ins.Settings.GetCurrentResolution();
            canvasScaler.referenceResolution = new Vector2(resolution.width, resolution.height);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
        }
    }

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
    }

    public override void FixedUpdate()
    {
        base.FixedUpdate();
    }

    public override void LateUpdate()
    {
        base.LateUpdate();
    }

    public override void OnEnable()
    {
        base.OnEnable();
    }

    public override void OnDisable()
    {
        base.OnDisable();
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
    }
}
