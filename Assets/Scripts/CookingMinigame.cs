using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CookingMinigame : MonoBehaviour
{
    [Header("UI Containers")]
    [Tooltip("The parent object holding your Fry/Steam/Boil buttons")]
    public GameObject selectionMenuContainer; 
    [Tooltip("The parent object holding the Sliders (Heat/Progress)")]
    public GameObject minigameUIContainer; 

    [Header("UI Controls")]
    public Slider heatSlider; 
    public RectTransform sweetSpotRect; 
    public Slider progressSlider;
    public TMP_Text statusText;

    [Header("Cooking Profiles")]
    public CookingProfile fryProfile;
    public CookingProfile steamProfile;
    public CookingProfile boilProfile;

    // Internal State
    private CookingProfile currentProfile;
    private float currentHeat = 0f;
    private float cookProgress = 0f;
    private bool isMinigameActive = false;
    private float sweetSpotCenter = 0.5f; 
    private float boilMoveTimer = 0f;

    [System.Serializable]
    public struct CookingProfile
    {
        public string name;
        [Range(0.1f, 5f)] public float heatUpSpeed;    
        [Range(0.1f, 5f)] public float coolDownSpeed;  
        [Range(0.05f, 0.5f)] public float sweetSpotSize; 
        public float cookSpeed;      
        public bool isMovingTarget; 
    }

    void Start()
    {
        // 1. Hide the Minigame Sliders at start
        if (minigameUIContainer != null) minigameUIContainer.SetActive(false);
        
        // 2. Ensure Buttons are visible at start
        if (selectionMenuContainer != null) selectionMenuContainer.SetActive(true);
        
        isMinigameActive = false;
    }

    // --- Button Click Events ---
    public void OnFryButton() => StartCooking(CookingType.Fry);
    public void OnSteamButton() => StartCooking(CookingType.Steam);
    public void OnBoilButton() => StartCooking(CookingType.Boil);

    public enum CookingType { Fry, Steam, Boil }

    public void StartCooking(CookingType type)
    {
        // 1. Show the Sliders
        if (minigameUIContainer != null) minigameUIContainer.SetActive(true);
        
        // 2. Hide the Buttons
        if (selectionMenuContainer != null) selectionMenuContainer.SetActive(false);

        currentHeat = 0f;
        cookProgress = 0f;
        isMinigameActive = true;
        
        switch (type)
        {
            case CookingType.Fry: currentProfile = fryProfile; break;
            case CookingType.Steam: currentProfile = steamProfile; break;
            case CookingType.Boil: currentProfile = boilProfile; break;
        }

        // Setup UI
        UpdateSweetSpotVisuals(0.5f); 
        
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
        bool isHoldingInput = Input.GetKey(KeyCode.Space) || Input.GetMouseButton(0);

        if (isHoldingInput)
        {
            currentHeat += currentProfile.heatUpSpeed * Time.deltaTime;
        }
        else
        {
            currentHeat -= currentProfile.coolDownSpeed * Time.deltaTime;
        }

        currentHeat = Mathf.Clamp01(currentHeat);
    }

    void HandleSweetSpotLogic()
    {
        if (currentProfile.isMovingTarget)
        {
            boilMoveTimer += Time.deltaTime;
            sweetSpotCenter = 0.5f + (Mathf.Sin(boilMoveTimer) * 0.25f);
            UpdateSweetSpotVisuals(sweetSpotCenter);
        }
        else
        {
            sweetSpotCenter = 0.5f;
            UpdateSweetSpotVisuals(sweetSpotCenter);
        }
    }

    void CheckProgress()
    {
        float halfSize = currentProfile.sweetSpotSize / 2f;
        float minBound = sweetSpotCenter - halfSize;
        float maxBound = sweetSpotCenter + halfSize;

        if (currentHeat >= minBound && currentHeat <= maxBound)
        {
            cookProgress += currentProfile.cookSpeed * Time.deltaTime;
            if (statusText) 
            {
                statusText.text = "COOKING!";
                statusText.color = Color.green;
            }
        }
        else
        {
            if (statusText) 
            {
                statusText.text = "Adjust Heat..."; 
                statusText.color = Color.white;
            }
        }

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
        if (sweetSpotRect == null || heatSlider == null) return;

        RectTransform sliderRect = heatSlider.GetComponent<RectTransform>();
        float referenceWidth = sliderRect.rect.width;
        
        float width = referenceWidth * currentProfile.sweetSpotSize;
        sweetSpotRect.sizeDelta = new Vector2(width, sweetSpotRect.sizeDelta.y);

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
        
        Invoke("HideMinigame", 1.5f);
    }

    void HideMinigame()
    {
        // 1. Hide the Sliders
        if (minigameUIContainer != null) minigameUIContainer.SetActive(false);

        // 2. Bring back the Buttons (so you can choose again or see the menu)
        if (selectionMenuContainer != null) selectionMenuContainer.SetActive(true);
    }
}