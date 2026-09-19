#if UNITY_EDITOR
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.UI;

namespace SafeZoneVR.Editor
{
    public static class UIBuilderUtil
    {
        public const string k_GeneratedMaterialsFolder = "Assets/SafeZoneVR/Materials/Generated";

        public static readonly Color PanelColor = new Color(0.08f, 0.11f, 0.16f, 0.92f);
        public static readonly Color PanelLight = new Color(0.14f, 0.19f, 0.27f, 0.95f);
        public static readonly Color Accent = new Color(0.12f, 0.62f, 0.85f, 1f);
        public static readonly Color AccentGreen = new Color(0.18f, 0.72f, 0.42f, 1f);
        public static readonly Color TextColor = new Color(0.96f, 0.97f, 0.98f, 1f);
        public static readonly Color MutedText = new Color(0.75f, 0.8f, 0.86f, 1f);

        public static TMP_FontAsset Font
        {
            get
            {
                var f = TMP_Settings.defaultFontAsset;
                if (f == null)
                    f = Resources.Load<TMP_FontAsset>("Fonts & Materials/LiberationSans SDF");
                return f;
            }
        }

        public static Material GetOrCreateMaterial(string name, Color color, bool transparent = false, bool unlit = false, float smoothness = 0.35f, int renderQueue = -1)
        {
            EnsureFolder(k_GeneratedMaterialsFolder);
            var path = $"{k_GeneratedMaterialsFolder}/{name}.mat";
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            var shaderName = unlit ? "Universal Render Pipeline/Unlit" : "Universal Render Pipeline/Lit";
            var shader = Shader.Find(shaderName);
            if (shader == null)
            {
                Debug.LogError("Shader não encontrado: " + shaderName);
                shader = Shader.Find("Standard");
            }

            if (mat == null)
            {
                mat = new Material(shader);
                AssetDatabase.CreateAsset(mat, path);
            }
            else if (mat.shader != shader)
            {
                mat.shader = shader;
            }

            mat.SetColor("_BaseColor", color);
            mat.color = color;
            if (!unlit)
                mat.SetFloat("_Smoothness", smoothness);

            if (transparent)
            {
                mat.SetFloat("_Surface", 1f);
                mat.SetFloat("_Blend", 0f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = renderQueue > 0 ? renderQueue : (int)RenderQueue.Transparent;
            }
            else
            {
                mat.SetFloat("_Surface", 0f);
                mat.SetOverrideTag("RenderType", "Opaque");
                mat.SetInt("_SrcBlend", (int)BlendMode.One);
                mat.SetInt("_DstBlend", (int)BlendMode.Zero);
                mat.SetInt("_ZWrite", 1);
                mat.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
                mat.renderQueue = renderQueue > 0 ? renderQueue : -1;
            }

            EditorUtility.SetDirty(mat);
            return mat;
        }

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path))
                return;
            var parent = System.IO.Path.GetDirectoryName(path).Replace('\\', '/');
            var leaf = System.IO.Path.GetFileName(path);
            if (!AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, leaf);
        }

        public static GameObject Child(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go;
        }

        public static GameObject Primitive(PrimitiveType type, string name, Transform parent, Vector3 worldPos, Vector3 euler, Vector3 scale, Material material, bool keepCollider = true)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.position = worldPos;
            go.transform.rotation = Quaternion.Euler(euler);
            go.transform.localScale = scale;
            if (material != null)
                go.GetComponent<MeshRenderer>().sharedMaterial = material;
            if (!keepCollider)
            {
                var c = go.GetComponent<Collider>();
                if (c != null) Object.DestroyImmediate(c);
            }
            return go;
        }

        public static TextMeshPro WorldText(Transform parent, string name, string text, float fontSize, Color color, Vector3 worldPos, Vector3 euler, Vector2 size)
        {
            var go = Child(parent, name);
            go.transform.position = worldPos;
            go.transform.rotation = Quaternion.Euler(euler);
            var tmp = go.AddComponent<TextMeshPro>();
            tmp.font = Font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.Center;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.rectTransform.sizeDelta = size;
            return tmp;
        }

        public static Canvas CreateWorldCanvas(string name, Transform parent, Vector2 sizePx, float metersPerPixel)
        {
            var go = Child(parent, name);
            var rt = go.AddComponent<RectTransform>();
            rt.sizeDelta = sizePx;
            rt.localScale = Vector3.one * metersPerPixel;

            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            var scaler = go.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 2f;
            go.AddComponent<TrackedDeviceGraphicRaycaster>();

            go.AddComponent<ScreenPointGraphicRaycaster>();
            go.AddComponent<CanvasCameraBinder>();
            return canvas;
        }

        public static RectTransform Rect(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var go = Child(parent, name);
            var rt = go.AddComponent<RectTransform>();
            SetRect(rt, anchorMin, anchorMax, offsetMin, offsetMax);
            return rt;
        }

        public static void SetRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.offsetMin = offsetMin;
            rt.offsetMax = offsetMax;
            rt.pivot = new Vector2(0.5f, 0.5f);
        }

        public static Image Panel(Transform parent, string name, Color color, Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax)
        {
            var rt = Rect(parent, name, anchorMin, anchorMax, offsetMin, offsetMax);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = color;
            img.raycastTarget = false;
            return img;
        }

        public static Image FullPanel(Transform parent, string name, Color color)
        {
            return Panel(parent, name, color, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        }

        public static TextMeshProUGUI Text(Transform parent, string name, string text, float fontSize, Color color, TextAlignmentOptions align,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, FontStyles style = FontStyles.Normal)
        {
            var rt = Rect(parent, name, anchorMin, anchorMax, offsetMin, offsetMax);
            var tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
            tmp.font = Font;
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.color = color;
            tmp.alignment = align;
            tmp.fontStyle = style;
            tmp.textWrappingMode = TextWrappingModes.Normal;
            tmp.overflowMode = TextOverflowModes.Truncate;
            tmp.enableAutoSizing = true;
            tmp.fontSizeMax = fontSize;
            tmp.fontSizeMin = Mathf.Max(6f, fontSize * 0.55f);
            tmp.raycastTarget = false;
            return tmp;
        }

        public static TextMeshProUGUI TopText(Transform parent, string name, string text, float fontSize, Color color, TextAlignmentOptions align,
            float top, float height, float sideMargin = 24f, FontStyles style = FontStyles.Normal)
        {
            return Text(parent, name, text, fontSize, color, align,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(sideMargin, -(top + height)), new Vector2(-sideMargin, -top), style);
        }

        public static Button Button(Transform parent, string name, string label, float fontSize, Color background, Color textColor,
            Vector2 anchorMin, Vector2 anchorMax, Vector2 offsetMin, Vector2 offsetMax, out TextMeshProUGUI labelText)
        {
            var rt = Rect(parent, name, anchorMin, anchorMax, offsetMin, offsetMax);
            var img = rt.gameObject.AddComponent<Image>();
            img.color = background;
            var button = rt.gameObject.AddComponent<Button>();
            button.targetGraphic = img;
            var nav = button.navigation;
            nav.mode = Navigation.Mode.None;
            button.navigation = nav;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.15f, 1.15f, 1.15f, 1f);
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.55f, 0.55f, 0.55f, 0.6f);
            button.colors = colors;

            labelText = Text(rt, "Label", label, fontSize, textColor, TextAlignmentOptions.Center,
                Vector2.zero, Vector2.one, new Vector2(6f, 1f), new Vector2(-6f, -1f), FontStyles.Bold);
            labelText.overflowMode = TextOverflowModes.Overflow;
            return button;
        }

        public static Button TopButton(Transform parent, string name, string label, float fontSize, Color background,
            float top, float height, float sideMargin, out TextMeshProUGUI labelText)
        {
            return Button(parent, name, label, fontSize, background, TextColor,
                new Vector2(0f, 1f), new Vector2(1f, 1f),
                new Vector2(sideMargin, -(top + height)), new Vector2(-sideMargin, -top), out labelText);
        }

        public static Button BottomButton(Transform parent, string name, string label, float fontSize, Color background,
            float bottom, float height, float xMin01, float xMax01, float sideMargin, out TextMeshProUGUI labelText)
        {
            return Button(parent, name, label, fontSize, background, TextColor,
                new Vector2(xMin01, 0f), new Vector2(xMax01, 0f),
                new Vector2(sideMargin, bottom), new Vector2(-sideMargin, bottom + height), out labelText);
        }

        public static LazyFollow AddLazyFollow(GameObject go, Transform target, Vector3 localOffset, float speed = 3f)
        {
            var lf = go.AddComponent<LazyFollow>();
            lf.target = target;
            lf.targetOffset = localOffset;
            lf.applyTargetInLocalSpace = true;
            lf.positionFollowMode = LazyFollow.PositionFollowMode.Follow;
            lf.rotationFollowMode = LazyFollow.RotationFollowMode.LookAtWithWorldUp;
            lf.movementSpeed = speed;
            lf.snapOnEnable = true;
            return lf;
        }

        public static void MarkStatic(GameObject go, bool includeChildren = true)
        {
            var flags = StaticEditorFlags.BatchingStatic | StaticEditorFlags.OccludeeStatic | StaticEditorFlags.OccluderStatic;
            GameObjectUtility.SetStaticEditorFlags(go, flags);
            if (!includeChildren)
                return;
            foreach (Transform c in go.transform)
                MarkStatic(c.gameObject, true);
        }
    }
}
#endif
