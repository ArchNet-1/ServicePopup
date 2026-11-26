// ArchNet · Popup Service — Popups recipe (ArchNet Hub > Services > Popups)
// Creates, under Assets/ArchNet/Popups/: Scripts/GamePopups.cs (your class) and
// Prefabs/Popups.prefab (the popup screen), then puts the screen under the ArchNet Bootstrap
// so popups open from any scene.
// This assembly only compiles when the ArchNet Hub (com.archnet.hub 2.0.0+) is installed.
// Docs: https://archnet-plateform.fr/#ServicePopup

using System;
using System.Linq;
using ArchNet.Core.Editor;
using ArchNet.Installer.Editor;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ArchNet.Service.Editor
{
    public sealed class PopupsRecipe : ServiceRecipe
    {
        public const string RecipeId = "popups";
        const string TemplatePath = "Packages/com.archnet.service.popup/Editor/Recipe/Templates/GamePopups.cs.txt";
        const string ScriptFile = "Scripts/GamePopups.cs";
        const string PrefabFile = "Prefabs/Popups.prefab";
        const int SortingOrder = 1000; // above every other ArchNet screen

        public override string Id => RecipeId;
        public override string DisplayName => "Popups";
        public override string Description => "Alerts, yes / no questions and text prompts you await from code, one at a time. Services.Get<GamePopups>().Confirm(\"Quit\", \"Leave the game?\").";
        public override string Category => "UI";
        public override string FolderName => "Popups";
        public override string[] RequiredServices => new[] { BootstrapRecipe.RecipeId };
        public override bool HasTool => true;

        public static string PrefabPath => $"{RecipeRunner.RootFolder}/Popups/{PrefabFile}";

        public override void Create(RecipeContext context)
        {
            UIBuilder.EnsureTextMeshPro();
            context.WriteTemplate(ScriptFile, RecipeContext.LoadTemplate(TemplatePath));
        }

        public override void AfterScriptsCompiled(RecipeContext context)
        {
            Type serviceType = TypeCache.GetTypesDerivedFrom<PopupService>().FirstOrDefault(t => t.FullName == $"{context.Namespace}.GamePopups")
                ?? throw new InvalidOperationException($"{context.Namespace}.GamePopups was not found. Check that {context.PathOf(ScriptFile)} compiles and keeps its class name and namespace.");
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(context.PathOf(PrefabFile))
                                ?? context.SavePrefab(PrefabFile, BuildScreen(serviceType));
            BootstrapRecipe.AddUnderBootstrap(prefab);
        }

        public override void OpenTool() => PopupsWindow.Open();

        static GameObject BuildScreen(Type serviceType)
        {
            GameObject root = UIBuilder.ScreenCanvas("Popups", SortingOrder);
            Image shade = UIBuilder.Image("Shade", root.transform, UIBuilder.Shade); // also blocks clicks behind the popup
            UIBuilder.Stretch(shade.rectTransform);

            Image window = UIBuilder.Image("Window", root.transform, UIBuilder.Window);
            UIBuilder.Anchor(window.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(920f, 0f), Vector2.zero);
            var layout = window.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(56, 56, 48, 48);
            layout.spacing = 28;
            layout.childAlignment = TextAnchor.UpperCenter;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            window.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            TextMeshProUGUI title = UIBuilder.Text("Title", window.transform, "Title", 52, style: FontStyles.Bold);
            TextMeshProUGUI message = UIBuilder.Text("Message", window.transform, "Message", 38);
            message.color = UIBuilder.MutedText;
            TMP_InputField input = UIBuilder.InputField("Input", window.transform, "Type here…");

            RectTransform buttons = UIBuilder.Rect("Buttons", window.transform);
            var row = buttons.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 24;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = false;
            row.childControlHeight = false;
            row.childForceExpandWidth = false;
            buttons.gameObject.AddComponent<LayoutElement>().preferredHeight = 84;
            Button cancel = UIBuilder.Button("Cancel", buttons, "No", UIBuilder.Neutral, out TextMeshProUGUI cancelLabel);
            Button confirm = UIBuilder.Button("Confirm", buttons, "Yes", UIBuilder.Accent, out TextMeshProUGUI confirmLabel);
            foreach (Button button in new[] { cancel, confirm }) ((RectTransform)button.transform).sizeDelta = new Vector2(300f, 84f);

            var view = root.AddComponent<PopupView>();
            Assign(view, "_title", title);
            Assign(view, "_message", message);
            Assign(view, "_input", input);
            Assign(view, "_confirm", confirm);
            Assign(view, "_confirmLabel", confirmLabel);
            Assign(view, "_cancel", cancel);
            Assign(view, "_cancelLabel", cancelLabel);

            Component service = root.AddComponent(serviceType);
            Assign(service, "_view", view);
            return root;
        }

        static void Assign(Object target, string field, Object value)
        {
            var serialized = new SerializedObject(target);
            SerializedProperty property = serialized.FindProperty(field)
                ?? throw new InvalidOperationException($"{target.GetType().Name} has no serialized field {field}.");
            property.objectReferenceValue = value;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
