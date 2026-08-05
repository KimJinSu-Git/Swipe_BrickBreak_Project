using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bird.InGame;
using UnityEngine;

namespace Bird.Ball
{
    public class BallManager : MonoBehaviour
    {
        [Header("Pool Settings")] 
        [SerializeField] private GameObject ballPrefab;
        [SerializeField] private int initialBallCount = 50;
        [SerializeField] private BlockManager blockManager;

        [Header("Shooting Settings")] 
        [SerializeField] private int currentBallCount = 3;
        [SerializeField] private float ballSpeed = 10f;
        [SerializeField] private int delayBetweenBallsMs = 100;
        
        private Queue<GameObject> ballPool = new Queue<GameObject>();
        private List<GameObject> activeBalls =  new List<GameObject>();
        
        private Transform managerTransform;

        public event Action OnAllBallsReturned;

        private void Awake()
        {
            managerTransform = transform;
            InitializePool();
        }

        private void InitializePool()
        {
            for (int i = 0; i < initialBallCount; i++)
            {
                GameObject newBall = Instantiate(ballPrefab, managerTransform);

                if (newBall.TryGetComponent(out BallController controller))
                {
                    controller.SetBlockManager(blockManager);
                }
                
                newBall.SetActive(false);
                ballPool.Enqueue(newBall);
            }
        }

        public GameObject GetBall()
        {
            GameObject ball = ballPool.Count > 0 ? ballPool.Dequeue() : Instantiate(ballPrefab, managerTransform);
            ball.SetActive(true);
            return ball;
        }

        public void ReturnBall(GameObject ball)
        {
            ball.SetActive(false);
            ballPool.Enqueue(ball);
        }

        public async Task FireBallsAsync(Vector2 spawnPosition, Vector2 direction)
        {
            activeBalls.Clear();
            
            for (int i = 0; i < currentBallCount; i++)
            {
                GameObject ball = GetBall();
                ball.transform.position = spawnPosition;
                
                activeBalls.Add(ball);
                
                if (ball.TryGetComponent(out Rigidbody2D rb))
                {
                    rb.linearVelocity = direction * ballSpeed;
                }

                await Task.Delay(delayBetweenBallsMs);
            }
            
            Debug.Log($"{currentBallCount} 개의 공 발사 완료");
        }

        /// <summary>
        /// 바닥에 닿은 공을 하나씩 회수합니다.
        /// </summary>
        public void RetrieveBall(GameObject ball)
        {
            if (!activeBalls.Contains(ball)) return;

            ReturnBall(ball);
            activeBalls.Remove(ball);
            
            if (activeBalls.Count == 0) OnAllBallsReturned?.Invoke();
        }

        /// <summary>
        /// 회수 버튼 클릭 시 호출되어 모든 공을 즉시 강제 회수합니다.
        /// </summary>
        public void ForceRetrieveAllActiveBalls()
        {
            foreach (var ball in activeBalls)
            {
                ReturnBall(ball);
            }
            activeBalls.Clear();
            
            OnAllBallsReturned?.Invoke();
        }
    }
}
