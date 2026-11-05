using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;
using TMPro;

/// <summary>One-time authoring tools for scene geometry and the hand-drawn character set.</summary>
public static class ProjectRepairTools
{
    const string OutputRoot = "Assets/Animations/GeneratedPlayer";
    const string ControllerPath = "Assets/Resources/Player_Redrawn.controller";
    const string PanoramaPath = "Assets/Art/Backgrounds/Level2_Monolith.png";
    static Sprite platformSprite;

    public static void BuildComplete()
    {
        BuildPlayerAnimations();
        BakeRoutes();
        BakeMenus();
        BakeLevelThree();
        BakeLevelTwoBackdrop();
    }

    [MenuItem("Tools/Project Repair/Create Main Menu, Level Select and Settings")]
    public static void BakeMenus()
    {
        BakeMenuScene("MainMenu", BuildMainMenu);
        BakeMenuScene("LevelSelect", BuildLevelSelect);
        BakeMenuScene("Settings", BuildSettings);
        AssetDatabase.SaveAssets();
    }

    static void BakeMenuScene(string sceneName, System.Action<Transform> build)
    {
        Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/" + sceneName + ".unity", OpenSceneMode.Single);
        var old = GameObject.Find("Authored " + sceneName + " UI");
        if (old != null) Object.DestroyImmediate(old);
        var root = new GameObject("Authored " + sceneName + " UI");
        var canvasObject = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasObject.transform.SetParent(root.transform, false);
        var canvas = canvasObject.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvasObject.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 1f;
        build(canvas.transform);
        if (root.GetComponentInChildren<EventSystem>(true) == null)
        {
            var es = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            es.transform.SetParent(root.transform, false);
        }
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
    }

    static void BuildMainMenu(Transform canvas)
    {
        MenuManager manager = new GameObject("Menu Actions").AddComponent<MenuManager>();
        manager.transform.SetParent(canvas, false);
        MenuCard(canvas, "MOON GARDEN", "2D PLATFORM ADVENTURE", new Color(.035f,.08f,.12f));
        MenuButton(canvas, "START GAME", new Vector2(0,55), new Vector2(360,76), manager.Play);
        MenuButton(canvas, "SETTINGS", new Vector2(0,-40), new Vector2(360,68), manager.OpenSettings);
        MenuButton(canvas, "QUIT", new Vector2(0,-125), new Vector2(360,68), manager.Quit);
        MenuText(canvas,"A/D or arrow keys: move    Space: jump    X: fire",new Vector2(0,-355),new Vector2(1000,42),20,new Color(.78f,.88f,.82f));
    }
    static void BuildLevelSelect(Transform canvas)
    {
        MenuManager manager = new GameObject("Menu Actions").AddComponent<MenuManager>(); manager.transform.SetParent(canvas,false);
        MenuCard(canvas,"SELECT LEVEL","Collect the coins, open the portal, and continue",new Color(.035f,.08f,.12f));
        string[] labels={"1 - FOREST TRAIL","2 - MOON TEMPLE RUINS","3 - LUNAR WARDEN"};
        for(int i=0;i<3;i++)
        {
            var button=MenuButton(canvas,labels[i],new Vector2(0,75-i*92),new Vector2(500,72),null);
            var selector=button.gameObject.AddComponent<LevelSelector>(); selector.levelNumber=i+1; selector.levelButton=button;
            UnityEventTools.AddPersistentListener(button.onClick,selector.LoadLevel);
        }
        MenuButton(canvas,"MAIN MENU",new Vector2(0,-245),new Vector2(360,66),manager.BackToMenu);
    }
    static void BuildSettings(Transform canvas)
    {
        MenuManager manager = new GameObject("Menu Actions").AddComponent<MenuManager>(); manager.transform.SetParent(canvas,false);
        MenuCard(canvas,"SETTINGS","Volume settings are saved automatically",new Color(.035f,.08f,.12f));
        var settings=new GameObject("Settings Actions").AddComponent<SettingsManager>(); settings.transform.SetParent(canvas,false);
        MenuText(canvas,"Music",new Vector2(-240,95),new Vector2(220,54),26,Color.white);
        MenuText(canvas,"Effects",new Vector2(-240,-5),new Vector2(220,54),26,Color.white);
        settings.musicSlider=MenuSlider(canvas,new Vector2(110,95)); settings.sfxSlider=MenuSlider(canvas,new Vector2(110,-5));
        MenuButton(canvas,"BACK",new Vector2(0,-150),new Vector2(360,72),manager.BackToMenu);
    }
    [MenuItem("Tools/Project Repair/Build Level 3 Boss Arena")]
    public static void BakeLevelThree()
    {
        platformSprite=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Sprites/Environment_Tiles.png").OfType<Sprite>().FirstOrDefault();
        Scene scene=EditorSceneManager.OpenScene("Assets/Scenes/Level3.unity",OpenSceneMode.Single);
        var old=GameObject.Find("Authored Boss Arena"); if(old!=null) Object.DestroyImmediate(old);
        var root=new GameObject("Authored Boss Arena");
        var camera=Camera.main;
        if(camera==null) camera=Object.FindFirstObjectByType<Camera>();
        if(camera!=null){camera.tag="MainCamera";camera.orthographic=true;camera.orthographicSize=5.4f;camera.transform.position=new Vector3(0,0,-10);camera.backgroundColor=new Color(.035f,.07f,.12f);}
        MakePlatform(root.transform,"Moonstone arena floor",new Vector2(0,-3f),new Vector2(18f,1f),new Color(.25f,.42f,.39f));
        MakePlatform(root.transform,"West arena wall",new Vector2(-9.1f,1.5f),new Vector2(.55f,9f),new Color(.18f,.32f,.32f));
        MakePlatform(root.transform,"East arena wall",new Vector2(9.1f,1.5f),new Vector2(.55f,9f),new Color(.18f,.32f,.32f));
        MakePlatform(root.transform,"West moonstone step",new Vector2(-4.5f,-1.45f),new Vector2(3.4f,.38f),new Color(.34f,.54f,.49f));
        MakePlatform(root.transform,"East moonstone step",new Vector2(4.3f,-1.45f),new Vector2(3.4f,.38f),new Color(.34f,.54f,.49f));
        var playerSprite=AssetDatabase.LoadAssetAtPath<Sprite>(OutputRoot+"/Idle_0.png");
        var player=new GameObject("Player"); player.transform.SetParent(root.transform); player.transform.position=new Vector3(-6f,-1.9f,0); player.tag="Player";
        var ps=player.AddComponent<SpriteRenderer>(); ps.sprite=playerSprite; ps.sortingOrder=5;
        var pc=player.AddComponent<CapsuleCollider2D>(); pc.size=new Vector2(.72f,1.15f);
        var prb=player.AddComponent<Rigidbody2D>(); prb.gravityScale=1.5f; prb.freezeRotation=true; prb.collisionDetectionMode=CollisionDetectionMode2D.Continuous; prb.interpolation=RigidbodyInterpolation2D.Interpolate;
        var movement=player.AddComponent<PlayerMovement>(); movement.speed=5.5f; movement.jumpHeight=2.5f; movement.groundLayer=(1<<0)|(1<<6);
        var animator=player.AddComponent<Animator>(); animator.runtimeAnimatorController=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath); animator.updateMode=AnimatorUpdateMode.UnscaledTime;
        player.AddComponent<PlayerAnimator>(); player.AddComponent<Assignment6.Health>(); player.AddComponent<Shooter1>();
        var enemySprites=AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Sprites/Enemy_Spritesheet.png").OfType<Sprite>().ToArray();
        Sprite bossSprite=enemySprites.Length>0?enemySprites[0]:platformSprite;
        var boss=new GameObject("Moon Warden"); boss.transform.SetParent(root.transform); boss.transform.position=new Vector3(5.6f,-1.8f,0); boss.transform.localScale=new Vector3(1.8f,1.8f,1);
        var bs=boss.AddComponent<SpriteRenderer>(); bs.sprite=bossSprite; bs.color=new Color(.84f,.73f,.94f); bs.sortingOrder=4;
        var bc=boss.AddComponent<BoxCollider2D>(); bc.isTrigger=true; bc.size=new Vector2(1.25f,1.9f);
        var bossController=boss.AddComponent<BossController>(); bossController.maxHealth=12; bossController.arenaLeft=-8f; bossController.arenaRight=8f; bossController.target=player.transform;
        if(camera!=null){var follow=camera.GetComponent<CameraFollow>();if(follow==null)follow=camera.gameObject.AddComponent<CameraFollow>();follow.target=player.transform;follow.offset=new Vector3(0,1f,-10);follow.smoothSpeed=4f;}

