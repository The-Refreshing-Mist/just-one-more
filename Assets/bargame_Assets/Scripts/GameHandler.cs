using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class GameHandler : MonoBehaviour
{
    [Header("Drink Spawn")]
    public Button drinkButtonTemplate;
    public GameObject drinkMenuPanel;
    public float spawnInterval = 3f;
    public float drinkLifetime = 10f;

    [Header("Minigame UI")]
    public GameObject minigamePanel;
    public RectTransform miniBarRect;
    public RectTransform greenZoneRect;
    public RectTransform scanLineRect;
    public TMP_Text minigameStatusText;

    [Header("Minigame Settings")]
    public float minigameDuration = 30f;
    public float scanSpeed = 220f;
    public float greenZoneHeight = 20f;

    private int totalDrunkness = 0;

    private bool minigameActive = false;
    private bool movingDown = true;

    private int currentDrinkIndex = -1;
    private int totalZones = 0;
    private int currentZoneNumber = 0;
    private int successfulHits = 0;

    private float lastDrinkEffectiveness = 0f;
    private float minigameTimeRemaining = 0f;

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
        2, // Citrus Crash
        3, // Mana Mixer
        7, // Blackout Breeze
        6, // Chaos Colada
        9, // Bar Crawl Survival
        4, // Crit
        3, // Neon Margarita
        2  // Lag Juice
    };

    void Start()
    {
        if (drinkButtonTemplate == null)
        {
            Debug.LogError("drinkButtonTemplate is not assigned.");
            return;
        }

        drinkButtonTemplate.gameObject.SetActive(false);

        Image img = drinkButtonTemplate.GetComponent<Image>();
        if (img != null)
            img.color = Color.blue;

        if (drinkMenuPanel != null)
            drinkMenuPanel.SetActive(true);

        if (minigamePanel != null)
            minigamePanel.SetActive(false);

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

        MoveScanLine();

        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            ResolveCurrentZone();
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
        if (parentRect == null)
        {
            Debug.LogError("Template button must be inside a UI panel.");
            return;
        }

        Button newDrink = Instantiate(drinkButtonTemplate);
        newDrink.transform.SetParent(parentRect, false);
        newDrink.gameObject.SetActive(true);

        int drinkIndex = Random.Range(0, drinkNames.Length);
        int displayNumber = drinkIndex + 1;

        SetLabel(newDrink, displayNumber.ToString());
        PositionRandomX(newDrink, parentRect);

        newDrink.onClick.RemoveAllListeners();
        newDrink.onClick.AddListener(() => StartDrinkMinigame(newDrink, drinkIndex));

        StartCoroutine(RemoveAfterTime(newDrink.gameObject));

        Debug.Log("Spawned #" + displayNumber + " = " + drinkNames[drinkIndex]);
    }

    void PositionRandomX(Button button, RectTransform parentRect)
    {
        RectTransform rect = button.GetComponent<RectTransform>();
        if (rect == null) return;

        float parentWidth = parentRect.rect.width;
        float buttonWidth = rect.rect.width;

        float minX = (-parentWidth / 2f) + (buttonWidth / 2f);
        float maxX = (parentWidth / 2f) - (buttonWidth / 2f);

        float randomX = Random.Range(minX, maxX);
        rect.anchoredPosition = new Vector2(randomX, rect.anchoredPosition.y);
    }

    void SetLabel(Button button, string text)
    {
        TMP_Text tmp = button.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null)
        {
            tmp.text = text;
            return;
        }

        Text legacy = button.GetComponentInChildren<Text>(true);
        if (legacy != null)
        {
            legacy.text = text;
        }
    }

    void StartDrinkMinigame(Button clickedDrink, int drinkIndex)
    {
        if (clickedDrink != null)
            Destroy(clickedDrink.gameObject);

        currentDrinkIndex = drinkIndex;
        totalZones = drinkDrunkValues[drinkIndex] * 2;
        currentZoneNumber = 1;
        successfulHits = 0;
        lastDrinkEffectiveness = 0f;
        minigameTimeRemaining = minigameDuration;
        minigameActive = true;
        movingDown = true;

        if (drinkMenuPanel != null)
            drinkMenuPanel.SetActive(false);

        if (minigamePanel != null)
            minigamePanel.SetActive(true);

        SetupCurrentZone();
        UpdateMinigameStatus();

        Debug.Log("Started minigame for " + drinkNames[drinkIndex] +
                  " | drunk value = " + drinkDrunkValues[drinkIndex] +
                  " | total green zones = " + totalZones);
    }

    void SetupCurrentZone()
    {
        if (miniBarRect == null || greenZoneRect == null || scanLineRect == null)
        {
            Debug.LogError("Minigame UI references are missing.");
            return;
        }

        float halfBarHeight = miniBarRect.rect.height / 2f;
        float halfGreenHeight = greenZoneHeight / 2f;

        scanLineRect.anchoredPosition = new Vector2(
            scanLineRect.anchoredPosition.x,
            halfBarHeight
        );

        movingDown = true;

        float minGreenY = -halfBarHeight + halfGreenHeight;
        float maxGreenY = halfBarHeight - halfGreenHeight;
        float randomGreenY = Random.Range(minGreenY, maxGreenY);

        greenZoneRect.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, greenZoneHeight);
        greenZoneRect.anchoredPosition = new Vector2(
            greenZoneRect.anchoredPosition.x,
            randomGreenY
        );
    }

    void MoveScanLine()
    {
        if (miniBarRect == null || scanLineRect == null)
            return;

        float halfBarHeight = miniBarRect.rect.height / 2f;
        Vector2 pos = scanLineRect.anchoredPosition;

        if (movingDown)
            pos.y -= scanSpeed * Time.deltaTime;
        else
            pos.y += scanSpeed * Time.deltaTime;

        if (pos.y <= -halfBarHeight)
        {
            pos.y = -halfBarHeight;
            movingDown = false;
        }
        else if (pos.y >= halfBarHeight)
        {
            pos.y = halfBarHeight;
            movingDown = true;
        }

        scanLineRect.anchoredPosition = pos;
    }

    void ResolveCurrentZone()
    {
        if (greenZoneRect == null || scanLineRect == null)
            return;

        float lineY = scanLineRect.anchoredPosition.y;
        float greenCenterY = greenZoneRect.anchoredPosition.y;
        float halfGreenHeight = greenZoneRect.rect.height / 2f;

        bool inGreen = lineY >= greenCenterY - halfGreenHeight &&
                       lineY <= greenCenterY + halfGreenHeight;

        if (inGreen)
        {
            successfulHits++;
            Debug.Log("SUCCESS on zone " + currentZoneNumber + "/" + totalZones);
        }
        else
        {
            Debug.Log("MISS on zone " + currentZoneNumber + "/" + totalZones);
        }

        if (currentZoneNumber >= totalZones)
        {
            FinishMinigame();
        }
        else
        {
            currentZoneNumber++;
            SetupCurrentZone();
            UpdateMinigameStatus();
        }
    }

    void FinishMinigame()
    {
        if (!minigameActive)
            return;

        minigameActive = false;

        if (totalZones > 0)
            lastDrinkEffectiveness = (float)successfulHits / totalZones;
        else
            lastDrinkEffectiveness = 0f;

        if (currentDrinkIndex >= 0)
            totalDrunkness += drinkDrunkValues[currentDrinkIndex];

        Debug.Log(
            "Finished " + drinkNames[currentDrinkIndex] +
            " | hits = " + successfulHits + "/" + totalZones +
            " | effectiveness = " + Mathf.RoundToInt(lastDrinkEffectiveness * 100f) + "%" +
            " | total drunkness = " + totalDrunkness
        );

        if (minigameStatusText != null)
        {
            minigameStatusText.text =
                "Done\n" +
                "Hits: " + successfulHits + "/" + totalZones + "\n" +
                "Effectiveness: " + Mathf.RoundToInt(lastDrinkEffectiveness * 100f) + "%\n" +
                "Total Drunkness: " + totalDrunkness;
        }

        StartCoroutine(ReturnToDrinkMenuAfterDelay(1.2f));
    }

    void UpdateMinigameStatus()
    {
        if (minigameStatusText == null)
            return;

        minigameStatusText.text =
            "Drink #" + (currentDrinkIndex + 1) + "\n" +
            "Zone: " + currentZoneNumber + "/" + totalZones + "\n" +
            "Hits: " + successfulHits + "\n" +
            "Time Left: " + Mathf.CeilToInt(minigameTimeRemaining) + "s";
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