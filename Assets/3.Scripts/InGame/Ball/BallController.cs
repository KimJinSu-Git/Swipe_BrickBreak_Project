using Bird.Core;
using Bird.InGame;
using UnityEngine;

namespace Bird.Ball
{
    public enum BallType { Normal, Explosion, Cross, Laser }
    public class BallController : MonoBehaviour
    {
        [Header("Components")] 
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        private BlockManager _blockManager;
        private VFXManager _vfxManager;
        private IAttackBehaviour _currentAttackBehavior;
        private BallType _ballType;
        
        public void SetBlockManager(BlockManager manager) => _blockManager = manager;
        public void SetVFXManager(VFXManager manager) => _vfxManager = manager;
        public void SetAttackBehavior(IAttackBehaviour behaviour) => _currentAttackBehavior = behaviour;

        public void InitializeVisual(BallType type, Sprite ballSprite, Color tintColor)
        {
            _ballType = type;
            if (spriteRenderer != null)
            {
                if (ballSprite != null) spriteRenderer.sprite = ballSprite;
                
                spriteRenderer.color = tintColor; 
            }
        }

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Block"))
            {
                Vector2 hitPosition = collision.transform.position;
                
                Vector2Int gridIndex = _blockManager.GetGridIndex(hitPosition);
                
                _currentAttackBehavior.ExecuteAttack(gridIndex, _blockManager, _vfxManager);
            }
        }
    }
}
