using Bird.InGame;
using UnityEngine;

namespace Bird.Ball
{
    /// <summary>
    /// 모든 공이 공통적으로 가져야 할 공격 행동의 규격입니다.
    /// </summary>
    public interface IAttackBehaviour
    {
        void ExecuteAttack(Vector2Int hitGridIndex, BlockManager blockManager);
    }

    /// <summary>
    /// 단일 블록에 데미지 1을 입히는 기본 공의 로직입니다.
    /// </summary>
    public class NormalAttack : IAttackBehaviour
    {
        public void ExecuteAttack(Vector2Int hitGridIndex, BlockManager blockManager) => blockManager.DamageBlock(hitGridIndex, 1);
    }

    public class ExplosionAttack : IAttackBehaviour
    {
        public void ExecuteAttack(Vector2Int hitGridIndex, BlockManager blockManager)
        {
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    Vector2Int targetIndex = new Vector2Int(hitGridIndex.x + x, hitGridIndex.y + y);
                    blockManager.DamageBlock(targetIndex, 1);
                }
            }
        }
    }
}
