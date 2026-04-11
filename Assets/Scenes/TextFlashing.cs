using UnityEngine;
using TMPro;

public class TextFlashing : MonoBehaviour
{
    public TMP_Text myText;

    [Header("Alpha Settings")]
    public float minAlpha = 0.2f;
    public float maxAlpha = 1f;

    [Header("Flash Speed")]
    public float speed = 1.5f;

    private float timer;
    private Color originalColor;

    void Start()
    {
        if (myText == null)
        {
            myText = GetComponent<TMP_Text>();
        }

        originalColor = myText.color;
    }

    void Update()
    {
        FlashText();
    }

    void FlashText()
    {
        // PingPong gives smooth back-and-forth animation
        float alpha = Mathf.Lerp(minAlpha, maxAlpha, Mathf.PingPong(timer * speed, 1f));

        myText.color = new Color(
            originalColor.r,
            originalColor.g,
            originalColor.b,
            alpha
        );

        timer += Time.deltaTime;
    }
}