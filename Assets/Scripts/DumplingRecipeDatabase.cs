using System.Collections.Generic;
using UnityEngine;

public class DumplingRecipeDatabase : MonoBehaviour
{
    public static DumplingRecipeDatabase Instance;
    
    public List<DumplingRecipe> allRecipes = new List<DumplingRecipe>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeRecipes();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void InitializeRecipes()
    {
        allRecipes = new List<DumplingRecipe>
        {
            // Gyoza Recipes
            new DumplingRecipe 
            { 
                type = DumplingType.PorkGyoza,
                family = DumplingFamily.Gyoza,
                filling = FillingType.Pork,
                displayName = "Pork Gyoza",
                requiredCrimping = CrimpingStyle.Pleated,
                requiresCabbage = true,
                requiresPork = true,
                requiresGinger = true
            },
            new DumplingRecipe 
            { 
                type = DumplingType.VeggieGyoza,
                family = DumplingFamily.Gyoza,
                filling = FillingType.Veggie,
                displayName = "Veggie Gyoza",
                requiredCrimping = CrimpingStyle.Pleated,
                requiresCabbage = true,
                requiresPork = false,
                requiresGinger = true
            },
            
            // Manapua Recipes
            new DumplingRecipe 
            { 
                type = DumplingType.PorkManapua,
                family = DumplingFamily.Manapua,
                filling = FillingType.Pork,
                displayName = "Pork Manapua",
                requiredCrimping = CrimpingStyle.Simple,
                requiresCabbage = false,
                requiresPork = true,
                requiresGinger = false
            },
            new DumplingRecipe 
            { 
                type = DumplingType.VeggieManapua,
                family = DumplingFamily.Manapua,
                filling = FillingType.Veggie,
                displayName = "Veggie Manapua",
                requiredCrimping = CrimpingStyle.Simple,
                requiresCabbage = false,
                requiresPork = false,
                requiresGinger = false
            },
            
            // Wonton Recipes
            new DumplingRecipe 
            { 
                type = DumplingType.PorkWonton,
                family = DumplingFamily.Wonton,
                filling = FillingType.Pork,
                displayName = "Pork Wonton",
                requiredCrimping = CrimpingStyle.Crescent,
                requiresCabbage = true,
                requiresPork = true,
                requiresGinger = true
            },
            new DumplingRecipe 
            { 
                type = DumplingType.VeggieWonton,
                family = DumplingFamily.Wonton,
                filling = FillingType.Veggie,
                displayName = "Veggie Wonton",
                requiredCrimping = CrimpingStyle.Crescent,
                requiresCabbage = true,
                requiresPork = false,
                requiresGinger = true
            }
        };
    }
    
    public DumplingRecipe GetRecipe(DumplingType type)
    {
        return allRecipes.Find(recipe => recipe.type == type);
    }
    
    public List<DumplingRecipe> GetRecipesByFamily(DumplingFamily family)
    {
        return allRecipes.FindAll(recipe => recipe.family == family);
    }
}