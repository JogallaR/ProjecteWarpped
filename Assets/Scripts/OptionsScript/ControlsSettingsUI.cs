using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

// Construeix en temps d'execucio els controls del panell "CONTROLS":
//  - CAMERA SPEED  -> slider
//  - INVERT HORIZONTAL / VERTICAL -> dos interruptors (H / V)
//  - REASSIGN KEYS -> llista d'accions on pots canviar cada tecla
// S'afegeix sol des d'OptionsPanelController, no cal configurar res a l'Inspector.
public class ControlsSettingsUI : MonoBehaviour
{
    private const float WidgetGap = 50f;

    private RectTransform cameraSpeedLabel;
    private RectTransform invertLabel;
    private RectTransform reassignLabel;
    private TMP_Text styleSource;

    private RectTransform speedWidget;
    private Slider speedSlider;
    private TMP_Text speedValueText;

    private RectTransform invertWidget;
    private TMP_Text invertHText;
    private TMP_Text invertVText;

    private RectTransform rebindView;
    private readonly Dictionary<GameAction, TMP_Text> keyTexts = new Dictionary<GameAction, TMP_Text>();

    private bool built;
    private bool rebindOpen;
    private bool listening;
    private GameAction listeningAction;

    public bool IsRebindOpen => rebindOpen;

    private void Awake()
    {
        Build();
    }

    private void OnEnable()
    {
        Build();
        ShowMainView();
        RefreshAll();
        GameSettings.Changed += RefreshAll;
    }

    private void OnDisable()
    {
        GameSettings.Changed -= RefreshAll;
        listening = false;
        rebindOpen = false;
        GameSettings.UiWantsEscape = false;
    }

    private void Update()
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (listening)
        {
            if (keyboard.escapeKey.wasPressedThisFrame)
            {
                StopListening();
                GameSettings.EscapeHandledFrame = Time.frameCount;
                return;
            }

            foreach (KeyControl key in keyboard.allKeys)
            {
                if (key == null || !key.wasPressedThisFrame) continue;
                if (key.keyCode == Key.Escape || key.keyCode == Key.None) continue;

                GameSettings.SetKey(listeningAction, key.keyCode);
                StopListening();
                GameSettings.EscapeHandledFrame = Time.frameCount;
                return;
            }
            return;
        }

