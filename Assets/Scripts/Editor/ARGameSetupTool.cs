#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.IO;

public class ARGameSetupTool : EditorWindow
{
    [MenuItem("Tools/AR Gem Hunter/Auto-Setup AR Scene & Prefabs")]
    public static void SetupProjectScene()
    {
        EnsureDirectory("Assets/Materials");
        EnsureDirectory("Assets/Prefabs");
        EnsureDirectory("Assets/Scenes");

        // 1. Create Gold Gem Material
        Material gemMat = AssetDatabase.LoadAssetAtPath<Material>("Assets/Materials/GemMaterial.mat");
        if (gemMat == null)
        {
            Shader standardShader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard") ?? Shader.Find("Mobile/Diffuse");
            gemMat = new Material(standardShader);
            gemMat.color = new Color(1f, 0.82f, 0.1f); // Vibrant Gold
            if (gemMat.HasProperty("_Metallic")) gemMat.SetFloat("_Metallic", 0.7f);
            if (gemMat.HasProperty("_Smoothness")) gemMat.SetFloat("_Smoothness", 0.9f);
            AssetDatabase.CreateAsset(gemMat, "Assets/Materials/GemMaterial.mat");
        }

        // 2. Register "Collectible" Layer in TagManager if needed
        int collectibleLayerIndex = LayerMask.NameToLayer("Collectible");
        if (collectibleLayerIndex < 0) collectibleLayerIndex = 6;

        // 3. Create AR_Gem Prefab
        GameObject tempGem = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        tempGem.name = "AR_Gem";
        tempGem.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        MeshRenderer renderer = tempGem.GetComponent<MeshRenderer>();
        if (renderer != null) renderer.sharedMaterial = gemMat;
        tempGem.layer = collectibleLayerIndex;

        // Add ARCollectible script
        ARCollectible collectibleComp = tempGem.AddComponent<ARCollectible>();

        string prefabPath = "Assets/Prefabs/AR_Gem.prefab";
        GameObject gemPrefab = PrefabUtility.SaveAsPrefabAsset(tempGem, prefabPath);
        DestroyImmediate(tempGem);
        AssetDatabase.SaveAssets();

        // 4. Create New Scene
        UnityEngine.SceneManagement.Scene newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // 5. Directional Light
        GameObject lightGo = new GameObject("Directional Light");
        Light lightComp = lightGo.AddComponent<Light>();
        lightComp.type = LightType.Directional;
        lightComp.intensity = 1.2f;
        lightComp.color = Color.white;
        lightGo.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        // 6. Main AR Camera
        GameObject cameraGo = new GameObject("Main Camera");
        Camera cam = cameraGo.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.12f, 0.14f, 0.20f); // Sleek AR Dark Blue/Slate
        cam.nearClipPlane = 0.1f;
        cam.farClipPlane = 100f;
        cameraGo.AddComponent<AudioListener>();
        cameraGo.tag = "MainCamera";
        cameraGo.transform.position = new Vector3(0f, 1.2f, -1.5f);
        cameraGo.transform.rotation = Quaternion.identity;

        // 7. Visible Preview Gem in Scene (So the scene is NEVER blank even in Edit Mode!)
        GameObject previewGem = (GameObject)PrefabUtility.InstantiatePrefab(gemPrefab);
        previewGem.name = "Preview_Gem";
        previewGem.transform.position = new Vector3(0f, 1.2f, 0.8f);

        // Built-in Font
        Font defaultFont = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf") ?? Resources.GetBuiltinResource<Font>("Arial.ttf");

