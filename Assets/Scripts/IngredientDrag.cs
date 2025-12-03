using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI; 

[RequireComponent(typeof(CanvasGroup))]
public class IngredientDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector2 originalPos;
    private CanvasGroup cg;
    private Transform trayTransform; // We will store the specific tray here

    void Awake() 
    { 
        cg = GetComponent<CanvasGroup>(); 
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 1. Save the position
        originalPos = ((RectTransform)transform).anchoredPosition;
        
        // 2. Find the Tray explicitly by name so we never lose it
        // Make sure your panel is named EXACTLY "Ingredient_Tray" in the Hierarchy
        GameObject trayObj = GameObject.Find("Ingredient_Tray");
        if (trayObj != null)
        {
            trayTransform = trayObj.transform;
        }
        else
        {
            Debug.LogError("❌ CRITICAL ERROR: Could not find 'Ingredient_Tray' in Hierarchy!");
            // Fallback: use current parent
            trayTransform = transform.parent;
        }

        // 3. Move to Root (Canvas) to drag
        transform.SetParent(transform.root); 
        cg.blocksRaycasts = false; 
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        cg.blocksRaycasts = true;

        // 1. Gameplay Logic (Check if dropped on Plate)
        try 
        {
            if (eventData.pointerEnter != null && eventData.pointerEnter.name == "Prep_Area")
            {
                var station = eventData.pointerEnter.GetComponentInParent<FillingStation>();
                if (station != null) station.ReceiveIngredient(gameObject.name);
            }
        }
        catch { } // Ignore errors here so we can still go home

        // 2. FORCE RETURN TO TRAY
        if (trayTransform != null)
        {
            Debug.Log("🏠 Returning " + gameObject.name + " to " + trayTransform.name);
            transform.SetParent(trayTransform);
            
            // Force the layout to refresh so it snaps into place
            LayoutRebuilder.ForceRebuildLayoutImmediate(trayTransform as RectTransform);
        }
        else
        {
            Debug.LogError("❌ I don't know where to go! Tray is missing.");
        }

        // 3. Reset Position (0,0 relative to its new slot in the tray)
        ((RectTransform)transform).anchoredPosition = originalPos;
    }
}