using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

// Move ALL class definitions inside the MainMenuManager class to avoid conflicts
public class MainMenuManager : MonoBehaviour
{
    [System.Serializable]
    public class DumplingOrder
    {
        public DumplingType dumplingType;
        public int quantity;
        public int points;
    }

    [System.Serializable]
    public class LevelData
    {
        public string sceneName = "Game_Main_layout";
        public string levelName;
        public int dayNumber;
        public DumplingOrder[] orders;
        public bool isUnlocked = false;
    }

    public enum DumplingType
    {
        Pork,
        Vegetable,
        Shrimp,
        Chicken,
        Beef
    }

    [System.Serializable]
    public class LevelButton
    {
        public Button button;
        public TextMeshProUGUI dayText;
        public GameObject lockIcon;
    }

    [Header("UI References")]
    public GameObject menuBackground;
    public Button startButton;
    public Button levelsButton;
    public Button quitButton;
    public Button backButton;
    public GameObject levelSelectPanel;
    
    [Header("Level Settings")]
    public LevelData[] levels;
    
    [Header("Level Buttons")]
    public LevelButton[] levelButtons;

    private void Start()
    {
        // Initialize buttons
        startButton.onClick.AddListener(StartGame);
        levelsButton.onClick.AddListener(ShowLevelSelect);
        quitButton.onClick.AddListener(QuitGame);

        if (backButton != null)
            backButton.onClick.AddListener(ShowMainMenu);
        // Initialize levels
        InitializeLevels();
        
        
        // Hide level select panel initially
        if (levelSelectPanel != null)
            levelSelectPanel.SetActive(false);
    }

    private void InitializeLevels()
    {
        // Set up level data
        if (levels.Length >= 1)
        {
            levels[0].dayNumber = 1;
            levels[0].levelName = "Day 1";
            levels[0].isUnlocked = true;
            levels[0].orders = new DumplingOrder[]
            {
                new DumplingOrder { dumplingType = DumplingType.Pork, quantity = 3, points = 10 },
                new DumplingOrder { dumplingType = DumplingType.Vegetable, quantity = 2, points = 8 }
            };
        }

        if (levels.Length >= 2)
        {
            levels[1].dayNumber = 2;
            levels[1].levelName = "Day 2";
            levels[1].isUnlocked = PlayerPrefs.GetInt("Day2Unlocked", 0) == 1;
            levels[1].orders = new DumplingOrder[]
            {
                new DumplingOrder { dumplingType = DumplingType.Pork, quantity = 2, points = 10 },
                new DumplingOrder { dumplingType = DumplingType.Shrimp, quantity = 3, points = 12 },
                new DumplingOrder { dumplingType = DumplingType.Vegetable, quantity = 1, points = 8 }
            };
        }

        // Add levels 3-5 as needed...
        UpdateLevelButtons();
    }

    private void UpdateLevelButtons()
    {
        for (int i = 0; i < levelButtons.Length; i++)
        {
            if (i < levels.Length)
            {
                levelButtons[i].dayText.text = levels[i].levelName;
                levelButtons[i].button.interactable = levels[i].isUnlocked;

                if (levelButtons[i].lockIcon != null)
                    levelButtons[i].lockIcon.SetActive(!levels[i].isUnlocked);
            }
        }
    }

    public void StartGame()
    {
        // Start from Day 1
        if (GameManager.Instance != null)
        {
            GameManager.Instance.currentDay = 1;
            GameManager.Instance.currentOrders = levels[0].orders;
        }
        SceneManager.LoadScene("Game_Main_layout");
    }

public void ShowLevelSelect()
{
    if (levelSelectPanel != null)
        levelSelectPanel.SetActive(true);
    
    // DON'T hide MenuBackground here - or make sure to show it in ShowMainMenu()
}

public void ShowMainMenu()
{
    if (levelSelectPanel != null)
        levelSelectPanel.SetActive(false);
    
    // Make sure MenuBackground is active
    // If you have a reference to it, add:
    // if (menuBackground != null)
    //     menuBackground.SetActive(true);
}

    public void SelectLevel(int levelIndex)
    {
        if (levelIndex < levels.Length && levels[levelIndex].isUnlocked && GameManager.Instance != null)
        {
            GameManager.Instance.currentDay = levels[levelIndex].dayNumber;
            GameManager.Instance.currentOrders = levels[levelIndex].orders;
            SceneManager.LoadScene(levels[levelIndex].sceneName);
        }
    }

    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    // Static method to unlock next level
    public static void UnlockNextLevel(int completedDay)
    {
        MainMenuManager menuManager = FindObjectOfType<MainMenuManager>();
        if (menuManager != null && completedDay < menuManager.levels.Length)
        {
            menuManager.levels[completedDay].isUnlocked = true;
            menuManager.UpdateLevelButtons();

            // Save progression
            PlayerPrefs.SetInt($"Day{completedDay + 1}Unlocked", 1);
            PlayerPrefs.Save();
        }
    }
}