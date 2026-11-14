
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterSelector : MonoBehaviour
{
    [Header("Llista de Personatges")]
    [SerializeField] private GameObject[] characters;

    [Tooltip("Opcional: Noms personalitzats per mostrar a la UI.")]
    [SerializeField] private string[] customNames;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI characterNameText;

    [Header("Configuració de Rotació")]
    [SerializeField] private float rotationSpeed = 30f;

    private int currentIndex = 0;
    private Quaternion initialRotation;
    private bool selectionInitialized = false;

    private void Start()
    {
        initialRotation = transform.rotation;

        DisableMenuBackgroundShadows();
        EnsureCharactersAssignedAndCentered();

        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("Error: No has assignat cap personatge!", this);
            return;
        }

        // No mostrem cap personatge en iniciar el menú.
        foreach (GameObject character in characters)
        {
            if (character != null)
            {
                character.transform.localPosition = Vector3.zero;
                character.transform.localRotation = Quaternion.identity;
                character.SetActive(false);
            }
        }

        // Ocultem també el nom fins a obrir la selecció.
        if (characterNameText != null)
        {
            characterNameText.text = "";
        }
    }

    private void DisableMenuBackgroundShadows()
    {
        Light[] lights = FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (Light l in lights)
        {
            l.shadows = LightShadows.None;
        }

        MeshRenderer[] renderers = FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None);
        foreach (MeshRenderer mr in renderers)
        {
            if (mr.gameObject.name == "Background")
            {
                mr.receiveShadows = false;
                mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
        }
    }

    private void EnsureCharactersAssignedAndCentered()
    {
        foreach (Transform child in transform)
        {
            child.localPosition = Vector3.zero;
            child.localRotation = Quaternion.identity;
        }

        if (characters == null) return;

        string[] expectedNames = { "Knight", "Ranger", "Mage", "Rogue", "Barbarian" };
        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] == null && i < expectedNames.Length)
            {
                foreach (Transform child in transform)
                {
                    if (child.name.IndexOf(expectedNames[i], System.StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        characters[i] = child.gameObject;
                        break;
                    }
                }
            }
        }
    }

    private void Update()
    {
        // Només rotem quan la selecció està oberta.
        if (selectionInitialized)
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);
        }
    }

    public void InitializeCharacterSelection()
    {
        if (characters == null || characters.Length == 0)
        {
            Debug.LogError("Error: No hi ha personatges assignats!", this);
            return;
        }

        // Seleccionem sempre el primer personatge de l'array.
        currentIndex = 0;

        selectionInitialized = true;
        ShowCharacter(currentIndex);
    }

    public void NextCharacter()
    {
        if (!selectionInitialized ||
            characters == null ||
            characters.Length == 0)
        {
            return;
        }

        currentIndex++;

        if (currentIndex >= characters.Length)
        {
            currentIndex = 0;
        }

        ShowCharacter(currentIndex);
    }

    public void PreviousCharacter()
    {
        if (!selectionInitialized ||
            characters == null ||
            characters.Length == 0)
        {
            return;
        }

        currentIndex--;

        if (currentIndex < 0)
        {
            currentIndex = characters.Length - 1;
        }

        ShowCharacter(currentIndex);
    }

    private void ShowCharacter(int index)
    {
        if (index < 0 || index >= characters.Length)
        {
            return;
        }

        transform.rotation = initialRotation;

        for (int i = 0; i < characters.Length; i++)
        {
            if (characters[i] == null)
            {
                continue;
            }

            bool isActive = (i == index);
            characters[i].SetActive(isActive);

            // Reiniciem la posició i l'animació del personatge seleccionat.
            if (isActive)
            {
                characters[i].transform.localPosition = Vector3.zero;
                characters[i].transform.localRotation = Quaternion.identity;

                Animator anim = characters[i].GetComponent<Animator>();

                if (anim != null)
                {
                    anim.Play("Idle", 0, 0f);
                }
            }
        }

        PlayerPrefs.SetInt("SelectedCharacterIndex", index);
        PlayerPrefs.Save();

        UpdateNameText(index);
        UpdateStatsUI(index);
    }

    // ===================== ESTADISTIQUES DELS PERSONATGES =====================
    // Els valors coincideixen amb els que aplica PlayerMover.SetupCharacterClassAndWeapons()

    private struct AttackInfo
    {
        public string name;
        public int damage;
        public int cost;
        public string type;
        public int minRange;
        public int maxRange;
        public Color color;

        public AttackInfo(string n, int d, int c, string t, int minR, int maxR, Color col)
        {
            name = n; damage = d; cost = c; type = t; minRange = minR; maxRange = maxR; color = col;
        }
    }

    private class ClassStats
    {
        public string role;
        public string weapons;
        public string description;
        public int health;
        public int manaMin;
        public int manaMax;
        public float speed;
        public int rangeMin;
        public int rangeMax;
        public Color color;
        public AttackInfo[] attacks;
    }

    private const float MaxHealthStat = 32f;
    private const float MaxManaStat = 14f;
    private const float MaxSpeedStat = 10f;
    private const float MaxRangeStat = 9f;

    private static ClassStats GetClassStats(string lowerName)
    {
        ClassStats s = new ClassStats();
        if (lowerName.Contains("mage"))
        {
            s.role = "Ranged"; s.weapons = "Staff + Tome";
            s.description = "Lots of mana and high damage, but low health";
            s.health = 16; s.manaMin = 8; s.manaMax = 14; s.speed = 8f; s.rangeMin = 3; s.rangeMax = 9;
            s.color = new Color(0.6f, 0.45f, 1f);
            s.attacks = new[]
            {
                new AttackInfo("Arcane Orb", 3, 2, "Ranged", 3, 9, new Color(0.3f, 0.85f, 1f)),
                new AttackInfo("Fireball", 6, 4, "Ranged", 3, 9, new Color(0.75f, 0.45f, 1f)),
                new AttackInfo("Meteor", 10, 6, "Ranged", 3, 9, new Color(1f, 0.55f, 0.2f)),
            };
        }
        else if (lowerName.Contains("ranger"))
        {
            s.role = "Ranged"; s.weapons = "Bow + Arrows";
            s.description = "Fragile, but attacks from afar";
            s.health = 18; s.manaMin = 6; s.manaMax = 12; s.speed = 9f; s.rangeMin = 3; s.rangeMax = 9;
            s.color = new Color(0.45f, 0.95f, 0.45f);
            s.attacks = new[]
            {
                new AttackInfo("Arrow Shot", 3, 2, "Ranged", 3, 9, new Color(0.45f, 1f, 0.5f)),
                new AttackInfo("Piercing Shot", 5, 3, "Ranged", 3, 9, new Color(0.85f, 1f, 0.35f)),
                new AttackInfo("Arrow Rain", 8, 5, "Ranged", 3, 9, new Color(0.3f, 1f, 0.8f)),
            };
        }
        else if (lowerName.Contains("rogue"))
        {
            s.role = "Hybrid"; s.weapons = "Dagger + Crossbow";
            s.description = "Fast and versatile, close or far";
            s.health = 22; s.manaMin = 8; s.manaMax = 14; s.speed = 10f; s.rangeMin = 1; s.rangeMax = 9;
            s.color = new Color(0.3f, 0.85f, 0.55f);
            s.attacks = new[]
            {
                new AttackInfo("Stab", 3, 1, "Melee", 1, 2, new Color(0.45f, 1f, 0.7f)),
                new AttackInfo("Crossbow", 5, 3, "Ranged", 3, 9, new Color(0.75f, 0.45f, 1f)),
                new AttackInfo("Mixed Assault", 8, 5, "Hybrid", 1, 4, new Color(1f, 0.35f, 0.45f)),
            };
        }
        else if (lowerName.Contains("barbarian"))
        {
            s.role = "Melee"; s.weapons = "Twin Axes";
            s.description = "Lots of health and the highest melee damage";
            s.health = 32; s.manaMin = 6; s.manaMax = 12; s.speed = 8.5f; s.rangeMin = 1; s.rangeMax = 2;
            s.color = new Color(1f, 0.5f, 0.25f);
            s.attacks = new[]
            {
                new AttackInfo("Axe Chop", 4, 2, "Melee", 1, 2, new Color(1f, 0.6f, 0.25f)),
                new AttackInfo("Whirlwind", 7, 4, "Melee", 1, 2, new Color(1f, 0.4f, 0.2f)),
                new AttackInfo("Brutal Fury", 11, 6, "Melee", 1, 2, new Color(1f, 0.25f, 0.25f)),
            };
        }
        else
        {
            s.role = "Melee"; s.weapons = "Sword + Shield";
            s.description = "Tough and balanced, fights up close";
            s.health = 28; s.manaMin = 6; s.manaMax = 10; s.speed = 7.5f; s.rangeMin = 1; s.rangeMax = 2;
            s.color = new Color(0.95f, 0.85f, 0.4f);
            s.attacks = new[]
            {
                new AttackInfo("Sword Slash", 3, 2, "Melee", 1, 2, new Color(1f, 0.92f, 0.6f)),
                new AttackInfo("Shield Bash", 6, 4, "Melee", 1, 2, new Color(1f, 0.8f, 0.3f)),
                new AttackInfo("Lunge", 9, 6, "Melee", 1, 2, new Color(0.5f, 0.8f, 1f)),
            };
        }
        return s;
    }

    private RectTransform statsPanel;
    private RectTransform attacksPanel;
    private TextMeshProUGUI roleValueText;
    private TextMeshProUGUI weaponsValueText;
    private TextMeshProUGUI descriptionText;
    private TextMeshProUGUI[] attackTexts;
    private RectTransform[] barFills;
    private Image[] barFillImages;
    private TextMeshProUGUI[] barValueTexts;
    private float[] barTargets;

    private static string ToHex(Color c)
    {
        return "#" + ColorUtility.ToHtmlStringRGB(c);
    }

    private void UpdateStatsUI(int index)
    {
        EnsureStatsUI();
        if (statsPanel == null || attacksPanel == null) return;

        string lookupName = characters[index] != null ? characters[index].name : "";
        if (customNames != null && index < customNames.Length && !string.IsNullOrEmpty(customNames[index]))
        {
            lookupName += " " + customNames[index];
        }
        ClassStats s = GetClassStats(lookupName.ToLowerInvariant());

        statsPanel.gameObject.SetActive(true);
        attacksPanel.gameObject.SetActive(true);

        roleValueText.text = $"<color={ToHex(s.color)}>{s.role}</color>";
        weaponsValueText.text = s.weapons;
        descriptionText.text = s.description;

        SetBar(0, s.health / MaxHealthStat, s.health.ToString(), new Color(0.9f, 0.2f, 0.2f));
        SetBar(1, s.manaMax / MaxManaStat, $"{s.manaMin}-{s.manaMax}", new Color(0.25f, 0.45f, 1f));
        SetBar(2, s.speed / MaxSpeedStat, s.speed.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture), new Color(1f, 0.85f, 0.25f));
        SetBar(3, s.rangeMax / MaxRangeStat, $"{s.rangeMin}-{s.rangeMax}", new Color(0.35f, 0.9f, 0.45f));

        for (int i = 0; i < attackTexts.Length && i < s.attacks.Length; i++)
        {
            AttackInfo a = s.attacks[i];
            attackTexts[i].text =
                $"<size=30><color={ToHex(a.color)}>{i + 1}. {a.name}</color></size>\n" +
                $"<color=#FF6B6B>{a.damage} Dmg</color>  -  <color=#7FA8FF>{a.cost} Mana</color>\n" +
                $"<size=20><color=#C8C8C8>{a.type}  -  Range {a.minRange}-{a.maxRange}</color></size>";
        }
    }

    private void SetBar(int i, float fraction, string valueLabel, Color color)
    {
        if (barFills == null || i >= barFills.Length) return;
        barTargets[i] = Mathf.Clamp01(fraction);
        barFillImages[i].color = color;
        barValueTexts[i].text = valueLabel;
        // Reiniciem l'animacio de la barra
        barFills[i].anchorMax = new Vector2(0f, 1f);
    }

    private void LateUpdate()
    {
        if (barFills == null) return;
        for (int i = 0; i < barFills.Length; i++)
        {
            if (barFills[i] == null) continue;
            float current = barFills[i].anchorMax.x;
            float next = Mathf.MoveTowards(current, barTargets[i], Time.unscaledDeltaTime * 2.2f);
            barFills[i].anchorMax = new Vector2(next, 1f);
        }
    }

    private void HideStatsUI()
    {
        if (statsPanel != null) statsPanel.gameObject.SetActive(false);
        if (attacksPanel != null) attacksPanel.gameObject.SetActive(false);
    }

    private void EnsureStatsUI()
    {
        if (statsPanel != null && attacksPanel != null) return;
        if (characterNameText == null) return;

        Transform parent = characterNameText.transform.parent;
        if (parent == null) return;
        TMP_FontAsset font = characterNameText.font;

        Transform oldStats = parent.Find("CharacterStatsPanel");
        if (oldStats != null) Destroy(oldStats.gameObject);
        Transform oldAttacks = parent.Find("CharacterAttacksPanel");
        if (oldAttacks != null) Destroy(oldAttacks.gameObject);

        Vector2 panelSize = new Vector2(540f, 500f);

        // ---------- Panell esquerre: Estadistiques ----------
        statsPanel = CreatePanel(parent, "CharacterStatsPanel", new Vector2(-580f, 40f), panelSize);
        CreateText(statsPanel, "Title", new Vector2(0f, -18f), new Vector2(panelSize.x, 48f), "STATS", 36f, TextAlignmentOptions.Center, Color.white, font, true);
        CreateSeparator(statsPanel, -70f, panelSize.x - 60f);

        CreateText(statsPanel, "RoleLabel", new Vector2(30f, -88f), new Vector2(160f, 38f), "Role", 24f, TextAlignmentOptions.Left, new Color(0.8f, 0.8f, 0.8f), font, false);
        roleValueText = CreateText(statsPanel, "RoleValue", new Vector2(190f, -88f), new Vector2(320f, 38f), "", 26f, TextAlignmentOptions.Right, Color.white, font, false);
        CreateText(statsPanel, "WeaponsLabel", new Vector2(30f, -132f), new Vector2(160f, 38f), "Weapons", 24f, TextAlignmentOptions.Left, new Color(0.8f, 0.8f, 0.8f), font, false);
        weaponsValueText = CreateText(statsPanel, "WeaponsValue", new Vector2(190f, -132f), new Vector2(320f, 38f), "", 24f, TextAlignmentOptions.Right, Color.white, font, false);

        string[] barLabels = { "Health", "Mana/Turn", "Speed", "Range" };
        barFills = new RectTransform[barLabels.Length];
        barFillImages = new Image[barLabels.Length];
        barValueTexts = new TextMeshProUGUI[barLabels.Length];
        barTargets = new float[barLabels.Length];

        for (int i = 0; i < barLabels.Length; i++)
        {
            float y = -196f - i * 58f;
            CreateText(statsPanel, barLabels[i] + "Label", new Vector2(30f, y), new Vector2(170f, 36f), barLabels[i], 22f, TextAlignmentOptions.Left, new Color(0.85f, 0.85f, 0.85f), font, false);

            RectTransform barBg = CreateRect(statsPanel, barLabels[i] + "BarBg", new Vector2(205f, y - 8f), new Vector2(210f, 22f));
            Image bgImg = barBg.gameObject.AddComponent<Image>();
            bgImg.color = new Color(0.08f, 0.08f, 0.08f, 0.9f);
            bgImg.raycastTarget = false;

            GameObject fillObj = new GameObject("Fill", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            fillObj.transform.SetParent(barBg, false);
            RectTransform fillRt = fillObj.GetComponent<RectTransform>();
            fillRt.anchorMin = Vector2.zero;
            fillRt.anchorMax = new Vector2(0f, 1f);
            fillRt.pivot = new Vector2(0f, 0.5f);
            fillRt.offsetMin = new Vector2(0f, 2f);
            fillRt.offsetMax = new Vector2(0f, -2f);
            Image fillImg = fillObj.GetComponent<Image>();
            fillImg.raycastTarget = false;

            barFills[i] = fillRt;
            barFillImages[i] = fillImg;
            barValueTexts[i] = CreateText(statsPanel, barLabels[i] + "Value", new Vector2(425f, y), new Vector2(90f, 36f), "", 22f, TextAlignmentOptions.Right, Color.white, font, false);
        }

        CreateSeparator(statsPanel, -420f, panelSize.x - 60f);
        descriptionText = CreateText(statsPanel, "Description", new Vector2(30f, -432f), new Vector2(panelSize.x - 60f, 54f), "", 20f, TextAlignmentOptions.Center, new Color(0.95f, 0.9f, 0.75f), font, false);

        // ---------- Panell dret: Atacs ----------
        attacksPanel = CreatePanel(parent, "CharacterAttacksPanel", new Vector2(580f, 40f), panelSize);
        CreateText(attacksPanel, "Title", new Vector2(0f, -18f), new Vector2(panelSize.x, 48f), "ATTACKS", 36f, TextAlignmentOptions.Center, Color.white, font, true);
        CreateSeparator(attacksPanel, -70f, panelSize.x - 60f);

        attackTexts = new TextMeshProUGUI[3];
        for (int i = 0; i < attackTexts.Length; i++)
        {
            float y = -88f - i * 132f;
            RectTransform slot = CreateRect(attacksPanel, "AttackSlot" + (i + 1), new Vector2(24f, y), new Vector2(panelSize.x - 48f, 118f));
            Image slotImg = slot.gameObject.AddComponent<Image>();
            slotImg.color = new Color(1f, 1f, 1f, 0.06f);
            slotImg.raycastTarget = false;

            attackTexts[i] = CreateText(slot, "AttackText", new Vector2(16f, -8f), new Vector2(panelSize.x - 80f, 102f), "", 24f, TextAlignmentOptions.Left, Color.white, font, false);
            attackTexts[i].enableAutoSizing = true;
            attackTexts[i].fontSizeMin = 14f;
            attackTexts[i].fontSizeMax = 24f;
        }

        HideStatsUI();
    }

    private RectTransform CreatePanel(Transform parent, string name, Vector2 anchoredPos, Vector2 size)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = anchoredPos;
        rt.sizeDelta = size;

        Image img = obj.GetComponent<Image>();
        img.color = new Color(0.05f, 0.05f, 0.07f, 0.62f);
        img.raycastTarget = false;
        return rt;
    }

    // Rect ancorat a la cantonada superior esquerra del pare
    private RectTransform CreateRect(Transform parent, string name, Vector2 topLeftPos, Vector2 size)
    {
        GameObject obj = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer));
        obj.transform.SetParent(parent, false);
        RectTransform rt = obj.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = topLeftPos;
        rt.sizeDelta = size;
        return rt;
    }

    private void CreateSeparator(Transform parent, float y, float width)
    {
        RectTransform rt = CreateRect(parent, "Separator", new Vector2(30f, y), new Vector2(width, 3f));
        Image img = rt.gameObject.AddComponent<Image>();
        img.color = new Color(1f, 1f, 1f, 0.35f);
        img.raycastTarget = false;
    }

    private TextMeshProUGUI CreateText(Transform parent, string name, Vector2 topLeftPos, Vector2 size, string text, float fontSize, TextAlignmentOptions align, Color color, TMP_FontAsset font, bool bold)
    {
        RectTransform rt = CreateRect(parent, name, topLeftPos, size);
        TextMeshProUGUI tmp = rt.gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null) tmp.font = font;
        tmp.text = text;
        tmp.fontSize = fontSize;
        tmp.alignment = align;
        tmp.color = color;
        tmp.richText = true;
        tmp.raycastTarget = false;
        tmp.textWrappingMode = TextWrappingModes.Normal;
        tmp.fontStyle = bold ? FontStyles.Bold : FontStyles.Normal;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        return tmp;
    }

    private void UpdateNameText(int index)
    {
        if (characterNameText == null)
        {
            return;
        }

        if (customNames != null &&
            index < customNames.Length &&
            !string.IsNullOrEmpty(customNames[index]))
        {
            characterNameText.text = customNames[index];
        }
        else if (characters[index] != null)
        {
            characterNameText.text = characters[index].name;
        }
        else
        {
            characterNameText.text = "";
        }
    }
}
