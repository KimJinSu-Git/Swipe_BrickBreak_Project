using System;
using Bird.InGame;
using UnityEngine;

namespace Bird.Ball
{
    public class BallController : MonoBehaviour
    {
        private BlockManager blockManager;
        private IAttackBehaviour currentAttackBehavior;
        
        public void SetBlockManager(BlockManager manager) => blockManager = manager;

        public void SetAttackBehavior(IAttackBehaviour behaviour) => currentAttackBehavior = behaviour;

        private void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Block"))
            {
                Vector2 hitPosition = collision.transform.position;
                
                Vector2Int gridIndex = blockManager.GetGridIndex(hitPosition);
                
                currentAttackBehavior.ExecuteAttack(gridIndex, blockManager);
            }
        }
    }
}
