using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>Connects mobile UI pointer presses directly to jump and fire gameplay actions.</summary>
public sealed class MobileControlButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    public bool jumpButton;
    PlayerMovement player;
    Shooter1 shooter;

    void Awake()
    {
        player = FindFirstObjectByType<PlayerMovement>();
        shooter = FindFirstObjectByType<Shooter1>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (jumpButton)
        {
            if (player == null) player = FindFirstObjectByType<PlayerMovement>();
            if (player != null) player.QueueJump();
        }
        else
        {
            if (shooter == null) shooter = FindFirstObjectByType<Shooter1>();
            if (shooter != null) shooter.Fire();
        }
    }

    public void OnPointerUp(PointerEventData eventData) => CutJump();
    public void OnPointerExit(PointerEventData eventData) => CutJump();

    void CutJump()
    {
        if (jumpButton && player != null) player.CutJump();
    }
}
