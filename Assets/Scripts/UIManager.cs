using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// UIManager handles the user interface elements of the game.
/// Currently manages the Main Menu but is designed to be extensible for future UI components.
/// </summary>
public class UIManager : MonoBehaviour
{
    // Singleton instance
    private static readonly object _lock = new object();
    private static UIManager _instance;
    public static UIManager Instance
    {
        get
        {
            // Lock ensures thread safety when accessing the instance
            lock (_lock)
            {
                if (_instance == null)
                {
                    // Find existing instance in the scene
                    _instance = FindObjectOfType<UIManager>();
                    if (_instance == null)
                    {
                        // If no instance is found, create a new one
                        GameObject uiManagerObject = new GameObject("UIManager");
                        _instance = uiManagerObject.AddComponent<UIManager>();
                        DontDestroyOnLoad(uiManagerObject); // Make sure the UIManager persists across scenes
                        Debug.Log("UIManager instance created.");
                    }
                    else
                    {
                        Debug.Log("UIManager instance found in the scene.");
                    }
                }
                return _instance;
            }
        }
    }

    private const string GameSceneName = "GameScene";
    private const string GameWorldSceneName = "GameWorld";
    private const string GameUISceneName = "GameUI";

    [Header("Main Menu Panels")]
    [SerializeField] private GameObject mainMenuPanel; // The panel that contains the main menu UI elements

    [Header("Main Menu Buttons")]
    [SerializeField] private Button startGameButton; // Button to start the game
    [SerializeField] private Button optionsButton; // Button to open options menu
    [SerializeField] private Button exitButton; // Button to exit the game

    // Dictionary to manage different UI panels in the future
    private Dictionary<string, GameObject> uiPanels = new Dictionary<string, GameObject>();

    private void Awake()
    {
        Debug.Log("UIManager Awake called.");
        // Implement Singleton pattern with thread safety
        if (_instance != null && _instance != this)
        {
            // Destroy duplicate instance if one already exists
            Debug.LogWarning("Duplicate UIManager instance found. Destroying it.");
            Destroy(this.gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(this.gameObject);

        // Initialize UI Panels dictionary
        InitializeUIPanels();

        // Subscribe to game state changes from GameManager
        //GameManager.Instance.OnStateChanged += HandleGameStateChanged;
    }

    private void OnDestroy()
    {
        // Unsubscribe from game state changes when UIManager is destroyed
        if (GameManager.Instance != null)
        {
            //GameManager.Instance.OnStateChanged -= HandleGameStateChanged;
        }
    }

    private void Start()
    {
        Debug.Log("UIManager Start called.");
        // Assign button listeners to their respective callbacks
        AssignButtonListeners();

        // Show Main Menu on start
        ShowPanel("MainMenu");
    }

    /// <summary>
    /// Initializes the UI panels and adds them to the dictionary.
    /// Extend this method to include more UI panels as the game grows.
    /// </summary>
    private void InitializeUIPanels()
    {
        Debug.Log("Initializing UI Panels.");
        if (mainMenuPanel != null)
        {
            uiPanels.Add("MainMenu", mainMenuPanel); // Add main menu panel to the dictionary
            Debug.Log("Main Menu panel added to UI Panels dictionary.");
        }
        else
        {
            Debug.LogError("Main Menu Panel is not assigned in the UIManager.");
        }

        // Future panels can be added here
        // Example:
        // uiPanels.Add("Options", optionsPanel);
        // uiPanels.Add("PauseMenu", pauseMenuPanel);
    }

    /// <summary>
    /// Assigns the click listeners to the main menu buttons.
    /// </summary>
    private void AssignButtonListeners()
    {
        Debug.Log("Assigning button listeners.");
        if (startGameButton != null)
        {
            startGameButton.onClick.AddListener(OnStartGameClicked); // Assign Start Game button click listener
            Debug.Log("Start Game button listener assigned.");
        }
        else
        {
            Debug.LogError("Start Game Button is not assigned in the UIManager.");
        }

        if (optionsButton != null)
        {
            optionsButton.onClick.AddListener(OnOptionsClicked); // Assign Options button click listener
            Debug.Log("Options button listener assigned.");
        }
        else
        {
            Debug.LogError("Options Button is not assigned in the UIManager.");
        }

        if (exitButton != null)
        {
            exitButton.onClick.AddListener(OnExitClicked); // Assign Exit button click listener
            Debug.Log("Exit button listener assigned.");
        }
        else
        {
            Debug.LogError("Exit Button is not assigned in the UIManager.");
        }
    }

    /// <summary>
    /// Shows the specified UI panel and hides others.
    /// </summary>
    /// <param name="panelName">The name of the panel to show.</param>
    public void ShowPanel(string panelName)
    {
        Debug.Log($"Showing panel: {panelName}");
        if (uiPanels.ContainsKey(panelName))
        {
            // Iterate through all panels and set the active state based on the specified panel name
            foreach (KeyValuePair<string, GameObject> panel in uiPanels)
            {
                panel.Value.SetActive(panel.Key == panelName);
                Debug.Log($"Panel {panel.Key} set to {(panel.Key == panelName ? "active" : "inactive")}.");
            }
        }
        else
        {
            Debug.LogError($"Panel with name {panelName} does not exist in the UIManager.");
        }
    }

    /// <summary>
    /// Hides all UI panels.
    /// </summary>
    public void HideAllPanels()
    {
        Debug.Log("Hiding all panels.");
        // Set all panels to inactive
        foreach (KeyValuePair<string, GameObject> panel in uiPanels)
        {
            panel.Value.SetActive(false);
            Debug.Log($"Panel {panel.Key} set to inactive.");
        }
    }

    #region Button Callbacks

    private void OnStartGameClicked()
    {
        Debug.Log("Start Game button clicked.");
        // Change game state to Playing
        GameManager.Instance.ChangeState(GameManager.GameState.Playing);
    }

    private void OnOptionsClicked()
    {
        Debug.Log("Options button clicked. To be implemented.");
        // Placeholder for showing options menu
        // Example:
        // ShowPanel("Options");
    }

    private void OnExitClicked()
    {
        Debug.Log("Exit button clicked. Quitting game.");
        // Quit the application
        GameManager.Instance.QuitGame();
    }

    #endregion

    #region Utility Methods

    /*
    /// <summary>
    /// Handles game state changes.
    /// </summary>
    /// <param name="newState">The new game state.</param>
    private void HandleGameStateChanged(GameManager.GameState newState)
    {
        Debug.Log($"Handling game state change: {newState}");
        switch (newState)
        {
            case GameManager.GameState.MainMenu:
                ShowPanel("MainMenu");
                break;
            case GameManager.GameState.Playing:
                HideAllPanels();
                break;
            case GameManager.GameState.Paused:
                Debug.Log("Game paused. UI can be updated to reflect pause state if needed.");
                break;
            case GameManager.GameState.GameOver:
                Debug.Log("Game over state reached. UI can be updated accordingly.");
                break;
            default:
                Debug.LogWarning("Unhandled game state: " + newState);
                break;
        }
    }
    */

    #endregion

    #region Future UI Methods

    /// <summary>
    /// Example method to show the Options panel.
    /// Implement this once the Options UI is created.
    /// </summary>
    public void ShowOptions()
    {
        Debug.Log("ShowOptions called. To be implemented.");
        // ShowPanel("Options");
    }

    // Add more methods to handle other UI elements as needed.

    #endregion
}
