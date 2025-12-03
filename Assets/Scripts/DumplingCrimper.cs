using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class DumplingCrimper : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject selectionMenu;
    public GameObject gameArea;

    [Header("Minigame Elements")]
    public Image dumplingImage;
    public RectTransform tickMark;
    public RectTransform greenZone;
    public RectTransform gaugeArea;
    public Button crimpButton;

    [Header("Selection Buttons")]
    public Button simpleButton;
    public Button pleatedButton;
    public Button crescentButton;

    [Header("Sprite Sequences")]
    public Sprite[] simpleSequence;
    public Sprite[] pleatedSequence;
    public Sprite[] crescentSequence;

    [Header("Unfolded Dumpling Sprites")]
    public Sprite porkUnfoldedSprite;
    public Sprite veggieUnfoldedSprite;

    [Header("Completed Dumpling Sprites")]
    public Sprite manapuaSprite;
    public Sprite gyozaSprite;
    public Sprite wontonSprite;

    [Header("Animation Settings")]
    public float scaleAnimationDuration = 0.3f;

    [Header("Difficulty Tuning")]
    public float simpleSpeed = 60f;      // SLOWER: was 100f
    public float pleatedSpeed = 80f;     // SLOWER: was 120f
    public float crescentSpeed = 100f;   // SLOWER: was 140f

    [Header("Debug")]
    public bool enableDebugControls = true;

    private Sprite[] currentSequence;
    private float currentSpeed;
    private int clicksNeeded;
    private int currentClicks;
    private int maxAttempts;
    private int attemptsUsed;
    private bool movingRight = true;
    private bool isGameActive = false;
    private bool isAnimating = false;
    private StationManager stationManager;
    private int selectedFoldStyle = -1;

    public enum DumplingType
    {
        Pork_Gyoza,
        Veggie_Gyoza,
        Pork_Manapua,
        Veggie_Manapua,
        Pork_Wonton,
        Veggie_Wonton
    }
    private DumplingType currentDumplingType = DumplingType.Pork_Manapua;

    void Start()
    {
        Debug.Log("=== DUMPLING CRIMPER START ===");
        stationManager = GetComponentInParent<StationManager>();
        SetupButtonListeners();
        TestButtonAssignments();
    }

    void TestButtonAssignments()
    {
        Debug.Log("=== BUTTON ASSIGNMENT TEST ===");
        Debug.Log("Simple Button: " + (simpleButton != null ? "ASSIGNED" : "NULL"));
        Debug.Log("Pleated Button: " + (pleatedButton != null ? "ASSIGNED" : "NULL"));
        Debug.Log("Crescent Button: " + (crescentButton != null ? "ASSIGNED" : "NULL"));
        Debug.Log("Crimp Button: " + (crimpButton != null ? "ASSIGNED" : "NULL"));
        Debug.Log("Selection Menu: " + (selectionMenu != null ? "ASSIGNED" : "NULL"));
        Debug.Log("Game Area: " + (gameArea != null ? "ASSIGNED" : "NULL"));
        Debug.Log("Green Zone Width: " + (greenZone != null ? greenZone.sizeDelta.x.ToString() : "NULL"));
    }

    void SetupButtonListeners()
    {
        Debug.Log("Setting up button listeners...");

        if (simpleButton != null)
        {
            simpleButton.onClick.RemoveAllListeners();
            simpleButton.onClick.AddListener(OnSimpleButtonClicked);
            Debug.Log("✓ Simple button listener added");
        }

        if (pleatedButton != null)
        {
            pleatedButton.onClick.RemoveAllListeners();
            pleatedButton.onClick.AddListener(OnPleatedButtonClicked);
            Debug.Log("✓ Pleated button listener added");
        }

        if (crescentButton != null)
        {
            crescentButton.onClick.RemoveAllListeners();
            crescentButton.onClick.AddListener(OnCrescentButtonClicked);
            Debug.Log("✓ Crescent button listener added");
        }

        if (crimpButton != null)
        {
            crimpButton.onClick.RemoveAllListeners();
            crimpButton.onClick.AddListener(OnCrimpButtonClicked);
            Debug.Log("✓ Crimp button listener added");
        }
    }

    void OnSimpleButtonClicked()
    {
        Debug.Log("🎯 SIMPLE BUTTON CLICKED!");
        SelectFoldStyle(0);
    }

    void OnPleatedButtonClicked()
    {
        Debug.Log("🎯 PLEATED BUTTON CLICKED!");
        SelectFoldStyle(1);
    }

    void OnCrescentButtonClicked()
    {
        Debug.Log("🎯 CRESCENT BUTTON CLICKED!");
        SelectFoldStyle(2);
    }

    void OnCrimpButtonClicked()
    {
        Debug.Log("🎯 CRIMP BUTTON CLICKED!");
        AttemptCrimp();
    }

    void OnEnable()
    {
        Debug.Log("=== DUMPLING CRIMPER ENABLED ===");
        ResetUIState();
        ShowUnfoldedDumpling();
    }

    void ResetUIState()
    {
        if (selectionMenu != null) 
        {
            selectionMenu.SetActive(true);
            Debug.Log("SelectionMenu: " + selectionMenu.activeInHierarchy);
        }

        if (gameArea != null) 
        {
            gameArea.SetActive(false);
            Debug.Log("GameArea: " + gameArea.activeInHierarchy);
        }
        
        isGameActive = false;
        isAnimating = false;
        selectedFoldStyle = -1;

        SetSelectionButtonsInteractable(true);
        if (crimpButton != null) crimpButton.interactable = false;
    }

    void SetSelectionButtonsInteractable(bool interactable)
    {
        if (simpleButton != null) simpleButton.interactable = interactable;
        if (pleatedButton != null) pleatedButton.interactable = interactable;
        if (crescentButton != null) crescentButton.interactable = interactable;
    }

    public void ReceiveDumplingFromFilling(DumplingType type)
    {
        Debug.Log("Received dumpling from filling: " + type);
        currentDumplingType = type;
        ShowUnfoldedDumpling();
    }

    private void ShowUnfoldedDumpling()
    {
        if (dumplingImage == null) 
        {
            Debug.LogError("DumplingImage is NULL!");
            return;
        }

        Sprite unfoldedSprite = GetUnfoldedSprite();
        if (unfoldedSprite != null)
        {
            dumplingImage.sprite = unfoldedSprite;
            dumplingImage.color = Color.white;
            Debug.Log("Unfolded sprite set: " + unfoldedSprite.name);
        }
    }

    private Sprite GetUnfoldedSprite()
    {
        bool isPork = currentDumplingType.ToString().Contains("Pork");
        return isPork ? 
            (porkUnfoldedSprite != null ? porkUnfoldedSprite : GetDefaultSprite()) :
            (veggieUnfoldedSprite != null ? veggieUnfoldedSprite : GetDefaultSprite());
    }

    private Sprite GetDefaultSprite()
    {
        if (simpleSequence != null && simpleSequence.Length > 0)
            return simpleSequence[0];
        return null;
    }

    public void SelectFoldStyle(int styleIndex)
    {
        Debug.Log("🎯 SELECT FOLD STYLE: " + styleIndex);
        
        if (selectionMenu == null || gameArea == null)
        {
            Debug.LogError("UI panels not assigned!");
            return;
        }

        selectionMenu.SetActive(false);
        gameArea.SetActive(true);
        
        Debug.Log("UI State - Selection: " + selectionMenu.activeInHierarchy + ", Game: " + gameArea.activeInHierarchy);
        
        selectedFoldStyle = styleIndex;
        StartMinigame(styleIndex);
    }

    private void StartMinigame(int styleIndex)
    {
        Debug.Log("🚀 Starting minigame: " + GetStyleName(styleIndex));
        
        currentClicks = 0;
        attemptsUsed = 0;
        isGameActive = true;
        isAnimating = false;
        
        if (crimpButton != null) 
        {
            crimpButton.interactable = true;
            Debug.Log("Crimp button enabled");
        }

        if (tickMark != null) tickMark.anchoredPosition = Vector2.zero;

        switch (styleIndex)
        {
            case 0: // Simple
                currentSequence = simpleSequence;
                currentSpeed = simpleSpeed;
                clicksNeeded = simpleSequence != null ? simpleSequence.Length - 1 : 0;
                maxAttempts = 5;
                break;
            case 1: // Pleated
                currentSequence = pleatedSequence;
                currentSpeed = pleatedSpeed;
                clicksNeeded = pleatedSequence != null ? pleatedSequence.Length - 1 : 0;
                maxAttempts = 6;
                break;
            case 2: // Crescent
                currentSequence = crescentSequence;
                currentSpeed = crescentSpeed;
                clicksNeeded = crescentSequence != null ? crescentSequence.Length - 1 : 0;
                maxAttempts = 7;
                break;
        }

        Debug.Log($"Minigame Config - Clicks: {clicksNeeded}, Speed: {currentSpeed}, Attempts: {maxAttempts}");
        Debug.Log($"Green Zone Width: {(greenZone != null ? greenZone.sizeDelta.x.ToString() : "NULL")}");

        if (currentSequence != null && currentSequence.Length > 0 && dumplingImage != null)
        {
            dumplingImage.sprite = currentSequence[0];
        }
    }

    string GetStyleName(int styleIndex)
    {
        switch (styleIndex)
        {
            case 0: return "Simple";
            case 1: return "Pleated";
            case 2: return "Crescent";
            default: return "Unknown";
        }
    }

    void Update()
    {
        // Minigame tick mark movement
        if (!isGameActive || isAnimating || tickMark == null || gaugeArea == null) return;

        float limit = (gaugeArea.rect.width / 2f) - (tickMark.rect.width / 2f);
        Vector2 pos = tickMark.anchoredPosition;

        if (movingRight)
        {
            pos.x += currentSpeed * Time.deltaTime;
            if (pos.x >= limit)
            {
                pos.x = limit;
                movingRight = false;
            }
        }
        else
        {
            pos.x -= currentSpeed * Time.deltaTime;
            if (pos.x <= -limit)
            {
                pos.x = -limit;
                movingRight = true;
            }
        }
        tickMark.anchoredPosition = pos;

        // Debug keyboard controls
        if (enableDebugControls)
        {
            HandleDebugInput();
        }
    }

    void HandleDebugInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            Debug.Log("🔄 DEBUG: Forcing Simple Fold");
            SelectFoldStyle(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            Debug.Log("🔄 DEBUG: Forcing Pleated Fold");
            SelectFoldStyle(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            Debug.Log("🔄 DEBUG: Forcing Crescent Fold");
            SelectFoldStyle(2);
        }
        if (Input.GetKeyDown(KeyCode.Space) && isGameActive)
        {
            Debug.Log("🔄 DEBUG: Forcing Crimp Attempt");
            AttemptCrimp();
        }
    }

    public void AttemptCrimp()
    {
        Debug.Log("🔨 AttemptCrimp called");
        
        if (!isGameActive) 
        {
            Debug.LogWarning("Minigame not active!");
            return;
        }
        if (isAnimating)
        {
            Debug.LogWarning("Already animating!");
            return;
        }
        if (tickMark == null || greenZone == null)
        {
            Debug.LogError("TickMark or GreenZone not assigned!");
            return;
        }

        attemptsUsed++;

        float tickX = tickMark.anchoredPosition.x;
        float zoneX = greenZone.anchoredPosition.x;
        float zoneHalfWidth = greenZone.rect.width / 2f;

        bool isPerfectHit = Mathf.Abs(tickX - zoneX) <= zoneHalfWidth;

        if (isPerfectHit)
        {
            Debug.Log($"✅ Perfect Hit! ({currentClicks + 1}/{clicksNeeded})");
            currentClicks++;
            currentSpeed += 15f; // Reduced speed increase
            
            // Start the crimp animation
            StartCoroutine(PlayCrimpAnimation());
        }
        else
        {
            Debug.Log($"❌ Miss! Attempt {attemptsUsed}/{maxAttempts}");
        }

        CheckGameCompletion();
    }

    private IEnumerator PlayCrimpAnimation()
    {
        Debug.Log("🎬 Starting crimp animation");
        isAnimating = true;
        
        if (crimpButton != null) 
        {
            crimpButton.interactable = false;
            Debug.Log("Crimp button disabled during animation");
        }

        // Scale up animation
        RectTransform dumplingRect = dumplingImage.GetComponent<RectTransform>();
        Vector3 originalScale = dumplingRect != null ? dumplingRect.localScale : Vector3.one;
        
        // Scale up
        float timer = 0f;
        while (timer < scaleAnimationDuration / 2f)
        {
            timer += Time.deltaTime;
            float t = timer / (scaleAnimationDuration / 2f);
            if (dumplingRect != null) 
            {
                dumplingRect.localScale = originalScale * (1f + t * 0.2f);
            }
            yield return null;
        }

        // Change to next sprite in sequence
        if (currentClicks < currentSequence.Length && dumplingImage != null)
        {
            dumplingImage.sprite = currentSequence[currentClicks];
            Debug.Log($"Sprite changed to frame {currentClicks}");
        }

        // Scale back down
        timer = 0f;
        while (timer < scaleAnimationDuration / 2f)
        {
            timer += Time.deltaTime;
            float t = timer / (scaleAnimationDuration / 2f);
            if (dumplingRect != null) 
            {
                dumplingRect.localScale = originalScale * (1.2f - t * 0.2f);
            }
            yield return null;
        }

        // Reset scale
        if (dumplingRect != null) 
        {
            dumplingRect.localScale = originalScale;
        }

        isAnimating = false;
        
        if (crimpButton != null) 
        {
            crimpButton.interactable = true;
            Debug.Log("Crimp button re-enabled after animation");
        }
        
        Debug.Log("🎬 Crimp animation completed");
    }

    private void CheckGameCompletion()
    {
        if (currentClicks >= clicksNeeded)
        {
            Debug.Log("🎉 Minigame completed!");
            FinishMinigame(true);
        }
        else if (attemptsUsed >= maxAttempts)
        {
            Debug.Log("💥 Minigame failed!");
            FinishMinigame(false);
        }
    }

    private void FinishMinigame(bool success)
    {
        isGameActive = false;
        if (crimpButton != null) crimpButton.interactable = false;

        if (success)
        {
            DetermineFinalDumplingType();
            if (dumplingImage != null) dumplingImage.sprite = GetCompletedSprite();
            Invoke("MoveToCooking", 1f);
        }
        else
        {
            if (dumplingImage != null) dumplingImage.color = Color.red;
        }
    }

    private void DetermineFinalDumplingType()
    {
        bool isPork = currentDumplingType.ToString().Contains("Pork");
        
        switch (selectedFoldStyle)
        {
            case 0: currentDumplingType = isPork ? DumplingType.Pork_Manapua : DumplingType.Veggie_Manapua; break;
            case 1: currentDumplingType = isPork ? DumplingType.Pork_Gyoza : DumplingType.Veggie_Gyoza; break;
            case 2: currentDumplingType = isPork ? DumplingType.Pork_Wonton : DumplingType.Veggie_Wonton; break;
        }
        
        Debug.Log("Final Dumpling Type: " + currentDumplingType);
    }

    private Sprite GetCompletedSprite()
    {
        switch (currentDumplingType)
        {
            case DumplingType.Pork_Gyoza:
            case DumplingType.Veggie_Gyoza: return gyozaSprite;
            case DumplingType.Pork_Manapua:
            case DumplingType.Veggie_Manapua: return manapuaSprite;
            case DumplingType.Pork_Wonton:
            case DumplingType.Veggie_Wonton: return wontonSprite;
            default: return GetUnfoldedSprite();
        }
    }

    private void MoveToCooking()
    {
        if (stationManager == null) return;
        var stations = stationManager.stations;
        if (stations == null || stations.Count < 4) return;

        GameObject cookingStation = stations[3].gameObject;
        CookingStationReceiver receiver = cookingStation.GetComponent<CookingStationReceiver>();
        if (receiver == null) receiver = cookingStation.AddComponent<CookingStationReceiver>();

        receiver.ReceiveDumplingFromCrimping(GetCompletedSprite(), currentDumplingType);
        stationManager.SwitchToStation(3);
    }

    [ContextMenu("Test Simple Fold")]
    public void TestSimpleFold()
    {
        Debug.Log("🧪 TEST: Simple Fold");
        SelectFoldStyle(0);
    }

    [ContextMenu("Test Pleated Fold")]
    public void TestPleatedFold()
    {
        Debug.Log("🧪 TEST: Pleated Fold");
        SelectFoldStyle(1);
    }

    [ContextMenu("Test Crescent Fold")]
    public void TestCrescentFold()
    {
        Debug.Log("🧪 TEST: Crescent Fold");
        SelectFoldStyle(2);
    }

    [ContextMenu("Set Super Slow Speeds")]
    public void SetSuperSlowSpeeds()
    {
        simpleSpeed = 30f;
        pleatedSpeed = 40f;
        crescentSpeed = 50f;
        Debug.Log("Set super slow speeds: 30, 40, 50");
    }

    [ContextMenu("Set Medium Speeds")]
    public void SetMediumSpeeds()
    {
        simpleSpeed = 60f;
        pleatedSpeed = 80f;
        crescentSpeed = 100f;
        Debug.Log("Set medium speeds: 60, 80, 100");
    }

    [ContextMenu("Set Fast Speeds")]
    public void SetFastSpeeds()
    {
        simpleSpeed = 100f;
        pleatedSpeed = 120f;
        crescentSpeed = 140f;
        Debug.Log("Set fast speeds: 100, 120, 140");
    }
}