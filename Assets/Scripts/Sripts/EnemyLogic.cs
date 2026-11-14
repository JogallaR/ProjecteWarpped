using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnemyLogic : MonoBehaviour
{
    enum AttackType { Square, Cross, Row, Column, Diagonal, Ring }

    [SerializeField] VictoryMenu victoryMenu;
    [SerializeField] TMP_Text bossHealth;
    [SerializeField] RectTransform bossHealthBarFill;
    [SerializeField] RawImage bossHealthBarFillImage;
    [SerializeField] GridBehaviour grid;
    [SerializeField] Color warningColor = new Color(1f, 0.55f, 0f);

    [Header("Coordinació d'Animacions i Partícules")]
    [SerializeField] private Animator enemyAnimator;
    [SerializeField] private ParticleSystem hitParticlePrefab;
    [SerializeField] private ParticleSystem deathParticlePrefab;
    [SerializeField] private float attackWindupTime = 0.45f;
    [SerializeField] private float attackRecoveryTime = 0.45f;
    [SerializeField] private float deathDelay = 1.2f;

    public PlayerMover playerMover;
    public int vida = 20;
    public int maxVida = 20;
    public int damage = 3;
    public int minSize = 1;
    public int maxSize = 2;

    [Header("Escalat de Dificultat per Fase")]
    public int extraVidaPerFase = 10;
    public int extraDamagePerFase = 2;

    readonly List<GridStat> pendingTiles = new List<GridStat>();
    AttackType pendingType;
    bool isDead;
    bool difficultyApplied;

    void OnEnable() => PlayerMover.TurnEnded += OnPlayerTurnEnded;
    void OnDisable() => PlayerMover.TurnEnded -= OnPlayerTurnEnded;

    void Awake()
    {
        ApplyPhaseDifficulty();
    }

    void Start()
    {
        ApplyPhaseDifficulty();
        SetupBossHealthUI();
        UpdateUI();
    }

    void ApplyPhaseDifficulty()
    {
        if (difficultyApplied) return;
        difficultyApplied = true;

        int extraFases = Mathf.Max(0, GameStats.fase - 1);
        if (extraFases > 0)
        {
            vida += extraFases * extraVidaPerFase;
            damage += extraFases * extraDamagePerFase;
            maxSize = Mathf.Min(maxSize + (extraFases + 1) / 2, 4);
        }

        maxVida = Mathf.Max(maxVida, vida);
        if (maxVida <= 0) maxVida = 20;
    }

    void OnPlayerTurnEnded(PlayerMover mover)
    {
        playerMover = mover;
        if (!grid) grid = mover.grid;
        if (!grid || grid.gridArray == null || vida <= 0 || isDead) return;
        if (PlayerDead()) return;

        if (pendingTiles.Count > 0)
        {
            StartCoroutine(CoordinatedEnemyTurnCoroutine());
        }
        else
        {
            BossTurn(true);
        }
    }

    IEnumerator CoordinatedEnemyTurnCoroutine()
    {
        if (playerMover != null) playerMover.SetEnemyTurnActive(true);

        PlayAttackAnimationForType(pendingType);
        GameAudio.Play(Sfx.EnemyRoar, 0.9f, 0.1f);

        yield return new WaitForSeconds(attackWindupTime);

        if (vida > 0 && !isDead)
        {
            ResolveAttack();
        }

        yield return new WaitForSeconds(attackRecoveryTime);

        if (!PlayerDead() && vida > 0 && !isDead)
        {
            BossTurn(false);
        }

        if (playerMover != null) playerMover.SetEnemyTurnActive(false);
    }

    bool PlayerDead() => playerMover.Central && playerMover.Central.vida <= 0;

    void BossTurn(bool playPrepareAnim = false)
    {
        if (playPrepareAnim)
        {
            PlayPrepareAnimation();
        }

        int typeOfAtack = Random.Range(0,30 + 1);
        int size = Random.Range(minSize, maxSize + 1);

        print(typeOfAtack + " roll of atack");
        
        Vector2Int center = playerMover.position;

        if (typeOfAtack < 7)
        {
            pendingType = AttackType.Square;   
        } else if (typeOfAtack < 14)
        {
            pendingType = AttackType.Ring;
        } else if (typeOfAtack < 20)
        {
            pendingType = AttackType.Cross;
        } else if (typeOfAtack < 26)
        {
            pendingType = AttackType.Diagonal;
        } else if (typeOfAtack <= 28)
        {
            pendingType = AttackType.Column;
        } else if (typeOfAtack <= 30)
        {
            pendingType = AttackType.Row;
        }
        
        foreach (Vector2Int p in GetPattern(pendingType, center, size))
        {
            if (!grid.IsWalkable(p.x, p.y)) {continue;}
            GridStat tile = grid.gridArray[p.x, p.y];
            if (!tile.dangerous && tile.walkable && !tile.heal && !pendingTiles.Contains(tile))
            {
                tile.SetWarning(warningColor);
                pendingTiles.Add(tile);
            }
        }

        int extraPatterns = Mathf.Min(Mathf.Max(0, GameStats.fase - 1), 3);
        for (int e = 0; e < extraPatterns; e++)
        {
            Vector2Int extraCenter = new Vector2Int(
                Mathf.Clamp(center.x + Random.Range(-4, 5), 0, grid.columns - 1),
                Mathf.Clamp(center.y + Random.Range(-4, 5), 0, grid.rows - 1)
            );
            AttackType extraType = (AttackType)Random.Range(0, 6);
            int extraSize = Random.Range(minSize, maxSize);
            foreach (Vector2Int p in GetPattern(extraType, extraCenter, extraSize))
            {
                if (!grid.IsWalkable(p.x, p.y)) continue;
                GridStat tile = grid.gridArray[p.x, p.y];
                if (!tile.dangerous && tile.walkable && !tile.heal && !pendingTiles.Contains(tile))
                {
                    tile.SetWarning(warningColor);
                    pendingTiles.Add(tile);
                }
            }
        }

        print($"Boss prepares {pendingType} (size {size}) on {center}: {pendingTiles.Count} tiles");
        if (pendingTiles.Count > 0) GameAudio.Play(Sfx.EnemyWarning, 0.6f, 0f);
    }

    void ResolveAttack()
    {
        if (pendingTiles.Count == 0) return;

        bool hit = false;
        GameAudio.Play(Sfx.EnemySlam, 0.9f, 0.08f);
        foreach (GridStat tile in pendingTiles)
        {
            tile.ClearWarning();
            grid.DamageEffect(tile.x, tile.y);
            if (tile.x == playerMover.position.x && tile.y == playerMover.position.y) hit = true;
            else
            {
                tile.setDangerous();
            }
        }
        pendingTiles.Clear();

        if (hit && playerMover.Central) playerMover.Central.TakeDamage(damage);
    }

    Animator GetEnemyAnimator()
    {
        if (enemyAnimator != null) return enemyAnimator;

        SpawnerAleatori spawner = FindFirstObjectByType<SpawnerAleatori>();
        if (spawner != null)
        {
            if (spawner.EnemicActual == null)
            {
                spawner.SpawnEnemic();
            }
            if (spawner.EnemicActual != null)
            {
                enemyAnimator = spawner.EnemicActual.GetComponentInChildren<Animator>();
            }
        }

        if (enemyAnimator == null)
        {
            enemyAnimator = GetComponentInChildren<Animator>();
        }

        return enemyAnimator;
    }

    bool PlayEnemyAnimation(params string[] candidateStates)
    {
        Animator anim = GetEnemyAnimator();
        if (anim == null) return false;

        foreach (string stateName in candidateStates)
        {
            int shortHash = Animator.StringToHash(stateName);
            if (anim.HasState(0, shortHash))
            {
                anim.CrossFadeInFixedTime(shortHash, 0.12f, 0, 0f);
                return true;
            }

            int fullHash = Animator.StringToHash("Base Layer." + stateName);
            if (anim.HasState(0, fullHash))
            {
                anim.CrossFadeInFixedTime(fullHash, 0.12f, 0, 0f);
                return true;
            }
        }
        return false;
    }

    void PlayPrepareAnimation()
    {
        PlayEnemyAnimation(
            "Rage",
            "Scream",
            "RaptorArmature|Raptor_Roar1_Anim",
            "Victory",
            "IdleAction"
        );
    }

    void PlayAttackAnimationForType(AttackType type)
    {
        switch (type)
        {
            case AttackType.Square:
            case AttackType.Ring:
                PlayEnemyAnimation(
                    "Rage",
                    "Flame Attack",
                    "Horn Attack",
                    "Attack02",
                    "Hit2",
                    "RaptorArmature|Raptor_Bite2_Anim",
                    "Basic Attack",
                    "Attack01",
                    "Hit",
                    "RaptorArmature|Raptor_Bite1_Anim"
                );
                break;

            case AttackType.Cross:
            case AttackType.Diagonal:
                PlayEnemyAnimation(
                    "Hit2",
                    "Claw Attack",
                    "Attack02",
                    "RaptorArmature|Raptor_Bite2_Anim",
                    "Hit",
                    "Basic Attack",
                    "Attack01",
                    "RaptorArmature|Raptor_Bite1_Anim"
                );
                break;

            default:
                PlayEnemyAnimation(
                    "Hit",
                    "Basic Attack",
                    "Attack01",
                    "RaptorArmature|Raptor_Bite1_Anim",
                    "Claw Attack",
                    "Hit2",
                    "Attack02"
                );
                break;
        }
    }

    void PlayHitAnimation()
    {
        PlayEnemyAnimation(
            "Damage",
            "GetHit",
            "Get Hit",
            "RaptorArmature|Raptor_Call1_Anim"
        );
    }

    void PlayDeathAnimation()
    {
        PlayEnemyAnimation(
            "Die",
            "RaptorArmature|Raptor_Drink_Anim"
        );
    }

    HashSet<Vector2Int> GetPattern(AttackType type, Vector2Int c, int size)
    {
        HashSet<Vector2Int> cells = new HashSet<Vector2Int>();

        switch (type)
        {
            case AttackType.Square:
                for (int dx = -size; dx <= size; dx++)
                    for (int dy = -size; dy <= size; dy++)
                        cells.Add(c + new Vector2Int(dx, dy));
                break;

            case AttackType.Cross:
                for (int i = -size * 2; i <= size * 2; i++)
                {
                    cells.Add(c + new Vector2Int(i, 0));
                    cells.Add(c + new Vector2Int(0, i));
                }
                break;

            case AttackType.Row:
                for (int x = 0; x < grid.columns; x++)
                    for (int dy = 1 - size; dy <= size - 1; dy++)
                        cells.Add(new Vector2Int(x, c.y + dy));
                break;

            case AttackType.Column:
                for (int y = 0; y < grid.rows; y++)
                    for (int dx = 1 - size; dx <= size - 1; dx++)
                        cells.Add(new Vector2Int(c.x + dx, y));
                break;

            case AttackType.Diagonal:
                for (int i = -size * 2; i <= size * 2; i++)
                {
                    cells.Add(c + new Vector2Int(i, i));
                    cells.Add(c + new Vector2Int(i, -i));
                }
                break;

            case AttackType.Ring:
            {
                int r = size;
                for (int dx = -r; dx <= r; dx++)
                    for (int dy = -r; dy <= r; dy++)
                        if (Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy)) == r)
                            cells.Add(c + new Vector2Int(dx, dy));
                break;
            }
        }

        return cells;
    }

    public void PlayPlayerAttackLaunchEffect(Vector3 playerWorldPos, int attackNumber)
    {
        Color defaultColor = attackNumber == 1
            ? new Color(0.3f, 0.85f, 1f, 1f)
            : (attackNumber == 2 ? new Color(1f, 0.65f, 0.15f, 1f) : new Color(1f, 0.2f, 0.3f, 1f));
        StartCoroutine(TravelAttackEffectCoroutine(playerWorldPos, attackNumber, defaultColor, null, null, true));
    }

    public void PlayPlayerAttackLaunchEffect(Vector3 playerWorldPos, int attackNumber, Color trailColor, GameObject projectileModelPrefab = null, Material projectileMaterial = null, bool isRanged = true)
    {
        StartCoroutine(TravelAttackEffectCoroutine(playerWorldPos, attackNumber, trailColor, projectileModelPrefab, projectileMaterial, isRanged));
    }

    IEnumerator TravelAttackEffectCoroutine(Vector3 startPos, int attackNumber, Color trailColor, GameObject projectileModelPrefab, Material projectileMaterial, bool isRanged)
    {
        Vector3 targetPos = GetEnemyWorldPosition() + Vector3.up * 1.0f;
        Vector3 origin = startPos + Vector3.up * 0.8f;

        if (!isRanged)
        {
            Vector3 slashPos = Vector3.Lerp(origin, targetPos, 0.35f);
            CreateProceduralBurst(slashPos, trailColor, 24, 4.5f, 0.28f);
            yield break;
        }

        CreateProceduralBurst(origin, trailColor, 18, 3.5f, 0.35f);

        GameObject orb = new GameObject("AttackProjectileFX");
        orb.transform.position = origin;
        Vector3 dir = (targetPos - origin);
        if (dir.sqrMagnitude > 0.001f)
        {
            orb.transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }

        if (projectileModelPrefab != null)
        {
            GameObject visual = Instantiate(projectileModelPrefab, orb.transform, false);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
            visual.transform.localScale = Vector3.one * 0.85f;
            if (projectileMaterial != null)
            {
                Renderer[] renderers = visual.GetComponentsInChildren<Renderer>(true);
                foreach (Renderer r in renderers)
                {
                    if (r != null) r.sharedMaterial = projectileMaterial;
                }
            }
        }

        ParticleSystem ps = orb.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.startLifetime = 0.25f;
        main.startSpeed = 1.2f;
        main.startSize = attackNumber == 3 ? 0.45f : 0.32f;
        main.startColor = trailColor;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 65f;

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.2f;

        var psRenderer = orb.GetComponent<ParticleSystemRenderer>();
        Shader sprShader = Shader.Find("Sprites/Default");
        if (sprShader != null)
        {
            psRenderer.material = new Material(sprShader);
        }

        float travelTime = 0.22f;
        float elapsed = 0f;
        while (elapsed < travelTime && orb != null)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / travelTime);
            orb.transform.position = Vector3.Lerp(origin, targetPos, t);
            yield return null;
        }

        if (orb != null)
        {
            Destroy(orb, 0.3f);
        }
    }

    Vector3 GetEnemyWorldPosition()
    {
        SpawnerAleatori spawner = FindFirstObjectByType<SpawnerAleatori>();
        if (spawner != null)
        {
            if (spawner.EnemicActual != null) return spawner.EnemicActual.position;
            if (spawner.puntAparicio != null) return spawner.puntAparicio.position;
        }
        if (grid != null)
        {
            return grid.GridToWorld(9, 9);
        }
        return transform.position;
    }

    void SpawnHitParticles(int amount)
    {
        Vector3 enemyPos = GetEnemyWorldPosition();
        float scaleMult = amount >= 8 ? 1.65f : (amount >= 5 ? 1.3f : 1.0f);

        if (hitParticlePrefab != null)
        {
            ParticleSystem fx = Instantiate(hitParticlePrefab, enemyPos + Vector3.up * 1.1f, Quaternion.identity);
            fx.transform.localScale *= scaleMult;
            fx.Play();
            Destroy(fx.gameObject, 2.5f);
        }

        if (grid != null)
        {
            grid.SpawnHitEffectAtWorldPos(enemyPos + Vector3.up * 0.9f, scaleMult);
            if (grid.InBounds(9, 9)) grid.DamageEffect(9, 9);
            if (grid.InBounds(9, 10)) grid.DamageEffect(9, 10);

            if (amount >= 5)
            {
                if (grid.InBounds(8, 9)) grid.DamageEffect(8, 9);
                if (grid.InBounds(10, 9)) grid.DamageEffect(10, 9);
            }
            if (amount >= 8)
            {
                if (grid.InBounds(8, 10)) grid.DamageEffect(8, 10);
                if (grid.InBounds(10, 10)) grid.DamageEffect(10, 10);
                if (grid.InBounds(9, 8)) grid.DamageEffect(9, 8);
            }
        }

        Color sparkColor = amount >= 8
            ? new Color(1f, 0.2f, 0.15f, 1f)
            : (amount >= 5 ? new Color(1f, 0.55f, 0.1f, 1f) : new Color(1f, 0.85f, 0.25f, 1f));
        CreateProceduralBurst(enemyPos + Vector3.up * 1.2f, sparkColor, 28 + amount * 4, 5.5f, 0.55f);
    }

    void SpawnDeathParticles()
    {
        Vector3 enemyPos = GetEnemyWorldPosition();
        if (deathParticlePrefab != null)
        {
            ParticleSystem fx = Instantiate(deathParticlePrefab, enemyPos + Vector3.up * 1.0f, Quaternion.identity);
            fx.transform.localScale *= 1.5f;
            fx.Play();
            Destroy(fx.gameObject, 3.5f);
        }
        CreateProceduralBurst(enemyPos + Vector3.up * 1.2f, new Color(1f, 0.25f, 0.1f, 1f), 60, 7f, 0.9f);
    }

    void CreateProceduralBurst(Vector3 worldPos, Color color, int count, float speed, float lifetime)
    {
        GameObject go = new GameObject("HitSparkBurst");
        go.transform.position = worldPos;

        ParticleSystem ps = go.AddComponent<ParticleSystem>();
        var main = ps.main;
        main.loop = false;
        main.playOnAwake = false;
        main.startLifetime = lifetime;
        main.startSpeed = new ParticleSystem.MinMaxCurve(speed * 0.4f, speed);
        main.startSize = new ParticleSystem.MinMaxCurve(0.18f, 0.38f);
        main.startColor = color;
        main.gravityModifier = 0.6f;
        main.simulationSpace = ParticleSystemSimulationSpace.World;

        var emission = ps.emission;
        emission.rateOverTime = 0f;
        emission.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, (short)count) });

        var shape = ps.shape;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = 0.4f;

        var psRenderer = go.GetComponent<ParticleSystemRenderer>();
        Shader sprShader = Shader.Find("Sprites/Default");
        if (sprShader != null)
        {
            psRenderer.material = new Material(sprShader);
        }

        ps.Play();
        Destroy(go, lifetime + 0.5f);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        vida = Mathf.Max(vida - amount, 0);
        SpawnHitParticles(amount);
        if (vida > 0) GameAudio.Play(Sfx.EnemyHurt, 0.85f, 0.1f);
        UpdateUI();
        if (bossHealthBarFillImage != null)
        {
            StartCoroutine(FlashHealthBarCoroutine());
        }

        if (vida == 0)
        {
            Die();
        }
        else
        {
            PlayHitAnimation();
        }
    }

    IEnumerator FlashHealthBarCoroutine()
    {
        if (bossHealthBarFillImage == null) yield break;
        bossHealthBarFillImage.color = new Color(1f, 0.85f, 0.35f, 1f);
        yield return new WaitForSeconds(0.15f);
        if (bossHealthBarFillImage != null)
        {
            bossHealthBarFillImage.color = new Color(0.85f, 0.12f, 0.12f, 1f);
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        foreach (GridStat tile in pendingTiles) tile.ClearWarning();
        pendingTiles.Clear();
        print("Boss defeated");
        GameAudio.Play(Sfx.EnemyDeath, 1f, 0f);
        SpawnDeathParticles();
        PlayDeathAnimation();
        StartCoroutine(WinAfterDeathCoroutine());
    }

    IEnumerator WinAfterDeathCoroutine()
    {
        yield return new WaitForSeconds(deathDelay);
        enabled = false;
        //Posar a pantalla de victoria
        if (victoryMenu) victoryMenu.Win();
    }

    void Update()
    {
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.gKey.wasPressedThisFrame) TakeDamage(5);
    }

    void SetupBossHealthUI()
    {
        GameObject hudObj = GameObject.Find("HUD") ?? GameObject.Find("Canvas 1");
        if (hudObj == null) return;

        Transform canvasTransform = hudObj.transform;

        TMP_FontAsset hudFont = null;
        Transform manaTxtTr = canvasTransform.Find("ManaText");
        if (manaTxtTr != null)
        {
            TMP_Text mt = manaTxtTr.GetComponent<TMP_Text>();
            if (mt != null) hudFont = mt.font;
        }

        Texture scrollTex = null;
        Transform bgMana = canvasTransform.Find("BackgroundMana");
        if (bgMana != null)
        {
            RawImage raw = bgMana.GetComponent<RawImage>();
            if (raw != null) scrollTex = raw.texture;
        }

        Transform panelTr = canvasTransform.Find("BossHealthPanel");
        if (panelTr == null)
        {
            GameObject panelObj = new GameObject("BossHealthPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            panelObj.transform.SetParent(canvasTransform, false);
            RectTransform rt = panelObj.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 1f);
            rt.anchorMax = new Vector2(0.5f, 1f);
            rt.anchoredPosition = new Vector2(0f, -42f);
            rt.sizeDelta = new Vector2(380f, 74f);

            RawImage panelRaw = panelObj.GetComponent<RawImage>();
            if (scrollTex != null) panelRaw.texture = scrollTex;
            panelRaw.color = new Color(1f, 1f, 1f, 0.96f);
            panelRaw.raycastTarget = false;

            GameObject bgObj = new GameObject("BossHealthBarBg", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            bgObj.transform.SetParent(panelObj.transform, false);
            RectTransform bgRt = bgObj.GetComponent<RectTransform>();
            bgRt.anchorMin = new Vector2(0.5f, 0.5f);
            bgRt.anchorMax = new Vector2(0.5f, 0.5f);
            bgRt.anchoredPosition = new Vector2(0f, -9f);
            bgRt.sizeDelta = new Vector2(240f, 16f);

            RawImage bgRaw = bgObj.GetComponent<RawImage>();
            bgRaw.color = new Color(0.18f, 0.08f, 0.08f, 0.95f);
            bgRaw.raycastTarget = false;

            GameObject fillObj = new GameObject("BossHealthBarFill", typeof(RectTransform), typeof(CanvasRenderer), typeof(RawImage));
            fillObj.transform.SetParent(bgObj.transform, false);
            bossHealthBarFill = fillObj.GetComponent<RectTransform>();
            bossHealthBarFill.anchorMin = Vector2.zero;
            bossHealthBarFill.anchorMax = Vector2.one;
            bossHealthBarFill.anchoredPosition = Vector2.zero;
            bossHealthBarFill.sizeDelta = new Vector2(-4f, -4f);

            bossHealthBarFillImage = fillObj.GetComponent<RawImage>();
            bossHealthBarFillImage.color = new Color(0.85f, 0.12f, 0.12f, 1f);
            bossHealthBarFillImage.raycastTarget = false;

            GameObject txtObj = new GameObject("BossHealthText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            txtObj.transform.SetParent(panelObj.transform, false);
            RectTransform txtRt = txtObj.GetComponent<RectTransform>();
            txtRt.anchorMin = new Vector2(0.5f, 0.5f);
            txtRt.anchorMax = new Vector2(0.5f, 0.5f);
            txtRt.anchoredPosition = new Vector2(0f, 11f);
            txtRt.sizeDelta = new Vector2(240f, 20f);

            bossHealth = txtObj.GetComponent<TextMeshProUGUI>();
            if (hudFont != null) bossHealth.font = hudFont;
            bossHealth.fontSize = 13f;
            bossHealth.enableAutoSizing = true;
            bossHealth.fontSizeMin = 8f;
            bossHealth.fontSizeMax = 13f;
            bossHealth.fontStyle = FontStyles.Bold;
            bossHealth.alignment = TextAlignmentOptions.Center;
            bossHealth.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            bossHealth.raycastTarget = false;
        }
        else
        {
            Transform bgTr = panelTr.Find("BossHealthBarBg");
            if (bgTr != null)
            {
                Transform fillTr = bgTr.Find("BossHealthBarFill");
                if (fillTr != null)
                {
                    bossHealthBarFill = fillTr.GetComponent<RectTransform>();
                    bossHealthBarFillImage = fillTr.GetComponent<RawImage>();
                }
            }
            Transform txtTr = panelTr.Find("BossHealthText");
            if (txtTr != null)
            {
                bossHealth = txtTr.GetComponent<TMP_Text>();
                if (bossHealth != null && hudFont != null)
                {
                    bossHealth.font = hudFont;
                }
            }
        }
    }

    void UpdateUI()
    {
        if (bossHealth == null || bossHealthBarFill == null)
        {
            SetupBossHealthUI();
        }

        if (maxVida <= 0) maxVida = Mathf.Max(vida, 20);

        if (bossHealth != null)
        {
            bossHealth.text = $"Phase {GameStats.fase} - Enemy: {vida} / {maxVida}";
            UiTextFit.Fit(bossHealth, 26f);
        }

        if (bossHealthBarFill != null)
        {
            float ratio = Mathf.Clamp01((float)vida / maxVida);
            bossHealthBarFill.anchorMin = new Vector2(0f, 0f);
            bossHealthBarFill.anchorMax = new Vector2(ratio, 1f);
            bossHealthBarFill.anchoredPosition = Vector2.zero;
            bossHealthBarFill.sizeDelta = new Vector2(-4f, -4f);
        }
    }
}