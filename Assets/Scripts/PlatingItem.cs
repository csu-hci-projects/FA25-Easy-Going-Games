using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class PlatingItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Transform parentAfterDrag;
    [HideInInspector] public PlatingMinigame minigameManager;
    [HideInInspector] public RectTransform plateArea;

    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 startPos;
    private Transform startParent;
    public bool isPlated = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Don't move if we are already served/locked
        if (minigameManager.isServed) return;

        startPos = rectTransform.anchoredPosition;
        startParent = transform.parent;
        
        // Unparent so it draws on top of everything while dragging
        transform.SetParent(minigameManager.transform); 
        
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (minigameManager.isServed) return;
        rectTransform.anchoredPosition += eventData.delta / minigameManager.GetComponentInParent<Canvas>().scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (minigameManager.isServed) return;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Check if dropped on plate
        if (RectTransformUtility.RectangleContainsScreenPoint(plateArea, Input.mousePosition))
        {
            // Valid Drop
            transform.SetParent(parentAfterDrag); // Parent to the Plate
            isPlated = true;
            minigameManager.CheckPlatingStatus();
            
            // Optional: Random rotation for natural look
            float randomRot = Random.Range(-15f, 15f);
            transform.localRotation = Quaternion.Euler(0, 0, randomRot);
        }
        else
        {
            // Invalid Drop - Return to tray
            transform.SetParent(startParent);
            rectTransform.anchoredPosition = startPos;
            isPlated = false;
            transform.localRotation = Quaternion.identity; // Reset rotation
        }
    }
}