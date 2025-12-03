using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlatingStationManager : MonoBehaviour
{
    [Header("Serve Button")]
    public Button serveButton;

    [Header("Score Popup")]
    public GameObject scorePopup;
    public Text scoreText;
    public Text levelText;
    public Button mainMenuButton;
    public Button nextLevelButton;

    [Header("Animation Settings")]
    public float popupAnimationDuration = 0.5f;
    public AnimationCurve popupCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Particle Effects")]
    public ParticleSystem starParticles;
    public ParticleSystem confettiParticles;
    public ParticleSystem sparkleParticles;

    [Header("Score Settings")]
    public int currentLevel = 1;
    public int maxLevel = 5;
    
    // Score calculation variables
    private int baseScore = 100;
    private int timeBonus = 50;
    private int perfectionBonus = 25;

    private void Start()
    {
        // Set up button listeners
        serveButton.onClick.AddListener(OnServeButtonClicked);
        mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        nextLevelButton.onClick.AddListener(OnNextLevelClicked);

        // Initially hide the popup
        if (scorePopup != null)
        {
            scorePopup.SetActive(false);
        }
        
        // Serve button starts enabled since there's no minigame
        serveButton.interactable = true;
        
        // Load saved level progress
        LoadGameProgress();
        
        Debug.Log("🥟 Plating Station Ready - Press Serve when ready!");
    }

    void OnEnable()
    {
        // Reset when station becomes active
        if (scorePopup != null)
        {
            scorePopup.SetActive(false);
        }
        
        StopAllParticles();
        
        // Ensure serve button is enabled
        serveButton.interactable = true;
    }

    // ===== SERVE BUTTON METHODS =====

    public void OnServeButtonClicked()
    {
        Debug.Log("🍽️ Serve button clicked!");
        
        // Calculate final score and stars
        int finalScore = CalculateFinalScore();
        int stars = CalculateStarRating(finalScore);
        int level = currentLevel;
        
        // Show the score popup with animation and particles
        ShowScorePopup(finalScore, stars, level);
        
        // Save progress if this is a new high score
        SaveGameProgress(finalScore, stars);
        
        // Optional: Disable serve button after clicking
        serveButton.interactable = false;
    }

    // ===== SCORE CALCULATION METHODS =====

    private int CalculateFinalScore()
    {
        // Simple scoring for now - replace with your actual game metrics
        int score = baseScore;
        
        // Add some random variation for testing
        score += Random.Range(-20, 30);
        
        return Mathf.Max(score, 50); // Ensure minimum score
    }

    private int CalculateStarRating(int score)
    {
        if (score >= 180) return 3;
        if (score >= 130) return 2;
        return 1;
    }

    private string GetStarDisplay(int stars)
    {
        switch (stars)
        {
            case 1: return "⭐";
            case 2: return "⭐⭐";
            case 3: return "⭐⭐⭐";
            default: return "No Stars";
        }
    }

    // ===== POPUP ANIMATION METHODS =====

    private void ShowScorePopup(int score, int stars, int level)
    {
        if (scorePopup == null) return;

        // Update UI text with actual values
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}\n{GetStarDisplay(stars)}";
        }

        if (levelText != null)
        {
            levelText.text = $"Level: {level}";
        }

        // Show and animate the popup
        scorePopup.SetActive(true);
        StartCoroutine(AnimatePopup(stars));
    }

    private IEnumerator AnimatePopup(int stars)
    {
        if (scorePopup == null) yield break;

        RectTransform popupRect = scorePopup.GetComponent<RectTransform>();
        if (popupRect == null) yield break;

        // Store original scale and start from scaled down
        Vector3 originalScale = popupRect.localScale;
        popupRect.localScale = Vector3.zero;

        // Phase 1: Scale up animation
        float timer = 0f;
        while (timer < popupAnimationDuration)
        {
            timer += Time.deltaTime;
            float t = popupCurve.Evaluate(timer / popupAnimationDuration);
            popupRect.localScale = Vector3.Lerp(Vector3.zero, originalScale, t);
            yield return null;
        }

        popupRect.localScale = originalScale;

        // Phase 2: Trigger particle effects based on star rating
        yield return StartCoroutine(TriggerParticleEffects(stars));
    }

    private IEnumerator TriggerParticleEffects(int stars)
    {
        // Always play base particles
        if (sparkleParticles != null)
        {
            sparkleParticles.Play();
        }

        // Wait a moment before star particles
        yield return new WaitForSeconds(0.3f);

        // Play star particles based on rating
        if (starParticles != null)
        {
            var main = starParticles.main;
            
            // Adjust particle count based on stars
            switch (stars)
            {
                case 1:
                    main.maxParticles = 10;
                    break;
                case 2:
                    main.maxParticles = 25;
                    break;
                case 3:
                    main.maxParticles = 50;
                    // For 3 stars, also play confetti
                    if (confettiParticles != null)
                    {
                        confettiParticles.Play();
                    }
                    break;
            }
            
            starParticles.Play();
        }
    }

    // ===== GAME PROGRESSION METHODS =====

    private void LoadGameProgress()
    {
        currentLevel = PlayerPrefs.GetInt("CurrentLevel", 1);
    }

    private void SaveGameProgress(int score, int stars)
    {
        if (stars >= 2 && currentLevel < maxLevel)
        {
            currentLevel++;
            PlayerPrefs.SetInt("CurrentLevel", currentLevel);
            Debug.Log($"Level advanced to {currentLevel}!");
        }

        int currentHighScore = PlayerPrefs.GetInt($"HighScore_Level{currentLevel}", 0);
        if (score > currentHighScore)
        {
            PlayerPrefs.SetInt($"HighScore_Level{currentLevel}", score);
        }

        PlayerPrefs.Save();
    }

public void OnMainMenuClicked()
{
    PlayButtonSound();
    Debug.Log("Returning to Main Menu");
    
    try
    {
        // Try by index first (more reliable)
        SceneManager.LoadScene(0);
    }
    catch
    {
        // Fallback to name if index fails
        SceneManager.LoadScene("Main Menu");
    }
}

    public void OnNextLevelClicked()
    {
        PlayButtonSound();
        Debug.Log($"Loading Level {currentLevel}");
        
        if (scorePopup != null)
        {
            scorePopup.SetActive(false);
        }

        // Re-enable serve button for next "level"
        serveButton.interactable = true;
        
        Debug.Log("Next level functionality coming soon!");
    }

    private void PlayButtonSound()
    {
        // Optional: Add button click sound
    }

    private void StopAllParticles()
    {
        if (starParticles != null) starParticles.Stop();
        if (confettiParticles != null) confettiParticles.Stop();
        if (sparkleParticles != null) sparkleParticles.Stop();
    }

    void OnDisable()
    {
        StopAllParticles();
    }
}