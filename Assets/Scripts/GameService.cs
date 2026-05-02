using System;
using UnityEngine;

public class GameService
{
    static object _lock = new object();
    static bool isStarted = false;
    static GameService _gameService;
    static Camera _mainCamera;
    static PlayerInputActions _playerInputActions = new PlayerInputActions();
    static BuildService _buildService = new BuildService();

    Game _game;
    ModernHashNoise _noise;

    WorldHandler _worldHandler;
    GameSettings _settings;

    
    

    

    public static GameService Ins
    {
        get
        {
            lock (_lock)
            {
                if (_gameService == null && isStarted)
                {
                    _gameService = new GameService();
                }else if (!isStarted)
                {
                    if(Application.isPlaying)
                    {
                        Debug.LogError("GameService is not started. Please register a world to start the game.");
                    }
                    return null;
                }
                return _gameService;
            }
        }
    }

    private GameService()
    {
        // Private constructor to prevent instantiation from outside
        Initialize();
    }


    private void Initialize()
    {
        _mainCamera = Camera.main;
        _game = GameObject.FindAnyObjectByType<Game>();
        if(_game == null)
        {
            Debug.LogError("No Game component found in the scene. Please add a Game component to a GameObject.");
            return;
        }
        if(SceneHandler.IsEntryScene())
        {
            
        }else if(SceneHandler.IsPlayScene())
        {
            // Initialize play scene specific components
        }
        #if UNITY_EDITOR
        else
        {
            //Test dev here
            Debug.Log(SceneHandler.CurrentScene);
        }
        #endif
        
        // Initialize game settings and other necessary components
        _settings = new GameSettings();
    }

    public static void RegisterWorld(World world)
    {
        isStarted = true;
        if(Ins._worldHandler != null)
        {
            Debug.LogError("WorldHandler is already registered. Multiple worlds are not supported.");
            return;
        }
        Ins._worldHandler = new GameObject("WorldHandler").AddComponent<WorldHandler>();
        Ins._noise = new ModernHashNoise(world.Seed); // Initialize the noise generator
    }

    public Game Game => _game;
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

}
