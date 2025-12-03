using UnityEngine;
using UnityEngine.SceneManagement;
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    [Header("Current Level Data")]
    public int currentDay;
    public MainMenuManager.DumplingOrder[] currentOrders; // Reference the nested class
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // Call this when a level is completed
    public void CompleteLevel()
    {
        // Your existing completion logic
        
        // Unlock the next level
        MainMenuManager.UnlockNextLevel(currentDay);
        
        // Return to main menu
        SceneManager.LoadScene("MainMenu"); // Replace with your main menu scene name
    }
}