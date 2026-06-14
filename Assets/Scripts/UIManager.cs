using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerDarya player;
    [SerializeField] private TMP_Text levelText;       
    [SerializeField] private TMP_Text enemiesText;     
    [SerializeField] private Image hpFillImage;

    [Header("Settings")]
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int enemiesLeft;

    private GameObject gameOverPanel;
    private GameObject classSelectionPanel;
    private bool enemiesHaveSpawned;
    private Action<WeaponManager.PlayerClass> classSelectedCallback;

    void Start()
    {
        EnsureGameOverPanel();
        UpdateUI();
    }

    private void Update()
    {
        UpdateHealthBar();
    }

    public void BindPlayer(Transform playerTransform)
    {
        player = playerTransform != null ? playerTransform.GetComponent<PlayerDarya>() : null;
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (levelText != null)
            levelText.text = $"Level: {currentLevel}";

        if (enemiesText != null)
            enemiesText.text = $"Enemies: {enemiesLeft}";

        UpdateHealthBar();
    }

    public void SetEnemiesCount(int count)
    {
        enemiesLeft = Mathf.Max(0, count);

        if (enemiesLeft > 0)
        {
            enemiesHaveSpawned = true;
            SetGameOverVisible(false);
        }
        else if (enemiesHaveSpawned)
        {
            SetGameOverVisible(true);
        }

        UpdateUI();
    }

    public void ShowClassSelection(Action<WeaponManager.PlayerClass> onClassSelected)
    {
        classSelectedCallback = onClassSelected;
        EnsureClassSelectionPanel();
        SetGameOverVisible(false);

        if (classSelectionPanel != null)
            classSelectionPanel.SetActive(true);
    }

    public void ShowGameOver()
    {
        SetGameOverVisible(true);
    }

    private void UpdateHealthBar()
    {
        if (hpFillImage == null)
            return;

        if (player == null || player.MaxHealth <= 0)
        {
            hpFillImage.fillAmount = 0f;
            return;
        }

        hpFillImage.fillAmount = Mathf.Clamp01((float)player.CurrentHealth / player.MaxHealth);
    }

    private void EnsureGameOverPanel()
    {
        if (gameOverPanel != null)
            return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            return;

        gameOverPanel = new GameObject("GameOverPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        gameOverPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = gameOverPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = gameOverPanel.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.45f);

        GameObject textObject = new GameObject("GameOverText", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(gameOverPanel.transform, false);
        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0.5f, 0.5f);
        textRect.anchorMax = new Vector2(0.5f, 0.5f);
        textRect.anchoredPosition = new Vector2(0f, 48f);
        textRect.sizeDelta = new Vector2(420f, 80f);

        TMP_Text gameOverText = textObject.GetComponent<TMP_Text>();
        gameOverText.text = "Game Over";
        gameOverText.alignment = TextAlignmentOptions.Center;
        gameOverText.fontSize = 48f;
        gameOverText.color = Color.white;

        GameObject buttonObject = new GameObject("RestartButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(gameOverPanel.transform, false);
        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = new Vector2(0f, -36f);
        buttonRect.sizeDelta = new Vector2(180f, 48f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(1f, 1f, 1f, 0.92f);

        Button restartButton = buttonObject.GetComponent<Button>();
        restartButton.onClick.AddListener(RestartScene);

        GameObject buttonTextObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        buttonTextObject.transform.SetParent(buttonObject.transform, false);
        RectTransform buttonTextRect = buttonTextObject.GetComponent<RectTransform>();
        buttonTextRect.anchorMin = Vector2.zero;
        buttonTextRect.anchorMax = Vector2.one;
        buttonTextRect.offsetMin = Vector2.zero;
        buttonTextRect.offsetMax = Vector2.zero;

        TMP_Text buttonText = buttonTextObject.GetComponent<TMP_Text>();
        buttonText.text = "Restart";
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontSize = 24f;
        buttonText.color = Color.black;

        SetGameOverVisible(false);
    }

    private void EnsureClassSelectionPanel()
    {
        if (classSelectionPanel != null)
            return;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null)
            return;

        classSelectionPanel = new GameObject("ClassSelectionPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        classSelectionPanel.transform.SetParent(canvas.transform, false);

        RectTransform panelRect = classSelectionPanel.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        Image panelImage = classSelectionPanel.GetComponent<Image>();
        panelImage.color = new Color(0f, 0f, 0f, 0.65f);

        GameObject titleObject = new GameObject("ClassSelectionTitle", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        titleObject.transform.SetParent(classSelectionPanel.transform, false);
        RectTransform titleRect = titleObject.GetComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.5f);
        titleRect.anchorMax = new Vector2(0.5f, 0.5f);
        titleRect.anchoredPosition = new Vector2(0f, 92f);
        titleRect.sizeDelta = new Vector2(500f, 70f);

        TMP_Text titleText = titleObject.GetComponent<TMP_Text>();
        titleText.text = "Choose Class";
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontSize = 42f;
        titleText.color = Color.white;

        CreateClassButton("KatanaButton", "Katana", new Vector2(-110f, 0f), WeaponManager.PlayerClass.Katana);
        CreateClassButton("GunButton", "Gun", new Vector2(110f, 0f), WeaponManager.PlayerClass.Gun);

        classSelectionPanel.SetActive(false);
    }

    private void CreateClassButton(string objectName, string label, Vector2 position, WeaponManager.PlayerClass playerClass)
    {
        GameObject buttonObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        buttonObject.transform.SetParent(classSelectionPanel.transform, false);

        RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
        buttonRect.anchoredPosition = position;
        buttonRect.sizeDelta = new Vector2(180f, 56f);

        Image buttonImage = buttonObject.GetComponent<Image>();
        buttonImage.color = new Color(1f, 1f, 1f, 0.94f);

        Button button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(() => SelectClass(playerClass));

        GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        textObject.transform.SetParent(buttonObject.transform, false);

        RectTransform textRect = textObject.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;

        TMP_Text buttonText = textObject.GetComponent<TMP_Text>();
        buttonText.text = label;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.fontSize = 24f;
        buttonText.color = Color.black;
    }

    private void SelectClass(WeaponManager.PlayerClass playerClass)
    {
        if (classSelectionPanel != null)
            classSelectionPanel.SetActive(false);

        enemiesHaveSpawned = false;
        classSelectedCallback?.Invoke(playerClass);
    }

    private void SetGameOverVisible(bool visible)
    {
        EnsureGameOverPanel();
        if (gameOverPanel != null)
            gameOverPanel.SetActive(visible);
    }

    private void RestartScene()
    {
        Scene currentScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(currentScene.name);
    }
}
