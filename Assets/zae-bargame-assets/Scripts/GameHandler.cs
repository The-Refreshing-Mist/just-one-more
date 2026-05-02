using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameHandler : MonoBehaviour
{
    enum MinigameType
    {
        CitrusFlash,
        ManaMixer,
        BlackoutMemory,
        ChaosTyping,
        BarCrawlBalance,
        CritClick,
        NeonFill,
        LagDelay
    }

    [Header("Drink Spawn")]
    public Button drinkButtonTemplate;
    public GameObject drinkMenuPanel;
    public float spawnInterval = 3f;
    public float drinkLifetime = 10f;

    [Header("Drink Sprites")]
    public Sprite[] drinkSprites;

    [Header("Main Game UI")]
    public TMP_Text gameTimerText;
    public TMP_Text drunknessText;
    public float gameDuration = 300f;
    [Header("Tutorial Mode")]
    public bool tutorialMode = false;
    public TMP_Text tutorialText;
    [Header("Minigame UI")]
    public GameObject minigamePanel;
    public RectTransform miniBarRect;
    public RectTransform greenZoneRect;
    public RectTransform scanLineRect;
    public TMP_Text minigameStatusText;

    [Header("Citrus Flash UI")]
    public Image citrusFlashOverlay;

    [Header("Crit UI")]
    public GameObject critBoxVisual;
    public float critClickScale = 1.15f;
    public float critClickEffectTime = 0.08f;

    [Header("Sound Effects")]
    public AudioSource sfxSource;
    public AudioSource neonLoopSource;
    public AudioClip drinkClickSound;
    public AudioClip critHitSound;
    public AudioClip citrusSpaceSound;
    public AudioClip neonHoldSound;

    [Header("Minigame Settings")]
    public float minigameDuration = 30f;

    [Header("Citrus Settings")]
    public float citrusWaitMin = 0.6f;
    public float citrusWaitMax = 1.8f;
    public float citrusFlashWindow = 0.55f;

    [Header("Crit Settings")]
    public float critChance = 0.12f;

    [Header("Bar Crawl Settings")]
    public float barCrawlDuration = 10f;
    public float balanceMoveSpeed = 1.4f;
    public float balanceDriftSpeed = 0.7f;

    [Header("Neon Settings")]
    public float neonFillSpeed = 0.65f;
    public float neonTargetMin = 0.72f;
    public float neonTargetMax = 0.90f;

    [Header("Neon Visual UI")]
    public GameObject neonVisualGroup;
    public RectTransform neonFillRect;
    public RectTransform neonTargetLine;
    public Image neonPixelPrefab;
    public RectTransform neonPixelParent;
    public float neonMaxFillHeight = 220f;
    public float neonPixelFallSpeed = 450f;
    public float neonPixelSpawnRate = 0.06f;

    [Header("Lag Juice Settings")]
    public float lagDelay = 0.55f;
    public float lagMarkerSpeed = 1.2f;
    public float lagTargetMin = 0.65f;
    public float lagTargetMax = 0.85f;

    private int totalDrunkness = 0;
    private float gameTimeRemaining = 0f;

    private bool hasLoadedDrivingScene = false;

    private bool minigameActive = false;
    private int currentDrinkIndex = -1;
    private MinigameType currentMinigame;

    private float minigameTimeRemaining = 0f;
    private float lastDrinkEffectiveness = 0f;

    private Coroutine critClickEffectRoutine;
    private Vector3 critOriginalScale = Vector3.one;

    private int[] enabledDrinkIndices = new int[]
    {
        0, // Citrus Crash
        //2, // Blackout Breeze
        5, // Crit
        6  // Neon Margarita
    };

    private int citrusTotalFlashes = 0;
    private int citrusCurrentFlash = 0;
    private int citrusHits = 0;
    private bool citrusFlashActive = false;
    private float citrusTimer = 0f;

    private string[] manaIngredients = { "Ice", "Blue Mana", "Lime", "Spark Dust" };
    private int[] manaSequence;
    private int manaStep = 0;
    private int manaCorrect = 0;
    private bool manaInputEnabled = false;

    private string[] memoryDoodles = { "Star", "Face", "Bottle", "Cloud" };
    private int[] blackoutSequence;
    private int blackoutStep = 0;
    private int blackoutCorrect = 0;
    private bool blackoutInputEnabled = false;

    private string[] chaosWords = { "fear", "buzz", "chaos", "rage", "neon", "void" };
    private string chaosWord = "";
    private int chaosLetterIndex = 0;
    private int chaosCorrect = 0;
    private float chaosLetterTimer = 0f;
    private float chaosLetterTimeLimit = 2.5f;

    private float balanceValue = 0f;
    private float balanceDrift = 0f;
    private float barCrawlTimer = 0f;
    private float barCrawlSafeTime = 0f;

    private int critClicks = 0;
    private bool critLanded = false;

    private float neonFill = 0f;
    private bool neonFinished = false;
    private bool neonWasHolding = false;
    private float neonPixelSpawnTimer = 0f;

    private int lagTotalAttempts = 0;
    private int lagAttemptsUsed = 0;
    private int lagHits = 0;
    private float lagMarker = 0f;
    private bool lagMovingRight = true;
    private bool lagPending = false;

    string[] drinkNames = new string[]
    {
        "Citrus Crash",
        "Mana Mixer",
        "Blackout Breeze",
        "Chaos Colada",
        "Bar Crawl Survival",
        "Crit",
        "Neon Margarita",
        "Lag Juice"
    };

    int[] drinkDrunkValues = new int[]
    {
        2, 3, 7, 6, 9, 4, 3, 2
    };

    void Start()
    {
        gameTimeRemaining = gameDuration;
        UpdateMainGameUI();
        if (drinkButtonTemplate == null)
        {
            Debug.LogError("drinkButtonTemplate is not assigned.");
            return;
        }

        drinkButtonTemplate.gameObject.SetActive(false);

        if (drinkMenuPanel != null)
            drinkMenuPanel.SetActive(true);

        if (minigamePanel != null)
            minigamePanel.SetActive(false);

        if (neonLoopSource != null)
        {
            neonLoopSource.loop = true;
            neonLoopSource.playOnAwake = false;
            neonLoopSource.clip = neonHoldSound;
        }

        HideExtraVisuals();

        if (tutorialText != null)
            tutorialText.gameObject.SetActive(false);

        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
        UpdateGameTimer();
        if (!minigameActive)
            return;

        minigameTimeRemaining -= Time.deltaTime;

        if (minigameTimeRemaining <= 0f)
        {
            FinishMinigame();
            return;
        }

        switch (currentMinigame)
        {
            case MinigameType.CitrusFlash:
                UpdateCitrusFlash();
                break;

            case MinigameType.ManaMixer:
                UpdateManaMixer();
                break;

            case MinigameType.BlackoutMemory:
                UpdateBlackoutMemory();
                break;

            case MinigameType.ChaosTyping:
                UpdateChaosTyping();
                break;

            case MinigameType.BarCrawlBalance:
                UpdateBarCrawlBalance();
                break;

            case MinigameType.CritClick:
                UpdateCritClick();
                break;

            case MinigameType.NeonFill:
                UpdateNeonFill();
                break;

            case MinigameType.LagDelay:
                UpdateLagDelay();
                break;
        }

        UpdateMinigameStatus();
    }

    IEnumerator SpawnLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);

            if (!minigameActive)
                SpawnDrink();
        }
    }

    void SpawnDrink()
    {
        RectTransform parentRect = drinkButtonTemplate.transform.parent as RectTransform;
        if (parentRect == null) return;

        Button newDrink = Instantiate(drinkButtonTemplate);
        newDrink.transform.SetParent(parentRect, false);
        newDrink.gameObject.SetActive(true);

        int drinkIndex = enabledDrinkIndices[Random.Range(0, enabledDrinkIndices.Length)];

        ApplyDrinkSprite(newDrink, drinkIndex);
        PositionRandomX(newDrink, parentRect);

        newDrink.onClick.RemoveAllListeners();
        newDrink.onClick.AddListener(() => StartDrinkMinigame(newDrink, drinkIndex));

        StartCoroutine(RemoveAfterTime(newDrink.gameObject));
    }

    void ApplyDrinkSprite(Button button, int drinkIndex)
    {
        Image img = button.GetComponent<Image>();
        RectTransform rect = button.GetComponent<RectTransform>();

        if (rect != null)
        {
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, 100f);
            rect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 100f);
        }

        if (img != null && drinkSprites.Length > drinkIndex)
        {
            img.sprite = drinkSprites[drinkIndex];
            img.color = Color.white;
            img.type = Image.Type.Simple;
            img.fillMethod = Image.FillMethod.Horizontal;
            img.fillAmount = 1f;
            img.preserveAspect = true;
        }
    }

    void PositionRandomX(Button button, RectTransform parentRect)
    {
        RectTransform rect = button.GetComponent<RectTransform>();

        float parentWidth = parentRect.rect.width;
        float buttonWidth = rect.rect.width;

        float minX = (-parentWidth / 2f) + (buttonWidth / 2f);
        float maxX = (parentWidth / 2f) - (buttonWidth / 2f);

        float randomX = Random.Range(minX, maxX);
        rect.anchoredPosition = new Vector2(randomX, -55f);
    }

    void StartDrinkMinigame(Button clickedDrink, int drinkIndex)
    {
        PlaySFX(drinkClickSound);

        if (clickedDrink != null)
            Destroy(clickedDrink.gameObject);

        currentDrinkIndex = drinkIndex;
        currentMinigame = GetMinigameForDrink(drinkIndex);

        minigameTimeRemaining = minigameDuration;
        minigameActive = true;
        lastDrinkEffectiveness = 0f;

        if (drinkMenuPanel != null)
            drinkMenuPanel.SetActive(false);

        if (minigamePanel != null)
            minigamePanel.SetActive(true);

        HideExtraVisuals();

        switch (currentMinigame)
        {
            case MinigameType.CitrusFlash:
                StartCitrusFlash();
                break;

            case MinigameType.ManaMixer:
                StartManaMixer();
                break;

            case MinigameType.BlackoutMemory:
                StartBlackoutMemory();
                break;

            case MinigameType.ChaosTyping:
                StartChaosTyping();
                break;

            case MinigameType.BarCrawlBalance:
                StartBarCrawlBalance();
                break;

            case MinigameType.CritClick:
                StartCritClick();
                break;

            case MinigameType.NeonFill:
                StartNeonFill();
                break;

            case MinigameType.LagDelay:
                StartLagDelay();
                break;
        }

        UpdateMinigameStatus();
    }

    MinigameType GetMinigameForDrink(int drinkIndex)
    {
        switch (drinkIndex)
        {
            case 0: return MinigameType.CitrusFlash;
            case 1: return MinigameType.ManaMixer;
            case 2: return MinigameType.BlackoutMemory;
            case 3: return MinigameType.ChaosTyping;
            case 4: return MinigameType.BarCrawlBalance;
            case 5: return MinigameType.CritClick;
            case 6: return MinigameType.NeonFill;
            case 7: return MinigameType.LagDelay;
            default: return MinigameType.CitrusFlash;
        }
    }

    void HideExtraVisuals()
    {
        StopNeonHoldSound();

        if (miniBarRect != null)
            miniBarRect.gameObject.SetActive(false);

        if (citrusFlashOverlay != null)
            citrusFlashOverlay.gameObject.SetActive(false);

        if (critBoxVisual != null)
        {
            critBoxVisual.SetActive(false);
            critBoxVisual.transform.localScale = critOriginalScale;
        }

        if (neonVisualGroup != null)
        {
            neonVisualGroup.SetActive(false);
        }
    }

    void PlaySFX(AudioClip clip)
    {
        if (sfxSource != null && clip != null)
            sfxSource.PlayOneShot(clip);
    }

    void StartNeonHoldSound()
    {
        if (neonLoopSource == null || neonHoldSound == null)
            return;

        if (neonLoopSource.clip != neonHoldSound)
            neonLoopSource.clip = neonHoldSound;

        if (!neonLoopSource.isPlaying)
            neonLoopSource.Play();
    }

    void StopNeonHoldSound()
    {
        if (neonLoopSource != null && neonLoopSource.isPlaying)
            neonLoopSource.Stop();
    }

    void StartCitrusFlash()
    {
        citrusTotalFlashes = drinkDrunkValues[currentDrinkIndex] * 2;
        citrusCurrentFlash = 1;
        citrusHits = 0;
        citrusFlashActive = false;

        StartNextCitrusWait();
    }

    void StartNextCitrusWait()
    {
        citrusFlashActive = false;
        citrusTimer = Random.Range(citrusWaitMin, citrusWaitMax);

        if (citrusFlashOverlay != null)
            citrusFlashOverlay.gameObject.SetActive(false);
    }

    void StartCitrusFlashWindow()
    {
        citrusFlashActive = true;
        citrusTimer = citrusFlashWindow;

        if (citrusFlashOverlay != null)
        {
            citrusFlashOverlay.gameObject.SetActive(true);
            citrusFlashOverlay.color = new Color(1f, 1f, 0f, 0.35f);
        }
    }

    void UpdateCitrusFlash()
    {
        citrusTimer -= Time.deltaTime;

        if (!citrusFlashActive && citrusTimer <= 0f)
        {
            StartCitrusFlashWindow();
            return;
        }

        if (citrusFlashActive)
        {
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                PlaySFX(citrusSpaceSound);
                citrusHits++;
                AdvanceCitrusFlash();
                return;
            }

            if (citrusTimer <= 0f)
                AdvanceCitrusFlash();
        }
    }

    void UpdateNeonFillVisual()
{
    if (neonFillRect == null)
        return;

    float fillHeight = neonFill * neonMaxFillHeight;
    neonFillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, fillHeight);
}

