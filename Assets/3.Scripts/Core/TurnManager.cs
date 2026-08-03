using System;
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

        public GameState CurrentState => currentState;

        private void Awake()
        {
            ChangeState(GameState.Idle);
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
        private void OnEnterShooting() => Debug.Log("발사 상태: 공 발사! (조작 불가)");
        private void OnEnterTurnEnd() => Debug.Log("🛠턴 종료: 공 회수 확인 및 블록 하강");
        private void OnEnterGameOverCheck() => Debug.Log("게임 오버 체크: 데드라인 도달 여부 확인");

        private void Update()
        {
            if (currentState == GameState.Aiming)
            {
                UpdateAiming();
            }
        }

        private void UpdateAiming()
        {
            // TODO :: 유저의 터치 입력을 받아 와 궤적을 업데이트하는 로직이 추가될 곳
        }
    }
}
