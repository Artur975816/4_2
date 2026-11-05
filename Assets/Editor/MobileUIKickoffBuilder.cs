#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

/// <summary>
/// Assignment 8-9 (Mobile Game Kickoff): builds the three authored UI screens
/// (Main Menu, HUD, Pause) plus Win / Game Over panels directly into the scenes.
///
/// Rules this tool follows:
///  - Canvas Scaler: Scale With Screen Size, 1920 x 1080, Match = 1 (landscape)
///  - every element is anchored to a corner / edge / center; full-screen panels are stretched
///  - buttons and text live inside a SafeArea panel; background art stays full screen
///  - every button is at least 120 x 120 reference px, text is at least 36 px
///
/// Run it from the menu:  Tools > Moon Garden > Build Mobile UI (Assignment 8-9)
/// </summary>
public static class MobileUIKickoffBuilder
{
    const float MinTouch = 130f;      // >= 120 px in the 1920x1080 reference
    const int TextMin = 40;           // >= 36 px
    static readonly Vector2 Reference = new Vector2(1920, 1080);

    static readonly Color Ink = new Color(.04f, .09f, .11f, 1f);
    static readonly Color Dim = new Color(.03f, .07f, .09f, .93f);
    static readonly Color Plate = new Color(.03f, .07f, .09f, .62f);
    static readonly Color Accent = new Color(.82f, .39f, .19f, 1f);
    static readonly Color AccentDark = new Color(.20f, .34f, .30f, 1f);
    static readonly Color Gold = new Color(1f, .82f, .52f, 1f);

    static readonly string[] Levels = { "Level1", "Level2", "Level3" };

    // ---------------------------------------------------------------- one-time prompt

    const string PromptKey = "MoonGarden.MobileUI.Prompted.v2";

