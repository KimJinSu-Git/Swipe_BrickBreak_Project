using Bird.Ball;
using Bird.Core;
using Bird.OutGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bird.UI
{
    public class UIManager : MonoBehaviour
    {
        [Header("Managers (Model)")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private CoinManager coinManager;
        [SerializeField] private BallManager ballManager;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private ComboManager comboManager;
        [SerializeField] private SkillManager skillManager;

        [Header("UI Texts (View)")]
        [SerializeField] private TextMeshProUGUI textTurn;
        [SerializeField] private TextMeshProUGUI textCoin;
        [SerializeField] private TextMeshProUGUI textBallCount;
        [SerializeField] private TextMeshProUGUI textScore;
        [SerializeField] private TextMeshProUGUI comboScore;
        
        [Header("Skill UI (View)")]
        [SerializeField] private TextMeshProUGUI textSkillLevel;
        [SerializeField] private TextMeshProUGUI textSkillGauge;
        [SerializeField] private Image imageSkillGaugeFill;
        [SerializeField] private Button buttonSkillUse;
        
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
            if (comboManager != null) comboManager.OnComboChanged += UpdateComboUI;
            
            if (skillManager != null)
            {
                skillManager.OnGaugeChanged += UpdateSkillGaugeUI;
                skillManager.OnLevelChanged += UpdateSkillLevelUI;
            }

            if (buttonSkillUse != null && turnManager != null)
            {
                buttonSkillUse.onClick.AddListener(turnManager.OnSkillButtonClicked);
            }
            
            InitializeUI();
        }

        private void InitializeUI()
        {
            UpdateTurnUI(1);
            UpdateCoinUI(0);
            UpdateBallCountUI(1);
            UpdateScoreUI(0);
            UpdateComboUI(0);
            UpdateSkillGaugeUI(0, 100);
            UpdateSkillLevelUI(1);
        }

        private void OnDestroy()
        {
            if (turnManager != null) turnManager.OnTurnChanged -= UpdateTurnUI;
            if (coinManager != null) coinManager.OnCoinChanged -= UpdateCoinUI;
            if (ballManager != null) ballManager.OnBallCountChanged -= UpdateBallCountUI;
            if (scoreManager != null) scoreManager.OnScoreChanged -= UpdateScoreUI;
            if (gachaManager != null) gachaManager.OnGachaCompleted -= gachaPopup.ShowResults;
            if (comboManager != null) comboManager.OnComboChanged -= UpdateComboUI;
            
            if (skillManager != null)
            {
                skillManager.OnGaugeChanged -= UpdateSkillGaugeUI;
                skillManager.OnLevelChanged -= UpdateSkillLevelUI;
            }
            if (buttonSkillUse != null) buttonSkillUse.onClick.RemoveAllListeners();
        }

        private void UpdateTurnUI(int turn) => textTurn.text = turn.ToString();
        private void UpdateCoinUI(int coin) => textCoin.text = coin.ToString();
        private void UpdateBallCountUI(int count) => textBallCount.text = $"x{count}";
        private void UpdateScoreUI(int score) => textScore.text = score.ToString();
        private void UpdateComboUI(int combo) => comboScore.text = $"x{combo}";
        
        private void UpdateSkillGaugeUI(float current, float max)
        {
            float ratio = Mathf.Clamp01(current / max); 
            
            if (imageSkillGaugeFill != null) imageSkillGaugeFill.fillAmount = ratio;
            
            if (textSkillGauge != null) textSkillGauge.text = $"{(ratio * 100f):F2}%"; 

            if (buttonSkillUse != null) buttonSkillUse.interactable = current >= max;
        }

        private void UpdateSkillLevelUI(int level)
        {
            if (textSkillLevel != null) textSkillLevel.text = $"Level.{level}";
        }
    }
}
