using UnityEngine;
using UnityEngine.SceneManagement;
using System;

public class GameManager : MonoBehaviour
{
    // Singleton instance
    private static readonly object lockObject = new object();
    private static GameManager instance;
    public static GameManager Instance
    {
        get
        {
            lock (lockObject)
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<GameManager>();
                    if (instance == null)
                    {
                        GameObject singletonObject = new("GameManager");
                        instance = singletonObject.AddComponent<GameManager>();
                        DontDestroyOnLoad(singletonObject);
                    }
                }
                return instance;
            }
        }
        private set { instance = value; }
    }

    // Define different game states
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        GameOver
    }

    // Current state of the game
    private GameState currentState;

    // Property to access the current state
    public GameState CurrentState
    {
        get { return currentState; }
        private set
        {
            currentState = value;
            Debug.Log("[INFO] Game state changed to: " + currentState);
            OnStateChanged?.Invoke(currentState);
        }
    }

    // Event triggered when the game state changes
    public event Action<GameState> OnStateChanged;

    // Awake is called when the script instance is being loaded
    private void Awake()
    {
        Debug.Log("[INFO] Awake called in GameManager");
        // Implement Singleton pattern
        lock (lockObject)
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(gameObject); // Persist across scenes
                Debug.Log("[INFO] GameManager instance created and set to persist across scenes");
            }
            else if (instance != this)
            {
                Debug.LogWarning("[WARNING] Duplicate GameManager instance found, destroying it");
                Destroy(gameObject); // Enforce singleton
                return;
            }
        }
    }

    // Start is called before the first frame update
    private void Start()
    {
        Debug.Log("[INFO] Start called in GameManager");
        // Initialize the game state
        ChangeState(GameState.MainMenu);
    }

    /// <summary>
    /// Changes the current game state and performs associated actions.
    /// </summary>
    /// <param name="newState">The new state to transition to.</param>
    public void ChangeState(GameState newState)
    {
        Debug.Log("[INFO] Attempting to change game state to: " + newState);
        if (currentState == newState)
        {
            Debug.Log("[INFO] Game state is already: " + newState + ", no change needed");
            return;
        }

        CurrentState = newState;

        switch (newState)
        {
            case GameState.MainMenu:
                Debug.Log("[INFO] Loading MainMenu scene");
                LoadScene("MainMenu");
                break;
            case GameState.Playing:
                Debug.Log("[INFO] Loading GameScene");
                LoadScene("GameWorld");
                LoadScene("GameUI", LoadSceneMode.Additive);
                break;
            case GameState.Paused:
                Debug.Log("[INFO] Game paused, disabling game objects");
                SetPauseState(true);
                break;
            case GameState.GameOver:
                Debug.Log("[INFO] Loading GameOver scene");
                LoadScene("GameOver");
                break;
            default:
                Debug.LogWarning("[WARNING] Unhandled game state: " + newState);
                break;
        }
    }

    /// <summary>
    /// Loads a scene asynchronously.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load.</param>
    public void LoadScene(string sceneName)
    {
        Debug.Log("[INFO] Loading scene asynchronously: " + sceneName);
        // Reset time scale in case the game was paused
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(sceneName);
    }

    /// <summary>
    /// Loads a scene asynchronously, with a mode.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load.</param>
    public void LoadScene(string sceneName, LoadSceneMode mode)
    {
        Debug.Log("[INFO] Loading scene asynchronously: " + sceneName);
        // Reset time scale in case the game was paused
        Time.timeScale = 1;
        SceneManager.LoadSceneAsync(sceneName, mode);
    }

    /// <summary>
    /// Unloads a scene asynchronously.
    /// </summary>
    /// <param name="sceneName">Name of the scene to load.</param>
    public void UnloadScene(string sceneName)
    {
        Debug.Log("[INFO] Loading scene asynchronously: " + sceneName);
        // Reset time scale in case the game was paused
        Time.timeScale = 1;
        SceneManager.UnloadSceneAsync(sceneName);
    }

    /// <summary>
    /// Reloads the current active scene.
    /// </summary>
    public void ReloadCurrentScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        Debug.Log("[INFO] Reloading current scene asynchronously: " + activeScene.name);
        LoadScene(activeScene.name);
    }

    /// <summary>
    /// Exits the game/application.
    /// </summary>
    public void QuitGame()
    {
        Debug.Log("[INFO] QuitGame called");
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    /// <summary>
    /// Toggles the paused state of the game.
    /// </summary>
    public void TogglePause()
    {
        Debug.Log("[INFO] TogglePause called");
        if (CurrentState == GameState.Paused)
        {
            Debug.Log("[INFO] Unpausing game, enabling game objects");
            SetPauseState(false);
            ChangeState(GameState.Playing);
        }
        else if (CurrentState == GameState.Playing)
        {
            Debug.Log("[INFO] Pausing game, disabling game objects");
            SetPauseState(true);
            ChangeState(GameState.Paused);
        }
    }

    /// <summary>
    /// Sets the paused state for game objects.
    /// </summary>
    /// <param name="isPaused">Whether the game is paused.</param>
    private void SetPauseState(bool isPaused)
    {
        GameObject[] gameObjects = FindObjectsOfType<GameObject>();
        foreach (GameObject obj in gameObjects)
        {
            if (obj.CompareTag("Pauseable"))
            {
                obj.SetActive(!isPaused);
            }
        }
    }

    // Optional: Handle actions when a new scene is loaded
    private void OnEnable()
    {
        Debug.Log("[INFO] GameManager OnEnable called");
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        Debug.Log("[INFO] GameManager OnDisable called");
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log("[INFO] Loaded scene: " + scene.name);
    }
}