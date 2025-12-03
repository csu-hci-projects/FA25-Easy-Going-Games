using UnityEngine;
using System.Collections.Generic;

public class PrepAreaDropZone : MonoBehaviour
{
    [Header("Station References")]
    public FillingStation fillingStation;
    public DumplingCrimper dumplingCrimper;

    [Header("Visual Settings")]
    public float ingredientSpacing = 30f;
    public float ingredientScale = 0.8f;

    private List<Transform> currentIngredients = new List<Transform>();

    public void ReceiveIngredients(List<Transform> ingredients)
    {
        Debug.Log($"PrepArea received {ingredients.Count} ingredients");

        // Clear existing ingredients
        ClearIngredients();

        // Add new ingredients
        foreach (Transform ingredient in ingredients)
        {
            if (ingredient != null)
            {
                ingredient.SetParent(transform);
                currentIngredients.Add(ingredient);

                // Disable dragging
                DraggableItem draggable = ingredient.GetComponent<DraggableItem>();
                if (draggable != null)
                {
                    draggable.enabled = false;
                }

                ingredient.gameObject.SetActive(true);
            }
        }

        // Arrange ingredients visually
        ArrangeIngredients();

        // Send to appropriate station based on ingredients
        ProcessIngredients(ingredients);
    }

    private void ProcessIngredients(List<Transform> ingredients)
    {
        if (ingredients.Count == 0) return;

        // Check what type of ingredients we have
        bool hasWrapper = false;
        bool hasFilling = false;

        foreach (Transform ingredient in ingredients)
        {
            string ingredientName = ingredient.name.ToLower();
            
            if (ingredientName.Contains("wrapper"))
            {
                hasWrapper = true;
            }
            else if (ingredientName.Contains("pork") || ingredientName.Contains("cabbage") || ingredientName.Contains("ginger"))
            {
                hasFilling = true;
            }
        }

        // If we have both wrapper and filling, send to crimping station
        if (hasWrapper && hasFilling)
        {
            SendToCrimping(ingredients);
        }
        // If we only have wrapper, send to filling station
        else if (hasWrapper)
        {
            SendToFilling();
        }
    }

    private void SendToCrimping(List<Transform> ingredients)
    {
        Debug.Log("Sending ingredients to crimping station");

        if (dumplingCrimper != null)
        {
            // Determine which unfolded sprite to show based on pork content
            bool containsPork = ContainsPork(ingredients);
            
            // Send a temporary dumpling type just for the unfolded sprite
            // The actual dumpling type will be determined by crimp selection
            DumplingCrimper.DumplingType tempType = containsPork ? 
                DumplingCrimper.DumplingType.Pork_Manapua : // Use any pork type for unfolded sprite
                DumplingCrimper.DumplingType.Veggie_Manapua; // Use any veggie type for unfolded sprite
            
            dumplingCrimper.ReceiveDumplingFromFilling(tempType);
            Debug.Log($"Sending temporary {tempType} to show {(containsPork ? "pork" : "veggie")} unfolded sprite");
        }
        else
        {
            Debug.LogWarning("DumplingCrimper reference not set!");
        }

        // Clear ingredients after sending
        ClearIngredients();
    }

    private void SendToFilling()
    {
        Debug.Log("Sending wrapper to filling station");

        if (fillingStation != null)
        {
            // You might need to call a method on filling station to receive the wrapper
            // fillingStation.ReceiveWrapper();
        }
        else
        {
            Debug.LogWarning("FillingStation reference not set!");
        }
    }

    private bool ContainsPork(List<Transform> ingredients)
    {
        foreach (Transform ingredient in ingredients)
        {
            string ingredientName = ingredient.name.ToLower();
            if (ingredientName.Contains("pork"))
            {
                return true;
            }
        }
        return false;
    }

    private void ArrangeIngredients()
    {
        if (currentIngredients.Count == 0) return;

        float totalWidth = (currentIngredients.Count - 1) * ingredientSpacing;
        float startX = -totalWidth / 2f;

        for (int i = 0; i < currentIngredients.Count; i++)
        {
            if (currentIngredients[i] != null)
            {
                RectTransform rt = currentIngredients[i].GetComponent<RectTransform>();
                if (rt != null)
                {
                    rt.anchoredPosition = new Vector2(startX + (i * ingredientSpacing), 0);
                    rt.localScale = Vector3.one * ingredientScale;
                    rt.localRotation = Quaternion.identity;
                    currentIngredients[i].SetSiblingIndex(i);
                }
            }
        }
    }

    private void ClearIngredients()
    {
        foreach (Transform ingredient in currentIngredients)
        {
            if (ingredient != null)
            {
                Destroy(ingredient.gameObject);
            }
        }
        currentIngredients.Clear();
    }

    void OnEnable()
    {
        ClearIngredients();
    }

    void OnDisable()
    {
        ClearIngredients();
    }
}