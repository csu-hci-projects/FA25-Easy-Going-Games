using System.Collections.Generic;
using UnityEngine;

public class DumplingProgressTracker : MonoBehaviour
{
    [System.Serializable]
    public class ActiveDumpling
    {
        public string dumplingID;
        public DumplingType type;
        public string orderID;
        
        // Station progress
        public bool fillingCompleted = false;
        public bool crimpingCompleted = false;
        public bool cookingCompleted = false;
        public bool platingCompleted = false;
        
        // Quality scores (0-1 scale)
        public float fillingQuality = 0f;
        public float crimpingQuality = 0f;
        public float cookingQuality = 0f;
        public float overallQuality = 0f;
        
        // Ingredients used
        public bool wrapperUsed = false;
        public bool cabbageUsed = false;
        public bool porkUsed = false;
        public bool gingerUsed = false;
        
        // Crimping style used
        public CrimpingStyle crimpingStyleUsed;
    }
    
    private List<ActiveDumpling> activeDumplings = new List<ActiveDumpling>();
    
    public string StartNewDumpling(DumplingType type, string orderID)
    {
        ActiveDumpling newDumpling = new ActiveDumpling
        {
            dumplingID = System.Guid.NewGuid().ToString().Substring(0, 8),
            type = type,
            orderID = orderID
        };
        
        activeDumplings.Add(newDumpling);
        Debug.Log($"Started new dumpling: {newDumpling.dumplingID} for order {orderID}");
        return newDumpling.dumplingID;
    }
    
    public ActiveDumpling GetDumpling(string dumplingID)
    {
        return activeDumplings.Find(d => d.dumplingID == dumplingID);
    }
    
    public void CompleteFilling(string dumplingID, bool wrapper, bool cabbage, bool pork, bool ginger, float quality)
    {
        ActiveDumpling dumpling = GetDumpling(dumplingID);
        if (dumpling != null)
        {
            dumpling.fillingCompleted = true;
            dumpling.wrapperUsed = wrapper;
            dumpling.cabbageUsed = cabbage;
            dumpling.porkUsed = pork;
            dumpling.gingerUsed = ginger;
            dumpling.fillingQuality = quality;
        }
    }
    
    public void CompleteCrimping(string dumplingID, CrimpingStyle style, float quality)
    {
        ActiveDumpling dumpling = GetDumpling(dumplingID);
        if (dumpling != null)
        {
            dumpling.crimpingCompleted = true;
            dumpling.crimpingStyleUsed = style;
            dumpling.crimpingQuality = quality;
        }
    }
    
    public void CompleteCooking(string dumplingID, float quality)
    {
        ActiveDumpling dumpling = GetDumpling(dumplingID);
        if (dumpling != null)
        {
            dumpling.cookingCompleted = true;
            dumpling.cookingQuality = quality;
        }
    }
    
    public void CompletePlating(string dumplingID)
    {
        ActiveDumpling dumpling = GetDumpling(dumplingID);
        if (dumpling != null)
        {
            dumpling.platingCompleted = true;
            
            // Calculate overall quality
            dumpling.overallQuality = (dumpling.fillingQuality + dumpling.crimpingQuality + dumpling.cookingQuality) / 3f;
            
            // Remove from active list (it's now completed)
            activeDumplings.Remove(dumpling);
        }
    }
    
    public bool IsDumplingCompleted(string dumplingID)
    {
        ActiveDumpling dumpling = GetDumpling(dumplingID);
        return dumpling != null && dumpling.platingCompleted;
    }
    
    public List<ActiveDumpling> GetActiveDumplings()
    {
        return activeDumplings;
    }
}