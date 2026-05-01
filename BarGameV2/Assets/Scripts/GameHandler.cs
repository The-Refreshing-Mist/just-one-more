using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class GameHandler : MonoBehaviour
{
    enum MinigameType
    {
        CitrusFlash,
        ManaMixer,
        ChaosTyping,
        CritClick
    }

    [Header("Drink Spawn")]
    public Button drinkButtonTemplate;
    public GameObject drinkMenuPanel;
    public float spawnInterval = 3f;
    public float drinkLifetime = 10f;

    [Header("Drink Sprites")]
    public Sprite[] drinkSprites;

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

    [Header("Minigame Settings")]
    public float minigameDuration = 30f;

    [Header("Citrus Settings")]
    public float citrusWaitMin = 0.6f;
    public float citrusWaitMax = 1.8f;
    public float citrusFlashWindow = 0.55f;

    [Header("Crit Settings")]
    public float critChance = 0.12f;

    private int totalDrunkness = 0;

    private bool minigameActive = false;
    private int currentDrinkIndex = -1;
    private MinigameType currentMinigame;

    private float minigameTimeRemaining = 0f;
    private float lastDrinkEffectiveness = 0f;

    // Citrus Flash
    private int citrusTotalFlashes = 0;
    private int citrusCurrentFlash = 0;
    private int citrusHits = 0;
    private bool citrusFlashActive = false;
    private float citrusTimer = 0f;

    // Mana Mixer
    private string[] manaIngredients = { "Ice", "Blue Mana", "Lime", "Spark Dust" };
    private int[] manaSequence;
    private int manaStep = 0;
    private int manaCorrect = 0;
    private bool manaInputEnabled = false;

    // Chaos Colada
    private string[] chaosWords = { "fear", "buzz", "chaos", "rage", "neon", "void" };
    private string chaosWord = "";
    private int chaosLetterIndex = 0;
    private int chaosCorrect = 0;
    private float chaosLetterTimer = 0f;
    private float chaosLetterTimeLimit = 2.5f;

    // Crit
    private int critClicks = 0;
    private bool critLanded = false;

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

        if (citrusFlashOverlay != null)
            citrusFlashOverlay.gameObject.SetActive(false);

        if (critBoxVisual != null)
            critBoxVisual.SetActive(false);

        SetTimingBarVisible(false);

        StartCoroutine(SpawnLoop());
    }

    void Update()
    {
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

            case MinigameType.ChaosTyping:
                UpdateChaosTyping();
                break;

            case MinigameType.CritClick:
                UpdateCritClick();
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

        int drinkIndex = Random.Range(0, drinkNames.Length);

        ApplyDrinkSprite(newDrink, drinkIndex);
        PositionRandomX(newDrink, parentRect);

        newDrink.onClick.RemoveAllListeners();
        newDrink.onClick.AddListener(() => StartDrinkMinigame(newDrink, drinkIndex));

        StartCoroutine(RemoveAfterTime(newDrink.gameObject));
    }

    void ApplyDrinkSprite(Button button, int drinkIndex)
    {
        Image img = button.GetComponent<Image>();

        if (img != null && drinkSprites.Length > drinkIndex)
        {
            img.sprite = drinkSprites[drinkIndex];
            img.color = Color.white;
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

        SetTimingBarVisible(false);

        if (citrusFlashOverlay != null)
            citrusFlashOverlay.gameObject.SetActive(false);

        if (critBoxVisual != null)
            critBoxVisual.SetActive(false);

        switch (currentMinigame)
        {
            case MinigameType.CitrusFlash:
                StartCitrusFlash();
                break;

            case MinigameType.ManaMixer:
                StartManaMixer();
                break;

            case MinigameType.ChaosTyping:
                StartChaosTyping();
                break;

            case MinigameType.CritClick:
                StartCritClick();
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
            case 3: return MinigameType.ChaosTyping;
            case 5: return MinigameType.CritClick;

            default:
                return MinigameType.CitrusFlash;
        }
    }

    void SetTimingBarVisible(bool visible)
    {
        if (miniBarRect != null)
            miniBarRect.gameObject.SetActive(visible);
    }

    // -------------------------
    // Citrus Crash flash reaction
    // -------------------------

    void StartCitrusFlash()
    {
        SetTimingBarVisible(false);

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
                citrusHits++;
                AdvanceCitrusFlash();
                return;
            }

            if (citrusTimer <= 0f)
            {
                AdvanceCitrusFlash();
            }
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

    // -------------------------
    // Mana Mixer ingredient sequence
    // -------------------------

    void StartManaMixer()
    {
        SetTimingBarVisible(false);

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
        string sequenceText = "Memorize Mana Mixer:\n";

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

        int pressedIngredient = GetPressedIngredientNumber();

        if (pressedIngredient == -1)
            return;

        if (pressedIngredient == manaSequence[manaStep])
            manaCorrect++;

        manaStep++;

        if (manaStep >= manaSequence.Length)
        {
            FinishMinigame();
        }
    }

    int GetPressedIngredientNumber()
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

    // -------------------------
    // Chaos Colada typing game
    // -------------------------

    void StartChaosTyping()
    {
        SetTimingBarVisible(false);

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
        {
            FinishMinigame();
        }
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

    // -------------------------
    // Crit click game
    // -------------------------

    void StartCritClick()
    {
        SetTimingBarVisible(false);

        critClicks = 0;
        critLanded = false;

        if (critBoxVisual != null)
            critBoxVisual.SetActive(true);
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

        critClicks++;

        if (Random.value <= critChance)
        {
            critLanded = true;
            FinishMinigame();
        }
    }

    // -------------------------
    // Finish / UI
    // -------------------------

    void FinishMinigame()
    {
        if (!minigameActive)
            return;

        minigameActive = false;

        if (citrusFlashOverlay != null)
            citrusFlashOverlay.gameObject.SetActive(false);

        if (critBoxVisual != null)
            critBoxVisual.SetActive(false);

        switch (currentMinigame)
        {
            case MinigameType.CitrusFlash:
                lastDrinkEffectiveness = citrusTotalFlashes > 0
                    ? (float)citrusHits / citrusTotalFlashes
                    : 0f;
                break;

            case MinigameType.ManaMixer:
                lastDrinkEffectiveness = manaSequence != null && manaSequence.Length > 0
                    ? (float)manaCorrect / manaSequence.Length
                    : 0f;
                break;

            case MinigameType.ChaosTyping:
                lastDrinkEffectiveness = chaosWord.Length > 0
                    ? (float)chaosCorrect / chaosWord.Length
                    : 0f;
                break;

            case MinigameType.CritClick:
                lastDrinkEffectiveness = critLanded ? 1f : 0f;
                break;
        }

        if (currentDrinkIndex >= 0)
            totalDrunkness += drinkDrunkValues[currentDrinkIndex];

        if (minigameStatusText != null)
        {
            minigameStatusText.text =
                "Done\n" +
                drinkNames[currentDrinkIndex] + "\n" +
                "Effectiveness: " + Mathf.RoundToInt(lastDrinkEffectiveness * 100f) + "%\n" +
                "Total Drunkness: " + totalDrunkness;
        }

        StartCoroutine(ReturnToDrinkMenuAfterDelay(1.2f));
    }

    void UpdateMinigameStatus()
    {
        if (minigameStatusText == null) return;

        switch (currentMinigame)
        {
            case MinigameType.CitrusFlash:
                minigameStatusText.text =
                    "Citrus Crash\n" +
                    "Press Space when the screen flashes yellow\n" +
                    "Flash: " + citrusCurrentFlash + "/" + citrusTotalFlashes +
                    "\nHits: " + citrusHits +
                    "\nTime: " + Mathf.CeilToInt(minigameTimeRemaining);
                break;

            case MinigameType.ManaMixer:
                if (!manaInputEnabled)
                    return;

                minigameStatusText.text =
                    "Mana Mixer\n" +
                    "Add ingredients in order:\n" +
                    "1 = Ice\n" +
                    "2 = Blue Mana\n" +
                    "3 = Lime\n" +
                    "4 = Spark Dust\n" +
                    "Step: " + (manaStep + 1) + "/" + manaSequence.Length +
                    "\nCorrect: " + manaCorrect;
                break;

            case MinigameType.ChaosTyping:
                minigameStatusText.text =
                    "Chaos Colada\n" +
                    "Word: " + chaosWord +
                    "\nType: " + chaosWord[chaosLetterIndex] +
                    "\nProgress: " + chaosLetterIndex + "/" + chaosWord.Length +
                    "\nTime for letter: " + chaosLetterTimer.ToString("F1");
                break;

            case MinigameType.CritClick:
                minigameStatusText.text =
                    "Crit\n" +
                    "Click the center box or press Space until you crit\n" +
                    "Clicks: " + critClicks +
                    "\nTime: " + Mathf.CeilToInt(minigameTimeRemaining);
                break;
        }
    }

    IEnumerator ReturnToDrinkMenuAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);

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