    [InitializeOnLoadMethod]
    static void OfferOnce()
    {
        if (SessionState.GetBool(PromptKey, false) || EditorPrefs.GetBool(Application.dataPath + PromptKey, false)) return;
        SessionState.SetBool(PromptKey, true);
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            EditorPrefs.SetBool(Application.dataPath + PromptKey, true);
            if (EditorUtility.DisplayDialog("Moon Garden mobile UI",
                    "Build the large mobile UI (Main Menu, HUD, Pause, Win, Game Over) into MainMenu and Level1-3 now?\n\nYou can also run it later from Tools > Moon Garden > Build Mobile UI.",
                    "Build now", "Later"))
                BuildAll();
        };
    }

    // ---------------------------------------------------------------- menu entry

    [MenuItem("Tools/Moon Garden/Build Mobile UI (Assignment 8-9)")]
    public static void BuildAll()
    {
        if (!EditorUtility.DisplayDialog("Build Mobile UI",
                "This rebuilds the Main Menu, HUD, Pause, Win and Game Over screens in MainMenu, Level1, Level2 and Level3.\n\nContinue?",
                "Build", "Cancel")) return;
        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

        BuildScene("Assets/Scenes/MainMenu.unity", BuildMainMenu);
        foreach (var level in Levels) BuildScene("Assets/Scenes/" + level + ".unity", BuildLevelUI);

        AssetDatabase.SaveAssets();
        Debug.Log("Mobile UI built: MainMenu + HUD/Pause in Level1-3. Now test in Device Simulator (16:9, notch phone, 4:3 tablet).");
    }

    static void BuildScene(string path, System.Action build)
    {
        var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
        build();
        EnsureEventSystem();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
    }

    // ---------------------------------------------------------------- Main Menu

    static void BuildMainMenu()
    {
        foreach (var name in new[] { "Authored MainMenu UI", "Main Menu UI" })
        {
            var old = GameObject.Find(name);
            if (old != null) Object.DestroyImmediate(old);
        }

        var root = new GameObject("Main Menu UI");
        var menu = new GameObject("Menu Actions").AddComponent<MenuManager>();
        menu.transform.SetParent(root.transform, false);

        var canvas = NewCanvas("Main Menu Canvas", root.transform);
        var background = NewImage("Background", canvas.transform, Ink);   // full screen art, outside SafeArea
        Stretch(background.rectTransform);
        var strip = NewImage("Top Strip", canvas.transform, new Color(.30f, .85f, .72f, 1f));
        var stripRect = strip.rectTransform;           // stretched along the top edge
        stripRect.anchorMin = new Vector2(0, 1); stripRect.anchorMax = new Vector2(1, 1);
        stripRect.pivot = new Vector2(.5f, 1f);
        stripRect.anchoredPosition = Vector2.zero; stripRect.sizeDelta = new Vector2(0, 14);

        var safe = NewSafeArea(canvas.transform);

        NewText("Title", safe, "MOON GARDEN", 120, Gold, new Vector2(.5f, 1f), new Vector2(0, -150), new Vector2(1400, 150));
        NewText("Subtitle", safe, "2D PLATFORM ADVENTURE", 48, Color.white, new Vector2(.5f, 1f), new Vector2(0, -310), new Vector2(1000, 70));

        var play = NewButton("Play Button", safe, "PLAY", 68, Accent, new Vector2(.5f, .5f), new Vector2(0, 30), new Vector2(620, 150));
        var settings = NewButton("Settings Button", safe, "SETTINGS", 56, AccentDark, new Vector2(.5f, .5f), new Vector2(0, -150), new Vector2(620, 140));
        var quit = NewButton("Quit Button", safe, "QUIT", 56, AccentDark, new Vector2(.5f, .5f), new Vector2(0, -320), new Vector2(620, 140));
        NewText("Controls Hint", safe, "Touch: stick = move  |  JUMP  |  FIRE", 38, new Color(.85f, .95f, .92f, 1f),
            new Vector2(.5f, 0f), new Vector2(0, 60), new Vector2(1500, 60));

        UnityEventTools.AddPersistentListener(play.onClick, new UnityAction(menu.Play));
        UnityEventTools.AddPersistentListener(settings.onClick, new UnityAction(menu.OpenSettings));
        UnityEventTools.AddPersistentListener(quit.onClick, new UnityAction(menu.Quit));
    }

    // ---------------------------------------------------------------- Level UI (HUD + Pause + Win + Game Over)

    static void BuildLevelUI()
    {
        var ui = Object.FindFirstObjectByType<UIManager>(FindObjectsInactive.Include);
        if (ui == null) ui = new GameObject("UIManager").AddComponent<UIManager>();

        // remove old authored result panels / legacy HUD labels (Level3 had its own small ones)
        DestroyIfSet(ref ui.pausePanel);
        DestroyIfSet(ref ui.winPanel);
        DestroyIfSet(ref ui.gameOverPanel);
        if (ui.coinsLegacyText != null) { Object.DestroyImmediate(ui.coinsLegacyText.gameObject); ui.coinsLegacyText = null; }
        if (ui.livesLegacyText != null) { Object.DestroyImmediate(ui.livesLegacyText.gameObject); ui.livesLegacyText = null; }

        var canvasObject = GameObject.Find("GameHUD");
        if (canvasObject == null) canvasObject = new GameObject("GameHUD");
        var canvas = SetupCanvas(canvasObject);

        // reuse the existing TMP labels if the scene has them (keeps their font asset)
        TMP_Text coins = FindTmp(canvas.transform, "CoinsText");
        TMP_Text lives = FindTmp(canvas.transform, "LivesText");
        TMP_FontAsset font = coins != null ? coins.font : (lives != null ? lives.font : TMP_Settings.defaultFontAsset);

        var oldSafe = canvas.transform.Find("SafeArea");
        if (oldSafe != null)
        {
            if (coins != null) coins.transform.SetParent(canvas.transform, false);
            if (lives != null) lives.transform.SetParent(canvas.transform, false);
            // keep runtime-made "Mobile controls" only when they exist already: they are rebuilt at runtime
            Object.DestroyImmediate(oldSafe.gameObject);
        }

        // ---------- HUD (inside SafeArea)
        var safe = NewSafeArea(canvas.transform);

        var hudPlate = NewImage("HUD Plate", safe, Plate);
        PlaceAt(hudPlate.rectTransform, new Vector2(0, 1), new Vector2(80, -60), new Vector2(1000, 210));

        if (lives == null) lives = NewTmp("LivesText", hudPlate.transform, "Lives: 3", font);
        if (coins == null) coins = NewTmp("CoinsText", hudPlate.transform, "Coins: 0", font);
        lives.transform.SetParent(hudPlate.transform, false);
        coins.transform.SetParent(hudPlate.transform, false);
        StyleHudLabel(lives, font, new Vector2(30, -18));
        StyleHudLabel(coins, font, new Vector2(30, -110));
        ui.livesText = lives;
        ui.coinsText = coins;

        var pause = NewButton("Pause Button", safe, "II", 80, new Color(.08f, .16f, .18f, .88f), new Vector2(1, 1), new Vector2(-90, -70), new Vector2(150, 150));
        var pauseButton = pause.gameObject.AddComponent<PauseButton>();
        UnityEventTools.AddPersistentListener(pause.onClick, new UnityAction(pauseButton.TogglePause));

        // ---------- Pause screen (full-screen panel + its own SafeArea)
        var pausePanel = NewPanel("Pause Panel", canvas.transform, Dim);
        var pauseSafe = NewSafeArea(pausePanel.transform);
        NewText("Pause Title", pauseSafe, "PAUSED", 110, Color.white, new Vector2(.5f, .5f), new Vector2(0, 330), new Vector2(1000, 140));
        var resume = NewButton("Resume Button", pauseSafe, "RESUME", 60, Accent, new Vector2(.5f, .5f), new Vector2(0, 130), new Vector2(640, 140));
        var restart = NewButton("Restart Button", pauseSafe, "RESTART", 60, AccentDark, new Vector2(.5f, .5f), new Vector2(0, -50), new Vector2(640, 140));
        var toMenu = NewButton("Menu Button", pauseSafe, "MENU", 60, AccentDark, new Vector2(.5f, .5f), new Vector2(0, -230), new Vector2(640, 140));
        UnityEventTools.AddPersistentListener(resume.onClick, new UnityAction(ui.Resume));
        UnityEventTools.AddPersistentListener(restart.onClick, new UnityAction(ui.RestartLevel));
        UnityEventTools.AddStringPersistentListener(toMenu.onClick, new UnityAction<string>(ui.LoadScene), "MainMenu");
        pausePanel.SetActive(false);
        ui.pausePanel = pausePanel;

        // ---------- Win screen
        var winPanel = NewPanel("Win Panel", canvas.transform, Dim);
        var winSafe = NewSafeArea(winPanel.transform);
        NewTmpText("Win Title", winSafe, "YOU WIN!", 110, Gold, new Vector2(.5f, .5f), new Vector2(0, 250), new Vector2(1200, 140), font);
        NewTmpText("Win Subtitle", winSafe, "Back to level select...", 48, Color.white, new Vector2(.5f, .5f), new Vector2(0, 130), new Vector2(1200, 80), font);
        var toSelect = NewButton("Level Select Button", winSafe, "LEVELS", 60, Accent, new Vector2(.5f, .5f), new Vector2(0, -60), new Vector2(640, 140));
        UnityEventTools.AddStringPersistentListener(toSelect.onClick, new UnityAction<string>(ui.LoadScene), "LevelSelect");
        winPanel.SetActive(false);
        ui.winPanel = winPanel;

        // ---------- Game over screen
        var overPanel = NewPanel("Game Over Panel", canvas.transform, Dim);
        var overSafe = NewSafeArea(overPanel.transform);
        NewText("Game Over Title", overSafe, "TRY AGAIN", 110, Color.white, new Vector2(.5f, .5f), new Vector2(0, 250), new Vector2(1200, 140));
        var retry = NewButton("Retry Button", overSafe, "RESTART", 60, Accent, new Vector2(.5f, .5f), new Vector2(0, 50), new Vector2(640, 140));
        var overMenu = NewButton("Over Menu Button", overSafe, "MENU", 60, AccentDark, new Vector2(.5f, .5f), new Vector2(0, -130), new Vector2(640, 140));
        UnityEventTools.AddPersistentListener(retry.onClick, new UnityAction(ui.RestartLevel));
        UnityEventTools.AddStringPersistentListener(overMenu.onClick, new UnityAction<string>(ui.LoadScene), "MainMenu");
        overPanel.SetActive(false);
        ui.gameOverPanel = overPanel;

        // the panels must be the last (top-most) children of the canvas
        pausePanel.transform.SetAsLastSibling();
        winPanel.transform.SetAsLastSibling();
        overPanel.transform.SetAsLastSibling();
        EditorUtility.SetDirty(ui);
    }

    // ---------------------------------------------------------------- helpers: canvas / safe area

    static Canvas SetupCanvas(GameObject go)
    {
        var canvas = go.GetComponent<Canvas>(); if (canvas == null) canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = go.GetComponent<CanvasScaler>(); if (scaler == null) scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = Reference;
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;
        scaler.referencePixelsPerUnit = 100f;
        if (go.GetComponent<GraphicRaycaster>() == null) go.AddComponent<GraphicRaycaster>();
        return canvas;
    }

    static Canvas NewCanvas(string name, Transform parent)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return SetupCanvas(go);
    }

    static Transform NewSafeArea(Transform parent)
    {
        var go = new GameObject("SafeArea", typeof(RectTransform));
        go.transform.SetParent(parent, false);
        Stretch(go.GetComponent<RectTransform>());
        go.AddComponent<SafeArea>();
        return go.transform;
    }

    static void EnsureEventSystem()
    {
        var es = Object.FindFirstObjectByType<EventSystem>();
        if (es == null) es = new GameObject("EventSystem", typeof(EventSystem)).GetComponent<EventSystem>();
        var old = es.GetComponent<StandaloneInputModule>();
        if (old != null) Object.DestroyImmediate(old);
        if (es.GetComponent<InputSystemUIInputModule>() == null)
            es.gameObject.AddComponent<InputSystemUIInputModule>().AssignDefaultActions();
    }

    // ---------------------------------------------------------------- helpers: rects

    static void Stretch(RectTransform r)
    {
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = Vector2.zero; r.offsetMax = Vector2.zero;
    }

    /// <summary>Anchor and pivot are the same corner / edge / center point; position is an offset from it.</summary>
    static void PlaceAt(RectTransform r, Vector2 anchor, Vector2 position, Vector2 size)
    {
        r.anchorMin = anchor; r.anchorMax = anchor; r.pivot = anchor;
        r.anchoredPosition = position; r.sizeDelta = size;
    }

    // ---------------------------------------------------------------- helpers: widgets

    static Sprite RoundedSprite() => AssetDatabase.GetBuiltinExtraResource<Sprite>("UI/Skin/UISprite.psd");

    static Image NewImage(string name, Transform parent, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    static GameObject NewPanel(string name, Transform parent, Color color)
    {
        var image = NewImage(name, parent, color);
        image.raycastTarget = true;   // blocks clicks to the game while the panel is open
        Stretch(image.rectTransform);
        return image.gameObject;
    }

    static TMP_Text NewTmp(string name, Transform parent, string text, TMP_FontAsset font)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<TextMeshProUGUI>();
        t.text = text;
        if (font != null) t.font = font;
        t.raycastTarget = false;
        return t;
    }

    static void StyleHudLabel(TMP_Text t, TMP_FontAsset font, Vector2 position)
    {
        var r = t.rectTransform;
        r.anchorMin = r.anchorMax = new Vector2(0, 1);
        r.pivot = new Vector2(0, 1);
        r.anchoredPosition = position;
        r.sizeDelta = new Vector2(940, 84);
        r.localScale = Vector3.one;
        t.alignment = TextAlignmentOptions.MidlineLeft;
        t.fontSize = 54;
        t.fontStyle = FontStyles.Bold;
        t.color = Color.white;
        t.raycastTarget = false;
        if (font != null) t.font = font;
    }

    static void NewText(string name, Transform parent, string text, int size, Color color, Vector2 anchor, Vector2 position, Vector2 box)
    {
        NewTmpText(name, parent, text, size, color, anchor, position, box, TMP_Settings.defaultFontAsset);
    }

    static TMP_Text NewTmpText(string name, Transform parent, string text, int size, Color color, Vector2 anchor, Vector2 position, Vector2 box, TMP_FontAsset font)
    {
        var t = NewTmp(name, parent, text, font);
        PlaceAt(t.rectTransform, anchor, position, box);
        t.fontSize = Mathf.Max(size, TextMin);
        t.fontStyle = FontStyles.Bold;
        t.color = color;
        t.alignment = TextAlignmentOptions.Center;
        return t;
    }

    static Button NewButton(string name, Transform parent, string label, int fontSize, Color color, Vector2 anchor, Vector2 position, Vector2 size)
    {
        size.x = Mathf.Max(size.x, MinTouch);
        size.y = Mathf.Max(size.y, MinTouch);
        var image = NewImage(name, parent, color);
        image.raycastTarget = true;
        image.sprite = RoundedSprite();
        image.type = Image.Type.Sliced;
        PlaceAt(image.rectTransform, anchor, position, size);

        var button = image.gameObject.AddComponent<Button>();
        button.targetGraphic = image;

        var text = NewTmp("Label", image.transform, label, TMP_Settings.defaultFontAsset);
        Stretch(text.rectTransform);
        text.fontSize = Mathf.Max(fontSize, TextMin);
        text.fontStyle = FontStyles.Bold;
        text.color = Color.white;
        text.alignment = TextAlignmentOptions.Center;
        return button;
    }

    static TMP_Text FindTmp(Transform root, string name)
    {
        foreach (var t in root.GetComponentsInChildren<TMP_Text>(true))
            if (t.name == name) return t;
        return null;
    }

    static void DestroyIfSet(ref GameObject go)
    {
        if (go != null) Object.DestroyImmediate(go);
        go = null;
    }
}
#endif
