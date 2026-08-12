using System.Collections.Generic;
using Bird.Ball;
using Bird.Core;
using UnityEngine;

namespace Bird.OutGame
{
    public class GachaManager : MonoBehaviour
    {
        [Header("Manager Settings")]
        [SerializeField] private GachaData gachaData;
        [SerializeField] private CoinManager coinManager;
        [SerializeField] private BallManager ballManager;
        [SerializeField] private TurnManager turnManager;

        [Header("Pity System")] 
        [SerializeField] private int pullCount = 0;

        public event System.Action<List<BallType>> OnGachaCompleted;
        
        /// <summary>
        /// 1회 뽑기 (50 코인)
        /// </summary>
        public void PullSingle()
        {
            if (turnManager != null && turnManager.CurrentState != GameState.Idle)
            {
                Debug.LogWarning("현재 대기(Idle) 상태가 아니므로 가챠를 진행할 수 없습니다!");
                return;
            }
            
            if (coinManager.TrySpendCoins(gachaData.singlePullCost))
            {
                BallType result = ExecuteGacha();

                if (ballManager != null)
                {
                    ballManager.AddBallToDeck(result);
                    ballManager.ResetBallCountUI();
                }
                
                OnGachaCompleted?.Invoke(new List<BallType> { result });
            }
            else
            {
                Debug.LogWarning("코인이 부족합니다.");
            }
        }

        /// <summary>
        /// 5회 뽑기 (200 코인)
        /// </summary>
        public void PullFive()
        {
            if (turnManager != null && turnManager.CurrentState != GameState.Idle)
            {
                Debug.LogWarning("현재 대기(Idle) 상태가 아니므로 가챠를 진행할 수 없습니다!");
                return;
            }
            
            if (coinManager.TrySpendCoins(gachaData.fivePullCost))
            {
                List<BallType> results = new List<BallType>();
                
                for (int i = 0; i < 5; i++)
                {
                    BallType result = ExecuteGacha();

                    if (ballManager != null)
                    {
                        ballManager.AddBallToDeck(result);
                        ballManager.ResetBallCountUI();
                    }
                    
                    results.Add(result);
                }
                
                OnGachaCompleted?.Invoke(results);
            }
            else
            {
                Debug.LogWarning("코인이 부족합니다.");
            }
        }

        /// <summary>
        /// 천장 시스템을 확인합니다.
        /// </summary>
        private BallType ExecuteGacha()
        {
            pullCount++;

            if (pullCount % 5 == 0)
            {
                Debug.Log("천장 도달 ! 확정 특수 공 지급");
                return GetRandomBall(gachaData.pityRates);
            }
            else
            {
                return GetRandomBall(gachaData.normalRates);
            }
        }

        /// <summary>
        /// 가중치 랜덤 알고리즘입니다.
        /// </summary>
        private BallType GetRandomBall(List<GachaRate> rates)
        {
            float totalWeight = 0;

            foreach (var rate in rates) totalWeight += rate.weight;
            
            float randomValue = Random.Range(0, totalWeight);

            foreach (var rate in rates)
            {
                randomValue -= rate.weight;
                if (randomValue <= 0)
                {
                    return rate.ballType;
                }
            }

            return BallType.Normal;
        }
    }
}
