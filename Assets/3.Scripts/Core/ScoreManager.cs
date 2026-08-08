using System;
using TMPro;
using UnityEngine;

namespace Bird.Core
{
    public class ScoreManager : MonoBehaviour
    {
        [SerializeField] private int currentScore;
        [SerializeField] private TextMeshProUGUI currentScoreText;
        
        public event Action<int> OnScoreChanged;
        
        public int CurrentScore => currentScore;

        private void Start()
        {
            UpdateScore(currentScore);
        }

        private void UpdateScore(int score)
        {
            currentScoreText.text = "Score : " + score;
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
            UpdateScore(currentScore);
            
            OnScoreChanged?.Invoke(currentScore);
        }
    }
}

