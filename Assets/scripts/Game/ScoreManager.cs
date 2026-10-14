using UnityEngine;
using UnityEngine.UI;

public sealed class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private Text scoreText;

    private int _score;

    public int Score => _score;

    public int TileBonus { get; private set; }

    private void Awake()
    {
        Instance = this;
        UpdateDisplay();

        TileBonus = Mathf.RoundToInt(StoreInventory.GetBoosterStat(BoosterEffect.ScoreBonus));
    }

    public void AddScore(int amount)
    {
        _score += amount;
        UpdateDisplay();
    }

    public void ResetScore()
    {
        _score = 0;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (scoreText != null)
            scoreText.text = _score.ToString();
    }

    public void AddTileScore(int value) => AddScore(value + TileBonus);
}
