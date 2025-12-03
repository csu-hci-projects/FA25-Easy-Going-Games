using UnityEngine;

public enum DumplingFamily
{
    Gyoza,
    Manapua, 
    Wonton
}

public enum FillingType
{
    Pork,
    Veggie
}

[System.Serializable]
public class DumplingRecipe
{
    public DumplingType type;
    public DumplingFamily family;
    public FillingType filling;
    public string displayName;
    public CrimpingStyle requiredCrimping;
    public Sprite icon;
    
    // Required ingredients
    public bool requiresCabbage = true;
    public bool requiresPork = false;
    public bool requiresGinger = true;
}

public enum DumplingType
{
    PorkGyoza,
    VeggieGyoza,
    PorkManapua,
    VeggieManapua,
    PorkWonton,
    VeggieWonton
}

public enum CrimpingStyle
{
    Simple,      // For Manapua
    Pleated,     // For Gyoza  
    Crescent     // For Wonton
}

public enum IngredientType
{
    Wrapper,  // Base - always required
    Cabbage,
    Pork,
    Ginger
}