using UnityEngine;

namespace Bird.Data
{
    [CreateAssetMenu(fileName = "SkillData", menuName = "Bird/Data/SkillData")]
    public class SkillData : ScriptableObject
    {
        [Header("Skill Settings : Line Strike")] 
        [SerializeField] private float maxGauge = 100f;
        [SerializeField] private int baseDamage = 1;

        public float MaxGauge => maxGauge;

        public int GetDamage(int currentTurn, int skillLevel)
        {
            return baseDamage + currentTurn + skillLevel;
        }

        public int GetTargetRowCount(int skillLevel)
        {
            return skillLevel;
        }
    }
}
