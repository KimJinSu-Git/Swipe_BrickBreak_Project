using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bird.InGame;
using Bird.OutGame;
using UnityEngine;
using VFXManager = Bird.Core.VFXManager;

namespace Bird.Ball
{
    public class BallManager : MonoBehaviour
    {
        [Serializable]
        public struct BallVisualMapping
        {
            public BallType type;
            public Sprite sprite;
            public Color ballColor;
        }
        
        [Header("Manager Settings")] 
        [SerializeField] private VFXManager vfxManager;
        
        [Header("Ball Visual Settings")]
        [SerializeField] private List<BallVisualMapping> ballVisualMappings;
        
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
        private Dictionary<BallType, BallVisualMapping> _ballVisualDict = new Dictionary<BallType, BallVisualMapping>();
        
        private Transform _managerTransform;

        private bool _isFiring = false;

        public event Action OnAllBallsReturned;
        public event Action<int> OnBallCountChanged;
        public List<BallType> PlayerDeck => playerDeck;

        private void Awake()
        {
            foreach (var mapping in ballVisualMappings)
            {
                _ballVisualDict[mapping.type] = mapping;
            }
            
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
                    controller.SetVFXManager(vfxManager);
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
                    SetupSpecialBall(ballController, ballType);
                }

                if (ball.TryGetComponent(out Rigidbody2D rb))
                {
                    rb.linearVelocity = direction * ballSpeed;
                }

                remainingBalls--;
                OnBallCountChanged?.Invoke(remainingBalls);
                
                await Task.Delay(delayBetweenBallsMs);
            }
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
        
        /// <summary>
        /// 공을 스폰하거나 풀에서 꺼낼 때 호출하여 공격 방식과 이미지를 주입합니다.
        /// </summary>
        public void SetupSpecialBall(BallController ball, BallType type)
        {
            if (_ballVisualDict.TryGetValue(type, out BallVisualMapping visualInfo))
            {
                ball.InitializeVisual(type, visualInfo.sprite, visualInfo.ballColor);
            }

            switch (type)
            {
                case BallType.Explosion:
                    ball.SetAttackBehavior(new ExplosionAttack());
                    break;
                case BallType.Cross:
                    ball.SetAttackBehavior(new CrossAttack());
                    break;
                case BallType.Laser:
                    ball.SetAttackBehavior(new LaserAttack());
                    break;
                case BallType.Normal:
                default:
                    ball.SetAttackBehavior(new NormalAttack());
                    break;
            }
        }
        
        public void RestoreDeck(List<BallType> savedDeck)
        {
            playerDeck.Clear();
            playerDeck.AddRange(savedDeck);
            ResetBallCountUI();
        }
    }
}