void SpawnNeonPixelsWhileHolding()
{
    if (neonPixelPrefab == null || neonPixelParent == null)
        return;

    neonPixelSpawnTimer -= Time.deltaTime;

    if (neonPixelSpawnTimer > 0f)
        return;

    neonPixelSpawnTimer = neonPixelSpawnRate;

    Image pixel = Instantiate(neonPixelPrefab, neonPixelParent);
    pixel.gameObject.SetActive(true);

    RectTransform pixelRect = pixel.GetComponent<RectTransform>();

    float randomX = Random.Range(-35f, 35f);
    pixelRect.anchoredPosition = new Vector2(randomX, 160f);

    StartCoroutine(NeonPixelFall(pixelRect));
}

IEnumerator NeonPixelFall(RectTransform pixelRect)
{
    while (pixelRect != null && pixelRect.anchoredPosition.y > -80f)
    {
        pixelRect.anchoredPosition += Vector2.down * neonPixelFallSpeed * Time.deltaTime;
        yield return null;
    }

    if (pixelRect != null)
        Destroy(pixelRect.gameObject);
}

void ClearNeonPixels()
{
    if (neonPixelParent == null)
        return;

    for (int i = neonPixelParent.childCount - 1; i >= 0; i--)
    {
        Destroy(neonPixelParent.GetChild(i).gameObject);
    }
}

    void AdvanceCitrusFlash()
    {
        if (citrusFlashOverlay != null)
            citrusFlashOverlay.gameObject.SetActive(false);

        if (citrusCurrentFlash >= citrusTotalFlashes)
        {
            FinishMinigame();
        }
        else
        {
            citrusCurrentFlash++;
            StartNextCitrusWait();
        }
    }

    void StartManaMixer()
    {
        int sequenceLength = drinkDrunkValues[currentDrinkIndex] + 1;
        manaSequence = new int[sequenceLength];

        for (int i = 0; i < manaSequence.Length; i++)
        {
            manaSequence[i] = Random.Range(0, manaIngredients.Length);
        }

        manaStep = 0;
        manaCorrect = 0;
        manaInputEnabled = false;

        StartCoroutine(ShowManaSequence());
    }

    IEnumerator ShowManaSequence()
    {
        string sequenceText = "Memorize Mana Mixer ingredients:\n";

        for (int i = 0; i < manaSequence.Length; i++)
        {
            sequenceText += manaIngredients[manaSequence[i]];

            if (i < manaSequence.Length - 1)
                sequenceText += " -> ";
        }

        if (minigameStatusText != null)
            minigameStatusText.text = sequenceText;

        yield return new WaitForSeconds(2.5f);

        manaInputEnabled = true;
        UpdateMinigameStatus();
    }

    void UpdateManaMixer()
    {
        if (!manaInputEnabled)
            return;

        int pressedIngredient = GetPressedNumber1To4();

        if (pressedIngredient == -1)
            return;

        if (pressedIngredient == manaSequence[manaStep])
            manaCorrect++;

        manaStep++;

        if (manaStep >= manaSequence.Length)
            FinishMinigame();
    }

    void StartBlackoutMemory()
    {
        int sequenceLength = 4;
        blackoutSequence = new int[sequenceLength];

        for (int i = 0; i < blackoutSequence.Length; i++)
            blackoutSequence[i] = Random.Range(0, memoryDoodles.Length);

        blackoutStep = 0;
        blackoutCorrect = 0;
        blackoutInputEnabled = false;

        StartCoroutine(ShowBlackoutSequence());
    }

    IEnumerator ShowBlackoutSequence()
    {
        string sequenceText = "Blackout Breeze memory flash:\n";

        for (int i = 0; i < blackoutSequence.Length; i++)
        {
            sequenceText += memoryDoodles[blackoutSequence[i]];

            if (i < blackoutSequence.Length - 1)
                sequenceText += " | ";
        }

        if (minigameStatusText != null)
            minigameStatusText.text = sequenceText;

        yield return new WaitForSeconds(1.2f);

        if (minigameStatusText != null)
            minigameStatusText.text = "Memory faded.\nRecall with 1-4.\n1 Star, 2 Face, 3 Bottle, 4 Cloud";

        blackoutInputEnabled = true;
    }

    void UpdateBlackoutMemory()
    {
        if (!blackoutInputEnabled)
            return;

        int pressed = GetPressedNumber1To4();

        if (pressed == -1)
            return;

        if (pressed == blackoutSequence[blackoutStep])
            blackoutCorrect++;

        blackoutStep++;

        if (blackoutStep >= blackoutSequence.Length)
            FinishMinigame();
    }

    void StartChaosTyping()
    {
        chaosWord = chaosWords[Random.Range(0, chaosWords.Length)];
        chaosLetterIndex = 0;
        chaosCorrect = 0;
        chaosLetterTimer = chaosLetterTimeLimit;
    }

    void UpdateChaosTyping()
    {
        chaosLetterTimer -= Time.deltaTime;

        if (chaosLetterTimer <= 0f)
        {
            AdvanceChaosLetter();
            return;
        }

        char needed = chaosWord[chaosLetterIndex];
        char pressed = GetPressedLetter();

        if (pressed == '\0')
            return;

        if (pressed == needed)
            chaosCorrect++;

        AdvanceChaosLetter();
    }

    void AdvanceChaosLetter()
    {
        chaosLetterIndex++;
        chaosLetterTimer = chaosLetterTimeLimit;

        if (chaosLetterIndex >= chaosWord.Length)
            FinishMinigame();
    }

    void StartBarCrawlBalance()
    {
        balanceValue = 0f;
        balanceDrift = Random.Range(-balanceDriftSpeed, balanceDriftSpeed);
        barCrawlTimer = barCrawlDuration;
        barCrawlSafeTime = 0f;
    }

    void UpdateBarCrawlBalance()
    {
        barCrawlTimer -= Time.deltaTime;

        if (Random.value < 0.02f)
            balanceDrift = Random.Range(-balanceDriftSpeed, balanceDriftSpeed);

        balanceValue += balanceDrift * Time.deltaTime;

        if (Keyboard.current != null)
        {
            if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
                balanceValue -= balanceMoveSpeed * Time.deltaTime;

            if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
                balanceValue += balanceMoveSpeed * Time.deltaTime;
        }

        balanceValue = Mathf.Clamp(balanceValue, -1f, 1f);

        if (Mathf.Abs(balanceValue) <= 0.25f)
            barCrawlSafeTime += Time.deltaTime;

        if (barCrawlTimer <= 0f)
            FinishMinigame();
    }

    void StartCritClick()
    {
        critClicks = 0;
        critLanded = false;

        if (critBoxVisual != null)
        {
            critBoxVisual.SetActive(true);
            critOriginalScale = critBoxVisual.transform.localScale;

            Image critImage = critBoxVisual.GetComponent<Image>();
            if (critImage != null && drinkSprites != null && currentDrinkIndex < drinkSprites.Length)
            {
                critImage.sprite = drinkSprites[currentDrinkIndex];
                critImage.color = Color.white;
                critImage.preserveAspect = true;
                critImage.raycastTarget = false;
            }
        }
    }

    void UpdateCritClick()
    {
        bool clicked = false;

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            clicked = true;

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            clicked = true;

        if (!clicked)
            return;

        PlaySFX(critHitSound);

        critClicks++;
        PlayCritClickEffect();

        if (Random.value <= critChance)
        {
            critLanded = true;
            FinishMinigame();
        }
    }

    void PlayCritClickEffect()
    {
        if (critBoxVisual == null)
            return;

        if (critClickEffectRoutine != null)
            StopCoroutine(critClickEffectRoutine);

        critClickEffectRoutine = StartCoroutine(CritClickEffectRoutine());
    }

    IEnumerator CritClickEffectRoutine()
    {
        critBoxVisual.transform.localScale = critOriginalScale * critClickScale;

        yield return new WaitForSeconds(critClickEffectTime);

        if (critBoxVisual != null)
            critBoxVisual.transform.localScale = critOriginalScale;
    }

    void StartNeonFill()
    {
        neonFill = 0f;
        neonFinished = false;
        neonWasHolding = false;
        neonPixelSpawnTimer = 0f;

        StopNeonHoldSound();

        if (neonVisualGroup != null)
            neonVisualGroup.SetActive(true);

        if (neonFillRect != null)
            neonFillRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, 0f);

        if (neonTargetLine != null)
        {
            float targetY = neonTargetMin * neonMaxFillHeight;
            neonTargetLine.anchoredPosition = new Vector2(neonTargetLine.anchoredPosition.x, targetY);
        }

        ClearNeonPixels();
    }

    void UpdateNeonFill()
    {
        if (neonFinished)
            return;

        bool holdingSpace = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;

        if (holdingSpace)
        {
            neonFill += neonFillSpeed * Time.deltaTime;
            StartNeonHoldSound();
            SpawnNeonPixelsWhileHolding();
        }
        else
        {
            StopNeonHoldSound();
        }

        neonFill = Mathf.Clamp01(neonFill);
        UpdateNeonFillVisual();

        if (neonFill >= 1f)
        {
            neonFinished = true;
            StopNeonHoldSound();
            FinishMinigame();
            return;
        }

        if (neonWasHolding && !holdingSpace)
        {
            neonFinished = true;
            StopNeonHoldSound();
            FinishMinigame();
            return;
        }

        neonWasHolding = holdingSpace;
    }

    void StartLagDelay()
    {
        lagTotalAttempts = drinkDrunkValues[currentDrinkIndex] * 2;
        lagAttemptsUsed = 0;
        lagHits = 0;
        lagMarker = 0f;
        lagMovingRight = true;
        lagPending = false;
    }

    void UpdateLagDelay()
    {
        if (lagMovingRight)
            lagMarker += lagMarkerSpeed * Time.deltaTime;
        else
            lagMarker -= lagMarkerSpeed * Time.deltaTime;

        if (lagMarker >= 1f)
        {
            lagMarker = 1f;
            lagMovingRight = false;
        }
        else if (lagMarker <= 0f)
        {
            lagMarker = 0f;
            lagMovingRight = true;
        }

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame && !lagPending)
        {
            lagAttemptsUsed++;
            lagPending = true;
            StartCoroutine(EvaluateLagPressAfterDelay());
        }
    }

    IEnumerator EvaluateLagPressAfterDelay()
    {
        yield return new WaitForSeconds(lagDelay);

        if (!minigameActive || currentMinigame != MinigameType.LagDelay)
            yield break;

        if (lagMarker >= lagTargetMin && lagMarker <= lagTargetMax)
            lagHits++;

        lagPending = false;

        if (lagAttemptsUsed >= lagTotalAttempts)
            FinishMinigame();
    }

    int GetPressedNumber1To4()
    {
        if (Keyboard.current == null) return -1;

        if (Keyboard.current.digit1Key.wasPressedThisFrame || Keyboard.current.numpad1Key.wasPressedThisFrame)
            return 0;

        if (Keyboard.current.digit2Key.wasPressedThisFrame || Keyboard.current.numpad2Key.wasPressedThisFrame)
            return 1;

        if (Keyboard.current.digit3Key.wasPressedThisFrame || Keyboard.current.numpad3Key.wasPressedThisFrame)
            return 2;

        if (Keyboard.current.digit4Key.wasPressedThisFrame || Keyboard.current.numpad4Key.wasPressedThisFrame)
            return 3;

        return -1;
    }

    char GetPressedLetter()
    {
        if (Keyboard.current == null) return '\0';

        if (Keyboard.current.aKey.wasPressedThisFrame) return 'a';
        if (Keyboard.current.bKey.wasPressedThisFrame) return 'b';
        if (Keyboard.current.cKey.wasPressedThisFrame) return 'c';
        if (Keyboard.current.dKey.wasPressedThisFrame) return 'd';
        if (Keyboard.current.eKey.wasPressedThisFrame) return 'e';
        if (Keyboard.current.fKey.wasPressedThisFrame) return 'f';
        if (Keyboard.current.gKey.wasPressedThisFrame) return 'g';
        if (Keyboard.current.hKey.wasPressedThisFrame) return 'h';
        if (Keyboard.current.iKey.wasPressedThisFrame) return 'i';
        if (Keyboard.current.jKey.wasPressedThisFrame) return 'j';
        if (Keyboard.current.kKey.wasPressedThisFrame) return 'k';
        if (Keyboard.current.lKey.wasPressedThisFrame) return 'l';
        if (Keyboard.current.mKey.wasPressedThisFrame) return 'm';
        if (Keyboard.current.nKey.wasPressedThisFrame) return 'n';
        if (Keyboard.current.oKey.wasPressedThisFrame) return 'o';
        if (Keyboard.current.pKey.wasPressedThisFrame) return 'p';
        if (Keyboard.current.qKey.wasPressedThisFrame) return 'q';
        if (Keyboard.current.rKey.wasPressedThisFrame) return 'r';
        if (Keyboard.current.sKey.wasPressedThisFrame) return 's';
        if (Keyboard.current.tKey.wasPressedThisFrame) return 't';
        if (Keyboard.current.uKey.wasPressedThisFrame) return 'u';
        if (Keyboard.current.vKey.wasPressedThisFrame) return 'v';
        if (Keyboard.current.wKey.wasPressedThisFrame) return 'w';
        if (Keyboard.current.xKey.wasPressedThisFrame) return 'x';
        if (Keyboard.current.yKey.wasPressedThisFrame) return 'y';
        if (Keyboard.current.zKey.wasPressedThisFrame) return 'z';

        return '\0';
    }

    void FinishMinigame()
    {
        if (!minigameActive)
            return;

        minigameActive = false;
        StopNeonHoldSound();
        HideExtraVisuals();

        switch (currentMinigame)
        {
            case MinigameType.CitrusFlash:
                lastDrinkEffectiveness = citrusTotalFlashes > 0 ? (float)citrusHits / citrusTotalFlashes : 0f;
                break;

            case MinigameType.ManaMixer:
                lastDrinkEffectiveness = manaSequence != null && manaSequence.Length > 0 ? (float)manaCorrect / manaSequence.Length : 0f;
                break;

            case MinigameType.BlackoutMemory:
                lastDrinkEffectiveness = blackoutSequence != null && blackoutSequence.Length > 0 ? (float)blackoutCorrect / blackoutSequence.Length : 0f;
                break;

            case MinigameType.ChaosTyping:
                lastDrinkEffectiveness = chaosWord.Length > 0 ? (float)chaosCorrect / chaosWord.Length : 0f;
                break;

            case MinigameType.BarCrawlBalance:
                lastDrinkEffectiveness = barCrawlDuration > 0f ? barCrawlSafeTime / barCrawlDuration : 0f;
                break;

            case MinigameType.CritClick:
                lastDrinkEffectiveness = critLanded ? 1f : 0f;
                break;

            case MinigameType.NeonFill:
                if (neonFill >= neonTargetMin && neonFill <= neonTargetMax)
                    lastDrinkEffectiveness = 1f;
                else
                    lastDrinkEffectiveness = 1f - Mathf.Clamp01(Mathf.Abs(neonFill - ((neonTargetMin + neonTargetMax) / 2f)) / 0.5f);
                break;

            case MinigameType.LagDelay:
                lastDrinkEffectiveness = lagTotalAttempts > 0 ? (float)lagHits / lagTotalAttempts : 0f;
                break;
        }

        if (currentDrinkIndex >= 0)
        {
            totalDrunkness += drinkDrunkValues[currentDrinkIndex];
            UpdateMainGameUI();
        }

        if (minigameStatusText != null)
        {
            minigameStatusText.text =
                "Done\n" +
                drinkNames[currentDrinkIndex] + "\n" +
                "Effectiveness: " + Mathf.RoundToInt(lastDrinkEffectiveness * 100f) + "%\n" +
                "Drunkness: " + totalDrunkness;
        }

        StartCoroutine(ReturnToDrinkMenuAfterDelay(1.2f));
    }

    void SetTutorialMessage(string message)
    {
        if (tutorialText == null)
            return;

        if (tutorialMode)
        {
            tutorialText.gameObject.SetActive(true);
            tutorialText.text = message;
        }
        else
        {
            tutorialText.gameObject.SetActive(false);
            tutorialText.text = "";
        }
    }

    void UpdateMinigameStatus()
    {
        if (minigameStatusText == null) return;

        switch (currentMinigame)
        {
            case MinigameType.CitrusFlash:
                SetTutorialMessage("Citrus Crash\nPress Space when the screen flashes yellow.");
                break;

            case MinigameType.ManaMixer:
                if (!manaInputEnabled) return;

                SetTutorialMessage("Mana Mixer\nMemorize the ingredient order, then press 1-4 to repeat it.");
                break;

            case MinigameType.BlackoutMemory:
                if (!blackoutInputEnabled) return;

                SetTutorialMessage("Blackout Breeze\nRemember the faded doodle sequence, then press 1-4 to recall it.");
                break;

            case MinigameType.ChaosTyping:
                SetTutorialMessage("Chaos Colada\nType each letter before time runs out.");
                break;

            case MinigameType.BarCrawlBalance:
                SetTutorialMessage("Bar Crawl Survival\nUse A/D or arrow keys to stay centered.");
                break;

            case MinigameType.CritClick:
                SetTutorialMessage("Crit\nSpam click or press Space until you land a crit.");
                break;

            case MinigameType.NeonFill:
                SetTutorialMessage("Neon Margarita\nHold Space to fill. Release inside the target range.");
                break;

            case MinigameType.LagDelay:
                SetTutorialMessage("Lag Juice\nPress Space early. Your input registers after a delay.");
                break;
        }
    }

