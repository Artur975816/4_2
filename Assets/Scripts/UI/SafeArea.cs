using UnityEngine;

/// <summary>Keeps its RectTransform inside the device's actual visible screen area.</summary>
[DisallowMultipleComponent]
public sealed class SafeArea : MonoBehaviour
{
    RectTransform rectTransform;
    Rect lastSafeArea;
    Vector2Int lastScreenSize;
    bool hasApplied;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
    }

    void Start()
    {
        RefreshNow();
    }

    void Update()
    {
        Vector2Int screenSize = new Vector2Int(Screen.width, Screen.height);
        Rect safeArea = Screen.safeArea;
        if (!hasApplied || screenSize != lastScreenSize || safeArea != lastSafeArea)
            Apply(safeArea, screenSize);
    }

    public void RefreshNow()
    {
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        Apply(Screen.safeArea, new Vector2Int(Screen.width, Screen.height));
    }

    void Apply(Rect safeArea, Vector2Int screenSize)
    {
        if (rectTransform == null || screenSize.x <= 0 || screenSize.y <= 0) return;

        Vector2 min = safeArea.position;
        Vector2 max = safeArea.position + safeArea.size;
        min.x = Mathf.Clamp01(min.x / screenSize.x);
        min.y = Mathf.Clamp01(min.y / screenSize.y);
        max.x = Mathf.Clamp01(max.x / screenSize.x);
        max.y = Mathf.Clamp01(max.y / screenSize.y);

        rectTransform.anchorMin = min;
        rectTransform.anchorMax = max;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        lastSafeArea = safeArea;
        lastScreenSize = screenSize;
        hasApplied = true;
    }
}
