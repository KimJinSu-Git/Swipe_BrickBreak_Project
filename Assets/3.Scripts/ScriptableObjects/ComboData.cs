using UnityEngine;

namespace Bird.Data
{
    /// <summary>
    /// 콤보 달성 수치와 그에 따른 보너스 배율을 관리하는 데이터입니다.
    /// </summary>
    [CreateAssetMenu(fileName = "ComboData", menuName = "Bird/Data/ComboData")]
    public class ComboData : ScriptableObject
    {
        [Header("Base Requirements")]
        [SerializeField] private int baseTier1Combo = 50;
        [SerializeField] private int baseTier2Combo = 100;
        [SerializeField] private int baseTier3Combo = 150;
        
        [Header("Dynamic Scaling")]
        [Tooltip("공이 늘어날 때마다 증가하는 요구 콤보 수치입니다.")]
        [SerializeField] private float requiredComboPerBall = 1.5f;

        [Header("Bonus Multipliers")]
        [SerializeField] private float tier1CoinBonus = 1.05f;
        [SerializeField] private float tier2CoinBonus = 1.20f;
        [SerializeField] private float tier3SkillBonus = 1.20f;

        private int GetTier1Req(int ballCount) => baseTier1Combo + Mathf.RoundToInt((ballCount -1) * requiredComboPerBall);
        private int GetTier2Req(int ballCount) => baseTier2Combo + Mathf.RoundToInt((ballCount -1) * requiredComboPerBall);
        private int GetTier3Req(int ballCount) => baseTier3Combo + Mathf.RoundToInt((ballCount -1) * requiredComboPerBall);
        
        public float GetCoinMultiplier(int currentCombo, int ballCount)
        {
            if (currentCombo >= GetTier2Req(ballCount)) return tier2CoinBonus;
            if (currentCombo >= GetTier1Req(ballCount)) return tier1CoinBonus;
            return 1.0f;
        }

        public float GetSkillMultiplier(int currentCombo, int ballCount)
        { 
            return currentCombo >= GetTier3Req(ballCount) ? tier3SkillBonus : 1.0f;
        }
        
    }
}
