using System;
using TMPro;
using UnityEngine;

namespace Bird.InGame
{
    public enum BlockType { Normal, Multiply, Recovery, Invincible }
    public class Block : MonoBehaviour
    {
        [Header("Block Info")] 
        [SerializeField] private BlockType blockType;
        
        [SerializeField] protected int currentHp;
        [SerializeField] protected TextMeshPro textHp;

        protected int maxHp;
        
        private BlockManager _blockManager;
        public int CurrentHp => currentHp;
        public BlockType Type => blockType;
        public virtual bool CausesGameOver => true;

        public virtual void Initialize(int hp, BlockManager manager)
        {
            maxHp = hp;
            currentHp = hp;
            _blockManager = manager;
            UpdateHpText();
        }

        public virtual int TakeDamage(int damage)
        {
            int actualDamage = Math.Min(currentHp, damage);
            currentHp -= actualDamage;

            UpdateHpText();
            
            if (currentHp <= 0)
            {
                if (_blockManager != null) 
                {
                    _blockManager.ReturnBlockToPool(gameObject);
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
            if (blockType == BlockType.Invincible)
            {
                textHp.enabled = false;
            }
            if (textHp != null) textHp.text = currentHp.ToString();
        }

        // 턴 종료 시 호출될 메소드
        public virtual void OnTurnEnd(BlockManager blockManager, Vector2Int gridIndex)
        {
            
        }

        public virtual void Heal(int amount)
        {
            currentHp = Mathf.Min(currentHp + amount, maxHp);
            UpdateHpText();
        }

        public virtual void ForceDestroy()
        {
            if(_blockManager != null) _blockManager.ReturnBlockToPool(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
