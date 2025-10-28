using UnityEngine;
using UnityEngine.UI;

public class DumplingCrimper : MonoBehaviour
{
    public CookingStation station;
    public Image dumplingImage; // The image representing the dumpling
    public Sprite[] crimpSprites; // Three sprites for each crimp stage
    public RectTransform tickMark; // The oscillating tick mark
    public RectTransform gaugeArea; // The gauge area for timing
    public RectTransform greenZone; // The green target area panel

    public int clickCount = 0;
    public bool crimpingActive = false;
    public float oscillationSpeed = 200f; // pixels per second
    private bool movingRight = true;
    private int score = 0;

    void Update()
    {
        if (!crimpingActive) return;

        // Move tick mark left and right
        Vector2 pos = tickMark.anchoredPosition;
        float maxX = gaugeArea.rect.width / 2;
        float minX = -maxX;
        pos.x += (movingRight ? 1 : -1) * oscillationSpeed * Time.deltaTime;

        if (pos.x > maxX) { pos.x = maxX; movingRight = false; }
        if (pos.x < minX) { pos.x = minX; movingRight = true; }

        tickMark.anchoredPosition = pos;
    }

   public void ClickCrimp()
{
    if (!crimpingActive) return;
    clickCount++;

    // Change dumpling color for feedback
    Color[] colors = { Color.yellow, Color.magenta, Color.green };
    if (clickCount - 1 < colors.Length)
        dumplingImage.color = colors[clickCount - 1];

    // Check if tickMark is inside green zone with tolerance
    Vector3 localTickPos = tickMark.localPosition;
    Vector3 localZonePos = greenZone.localPosition;
    float zoneHalfWidth = greenZone.rect.width / 2;
    float tolerance = .9f; // extra pixels for fair perfect hit

    bool inGreenZone = Mathf.Abs(localTickPos.x - localZonePos.x) <= (zoneHalfWidth + tolerance);

    if (inGreenZone)
        Debug.Log("✅ Perfect Crimp! +5 points");
    else
        Debug.Log("❌ Missed Crimp! +1 point");

    // Update score in console
    if (inGreenZone)
        score += 5;
    else
        score += 1;

    Debug.Log($"Current Score: {score}");

    // Increase oscillation speed after each click
    oscillationSpeed += 2f;

    if (clickCount >= 3)
        FinishCrimp();
}



    public void StartCrimping()
    {
        if (!station.ReadyToCrimp()) return;

        crimpingActive = true;
        clickCount = 0;
        Debug.Log("Crimping started!");
    }

    void FinishCrimp()
    {
        crimpingActive = false;
        Debug.Log($"Dumpling folded! Final Score: {score}");
        // You can add code here later to show the folded dumpling sprite
    }
}
