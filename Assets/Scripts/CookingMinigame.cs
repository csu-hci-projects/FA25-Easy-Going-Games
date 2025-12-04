using UnityEngine;
using UnityEngine.UI;
using TMPro; // Added for TextMeshPro support

public class CookingMinigame : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("The entire GameObject holding the minigame UI (Sliders, etc.) to hide/show")]
    public GameObject minigameUIContainer; 
    [Tooltip("The slider representing the current temperature/heat")]
    public Slider heatSlider; 
    [Tooltip("The visual graphic for the green zone behind the heat slider")]
    public RectTransform sweetSpotRect; 
    [Tooltip("The slider showing how close the dumpling is to being done")]
    public Slider progressSlider;
    [Tooltip("Assign a separate Text object here (NOT a button label) to show status")]
    public TMP_Text statusText; // Changed from Text to TMP_Text

    [Header("Cooking Profiles")]
    public CookingProfile fryProfile;
    public CookingProfile steamProfile;
    public CookingProfile boilProfile;

    // Internal State
    private CookingProfile currentProfile;
    private float currentHeat = 0f;
    private float cookProgress = 0f;
    private bool isMinigameActive = false;
    private float sweetSpotCenter = 0.5f; // 0.0 to 1.0
    private float boilMoveTimer = 0f;

    [System.Serializable]
    public struct CookingProfile
    {
        public string name;
        [Range(0.1f, 5f)] public float heatUpSpeed;    // How fast bar goes up
        [Range(0.1f, 5f)] public float coolDownSpeed;  // How fast bar drops (Gravity)
        [Range(0.05f, 0.5f)] public float sweetSpotSize; // Width of green zone (0-1)
        public float cookSpeed;      // How fast progress fills when in zone
        public bool isMovingTarget;  // Does the green zone move? (For Boiling)
    }

    void Start()
    {
        // Ensure the minigame UI is hidden when the scene starts
        if (minigameUIContainer != null)
        {
            minigameUIContainer.SetActive(false);
        }
        isMinigameActive = false;
    }

    // --- Button Click Events ---
    // Link these to your Buttons' OnClick() events in the Inspector
    public void OnFryButton() => StartCooking(CookingType.Fry);
    public void OnSteamButton() => StartCooking(CookingType.Steam);
    public void OnBoilButton() => StartCooking(CookingType.Boil);

    public enum CookingType { Fry, Steam, Boil }

    public void StartCooking(CookingType type)
    {
        // Show the UI
        if (minigameUIContainer != null)
        {
            minigameUIContainer.SetActive(true);
        }

        currentHeat = 0f;
        cookProgress = 0f;
        isMinigameActive = true;
        
        switch (type)
        {
            case CookingType.Fry: currentProfile = fryProfile; break;
            case CookingType.Steam: currentProfile = steamProfile; break;
            case CookingType.Boil: currentProfile = boilProfile; break;
        }

        // Setup UI initially
        UpdateSweetSpotVisuals(0.5f); // Start in middle
        
        // Reset status text
        if (statusText) 
        {
            statusText.text = "Heat up!";
            statusText.color = Color.white;
        }
    }

    void Update()
    {
        if (!isMinigameActive) return;

        HandleHeatPhysics();
        HandleSweetSpotLogic();
        CheckProgress();
        UpdateUI();
    }

    void HandleHeatPhysics()
    {
        // Input: Hold Space or Touch Screen
        bool isHoldingInput = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);

        if (isHoldingInput)
        {
            currentHeat += currentProfile.heatUpSpeed * Time.deltaTime;
        }
        else
        {
            currentHeat -= currentProfile.coolDownSpeed * Time.deltaTime;
        }

        // Clamp heat between 0 and 1
        currentHeat = Mathf.Clamp01(currentHeat);
    }

    void HandleSweetSpotLogic()
    {
        // Logic for moving target (Boiling effect)
        if (currentProfile.isMovingTarget)
        {
            boilMoveTimer += Time.deltaTime;
            // Moves the spot back and forth using Sine wave
            sweetSpotCenter = 0.5f + (Mathf.Sin(boilMoveTimer) * 0.25f);
            UpdateSweetSpotVisuals(sweetSpotCenter);
        }
        else
        {
            // Static target, keep it steady or randomize once at start
            sweetSpotCenter = 0.5f;
            UpdateSweetSpotVisuals(sweetSpotCenter);
        }
    }

    void CheckProgress()
    {
        // Calculate the min and max bounds of the green zone
        float halfSize = currentProfile.sweetSpotSize / 2f;
        float minBound = sweetSpotCenter - halfSize;
        float maxBound = sweetSpotCenter + halfSize;

        // Check if our heat needle is inside the bounds
        if (currentHeat >= minBound && currentHeat <= maxBound)
        {
            // We are cooking!
            cookProgress += currentProfile.cookSpeed * Time.deltaTime;
            
            // Visual Feedback: Text turns green and says "COOKING!"
            if (statusText) 
            {
                statusText.text = "COOKING!";
                statusText.color = Color.green;
            }
        }
        else
        {
            // Optional: Decrease progress or burn logic here
            
            // Visual Feedback: Text turns white and tells user to adjust
            if (statusText) 
            {
                // Simple logic to tell user what to do
                statusText.text = "Adjust Heat..."; 
                statusText.color = Color.white;
            }
        }

        // Win Condition
        if (cookProgress >= 1f)
        {
            cookProgress = 1f;
            WinGame();
        }
    }

    void UpdateUI()
    {
        heatSlider.value = currentHeat;
        progressSlider.value = cookProgress;
    }

    void UpdateSweetSpotVisuals(float center)
    {
        // Adjusts the Green Zone RectTransform to match the logic
        if (sweetSpotRect == null || heatSlider == null) return;

        // Use the Slider's width as the reference, not the SweetSpot's parent
        // This allows the SweetSpot to be placed anywhere (e.g. on top of the slider)
        RectTransform sliderRect = heatSlider.GetComponent<RectTransform>();
        float referenceWidth = sliderRect.rect.width;
        
        // Set width based on difficulty size
        float width = referenceWidth * currentProfile.sweetSpotSize;
        sweetSpotRect.sizeDelta = new Vector2(width, sweetSpotRect.sizeDelta.y);

        // Set position based on center (0 to 1 mapping to local position)
        float xPos = (center - 0.5f) * referenceWidth;
        sweetSpotRect.localPosition = new Vector3(xPos, sweetSpotRect.localPosition.y, 0);
    }

    void WinGame()
    {
        isMinigameActive = false;
        
        if (statusText) 
        {
            statusText.text = "FINISHED!";
            statusText.color = Color.green;
        }
        
        // Wait 1.5 seconds then hide the minigame
        Invoke("HideMinigame", 1.5f);
    }

    void HideMinigame()
    {
        if (minigameUIContainer != null)
        {
            minigameUIContainer.SetActive(false);
        }
    }
}