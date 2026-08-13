using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bird.InGame;
using Bird.OutGame;
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
        [SerializeField] private int currentBallCount = 1;
        [SerializeField] private float ballSpeed = 10f;
        [SerializeField] private int delayBetweenBallsMs = 100;
        
        [Header("Player Deck")]
        [SerializeField] private List<BallType> playerDeck = new List<BallType>();
        
        private Queue<GameObject> _ballPool = new Queue<GameObject>();
        private List<GameObject> _activeBalls =  new List<GameObject>();
        
        private Transform _managerTransform;

        private bool _isFiring = false;

        public event Action OnAllBallsReturned;
        public event Action<int> OnBallCountChanged;

        private void Awake()
        {
            _managerTransform = transform;
            InitializePool();
        }

        private void Start()
        {
            if (playerDeck.Count == 0)
            {
                playerDeck.Add(BallType.Normal);
            }
        }
        
        public void ResetBallCountUI() => OnBallCountChanged?.Invoke(playerDeck.Count);

        private void InitializePool()
        {
            for (int i = 0; i < initialBallCount; i++)
            {
                GameObject newBall = Instantiate(ballPrefab, _managerTransform);

                if (newBall.TryGetComponent(out BallController controller))
                {
                    controller.SetBlockManager(blockManager);
                }
                
                newBall.SetActive(false);
                _ballPool.Enqueue(newBall);
            }
        }

        public GameObject GetBall()
        {
            GameObject ball = _ballPool.Count > 0 ? _ballPool.Dequeue() : Instantiate(ballPrefab, _managerTransform);
            ball.SetActive(true);
            return ball;
        }

        private void ReturnBall(GameObject ball)
        {
            ball.SetActive(false);
            _ballPool.Enqueue(ball);
        }
        
        public void AddBallToDeck(BallType newBall)
        {
            playerDeck.Add(newBall);
            Debug.Log($"덱에 추가됨: {newBall} (현재 총 공 개수: {playerDeck.Count})");
        }

        public async Task FireBallsAsync(Vector2 spawnPosition, Vector2 direction)
        {
            _activeBalls.Clear();
            _isFiring = true;
            
            int remainingBalls = playerDeck.Count;
            OnBallCountChanged?.Invoke(remainingBalls);

            foreach (BallType ballType in playerDeck)
            {
                if (!_isFiring) break;

                GameObject ball = GetBall();
                ball.transform.position = spawnPosition;
                _activeBalls.Add(ball);

                if (ball.TryGetComponent(out BallController ballController))
                {
                    IAttackBehaviour attackBehavior = GetAttackBehavior(ballType);
                    ballController.SetAttackBehavior(attackBehavior);
                }

                if (ball.TryGetComponent(out Rigidbody2D rb))
                {
                    rb.linearVelocity = direction * ballSpeed;
                }

                remainingBalls--;
                OnBallCountChanged?.Invoke(remainingBalls);
                
                await Task.Delay(delayBetweenBallsMs);
            }
            /*
            for (int i = 0; i < currentBallCount; i++)
            {
                if (!isFiring) break;
                
                GameObject ball = GetBall();
                ball.transform.position = spawnPosition;
                activeBalls.Add(ball);
                
                if (ball.TryGetComponent(out Rigidbody2D rb))
                {
                    rb.linearVelocity = direction * ballSpeed;
                }

                await Task.Delay(delayBetweenBallsMs);
            }
            */
            _isFiring = false;
            Debug.Log($"{currentBallCount} 개의 공 발사 완료");
        }

        private IAttackBehaviour GetAttackBehavior(BallType type) => type switch
        {
            BallType.Normal => new NormalAttack(),
            BallType.Explosion => new ExplosionAttack(),
            BallType.Cross => new CrossAttack(),
            BallType.Laser => new LaserAttack(),
            _ => new NormalAttack()
        };

        /// <summary>
        /// 바닥에 닿은 공을 하나씩 회수합니다.
        /// </summary>
        public void RetrieveBall(GameObject ball)
        {
            if (!_activeBalls.Contains(ball)) return;

            ReturnBall(ball);
            _activeBalls.Remove(ball);
            
            if (_activeBalls.Count == 0) OnAllBallsReturned?.Invoke();
        }

        /// <summary>
        /// 회수 버튼 클릭 시 호출되어 모든 공을 즉시 강제 회수합니다.
        /// </summary>
        public void ForceRetrieveAllActiveBalls()
        {
            _isFiring = false;
            
            foreach (var ball in _activeBalls)
            {
                ReturnBall(ball);
            }
            _activeBalls.Clear();
            
            OnAllBallsReturned?.Invoke();
        }
    }
}
