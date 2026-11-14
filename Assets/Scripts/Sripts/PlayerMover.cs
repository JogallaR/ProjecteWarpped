using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using System;
using Random = UnityEngine.Random;

public class PlayerMover : MonoBehaviour
{
    public static event Action<PlayerMover> TurnEnded;

    //Moves per mana
    const int MovesPerMana = 2;

    //TextmeshPro mana
    [SerializeField] TMP_Text MoveText;

    [SerializeField] CentralPlayer central;
    public CentralPlayer Central => central;

    public int manaThisTurn = 0;
    public int minRoll = 2;
    public int maxRoll = 6;
    //Mana aquest punts
    public int movePoints;

    public bool effectDo = true;

    public GridBehaviour grid;
    public Vector2Int position;
    public float moveSpeed = 8f;
    public float heightOffset = 0.05f;

    public bool smartWalk = false;

    [Header("Armes del Personatge (s'equipen automaticament a handslot.r i handslot.l)")]
    [SerializeField] private GameObject rightHandWeaponPrefab;
    [SerializeField] private GameObject leftHandWeaponPrefab;

    [Header("Configuració d'Atacs (Tecles 1, 2, 3 i Botons Canvas)")]
    public int attack1ManaCost = 2;
    public int attack1Damage = 2;

    public int attack2ManaCost = 4;
    public int attack2Damage = 5;

    public int attack3ManaCost = 6;
    public int attack3Damage = 8;

    private string attack1Name = "1. Sword Slash";
    private string attack2Name = "2. Shield Bash";
    private string attack3Name = "3. Lunge";

    private string attack1TypeLabel = "Melee";
    private string attack2TypeLabel = "Melee";
    private string attack3TypeLabel = "Melee";

    private int attack1MinRange = 1;
    private int attack1MaxRange = 2;
    private int attack2MinRange = 1;
    private int attack2MaxRange = 2;
    private int attack3MinRange = 1;
    private int attack3MaxRange = 2;

    private bool attack1IsRanged = false;
    private bool attack2IsRanged = false;
    private bool attack3IsRanged = false;

    private string attack1Anim = "Attack1";
    private string attack2Anim = "Attack2";
    private string attack3Anim = "Attack3";
    private float attack1Duration = 1.05f;
    private float attack2Duration = 1.35f;
    private float attack3Duration = 1.3f;
    private Color attack1Color = new Color(0.3f, 0.85f, 1f, 1f);
    private Color attack2Color = new Color(1f, 0.65f, 0.15f, 1f);
    private Color attack3Color = new Color(1f, 0.2f, 0.3f, 1f);
    private GameObject projectileVisualPrefab;
    private Material characterSharedMaterial;
    private int classMaxHealth = 20;
    private bool classConfigured = false;
    private bool controlsHidden = false;

    [Header("Referències UI Atacs i HUD ( S'assignen/creen automàticament al Canvas )")]
    [SerializeField] private Button attackButton1;
    [SerializeField] private Button attackButton2;
    [SerializeField] private Button attackButton3;
    [SerializeField] private Button skipTurnButton;
    [SerializeField] private TMP_Text attackText1;
    [SerializeField] private TMP_Text attackText2;
    [SerializeField] private TMP_Text attackText3;
    [SerializeField] private TMP_Text skipTurnText;
    [SerializeField] private TMP_Text roundText;
    [SerializeField] private TMP_Text controlsText;

    [SerializeField] private EnemyLogic enemy;
    [SerializeField] private Animator animator;

    bool moving;
    bool attacking;
    bool enemyTurnActive;

    //Referencia publica
    public bool IsMoving => moving || attacking || enemyTurnActive;


    void Awake()
    {
        if (!central) central = GetComponentInParent<CentralPlayer>();
        if (!animator) animator = GetComponentInChildren<Animator>();
        SetupCharacterClassAndWeapons();
    }

    void OnEnable()
    {
        SetupCharacterClassAndWeapons();
        GameSettings.Changed += UpdateUI;
    }

    void OnDisable()
    {
        GameSettings.Changed -= UpdateUI;
    }

    // Canvia el numero inicial del nom de l'atac ("1. Sword Slash") per la tecla assignada
    string AttackTitleWithKey(string title, GameAction action)
    {
        if (string.IsNullOrEmpty(title)) return title;
        int dot = title.IndexOf(". ");
        if (dot <= 0 || dot > 2) return title;
        return GameSettings.KeyName(action) + title.Substring(dot);
    }

    public void Init(GridBehaviour gridRef, TMP_Text moveText, Vector2Int spawnTile)
    {
        grid = gridRef;
        MoveText = moveText;
        position = spawnTile;
        if (!central) central = GetComponentInParent<CentralPlayer>();
        SetupCharacterClassAndWeapons();
        if (central != null) central.SetClassMaxHealth(classMaxHealth);
        transform.position = TileWorld(position);
        SetupAttackButtonsUI();
        UpdateUI();
    }

    void Start()
    {
        if (!central) central = GetComponentInParent<CentralPlayer>();
        SetupCharacterClassAndWeapons();
        if (central != null) central.SetClassMaxHealth(classMaxHealth);
        if (grid == null) return;

        if (!enemy) enemy = FindFirstObjectByType<EnemyLogic>();
        SetupAttackButtonsUI();
        //Creacio grid
        transform.position = TileWorld(position);
        EndTurn();
    }

    public void SetEnemyTurnActive(bool active)
    {
        enemyTurnActive = active;
        UpdateUI();
    }

    public void TriggerAttack1()
    {
        if (IsMoving || grid == null) return;
        TryAttack(1, attack1Name, attack1Anim, attack1ManaCost, attack1Damage, attack1Duration, attack1Color, attack1MinRange, attack1MaxRange, attack1IsRanged);
    }

    public void TriggerAttack2()
    {
        if (IsMoving || grid == null) return;
        TryAttack(2, attack2Name, attack2Anim, attack2ManaCost, attack2Damage, attack2Duration, attack2Color, attack2MinRange, attack2MaxRange, attack2IsRanged);
    }

    public void TriggerAttack3()
    {
        if (IsMoving || grid == null) return;
        TryAttack(3, attack3Name, attack3Anim, attack3ManaCost, attack3Damage, attack3Duration, attack3Color, attack3MinRange, attack3MaxRange, attack3IsRanged);
    }

    public void TriggerSkipTurn()
    {
        if (IsMoving || grid == null) return;
        EndTurn();
    }

    public void ToggleControls()
    {
        controlsHidden = !controlsHidden;
        UpdateUI();
    }

