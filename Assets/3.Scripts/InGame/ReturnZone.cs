using UnityEngine;
using Bird.Ball;
using Bird.Core;

namespace Bird.InGame
{
    public class ReturnZone : MonoBehaviour
    {
        [SerializeField] private BallManager ballManager;
        [SerializeField] private TurnManager turnManager;

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Ball"))
            {
                // 공이 바닥에 닿은 X 위치를 다음 발사 지점으로 갱신
                turnManager.UpdateSpawnPositionX(other.transform.position.x);
                
                ballManager.RetrieveBall(other.gameObject);
            }
        }
    }
}