        var canvasObject=new GameObject("Boss Arena UI",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster)); canvasObject.transform.SetParent(root.transform,false);
        var canvas=canvasObject.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;var scaler=canvasObject.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1920,1080);scaler.matchWidthOrHeight=1f;
        var bossPanel=MenuPanel(canvas.transform,new Vector2(.5f,1f),new Vector2(0,-55),new Vector2(480,42),new Color(.025f,.06f,.09f,.9f));
        var sliderObject=new GameObject("Boss health",typeof(RectTransform),typeof(Slider));sliderObject.transform.SetParent(bossPanel.transform,false);
        var srct=sliderObject.GetComponent<RectTransform>();srct.anchorMin=Vector2.zero;srct.anchorMax=Vector2.one;srct.offsetMin=new Vector2(6,6);srct.offsetMax=new Vector2(-6,-6);
        var fillArea=new GameObject("Fill Area",typeof(RectTransform));fillArea.transform.SetParent(sliderObject.transform,false);var far=fillArea.GetComponent<RectTransform>();far.anchorMin=Vector2.zero;far.anchorMax=Vector2.one;far.offsetMin=Vector2.zero;far.offsetMax=Vector2.zero;
        var fillObject=new GameObject("Fill",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image));fillObject.transform.SetParent(fillArea.transform,false);var fillRect=fillObject.GetComponent<RectTransform>();fillRect.anchorMin=Vector2.zero;fillRect.anchorMax=Vector2.one;fillRect.offsetMin=Vector2.zero;fillRect.offsetMax=Vector2.zero;
        var fillImage=fillObject.GetComponent<Image>();fillImage.color=new Color(.92f,.27f,.32f);fillImage.type=Image.Type.Filled;fillImage.fillMethod=Image.FillMethod.Horizontal;
        var slider=sliderObject.GetComponent<Slider>();slider.direction=Slider.Direction.LeftToRight;slider.fillRect=fillRect;slider.targetGraphic=fillImage;slider.minValue=0;slider.maxValue=bossController.maxHealth;slider.value=bossController.maxHealth;bossController.healthBar=slider;
        MenuText(canvas.transform,"MOON WARDEN",new Vector2(0,400),new Vector2(420,40),24,new Color(1f,.82f,.55f));
        var ui=new GameObject("Game UI").AddComponent<UIManager>();ui.transform.SetParent(canvas.transform,false);
        ui.coinsLegacyText=MenuText(canvas.transform,"",new Vector2(-520,400),new Vector2(440,40),22,Color.white);
        ui.coinsLegacyText.alignment=TextAnchor.MiddleLeft;
        ui.livesLegacyText=MenuText(canvas.transform,"",new Vector2(-520,365),new Vector2(360,36),19,new Color(.86f,.92f,.88f));
        ui.livesLegacyText.alignment=TextAnchor.MiddleLeft;
        ui.winPanel=MenuPanel(canvas.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(600,340),new Color(.025f,.06f,.09f,.96f));
        MenuText(ui.winPanel.transform,"VICTORY",new Vector2(0,55),new Vector2(520,90),42,new Color(1f,.82f,.52f));
        MenuText(ui.winPanel.transform,"Returning to level select...",new Vector2(0,-30),new Vector2(520,55),23,Color.white);
        ui.gameOverPanel=MenuPanel(canvas.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(580,350),new Color(.025f,.06f,.09f,.97f));
        MenuText(ui.gameOverPanel.transform,"TRY AGAIN",new Vector2(0,70),new Vector2(520,75),36,Color.white);
        MenuButton(ui.gameOverPanel.transform,"RETRY",new Vector2(0,-20),new Vector2(300,64),ui.RestartLevel);
        MenuButton(ui.gameOverPanel.transform,"LEVEL SELECT",new Vector2(0,-95),new Vector2(340,62),ui.LoadLevelSelect);
        ui.pausePanel=MenuPanel(canvas.transform,new Vector2(.5f,.5f),Vector2.zero,new Vector2(520,300),new Color(.025f,.06f,.09f,.97f));
        MenuText(ui.pausePanel.transform,"PAUSED",new Vector2(0,55),new Vector2(420,65),36,Color.white);
        MenuButton(ui.pausePanel.transform,"RESUME",new Vector2(0,-20),new Vector2(300,62),ui.Resume);
        MenuButton(ui.pausePanel.transform,"RESTART",new Vector2(0,-92),new Vector2(300,62),ui.RestartLevel);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Debug.Log("Level3 boss arena and HUD are now authored in Level3.unity.");
    }

    [MenuItem("Tools/Project Repair/Make Level 2 Background Continuous")]
    public static void BakeLevelTwoBackdrop()
    {
        Directory.CreateDirectory("Assets/Art/Backgrounds");
        WritePanorama();
        AssetDatabase.Refresh();
        var importer=AssetImporter.GetAtPath(PanoramaPath) as TextureImporter;
        if(importer!=null){importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;importer.spritePixelsPerUnit=12;importer.filterMode=FilterMode.Point;importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();}
        var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(PanoramaPath);
        Scene scene=EditorSceneManager.OpenScene("Assets/Scenes/Level2.unity",OpenSceneMode.Single);
        foreach(var old in Object.FindObjectsByType<InfiniteBackground>(FindObjectsSortMode.None))
            if(old.gameObject.scene==scene) old.gameObject.SetActive(false);
        var oldBackdrop=GameObject.Find("Level2 Continuous Monolith Backdrop");if(oldBackdrop!=null)Object.DestroyImmediate(oldBackdrop);
        var bg=new GameObject("Level2 Continuous Monolith Backdrop");bg.transform.position=new Vector3(50f,0f,8f);
        var renderer=bg.AddComponent<SpriteRenderer>();renderer.sprite=sprite;renderer.sortingOrder=-100;
        var cam=Camera.main;if(cam!=null)cam.backgroundColor=new Color(.04f,.075f,.12f);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Debug.Log("Level2 now uses one continuous panoramic background image.");
    }

    static void WritePanorama()
    {
        const int w=1536,h=512;var pixels=new Color32[w*h];
        Color32 top=new Color32(19,34,63,255),middle=new Color32(58,87,112,255),horizon=new Color32(183,133,111,255);
        for(int y=0;y<h;y++){Color32 c=y>250?Color32.Lerp(middle,top,(y-250f)/(h-250f)):Color32.Lerp(new Color32(30,50,79,255),horizon,y/250f);for(int x=0;x<w;x++)pixels[y*w+x]=c;}
        void Rect(int x0,int y0,int x1,int y1,Color32 c){for(int y=Mathf.Max(0,y0);y<=Mathf.Min(h-1,y1);y++)for(int x=Mathf.Max(0,x0);x<=Mathf.Min(w-1,x1);x++)pixels[y*w+x]=c;}
        void Disk(int cx,int cy,int r,Color32 c){for(int y=-r;y<=r;y++)for(int x=-r;x<=r;x++)if(x*x+y*y<=r*r&&cx+x>=0&&cx+x<w&&cy+y>=0&&cy+y<h)pixels[(cy+y)*w+cx+x]=c;}
        for(int i=0;i<95;i++){int x=(i*191+73)%w;int y=280+(i*97+19)%220;Disk(x,y,i%7==0?2:1,new Color32(211,218,204,190));}
        Disk(1220,385,34,new Color32(236,207,156,255));Disk(1232,396,32,new Color32(48,72,99,255));
        Color32 far=new Color32(50,68,85,255),ruin=new Color32(36,51,68,255),front=new Color32(26,40,56,255),glow=new Color32(216,143,93,255);
        for(int x=0;x<w;x++)
        {
            int ridge=120+(int)(18*Mathf.Sin(x*.013f)+13*Mathf.Sin(x*.037f)+8*Mathf.Sin(x*.071f));
            for(int y=0;y<ridge;y++)pixels[y*w+x]=far;
            int near=82+(int)(10*Mathf.Sin(x*.018f)+7*Mathf.Sin(x*.049f));
            for(int y=0;y<near;y++)pixels[y*w+x]=front;
        }
        // Distant, broken towers form one continuous skyline rather than repeated tiles.
        for(int i=0;i<24;i++)
        {
            int x=24+i*64+(i*17%23),baseY=94+(i*29%42),towerW=20+(i*11%21),towerH=58+(i*37%112);
            Rect(x,baseY,x+towerW,baseY+towerH,ruin);
            Rect(x+4,baseY+towerH-9,x+towerW-4,baseY+towerH,far);
            if(i%3==0){Rect(x-7,baseY+towerH-13,x+towerW+7,baseY+towerH-7,ruin);Rect(x+7,baseY+5,x+towerW-6,baseY+18,glow);}
            else {Rect(x+5,baseY+10,x+towerW-5,baseY+24,front);}
        }
        var tex=new Texture2D(w,h,TextureFormat.RGBA32,false);tex.filterMode=FilterMode.Point;tex.SetPixels32(pixels);tex.Apply();File.WriteAllBytes(PanoramaPath,tex.EncodeToPNG());Object.DestroyImmediate(tex);
    }

    static void MenuCard(Transform canvas,string title,string subtitle,Color color)
    {
        var bg=MenuPanel(canvas,new Vector2(.5f,.5f),Vector2.zero,new Vector2(1920,1080),color);
        bg.transform.SetAsFirstSibling();
        var strip=MenuPanel(canvas,new Vector2(.5f,1f),new Vector2(0,-24),new Vector2(1600,12),new Color(.29f,.79f,.64f)); strip.transform.SetAsFirstSibling();
        MenuText(canvas,title,new Vector2(0,310),new Vector2(1050,90),48,new Color(1f,.82f,.52f));
        MenuText(canvas,subtitle,new Vector2(0,252),new Vector2(1060,50),23,new Color(.86f,.93f,.89f));
    }

    static Button MenuButton(Transform parent,string label,Vector2 pos,Vector2 size,UnityAction action)
    {
        var go=MenuPanel(parent,new Vector2(.5f,.5f),pos,size,new Color(.16f,.37f,.37f));
        var button=go.AddComponent<Button>(); button.targetGraphic=go.GetComponent<Image>();
        var colors=button.colors; colors.normalColor=new Color(.17f,.39f,.38f); colors.highlightedColor=new Color(.25f,.59f,.5f); colors.pressedColor=new Color(.1f,.27f,.31f); colors.selectedColor=colors.highlightedColor; button.colors=colors;
        MenuText(go.transform,label,Vector2.zero,size,24,Color.white);
        if(action!=null) UnityEventTools.AddPersistentListener(button.onClick,action);
        return button;
    }

    static Text MenuText(Transform parent,string text,Vector2 pos,Vector2 size,int fontSize,Color color)
    {
        var go=new GameObject("Label",typeof(RectTransform),typeof(CanvasRenderer),typeof(Text)); go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>(); rect.anchorMin=rect.anchorMax=new Vector2(.5f,.5f); rect.anchoredPosition=pos; rect.sizeDelta=size;
        var label=go.GetComponent<Text>(); label.text=text; label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); label.fontSize=fontSize; label.fontStyle=FontStyle.Bold; label.alignment=TextAnchor.MiddleCenter; label.color=color; label.horizontalOverflow=HorizontalWrapMode.Wrap; label.verticalOverflow=VerticalWrapMode.Overflow;
        return label;
    }

    static GameObject MenuPanel(Transform parent,Vector2 anchor,Vector2 pos,Vector2 size,Color color)
    {
        var go=new GameObject("Panel",typeof(RectTransform),typeof(CanvasRenderer),typeof(Image)); go.transform.SetParent(parent,false);
        var rect=go.GetComponent<RectTransform>(); rect.anchorMin=rect.anchorMax=anchor; rect.anchoredPosition=pos; rect.sizeDelta=size; go.GetComponent<Image>().color=color; return go;
    }

    static Slider MenuSlider(Transform parent,Vector2 pos)
    {
        var go=MenuPanel(parent,new Vector2(.5f,.5f),pos,new Vector2(440,32),new Color(.13f,.2f,.23f));
        var slider=go.AddComponent<Slider>(); slider.minValue=0; slider.maxValue=1; slider.value=.7f; slider.direction=Slider.Direction.LeftToRight;
        var fill=MenuPanel(go.transform,Vector2.zero,Vector2.zero,Vector2.zero,new Color(.29f,.79f,.64f)); var fr=fill.GetComponent<RectTransform>(); fr.anchorMin=Vector2.zero; fr.anchorMax=Vector2.one; fr.offsetMin=Vector2.zero; fr.offsetMax=Vector2.zero;
        slider.fillRect=fr; slider.targetGraphic=go.GetComponent<Image>(); return slider;
    }

    [MenuItem("Tools/Project Repair/Bake Level 1 and 2 Routes")]
    public static void BakeRoutes()
    {
        platformSprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Sprites/Environment_Tiles.png").OfType<Sprite>().FirstOrDefault();
        foreach (string sceneName in new[] { "Level1", "Level2" })
        {
            string path = "Assets/Scenes/" + sceneName + ".unity";
            Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            KeepSingleGameplayCamera(scene);
            GameObject oldRoot = GameObject.Find("Authored Parkour Route");
            if (oldRoot != null) Object.DestroyImmediate(oldRoot);
            GameObject root = new GameObject("Authored Parkour Route");
            Vector2[] points = sceneName == "Level1" ? ForestRoute() : RuinsRoute();
            Color baseTint = sceneName == "Level1" ? new Color(.42f, .72f, .40f) : new Color(.43f, .67f, .69f);
            Color darkTint = sceneName == "Level1" ? new Color(.18f, .39f, .25f) : new Color(.22f, .39f, .49f);
            for (int i = 0; i < points.Length; i++)
            {
                float width = (i % 5 == 2) ? 2.05f : (i % 4 == 0 ? 3.1f : 2.55f);
                GameObject platform = MakePlatform(root.transform, $"{sceneName} • паркур {i + 1:00}", points[i], new Vector2(width, .48f), (i % 2 == 0 ? baseTint : darkTint));
                if (i == 6 || i == 15)
                {
                    MovingPlatform mover = platform.AddComponent<MovingPlatform>();
                    mover.endpointOffset = i == 6 ? new Vector2(2.2f, 0f) : new Vector2(0f, 1.65f);
                    mover.speed = 1.15f;
                    mover.endpointPause = .8f;
                }
                if ((sceneName == "Level1" && (i == 10 || i == 18)) || (sceneName == "Level2" && (i == 8 || i == 17)))
                    MakeHazard(root.transform, points[i] + Vector2.up * .42f, sceneName == "Level1" ? new Color(.91f, .34f, .31f) : new Color(.91f, .52f, .29f));
                if (i == 8 || i == 16) MakeCheckpoint(root.transform, points[i] + Vector2.up * 1.15f);
                if (i == 3 || i == 7 || i == 12 || i == 17 || i == 21)
                    MakeCoin(root.transform, points[i] + Vector2.up * 1.1f);
            }
            MakePlatform(root.transform, "Финишная площадка", new Vector2(99.5f, points[points.Length - 1].y), new Vector2(4.2f, .6f), baseTint);
            GameObject exit = GameObject.Find("Exit");
            if (exit != null)
            {
                Undo.RecordObject(exit.transform, "Place level portal");
                exit.transform.position = new Vector3(100f, points[points.Length - 1].y + .85f, 0f);
                exit.transform.localScale = new Vector3(1.45f, 2.2f, 1f);
                var sr = exit.GetComponent<SpriteRenderer>();
                if (sr != null) sr.color = new Color(.28f, .92f, .63f);
                var collider = exit.GetComponent<Collider2D>();
                if (collider != null) collider.isTrigger = true;
            }
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) AssignPlayerController(player);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Authored routes saved into Level1 and Level2 scenes.");
    }

    [MenuItem("Tools/Project Repair/Draw Player Animations and Build Controller")]
    public static void BuildPlayerAnimations()
    {
        Directory.CreateDirectory(OutputRoot);
        foreach (string clipName in new[] { "Idle", "Run", "Jump", "Fall", "Hurt", "Death", "Victory" })
            AssetDatabase.DeleteAsset(OutputRoot + "/" + clipName + ".anim");
        AssetDatabase.DeleteAsset(ControllerPath);
        string[][] groups =
        {
            new[] { "Idle_0", "Idle_1" }, new[] { "Run_0", "Run_1", "Run_2", "Run_3" },
            new[] { "Jump_0", "Fall_0" }, new[] { "Hurt_0" }, new[] { "Death_0", "Death_1" }, new[] { "Victory_0", "Victory_1" }
        };
        foreach (string[] group in groups)
            foreach (string frame in group) WriteFrame(frame);
        AssetDatabase.Refresh();
        foreach (string[] group in groups)
            foreach (string frame in group) ConfigureSprite(frame);
        AssetDatabase.Refresh();

        Directory.CreateDirectory("Assets/Animations/GeneratedPlayer");
        AnimationClip idle = MakeClip("Idle", new[] { "Idle_0", "Idle_1" }, .34f, true);
        AnimationClip run = MakeClip("Run", new[] { "Run_0", "Run_1", "Run_2", "Run_3" }, .105f, true);
        AnimationClip jump = MakeClip("Jump", new[] { "Jump_0" }, .14f, false);
        AnimationClip fall = MakeClip("Fall", new[] { "Fall_0" }, .14f, false);
        AnimationClip hurt = MakeClip("Hurt", new[] { "Hurt_0" }, .12f, false);
        AnimationClip death = MakeClip("Death", new[] { "Death_0", "Death_1" }, .25f, false);
        AnimationClip victory = MakeClip("Victory", new[] { "Victory_0", "Victory_1" }, .24f, false);
        var controller = AnimatorController.CreateAnimatorControllerAtPath(ControllerPath);
        controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Grounded", AnimatorControllerParameterType.Bool);
        controller.AddParameter("VerticalSpeed", AnimatorControllerParameterType.Float);
        controller.AddParameter("Hurt", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Dead", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Victory", AnimatorControllerParameterType.Trigger);
        AnimatorStateMachine sm = controller.layers[0].stateMachine;
        AnimatorState idleState = sm.AddState("Idle"); idleState.motion = idle; sm.defaultState = idleState;
        AnimatorState runState = sm.AddState("Run"); runState.motion = run;
        AnimatorState jumpState = sm.AddState("Jump"); jumpState.motion = jump;
        AnimatorState fallState = sm.AddState("Fall"); fallState.motion = fall;
        AnimatorState hurtState = sm.AddState("Hurt"); hurtState.motion = hurt;
        AnimatorState deathState = sm.AddState("Death"); deathState.motion = death;
        AnimatorState victoryState = sm.AddState("Victory"); victoryState.motion = victory;
        Transition(idleState, runState, "Speed", AnimatorConditionMode.Greater, .12f);
        Transition(runState, idleState, "Speed", AnimatorConditionMode.Less, .12f);
        GroundTransition(idleState, jumpState, false, true); GroundTransition(runState, jumpState, false, true);
        GroundTransition(idleState, fallState, false, false); GroundTransition(runState, fallState, false, false);
        Transition(jumpState, fallState, "VerticalSpeed", AnimatorConditionMode.Less, -.05f);
        GroundTransition(fallState, idleState, true, false, false);
        GroundTransition(fallState, runState, true, false, true);
        TriggerTransition(sm, hurtState, "Hurt"); TriggerTransition(sm, deathState, "Dead"); TriggerTransition(sm, victoryState, "Victory");
        AnimatorStateTransition hurtExit = hurtState.AddTransition(idleState); hurtExit.hasExitTime = true; hurtExit.exitTime = 1f; hurtExit.duration = .05f;
        AssetDatabase.SaveAssets();

        foreach (string sceneName in new[] { "Level1", "Level2" })
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/" + sceneName + ".unity", OpenSceneMode.Single);
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) AssignPlayerController(player);
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("New hand-drawn pixel frames and player Animator Controller created.");
    }

    static void KeepSingleGameplayCamera(Scene scene)
    {
        var cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None).Where(c => c.gameObject.scene == scene).ToArray();
        if (cameras.Length < 2) return;
        Camera keep = cameras.FirstOrDefault(c => c.GetComponent<CameraFollow>() != null)
                      ?? cameras.FirstOrDefault(c => c.CompareTag("MainCamera"))
                      ?? cameras[0];
        keep.tag = "MainCamera";
        keep.enabled = true;
        foreach (var cam in cameras)
        {
            if (cam == keep) continue;
            cam.enabled = false;
            cam.tag = "Untagged";
            var listener = cam.GetComponent<AudioListener>();
            if (listener != null) listener.enabled = false;
        }
    }

    static Vector2[] ForestRoute() => new[] {
        new Vector2(46.4f,-1.95f),new Vector2(49.1f,-1.55f),new Vector2(51.9f,-1.25f),new Vector2(54.8f,-1.6f),new Vector2(57.4f,-1.05f),
        new Vector2(60.4f,-.8f),new Vector2(63.2f,-1.25f),new Vector2(66.1f,-.55f),new Vector2(68.8f,-.3f),new Vector2(71.7f,-.85f),
        new Vector2(74.6f,-.45f),new Vector2(77.4f,-.95f),new Vector2(80.2f,-.3f),new Vector2(83.1f,-.6f),new Vector2(85.8f,-1.15f),
        new Vector2(88.7f,-.45f),new Vector2(91.6f,-.1f),new Vector2(94.4f,-.55f),new Vector2(97.1f,-.2f),new Vector2(99f,-.4f)
    };
    static Vector2[] RuinsRoute() => new[] {
        new Vector2(46.2f,-1.95f),new Vector2(48.8f,-1.25f),new Vector2(51.8f,-.65f),new Vector2(54.7f,-1.15f),new Vector2(57.4f,-1.85f),
        new Vector2(60.3f,-1.1f),new Vector2(63.1f,-.4f),new Vector2(66f,-1f),new Vector2(68.8f,-1.75f),new Vector2(71.6f,-1.05f),
        new Vector2(74.5f,-.25f),new Vector2(77.4f,-.8f),new Vector2(80.1f,-1.55f),new Vector2(83f,-.9f),new Vector2(85.8f,-.15f),
        new Vector2(88.7f,-.7f),new Vector2(91.5f,-1.35f),new Vector2(94.4f,-.65f),new Vector2(97.1f,-.05f),new Vector2(99f,-.3f)
    };

    static GameObject MakePlatform(Transform parent, string name, Vector2 pos, Vector2 size, Color color)
    {
        var go = new GameObject(name); go.transform.SetParent(parent); go.transform.position = pos; go.transform.localScale = new Vector3(size.x, size.y, 1f);
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = platformSprite; sr.color = color; sr.sortingOrder = 0;
        go.AddComponent<BoxCollider2D>(); return go;
    }
    static void MakeCoin(Transform parent, Vector2 pos)
    {
        var go = new GameObject("Required route coin"); go.transform.SetParent(parent); go.transform.position = pos; go.transform.localScale = Vector3.one * .48f;
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Sprites/Coin_Spritesheet.png").OfType<Sprite>().FirstOrDefault(); sr.color = new Color(1f,.82f,.28f); sr.sortingOrder = 3;
        var col = go.AddComponent<CircleCollider2D>(); col.isTrigger = true; go.AddComponent<Coin>();
    }
    static void MakeHazard(Transform parent, Vector2 pos, Color color)
    {
        var go = new GameObject("Route spike trap"); go.transform.SetParent(parent); go.transform.position = pos; go.transform.localScale = new Vector3(1.35f,.38f,1f);
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = platformSprite; sr.color = color; sr.sortingOrder = 2;
        var col = go.AddComponent<BoxCollider2D>(); col.isTrigger = true; col.size = new Vector2(.95f,.85f); go.AddComponent<HazardTrigger>();
    }
    static void MakeCheckpoint(Transform parent, Vector2 pos)
    {
        var go = new GameObject("Checkpoint lantern"); go.transform.SetParent(parent); go.transform.position = pos; go.transform.localScale = new Vector3(.42f,1.1f,1f);
        var sr = go.AddComponent<SpriteRenderer>(); sr.sprite = platformSprite; sr.color = new Color(1f,.72f,.26f); sr.sortingOrder = 2;
        var col = go.AddComponent<BoxCollider2D>(); col.isTrigger = true; go.AddComponent<Checkpoint>();
    }

    static void AssignPlayerController(GameObject player)
    {
        Sprite idleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(OutputRoot + "/Idle_0.png");
        var spriteRenderer = player.GetComponent<SpriteRenderer>();
        if (spriteRenderer == null) spriteRenderer = player.GetComponentInChildren<SpriteRenderer>(true);
        if (spriteRenderer != null && idleSprite != null)
        {
            spriteRenderer.sprite = idleSprite;
            spriteRenderer.color = Color.white;
        }
        var controller = AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(ControllerPath);
        if (controller != null)
        {
            var animator = player.GetComponent<Animator>(); if (animator == null) animator = player.AddComponent<Animator>();
            animator.runtimeAnimatorController = controller;
            animator.updateMode = AnimatorUpdateMode.UnscaledTime;
            if (player.GetComponent<PlayerAnimator>() == null) player.AddComponent<PlayerAnimator>();
        }
        var movement = player.GetComponent<PlayerMovement>();
        if (movement != null) movement.jumpHeight = 2.5f;
    }

    [MenuItem("Tools/Project Repair/Set Shared Hero and Add Level 2 Walls")]
    public static void ApplySharedHeroAndLevelTwoWalls()
    {
        platformSprite = AssetDatabase.LoadAllAssetsAtPath("Assets/Art/Sprites/Environment_Tiles.png").OfType<Sprite>().FirstOrDefault();
        foreach (string sceneName in new[] { "Level1", "Level2" })
        {
            Scene scene = EditorSceneManager.OpenScene("Assets/Scenes/" + sceneName + ".unity", OpenSceneMode.Single);
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player != null) AssignPlayerController(player);
            if (sceneName == "Level2")
            {
                var root = GameObject.Find("Authored Level2 Arena Boundaries");
                if (root == null) root = new GameObject("Authored Level2 Arena Boundaries");
                var exit = Object.FindFirstObjectByType<Exit>();
                float rightX = exit != null ? exit.transform.position.x + 2.5f : 102.5f;
                var leftWall = root.transform.Find("Visible left arena wall")?.gameObject;
                if (leftWall == null) leftWall = MakePlatform(root.transform, "Visible left arena wall", new Vector2(-15f, -1f), new Vector2(1f, 14f), new Color(.2f, .42f, .4f));
                var rightWall = root.transform.Find("Visible right arena wall")?.gameObject;
                if (rightWall == null) rightWall = MakePlatform(root.transform, "Visible right arena wall", new Vector2(rightX, -1f), new Vector2(1f, 14f), new Color(.2f, .42f, .4f));
                leftWall.transform.position = new Vector3(-15f, -1f, 0f);
                leftWall.transform.localScale = new Vector3(1f, 14f, 1f);
                rightWall.transform.position = new Vector3(rightX, -1f, 0f);
                rightWall.transform.localScale = new Vector3(1f, 14f, 1f);
                leftWall.GetComponent<SpriteRenderer>().sortingOrder = -1;
                rightWall.GetComponent<SpriteRenderer>().sortingOrder = -1;
            }
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("Level 1 and 2 now use the shared animated hero. Level 2 boundary walls are saved as visible scene objects.");
    }
    static void ConfigureSprite(string name)
    {
        var importer = AssetImporter.GetAtPath(OutputRoot + "/" + name + ".png") as TextureImporter;
        if (importer == null) return;
        importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = 32; importer.filterMode = FilterMode.Point; importer.textureCompression = TextureImporterCompression.Uncompressed; importer.alphaIsTransparency = true;
        importer.SaveAndReimport();
    }
    static AnimationClip MakeClip(string name, string[] frames, float step, bool loop)
    {
        string path = OutputRoot + "/" + name + ".anim";
        var clip = new AnimationClip { frameRate = 1f / step };
        var binding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");
        var keys = new ObjectReferenceKeyframe[frames.Length];
        for (int i=0;i<frames.Length;i++) keys[i] = new ObjectReferenceKeyframe { time = i*step, value = AssetDatabase.LoadAssetAtPath<Sprite>(OutputRoot+"/"+frames[i]+".png") };
        AnimationUtility.SetObjectReferenceCurve(clip,binding,keys);
        var settings = AnimationUtility.GetAnimationClipSettings(clip); settings.loopTime = loop; AnimationUtility.SetAnimationClipSettings(clip,settings);
        AssetDatabase.CreateAsset(clip,path); return clip;
    }
    static void Transition(AnimatorState from, AnimatorState to, string parameter, AnimatorConditionMode mode, float threshold)
    {
        var t = from.AddTransition(to); t.hasExitTime=false; t.duration=.06f; t.AddCondition(mode,threshold,parameter);
    }
    static void GroundTransition(AnimatorState from, AnimatorState to, bool grounded, bool positive, bool speedRun = false)
    {
        var t=from.AddTransition(to); t.hasExitTime=false; t.duration=.06f;
        t.AddCondition(grounded?AnimatorConditionMode.If:AnimatorConditionMode.IfNot,0,"Grounded");
        if (!grounded) t.AddCondition(positive?AnimatorConditionMode.Greater:AnimatorConditionMode.Less,positive?.05f:-.05f,"VerticalSpeed");
        else t.AddCondition(speedRun?AnimatorConditionMode.Greater:AnimatorConditionMode.Less,speedRun?.12f:.12f,"Speed");
    }
    static void TriggerTransition(AnimatorStateMachine sm, AnimatorState to, string trigger)
    {
        var t=sm.AddAnyStateTransition(to); t.hasExitTime=false; t.duration=.04f; t.AddCondition(AnimatorConditionMode.If,0,trigger);
    }

    static void WriteFrame(string name)
    {
        const int w=32,h=32; Color32 clear=new Color32(0,0,0,0), outline=new Color32(31,39,57,255), cloak=new Color32(38,135,129,255), cloakLight=new Color32(65,187,166,255), skin=new Color32(246,190,139,255), hair=new Color32(89,54,67,255), gold=new Color32(248,194,91,255), boot=new Color32(54,57,78,255), white=new Color32(255,244,217,255);
        var p=new Color32[w*h]; for(int i=0;i<p.Length;i++) p[i]=clear;
        void Dot(int x,int y,Color32 c){if(x>=0&&x<w&&y>=0&&y<h)p[y*w+x]=c;}
        void Rect(int x0,int y0,int x1,int y1,Color32 c){for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)Dot(x,y,c);}
        void Ellipse(int cx,int cy,int rx,int ry,Color32 c){for(int y=-ry;y<=ry;y++)for(int x=-rx;x<=rx;x++)if((float)x*x/(rx*rx)+(float)y*y/(ry*ry)<=1f)Dot(cx+x,cy+y,c);}
        int phase=name.Contains("_1")?1:name.Contains("_2")?2:name.Contains("_3")?3:0;
        int lean=name.StartsWith("Run")?(phase%2==0?0:1):0;
        // Hood, face, scarf and tunic are deliberately drawn as a coherent pixel character.
        Ellipse(16,23,8,8,outline); Ellipse(16,24,7,7,cloak);
        Ellipse(16,25,5,5,skin); Rect(10,25,20,29,hair); Rect(11,25,19,28,skin);
        Rect(11,29,21,30,cloakLight); Rect(10,28,21,29,gold);
        Rect(10,17,21,27,outline); Rect(11,18,20,26,cloak); Rect(12,19,19,25,cloakLight);
        Rect(8,19,11,25,outline); Rect(8,19,10,24,cloak); Rect(20,19,23,25,outline); Rect(21,19,22,24,cloak);
        Dot(18,26,outline); Dot(19,26,white);
        if(name.StartsWith("Idle")){Rect(11,7+phase,14,17,outline);Rect(18,7+phase,21,17,outline);Rect(12,7+phase,14,16,boot);Rect(18,7+phase,20,16,boot);}
        else if(name.StartsWith("Run")){int a=phase%2==0?0:2;Rect(11,8,14,17,outline);Rect(18,8,21,17,outline);Rect(10+a,6,14+a,8,boot);Rect(17-a,6,21-a,8,boot);}
        else if(name.StartsWith("Jump")){Rect(11,12,14,18,outline);Rect(18,12,21,18,outline);Rect(10,10,14,12,boot);Rect(18,10,22,12,boot);}
        else if(name.StartsWith("Fall")){Rect(11,6,14,17,outline);Rect(18,6,21,17,outline);Rect(10,4,15,6,boot);Rect(17,4,22,6,boot);}
        else if(name.StartsWith("Hurt")){Rect(11,8,14,17,outline);Rect(18,8,21,17,outline);Rect(10,6,14,8,boot);Rect(18,6,22,8,boot);Rect(23,22,25,24,gold);}
        else if(name.StartsWith("Death")){if(phase==0){Rect(8,12,24,18,outline);Rect(10,13,22,17,cloak);Rect(8,10,10,13,boot);Rect(22,17,24,20,boot);}else{Rect(7,7,25,11,outline);Rect(9,8,23,10,cloak);Dot(11,12,gold);Dot(21,12,gold);}}
        else {Rect(11,8,14,17,outline);Rect(18,8,21,17,outline);Rect(10,6,14,8,boot);Rect(18,6,22,8,boot);Rect(23,26,27,30,gold);}
        // Crisp dark contour around opaque pixels.
        var copy=(Color32[])p.Clone();
        for(int y=1;y<h-1;y++)for(int x=1;x<w-1;x++)if(copy[y*w+x].a==0){bool edge=false;for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++)if(copy[(y+dy)*w+x+dx].a>0)edge=true;if(edge)p[y*w+x]=outline;}
        var tex=new Texture2D(w,h,TextureFormat.RGBA32,false); tex.filterMode=FilterMode.Point; tex.SetPixels32(p); tex.Apply();
        File.WriteAllBytes(OutputRoot+"/"+name+".png",tex.EncodeToPNG()); Object.DestroyImmediate(tex);
    }
}
