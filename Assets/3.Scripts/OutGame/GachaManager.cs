using System.Collections.Generic;
using Bird.Ball;
using Bird.Core;
using UnityEngine;

namespace Bird.OutGame
{
    public class GachaManager : MonoBehaviour
    {
        [SerializeField] private GachaData gachaData;
        [SerializeField] private CoinManager coinManager;
        [SerializeField] private BallManager ballManager;

        [Header("Pity System")] 
        [SerializeField] private int pullCount = 0;

        /// <summary>
        /// 1회 뽑기 (50 코인)
        /// </summary>
        public void PullSingle()
        {
            if (coinManager.TrySpendCoins(gachaData.singlePullCost))
            {
                BallType result = ExecuteGacha();
                
                if(ballManager != null) ballManager.AddBallToDeck(result);
                
                Debug.Log($"1회 뽑기 성공! 결과: {result} (현재 누적 뽑기: {pullCount})");
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
            if (coinManager.TrySpendCoins(gachaData.fivePullCost))
            {
                Debug.Log("5회 뽑기 시작 !");
                for (int i = 0; i < 5; i++)
                {
                    BallType result = ExecuteGacha();
                    
                    if(ballManager != null) ballManager.AddBallToDeck(result);
                    
                    Debug.Log($"   [{i+1}번째] 결과: {result}");
                }
                // TODO :: 5개의 공을 팝업 UI에 표시
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
