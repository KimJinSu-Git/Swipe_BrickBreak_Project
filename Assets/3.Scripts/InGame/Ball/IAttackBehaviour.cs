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

    /// <summary>
    /// 폭발 공 : 충돌 블록 중심 3x3 범위에 광역 데미지를 가합니다.
    /// </summary>
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

    /// <summary>
    /// 십자 공 : 충돌 블록 기준 상, 하, 좌, 우 및 중심(5칸)을 타격합니다.
    /// </summary>
    public class CrossAttack : IAttackBehaviour
    {
        private readonly Vector2Int[] crossOffsets =
        {
            new Vector2Int(0, 0),
            new Vector2Int(0, -1),
            new Vector2Int(0, 1),
            new Vector2Int(-1, 0),
            new Vector2Int(1, 0)
        };
        
        public void ExecuteAttack(Vector2Int hitGridIndex, BlockManager blockManager)
        {
            foreach (Vector2Int offset in crossOffsets)
            {
                Vector2Int targetIndex = new Vector2Int(hitGridIndex.x + offset.x, hitGridIndex.y + offset.y);
                blockManager.DamageBlock(targetIndex, 1);
            }
        }
    }
    
    /// <summary>
    /// 레이저 공 : 충돌 위치 기준 가로 1줄 전체를 타격합니다.
    /// </summary>
    public class LaserAttack : IAttackBehaviour
    {
        public void ExecuteAttack(Vector2Int hitGridIndex, BlockManager blockManager)
        {
            for (int x = 0; x < blockManager.MaxColumns; x++)
            {
                Vector2Int targetIndex = new Vector2Int(x, hitGridIndex.y);
                blockManager.DamageBlock(targetIndex, 1);
            }
        }
    }
}
