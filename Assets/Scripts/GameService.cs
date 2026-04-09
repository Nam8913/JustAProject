using UnityEngine;

public class GameService
{
    static object _lock = new object();
    static GameService _game;

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

    public GameSettings Settings => _settings;

}
