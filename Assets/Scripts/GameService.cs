using UnityEngine;
using UnityEngine.UI;

public class GameService
{
    static object _lock = new object();
    static GameService _gameService;
    static Camera _mainCamera;
    static PlayerInputActions _playerInputActions = new PlayerInputActions();
    static BuildService _buildService = new BuildService();

    static Game _game;
    static Game_Play _gamePlay;
    static Game_Entry _gameEntry;

    static Canvas _canvas;
    static CanvasScaler _canvasScaler;
    static GraphicRaycaster _graphicRaycaster;

    ModernHashNoise _noise;
    World _world;

    WorldHandler _worldHandler;
    GameSettings _settings;

    PlayerController _playerController;
    GameObject _focusObject;
    

    public static GameService Ins
    {
        get
        {
            lock (_lock)
            {
                if (_gameService == null)
                {
                    _gameService = new GameService();
                }
                return _gameService;
            }
        }
    }

    private GameService()
    {
        // Private constructor to prevent instantiation from outside
    }

    public void GlobalInitialize()
    {
        // Initialize game settings and other necessary components
        _settings = XmlLoader.LoadFromXml<GameSettings>(FilePathHandler.SettingsFilePath);
        if(_settings == null)
        {
            _settings = new GameSettings();
        }

        LoadAllData.LoadAll();
    }

    // This method should be called when the scene changes to re-initialize necessary components
    public void InitializeWhenChangeScene()
    {
        Debug.Log("Initializing GameService for new scene...");
        _mainCamera = Camera.main;
        
        
        if(SceneHandler.IsEntryScene())
        {
            _gamePlay = null;
            _gameEntry = GameObject.FindAnyObjectByType<Game_Entry>();
            _game = _gameEntry;
        }else if(SceneHandler.IsPlayScene())
        {
            _gameEntry = null;
            _gamePlay = GameObject.FindAnyObjectByType<Game_Play>();
            _game = _gamePlay;
        }
        #if UNITY_EDITOR
        else
        {
            //Test dev here
            Debug.Log(SceneHandler.CurrentScene);
        }
        #endif

        GameObject canvasObj = GameObject.Find("Canvas");
        if (canvasObj != null)
        {
            _canvas = canvasObj.GetComponent<Canvas>();
            _canvasScaler = canvasObj.GetComponent<CanvasScaler>();
            _graphicRaycaster = canvasObj.GetComponent<GraphicRaycaster>();
        }else
        {
            canvasObj = new GameObject("Canvas");
            _canvas = canvasObj.AddComponent<Canvas>();
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            _canvasScaler = canvasObj.AddComponent<CanvasScaler>();
            _graphicRaycaster = canvasObj.AddComponent<GraphicRaycaster>();
        }

        {
            _canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            Resolution resolution = GameService.Settings.GetCurrentResolution();
            _canvasScaler.referenceResolution = new Vector2(resolution.width, resolution.height);
            _canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            _canvasScaler.matchWidthOrHeight = 0.5f;
        }
    }

    public void SetFocusObject(GameObject obj)
    {
        _focusObject = obj;

        if(this._playerController != null)
        {
            this._playerController.SetFocusObject(obj);
        }else
        {
            _playerController = obj.GetComponent<PlayerController>();
            if(_playerController == null)
            {
                _playerController = obj.AddComponent<PlayerController>();
            }
            this._playerController.SetFocusObject(obj);
        }
    }

    public GameObject GetFocusObject()
    {
        return _focusObject;
    }

    

    public static Game Game => _game;
    public static Game_Play GamePlay => _gamePlay;
    public static Game_Entry GameEntry => _gameEntry;
    public static ModernHashNoise Noise => _gameService._noise;
    public static GameSettings Settings => _gameService._settings;
    public static WorldHandler WorldHandler => _gameService._worldHandler;

    public static Canvas Canvas => _canvas;
    public static CanvasScaler CanvasScaler => _canvasScaler;
    public static GraphicRaycaster GraphicRaycaster => _graphicRaycaster;
    
    public static PlayerController PlayerController
    {
        get
        {
            return Ins._playerController;
        }
        set
        {
            Ins._playerController = value;
        }
    }
    public static CraftWindow CraftWindow
    {
        get
        {
            return PlayUI.CraftWindow;
        }
    }
    public static BuildWindow BuildWindow
    {
        get
        {
            return PlayUI.BuildWindow;
        }
    }
    
    public static Camera MainCamera
    {
        get
        {
            if(_mainCamera == null)
            {
                _mainCamera = Camera.main;
            }
            return _mainCamera;
        }
        set
        {
            _mainCamera = value;
        }
    }

    public NavService Navigation => NavService.Instance;

    public static BuildService BuildService
    {
        get
        {
            return _buildService;
        }
    }

    public static PlayerInputActions PlayerInput => _playerInputActions;

    public static void SetWorld(World world)
    {
        Ins._world = world;
    }

    public static World GetWorld()
    {
        return Ins._world;
    }

    public static void SetWorldHandler(WorldHandler worldHandler)
    {
        Ins._worldHandler = worldHandler;
    }

    public static WorldHandler GetWorldHandler()
    {
        return Ins._worldHandler;
    }

    public static void SetNoise(ModernHashNoise noise)
    {
        Ins._noise = noise;
    }

    public static ModernHashNoise GetNoise()
    {
        return Ins._noise;
    }

}
