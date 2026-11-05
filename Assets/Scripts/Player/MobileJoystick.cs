using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Reads the on-screen movement stick directly from pointer and touch events.</summary>
public sealed class MobileJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Min(1f)] public float maxDragDistance = 68f;
    public RectTransform knob;
    public static MobileJoystick Current { get; private set; }
    public float Horizontal { get; private set; }

    RectTransform rectTransform;
    bool pressed;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        if (knob == null)
        {
            var child = transform.Find("Stick knob");
            if (child != null) knob = child.GetComponent<RectTransform>();
        }
        ResetKnob();
    }

    void OnEnable() => Current = this;

    void OnDisable()
    {
        if (Current == this) Current = null;
        pressed = false;
        Horizontal = 0f;
        ResetKnob();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        pressed = true;
        UpdateStick(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (pressed) UpdateStick(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        pressed = false;
        Horizontal = 0f;
        ResetKnob();
    }

    void UpdateStick(PointerEventData eventData)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, eventData.position, eventData.pressEventCamera, out var localPoint)) return;
        Vector2 offset = Vector2.ClampMagnitude(localPoint - rectTransform.rect.center, maxDragDistance);
        Horizontal = Mathf.Clamp(offset.x / Mathf.Max(1f, maxDragDistance), -1f, 1f);
        if (knob != null) knob.anchoredPosition = offset;
    }

    void ResetKnob()
    {
        if (knob != null) knob.anchoredPosition = Vector2.zero;
    }
}
