#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Linq;

public static class LevelsBootstrapper
{
    [MenuItem("Tools/Levels/Generate Prefab & Boot Scene")]
    public static void Generate()
    {
        Directory.CreateDirectory("Assets/Prefabs");
        Directory.CreateDirectory("Assets/Scenes");

        // Prefab in Assets/Prefabs
        var go = new GameObject("RegionLoaderPrefab");
        go.AddComponent<RegionLoader>();
        go.AddComponent<OverlayController>();
        PrefabUtility.SaveAsPrefabAsset(go, "Assets/Prefabs/RegionLoaderPrefab.prefab");
        Object.DestroyImmediate(go);

        // Boot scene in Assets/Scenes
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var canvasGO = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        canvasGO.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        new GameObject("EventSystem", typeof(UnityEngine.EventSystems.EventSystem), typeof(UnityEngine.EventSystems.StandaloneInputModule));

        var panel = new GameObject("Panel", typeof(Image));
        panel.transform.SetParent(canvasGO.transform, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.05f, 0.05f);
        rect.anchorMax = new Vector2(0.35f, 0.95f);
        rect.offsetMin = rect.offsetMax = Vector2.zero;
        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childForceExpandHeight = false;
        layout.childForceExpandWidth = true;
        layout.spacing = 8;

        // Find RegionLevel assets
        var guids = AssetDatabase.FindAssets("t:RegionLevel");
        var levels = guids.Select(g => AssetDatabase.LoadAssetAtPath<RegionLevel>(AssetDatabase.GUIDToAssetPath(g))).Where(a => a != null).ToArray();

        var spawner = new GameObject("Menu");
        var menu = spawner.AddComponent<SimpleLevelMenu>();
        menu.loaderPrefab = AssetDatabase.LoadAssetAtPath<RegionLoader>("Assets/Prefabs/RegionLoaderPrefab.prefab");

        if (levels.Length == 0)
        {
            var txtGO = new GameObject("Label", typeof(Text));
            var t = txtGO.GetComponent<Text>();
            t.text = "No RegionLevel assets found.\nUse: Tools/Levels/Create RegionLevel (Wizard)";
            t.color = Color.white;
            t.alignment = TextAnchor.MiddleCenter;
            t.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            txtGO.transform.SetParent(panel.transform, false);
        }
        else
        {
            menu.levels = levels;
            for (int i = 0; i < levels.Length && i < 12; i++)
            {
                var btnGO = new GameObject($"Btn_{levels[i].regionName}_{levels[i].areaRef}", typeof(Button), typeof(Image));
                btnGO.transform.SetParent(panel.transform, false);

                var txtGO = new GameObject("Text", typeof(Text));
                txtGO.transform.SetParent(btnGO.transform, false);
                var txt = txtGO.GetComponent<Text>();
                txt.text = $"{levels[i].regionName} / {levels[i].areaRef}";
                txt.alignment = TextAnchor.MiddleCenter;
                txt.color = Color.black;
                txt.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                var rectTxt = txt.GetComponent<RectTransform>();
                rectTxt.anchorMin = Vector2.zero;
                rectTxt.anchorMax = Vector2.one;
                rectTxt.offsetMin = rectTxt.offsetMax = Vector2.zero;

                var b = btnGO.GetComponent<Button>();
                int idx = i;
                b.onClick.AddListener(() => menu.Load(idx));
                var img = btnGO.GetComponent<Image>();
                img.color = new Color(0.85f, 0.85f, 0.85f, 1f);
                var rectBtn = btnGO.GetComponent<RectTransform>();
                rectBtn.sizeDelta = new Vector2(0, 40);
            }
        }

        var scenePath = "Assets/Scenes/Boot.unity";
        EditorSceneManager.SaveScene(scene, scenePath);
        AssetDatabase.SaveAssets();
        EditorUtility.DisplayDialog("Levels Bootstrapper", "Created Prefab and Boot scene.\nAdd RegionLevel assets to see buttons.", "OK");
    }
}
#endif