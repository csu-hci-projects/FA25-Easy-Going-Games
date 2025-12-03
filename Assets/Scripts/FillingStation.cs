using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Collections;

public class FillingStation : MonoBehaviour
{
    [Header("Drag these in from Hierarchy")]
    public Image visualWrapper;
    public Image visualFilling;
    public Button doneButton;
    
    [Header("Scoring")]
    public int perfectFillingScore = 50;
    public int goodFillingScore = 30;
    public int baseFillingScore = 20;
    
    private bool hasWrapper = false;
    private bool hasFilling = false;
    private string currentFillingType = "";
    private StationManager sm;
    
    // Track current dumpling for order system
    private DumplingType currentDumplingType;
    private string currentOrderID;
    private string currentDumplingID;

    void Start()
    {
        sm = GetComponentInParent<StationManager>();
        visualWrapper.enabled = false;
        visualFilling.enabled = false;
        doneButton.interactable = true;
        
        // Set up button listener
        doneButton.onClick.AddListener(SendToCrimping);
    }

    void OnEnable()
    {
        // Reset station when entering
        ResetStation();
    }

    public void ReceiveIngredient(string name)
    {
        if (name == "Draggable_Wrapper")
        {
            hasWrapper = true;
            visualWrapper.enabled = true;
            Debug.Log("Wrapper added");
        }
        else if (hasWrapper && (name == "Draggable_Pork" || name == "Draggable_Cabbage" || name == "Draggable_Ginger"))
        {
            hasFilling = true;
            currentFillingType = name.Replace("Draggable_", "").ToLower();
            visualFilling.enabled = true;
            doneButton.interactable = true;
            Debug.Log($"Filling added: {currentFillingType}");
        }
        else
        {
            Debug.Log("Need Wrapper First!");
        }
    }

    // Call this when starting a dumpling from an order
    public void StartDumplingFromOrder(DumplingType dumplingType, string orderID)
    {
        currentDumplingType = dumplingType;
        currentOrderID = orderID;
        
        // Get the order manager and start tracking this dumpling
        OrderQueueManager orderManager = FindAnyObjectByType<OrderQueueManager>();
        DumplingProgressTracker progressTracker = FindAnyObjectByType<DumplingProgressTracker>();
        
        if (orderManager != null && progressTracker != null)
        {
            currentDumplingID = progressTracker.StartNewDumpling(dumplingType, orderID);
            orderManager.StartDumplingForOrder(dumplingType);
        }
        
        ResetStation();
        Debug.Log($"Starting {dumplingType} for order {orderID}");
    }

    public void SendToCrimping()
    {
        if (!hasWrapper || !hasFilling)
        {
            Debug.Log("Need both wrapper and filling!");
            return;
        }

        // Calculate filling quality score
        float fillingQuality = CalculateFillingQuality();
        
        // Update progress tracker
        DumplingProgressTracker progressTracker = FindAnyObjectByType<DumplingProgressTracker>();
        if (progressTracker != null && !string.IsNullOrEmpty(currentDumplingID))
        {
            // Convert string filling type to proper ingredient tracking
            bool porkUsed = currentFillingType == "pork";
            bool cabbageUsed = currentFillingType == "cabbage"; 
            bool gingerUsed = currentFillingType == "ginger";
            
            progressTracker.CompleteFilling(currentDumplingID, true, cabbageUsed, porkUsed, gingerUsed, fillingQuality);
        }

        // Add immediate score for filling
        OrderQueueManager orderManager = FindAnyObjectByType<OrderQueueManager>();
        if (orderManager != null)
        {
            int fillingScore = Mathf.RoundToInt(baseFillingScore * fillingQuality);
            orderManager.AddScore(fillingScore);
            Debug.Log($"Filling completed! Quality: {fillingQuality:F2} Score: +{fillingScore}");
        }

        // NEW: Send dumpling type to crimping station
        SendDumplingTypeToCrimping();
        
        // Switch to crimping station
        sm.SwitchToStation(2);
    }

    // NEW: Determine and send dumpling type to crimping station
    private void SendDumplingTypeToCrimping()
    {
        // Find the crimping station
        DumplingCrimper crimper = FindObjectOfType<DumplingCrimper>();
        if (crimper != null)
        {
            // Determine which unfolded sprite to show based on pork content
            // Send a temporary dumpling type just for the unfolded sprite
            // The actual dumpling type will be determined by crimp selection
            DumplingCrimper.DumplingType tempType = (currentFillingType == "pork") ? 
                DumplingCrimper.DumplingType.Pork_Manapua : // Use any pork type for unfolded sprite
                DumplingCrimper.DumplingType.Veggie_Manapua; // Use any veggie type for unfolded sprite
            
            // Send to crimping station
            crimper.ReceiveDumplingFromFilling(tempType);
            
            Debug.Log($"Sent temporary {tempType} to show {(currentFillingType == "pork" ? "pork" : "veggie")} unfolded sprite");
        }
        else
        {
            Debug.LogWarning("DumplingCrimper not found in scene!");
        }
    }

