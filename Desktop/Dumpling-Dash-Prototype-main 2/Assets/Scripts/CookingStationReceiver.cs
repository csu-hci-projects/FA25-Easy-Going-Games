using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CookingStationReceiver : MonoBehaviour
{
    [Header("Cooking Station UI")]
    public Image dumplingImage; // Drag your cooking station dumpling image here
    
    [Header("Visual Effects")]
    public ParticleSystem cookingParticles;
    public ParticleSystem steamParticles;
    
    [Header("Cooking Settings")]
    public float porkCookTime = 8f;
    public float veggieCookTime = 6f;
    
    private DumplingCrimper.DumplingType currentDumplingType;
    private bool isCooking = false;
    
    // Main method called from crimping station
    public void ReceiveDumplingFromCrimping(Sprite completedDumplingSprite, DumplingCrimper.DumplingType type)
    {
        Debug.Log($"🍳 Cooking Station: Received {type} dumpling!");
        
        currentDumplingType = type;
        
        // Set up the dumpling for cooking
        if (dumplingImage != null)
        {
            dumplingImage.sprite = completedDumplingSprite;
            dumplingImage.color = Color.white;
            dumplingImage.preserveAspect = true;
        }
        
        // Start cooking process
        StartCooking();
    }
    
    private void StartCooking()
    {
        if (isCooking) return;
        
        isCooking = true;
        
        // Play cooking effects
        if (cookingParticles != null)
            cookingParticles.Play();
            
        if (steamParticles != null)  
            steamParticles.Play();
        
        // Determine cook time based on dumpling type (pork vs veggie)
        float cookTime = IsPorkDumpling(currentDumplingType) ? porkCookTime : veggieCookTime;
        
        Debug.Log($"Starting to cook {currentDumplingType} dumpling for {cookTime} seconds");
        
        // Start cooking coroutine
        StartCoroutine(CookingProcess(cookTime));
    }
    
    // Helper method to check if dumpling is pork-based
    private bool IsPorkDumpling(DumplingCrimper.DumplingType type)
    {
        switch (type)
        {
            case DumplingCrimper.DumplingType.Pork_Gyoza:
            case DumplingCrimper.DumplingType.Pork_Manapua:
            case DumplingCrimper.DumplingType.Pork_Wonton:
                return true;
            case DumplingCrimper.DumplingType.Veggie_Gyoza:
            case DumplingCrimper.DumplingType.Veggie_Manapua:
            case DumplingCrimper.DumplingType.Veggie_Wonton:
            default:
                return false;
        }
    }
    
    // Optional: Get specific cook time adjustments based on dumpling type
    private float GetAdjustedCookTime(DumplingCrimper.DumplingType type)
    {
        float baseTime = IsPorkDumpling(type) ? porkCookTime : veggieCookTime;
        
        // Optional: Adjust cook time based on specific dumpling type
        switch (type)
        {
            case DumplingCrimper.DumplingType.Pork_Manapua:
            case DumplingCrimper.DumplingType.Veggie_Manapua:
                return baseTime + 1f; // Manapua might need slightly longer
            case DumplingCrimper.DumplingType.Pork_Wonton:
            case DumplingCrimper.DumplingType.Veggie_Wonton:
                return baseTime - 1f; // Wontons might cook faster
            default:
                return baseTime;
        }
    }
    
    private IEnumerator CookingProcess(float cookTime)
    {
        float timer = 0f;
        
        while (timer < cookTime)
        {
            timer += Time.deltaTime;
            
            // Optional: Visual feedback during cooking
            // You could change color, add progress bar, etc.
            
            yield return null;
        }
        
        // Cooking complete!
        FinishCooking();
    }
    
    private void FinishCooking()
    {
        isCooking = false;
        Debug.Log($"✅ {currentDumplingType} dumpling cooked perfectly!");
        
        // Optional: Change appearance to "cooked" version
        // You could swap sprites here if you have cooked vs uncooked versions
        
        // Stop cooking particles, keep steam for serving
        if (cookingParticles != null)
            cookingParticles.Stop();
    }
    
    // Call this when serving the dumpling
    public void ServeDumpling()
    {
        if (steamParticles != null)
            steamParticles.Stop();
            
        ClearCookingStation();
    }
    
    public void ClearCookingStation()
    {
        if (dumplingImage != null)
        {
            dumplingImage.sprite = null;
            dumplingImage.color = new Color(1, 1, 1, 0);
        }
        
        if (cookingParticles != null)
            cookingParticles.Stop();
            
        if (steamParticles != null)
            steamParticles.Stop();
            
        isCooking = false;
    }
    
    void OnEnable()
    {
        ClearCookingStation();
    }
    
    void OnDisable()
    {
        StopAllCoroutines();
    }
    
    // Optional: Public method to check current dumpling type
    public DumplingCrimper.DumplingType GetCurrentDumplingType()
    {
        return currentDumplingType;
    }
    
    // Optional: Check if station is cooking
    public bool IsCurrentlyCooking()
    {
        return isCooking;
    }
}