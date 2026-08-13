using System;
using TMPro;
using UnityEngine;

namespace Bird.InGame
{
    public class Block : MonoBehaviour
    {
        [SerializeField] protected int currentHp;
        [SerializeField] protected TextMeshPro textHp;

        private BlockManager blockManager;
        public int CurrentHp => currentHp;

        public virtual void Initialize(int hp, BlockManager manager)
        {
            currentHp = hp;
            blockManager = manager;
            UpdateHpText();
        }

        public virtual int TakeDamage(int damage)
        {
            int actualDamage = Math.Min(currentHp, damage);
            currentHp -= actualDamage;

            UpdateHpText();
            
            if (currentHp <= 0)
            {
                if (blockManager != null) 
                {
                    blockManager.ReturnBlockToPool(gameObject);
                }
                else 
                {
                    gameObject.SetActive(false);
                }
            }
            return actualDamage;
        }

        protected void UpdateHpText()
        {
            if (textHp != null) textHp.text = currentHp.ToString();
        }

        // 턴 종료 시 호출될 메소드
        public virtual void OnTurnEnd(BlockManager blockManager, Vector2Int gridIndex)
        {
            
        }
    }
}
