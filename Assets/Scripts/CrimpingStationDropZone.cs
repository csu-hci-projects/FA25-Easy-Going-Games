using UnityEngine;
using System.Collections.Generic;

public class CrimpingStationDropZone : MonoBehaviour
{
    [Header("Visual Layering Settings")]
    public float verticalSpacing = 20f;          // Space between ingredients
    public float maxVerticalOffset = 80f;        // Maximum total spread
    public float scaleReduction = 0.1f;          // Scale reduction per layer
    public float minScale = 0.6f;                // Minimum scale for bottom layers
    public bool enableRandomRotation = true;     // Natural-looking slight rotations
    public float maxRotation = 5f;               // Max rotation in degrees
    
    [Header("Animation Settings")]
    public float transferDuration = 0.3f;        // Time to move ingredients
    public AnimationCurve transferCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    private List<Transform> currentIngredients = new List<Transform>();
    
    public void ReceiveIngredientsFromFilling(Transform[] ingredients)
    {
        // Clear existing ingredients if needed
        ClearStation();
        
        // Add new ingredients
        currentIngredients.AddRange(ingredients);
        
        // Start coroutine to animate the layering
        StartCoroutine(ArrangeIngredientsWithAnimation());
    }
    
    // Call this when ingredients are moved directly (not instantiated)
    public void AddIngredientsDirectly(List<Transform> ingredients)
    {
        currentIngredients.AddRange(ingredients);
        ArrangeIngredientsImmediately();
    }
    
    private System.Collections.IEnumerator ArrangeIngredientsWithAnimation()
    {
        Vector3[] startPositions = new Vector3[currentIngredients.Count];
        Vector3[] startScales = new Vector3[currentIngredients.Count];
        Quaternion[] startRotations = new Quaternion[currentIngredients.Count];
        
        // Store initial states
        for (int i = 0; i < currentIngredients.Count; i++)
        {
            if (currentIngredients[i] != null)
            {
                startPositions[i] = currentIngredients[i].position;
                startScales[i] = currentIngredients[i].localScale;
                startRotations[i] = currentIngredients[i].rotation;
            }
        }
        
        // Get target positions
        Vector3[] targetPositions = CalculateTargetPositions();
        Vector3[] targetScales = CalculateTargetScales();
        Quaternion[] targetRotations = CalculateTargetRotations();
        
        // Animate to target positions
        float timer = 0f;
        while (timer < transferDuration)
        {
            timer += Time.deltaTime;
            float t = transferCurve.Evaluate(timer / transferDuration);
            
            for (int i = 0; i < currentIngredients.Count; i++)
            {
                if (currentIngredients[i] != null)
                {
                    currentIngredients[i].position = Vector3.Lerp(startPositions[i], targetPositions[i], t);
                    currentIngredients[i].localScale = Vector3.Lerp(startScales[i], targetScales[i], t);
                    currentIngredients[i].rotation = Quaternion.Lerp(startRotations[i], targetRotations[i], t);
                }
            }
            yield return null;
        }
        
        // Ensure final positions
        ArrangeIngredientsImmediately();
    }
    
    private void ArrangeIngredientsImmediately()
    {
        Vector3[] positions = CalculateTargetPositions();
        Vector3[] scales = CalculateTargetScales();
        Quaternion[] rotations = CalculateTargetRotations();
        
        for (int i = 0; i < currentIngredients.Count; i++)
        {
            if (currentIngredients[i] != null)
            {
                currentIngredients[i].localPosition = positions[i];
                currentIngredients[i].localScale = scales[i];
                currentIngredients[i].localRotation = rotations[i];
                
                // Disable interaction
                DraggableItem draggable = currentIngredients[i].GetComponent<DraggableItem>();
                if (draggable != null)
                {
                    draggable.enabled = false;
                }
                
                // Move to bottom of hierarchy for proper rendering order
                currentIngredients[i].SetAsFirstSibling();
            }
        }
    }
    
    private Vector3[] CalculateTargetPositions()
    {
        Vector3[] positions = new Vector3[currentIngredients.Count];
        int count = currentIngredients.Count;
        
        if (count == 0) return positions;
        
        // Calculate centered positioning with vertical stacking
        float totalHeight = Mathf.Min((count - 1) * verticalSpacing, maxVerticalOffset);
        float startY = -totalHeight / 2f;
        
        for (int i = 0; i < count; i++)
        {
            float yOffset = startY + (i * verticalSpacing);
            positions[i] = new Vector3(0, yOffset, 0);
        }
        
        return positions;
    }
    
    private Vector3[] CalculateTargetScales()
    {
        Vector3[] scales = new Vector3[currentIngredients.Count];
        int count = currentIngredients.Count;
        
        for (int i = 0; i < count; i++)
        {
            // Bottom ingredients are slightly larger for better visibility
            float scaleFactor = 1f - (i * scaleReduction);
            scaleFactor = Mathf.Max(scaleFactor, minScale);
            scales[i] = Vector3.one * scaleFactor;
        }
        
        return scales;
    }
    
    private Quaternion[] CalculateTargetRotations()
    {
        Quaternion[] rotations = new Quaternion[currentIngredients.Count];
        
        if (!enableRandomRotation)
        {
            for (int i = 0; i < rotations.Length; i++)
            {
                rotations[i] = Quaternion.identity;
            }
            return rotations;
        }
        
        // Create natural-looking slight rotations
        System.Random random = new System.Random(currentIngredients.Count);
        for (int i = 0; i < rotations.Length; i++)
        {
            float randomRotation = ((float)random.NextDouble() * 2f - 1f) * maxRotation;
            rotations[i] = Quaternion.Euler(0, 0, randomRotation);
        }
        
        return rotations;
    }
    
    public void ClearStation()
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
    
    public List<Transform> GetCurrentIngredients()
    {
        return new List<Transform>(currentIngredients);
    }
    
    // Call this when moving to next station to transfer ingredients
    public List<Transform> TakeAllIngredients()
    {
        List<Transform> ingredients = new List<Transform>(currentIngredients);
        currentIngredients.Clear();
        return ingredients;
    }
}