using System;
using UnityEngine;

public class GameService
{
    static object _lock = new object();
    static GameService _game;

    WorldHandler _worldHandler;
    GameSettings _settings;

    public static GameService Game
    {
        get
        {
            lock (_lock)
            {
                if (_game == null)
                {
                    _game = new GameService();
                }
                return _game;
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
        // Initialize game settings and other necessary components
        _settings = new GameSettings();
    }

    public void RegisterWorld(World world)
    {
        if(_worldHandler != null)
        {
            Debug.LogError("WorldHandler is already registered. Multiple worlds are not supported.");
            return;
        }
        _worldHandler = new GameObject("WorldHandler").AddComponent<WorldHandler>();
    }

    public GameSettings Settings => _settings;
    public WorldHandler WorldHandler => _worldHandler;

    public NavService Navigation => NavService.Instance;

}
