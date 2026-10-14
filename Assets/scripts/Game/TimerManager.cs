using UnityEngine;
using UnityEngine.UI;

public sealed class TimerManager : MonoBehaviour
{
    public static TimerManager Instance { get; private set; }

    [SerializeField] private Text timerText;
    [SerializeField] private float startTimeSeconds = 120f;

    [Header("Экран окончания игры")]
    [SerializeField] private GameObject endGameMenu;

    private float _timeRemaining;
    private bool  _isRunning;

    private void Awake()
    {
        Instance = this;
        _timeRemaining = startTimeSeconds + StoreInventory.GetBoosterStat(BoosterEffect.ExtraTime);
        UpdateDisplay();
    }

    public void StartTimer() => _isRunning = true;

    private void Update()
    {
        if (!_isRunning) return;

        _timeRemaining -= Time.deltaTime;
        if (_timeRemaining <= 0f)
        {
            _timeRemaining = 0f;
            _isRunning = false;
            UpdateDisplay();
            OnTimeUp();
            return;
        }

        UpdateDisplay();
    }

    public void AddTime(float seconds)
    {
        if (!_isRunning) return;
        _timeRemaining += seconds;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (timerText == null) return;

        var minutes = Mathf.FloorToInt(_timeRemaining / 60f);
        var seconds = Mathf.FloorToInt(_timeRemaining % 60f);
        timerText.text = $"{minutes}:{seconds:00}";
    }

    private async void OnTimeUp()
    {
        if (Board.Instance != null)
            await Board.Instance.ActivateRemainingSpecials();

        Time.timeScale = 0f;
        if (endGameMenu != null) endGameMenu.SetActive(true);
    }
}
