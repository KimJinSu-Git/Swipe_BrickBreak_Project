using System;
using Bird.Ball;
using Bird.InGame;
using UnityEngine;

namespace Bird.Core
{
    public enum GameState
    {
        Idle,
        Aiming,
        Shooting,
        TurnEnd,
        GameOverCheck
    }
    
    public class TurnManager : MonoBehaviour
    {
        [SerializeField] private GameState currentState;
        [SerializeField] private TrajectoryRenderer trajectoryRenderer;
        [SerializeField] private BallManager ballManager;
        [SerializeField] private BlockManager blockManager;

        private Vector2 dragStartPosition;
        private Vector2 currentAimDirection;

        private Vector2 ballSpawnPosition = new Vector2(0f, -3f);
        public GameState CurrentState => currentState;

        private void Awake() => ChangeState(GameState.Idle);

        private void Start()
        {
            ballManager.OnAllBallsReturned += OnTurnEndReady;
        }

        private void OnDestroy()
        {
            if (ballManager != null) ballManager.OnAllBallsReturned -= OnTurnEndReady;
        }
        
        /// <summary>
        /// ReturnZone에서 호출하여 다음 턴의 발사 위치(X좌표)를 갱신합니다.
        /// </summary>
        public void UpdateSpawnPositionX(float newX) => ballSpawnPosition.x = newX;
        
        private void OnTurnEndReady()
        {
            if (currentState == GameState.Shooting)
            {
                ChangeState(GameState.TurnEnd);
            }
        }

        /// <summary>
        /// 상태를 안전하게 전환하고, 진입 시 필요한 1회성 로직을 실행합니다.
        /// </summary>
        public void ChangeState(GameState newState)
        {
            currentState = newState;

            switch (currentState)
            {
                case GameState.Idle:
                    OnEnterIdle();
                    break;
                case GameState.Aiming:
                    OnEnterAiming();
                    break;
                case GameState.Shooting:
                    OnEnterShooting();
                    break;
                case GameState.TurnEnd:
                    OnEnterTurnEnd();
                    break;
                case GameState.GameOverCheck:
                    OnEnterGameOverCheck();
                    break;
            }
        }
        
        private void OnEnterIdle() => Debug.Log("대기 상태: 스와이프 및 스킬 사용 가능");
        private void OnEnterAiming() => Debug.Log("조준 상태: LineRenderer 예상 궤적 표시 시작");

        private void OnEnterShooting()
        {
            trajectoryRenderer.HideLine();
            Debug.Log("발사 상태: 공 발사! (조작 불가)");
            
            // BallManager의 비동기 순차 발사 메서드
            // 반환되는 Task를 따로 기다리지(await) 않고 Fire & Forget 방식으로 실행합니다.
            _ = ballManager.FireBallsAsync(ballSpawnPosition, currentAimDirection);
        }

        private async void OnEnterTurnEnd()
        {
            Debug.Log("턴 종료: 공 회수 확인 및 블록 하강");
            
            await blockManager.MoveBlocksDownAsync();
            
            ChangeState(GameState.GameOverCheck);
        }
        private void OnEnterGameOverCheck()
        {
            Debug.Log("게임 오버 체크: 데드라인 도달 여부 확인");
            
            // 블록이 바닥에 닿았는지 검사합니다.[cite: 8]
            if (blockManager.IsGameOverFlag)
            {
                Debug.Log("게임 오버!");
                // TODO: 팝업 UI 재시작 버튼 연동
            }
            else
            {
                ChangeState(GameState.Idle);
            }
        }

        private void Update()
        {
            if (currentState == GameState.Idle)
            {
                CheckDragStart();
            }
            else if (currentState == GameState.Aiming)
            {
                UpdateAiming();
            }
        }
        
        private void CheckDragStart()
        {
            if (Input.GetMouseButtonDown(0))
            {
                dragStartPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                ChangeState(GameState.Aiming);
            }
        }

        private void UpdateAiming()
        {
            if (Input.GetMouseButton(0))
            {
                Vector2 currentDragPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                
                // 드래그 벡터 계산 (시작점 - 현재점 = 당긴 반대 방향)
                Vector2 direction = dragStartPosition - currentDragPosition;
                // ⚠터치 직후 0으로 나누기가 발생하는 것을 방지하는 방어 코드
                if (direction.sqrMagnitude < 0.01f) return; 
                direction.Normalize();

                // 각도 변환 및 제한 (Clamp)
                // Mathf.Atan2를 사용하여 현재 방향을 각도(-180도 ~ 180도)로 변환합니다.
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                // 공이 아래로 향하거나(음수 각도), 너무 평행하게 누워서 무한 바운스 되는 것을 방지합니다.
                // 15도 ~ 165도 사이로 각도를 통제합니다.
                angle = Mathf.Clamp(angle, 15f, 165f);
                
                // 통제된 각도를 다시 삼각함수를 통해 방향(Vector2)으로 조립합니다.
                currentAimDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                trajectoryRenderer.ShowLine();
                trajectoryRenderer.DrawTrajectory(ballSpawnPosition, currentAimDirection);
            }
            else if (Input.GetMouseButtonUp(0))
            {
                ChangeState(GameState.Shooting);
            }
        }
        
        /// <summary>
        /// UI 버튼의 OnClick 이벤트에 연결될 함수입니다.
        /// </summary>
        public void OnSkipButtonClicked()
        {
            if (currentState != GameState.Shooting) return;
            
            Debug.Log("회수 버튼 클릭: 모든 공 강제 회수!");
            ballManager.ForceRetrieveAllActiveBalls(); 
        }
    }
}
