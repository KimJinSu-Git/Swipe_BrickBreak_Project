using System;
using Bird.InGame;
using UnityEngine;

namespace Bird.Ball
{
    public class BallController : MonoBehaviour
    {
        private BlockManager _blockManager;
        private IAttackBehaviour _currentAttackBehavior;
        
        public void SetBlockManager(BlockManager manager) => _blockManager = manager;

        public void SetAttackBehavior(IAttackBehaviour behaviour) => _currentAttackBehavior = behaviour;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Block"))
            {
                Vector2 hitPosition = collision.transform.position;
                
                Vector2Int gridIndex = _blockManager.GetGridIndex(hitPosition);
                
                _currentAttackBehavior.ExecuteAttack(gridIndex, _blockManager);
            }
        }
    }
}
