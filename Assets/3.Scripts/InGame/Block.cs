using System;
using System.Collections;
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
        
        [Header("Shake Settings")]
        [SerializeField] private float shakeDuration = 0.1f;
        [SerializeField] private float shakeMagnitude = 0.05f;

        protected int maxHp;
        
        private BlockManager _blockManager;
        private Coroutine _shakeCoroutine;
        private Vector3 _originalPosition;
        private bool _isInitialized;
        
        public int CurrentHp => currentHp;
        public BlockType Type => blockType;
        public virtual bool CausesGameOver => true;

        public virtual void Initialize(int hp, BlockManager manager)
        {
            maxHp = hp;
            currentHp = hp;
            _blockManager = manager;
            
            _originalPosition = transform.localPosition;
            _isInitialized = true;
            
            UpdateHpText();
        }
        
        public void SyncBaselinePosition()
        {
            if (_shakeCoroutine != null)
            {
                StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = null;
            }
            
            _originalPosition = transform.localPosition;
        }

        public virtual int TakeDamage(int damage)
        {
            int actualDamage = Math.Min(currentHp, damage);
            currentHp -= actualDamage;

            UpdateHpText();
            
            if (gameObject.activeInHierarchy && _isInitialized)
            {
                if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
                _shakeCoroutine = StartCoroutine(ShakeRoutine());
            }
            
            if (currentHp <= 0)
            {
                ForceDestroy(); // 중복 코드 제거를 위해 기존 하단에 있던 로직 재활용
            }
            return actualDamage;
        }

        private IEnumerator ShakeRoutine()
        {
            float elapsed = 0f;

            while (elapsed < shakeDuration)
            {
                float xOffset = UnityEngine.Random.Range(-1f, 1f) * shakeMagnitude;
                transform.localPosition = new Vector3(_originalPosition.x + xOffset, _originalPosition.y, _originalPosition.z);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            transform.localPosition = _originalPosition;
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
            if (_shakeCoroutine != null) StopCoroutine(_shakeCoroutine);
            transform.localPosition = _originalPosition;

            if(_blockManager != null) _blockManager.ReturnBlockToPool(gameObject);
            else gameObject.SetActive(false);
        }
    }
}
