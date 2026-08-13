using UnityEngine;

namespace Bird.InGame.SpecialBlocks
{
    public class RecoveryBlock : Block
    {
        [Header("Recovery Settings")]
        [SerializeField] private int recoveryAmount = 5; 

        public override void OnTurnEnd(BlockManager blockManager, Vector2Int gridIndex)
        {
            blockManager.HealAllBlocks(recoveryAmount);
            
            Debug.Log($"[회복 블록] 맵 전체 블록의 체력을 {recoveryAmount}만큼 회복시켰습니다!");
        }
    }
}