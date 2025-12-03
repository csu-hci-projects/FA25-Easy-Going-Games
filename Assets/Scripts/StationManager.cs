using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class StationManager : MonoBehaviour
{
    [Header("Station Configuration")]
    public List<RectTransform> stations; 
    public float slideSpeed = 0.5f;
    private int currentStationIndex = 0;

    void Start()
    {
        InitializeStations();
    }

    void InitializeStations()
    {
        // 1. Calculate Width (With a Safety Net)
        float width = Screen.width;
        if (width < 100) width = 1920f; 

        // 2. Force Position Every Single Station
        for (int i = 0; i < stations.Count; i++)
        {
            if (stations[i] == null) continue;
            stations[i].gameObject.SetActive(true);

            if (i == currentStationIndex)
            {
                stations[i].anchoredPosition = Vector2.zero;
            }
            else
            {
                float direction = (i > currentStationIndex) ? 1f : -1f;
                stations[i].anchoredPosition = new Vector2(direction * width, 0);
            }
        }
    }

    // Public methods that buttons can call directly
    public void GoToOrderStation() => SwitchToStation(0);
    public void GoToFillingStation() => SwitchToStation(1);
    public void GoToCrimpingStation() => SwitchToStation(2);
    public void GoToCookingStation() => SwitchToStation(3);
    public void GoToPlatingStation() => SwitchToStation(4);

    public int GetCurrentStationIndex()
    {
        return currentStationIndex;
    }


    public void SwitchToStation(int targetIndex)
    {
        Debug.Log($"🚂 Switching to Station Index: {targetIndex}");

        if (targetIndex == currentStationIndex) 
        {
            Debug.Log("🛑 Already at this station!");
            return;
        }

        if (targetIndex >= stations.Count)
        {
            Debug.LogError($"❌ Station {targetIndex} doesn't exist!");
            return;
        }

        StartCoroutine(AnimateSlide(targetIndex));
    }

    IEnumerator AnimateSlide(int targetIndex)
    {
        RectTransform current = stations[currentStationIndex];
        RectTransform next = stations[targetIndex];
        float width = Screen.width;
        
        float dir = (targetIndex > currentStationIndex) ? -1f : 1f;

        // Prepare next station
        next.anchoredPosition = new Vector2(-dir * width, 0);

        float timer = 0f;
        Vector2 startCurr = current.anchoredPosition;
        Vector2 startNext = next.anchoredPosition;

        while (timer < slideSpeed)
        {
            timer += Time.deltaTime;
            float t = timer / slideSpeed;
            t = t * t * (3f - 2f * t); // Smooth ease

            current.anchoredPosition = Vector2.Lerp(startCurr, new Vector2(dir * width, 0), t);
            next.anchoredPosition = Vector2.Lerp(startNext, Vector2.zero, t);
            yield return null;
        }
        
        current.anchoredPosition = new Vector2(dir * width, 0);
        next.anchoredPosition = Vector2.zero;
        currentStationIndex = targetIndex;
    }
}