    float CalculateFillingQuality()
    {
        // Get the recipe for current dumpling
        DumplingRecipeDatabase recipeDB = FindAnyObjectByType<DumplingRecipeDatabase>();
        if (recipeDB == null) return 0.5f;
        
        DumplingRecipe recipe = recipeDB.GetRecipe(currentDumplingType);
        if (recipe == null) return 0.5f;

        float quality = 1.0f; // Start perfect
        
        // Check if correct filling was used
        if (recipe.requiresPork && currentFillingType != "pork") quality -= 0.4f;
        if (recipe.requiresCabbage && currentFillingType != "cabbage") quality -= 0.4f;
        if (recipe.requiresGinger && currentFillingType != "ginger") quality -= 0.4f;
        
        // Check for wrong ingredients (smaller penalty)
        if (!recipe.requiresPork && currentFillingType == "pork") quality -= 0.2f;
        if (!recipe.requiresCabbage && currentFillingType == "cabbage") quality -= 0.2f;
        if (!recipe.requiresGinger && currentFillingType == "ginger") quality -= 0.2f;

        return Mathf.Clamp(quality, 0.1f, 1.0f);
    }

    void ResetStation()
    {
        hasWrapper = false;
        hasFilling = false;
        currentFillingType = "";
        visualWrapper.enabled = false;
        visualFilling.enabled = false;
        doneButton.interactable = false;
        
        // Reset colors if they were modified by animation
        if (visualWrapper != null)
        {
            Color wrapperColor = visualWrapper.color;
            wrapperColor.a = 1f;
            visualWrapper.color = wrapperColor;
        }
        
        if (visualFilling != null)
        {
            Color fillingColor = visualFilling.color;
            fillingColor.a = 1f;
            visualFilling.color = fillingColor;
        }
    }

    // Call this when entering filling station with an active order
    public void OnEnterWithOrder(Order order)
    {
        StartDumplingFromOrder(order.dumplingType, order.orderID);
    }

    // NEW: Optional visual feedback when sending to crimping
    private System.Collections.IEnumerator AnimateTransitionToCrimping()
    {
        // Optional: Add a simple fade out animation
        if (visualWrapper != null && visualFilling != null)
        {
            float duration = 0.3f;
            float timer = 0f;
            
            Color wrapperColor = visualWrapper.color;
            Color fillingColor = visualFilling.color;
            
            while (timer < duration)
            {
                timer += Time.deltaTime;
                float alpha = 1f - (timer / duration);
                
                wrapperColor.a = alpha;
                fillingColor.a = alpha;
                
                visualWrapper.color = wrapperColor;
                visualFilling.color = fillingColor;
                
                yield return null;
            }
        }
        
        // Reset visuals after animation
        ResetStation();
    }
    
    // Optional: Call this if you want to use the animation
    public void SendToCrimpingWithAnimation()
    {
        if (!hasWrapper || !hasFilling)
        {
            Debug.Log("Need both wrapper and filling!");
            return;
        }

        // Start animation and then send to crimping
        StartCoroutine(AnimateAndSendToCrimping());
    }
    
    private System.Collections.IEnumerator AnimateAndSendToCrimping()
    {
        // Play animation
        yield return StartCoroutine(AnimateTransitionToCrimping());
        
        // Then do all the normal sending logic
        float fillingQuality = CalculateFillingQuality();
        
        // Update progress tracker
        DumplingProgressTracker progressTracker = FindAnyObjectByType<DumplingProgressTracker>();
        if (progressTracker != null && !string.IsNullOrEmpty(currentDumplingID))
        {
            bool porkUsed = currentFillingType == "pork";
            bool cabbageUsed = currentFillingType == "cabbage"; 
            bool gingerUsed = currentFillingType == "ginger";
            
            progressTracker.CompleteFilling(currentDumplingID, true, cabbageUsed, porkUsed, gingerUsed, fillingQuality);
        }

        // Add score
        OrderQueueManager orderManager = FindAnyObjectByType<OrderQueueManager>();
        if (orderManager != null)
        {
            int fillingScore = Mathf.RoundToInt(baseFillingScore * fillingQuality);
            orderManager.AddScore(fillingScore);
        }

        // Send to crimping
        SendDumplingTypeToCrimping();
        sm.SwitchToStation(2);
    }
}