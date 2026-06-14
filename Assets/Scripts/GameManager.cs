using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private LevelManager levelManager;
    [SerializeField] private bool startLevelOnStart = true;

    private int lastStartedSceneHandle = -1;
    private bool classSelected;
    private WeaponManager.PlayerClass selectedPlayerClass = WeaponManager.PlayerClass.Katana;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Start()
    {
        if (!startLevelOnStart)
        {
            return;
        }

        ShowClassSelectionOrStart();
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (!startLevelOnStart)
        {
            return;
        }

        levelManager = null;
        classSelected = false;
        ShowClassSelectionOrStart();
    }

    public void SelectPlayerClass(WeaponManager.PlayerClass playerClass)
    {
        selectedPlayerClass = playerClass;
        classSelected = true;
        StartNewRunForActiveScene();
    }

    public void StartNewRun()
    {
        ResolveLevelManager();

        if (levelManager == null)
        {
            Debug.LogWarning("GameManager could not find a LevelManager to start a new run.");
            return;
        }

        levelManager.StartRun(selectedPlayerClass);
    }

    private void ShowClassSelectionOrStart()
    {
        if (classSelected)
        {
            StartNewRunForActiveScene();
            return;
        }

        UIManager uiManager = FindAnyObjectByType<UIManager>();
        if (uiManager != null)
        {
            uiManager.ShowClassSelection(SelectPlayerClass);
            return;
        }

        classSelected = true;
        StartNewRunForActiveScene();
    }

    private void StartNewRunForActiveScene()
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (lastStartedSceneHandle == activeScene.handle)
        {
            return;
        }

        lastStartedSceneHandle = activeScene.handle;
        StartNewRun();
    }

    private void ResolveLevelManager()
    {
        if (levelManager == null)
        {
            levelManager = FindAnyObjectByType<LevelManager>();
        }
    }
}
