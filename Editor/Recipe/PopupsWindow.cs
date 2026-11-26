// ArchNet · Popup Service — Popups tool (ArchNet Hub > Services > Popups > Open tool)
// Restyle the popup and try each kind in Play Mode; the answers are written to the console.
// Docs: https://archnet-plateform.fr/#ServicePopup

using ArchNet.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace ArchNet.Service.Editor
{
    public class PopupsWindow : EditorWindow
    {
        const string StylePath = "Packages/com.archnet.hub/Editor/UI/Hub.uss";

        public static void Open()
        {
            var window = GetWindow<PopupsWindow>();
            window.titleContent = new GUIContent("Popups");
            window.minSize = new Vector2(380, 220);
        }

        void OnEnable() => EditorApplication.playModeStateChanged += OnPlayModeChanged;
        void OnDisable() => EditorApplication.playModeStateChanged -= OnPlayModeChanged;
        void OnPlayModeChanged(PlayModeStateChange _) => Rebuild();

        void CreateGUI() => Rebuild();

        void Rebuild()
        {
            VisualElement root = rootVisualElement;
            root.Clear();
            root.AddToClassList("archnet-hub");
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylePath);
            if (style != null) root.styleSheets.Add(style);
            root.style.paddingLeft = root.style.paddingRight = root.style.paddingTop = 10;

            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PopupsRecipe.PrefabPath);
            if (prefab == null)
            {
                root.Add(new HelpBox($"{PopupsRecipe.PrefabPath} was not found. Add the Popups service from ArchNet > Hub > Services.", HelpBoxMessageType.Warning));
                return;
            }

            var restyle = new Button(() => AssetDatabase.OpenAsset(prefab)) { text = "Restyle the popup", tooltip = "Opens Popups.prefab: colors, fonts, sizes…" };
            restyle.AddToClassList("primary");
            root.Add(restyle);

            var intro = new Label("Try them in Play Mode:");
            intro.AddToClassList("intro");
            intro.style.marginTop = 12;
            root.Add(intro);

            var row = new VisualElement();
            row.AddToClassList("row");
            row.Add(new Button(async () =>
            {
                await Popups().Alert("Saved", "Your progress is safe.");
                Debug.Log("[ArchNet] Alert closed.");
            }) { text = "Alert" });
            row.Add(new Button(async () => Debug.Log($"[ArchNet] Confirm answered: {await Popups().Confirm("Quit", "Leave the game?")}")) { text = "Confirm" });
            row.Add(new Button(async () =>
            {
                string text = await Popups().Prompt("New save", "Name your save:", "My adventure");
                Debug.Log(text == null ? "[ArchNet] Prompt cancelled." : $"[ArchNet] Prompt answered: \"{text}\"");
            }) { text = "Prompt" });
            row.SetEnabled(EditorApplication.isPlaying);
            root.Add(row);
            if (!EditorApplication.isPlaying) root.Add(new Button(EditorApplication.EnterPlaymode) { text = "Enter Play Mode" });
        }

        static PopupService Popups() => Services.Get<PopupService>();
    }
}
