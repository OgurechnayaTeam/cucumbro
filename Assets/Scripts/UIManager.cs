using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text levelText;       
    [SerializeField] private TMP_Text enemiesText;     
    [SerializeField] private Image xpFillImage;   

    [Header("Settings")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int enemiesLeft = 5;
    [SerializeField] private float currentXP = 50f;
    [SerializeField] private float maxXP = 100f;

    void Start()
    {
        UpdateUI();
    }


    public void UpdateUI()
    {
        if (levelText != null)
            levelText.text = $"Level: {currentLevel}";

        if (enemiesText != null)
            enemiesText.text = $"Enemies: {enemiesLeft}";

        if (xpFillImage != null)
        {
            float xpPercent = Mathf.Clamp01(currentXP / maxXP);
            xpFillImage.fillAmount = xpPercent;
        }
    }


    public void AddXP(float amount)
    {
        currentXP += amount;
        if (currentXP >= maxXP)
        {
            LevelUp();
        }
        UpdateUI();
    }

    public void SetEnemiesCount(int count)
    {
        enemiesLeft = count;
        UpdateUI();
    }

    private void LevelUp()
    {
        currentLevel++;
        currentXP = 0; 
        maxXP *= 1.5f; 
        Debug.Log("Level Up!");
        UpdateUI();
    }
}