void UpdateGameTimer()
{
    if (hasLoadedDrivingScene)
        return;

    gameTimeRemaining -= Time.deltaTime;

    if (gameTimeRemaining < 0f)
        gameTimeRemaining = 0f;

    UpdateMainGameUI();

    if (gameTimeRemaining <= 0f)
    {
        hasLoadedDrivingScene = true;

        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartDriving();
        }
        else
        {
            Debug.LogError("GameManager not found. Make sure GameManager exists in the first scene.");
        }
    }
}

    void UpdateMainGameUI()
    {
        if (gameTimerText != null)
        {
            int minutes = Mathf.FloorToInt(gameTimeRemaining / 60f);
            int seconds = Mathf.FloorToInt(gameTimeRemaining % 60f);

            gameTimerText.text = minutes.ToString("00") + ":" + seconds.ToString("00");
        }

        if (drunknessText != null)
        {
            drunknessText.text = "Drunkness: " + totalDrunkness;
        }
    }

    IEnumerator ReturnToDrinkMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

        StopNeonHoldSound();

        if (minigamePanel != null)
            minigamePanel.SetActive(false);

        if (drinkMenuPanel != null)
            drinkMenuPanel.SetActive(true);
    }

    IEnumerator RemoveAfterTime(GameObject obj)
    {
        yield return new WaitForSeconds(drinkLifetime);

        if (obj != null)
            Destroy(obj);
    }
}