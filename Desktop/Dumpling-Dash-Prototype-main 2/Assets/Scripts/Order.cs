using UnityEngine;

[System.Serializable]
public class Order
{
    public string orderID;
    public DumplingType dumplingType;
    public int quantity;
    public float timeLimit;
    public float timeRemaining;
    public bool isCompleted;
    public bool isFailed;
    
    // Track dumplings in progress
    public int dumplingsInProgress;
    public int dumplingsCompleted;
    
    public Order(DumplingType type, int qty, float limit)
    {
        orderID = System.Guid.NewGuid().ToString().Substring(0, 8);
        dumplingType = type;
        quantity = qty;
        timeLimit = limit;
        timeRemaining = limit;
        isCompleted = false;
        isFailed = false;
        dumplingsInProgress = 0;
        dumplingsCompleted = 0;
    }
    
    public string GetDisplayName()
    {
        switch (dumplingType)
        {
            case DumplingType.PorkGyoza: return "Pork Gyoza";
            case DumplingType.VeggieGyoza: return "Veggie Gyoza";
            case DumplingType.PorkManapua: return "Pork Manapua";
            case DumplingType.VeggieManapua: return "Veggie Manapua";
            case DumplingType.PorkWonton: return "Pork Wonton";
            case DumplingType.VeggieWonton: return "Veggie Wonton";
            default: return "Dumpling";
        }
    }
}