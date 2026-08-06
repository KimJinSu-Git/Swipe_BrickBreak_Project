using System;
using TMPro;
using UnityEngine;

namespace Bird.InGame
{
    public class Block : MonoBehaviour
    {
        [SerializeField] private int currentHp;
        [SerializeField] private TextMeshPro textHp;
        public int CurrentHp => currentHp;

        public void Initialize(int hp)
        {
            currentHp = hp;
            UpdateHpText();
        }

        public int TakeDamage(int damage)
        {
            int actualDamage = Math.Min(currentHp, damage);
            currentHp -= actualDamage;

            UpdateHpText();
            
            if (currentHp <= 0)
            {
                // TODO :: 오브젝트 풀로 반환 추가 예정
                gameObject.SetActive(false);
            }
            return actualDamage;
        }

        private void UpdateHpText()
        {
            if (textHp != null) textHp.text = currentHp.ToString();
        }
    }
}