        if (rebindOpen && keyboard.escapeKey.wasPressedThisFrame)
        {
            GameSettings.EscapeHandledFrame = Time.frameCount;
            ShowMainView();
        }
    }

    // ---------------- Construccio ----------------

    private void Build()
    {
        if (built) return;
        built = true;

        cameraSpeedLabel = FindChildRect("CameraSpeed");
        invertLabel = FindChildRect("InvertH/V");
        reassignLabel = FindChildRect("ReassignKeys");

        Transform titleT = transform.Find("Title");
        styleSource = titleT != null ? titleT.GetComponent<TMP_Text>() : null;
        if (cameraSpeedLabel != null)
        {
            TMP_Text labelText = cameraSpeedLabel.GetComponent<TMP_Text>();
            if (labelText != null) styleSource = labelText;
        }

        if (reassignLabel != null)
        {
            // A les escenes del joc el text deia "REASING KEYS"
            TMP_Text reassignText = reassignLabel.GetComponent<TMP_Text>();
            if (reassignText != null) reassignText.text = "REASSIGN KEYS";

            Button b = reassignLabel.GetComponent<Button>();
            if (b == null) b = reassignLabel.gameObject.AddComponent<Button>();
            b.onClick.AddListener(OpenRebindView);
        }

        BuildSpeedWidget();
        BuildInvertWidget();
        BuildRebindView();
    }

    private RectTransform FindChildRect(string childName)
    {
        // No fem servir transform.Find: un nom amb "/" (com "InvertH/V") s'interpreta com un cami
        foreach (Transform child in transform)
        {
            if (child.name == childName) return child as RectTransform;
        }
        return null;
    }

    private void BuildSpeedWidget()
    {
        speedWidget = CreateRect("CameraSpeedWidget", transform);
        speedWidget.sizeDelta = new Vector2(560f, 80f);
        speedWidget.pivot = new Vector2(0f, 0.5f);

        speedSlider = CreateSlider(speedWidget);
        RectTransform sliderRt = (RectTransform)speedSlider.transform;
        sliderRt.anchorMin = new Vector2(0f, 0.5f);
        sliderRt.anchorMax = new Vector2(0f, 0.5f);
        sliderRt.pivot = new Vector2(0f, 0.5f);
        sliderRt.anchoredPosition = Vector2.zero;
        sliderRt.sizeDelta = new Vector2(380f, 24f);

        speedSlider.minValue = GameSettings.MinCameraSpeed;
        speedSlider.maxValue = GameSettings.MaxCameraSpeed;
        speedSlider.wholeNumbers = false;
        speedSlider.onValueChanged.AddListener(v =>
        {
            GameSettings.CameraSpeed = Mathf.Round(v * 20f) / 20f;
        });

        speedValueText = CreateText("Value", speedWidget, "1.0X", 40f, TextAlignmentOptions.Left);
        RectTransform vt = speedValueText.rectTransform;
        vt.anchorMin = vt.anchorMax = new Vector2(0f, 0.5f);
        vt.pivot = new Vector2(0f, 0.5f);
        vt.anchoredPosition = new Vector2(410f, 0f);
        vt.sizeDelta = new Vector2(160f, 70f);
    }

    private void BuildInvertWidget()
    {
        invertWidget = CreateRect("InvertWidget", transform);
        invertWidget.sizeDelta = new Vector2(420f, 80f);
        invertWidget.pivot = new Vector2(0f, 0.5f);

        invertHText = CreateTextButton("InvertH", invertWidget, new Vector2(0f, 0f), new Vector2(200f, 70f), 40f,
            () => GameSettings.InvertHorizontal = !GameSettings.InvertHorizontal);
        invertVText = CreateTextButton("InvertV", invertWidget, new Vector2(220f, 0f), new Vector2(200f, 70f), 40f,
            () => GameSettings.InvertVertical = !GameSettings.InvertVertical);
    }

    private void BuildRebindView()
    {
        rebindView = CreateRect("RebindView", transform);

        GameAction[] actions = GameSettings.AllActions;
        int rowsPerColumn = Mathf.CeilToInt(actions.Length / 2f);
        float rowHeight = 1f / (rowsPerColumn + 1);

        for (int i = 0; i < actions.Length; i++)
        {
            GameAction action = actions[i];
            int column = i / rowsPerColumn;
            int row = i % rowsPerColumn;

            float xMin = column == 0 ? 0f : 0.52f;
            float xMax = column == 0 ? 0.48f : 1f;
            float yMax = 1f - row * rowHeight;
            float yMin = yMax - rowHeight;

            RectTransform rowRt = CreateRect("Row_" + action, rebindView);
            rowRt.anchorMin = new Vector2(xMin, yMin);
            rowRt.anchorMax = new Vector2(xMax, yMax);
            rowRt.offsetMin = rowRt.offsetMax = Vector2.zero;

            TMP_Text label = CreateText("Label", rowRt, GameSettings.ActionLabel(action), 34f, TextAlignmentOptions.Left);
            label.rectTransform.anchorMin = new Vector2(0f, 0f);
            label.rectTransform.anchorMax = new Vector2(0.6f, 1f);
            label.rectTransform.offsetMin = label.rectTransform.offsetMax = Vector2.zero;

            TMP_Text keyText = CreateTextButton("Key", rowRt, Vector2.zero, Vector2.zero, 34f, () => StartListening(action));
            RectTransform kr = (RectTransform)keyText.transform.parent;
            kr.anchorMin = new Vector2(0.62f, 0.1f);
            kr.anchorMax = new Vector2(1f, 0.9f);
            kr.pivot = new Vector2(0.5f, 0.5f);
            kr.offsetMin = kr.offsetMax = Vector2.zero;
            keyText.alignment = TextAlignmentOptions.Center;

            Image bg = kr.GetComponent<Image>();
            if (bg != null) bg.color = new Color(1f, 1f, 1f, 0.08f);

            keyTexts[action] = keyText;
        }

        // Ultima fila: restablir i tornar
        RectTransform bottom = CreateRect("BottomRow", rebindView);
        bottom.anchorMin = new Vector2(0f, 0f);
        bottom.anchorMax = new Vector2(1f, rowHeight);
        bottom.offsetMin = bottom.offsetMax = Vector2.zero;

        TMP_Text reset = CreateTextButton("ResetButton", bottom, Vector2.zero, Vector2.zero, 38f, GameSettings.ResetControls);
        reset.text = "RESET DEFAULTS";
        RectTransform rr = (RectTransform)reset.transform.parent;
        rr.anchorMin = new Vector2(0f, 0f);
        rr.anchorMax = new Vector2(0.48f, 1f);
        rr.offsetMin = rr.offsetMax = Vector2.zero;
        reset.alignment = TextAlignmentOptions.Left;

        TMP_Text back = CreateTextButton("BackButton", bottom, Vector2.zero, Vector2.zero, 38f, ShowMainView);
        back.text = "BACK (ESC)";
        RectTransform br = (RectTransform)back.transform.parent;
        br.anchorMin = new Vector2(0.52f, 0f);
        br.anchorMax = new Vector2(1f, 1f);
        br.offsetMin = br.offsetMax = Vector2.zero;
        back.alignment = TextAlignmentOptions.Right;

        rebindView.gameObject.SetActive(false);
    }

    // ---------------- Vistes ----------------

    private void ShowMainView()
    {
        StopListening();
        rebindOpen = false;
        GameSettings.UiWantsEscape = false;

        SetOtherChildrenVisible(true);
        if (speedWidget != null) speedWidget.gameObject.SetActive(cameraSpeedLabel != null);
        if (invertWidget != null) invertWidget.gameObject.SetActive(invertLabel != null);
        if (rebindView != null) rebindView.gameObject.SetActive(false);

        LayoutWidgets();
        ClearSelection();
    }

    private void OpenRebindView()
    {
        rebindOpen = true;
        GameSettings.UiWantsEscape = true;

        SetOtherChildrenVisible(false);
        if (speedWidget != null) speedWidget.gameObject.SetActive(false);
        if (invertWidget != null) invertWidget.gameObject.SetActive(false);

        FitRebindViewToCanvas();
        rebindView.gameObject.SetActive(true);
        RefreshAll();
        ClearSelection();
    }

    // A la llista de tecles nomes es veuen el titol, el fons, RETURN i la propia llista
    private void SetOtherChildrenVisible(bool visible)
    {
        foreach (Transform child in transform)
        {
            if (child == rebindView || child == speedWidget || child == invertWidget) continue;
            string n = child.name;
            if (n == "Title" || n == "Background" || n == "ReturnButton") continue;
            SetLabelVisible(child as RectTransform, visible);
        }
    }

    // Amaguem amb CanvasGroup (no SetActive) per no trencar l'escala que guarda ButtonHover
    private static void SetLabelVisible(RectTransform label, bool visible)
    {
        if (label == null) return;
        CanvasGroup group = label.GetComponent<CanvasGroup>();
        if (group == null) group = label.gameObject.AddComponent<CanvasGroup>();
        group.alpha = visible ? 1f : 0f;
        group.interactable = visible;
        group.blocksRaycasts = visible;
    }

    private void StartListening(GameAction action)
    {
        listening = true;
        listeningAction = action;
        GameSettings.UiWantsEscape = true;
        if (keyTexts.TryGetValue(action, out TMP_Text t)) t.text = "PRESS A KEY";
        ClearSelection();
    }

    private void StopListening()
    {
        if (!listening) return;
        listening = false;
        GameSettings.UiWantsEscape = rebindOpen;
        RefreshAll();
        ClearSelection();
    }

    private static void ClearSelection()
    {
        if (EventSystem.current != null) EventSystem.current.SetSelectedGameObject(null);
    }

    private void RefreshAll()
    {
        if (speedSlider != null) speedSlider.SetValueWithoutNotify(GameSettings.CameraSpeed);
        if (speedValueText != null) speedValueText.text = GameSettings.CameraSpeed.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture) + "X";
        if (invertHText != null) invertHText.text = "H: " + (GameSettings.InvertHorizontal ? "ON" : "OFF");
        if (invertVText != null) invertVText.text = "V: " + (GameSettings.InvertVertical ? "ON" : "OFF");

        foreach (KeyValuePair<GameAction, TMP_Text> pair in keyTexts)
        {
            if (listening && pair.Key == listeningAction) continue;
            pair.Value.text = GameSettings.KeyName(pair.Key);
        }
    }

    // ---------------- Posicionament ----------------

    // Col·loca cada widget just a la dreta del text de la seva etiqueta
    private void LayoutWidgets()
    {
        Canvas.ForceUpdateCanvases();
        PlaceAfterLabel(speedWidget, cameraSpeedLabel);
        PlaceAfterLabel(invertWidget, invertLabel);
    }

    private static void PlaceAfterLabel(RectTransform widget, RectTransform label)
    {
        if (widget == null || label == null) return;

        TMP_Text text = label.GetComponent<TMP_Text>();
        Vector3 localPoint;
        if (text != null)
        {
            text.ForceMeshUpdate();
            Bounds b = text.textBounds;
            localPoint = new Vector3(b.max.x, b.center.y, 0f);
        }
        else
        {
            Rect r = label.rect;
            localPoint = new Vector3(r.xMax, r.center.y, 0f);
        }

        widget.position = label.TransformPoint(localPoint);
        widget.anchoredPosition += new Vector2(WidgetGap, 0f);
    }

    private void FitRebindViewToCanvas()
    {
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null) return;
        RectTransform canvasRt = (RectTransform)canvas.rootCanvas.transform;

        Transform originalParent = rebindView.parent;
        rebindView.SetParent(canvasRt, false);
        rebindView.anchorMin = Vector2.zero;
        rebindView.anchorMax = Vector2.one;
        rebindView.pivot = new Vector2(0.5f, 0.5f);
        // Marges: deixa lloc al titol (a dalt) i al boto RETURN (a baix)
        rebindView.offsetMin = new Vector2(150f, 200f);
        rebindView.offsetMax = new Vector2(-150f, -250f);

        // La llista comenca just per sota del titol "CONTROLS"
        Transform titleT = transform.Find("Title");
        TMP_Text title = titleT != null ? titleT.GetComponent<TMP_Text>() : null;
        if (title != null)
        {
            title.ForceMeshUpdate();
            Vector3 worldBottom = title.transform.TransformPoint(new Vector3(0f, title.textBounds.min.y, 0f));
            float bottomY = canvasRt.InverseTransformPoint(worldBottom).y; // relatiu al centre del canvas
            float fromTop = canvasRt.rect.yMax - bottomY;                 // distancia des de dalt
            rebindView.offsetMax = new Vector2(-150f, -(fromTop + 40f));
        }
        rebindView.localScale = Vector3.one;
        rebindView.SetParent(originalParent, true);
    }

    // ---------------- Creacio d'elements UI ----------------

    private static RectTransform CreateRect(string name, Transform parent)
    {
        GameObject go = new GameObject(name, typeof(RectTransform));
        go.layer = parent.gameObject.layer;
        RectTransform rt = (RectTransform)go.transform;
        rt.SetParent(parent, false);
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        return rt;
    }

    private TMP_Text CreateText(string name, Transform parent, string content, float maxSize, TextAlignmentOptions alignment)
    {
        RectTransform rt = CreateRect(name, parent);
        TextMeshProUGUI t = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (styleSource != null)
        {
            t.font = styleSource.font;
            t.fontSharedMaterial = styleSource.fontSharedMaterial;
            t.color = styleSource.color;
        }
        else
        {
            t.color = Color.white;
        }
        t.text = content;
        t.enableAutoSizing = true;
        t.fontSizeMin = 14f;
        t.fontSizeMax = maxSize;
        t.fontSize = maxSize;
        t.alignment = alignment;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.raycastTarget = false;
        return t;
    }

    // Boto amb fons transparent i text; el text canvia de color al passar-hi per sobre
    private TMP_Text CreateTextButton(string name, Transform parent, Vector2 pos, Vector2 size, float maxSize, UnityEngine.Events.UnityAction onClick)
    {
        RectTransform rt = CreateRect(name, parent);
        rt.anchorMin = rt.anchorMax = new Vector2(0f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;

        Image bg = rt.gameObject.AddComponent<Image>();
        bg.color = new Color(1f, 1f, 1f, 0f);

        TMP_Text label = CreateText("Text", rt, "", maxSize, TextAlignmentOptions.Left);
        RectTransform lr = label.rectTransform;
        lr.anchorMin = Vector2.zero;
        lr.anchorMax = Vector2.one;
        lr.offsetMin = new Vector2(8f, 0f);
        lr.offsetMax = new Vector2(-8f, 0f);

        Button button = rt.gameObject.AddComponent<Button>();
        button.targetGraphic = label;
        ColorBlock cb = button.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        cb.selectedColor = Color.white;
        cb.pressedColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        cb.fadeDuration = 0.08f;
        button.colors = cb;
        button.onClick.AddListener(onClick);

        return label;
    }

    private Slider CreateSlider(Transform parent)
    {
        // Si existeix el panell SOUND, copiem un dels seus sliders per tenir el mateix estil
        Transform sound = transform.parent != null ? transform.parent.Find("SoundPanel") : null;
        Slider template = sound != null ? sound.GetComponentInChildren<Slider>(true) : null;

        if (template != null)
        {
            Slider copy = Instantiate(template, parent, false);
            copy.name = "CameraSpeedSlider";
            copy.onValueChanged = new Slider.SliderEvent();
            copy.gameObject.SetActive(true);
            return copy;
        }

        // Si no, en creem un de simple
        RectTransform root = CreateRect("CameraSpeedSlider", parent);
        Slider slider = root.gameObject.AddComponent<Slider>();

        RectTransform bgRt = CreateRect("Background", root);
        bgRt.anchorMin = new Vector2(0f, 0.25f);
        bgRt.anchorMax = new Vector2(1f, 0.75f);
        bgRt.offsetMin = bgRt.offsetMax = Vector2.zero;
        bgRt.gameObject.AddComponent<Image>().color = new Color(1f, 1f, 1f, 0.25f);

        RectTransform fillArea = CreateRect("Fill Area", root);
        fillArea.anchorMin = new Vector2(0f, 0.25f);
        fillArea.anchorMax = new Vector2(1f, 0.75f);
        fillArea.offsetMin = fillArea.offsetMax = Vector2.zero;
        RectTransform fill = CreateRect("Fill", fillArea);
        fill.anchorMin = Vector2.zero;
        fill.anchorMax = new Vector2(0f, 1f);
        fill.offsetMin = fill.offsetMax = Vector2.zero;
        fill.gameObject.AddComponent<Image>().color = Color.white;

        RectTransform handleArea = CreateRect("Handle Slide Area", root);
        handleArea.anchorMin = Vector2.zero;
        handleArea.anchorMax = Vector2.one;
        handleArea.offsetMin = handleArea.offsetMax = Vector2.zero;
        RectTransform handle = CreateRect("Handle", handleArea);
        handle.sizeDelta = new Vector2(20f, 0f);
        handle.anchorMin = new Vector2(0f, 0f);
        handle.anchorMax = new Vector2(0f, 1f);
        Image handleImg = handle.gameObject.AddComponent<Image>();
        handleImg.color = Color.white;

        slider.fillRect = fill;
        slider.handleRect = handle;
        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
        return slider;
    }
}
