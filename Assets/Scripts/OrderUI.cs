using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OrderUI : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI orderText;
    public TextMeshProUGUI timerText;
    public Image timerFill;
    public Image dumplingIcon;
    public TextMeshProUGUI progressText;
    
    private Order currentOrder;
    private OrderQueueManager orderManager;
    
    public void Initialize(Order order, OrderQueueManager manager, Sprite icon)
    {
        currentOrder = order;
        orderManager = manager;
        
        if (dumplingIcon != null && icon != null)
        {
            dumplingIcon.sprite = icon;
        }
        
        UpdateUI(); // Call it initially to set up the text
    }
    
    void Update()
    {
        if (currentOrder != null && !currentOrder.isCompleted && !currentOrder.isFailed)
        {
            UpdateUI(); // Call it every frame to update timer and progress
        }
        else if (currentOrder != null && (currentOrder.isCompleted || currentOrder.isFailed))
        {
            Destroy(gameObject);
        }
    }
    
    // ADD THIS FUNCTION TO YOUR EXISTING FILE:
    void UpdateUI()
    {
        if (orderText != null)
        {
            orderText.text = $"{currentOrder.quantity}x {currentOrder.GetDisplayName()}";
        }
        
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(currentOrder.timeRemaining).ToString() + "s";
        }
        
        if (progressText != null)
        {
            progressText.text = $"{currentOrder.dumplingsCompleted}/{currentOrder.quantity}";
        }
        
        // Keep your existing timer fill bar logic if you're using it
        if (timerFill != null)
        {
            timerFill.fillAmount = currentOrder.timeRemaining / currentOrder.timeLimit;
            
            // Color coding
            if (currentOrder.timeRemaining < currentOrder.timeLimit * 0.3f)
                timerFill.color = Color.red;
            else if (currentOrder.timeRemaining < currentOrder.timeLimit * 0.6f)
                timerFill.color = Color.yellow;
            else
                timerFill.color = Color.green;
        }
    }
    
    // For testing
    public void OnCompleteButton()
    {
        orderManager.TestCompleteOrder();
    }
}