    void Update()
    {
        if (grid == null) return;

        // Mentre el menu de pausa es obert no es llegeix cap tecla ni clic del joc
        if (PauseMenu.IsPaused) return;

        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && (GameSettings.WasPressed(GameAction.ToggleHelp) || (GameSettings.GetKey(GameAction.ToggleHelp) == GameSettings.DefaultKey(GameAction.ToggleHelp) && keyboard.hKey.wasPressedThisFrame)))
        {
            ToggleControls();
        }

        if (moving || attacking || enemyTurnActive) return;

        Mouse mouse = Mouse.current;

        if (keyboard != null)
        {
            if (GameSettings.WasPressed(GameAction.Attack1))
            {
                TriggerAttack1();
                return;
            }
            if (GameSettings.WasPressed(GameAction.Attack2))
            {
                TriggerAttack2();
                return;
            }
            if (GameSettings.WasPressed(GameAction.Attack3))
            {
                TriggerAttack3();
                return;
            }
            if (GameSettings.WasPressed(GameAction.SkipTurn))
            {
                TriggerSkipTurn();
                return;
            }

            if (GameSettings.WasPressed(GameAction.TogglePath))
            {
                smartWalk = !smartWalk;
                UpdateUI();
            }

            Vector2Int dir = Vector2Int.zero;
            if (GameSettings.WasPressed(GameAction.MoveUp)) dir = Vector2Int.down;
            else if (GameSettings.WasPressed(GameAction.MoveRight)) dir = Vector2Int.left;
            else if (GameSettings.WasPressed(GameAction.MoveDown)) dir = Vector2Int.up;
            else if (GameSettings.WasPressed(GameAction.MoveLeft)) dir = Vector2Int.right;

            if (dir != Vector2Int.zero)
            {
                Vector2Int target = position + dir;
                if (grid.IsWalkable(target.x, target.y))
                {
                    if (grid.gridArray[position.x, position.y].dangerous && movePoints > 0)
                    {
                        //grid.DamageEffect(position.x,position.y);
                        central.Heal(1);
                    }
                    TryMove(new List<GridStat> { grid.gridArray[position.x, position.y], grid.gridArray[target.x, target.y] });
                }
                return;
            }
        }

        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            Ray ray = Camera.main.ScreenPointToRay(mouse.position.ReadValue());
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GridStat tile = hit.collider.GetComponentInParent<GridStat>();
                if (tile)
                {
                    List<GridStat> path;
                    
                    if (smartWalk)
                    {
                        path = grid.Safewalk(position, new Vector2Int(tile.x, tile.y));    
                    } else
                    {
                        path = grid.FindPath(position, new Vector2Int(tile.x, tile.y));    
                    }
                    if (path.Count > 1)
                        TryMove(path);
                }
            }
        }
    }

    public void EndTurn()
    {

        GameStats.torns++;
        manaThisTurn = Random.Range(minRoll, maxRoll + 1);
        movePoints = manaThisTurn * MovesPerMana;
        print($"New turn: {manaThisTurn} mana x {MovesPerMana} = {movePoints} move points");
        UpdateUI();
        try {
        grid.chekcDisableDamage();
        } catch (Exception){}
        
        TurnEnded?.Invoke(this);
    }

    public void changePathLogic()
    {
        smartWalk = !smartWalk;
        UpdateUI();
    }

    public Vector2 GetPosition()
    {
        return position;
    }

    void SetupAttackButtonsUI()
    {
        if (attackButton1 != null && attackButton2 != null && attackButton3 != null && skipTurnButton != null && roundText != null && controlsText != null) return;

        Canvas hudCanvas = null;
        if (MoveText != null) hudCanvas = MoveText.canvas;
        if (hudCanvas == null)
        {
            GameObject hudObj = GameObject.Find("HUD") ?? GameObject.Find("Canvas 1");
            if (hudObj != null) hudCanvas = hudObj.GetComponent<Canvas>();
        }
        if (hudCanvas == null) return;

        Transform canvasTransform = hudCanvas.transform;
        TMP_FontAsset font = MoveText != null ? MoveText.font : null;
        Texture buttonBgTex = null;
        Transform bgMana = canvasTransform.Find("BackgroundMana");
        if (bgMana != null)
        {
            RawImage raw = bgMana.GetComponent<RawImage>();
            if (raw != null) buttonBgTex = raw.texture;
        }

        Transform oldPreview = canvasTransform.Find("AttackPreviewPanel");
        if (oldPreview != null) Destroy(oldPreview.gameObject);

        attackButton1 = FindOrCreateAttackButton(canvasTransform, "AttackButton1", "AttackText1", new Vector2(-145f, -225f), new Vector2(230f, 122f), font, buttonBgTex, out attackText1);
        attackButton2 = FindOrCreateAttackButton(canvasTransform, "AttackButton2", "AttackText2", new Vector2(95f, -225f), new Vector2(230f, 122f), font, buttonBgTex, out attackText2);
        attackButton3 = FindOrCreateAttackButton(canvasTransform, "AttackButton3", "AttackText3", new Vector2(335f, -225f), new Vector2(230f, 122f), font, buttonBgTex, out attackText3);
        skipTurnButton = FindOrCreateAttackButton(canvasTransform, "SkipTurnButton", "SkipTurnText", new Vector2(560f, -225f), new Vector2(195f, 122f), font, buttonBgTex, out skipTurnText);

        roundText = FindOrCreateInfoPanel(
            canvasTransform,
            "RoundPanel",
            "RoundText",
            new Vector2(0f, 1f),
            new Vector2(0f, 1f),
            new Vector2(130f, -46f),
            new Vector2(220f, 74f),
            new Vector2(-46f, -22f),
            13f,
            font,
            buttonBgTex,
            true
        );

        controlsText = FindOrCreateInfoPanel(
            canvasTransform,
            "ControlsPanel",
            "ControlsText",
            new Vector2(1f, 1f),
            new Vector2(1f, 1f),
            new Vector2(-200f, -92f),
            new Vector2(380f, 165f),
            new Vector2(-10f, -10f),
            16.5f,
            font,
            null,
            false
        );

        ConfigureControlsPanel(canvasTransform.Find("ControlsPanel"));

        BindButtonEvents(attackButton1, TriggerAttack1);
        BindButtonEvents(attackButton2, TriggerAttack2);
        BindButtonEvents(attackButton3, TriggerAttack3);
        BindButtonEvents(skipTurnButton, TriggerSkipTurn);

        ApplyHudLayout(canvasTransform);
    }

    Button FindOrCreateAttackButton(Transform parent, string btnName, string txtName, Vector2 anchoredPos, Vector2 size, TMP_FontAsset font, Texture bgTex, out TMP_Text label)
    {
        Transform existing = parent.Find(btnName);
        if (existing != null)
        {
            RectTransform existingRt = existing.GetComponent<RectTransform>();
            if (existingRt != null)
            {
                existingRt.anchorMin = new Vector2(0.5f, 0.5f);
                existingRt.anchorMax = new Vector2(0.5f, 0.5f);
                existingRt.anchoredPosition = anchoredPos;
                existingRt.sizeDelta = size;
            }
            Button btn = existing.GetComponent<Button>();
            if (btn != null)
            {
                ColorBlock existingCb = btn.colors;
                existingCb.normalColor = Color.white;
                existingCb.highlightedColor = new Color(1f, 0.92f, 0.75f, 1f);
                existingCb.pressedColor = new Color(0.8f, 0.7f, 0.55f, 1f);
                existingCb.disabledColor = new Color(0.76f, 0.72f, 0.66f, 0.94f);
                btn.colors = existingCb;
            }
            label = existing.GetComponentInChildren<TMP_Text>();
            if (label != null)
            {
                if (font != null) label.font = font;
                label.fontSize = 18f;
                label.enableAutoSizing = true;
                label.fontSizeMin = 11f;
                label.fontSizeMax = 18f;
                RectTransform labelRt = label.GetComponent<RectTransform>();
                if (labelRt != null) labelRt.sizeDelta = new Vector2(-36f, -22f);
            }
            return btn;
        }

        GameObject btnObj = new GameObject(btnName, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage), typeof(Button));
        btnObj.transform.SetParent(parent, false);
        RectTransform rt = btnObj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        RawImage raw = btnObj.GetComponent<RawImage>();
        if (bgTex != null) raw.texture = bgTex;
        else raw.color = new Color(0.92f, 0.85f, 0.72f, 0.95f);

        Button button = btnObj.GetComponent<Button>();
        button.targetGraphic = raw;
        ColorBlock cb = button.colors;
        cb.normalColor = Color.white;
        cb.highlightedColor = new Color(1f, 0.92f, 0.75f, 1f);
        cb.pressedColor = new Color(0.8f, 0.7f, 0.55f, 1f);
        cb.disabledColor = new Color(0.76f, 0.72f, 0.66f, 0.94f);
        button.colors = cb;

        GameObject txtObj = new GameObject(txtName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(btnObj.transform, false);
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.anchoredPosition = Vector2.zero;
        txtRt.sizeDelta = new Vector2(-36f, -22f);

        TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.fontSize = 18f;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 11f;
        tmp.fontSizeMax = 18f;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        tmp.raycastTarget = false;

        label = tmp;
        return button;
    }

    // ---------------- Distribucio del HUD ----------------
    // Tot el HUD s'escala amb la pantalla (referencia 1920x1080) i queda enganxat a les cantonades:
    //  - a baix a l'esquerra: mana i vida
    //  - a baix a la dreta: els 3 atacs i "Saltar ronda"
    //  - a dalt: ronda (esquerra), vida de l'enemic (centre) i controls (dreta)
    const float HudMargin = 30f;
    static readonly Vector2 StatCardSize = new Vector2(250f, 115f);
    static readonly Vector2 AttackCardSize = new Vector2(300f, 160f);
    static readonly Vector2 SkipCardSize = new Vector2(210f, 160f);
    const float AttackFontMax = 26f;
    const float SkipFontMax = 30f;
    const float RoundFontMax = 28f;
    const float CardGap = 18f;

    void ApplyHudLayout(Transform canvasTransform)
    {
        if (canvasTransform == null) return;

        CanvasScaler scaler = canvasTransform.GetComponent<CanvasScaler>();
        if (scaler != null)
        {
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
        }

        // Vida i mana (a baix a l'esquerra, mana a sobre)
        LayoutStatCard(canvasTransform, "BackgroundHealth", "HeartIcon", "HealthText", new Vector2(HudMargin, HudMargin));
        LayoutStatCard(canvasTransform, "BackgroundMana", "ManaIcon", "ManaText", new Vector2(HudMargin, HudMargin + StatCardSize.y + CardGap));

        // Atacs i saltar ronda (a baix a la dreta)
        float x = -HudMargin;
        LayoutCard(canvasTransform.Find("SkipTurnButton"), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(x, HudMargin), SkipCardSize, SkipFontMax);
        x -= SkipCardSize.x + CardGap * 2f;
        LayoutCard(canvasTransform.Find("AttackButton3"), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(x, HudMargin), AttackCardSize, AttackFontMax);
        x -= AttackCardSize.x + CardGap;
        LayoutCard(canvasTransform.Find("AttackButton2"), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(x, HudMargin), AttackCardSize, AttackFontMax);
        x -= AttackCardSize.x + CardGap;
        LayoutCard(canvasTransform.Find("AttackButton1"), new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(x, HudMargin), AttackCardSize, AttackFontMax);

        // Ronda (a dalt a l'esquerra)
        LayoutCard(canvasTransform.Find("RoundPanel"), new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(HudMargin, -HudMargin), new Vector2(250f, 105f), RoundFontMax);

        // Controls (a dalt a la dreta, sense fons)
        Transform controls = canvasTransform.Find("ControlsPanel");
        if (controls != null)
        {
            PlaceRect(controls, new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(-HudMargin, -HudMargin), new Vector2(560f, 230f));
            TMP_Text t = controls.GetComponentInChildren<TMP_Text>();
            if (t != null)
            {
                Stretch(t.rectTransform, 0f, 0f);
                t.enableAutoSizing = true;
                t.fontSizeMin = 12f;
                t.fontSizeMax = 26f;
                t.lineSpacing = 6f;
            }
        }

        // Vida de l'enemic (a dalt al centre)
        Transform boss = canvasTransform.Find("BossHealthPanel");
        if (boss != null)
        {
            PlaceRect(boss, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -HudMargin + 10f), new Vector2(600f, 125f));

            Transform bar = boss.Find("BossHealthBarBg");
            if (bar != null)
            {
                RectTransform br = (RectTransform)bar;
                br.anchorMin = br.anchorMax = new Vector2(0.5f, 0.5f);
                br.pivot = new Vector2(0.5f, 0.5f);
                br.anchoredPosition = new Vector2(0f, -18f);
                br.sizeDelta = new Vector2(400f, 20f);
                br.localScale = Vector3.one;
            }

            Transform bossText = boss.Find("BossHealthText");
            if (bossText != null)
            {
                RectTransform tr = (RectTransform)bossText;
                tr.anchorMin = tr.anchorMax = new Vector2(0.5f, 0.5f);
                tr.pivot = new Vector2(0.5f, 0.5f);
                tr.anchoredPosition = new Vector2(0f, 16f);
                tr.sizeDelta = new Vector2(400f, 34f);
                tr.localScale = Vector3.one;
                TMP_Text t = bossText.GetComponent<TMP_Text>();
                if (t != null)
                {
                    t.margin = Vector4.zero;
                    t.textWrappingMode = TextWrappingModes.NoWrap;
                    t.alignment = TextAlignmentOptions.Center;
                    UiTextFit.Fit(t, 26f);
                }
            }
        }
    }

    void LayoutStatCard(Transform canvasTransform, string bgName, string iconName, string textName, Vector2 bottomLeft)
    {
        Transform bg = canvasTransform.Find(bgName);
        if (bg != null)
            PlaceRect(bg, Vector2.zero, Vector2.zero, bottomLeft, StatCardSize);

        Vector2 center = bottomLeft + StatCardSize * 0.5f;

        Transform icon = canvasTransform.Find(iconName);
        if (icon != null)
        {
            // Icona a l'esquerra de la targeta
            PlaceRect(icon, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(bottomLeft.x + StatCardSize.x * 0.32f, center.y), new Vector2(68f, 68f));
        }

        Transform text = canvasTransform.Find(textName);
        if (text != null)
        {
            // Numero a la dreta de la targeta
            PlaceRect(text, Vector2.zero, new Vector2(0.5f, 0.5f), new Vector2(bottomLeft.x + StatCardSize.x * 0.66f, center.y), new Vector2(100f, 70f));
            TMP_Text t = text.GetComponent<TMP_Text>();
            if (t != null)
            {
                // El prefab tenia un marge dret de 145 que deixava el numero minuscul
                t.margin = Vector4.zero;
                t.enableAutoSizing = true;
                t.fontSizeMin = 20f;
                t.fontSizeMax = 54f;
                t.alignment = TextAlignmentOptions.Center;
                t.textWrappingMode = TextWrappingModes.NoWrap;
            }
        }
    }

    // Targeta de pergami amb text a dins (deixa marge per les vores cremades)
    void LayoutCard(Transform card, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size, float maxFont)
    {
        if (card == null) return;
        PlaceRect(card, anchor, pivot, pos, size);

        TMP_Text t = card.GetComponentInChildren<TMP_Text>();
        if (t == null) return;

        // La textura de pergami te vores cremades i transparents: deixem prou marge
        Stretch(t.rectTransform, size.x * 0.17f, size.y * 0.22f);
        t.margin = Vector4.zero;
        t.enableAutoSizing = false;   // la mida la calcula FitText cada cop que canvia el text
        t.fontSizeMax = maxFont;
        t.fontSize = maxFont;
        t.alignment = TextAlignmentOptions.Center;
        t.textWrappingMode = TextWrappingModes.NoWrap;
        t.overflowMode = TextOverflowModes.Overflow;
        t.lineSpacing = 4f;
    }

    // Ajusta la mida de la lletra perque el text hi capiga sencer dins la targeta
    static void FitText(TMP_Text t, float maxSize, float minSize = 10f)
    {
        UiTextFit.Fit(t, maxSize, minSize);
    }

    static RectTransform PlaceRect(Transform tr, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
    {
        RectTransform rt = tr as RectTransform;
        if (rt == null) return null;
        rt.anchorMin = anchor;
        rt.anchorMax = anchor;
        rt.pivot = pivot;
        rt.anchoredPosition = pos;
        rt.sizeDelta = size;
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;
        return rt;
    }

    static void Stretch(RectTransform rt, float padX, float padY)
    {
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.offsetMin = new Vector2(padX, padY);
        rt.offsetMax = new Vector2(-padX, -padY);
        rt.localScale = Vector3.one;
    }

    TMP_Text FindOrCreateInfoPanel(Transform parent, string panelName, string txtName, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPos, Vector2 size, Vector2 txtPadding, float maxFontSize, TMP_FontAsset font, Texture bgTex, bool showBackground)
    {
        Transform existing = parent.Find(panelName);
        if (existing != null)
        {
            RectTransform existingRt = existing.GetComponent<RectTransform>();
            if (existingRt != null)
            {
                existingRt.anchorMin = anchorMin;
                existingRt.anchorMax = anchorMax;
                existingRt.anchoredPosition = anchoredPos;
                existingRt.sizeDelta = size;
            }
            RawImage existingRaw = existing.GetComponent<RawImage>();
            if (existingRaw != null)
            {
                existingRaw.enabled = showBackground;
            }
            TMP_Text txt = existing.GetComponentInChildren<TMP_Text>();
            if (txt != null)
            {
                if (font != null) txt.font = font;
                txt.fontSize = maxFontSize;
                txt.fontSizeMax = maxFontSize;
                RectTransform txtRtExisting = txt.GetComponent<RectTransform>();
                if (txtRtExisting != null) txtRtExisting.sizeDelta = txtPadding;
            }
            return txt;
        }

        GameObject panelObj = new GameObject(panelName, typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
        panelObj.transform.SetParent(parent, false);
        RectTransform rt = panelObj.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        RawImage raw = panelObj.GetComponent<RawImage>();
        if (bgTex != null) raw.texture = bgTex;
        raw.color = new Color(1f, 1f, 1f, 0.95f);
        raw.raycastTarget = false;
        raw.enabled = showBackground;

        GameObject txtObj = new GameObject(txtName, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        txtObj.transform.SetParent(panelObj.transform, false);
        RectTransform txtRt = txtObj.GetComponent<RectTransform>();
        txtRt.anchorMin = Vector2.zero;
        txtRt.anchorMax = Vector2.one;
        txtRt.anchoredPosition = Vector2.zero;
        txtRt.sizeDelta = txtPadding;

        TextMeshProUGUI tmp = txtObj.GetComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.fontSize = maxFontSize;
        tmp.enableAutoSizing = true;
        tmp.fontSizeMin = 9f;
        tmp.fontSizeMax = maxFontSize;
        tmp.fontStyle = FontStyles.Bold;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.color = new Color(0.1f, 0.1f, 0.1f, 1f);
        tmp.raycastTarget = false;

        return tmp;
    }

    void ConfigureControlsPanel(Transform panelTransform)
    {
        if (panelTransform == null) return;

        RawImage raw = panelTransform.GetComponent<RawImage>();
        if (raw != null)
        {
            raw.enabled = false;
        }

        if (controlsText != null)
        {
            controlsText.alignment = TextAlignmentOptions.TopRight;
            controlsText.color = new Color(1f, 0.97f, 0.88f, 1f);
            controlsText.fontSizeMin = 11f;
            controlsText.fontSizeMax = 16.5f;
            controlsText.fontSize = 16.5f;
            controlsText.raycastTarget = true;

            Outline outline = controlsText.GetComponent<Outline>();
            if (outline == null) outline = controlsText.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.05f, 0.05f, 0.05f, 0.95f);
            outline.effectDistance = new Vector2(1.8f, -1.8f);

            Shadow shadow = controlsText.GetComponent<Shadow>();
            if (shadow == null) shadow = controlsText.gameObject.AddComponent<Shadow>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(2.5f, -2.5f);

            Button toggleBtn = controlsText.GetComponent<Button>();
            if (toggleBtn == null) toggleBtn = controlsText.gameObject.AddComponent<Button>();
            toggleBtn.transition = Selectable.Transition.None;
            BindButtonEvents(toggleBtn, ToggleControls);
        }
    }

    void BindButtonEvents(Button btn, UnityEngine.Events.UnityAction clickAction)
    {
        if (btn == null) return;
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(clickAction);
    }

    void UpdateUI()
    {
        if (MoveText) MoveText.text = $"{movePoints}";

        bool canAct = !moving && !attacking && !enemyTurnActive && (enemy == null || enemy.vida > 0);

        UpdateAttackButtonVisual(attackButton1, attackText1, AttackTitleWithKey(attack1Name, GameAction.Attack1), attack1TypeLabel, attack1Damage, attack1ManaCost, attack1MinRange, attack1MaxRange, canAct);
        UpdateAttackButtonVisual(attackButton2, attackText2, AttackTitleWithKey(attack2Name, GameAction.Attack2), attack2TypeLabel, attack2Damage, attack2ManaCost, attack2MinRange, attack2MaxRange, canAct);
        UpdateAttackButtonVisual(attackButton3, attackText3, AttackTitleWithKey(attack3Name, GameAction.Attack3), attack3TypeLabel, attack3Damage, attack3ManaCost, attack3MinRange, attack3MaxRange, canAct);

        if (skipTurnButton != null)
        {
            skipTurnButton.interactable = canAct;
        }
        if (skipTurnText != null)
        {
            skipTurnText.text = $"Skip\nTurn\n<color=#1C3ADA>({GameSettings.KeyName(GameAction.SkipTurn)})</color>";
            FitText(skipTurnText, SkipFontMax);
        }
        FitText(attackText1, AttackFontMax);
        FitText(attackText2, AttackFontMax);
        FitText(attackText3, AttackFontMax);

        if (roundText != null)
        {
            int currentRound = Mathf.Max(1, GameStats.torns);
            roundText.text = $"Phase: {GameStats.fase}\nRound: {currentRound}";
            FitText(roundText, RoundFontMax);
        }

        if (controlsText != null)
        {
            if (controlsHidden)
            {
                controlsText.text = $"{GameSettings.KeyName(GameAction.ToggleHelp)}: Show controls";
            }
            else
            {
                string modeCami = smartWalk ? "Safe" : "Normal";
                string k(GameAction a) => GameSettings.KeyName(a);
                bool defaultArrows = GameSettings.GetKey(GameAction.MoveUp) == Key.UpArrow && GameSettings.GetKey(GameAction.MoveDown) == Key.DownArrow
                    && GameSettings.GetKey(GameAction.MoveLeft) == Key.LeftArrow && GameSettings.GetKey(GameAction.MoveRight) == Key.RightArrow;
                string moveKeys = defaultArrows ? "Arrows" : $"{k(GameAction.MoveUp)}/{k(GameAction.MoveDown)}/{k(GameAction.MoveLeft)}/{k(GameAction.MoveRight)}";
                controlsText.text = $"CONTROLS ({k(GameAction.ToggleHelp)}: Hide)\nClick / {moveKeys}: Move\n{k(GameAction.Attack1)}, {k(GameAction.Attack2)}, {k(GameAction.Attack3)}: Attack\n{k(GameAction.SkipTurn)}: Skip turn\n{k(GameAction.TogglePath)}: {modeCami} path - Esc: Pause";
            }
        }
    }

    public int GetDistanceToBoss()
    {
        int dx = 0;
        if (position.x < 8) dx = 8 - position.x;
        else if (position.x > 10) dx = position.x - 10;

        int dy = 0;
        if (position.y < 8) dy = 8 - position.y;
        else if (position.y > 10) dy = position.y - 10;

        return Mathf.Max(1, Mathf.Max(dx, dy));
    }

    bool CheckAttackRange(int minRange, int maxRange, out bool tooClose, out bool tooFar)
    {
        int dist = GetDistanceToBoss();
        tooClose = dist < minRange;
        tooFar = dist > maxRange;
        return !tooClose && !tooFar;
    }

    void UpdateAttackButtonVisual(Button btn, TMP_Text txt, string attackTitle, string typeLabel, int dmg, int cost, int minRange, int maxRange, bool canAct)
    {
        bool enoughMana = movePoints >= cost;
        bool inRange = CheckAttackRange(minRange, maxRange, out bool tooClose, out bool _);
        if (btn != null)
        {
            btn.interactable = canAct && enoughMana && inRange;
        }
        if (txt != null)
        {
            if (!inRange)
            {
                string reason = tooClose ? "Too close" : "Too far";
                txt.text = $"<b>{attackTitle}</b>\n<size=82%>{dmg} Dmg - {typeLabel}</size>\n<size=82%><color=#B81D1D>{reason} ({cost}M)</color></size>";
            }
            else
            {
                string manaColor = enoughMana ? "#1C3ADA" : "#B81D1D";
                txt.text = $"<b>{attackTitle}</b>\n<size=82%>{dmg} Dmg - {typeLabel}</size>\n<size=82%><color={manaColor}>{cost} Mana</color></size>";
            }
        }
    }

    void TryAttack(int attackNumber, string attackTitle, string animState, int manaCost, int damage, float duration, Color attackColor, int minRange, int maxRange, bool isRanged)
    {
        if (!enemy) enemy = FindFirstObjectByType<EnemyLogic>();
        if (enemy != null && enemy.vida <= 0) return;

        if (!CheckAttackRange(minRange, maxRange, out bool tooClose, out bool _))
        {
            string msg = tooClose ? "Too close for this ranged attack!" : "Too far for this attack!";
            print($"{attackTitle}: {msg} (Current distance: {GetDistanceToBoss()}, Allowed range: {minRange}-{maxRange})");
            GameAudio.Play(Sfx.ActionDenied, 0.7f, 0f);
            return;
        }

        if (movePoints < manaCost)
        {
            if (movePoints == 0)
                print("End your turn to get more moves");
            else
                print($"Not enough mana for Attack {attackNumber}: need {manaCost}, have {movePoints}");
            GameAudio.Play(Sfx.ActionDenied, 0.7f, 0f);
            return;
        }

        movePoints -= manaCost;
        UpdateUI();
        StartCoroutine(PerformAttack(attackNumber, animState, damage, duration, attackColor, isRanged));
    }

    IEnumerator PerformAttack(int attackNumber, string animState, int damage, float duration, Color attackColor, bool isRanged)
    {
        attacking = true;
        UpdateUI();

        if (!animator) animator = GetComponentInChildren<Animator>();
        if (animator)
        {
            animator.CrossFadeInFixedTime(animState, 0.1f, 0, 0f);
        }

        // So de llançament de l'atac
        bool isMagic = animState != null && animState.StartsWith("Mage");
        if (!isRanged) GameAudio.Play(Sfx.MeleeSwing, 0.8f);
        else if (isMagic) GameAudio.Play(Sfx.MagicCast, 0.8f);
        else GameAudio.Play(Sfx.ArrowShoot, 0.9f);

        if (enemy != null)
        {
            GameObject projModel = isRanged ? projectileVisualPrefab : null;
            enemy.PlayPlayerAttackLaunchEffect(transform.position, attackNumber, attackColor, projModel, characterSharedMaterial, isRanged);
        }

        yield return new WaitForSeconds(duration * 0.5f);

        if (enemy != null)
        {
            // So de l'impacte
            if (!isRanged) GameAudio.Play(Sfx.MeleeHit);
            else if (isMagic) GameAudio.Play(Sfx.MagicHit);
            else GameAudio.Play(Sfx.ArrowHit);

            enemy.TakeDamage(damage);
            print($"Attack {attackNumber}: dealt {damage} damage to boss! Boss HP: {enemy.vida}");
        }

        yield return new WaitForSeconds(duration * 0.5f);

        attacking = false;
        UpdateUI();
        print($"Move points left: {movePoints}");

        CheckAutoEndTurn();
    }

    void SetupCharacterClassAndWeapons()
    {
        if (classConfigured) return;
        classConfigured = true;

        SkinnedMeshRenderer smr = GetComponentInChildren<SkinnedMeshRenderer>();
        if (smr != null && smr.sharedMaterial != null)
        {
            characterSharedMaterial = smr.sharedMaterial;
        }

        string lowerName = gameObject.name.ToLowerInvariant();
        if (lowerName.Contains("mage"))
        {
            // Mag (Nomes a distancia): Basto magic + Llibre d'encanteris
            classMaxHealth = 16;
            minRoll = 4;
            maxRoll = 7;
            moveSpeed = 8f;

            attack1Name = "1. Arcane Orb";
            attack2Name = "2. Fireball";
            attack3Name = "3. Meteor";

            attack1TypeLabel = "Ranged";
            attack2TypeLabel = "Ranged";
            attack3TypeLabel = "Ranged";

            attack1MinRange = 3; attack1MaxRange = 9;
            attack2MinRange = 3; attack2MaxRange = 9;
            attack3MinRange = 3; attack3MaxRange = 9;

            attack1IsRanged = true;
            attack2IsRanged = true;
            attack3IsRanged = true;

            attack1Anim = "Mage_Attack1";
            attack2Anim = "Mage_Attack2";
            attack3Anim = "Mage_Attack3";

            attack1ManaCost = 2; attack1Damage = 3; attack1Duration = 0.85f;
            attack2ManaCost = 4; attack2Damage = 6; attack2Duration = 1.05f;
            attack3ManaCost = 6; attack3Damage = 10; attack3Duration = 1.15f;

            attack1Color = new Color(0.3f, 0.85f, 1f, 1f);
            attack2Color = new Color(0.75f, 0.3f, 1f, 1f);
            attack3Color = new Color(1f, 0.45f, 0.1f, 1f);
            projectileVisualPrefab = null;
        }
        else if (lowerName.Contains("ranger"))
        {
            // Arquer (Nomes a distancia): Arc + Fletxa
            classMaxHealth = 18;
            minRoll = 3;
            maxRoll = 6;
            moveSpeed = 9f;

            attack1Name = "1. Arrow Shot";
            attack2Name = "2. Piercing Shot";
            attack3Name = "3. Arrow Rain";

            attack1TypeLabel = "Ranged";
            attack2TypeLabel = "Ranged";
            attack3TypeLabel = "Ranged";

            attack1MinRange = 3; attack1MaxRange = 9;
            attack2MinRange = 3; attack2MaxRange = 9;
            attack3MinRange = 3; attack3MaxRange = 9;

            attack1IsRanged = true;
            attack2IsRanged = true;
            attack3IsRanged = true;

            attack1Anim = "Ranger_Attack1";
            attack2Anim = "Ranger_Attack2";
            attack3Anim = "Ranger_Attack3";

            attack1ManaCost = 2; attack1Damage = 3; attack1Duration = 0.75f;
            attack2ManaCost = 3; attack2Damage = 5; attack2Duration = 0.85f;
            attack3ManaCost = 5; attack3Damage = 8; attack3Duration = 1.0f;

            attack1Color = new Color(0.35f, 1f, 0.45f, 1f);
            attack2Color = new Color(0.85f, 1f, 0.25f, 1f);
            attack3Color = new Color(0.15f, 1f, 0.75f, 1f);
            projectileVisualPrefab = rightHandWeaponPrefab;
        }
        else if (lowerName.Contains("rogue"))
        {
            // Murri (Atacs Mixtos): Daga (Mele) + Ballesta d'una ma (Distancia) + Assalt Ombra (Mixt)
            classMaxHealth = 22;
            minRoll = 4;
            maxRoll = 7;
            moveSpeed = 10f;

            attack1Name = "1. Stab";
            attack2Name = "2. Crossbow";
            attack3Name = "3. Mixed Assault";

            attack1TypeLabel = "Melee";
            attack2TypeLabel = "Ranged";
            attack3TypeLabel = "Mixed";

            attack1MinRange = 1; attack1MaxRange = 2;
            attack2MinRange = 3; attack2MaxRange = 9;
            attack3MinRange = 1; attack3MaxRange = 4;

            attack1IsRanged = false;
            attack2IsRanged = true;
            attack3IsRanged = true;

            attack1Anim = "Rogue_Attack1";
            attack2Anim = "Rogue_Attack2";
            attack3Anim = "Rogue_Attack3";

            attack1ManaCost = 1; attack1Damage = 3; attack1Duration = 0.65f;
            attack2ManaCost = 3; attack2Damage = 5; attack2Duration = 0.8f;
            attack3ManaCost = 5; attack3Damage = 8; attack3Duration = 0.95f;

            attack1Color = new Color(0.4f, 1f, 0.7f, 1f);
            attack2Color = new Color(0.7f, 0.25f, 1f, 1f);
            attack3Color = new Color(1f, 0.15f, 0.35f, 1f);
            projectileVisualPrefab = rightHandWeaponPrefab;
        }
        else if (lowerName.Contains("barbarian"))
        {
            // Barbar (Nomes Mele d'aprop): Destral de 2 mans + Destral d'1 ma
            classMaxHealth = 32;
            minRoll = 3;
            maxRoll = 6;
            moveSpeed = 8.5f;

            attack1Name = "1. Axe Chop";
            attack2Name = "2. Whirlwind";
            attack3Name = "3. Brutal Fury";

            attack1TypeLabel = "Melee";
            attack2TypeLabel = "Melee";
            attack3TypeLabel = "Melee";

            attack1MinRange = 1; attack1MaxRange = 2;
            attack2MinRange = 1; attack2MaxRange = 2;
            attack3MinRange = 1; attack3MaxRange = 2;

            attack1IsRanged = false;
            attack2IsRanged = false;
            attack3IsRanged = false;

            attack1Anim = "Barbarian_Attack1";
            attack2Anim = "Barbarian_Attack2";
            attack3Anim = "Barbarian_Attack3";

            attack1ManaCost = 2; attack1Damage = 4; attack1Duration = 0.85f;
            attack2ManaCost = 4; attack2Damage = 7; attack2Duration = 1.15f;
            attack3ManaCost = 6; attack3Damage = 11; attack3Duration = 1.1f;

            attack1Color = new Color(1f, 0.55f, 0.15f, 1f);
            attack2Color = new Color(1f, 0.3f, 0.1f, 1f);
            attack3Color = new Color(1f, 0.1f, 0.1f, 1f);
            projectileVisualPrefab = null;
        }
        else
        {
            // Cavaller / Knight (Nomes Mele d'aprop): Espasa + Escut
            classMaxHealth = 28;
            minRoll = 3;
            maxRoll = 5;
            moveSpeed = 7.5f;

            attack1Name = "1. Sword Slash";
            attack2Name = "2. Shield Bash";
            attack3Name = "3. Lunge";

            attack1TypeLabel = "Melee";
            attack2TypeLabel = "Melee";
            attack3TypeLabel = "Melee";

            attack1MinRange = 1; attack1MaxRange = 2;
            attack2MinRange = 1; attack2MaxRange = 2;
            attack3MinRange = 1; attack3MaxRange = 2;

            attack1IsRanged = false;
            attack2IsRanged = false;
            attack3IsRanged = false;

            attack1Anim = "Knight_Attack1";
            attack2Anim = "Knight_Attack2";
            attack3Anim = "Knight_Attack3";

            attack1ManaCost = 2; attack1Damage = 3; attack1Duration = 0.8f;
            attack2ManaCost = 4; attack2Damage = 6; attack2Duration = 1.0f;
            attack3ManaCost = 6; attack3Damage = 9; attack3Duration = 1.1f;

            attack1Color = new Color(0.95f, 0.9f, 0.55f, 1f);
            attack2Color = new Color(1f, 0.8f, 0.2f, 1f);
            attack3Color = new Color(0.4f, 0.75f, 1f, 1f);
            projectileVisualPrefab = null;
        }

        if (!central) central = GetComponentInParent<CentralPlayer>();
        if (central != null)
        {
            central.SetClassMaxHealth(classMaxHealth);
        }

        EquipWeaponInSlot("handslot.r", "EquippedWeapon_R", rightHandWeaponPrefab, lowerName, true);
        EquipWeaponInSlot("handslot.l", "EquippedWeapon_L", leftHandWeaponPrefab, lowerName, false);
    }

    void EquipWeaponInSlot(string slotName, string instanceName, GameObject weaponPrefab, string lowerClassName, bool isRightHand)
    {
        Transform handSlot = FindDeepChild(transform, slotName);
        if (handSlot == null) return;

        Transform existing = handSlot.Find(instanceName);
        if (existing != null) return;

        GameObject weaponInstance = null;
        if (weaponPrefab != null)
        {
            weaponInstance = Instantiate(weaponPrefab, handSlot);
            weaponInstance.name = instanceName;
            ApplyNaturalWeaponTransform(weaponInstance.transform, lowerClassName, isRightHand);

            if (characterSharedMaterial != null)
            {
                Renderer[] renderers = weaponInstance.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer r in renderers)
                {
                    r.sharedMaterial = characterSharedMaterial;
                }
            }
        }
        else
        {
            weaponInstance = CreateProceduralFallbackWeapon(handSlot, instanceName, lowerClassName, isRightHand);
        }

        if (weaponInstance != null)
        {
            Collider[] cols = weaponInstance.GetComponentsInChildren<Collider>(true);
            foreach (Collider c in cols)
            {
                Destroy(c);
            }
        }
    }

    void ApplyNaturalWeaponTransform(Transform weaponTransform, string lowerClassName, bool isRightHand)
    {
        weaponTransform.localScale = Vector3.one;

        if (lowerClassName.Contains("mage"))
        {
            if (isRightHand)
            {
                // Basto magic dret (inclinat 70 graus cap amunt en lloc d'horitzontal)
                weaponTransform.localPosition = new Vector3(0.02f, 0.02f, 0f);
                weaponTransform.localRotation = Quaternion.Euler(0f, 0f, -68f);
            }
            else
            {
                // Llibre d'encanteris obert davant de la ma esquerra mirant cap amunt
                weaponTransform.localPosition = new Vector3(-0.04f, 0.12f, 0.10f);
                weaponTransform.localRotation = Quaternion.Euler(-35f, 0f, 90f);
            }
        }
        else if (lowerClassName.Contains("ranger"))
        {
            if (isRightHand)
            {
                // Fletxa apuntant cap endavant
                weaponTransform.localPosition = new Vector3(0f, 0.05f, 0f);
                weaponTransform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            }
            else
            {
                // Arc vertical agafat per l'empunyadura de fusta amb la corda cap enrere
                weaponTransform.localPosition = new Vector3(0f, 0.08f, 0.02f);
                weaponTransform.localRotation = Quaternion.Euler(0f, -75f, -90f);
            }
        }
        else if (lowerClassName.Contains("rogue"))
        {
            if (isRightHand)
            {
                // Daga lleugerament inclinada cap amunt en posicio de guardia
                weaponTransform.localPosition = new Vector3(0f, -0.03f, 0f);
                weaponTransform.localRotation = Quaternion.Euler(0f, 0f, -22f);
            }
            else
            {
                // Ballesta d'una ma apuntant endavant amb l'arc horitzontal i agafada pel gallet
                weaponTransform.localPosition = new Vector3(0.02f, 0.04f, 0f);
                weaponTransform.localRotation = Quaternion.Euler(-90f, 0f, 90f);
            }
        }
        else if (lowerClassName.Contains("barbarian"))
        {
            if (isRightHand)
            {
                // Destral gran inclinada 35 graus cap amunt
                weaponTransform.localPosition = new Vector3(0f, -0.05f, 0f);
                weaponTransform.localRotation = Quaternion.Euler(0f, 0f, -35f);
            }
            else
            {
                // Destral d'una ma inclinada 35 graus cap amunt amb la fulla mirant avall/endavant
                weaponTransform.localPosition = new Vector3(0f, -0.04f, 0f);
                weaponTransform.localRotation = Quaternion.Euler(0f, 0f, 35f);
            }
        }
        else
        {
            // Cavaller (Knight)
            if (isRightHand)
            {
                // Espasa d'una ma agafada pel puny i inclinada 35 graus cap amunt
                weaponTransform.localPosition = new Vector3(0f, -0.04f, 0f);
                weaponTransform.localRotation = Quaternion.Euler(0f, 0f, -35f);
            }
            else
            {
                // Escut dret (punta cap avall) i separat de la ma perque l'emblema quedi a fora
                weaponTransform.localPosition = new Vector3(0f, 0.06f, 0.14f);
                weaponTransform.localRotation = Quaternion.Euler(-20f, 0f, 90f);
            }
        }
    }

    Transform FindDeepChild(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName) return child;
            Transform found = FindDeepChild(child, childName);
            if (found != null) return found;
        }
        return null;
    }

    GameObject CreateProceduralFallbackWeapon(Transform handSlot, string instanceName, string lowerClassName, bool isRightHand)
    {
        GameObject root = new GameObject(instanceName);
        root.transform.SetParent(handSlot, false);
        root.transform.localPosition = Vector3.zero;
        root.transform.localRotation = Quaternion.identity;
        root.transform.localScale = Vector3.one;

        Shader shader = characterSharedMaterial != null ? characterSharedMaterial.shader : Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null) shader = Shader.Find("Standard");

        if (lowerClassName.Contains("mage") && isRightHand)
        {
            GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaft.transform.SetParent(root.transform, false);
            shaft.transform.localPosition = new Vector3(0f, 0.25f, 0f);
            shaft.transform.localScale = new Vector3(0.08f, 0.75f, 0.08f);
            SetPrimitiveColor(shaft, shader, new Color(0.45f, 0.25f, 0.1f));

            GameObject orb = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            orb.transform.SetParent(root.transform, false);
            orb.transform.localPosition = new Vector3(0f, 1.05f, 0f);
            orb.transform.localScale = new Vector3(0.24f, 0.24f, 0.24f);
            SetPrimitiveColor(orb, shader, new Color(0.25f, 0.85f, 1f));
        }
        else if (lowerClassName.Contains("mage") && !isRightHand)
        {
            GameObject book = GameObject.CreatePrimitive(PrimitiveType.Cube);
            book.transform.SetParent(root.transform, false);
            book.transform.localPosition = new Vector3(0f, 0.08f, 0.12f);
            book.transform.localScale = new Vector3(0.32f, 0.08f, 0.42f);
            SetPrimitiveColor(book, shader, new Color(0.55f, 0.15f, 0.65f));
        }
        else if (lowerClassName.Contains("ranger") && !isRightHand)
        {
            GameObject bow = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            bow.transform.SetParent(root.transform, false);
            bow.transform.localPosition = new Vector3(0f, 0f, 0.15f);
            bow.transform.localScale = new Vector3(0.06f, 0.65f, 0.12f);
            SetPrimitiveColor(bow, shader, new Color(0.55f, 0.32f, 0.12f));
        }
        else
        {
            GameObject blade = GameObject.CreatePrimitive(PrimitiveType.Cube);
            blade.transform.SetParent(root.transform, false);
            blade.transform.localPosition = new Vector3(0f, 0.45f, 0f);
            blade.transform.localScale = new Vector3(0.08f, 0.85f, 0.14f);
            SetPrimitiveColor(blade, shader, new Color(0.8f, 0.82f, 0.88f));
        }

        return root;
    }

    void SetPrimitiveColor(GameObject obj, Shader shader, Color col)
    {
        Renderer r = obj.GetComponent<Renderer>();
        if (r != null && shader != null)
        {
            Material mat = new Material(shader);
            mat.color = col;
            if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", col);
            r.material = mat;
        }
    }

    void TryMove(List<GridStat> path)
    {
        int steps = path.Count - 1;

        if (steps > movePoints)
        {
            if (movePoints == 0)
                print("End your turn to get more moves");
            else
                print($"Not enough mana: need {steps}, have {movePoints}");
            return;
        }

        movePoints -= steps;
        GameStats.passos += steps;
        UpdateUI();
        StartCoroutine(FollowPath(path));
    }

    static readonly int WalkStateHash = Animator.StringToHash("Walk");
    static readonly int IdleStateHash = Animator.StringToHash("Idle");

    // Canvia entre l'animacio de caminar i la de repos (estat "Walk" de l'Animator Idle_A)
    void SetWalkingAnimation(bool walking)
    {
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!animator || animator.runtimeAnimatorController == null) return;

        int state = walking ? WalkStateHash : IdleStateHash;
        if (animator.HasState(0, state))
            animator.CrossFadeInFixedTime(state, 0.12f, 0);
    }

    IEnumerator FollowPath(List<GridStat> path)
    {
        moving = true;
        UpdateUI();
        SetWalkingAnimation(true);
        MirarObjectiu facing = GetComponent<MirarObjectiu>();
        foreach (GridStat tile in path)
        {
            if (tile.dangerous)
            {
                print(tile + " dangerous");
                central.TakeDamage(1);

                if (effectDo)
                {
                    grid.DamageEffect(tile.x,tile.y);
                    effectDo = false;    
                } else
                {
                    effectDo = true;
                }
                //Comp
            }
            if (tile.heal)
            {
                print(tile + " heal");
                central.Heal(5);
                grid.UseHeal(tile.x, tile.y);
            }

            Vector3 target = TileWorld(new Vector2Int(tile.x, tile.y));

            // Mira cap on camina (despres torna a mirar l'enemic)
            if (facing != null) facing.SetFaceDirection(target - transform.position);

            while ((transform.position - target).sqrMagnitude > 0.0001f)
            {
                transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
                yield return null;
            }
            transform.position = target;
            position = new Vector2Int(tile.x, tile.y);
            GameAudio.Play(Sfx.Footstep, 0.55f, 0.12f);
        }
        if (facing != null) facing.ClearFaceDirection();
        SetWalkingAnimation(false);
        moving = false;
        UpdateUI();
        print($"Move points left: {movePoints}");

        CheckAutoEndTurn();
    }

    void CheckAutoEndTurn()
    {
        if (movePoints <= 0 && (central == null || central.vida > 0) && (enemy == null || enemy.vida > 0))
        {
            print("Mana esgotat! Passant de torn automàticament...");
            EndTurn();
        }
    }

    Vector3 TileWorld(Vector2Int p)
    {
        try {
            return grid.GridToWorld(p.x, p.y) + Vector3.up * heightOffset;
        } catch (Exception)
        {
            //Assuming you are on the menu
            return new Vector3(0,0,0);
        }
    }
}