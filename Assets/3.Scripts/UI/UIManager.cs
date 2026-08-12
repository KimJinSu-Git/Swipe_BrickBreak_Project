using Bird.Ball;
using Bird.Core;
using Bird.OutGame;
using TMPro;
using UnityEngine;

namespace Bird.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Managers (Model)")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private CoinManager coinManager;
        [SerializeField] private BallManager ballManager;
        [SerializeField] private ScoreManager scoreManager;

        [Header("UI Texts (View)")]
        [SerializeField] private TextMeshProUGUI textTurn;
        [SerializeField] private TextMeshProUGUI textCoin;
        [SerializeField] private TextMeshProUGUI textBallCount;
        [SerializeField] private TextMeshProUGUI textScore;
        
        [Header("UI Popups")] 
        [SerializeField] private UIGachaPopup gachaPopup;
        [SerializeField] private GachaManager gachaManager;
        
        private void Start()
        {
            if (turnManager != null) turnManager.OnTurnChanged += UpdateTurnUI;
            if (coinManager != null) coinManager.OnCoinChanged += UpdateCoinUI;
            if (ballManager != null) ballManager.OnBallCountChanged += UpdateBallCountUI;
            if (scoreManager != null) scoreManager.OnScoreChanged += UpdateScoreUI;
            if (gachaManager != null) gachaManager.OnGachaCompleted += gachaPopup.ShowResults;
        }

        private void OnDestroy()
        {
            if (turnManager != null) turnManager.OnTurnChanged -= UpdateTurnUI;
            if (coinManager != null) coinManager.OnCoinChanged -= UpdateCoinUI;
            if (ballManager != null) ballManager.OnBallCountChanged -= UpdateBallCountUI;
            if (scoreManager != null) scoreManager.OnScoreChanged -= UpdateScoreUI;
            if (gachaManager != null) gachaManager.OnGachaCompleted -= gachaPopup.ShowResults;
        }

        private void UpdateTurnUI(int turn) => textTurn.text = turn.ToString();
        private void UpdateCoinUI(int coin) => textCoin.text = coin.ToString();
        private void UpdateBallCountUI(int count) => textBallCount.text = $"x{count}";
        private void UpdateScoreUI(int score) => textScore.text = score.ToString();
    }
}
