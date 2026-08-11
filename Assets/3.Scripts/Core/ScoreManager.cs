using System;
using TMPro;
using UnityEngine;

namespace Bird.Core
{
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] private int currentScore;
        
        public event Action<int> OnScoreChanged;
        
        public int CurrentScore => currentScore;

        private void Start()
        {
            OnScoreChanged?.Invoke(currentScore);
        }

        public void AddScore(int damage, bool isDestroyed)
        {
            int earnedScore = damage;

            if (isDestroyed)
            {
                int destoryBonus = 10;
                earnedScore += destoryBonus;
            }
            
            currentScore += earnedScore;
            
            OnScoreChanged?.Invoke(currentScore);
        }
    }
}

