using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class OrderQueueManager : MonoBehaviour
{
    [Header("Order Settings")]
    public int maxActiveOrders = 3;
    public float orderSpawnInterval = 45f;
    
    [Header("UI References")]
    public Transform ordersContainer;
    public GameObject orderUIPrefab;
    
    [Header("Dumpling Sprites")]
    public Sprite porkGyozaSprite;
    public Sprite veggieGyozaSprite;
    public Sprite porkManapuaSprite;
    public Sprite veggieManapuaSprite;
    public Sprite porkWontonSprite;
    public Sprite veggieWontonSprite;
    
    [Header("Scoring")]
    public int baseOrderScore = 100;
    public int timeBonusMultiplier = 2;
    public int perfectOrderBonus = 50;
    
    private Queue<Order> orderQueue = new Queue<Order>();
    private List<Order> activeOrders = new List<Order>();
    private float orderTimer;
    private int totalScore = 0;
    
    // References
    private TextMeshProUGUI scoreText;
    private DumplingProgressTracker progressTracker;
    
void Start()
    {
    // Ensure recipe database exists
    if (FindAnyObjectByType<DumplingRecipeDatabase>() == null)
    {
        GameObject recipeDB = new GameObject("RecipeDatabase");
        recipeDB.AddComponent<DumplingRecipeDatabase>();
    }
    
    // Find references
    FindScoreText(); // Add this line!
    progressTracker = FindAnyObjectByType<DumplingProgressTracker>();
    
    if (progressTracker == null)
    {
        GameObject trackerObj = new GameObject("DumplingProgressTracker");
        progressTracker = trackerObj.AddComponent<DumplingProgressTracker>();
    }
    
    orderTimer = orderSpawnInterval;
    UpdateScoreUI(); // Initialize the score display
    
    // Start with one test order
    Invoke("GenerateTestOrder", 2f);
    }
    
    void Update()
    {
        if (IsOrderStationActive())
        {
            UpdateOrderTimers();
            UpdateOrderSpawning();
        }
    }
    
    void GenerateTestOrder()
    {
        // Random dumpling type from all 6 options
        DumplingType[] allTypes = {
            DumplingType.PorkGyoza, DumplingType.VeggieGyoza,
            DumplingType.PorkManapua, DumplingType.VeggieManapua,
            DumplingType.PorkWonton, DumplingType.VeggieWonton
        };
        
        DumplingType randomType = allTypes[Random.Range(0, allTypes.Length)];
        int randomQty = Random.Range(1, 4);
        float randomTime = Random.Range(60f, 120f);
        
        Order newOrder = new Order(randomType, randomQty, randomTime);
        AddOrder(newOrder);
    }
    
    void AddOrder(Order order)
    {
        if (activeOrders.Count < maxActiveOrders)
        {
            activeOrders.Add(order);
            CreateOrderUI(order);
            Debug.Log($"New order: {order.quantity}x {order.GetDisplayName()}");
        }
        else
        {
            orderQueue.Enqueue(order);
        }
    }
    
    void CreateOrderUI(Order order)
    {
        if (orderUIPrefab && ordersContainer != null)
        {
            GameObject orderUI = Instantiate(orderUIPrefab, ordersContainer);
            OrderUI orderUIComponent = orderUI.GetComponent<OrderUI>();
            
            if (orderUIComponent != null)
            {
                orderUIComponent.Initialize(order, this, GetDumplingSprite(order.dumplingType));
            }
        }
    }
    
    Sprite GetDumplingSprite(DumplingType type)
    {
        switch (type)
        {
            case DumplingType.PorkGyoza: return porkGyozaSprite;
            case DumplingType.VeggieGyoza: return veggieGyozaSprite;
            case DumplingType.PorkManapua: return porkManapuaSprite;
            case DumplingType.VeggieManapua: return veggieManapuaSprite;
            case DumplingType.PorkWonton: return porkWontonSprite;
            case DumplingType.VeggieWonton: return veggieWontonSprite;
            default: return porkGyozaSprite;
        }
    }
    
    void UpdateOrderTimers()
    {
        for (int i = activeOrders.Count - 1; i >= 0; i--)
        {
            Order order = activeOrders[i];
            
            if (!order.isCompleted && !order.isFailed)
            {
                order.timeRemaining -= Time.deltaTime;
                
                if (order.timeRemaining <= 0)
                {
                    FailOrder(order);
                }
            }
        }
    }
    
    void UpdateOrderSpawning()
    {
        orderTimer -= Time.deltaTime;
        
        if (orderTimer <= 0 && activeOrders.Count < maxActiveOrders)
        {
            GenerateTestOrder();
            orderTimer = orderSpawnInterval;
        }
    }
    
    // Called from Filling Station when starting a dumpling
    public void StartDumplingForOrder(DumplingType type)
    {
        Order order = FindMatchingOrder(type);
        if (order != null && order.dumplingsInProgress + order.dumplingsCompleted < order.quantity)
        {
            order.dumplingsInProgress++;
            progressTracker.StartNewDumpling(type, order.orderID);
            Debug.Log($"Started {order.GetDisplayName()} for order {order.orderID}");
        }
    }
    
    // Called from Plating Station when completing a dumpling
    public void CompleteDumplingForOrder(DumplingType type, float qualityScore = 1f)
    {
        Order order = FindMatchingOrder(type);
        if (order != null && order.dumplingsInProgress > 0)
        {
            order.dumplingsInProgress--;
            order.dumplingsCompleted++;
            
            // Check if order is fully completed
            if (order.dumplingsCompleted >= order.quantity)
            {
                CompleteOrder(order, qualityScore);
            }
            
            Debug.Log($"Completed {order.GetDisplayName()}. Progress: {order.dumplingsCompleted}/{order.quantity}");
        }
    }
    
void CompleteOrder(Order order, float qualityScore)
    {
    order.isCompleted = true;
    
    // Calculate score with quality multiplier
    int timeBonus = Mathf.RoundToInt(order.timeRemaining * timeBonusMultiplier);
    int qualityBonus = Mathf.RoundToInt(baseOrderScore * (qualityScore - 1f));
    int orderScore = baseOrderScore + timeBonus + qualityBonus;
    
    // Use AddScore instead of directly modifying totalScore
    AddScore(orderScore);
    
    Debug.Log($"Order completed! Score: {orderScore}");
    
    activeOrders.Remove(order);
    
    if (orderQueue.Count > 0)
    {
        AddOrder(orderQueue.Dequeue());
    }
    }
    
    void FailOrder(Order order)
    {
        order.isFailed = true;
        activeOrders.Remove(order);
        Debug.Log($"Order failed! {order.quantity}x {order.GetDisplayName()}");
        
        if (orderQueue.Count > 0)
        {
            AddOrder(orderQueue.Dequeue());
        }
    }
    
    Order FindMatchingOrder(DumplingType type)
    {
        foreach (Order order in activeOrders)
        {
            if (order.dumplingType == type && !order.isCompleted && !order.isFailed)
            {
                return order;
            }
        }
        return null;
    }
    
    void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {totalScore}";
        }
        else
        {
            FindScoreText();
            if (scoreText != null)
            {
                scoreText.text = $"Score: {totalScore}";
            }
        }
    }
    
    bool IsOrderStationActive()
    {
        StationManager stationManager = FindAnyObjectByType<StationManager>();
        return stationManager != null && stationManager.GetCurrentStationIndex() == 0;
    }
    
    // For testing

    void FindScoreText()
{
    if (scoreText == null)
    {
        // Look for the Score object in Header_Panel
        GameObject headerPanel = GameObject.Find("Header_Panel");
        if (headerPanel != null)
        {
            Transform scoreTransform = headerPanel.transform.Find("Score");
            if (scoreTransform != null)
            {
                scoreText = scoreTransform.GetComponent<TextMeshProUGUI>();
                if (scoreText != null)
                {
                    Debug.Log("✅ Score text found and assigned!");
                }
                else
                {
                    Debug.LogError("❌ Score object found but no TextMeshPro component!");
                }
            }
            else
            {
                Debug.LogError("❌ Could not find 'Score' object in Header_Panel!");
            }
        }
        else
        {
            Debug.LogError("❌ Could not find Header_Panel!");
        }
    }
}
    public void TestCompleteOrder()
    {
        if (activeOrders.Count > 0)
        {
            CompleteOrder(activeOrders[0], 1f);
        }
    }

    public void AddScore(int points)
    {
    totalScore += points;
    UpdateScoreUI();
    Debug.Log($"🎯 Score: +{points} (Total: {totalScore})");
    }

    
    
    public List<Order> GetActiveOrders()
    {
        return activeOrders;
    }
    
    public DumplingProgressTracker GetProgressTracker()
    {
        return progressTracker;
    }
}