using UnityEngine;

namespace Bird.InGame.SpecialBlocks
{
    public class MultiplyBlock : Block
    {
        public override void OnTurnEnd(BlockManager blockManager, Vector2Int gridIndex)
        {
            if (currentHp <= 1) return;

            bool isMultiplied = blockManager.TryMultiplyBlock(gridIndex, currentHp);

            if (isMultiplied)
            {
                int halfHp = currentHp / 2;
                TakeDamage(halfHp);
            }
        }
    }
}
