using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bird.Ball;
using Bird.Data;
using Bird.InGame;
using UnityEngine;
using UnityEngine.SceneManagement;


namespace Bird.Core
{
    public class GameManager : MonoBehaviour
    {
        [Header("System Managers")]
        [SerializeField] private SaveManager saveManager;
        
        [Header("Data Providers (Managers)")]
        [SerializeField] private TurnManager turnManager; // 턴 정보
        [SerializeField] private BallManager ballManager; // 플레이어 덱 정보
        [SerializeField] private BlockManager blockManager; // 블록 정보
        [SerializeField] private ScoreManager scoreManager; // 점수 정보
        [SerializeField] private CoinManager coinManager; // 코인 정보
        [SerializeField] private SkillManager skillManager; // 스킬 게이지 정보

        private void Awake()
        {
            // 수직 동기화 끄기
            QualitySettings.vSyncCount = 0;
            
            // 프레임 60으로 고정 (유니티 모바일 기본값인 30을 해제)
            Application.targetFrameRate = 60;
        }

        private async void Start()
        {
            await ResumeGameAsync();
            
            if (turnManager != null)
            {
                turnManager.OnGameStateChanged += HandleGameStateChanged;
                turnManager.OnGameOver += HandleGameOver;
            }
        }
        
        private void OnDestroy()
        {
            if (turnManager != null)
            {
                turnManager.OnGameStateChanged -= HandleGameStateChanged;
                turnManager.OnGameOver -= HandleGameOver;
            }
        }
        
        private void HandleGameStateChanged(GameState newState)
        {
            if (newState == GameState.Idle)
            {
                Debug.Log("[GameManager] 새 턴 시작 (Idle) -> 체크포인트 자동 저장");
                SaveCurrentGame();
            }
        }
        
        private void HandleGameOver()
        {
            Debug.Log("[GameManager] 게임 오버 감지 -> 세이브 파일 영구 삭제");
            if (saveManager != null)
            {
                saveManager.DeleteSaveData();
            }
        }
        
        public void RestartGame()
        {
            Debug.Log("[GameManager] 게임 재시작 로직 가동: 세이브 파일 삭제 및 씬 재로드");
            
            if (saveManager != null) saveManager.DeleteSaveData();

            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        
        private async Task ResumeGameAsync()
        {
            if (saveManager == null) return;

            // JSON 파일을 읽어 역직렬화
            SaveData loadedData = await saveManager.LoadGameAsync();

            // 만약 새 게임이라면 복구 로직을 건너뜁니다
            if (loadedData.currentTurn <= 1 && loadedData.score == 0)
            {
                Debug.Log("[GameManager] 새로 시작된 게임입니다. (복구 건너뜀)");
                return;
            }

            Debug.Log($"[GameManager] 세이브 파일 발견! {loadedData.currentTurn} 턴부터 복구를 시작합니다.");

            // 각 매니저에게 과거 데이터를 주입하여 상태 덮어쓰기
            if (turnManager != null) turnManager.RestoreTurn(loadedData.currentTurn);
            if (scoreManager != null) scoreManager.RestoreScore(loadedData.score);
            
            // Skill, Coin 등도 Restore 메서드를 구현했다면 여기에 연결합니다.
            if (coinManager != null) coinManager.RestoreCoin(loadedData.coin);
            // if (skillManager != null) skillManager.RestoreGauge(loadedData.skillGauge);

            // 리스트 및 배열 복구
            if (ballManager != null) ballManager.RestoreDeck(loadedData.playerDeck);
            if (blockManager != null) blockManager.RestoreBoardState(loadedData.boardState);

            Debug.Log("[GameManager] 게임 복구(Resume)가 완벽하게 완료되었습니다!");
        }
        
        public void SaveCurrentGame()
        {
            if (saveManager == null) return;

            if (turnManager != null && (turnManager.CurrentState == GameState.Shooting || turnManager.CurrentState == GameState.TurnEnd) || blockManager.IsGameOverFlag)
            {
                Debug.LogWarning("[GameManager] 어뷰징 방지: 턴 진행 중에는 진행 상황을 저장하지 않습니다.");
                return;
            }
            
            SaveData data = new SaveData();

            // 기본 진행 정보 및 재화/점수 수집
            if (turnManager != null) data.currentTurn = turnManager.CurrentTurn;
            if (scoreManager != null) data.score = scoreManager.CurrentScore;
            if (coinManager != null) data.coin = coinManager.CurrentCoins;
            if (skillManager != null) data.skillGauge = skillManager.CurrentGauge;
            
            // 보유 공 리스트 수집
            if (ballManager != null) data.playerDeck = new List<BallType>(ballManager.PlayerDeck);

            // 현재 블록 상태 수집
            if (blockManager != null) data.boardState = blockManager.GetBoardSaveData();

            // SaveManager에게 백그라운드 비동기 저장을 지시
            _ = saveManager.SaveGameAsync(data);
        }
        
        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Debug.Log("[GameManager] 앱 백그라운드 전환 감지 -> 자동 저장 실행");
                SaveCurrentGame();
            }
        }
        
        private void OnApplicationQuit()
        {
            Debug.Log("[GameManager] 앱 종료 감지 -> 자동 저장 실행");
            SaveCurrentGame();
        }
    }
}

