using UnityEngine;

/// Shared helpers for all three enemies (2D, XY plane, sprites face RIGHT by default).
public static class Sight2D
{
    /// 1) distance  2) angle  3) wall raycast. Cheap checks first.
    public static bool CanSee(Vector2 eye, Vector2 facing, Transform target,
                              float range, float fov, LayerMask walls)
    {
        Vector2 to = (Vector2)target.position - eye;
        if (to.magnitude > range) return false;                        // 1. distance
        if (Vector2.Angle(facing, to) > fov * 0.5f) return false;      // 2. angle
        return !Physics2D.Raycast(eye, to.normalized, to.magnitude, walls); // 3. walls block
    }

    /// "No attacks from off-screen": an enemy may start a wind-up only while it is inside the camera view.
    public static bool OnScreen(Vector2 worldPos, float margin = 0.03f)
    {
        var cam = Camera.main;
        if (cam == null) return true;
        Vector3 v = cam.WorldToViewportPoint(worldPos);
        return v.z > 0f && v.x > margin && v.x < 1f - margin && v.y > margin && v.y < 1f - margin;
    }

    /// Creates (or reuses) a LineRenderer on host. Used for the laser and the charge lane.
    public static LineRenderer MakeLine(GameObject host, float width, int sortingOrder = 6)
    {
        var lr = host.GetComponent<LineRenderer>();
        if (lr == null) lr = host.AddComponent<LineRenderer>();
        lr.positionCount = 2;
        lr.useWorldSpace = true;
        lr.startWidth = lr.endWidth = width;
        lr.material = new Material(Shader.Find("Sprites/Default"));
        lr.sortingOrder = sortingOrder;
        lr.enabled = false;
        return lr;
    }

    public static void DrawCone(Vector2 pos, Vector2 facing, float range, float fov)
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(pos, range);
        Gizmos.color = Color.cyan;
        Vector2 l = Quaternion.Euler(0f, 0f, -fov * 0.5f) * facing;
        Vector2 r = Quaternion.Euler(0f, 0f,  fov * 0.5f) * facing;
        Gizmos.DrawRay(pos, l * range);
        Gizmos.DrawRay(pos, r * range);
    }
}
