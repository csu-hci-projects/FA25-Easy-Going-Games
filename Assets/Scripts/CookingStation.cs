using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CookingStation : MonoBehaviour, IDropHandler
{
    public List<string> currentIngredients = new List<string>();
    public bool wrapperPlaced = false;

    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag;
        if (dragged == null) return;

        var ingredient = dragged.GetComponent<IngredientDrag>();
        if (ingredient != null)
        {
            // Enforce wrapper first
            if (dragged.name == "Wrapper")
            {
                wrapperPlaced = true;
                currentIngredients.Add(dragged.name);
                Debug.Log("Wrapper placed");
            }
            else
            {
                if (!wrapperPlaced)
                {
                    Debug.Log("You must place the wrapper first!");
                    return; // don't allow other ingredients yet
                }
                currentIngredients.Add(dragged.name);
                Debug.Log("Added ingredient: " + dragged.name);
            }
        }
    }

    public bool ReadyToCrimp()
    {
        return wrapperPlaced && currentIngredients.Count > 1; // wrapper + at least one ingredient
    }
}
