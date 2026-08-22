using System;
using Bird.Ball;
using Bird.InGame;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Bird.Core
{
    public enum GameState
    {
        Idle,
        Aiming,
        Shooting,
        TurnEnd,
        GameOverCheck,
        SkillTargeting
    }
    
    public class TurnManager : MonoBehaviour
    {
        [Header("Manager Settings")]
        [SerializeField] private BallManager ballManager;
        [SerializeField] private BlockManager blockManager;
        [SerializeField] private ComboManager comboManager;
        [SerializeField] private SkillManager skillManager;
        
        [SerializeField] private GameState currentState;
        [SerializeField] private TrajectoryRenderer trajectoryRenderer;
        
        [SerializeField] private int currentTurn = 1;
        
        [Header("Spawn Settings")]
        [SerializeField] private Vector2 ballSpawnPosition = new Vector2(0f, -3f);

        private Vector2 _dragStartPosition;
        private Vector2 _currentAimDirection;
        
        public event Action OnGameOver;
        public event Action<int> OnTurnChanged;
        public event Action<GameState> OnGameStateChanged;
        public GameState CurrentState => currentState;
        public int CurrentTurn => currentTurn;

        private void Awake() => ChangeState(GameState.Idle);

        private void Start()
        {
            ballManager.OnAllBallsReturned += OnTurnEndReady;
            OnTurnChanged?.Invoke(currentTurn);
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
            
            OnGameStateChanged?.Invoke(newState);

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
                case GameState.SkillTargeting:
                    Debug.Log("스킬 타겟팅 상태: 타격할 가로줄을 터치하세요!");
                    break;
            }
        }

        private void OnEnterIdle()
        {
            ballManager.ResetBallCountUI();
        }
        private void OnEnterAiming() => Debug.Log("조준 상태: LineRenderer 예상 궤적 표시 시작");

        private void OnEnterShooting()
        {
            trajectoryRenderer.HideLine();
            Debug.Log("발사 상태: 공 발사! (조작 불가)");
            
            // BallManager의 비동기 순차 발사 메서드
            // 반환되는 Task를 따로 기다리지(await) 않고 Fire & Forget 방식으로 실행합니다.
            _ = ballManager.FireBallsAsync(ballSpawnPosition, _currentAimDirection);
        }

        private async void OnEnterTurnEnd()
        {
            Debug.Log("턴 종료: 공 회수 확인 및 블록 하강");

            currentTurn++;
            OnTurnChanged?.Invoke(currentTurn);
            
            if(comboManager != null) comboManager.ApplyTurnEndRewards();
            
            blockManager.ExecuteTurnEndEffects();
            
            await blockManager.MoveBlocksDownAsync(currentTurn);
            
            ChangeState(GameState.GameOverCheck);
        }
        private void OnEnterGameOverCheck()
        {
            Debug.Log("게임 오버 체크: 데드라인 도달 여부 확인");
            
            // 블록이 바닥에 닿았는지 검사합니다.
            if (blockManager.IsGameOverFlag)
            {
                Debug.Log("게임 오버!");
                OnGameOver?.Invoke();
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
            else if (currentState == GameState.SkillTargeting)
            {
                CheckSkillTargetingInput(); 
            }
        }
        
        private void CheckDragStart()
        {
            if (IsPointerOverUI()) return;
            
            if (Input.GetMouseButtonDown(0))
            {
                _dragStartPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                ChangeState(GameState.Aiming);
            }
        }
        
        private void CheckSkillTargetingInput()
        {
            if (IsPointerOverUI()) return;

            if (Input.GetMouseButtonDown(0))
            {
                Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                Vector2Int gridIndex = blockManager.GetGridIndex(worldPos);

                skillManager.ExecuteLineStrike(gridIndex.y, currentTurn);
                ChangeState(GameState.Idle); 
            }
        }

        private bool IsPointerOverUI()
        {
            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                if (touch.phase == TouchPhase.Began)
                {
                    return EventSystem.current.IsPointerOverGameObject(touch.fingerId);
                }
            }
            
            return EventSystem.current.IsPointerOverGameObject();
        }

        private void UpdateAiming()
        {
            if (Input.GetMouseButton(0))
            {
                Vector2 currentDragPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                
                // 드래그 벡터 계산 (시작점 - 현재점 = 당긴 반대 방향)
                Vector2 direction = _dragStartPosition - currentDragPosition;
                
                if (direction.sqrMagnitude < 0.01f) 
                {
                    _currentAimDirection = Vector2.up; 
                }
                else
                {
                    direction.Normalize();

                    // 각도 변환 및 제한 (Clamp)
                    float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                    angle = Mathf.Clamp(angle, 15f, 165f);
                    
                    // 통제된 각도를 다시 방향(Vector2)으로 조립합니다
                    _currentAimDirection = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
                }
                
                trajectoryRenderer.ShowLine();
                trajectoryRenderer.DrawTrajectory(ballSpawnPosition, _currentAimDirection);
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
        
        public void OnSkillButtonClicked()
        {
            // 스킬은 무조건 대기(Idle) 상태일 때만 켤 수 있습니다
            if (currentState == GameState.Idle && skillManager.IsSkillReady)
            {
                ChangeState(GameState.SkillTargeting);
            }
        }
        
        public void RestoreTurn(int savedTurn)
        {
            currentTurn = savedTurn;
            OnTurnChanged?.Invoke(currentTurn);
        }
    }
}