        // 8. Create UI Canvas
        GameObject canvasGo = new GameObject("AR_UI_Canvas");
        Canvas canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        CanvasScaler scaler = canvasGo.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1080, 1920);
        scaler.matchWidthOrHeight = 0.5f;
        canvasGo.AddComponent<GraphicRaycaster>();

        GameObject eventSystemGo = new GameObject("EventSystem");
        eventSystemGo.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemGo.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();

        // HUD: Score Text (Top-Left)
        GameObject scoreGo = new GameObject("ScoreText");
        scoreGo.transform.SetParent(canvasGo.transform, false);
        Text scoreText = scoreGo.AddComponent<Text>();
        scoreText.text = "Score: 0";
        scoreText.font = defaultFont;
        scoreText.fontSize = 44;
        scoreText.alignment = TextAnchor.MiddleLeft;
        scoreText.color = Color.white;
        RectTransform scoreRect = scoreGo.GetComponent<RectTransform>();
        scoreRect.anchorMin = new Vector2(0f, 1f);
        scoreRect.anchorMax = new Vector2(0f, 1f);
        scoreRect.pivot = new Vector2(0f, 1f);
        scoreRect.anchoredPosition = new Vector2(60f, -60f);
        scoreRect.sizeDelta = new Vector2(400f, 80f);

        // HUD: Timer Text (Top-Right)
        GameObject timerGo = new GameObject("TimerText");
        timerGo.transform.SetParent(canvasGo.transform, false);
        Text timerText = timerGo.AddComponent<Text>();
        timerText.text = "Time: 60s";
        timerText.font = defaultFont;
        timerText.fontSize = 44;
        timerText.alignment = TextAnchor.MiddleRight;
        timerText.color = new Color(1f, 0.9f, 0.2f);
        RectTransform timerRect = timerGo.GetComponent<RectTransform>();
        timerRect.anchorMin = new Vector2(1f, 1f);
        timerRect.anchorMax = new Vector2(1f, 1f);
        timerRect.pivot = new Vector2(1f, 1f);
        timerRect.anchoredPosition = new Vector2(-60f, -60f);
        timerRect.sizeDelta = new Vector2(400f, 80f);

        // HUD: Instruction Text (Bottom Center)
        GameObject instructGo = new GameObject("InstructionText");
        instructGo.transform.SetParent(canvasGo.transform, false);
        Text instructText = instructGo.AddComponent<Text>();
        instructText.text = "Tap / Click on Gems to Collect!";
        instructText.font = defaultFont;
        instructText.fontSize = 32;
        instructText.alignment = TextAnchor.MiddleCenter;
        instructText.color = new Color(0.9f, 0.9f, 0.9f, 0.8f);
        RectTransform instructRect = instructGo.GetComponent<RectTransform>();
        instructRect.anchorMin = new Vector2(0.5f, 0f);
        instructRect.anchorMax = new Vector2(0.5f, 0f);
        instructRect.pivot = new Vector2(0.5f, 0f);
        instructRect.anchoredPosition = new Vector2(0f, 80f);
        instructRect.sizeDelta = new Vector2(800f, 60f);

        // 9. Game Over Panel
        GameObject gameOverPanel = new GameObject("GameOverPanel");
        gameOverPanel.transform.SetParent(canvasGo.transform, false);
        Image panelImage = gameOverPanel.AddComponent<Image>();
        panelImage.color = new Color(0.04f, 0.06f, 0.12f, 0.95f);
        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.25f);
        panelRect.anchorMax = new Vector2(0.9f, 0.75f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        // Title
        GameObject goTitle = new GameObject("GameOverTitle");
        goTitle.transform.SetParent(gameOverPanel.transform, false);
        Text goTitleText = goTitle.AddComponent<Text>();
        goTitleText.text = "TIME'S UP!";
        goTitleText.font = defaultFont;
        goTitleText.fontSize = 52;
        goTitleText.alignment = TextAnchor.MiddleCenter;
        goTitleText.color = new Color(1f, 0.3f, 0.3f);
        RectTransform goTitleRect = goTitle.GetComponent<RectTransform>();
        goTitleRect.anchoredPosition = new Vector2(0f, 150f);
        goTitleRect.sizeDelta = new Vector2(600f, 80f);

        // Final Score Text
        GameObject finalScoreGo = new GameObject("FinalScoreText");
        finalScoreGo.transform.SetParent(gameOverPanel.transform, false);
        Text finalScoreText = finalScoreGo.AddComponent<Text>();
        finalScoreText.text = "Final Score: 0";
        finalScoreText.font = defaultFont;
        finalScoreText.fontSize = 44;
        finalScoreText.alignment = TextAnchor.MiddleCenter;
        finalScoreText.color = Color.white;
        RectTransform finalScoreRect = finalScoreGo.GetComponent<RectTransform>();
        finalScoreRect.anchoredPosition = new Vector2(0f, 40f);
        finalScoreRect.sizeDelta = new Vector2(600f, 80f);

        // Restart Button
        GameObject restartBtnGo = new GameObject("RestartButton");
        restartBtnGo.transform.SetParent(gameOverPanel.transform, false);
        Image btnImg = restartBtnGo.AddComponent<Image>();
        btnImg.color = new Color(0.18f, 0.65f, 0.3f);
        Button restartBtn = restartBtnGo.AddComponent<Button>();
        RectTransform btnRect = restartBtnGo.GetComponent<RectTransform>();
        btnRect.anchoredPosition = new Vector2(0f, -90f);
        btnRect.sizeDelta = new Vector2(340f, 95f);

        GameObject btnTextGo = new GameObject("BtnText");
        btnTextGo.transform.SetParent(restartBtnGo.transform, false);
        Text btnText = btnTextGo.AddComponent<Text>();
        btnText.text = "Play Again";
        btnText.font = defaultFont;
        btnText.fontSize = 38;
        btnText.alignment = TextAnchor.MiddleCenter;
        btnText.color = Color.white;
        RectTransform btnTextRect = btnTextGo.GetComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;

        // Hide GameOverPanel by default
        gameOverPanel.SetActive(false);

        // 10. GameController Object
        GameObject controllerGo = new GameObject("GameController");
        GameManager gameManager = controllerGo.AddComponent<GameManager>();
        UIManager uiManager = controllerGo.AddComponent<UIManager>();
        ARSpawner arSpawner = controllerGo.AddComponent<ARSpawner>();

        // Wire UIManager
        SerializedObject soUI = new SerializedObject(uiManager);
        soUI.FindProperty("scoreTextLegacy").objectReferenceValue = scoreText;
        soUI.FindProperty("timerTextLegacy").objectReferenceValue = timerText;
        soUI.FindProperty("gameOverPanel").objectReferenceValue = gameOverPanel;
        soUI.FindProperty("finalScoreTextLegacy").objectReferenceValue = finalScoreText;
        soUI.ApplyModifiedProperties();

        // Wire GameManager
        SerializedObject soGame = new SerializedObject(gameManager);
        soGame.FindProperty("uiManager").objectReferenceValue = uiManager;
        soGame.FindProperty("arSpawner").objectReferenceValue = arSpawner;
        soGame.FindProperty("gameDuration").floatValue = 60f;
        soGame.ApplyModifiedProperties();

        // Wire ARSpawner
        SerializedObject soSpawner = new SerializedObject(arSpawner);
        soSpawner.FindProperty("arCamera").objectReferenceValue = cam;
        soSpawner.FindProperty("collectiblePrefab").objectReferenceValue = gemPrefab.GetComponent<ARCollectible>();
        soSpawner.FindProperty("poolSize").intValue = 10;
        soSpawner.FindProperty("maxActiveObjects").intValue = 5;
        soSpawner.FindProperty("collectibleLayer").intValue = 1 << collectibleLayerIndex;
        soSpawner.ApplyModifiedProperties();

        // Wire Restart Button click
        UnityEditor.Events.UnityEventTools.AddPersistentListener(restartBtn.onClick, uiManager.OnRestartButtonClicked);

        // 11. Save Scene and Build Settings
        string scenePath = "Assets/Scenes/MainARScene.unity";
        EditorSceneManager.SaveScene(newScene, scenePath);
        EditorBuildSettings.scenes = new EditorBuildSettingsScene[] {
            new EditorBuildSettingsScene(scenePath, true)
        };

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        if (!Application.isBatchMode)
        {
            EditorUtility.DisplayDialog("AR Gem Hunter Setup", 
                "Success! Scene, UI, and Prefabs configured successfully!\nPress Play (Ctrl+P) to play!", "OK");
        }
    }

    private static void EnsureDirectory(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            string parent = Path.GetDirectoryName(path);
            string folder = Path.GetFileName(path);
            AssetDatabase.CreateFolder(parent, folder);
        }
    }
}
#endif
