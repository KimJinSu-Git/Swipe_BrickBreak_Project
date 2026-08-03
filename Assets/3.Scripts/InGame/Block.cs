using System;
using UnityEngine;

namespace Bird.InGame
{
    public class Block : MonoBehaviour
    {
        [SerializeField] private int currentHp;
        
        public int CurrentHp => currentHp;
        
        public void Initialize(int hp) => currentHp = hp;

        public int TakeDamage(int damage)
        {
            int actualDamage = Math.Min(currentHp, damage);
            currentHp -= actualDamage;

            if (currentHp <= 0)
            {
                // TODO :: 오브젝트 풀로 반환 추가 예정
                gameObject.SetActive(false);
            }
            return actualDamage;
        }
    }
}
