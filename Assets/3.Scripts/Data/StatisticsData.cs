using UnityEngine;

namespace Bird.Data
{
    public class StatisticsData
    {
        // 최고 기록 관련
        public int highScore = 0;
        public int maxTurn = 0;
        public int maxCombo = 0;

        // 누적 횟수 및 재화 관련
        public int totalPlayCount = 0;
        public int totalCoinEarned = 0;
        public int totalGachaCount = 0;

        // 누적 전투 데이터
        public int totalBlockDestroyed = 0;
        
        public long totalDamageDealt = 0; 
    }
}
