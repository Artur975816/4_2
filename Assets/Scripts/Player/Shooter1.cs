using UnityEngine;
using UnityEngine.InputSystem;

public sealed class Shooter1 : MonoBehaviour
{
    [Min(0.1f)] public float fireCooldown = 0.28f;
    [Min(1f)] public float projectileSpeed = 14f;
    [Min(1)] public int damage = 1;
    public Transform muzzle;
    float nextFireTime;
    Sprite projectileSprite;

    void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard.xKey.wasPressedThisFrame) Fire();
        var pad = Gamepad.current;
        if (pad != null && pad.buttonWest.wasPressedThisFrame) Fire();
        var mouse = Mouse.current;
        if (mouse != null && mouse.leftButton.wasPressedThisFrame) Fire();
    }

    public void Fire()
    {
        if (Time.time < nextFireTime || (GameManager.Instance != null && GameManager.Instance.IsFinished)) return;
        nextFireTime = Time.time + fireCooldown;
        Assignment6.A6Audio.PlayShot();
        Vector2 direction = Vector2.right;
        var renderer = GetComponentInChildren<SpriteRenderer>();
        if (renderer != null && renderer.flipX) direction = Vector2.left;
        Vector3 origin = muzzle != null ? muzzle.position : transform.position + (Vector3)(direction * 0.55f);
        var shot = new GameObject("Player projectile");
        shot.transform.position = origin;
        shot.transform.localScale = Vector3.one * 0.22f;
        var sr = shot.AddComponent<SpriteRenderer>();
        sr.sprite = GetProjectileSprite();
        sr.color = new Color(1f, 0.75f, 0.32f);
        sr.sortingOrder = 5;
        var collider = shot.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        var body = shot.AddComponent<Rigidbody2D>();
        body.gravityScale = 0f;
        body.bodyType = RigidbodyType2D.Kinematic;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        var bullet = shot.AddComponent<Bullet>();
        bullet.Initialize(direction, projectileSpeed, damage);
    }

    Sprite GetProjectileSprite()
    {
        if (projectileSprite != null) return projectileSprite;
        var texture = new Texture2D(8, 8, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        var pixels = new Color[64];
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.white;
        texture.SetPixels(pixels);
        texture.Apply();
        projectileSprite = Sprite.Create(texture, new Rect(0, 0, 8, 8), new Vector2(.5f, .5f), 16f);
        return projectileSprite;
    }
}
