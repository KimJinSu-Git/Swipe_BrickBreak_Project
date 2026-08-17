using System;
using Bird.Data;
using Bird.InGame;
using UnityEngine;

namespace Bird.Core
{
    public class SkillManager : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private SkillData skillData;
        [SerializeField] private BlockManager blockManager;

        [Header("Runtime State")] 
        [SerializeField] private float currentGauge = 0f;
        [SerializeField] private int skillLevel = 1;
        
        public event Action<float, float> OnGaugeChanged;
        public event Action<int> OnLevelChanged;

        public bool IsSkillReady => currentGauge >= skillData.MaxGauge;

        private void Start()
        {
            OnGaugeChanged?.Invoke(currentGauge, skillData.MaxGauge);
            OnLevelChanged?.Invoke(skillLevel);
        }

        // 블록에 데미지를 가할 때 호출되어 게이지를 충전시킵니다.
        public void AddGauge(float amount)
        {
            if (IsSkillReady) return;
            
            currentGauge = Mathf.Min(currentGauge + amount, skillData.MaxGauge);
            OnGaugeChanged?.Invoke(currentGauge, skillData.MaxGauge);
        }

        public void ExecuteLineStrike(int targetRow, int currentTurn)
        {
            if (!IsSkillReady) return;

            int damage = skillData.GetDamage(currentTurn, skillLevel);
            int rowCount = skillData.GetTargetRowCount(skillLevel);

            int startRow = Mathf.Max(0, targetRow - (rowCount - 1) / 2);
            int endRow = Mathf.Min(blockManager.MaxRows - 1, startRow + rowCount - 1);
            
            blockManager.DamageRows(startRow, endRow, damage);
            
            currentGauge = 0f;
            skillLevel++;
            
            OnGaugeChanged?.Invoke(currentGauge, skillData.MaxGauge);
            OnLevelChanged?.Invoke(skillLevel);

            Debug.Log($"[Skill] Line Strike 발동! 타격 줄: {startRow}~{endRow}, 데미지: {damage}, 다음 레벨: {skillLevel}");
        }
    }
}
