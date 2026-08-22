using System;
using System.IO;
using System.Threading.Tasks;
using Bird.Core;
using Bird.Data;
using Bird.InGame;
using Newtonsoft.Json;
using UnityEngine;

namespace Bird.OutGame
{
    public class StatisticsManager : MonoBehaviour
    {
        private const string STAT_FILE_NAME = "SwipeBrickStats.json";
        
        [Header("Broadcasters")]
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private CoinManager coinManager;
        [SerializeField] private BlockManager blockManager;
        [SerializeField] private ComboManager comboManager;
        
        // SaveData와 겹치지 않는 독립적인 통계 전용 경로를 사용합니다.
        private string StatFilePath => Path.Combine(Application.persistentDataPath, STAT_FILE_NAME);

        private StatisticsData _currentStats = new StatisticsData();

        private async void Start()
        {
            await LoadStatisticsAsync();
            AddPlayCount();
            
            if (turnManager != null) turnManager.OnTurnChanged += UpdateMaxTurn;
            if (scoreManager != null) scoreManager.OnScoreChanged += UpdateHighScore;
            if (coinManager != null) coinManager.OnCoinEarned += AddCoinEarned;
            
            if (blockManager != null)
            {
                blockManager.OnBlockDestroyed += AddBlockDestroyed;
                blockManager.OnDamageDealt += AddDamage;
            }
        }
        
        private void OnDestroy()
        {
            if (turnManager != null) turnManager.OnTurnChanged -= UpdateMaxTurn;
            if (scoreManager != null) scoreManager.OnScoreChanged -= UpdateHighScore;
            if (coinManager != null) coinManager.OnCoinEarned -= AddCoinEarned;
            
            if (blockManager != null)
            {
                blockManager.OnBlockDestroyed -= AddBlockDestroyed;
                blockManager.OnDamageDealt -= AddDamage;
            }
        }

        // --- 통계 갱신 로직 ---
        
        public void UpdateHighScore(int newScore)
        {
            if (newScore > _currentStats.highScore)
            {
                _currentStats.highScore = newScore;
                Debug.Log($"[Statistics] 최고 점수 갱신: {_currentStats.highScore}");
            }
        }

        public void UpdateMaxTurn(int newTurn)
        {
            if (newTurn > _currentStats.maxTurn) _currentStats.maxTurn = newTurn;
        }

        public void UpdateMaxCombo(int newCombo)
        {
            if (newCombo > _currentStats.maxCombo) _currentStats.maxCombo = newCombo;
        }

        public void AddDamage(int damageDealt) => _currentStats.totalDamageDealt += damageDealt;
        
        public void AddBlockDestroyed() => _currentStats.totalBlockDestroyed++;
        
        public void AddCoinEarned(int amount) => _currentStats.totalCoinEarned += amount;
        
        public void AddGachaCount() => _currentStats.totalGachaCount++;
        
        private void AddPlayCount()
        {
            _currentStats.totalPlayCount++;
            _ = SaveStatisticsAsync();
        }

        // --- 파일 I/O 로직 ---

        /// <summary>
        /// 백그라운드 스레드에서 통계 데이터를 JSON으로 저장합니다.
        /// </summary>
        public async Task SaveStatisticsAsync()
        {
            try
            {
                string json = JsonConvert.SerializeObject(_currentStats, Formatting.Indented);
                using StreamWriter writer = new StreamWriter(StatFilePath);
                await writer.WriteAsync(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[StatisticsManager] 통계 저장 실패: {e.Message}");
            }
        }

        private async Task LoadStatisticsAsync()
        {
            if (!File.Exists(StatFilePath)) return;

            try
            {
                using StreamReader reader = new StreamReader(StatFilePath);
                string json = await reader.ReadToEndAsync();
                _currentStats = JsonConvert.DeserializeObject<StatisticsData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError($"[StatisticsManager] 통계 불러오기 실패: {e.Message}");
            }
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus) _ = SaveStatisticsAsync();
        }

        private void OnApplicationQuit() => _ = SaveStatisticsAsync();
    }
}