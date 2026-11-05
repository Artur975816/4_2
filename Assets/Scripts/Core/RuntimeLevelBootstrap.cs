using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem.UI;

[DefaultExecutionOrder(-1000)]
public sealed class RuntimeLevelBootstrap : MonoBehaviour
{
    static RuntimeLevelBootstrap instance;
    static Sprite heroSprite, tileSprite, bossSprite, orbSprite, spikeSprite, joystickKnobSprite;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Install()
    {
        if (instance != null) return;
        var host = new GameObject("Runtime Level Setup");
        instance = host.AddComponent<RuntimeLevelBootstrap>();
        DontDestroyOnLoad(host);
        new GameObject("Game Manager").AddComponent<GameManager>();
    }

    void Awake()
    {
        if (instance != null && instance != this) { Destroy(gameObject); return; }
        instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDestroy() { if (instance == this) SceneManager.sceneLoaded -= OnSceneLoaded; }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // Android supports the Input System package, not the legacy input module.
        // Keep menu and settings pointer controls working in Input System-only mode too.
        EnsureSafeAreas();
        if (!scene.name.StartsWith("Level"))
        {
            EnsureEventSystem();
            return;
        }
        if (GameManager.Instance == null) new GameObject("Game Manager").AddComponent<GameManager>();
        if (scene.name == "Level3")
        {
            if (FindFirstObjectByType<BossController>() == null) BuildBossArena();
            else
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) EnsurePlayer(player);
            }
            AddTouchControls();
            return;
        }
        SetupExistingLevel(scene.name);
        AddTouchControls();
    }

    void SetupExistingLevel(string sceneName)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null) return;
        EnsurePlayer(player);
        bool hasAuthoredSceneWalls =
            (sceneName == "Level1" && GameObject.Find("Level1 Side Walls") != null) ||
            (sceneName == "Level2" && GameObject.Find("Authored Level2 Arena Boundaries") != null);
        var exit = FindFirstObjectByType<Exit>();
        if (exit != null)
        {
            Vector3 pos = exit.transform.position;
            // Level geometry is authored into the scenes. Only retain a compatibility
            // fallback for scenes that have not yet been baked by the editor tool.
            if (GameObject.Find("Authored Parkour Route") == null)
            {
                pos.x = 100f;
                exit.transform.position = pos;
                AddRoutePlatforms(sceneName, pos.x);
            }
            var renderer = exit.GetComponent<SpriteRenderer>();
            if (renderer != null) renderer.color = new Color(.24f, .9f, .62f);
            if (!hasAuthoredSceneWalls) AddWall("Right level boundary", pos.x + 2.5f, -1f, 1f, 14f);
        }
        if (!hasAuthoredSceneWalls) AddWall("Left level boundary", -15f, -1f, 1f, 14f);
        AddFallZone();
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) canvas = CreateCanvas();
        var ui = FindFirstObjectByType<UIManager>();
        if (ui != null) EnsureLevelPanels(ui, canvas);
        AddTouchControls();
        InstallUiActions();
    }

    void EnsureLevelPanels(UIManager ui, Canvas canvas)
    {
        if (ui.winPanel == null)
        {
            ui.winPanel = MakeFullPanel(canvas.transform, new Color(.025f, .06f, .07f, .94f));
            CreateText(ui.winPanel.transform, "ТЫ ПОБЕДИЛ!", new Vector2(0, 250), new Vector2(900, 110), 72, new Color(1f, .82f, .52f));
            CreateText(ui.winPanel.transform, "Возвращаемся к выбору уровня…", new Vector2(0, 150), new Vector2(900, 70), 40, Color.white);
            CreateButton(ui.winPanel.transform, "ВЫБРАТЬ УРОВЕНЬ", new Vector2(0, -20), new Vector2(620, 140), () => ui.LoadScene("LevelSelect"));
            ui.winPanel.SetActive(false);
        }
        if (ui.gameOverPanel == null)
        {
            ui.gameOverPanel = MakeFullPanel(canvas.transform, new Color(.025f, .06f, .07f, .94f));
            CreateText(ui.gameOverPanel.transform, "TRY AGAIN", new Vector2(0, 250), new Vector2(900, 110), 72, Color.white);
            CreateButton(ui.gameOverPanel.transform, "RESTART LEVEL", new Vector2(0, 70), new Vector2(620, 140), ui.RestartLevel);
            CreateButton(ui.gameOverPanel.transform, "MAIN MENU", new Vector2(0, -90), new Vector2(620, 140), () => ui.LoadScene("MainMenu"));
            ui.gameOverPanel.SetActive(false);
        }
        if (ui.pausePanel == null)
        {
            ui.pausePanel = MakeFullPanel(canvas.transform, new Color(.025f, .06f, .07f, .94f));
            CreateText(ui.pausePanel.transform, "PAUSED", new Vector2(0, 290), new Vector2(900, 110), 72, Color.white);
            CreateButton(ui.pausePanel.transform, "RESUME", new Vector2(0, 140), new Vector2(620, 140), ui.Resume);
            CreateButton(ui.pausePanel.transform, "RESTART LEVEL", new Vector2(0, -20), new Vector2(620, 140), ui.RestartLevel);
            CreateButton(ui.pausePanel.transform, "MAIN MENU", new Vector2(0, -180), new Vector2(620, 140), () => ui.LoadScene("MainMenu"));
            ui.pausePanel.SetActive(false);
        }
        if (ui.messageText == null)
        {
            ui.messageText = CreateText(canvas.transform, "", new Vector2(0, -92), new Vector2(1100, 60), 40, new Color(1f, .88f, .58f));
            var rect = ui.messageText.rectTransform;
            rect.anchorMin = new Vector2(.5f, 1f);
            rect.anchorMax = new Vector2(.5f, 1f);
            rect.anchoredPosition = new Vector2(0, -102);
            ui.messageText.gameObject.SetActive(false);
        }
    }

    void AddRoutePlatforms(string sceneName, float finishX)
    {
        bool ruins = sceneName == "Level2";
        int count = 19;
        float startX = 46f;
        float spacing = (finishX - startX) / count;
        for (int i = 0; i < count; i++)
        {
            float x = startX + i * spacing;
            float y = -1.8f + Mathf.Sin(i * .9f) * (ruins ? .52f : .62f) + (ruins ? Mathf.Cos(i * .55f) * .22f : 0f);
            Color platformTint = ruins
                ? (i % 2 == 0 ? new Color(.27f, .43f, .47f) : new Color(.37f, .49f, .47f))
                : (i % 2 == 0 ? new Color(.25f, .48f, .34f) : new Color(.32f, .52f, .38f));
            var platform = CreatePlatform("Parkour platform " + (i + 1), new Vector2(x, y), new Vector2(2.5f, .42f), platformTint);
            if (i == 5 || i == 12)
            {
                var mover = platform.AddComponent<MovingPlatform>();
                mover.endpointOffset = i == 5 ? new Vector2(2f, 0f) : new Vector2(0f, -2.2f);
                mover.speed = 1.25f;
                mover.endpointPause = .7f;
            }
            if (i == 9)
            {
                var checkpoint = new GameObject("Moss checkpoint");
                checkpoint.transform.position = new Vector3(x, y + 1f, 0f);
                checkpoint.transform.localScale = new Vector3(.7f, 1.3f, 1f);
                var sr = checkpoint.AddComponent<SpriteRenderer>();
                sr.sprite = TileSprite;
                sr.color = new Color(.82f, .63f, .3f);
                var col = checkpoint.AddComponent<BoxCollider2D>(); col.isTrigger = true;
                checkpoint.AddComponent<Checkpoint>();
            }
            if (i == 3 || i == 15)
            {
                var coin = new GameObject("Route coin");
                coin.transform.position = new Vector3(x, y + 1f, 0f);
                coin.transform.localScale = Vector3.one * .42f;
                var sr = coin.AddComponent<SpriteRenderer>(); sr.sprite = OrbSprite; sr.color = new Color(1f, .82f, .35f);
                var col = coin.AddComponent<CircleCollider2D>(); col.isTrigger = true;
                coin.AddComponent<Coin>();
            }
            if (i == 7 || i == 16)
            {
                var thorn = new GameObject("Moonthorn hazard");
                thorn.transform.position = new Vector3(x, y + .42f, 0f);
                thorn.transform.localScale = new Vector3(1.1f, .45f, 1f);
                var sr = thorn.AddComponent<SpriteRenderer>(); sr.sprite = SpikeSprite; sr.color = new Color(.95f, .38f, .42f); sr.sortingOrder = 2;
                var col = thorn.AddComponent<BoxCollider2D>(); col.isTrigger = true; col.size = new Vector2(.9f, .8f);
                thorn.AddComponent<HazardTrigger>();
            }
        }
        CreatePlatform("Finish approach", new Vector2(finishX - 1.8f, -2.05f), new Vector2(4f, .5f), ruins ? new Color(.31f, .45f, .45f) : new Color(.28f, .46f, .35f));
    }

    void BuildBossArena()
    {
        var camera = Camera.main;
        if (camera != null) { camera.orthographic = true; camera.orthographicSize = 5.5f; camera.transform.position = new Vector3(0, 0, -10); camera.backgroundColor = new Color(.045f, .09f, .14f); }
        CreatePlatform("Moonstone arena floor", Vector2.down * 3f, new Vector2(17f, 1f), new Color(.24f, .39f, .34f));
        CreatePlatform("West moonstone step", new Vector2(-4.5f, -1.45f), new Vector2(3.4f, .38f), new Color(.32f, .5f, .38f));
        CreatePlatform("East moonstone step", new Vector2(4.3f, -1.45f), new Vector2(3.4f, .38f), new Color(.32f, .5f, .38f));
        AddWall("Arena west wall", -8.6f, 1f, .5f, 12f);
        AddWall("Arena east wall", 8.6f, 1f, .5f, 12f);
        AddFallZone();
        var player = CreateCharacter("Player", new Vector2(-6f, -1.9f), heroSprite, new Vector2(.72f, 1.15f), new Color(.94f, .62f, .35f));
        player.tag = "Player";
        EnsurePlayer(player);
        var boss = CreateCharacter("Moon Warden", new Vector2(5.6f, -1.8f), bossSprite, new Vector2(1.15f, 1.8f), new Color(.62f, .45f, .78f));
        boss.transform.localScale = new Vector3(1.7f, 1.7f, 1f);
        var trigger = boss.AddComponent<BoxCollider2D>(); trigger.isTrigger = true;
        var controller = boss.AddComponent<BossController>(); controller.maxHealth = 12; controller.arenaLeft = -8f; controller.arenaRight = 8f;
        var follow = camera != null ? camera.GetComponent<CameraFollow>() : null;
        if (camera != null) { if (follow == null) follow = camera.gameObject.AddComponent<CameraFollow>(); follow.target = player.transform; follow.offset = new Vector3(0, 1f, -10f); follow.smoothSpeed = 4f; }
        var canvas = CreateCanvas();
        var healthPanel = MakePanel(canvas.transform, new Vector2(.5f, 1f), new Vector2(0f, -50f), new Vector2(460f, 36f), new Color(.025f, .055f, .07f, .9f));
        var sliderObject = new GameObject("Boss health slider", typeof(RectTransform), typeof(Slider)); sliderObject.transform.SetParent(healthPanel.transform, false);
        var rect = sliderObject.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = new Vector2(5, 5); rect.offsetMax = new Vector2(-5, -5);
        var fillArea = new GameObject("Fill Area", typeof(RectTransform)); fillArea.transform.SetParent(sliderObject.transform, false);
        var fa = fillArea.GetComponent<RectTransform>(); fa.anchorMin = Vector2.zero; fa.anchorMax = Vector2.one; fa.offsetMin = Vector2.zero; fa.offsetMax = Vector2.zero;
        var fill = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); fill.transform.SetParent(fillArea.transform, false);
        var fr = fill.GetComponent<RectTransform>(); fr.anchorMin = Vector2.zero; fr.anchorMax = Vector2.one; fr.offsetMin = Vector2.zero; fr.offsetMax = Vector2.zero; fill.GetComponent<Image>().color = new Color(.91f, .28f, .31f);
        var fillImage = fill.GetComponent<Image>();
        fillImage.type = Image.Type.Filled; fillImage.fillMethod = Image.FillMethod.Horizontal; fillImage.fillOrigin = (int)Image.OriginHorizontal.Left; fillImage.fillAmount = 1f;
        var slider = sliderObject.GetComponent<Slider>(); slider.direction = Slider.Direction.LeftToRight; slider.targetGraphic = fillImage; slider.fillRect = fr; slider.minValue = 0; slider.maxValue = controller.maxHealth; slider.value = controller.maxHealth;
        controller.SetHealthBar(slider);
        var title = CreateText(canvas.transform, "MOON WARDEN", new Vector2(0, -20), new Vector2(380, 40), 23, new Color(1f, .82f, .58f));
        var manager = GameManager.Instance;
        var ui = new GameObject("Game UI").AddComponent<UIManager>();
        ui.winPanel = MakePanel(canvas.transform, new Vector2(.5f, .5f), Vector2.zero, new Vector2(580f, 360f), new Color(.035f, .075f, .09f, .97f));
        CreateText(ui.winPanel.transform, "THE MOON GARDEN IS SAFE", new Vector2(0, 90), new Vector2(520, 90), 35, new Color(1f, .82f, .52f));
        CreateButton(ui.winPanel.transform, "RETURN TO MENU", new Vector2(0, -55), new Vector2(310, 70), () => ui.LoadScene("MainMenu"));
        ui.winPanel.SetActive(false);
        ui.pausePanel = MakePanel(canvas.transform, new Vector2(.5f, .5f), Vector2.zero, new Vector2(540f, 340f), new Color(.035f, .075f, .09f, .97f));
        CreateText(ui.pausePanel.transform, "PAUSED", new Vector2(0, 80), new Vector2(450, 70), 38, Color.white);
        CreateButton(ui.pausePanel.transform, "RESUME", new Vector2(0, 5), new Vector2(260, 60), () => manager?.TogglePause());
        CreateButton(ui.pausePanel.transform, "RESTART", new Vector2(0, -70), new Vector2(260, 60), () => ui.RestartLevel());
        ui.pausePanel.SetActive(false);
        ui.gameOverPanel = MakePanel(canvas.transform, new Vector2(.5f, .5f), Vector2.zero, new Vector2(580f, 360f), new Color(.035f, .075f, .09f, .97f));
        CreateText(ui.gameOverPanel.transform, "THE GARDEN WILL WAIT", new Vector2(0, 80), new Vector2(520, 80), 34, Color.white);
        CreateButton(ui.gameOverPanel.transform, "TRY AGAIN", new Vector2(0, -20), new Vector2(280, 65), () => ui.RestartLevel());
        CreateButton(ui.gameOverPanel.transform, "MAIN MENU", new Vector2(0, -100), new Vector2(280, 65), () => ui.LoadScene("MainMenu"));
        ui.gameOverPanel.SetActive(false);
        if (manager != null) manager.RegisterCheckpoint(player.transform.position);
        AddMobileControls(canvas);
    }

    GameObject CreateCharacter(string name, Vector2 position, Sprite sprite, Vector2 colliderSize, Color color)
    {
        var go = new GameObject(name); go.transform.position = position;
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = sprite; sr.color = color; sr.sortingOrder = 3;
        var col = go.AddComponent<CapsuleCollider2D>(); col.size = colliderSize;
        var rb = go.AddComponent<Rigidbody2D>(); rb.freezeRotation = true; rb.gravityScale = 1.5f; rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        return go;
    }

    void EnsurePlayer(GameObject player)
    {
        if (player.GetComponent<Rigidbody2D>() == null) { var rb = player.AddComponent<Rigidbody2D>(); rb.freezeRotation = true; rb.gravityScale = 1.5f; rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; rb.interpolation = RigidbodyInterpolation2D.Interpolate; }
        if (player.GetComponent<Collider2D>() == null) { var col = player.AddComponent<CapsuleCollider2D>(); col.size = new Vector2(.72f, 1.15f); }
        var movement = player.GetComponent<PlayerMovement>(); if (movement == null) movement = player.AddComponent<PlayerMovement>();
        movement.jumpHeight = 2.5f;
        movement.groundLayer = (1 << 0) | (1 << 6);
        if (player.GetComponent<Assignment6.A6Audio>() == null) player.AddComponent<Assignment6.A6Audio>();
        var controller = Resources.Load<RuntimeAnimatorController>("Player_Redrawn");
        if (controller != null)
        {
            var animator = player.GetComponent<Animator>(); if (animator == null) animator = player.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            if (player.GetComponent<PlayerAnimator>() == null) player.AddComponent<PlayerAnimator>();
        }
        if (player.GetComponent<Assignment6.Health>() == null) player.AddComponent<Assignment6.Health>();
        if (player.GetComponent<Shooter1>() == null) player.AddComponent<Shooter1>();
    }

    GameObject CreatePlatform(string name, Vector2 position, Vector2 size, Color tint)
    {
        var go = new GameObject(name); go.transform.position = position; go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = TileSprite; sr.color = tint; sr.sortingOrder = 0;
        go.AddComponent<BoxCollider2D>();
        return go;
    }

    void AddWall(string name, float x, float y, float width, float height)
    {
        var wall = new GameObject(name); wall.layer = 6; wall.transform.position = new Vector3(x, y, 0); wall.transform.localScale = new Vector3(width, height, 1);
        var sr = wall.AddComponent<SpriteRenderer>(); sr.sprite = TileSprite; sr.color = new Color(.13f, .28f, .27f); sr.sortingOrder = -1;
        wall.AddComponent<BoxCollider2D>();
    }

    void AddFallZone()
    {
        var zone = new GameObject("Fall and respawn zone"); zone.transform.position = new Vector3(30f, -12f, 0f);
        var col = zone.AddComponent<BoxCollider2D>(); col.isTrigger = true; col.size = new Vector2(300f, 2f);
        zone.AddComponent<PitTrigger>();
    }

    void AddTouchControls()
    {
        var canvas = FindFirstObjectByType<Canvas>();
        if (canvas == null) canvas = CreateCanvas();
        AddMobileControls(canvas);
    }

    Canvas CreateCanvas()
    {
        var go = new GameObject("Runtime HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        var canvas = go.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 1f;
        return canvas;
    }

    void AddMobileControls(Canvas canvas)
    {
        Transform safeArea = EnsureSafeArea(canvas);
        if (safeArea.Find("Mobile controls") != null) return;
        EnsureEventSystem();
        var root = new GameObject("Mobile controls", typeof(RectTransform)); root.transform.SetParent(safeArea, false);
        var r = root.GetComponent<RectTransform>(); r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one; r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
        var pad = MakePanel(root.transform, new Vector2(0, 0), new Vector2(250, 240), new Vector2(240, 240), new Color(.12f, .22f, .2f, .42f));
        pad.name = "Movement stick";
        var joystick = pad.AddComponent<MobileJoystick>();
        joystick.maxDragDistance = 72f;
        var knobObject = new GameObject("Stick knob", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        knobObject.transform.SetParent(pad.transform, false);
        var knobRect = knobObject.GetComponent<RectTransform>();
        knobRect.anchorMin = knobRect.anchorMax = new Vector2(.5f, .5f);
        knobRect.anchoredPosition = Vector2.zero;
        knobRect.sizeDelta = new Vector2(82, 82);
        var knobImage = knobObject.GetComponent<Image>();
        knobImage.sprite = JoystickKnobSprite;
        knobImage.color = new Color(.95f, .62f, .35f, .92f);
        knobImage.raycastTarget = false;
        joystick.knob = knobRect;

        var jump = MakePanel(root.transform, new Vector2(1, 0), new Vector2(-250, 240), new Vector2(170, 170), new Color(.88f, .48f, .22f, .84f));
        jump.name = "Jump button";
        jump.AddComponent<MobileControlButton>().jumpButton = true;
        CreateText(jump.transform, "JUMP", Vector2.zero, new Vector2(150, 140), 36, Color.white);

        var fire = MakePanel(root.transform, new Vector2(1, 0), new Vector2(-450, 190), new Vector2(150, 150), new Color(.68f, .25f, .25f, .82f));
        fire.name = "Fire button";
        fire.AddComponent<MobileControlButton>().jumpButton = false;
        CreateText(fire.transform, "FIRE", Vector2.zero, new Vector2(140, 130), 36, Color.white);

        // The authored HUD already has a Pause button (PauseButton component); only add a fallback if it is missing.
        if (FindFirstObjectByType<PauseButton>() == null)
        {
            var pause = MakePanel(root.transform, new Vector2(1, 1), new Vector2(-170, -150), new Vector2(130, 130), new Color(.08f, .16f, .18f, .82f));
            pause.name = "Pause button";
            pause.AddComponent<Button>().onClick.AddListener(() => { if (GameManager.Instance != null) GameManager.Instance.TogglePause(); });
            CreateText(pause.transform, "II", Vector2.zero, new Vector2(120, 120), 48, Color.white);
        }
    }

    void EnsureSafeAreas()
    {
        var canvases = FindObjectsByType<Canvas>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var canvas in canvases) EnsureSafeArea(canvas);
    }

    // Backgrounds and decorative strips (stretched images with no button and no children) stay full screen.
    static bool IsFullScreenArt(Transform child)
    {
        var rect = child as RectTransform;
        if (rect == null || child.childCount > 0) return false;
        if (child.GetComponent<Image>() == null || child.GetComponent<Button>() != null) return false;
        return Mathf.Approximately(rect.anchorMin.x, 0f) && Mathf.Approximately(rect.anchorMax.x, 1f);
    }

    Transform EnsureSafeArea(Canvas canvas)
    {
        var existing = canvas.transform.Find("SafeArea");
        if (existing != null)
        {
            var existingComponent = existing.GetComponent<SafeArea>();
            if (existingComponent == null) existingComponent = existing.gameObject.AddComponent<SafeArea>();
            existingComponent.RefreshNow();
            return existing;
        }

        var safeObject = new GameObject("SafeArea", typeof(RectTransform));
        safeObject.transform.SetParent(canvas.transform, false);
        var safeRect = safeObject.GetComponent<RectTransform>();
        safeRect.anchorMin = Vector2.zero;
        safeRect.anchorMax = Vector2.one;
        safeRect.offsetMin = Vector2.zero;
        safeRect.offsetMax = Vector2.zero;

        // Move children in their original order (front to back) so the draw order is preserved.
        var children = new System.Collections.Generic.List<Transform>();
        for (int i = 0; i < canvas.transform.childCount; i++)
        {
            var child = canvas.transform.GetChild(i);
            if (child != safeObject.transform && !IsFullScreenArt(child)) children.Add(child);
        }
        foreach (var child in children) child.SetParent(safeObject.transform, false);

        var component = safeObject.AddComponent<SafeArea>();
        component.RefreshNow();
        return safeObject.transform;
    }

    void EnsureEventSystem()
    {
        var es = EventSystem.current;
        if (es == null) es = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
        var old = es.GetComponent<StandaloneInputModule>(); if (old != null) Destroy(old);
        var module = es.GetComponent<InputSystemUIInputModule>(); if (module == null) module = es.gameObject.AddComponent<InputSystemUIInputModule>();
        module.AssignDefaultActions();
    }

    GameObject MakePanel(Transform parent, Vector2 anchor, Vector2 position, Vector2 size, Color color)
    {
        var go = new GameObject("Panel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image)); go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>(); r.anchorMin = r.anchorMax = anchor; r.anchoredPosition = position; r.sizeDelta = size;
        go.GetComponent<Image>().color = color;
        return go;
    }

    GameObject MakeFullPanel(Transform parent, Color color)
    {
        var panel = MakePanel(parent, Vector2.zero, Vector2.zero, Vector2.zero, color);
        var rect = panel.GetComponent<RectTransform>(); rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one; rect.offsetMin = Vector2.zero; rect.offsetMax = Vector2.zero;
        return panel;
    }

    Text CreateText(Transform parent, string text, Vector2 position, Vector2 size, int fontSize, Color color)
    {
        var go = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text)); go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>(); r.anchorMin = r.anchorMax = new Vector2(.5f, .5f); r.anchoredPosition = position; r.sizeDelta = size;
        var component = go.GetComponent<Text>(); component.text = text; component.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); component.fontSize = Mathf.Max(fontSize, 36); component.fontStyle = FontStyle.Bold; component.color = color; component.alignment = TextAnchor.MiddleCenter; component.horizontalOverflow = HorizontalWrapMode.Wrap; component.verticalOverflow = VerticalWrapMode.Overflow;
        return component;
    }

    void CreateButton(Transform parent, string label, Vector2 position, Vector2 size, UnityEngine.Events.UnityAction action)
    {
        size = new Vector2(Mathf.Max(size.x, 520f), Mathf.Max(size.y, 130f));   // thumb-sized: >= 120 reference px
        var buttonObject = MakePanel(parent, new Vector2(.5f, .5f), position, size, new Color(.82f, .39f, .19f));
        buttonObject.AddComponent<Button>().onClick.AddListener(action);
        CreateText(buttonObject.transform, label, Vector2.zero, size, 44, Color.white);
    }

    void InstallUiActions()
    {
        var es = EventSystem.current;
        if (es == null) return;
        var old = es.GetComponent<StandaloneInputModule>(); if (old != null) Destroy(old);
        var module = es.GetComponent<InputSystemUIInputModule>(); if (module == null) module = es.gameObject.AddComponent<InputSystemUIInputModule>();
        module.AssignDefaultActions();
    }

    static Sprite TileSprite { get { if (tileSprite == null) tileSprite = MakeSprite(new Color(.27f, .48f, .34f), 16, 16); return tileSprite; } }
    static Sprite JoystickKnobSprite { get { if (joystickKnobSprite == null) joystickKnobSprite = MakeJoystickKnobSprite(); return joystickKnobSprite; } }
    static Sprite HeroSprite { get { if (heroSprite == null) heroSprite = MakeSprite(new Color(.95f, .62f, .35f), 18, 24); return heroSprite; } }
    static Sprite BossSprite { get { if (bossSprite == null) bossSprite = MakeSprite(new Color(.64f, .46f, .79f), 28, 32); return bossSprite; } }
    static Sprite OrbSprite { get { if (orbSprite == null) orbSprite = MakeSprite(new Color(1f, .82f, .35f), 8, 8); return orbSprite; } }
    static Sprite SpikeSprite { get { if (spikeSprite == null) spikeSprite = MakeSpikeSprite(); return spikeSprite; } }

    static Sprite MakeSpikeSprite()
    {
        const int width = 16, height = 12;
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false); texture.filterMode = FilterMode.Point;
        var pixels = new Color[width * height];
        for (int y = 0; y < height; y++)
        {
            int half = Mathf.RoundToInt((height - 1 - y) * .72f);
            for (int x = width / 2 - half; x <= width / 2 + half; x++)
            {
                if (x < 0 || x >= width) continue;
                bool edge = x == width / 2 - half || x == width / 2 + half || y == 0;
                pixels[y * width + x] = edge ? new Color(.12f, .15f, .16f) : Color.white;
            }
        }
        texture.SetPixels(pixels); texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(.5f, .5f), 16f);
    }

    static Sprite MakeJoystickKnobSprite()
    {
        const int size = 64;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        texture.wrapMode = TextureWrapMode.Clamp;
        var pixels = new Color[size * size];
        Vector2 center = new Vector2((size - 1) * .5f, (size - 1) * .5f);
        float outerRadius = size * .48f;
        float innerRadius = size * .40f;
        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                if (distance <= innerRadius) pixels[y * size + x] = Color.white;
                else if (distance <= outerRadius) pixels[y * size + x] = new Color(.07f, .11f, .12f, 1f);
                else pixels[y * size + x] = Color.clear;
            }
        texture.SetPixels(pixels);
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(.5f, .5f), size);
    }

    static Sprite MakeSprite(Color color, int width, int height)
    {
        var texture = new Texture2D(width, height, TextureFormat.RGBA32, false); texture.filterMode = FilterMode.Point;
        var pixels = new Color[width * height];
        for (int y = 0; y < height; y++) for (int x = 0; x < width; x++)
        {
            bool edge = x == 0 || y == 0 || x == width - 1 || y == height - 1;
            pixels[y * width + x] = edge ? new Color(.07f, .11f, .12f) : color;
        }
        texture.SetPixels(pixels); texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(.5f, .5f), 16f);
    }
}