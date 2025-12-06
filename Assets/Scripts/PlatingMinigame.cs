using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class PlatingMinigame : MonoBehaviour
{
    [Header("Testing")]
    [Tooltip("Check this to automatically start the minigame when you press Play")]
    public bool testMode = false;

    [Header("UI References")]
    public GameObject minigameUI;    // The container (Minigame_UI)
    public RectTransform plateArea;  // The Plate Image
    public Transform foodTray;       // The FoodTray Panel
    public Button serveButton;       // The Serve Button

    [Header("Prefabs")]
    [Tooltip("You MUST drag a prefab with an Image component here")]
    public GameObject dumplingPrefab;
    public GameObject garnishPrefab; 

    [Header("Effects")]
    public ParticleSystem confettiParticles; 
    public ParticleSystem starParticles;

    [Header("Game Settings")]
    public int dumplingsCount = 3;
    
    [HideInInspector] public bool isServed = false;
    private List<PlatingItem> spawnedItems = new List<PlatingItem>();

    void Start()
    {
        // Hook up the serve button logic
        if (serveButton)
        {
            serveButton.onClick.RemoveAllListeners(); // Prevent double clicks
            serveButton.onClick.AddListener(OnServeClicked);
            serveButton.interactable = false;
        }

        // --- CHANGE HERE: Auto-Start Logic ---
        if (testMode)
        {
            Debug.Log("Test Mode ON: Starting Plating immediately.");
            StartPlating();
        }
        else
        {
            // Normal game: Hide it until told otherwise
            if(minigameUI) minigameUI.SetActive(false);
        }
    }

    // Right-click the component in Inspector to trigger this manually!
    [ContextMenu("Start Plating Minigame")]
    public void StartPlating()
    {
        Debug.Log("Starting Plating Minigame...");
        
        if (minigameUI) minigameUI.SetActive(true);
        
        isServed = false;
        
        if (serveButton) serveButton.interactable = false;

        SpawnFood();
    }

    void SpawnFood()
    {
        // 1. Safety Check: Is the prefab assigned?
        if (dumplingPrefab == null)
        {
            Debug.LogError("ERROR: 'Dumpling Prefab' slot is empty! Please assign it in the Inspector.");
            return;
        }

        // 2. Clear old food
        foreach (var item in spawnedItems)
        {
            if(item != null) Destroy(item.gameObject);
        }
        spawnedItems.Clear();

        // --- NEW: Force Tray Alignment ---
        // This ensures the tray itself tries to center its children
        LayoutGroup trayLayout = foodTray.GetComponent<LayoutGroup>();
        if (trayLayout != null)
        {
            trayLayout.childAlignment = TextAnchor.MiddleCenter;
        }

        // 3. Spawn Dumplings
        for (int i = 0; i < dumplingsCount; i++)
        {
            GameObject d = Instantiate(dumplingPrefab, foodTray);
            
            // --- FIX START: Handle Missing RectTransform (Non-UI Prefab) ---
            RectTransform rt = d.GetComponent<RectTransform>();
            
            if (rt == null)
            {
                Debug.LogWarning("Prefab is a Sprite, not a UI Image. Auto-fixing...");
                Sprite sprite = null;
                var sr = d.GetComponent<SpriteRenderer>();
                if (sr != null) sprite = sr.sprite;
                Destroy(d);
                d = new GameObject("Fixed_Dumpling", typeof(RectTransform), typeof(Image));
                d.transform.SetParent(foodTray);
                if (sprite != null) d.GetComponent<Image>().sprite = sprite;
                rt = d.GetComponent<RectTransform>();
            }
            // --- FIX END ---

            // Fix Scale (Unity UI bug fix)
            d.transform.localScale = Vector3.one;
            d.transform.localPosition = Vector3.zero;
            d.transform.localRotation = Quaternion.identity;

            // --- CHANGED: FORCE PIVOT & SIZE ---
            // Force Pivot to Center (0.5, 0.5) so images spawn centered
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            
            // Reduced size to prevent "Giant Dumpling"
            rt.sizeDelta = new Vector2(80, 80);
            
            // Ensure aspect ratio stays correct (Important for new art!)
            Image img = d.GetComponent<Image>();
            if (img != null) img.preserveAspect = true;
            
            // --- NEW: Force Layout Element ---
            LayoutElement le = d.GetComponent<LayoutElement>();
            if (le == null) le = d.AddComponent<LayoutElement>();
            le.minWidth = 50;
            le.minHeight = 50;
            le.preferredWidth = 80;
            le.preferredHeight = 80;

            SetupItem(d);
        }

        // 4. Spawn Garnish
        if (garnishPrefab != null)
        {
            GameObject g = Instantiate(garnishPrefab, foodTray);
            
            // Fix missing UI component if needed
            if (g.GetComponent<RectTransform>() == null)
            {
                Sprite sprite = null;
                var sr = g.GetComponent<SpriteRenderer>();
                if (sr != null) sprite = sr.sprite;
                Destroy(g);
                g = new GameObject("Fixed_Garnish", typeof(RectTransform), typeof(Image));
                g.transform.SetParent(foodTray);
                if (sprite != null) g.GetComponent<Image>().sprite = sprite;
            }

            g.transform.localScale = Vector3.one;
            g.transform.localPosition = Vector3.zero;
            g.transform.localRotation = Quaternion.identity;
            
            // --- CHANGED: FORCE PIVOT & PRESERVE ASPECT ---
            RectTransform grt = g.GetComponent<RectTransform>();
            if (grt != null)
            {
                grt.pivot = new Vector2(0.5f, 0.5f);
                grt.anchorMin = new Vector2(0.5f, 0.5f);
                grt.anchorMax = new Vector2(0.5f, 0.5f);
                grt.sizeDelta = new Vector2(50, 50);
            }
            
            // Ensure aspect ratio is preserved for garnish too
            Image gImg = g.GetComponent<Image>();
            if (gImg != null) gImg.preserveAspect = true;
            
            SetupItem(g);
        }

        // --- NEW: Force UI Refresh ---
        if (foodTray.GetComponent<RectTransform>())
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(foodTray.GetComponent<RectTransform>());
        }
    }

    void SetupItem(GameObject obj)
    {
        PlatingItem itemScript = obj.GetComponent<PlatingItem>();
        // If the prefab doesn't have the script, add it automatically
        if (itemScript == null) itemScript = obj.AddComponent<PlatingItem>();

        itemScript.minigameManager = this;
        itemScript.plateArea = plateArea;
        itemScript.parentAfterDrag = plateArea.transform; 

        spawnedItems.Add(itemScript);
    }

    public void CheckPlatingStatus()
    {
        int platedCount = 0;
        foreach(var item in spawnedItems)
        {
            if (item != null && item.isPlated) platedCount++;
        }

        if (serveButton)
        {
            // Only clickable if we moved enough items to the plate
            serveButton.interactable = (platedCount >= dumplingsCount);
        }
    }

    public void OnServeClicked()
    {
        if (isServed) return;
        isServed = true;

        if (confettiParticles) confettiParticles.Play();
        if (starParticles) starParticles.Play();

        Debug.Log("Order Served!");
        
        // --- CHANGED: Hide immediately (removed delay) ---
        FinishLevel();
    }

    void FinishLevel()
    {
        if (minigameUI) minigameUI.SetActive(false);
    }
}