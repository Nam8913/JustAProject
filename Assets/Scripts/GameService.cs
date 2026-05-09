using UnityEngine;

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

    ModernHashNoise _noise;
    World _world;

    WorldHandler _worldHandler;
    GameSettings _settings;

    

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
        _settings = new GameSettings();

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
    }

    

    public Game Game => _game;
    public Game_Play GamePlay => _gamePlay;
    public Game_Entry GameEntry => _gameEntry;
    public ModernHashNoise Noise => _noise;
    public GameSettings Settings => _settings;
    public WorldHandler WorldHandler => _worldHandler;
    
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
