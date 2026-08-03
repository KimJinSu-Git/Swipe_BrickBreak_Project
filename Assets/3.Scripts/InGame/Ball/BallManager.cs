using System;
using System.Collections.Generic;
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
        
        private Queue<GameObject> ballPool = new Queue<GameObject>();
        
        private Transform managerTransform;

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
    }
}
