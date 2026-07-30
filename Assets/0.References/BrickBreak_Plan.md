# 🧱 스와이프 벽돌깨기 - 게임 기획서

> **프로젝트명:** Swipe Brick Breaker\
> **장르:** Casual Arcade Puzzle (Endless Mode)\
> **플랫폼:** Mobile (Android)\
> **핵심 타겟:** 직관적인 조작과 연쇄 충돌의 쾌감을 즐기는 모바일 유저

---

## 1. 프로젝트 목표
직관적인 스와이프 조작으로 다양한 공을 발사하여, 지속적으로 내려오는 블록을 파괴하고 최고 점수를 갱신하는 캐주얼 아케이드 게임이다.\
단순한 구현을 넘어 대규모 물리 연산 최적화, 객체지향 설계(OOP), 확장성 있는 아키텍처(디자인 패턴)를 구축하는 것을 목표로 한다.

## 2. 핵심 게임 흐름
게임의 흐름은 `TurnManager`의 FSM(유한상태기계)에 의해 통제된다.

| 상태 (State) | 설명 및 유저 액션 | 비고 및 예외 처리 |
| :--- | :--- | :--- |
| **Idle** | 조준 대기. 유저가 화면을 터치하여 스와이프 시작 | 액티브 스킬 사용 가능 상태 |
| **Aiming** | 유저의 스와이프 각도에 따라 예상 궤적(LineRenderer) 표시 | 터치 해제 시 `Shooting`으로 전환 |
| **Shooting** | 지정된 궤적으로 공들이 순차적 발사 및 물리 충돌 | ⚠️ **예외 처리:** 공이 무한히 튕기는 것을 방지하기 위해, 우측 하단에 **[회수 (Skip)]** 버튼 활성화. 클릭 시 모든 공 즉시 회수 후 `TurnEnd`로 강제 전환. |
| **TurnEnd** | 맵 상의 모든 공이 바닥(회수 지점)에 도달한 상태 | 살아남은 블록 1칸 하강, 최상단 신규 블록 스폰 |
| **GameOver Check** | 블록이 바닥(데드라인)에 도달했는지 검사 | 도달 시 게임 오버, 미도달 시 `Idle` 복귀 |

## 3. 게임 규칙
*   블록은 매 턴 한 칸씩 아래로 이동하며, 새로운 블록은 맨 위에서 생성된다.
*   블록이 하단 데드라인에 도달하면 게임이 종료된다.
*   플레이어는 공을 발사하여 블록의 내구도를 0으로 만들어 제거한다.
*   발사된 모든 공이 바닥으로 회수되어야 다음 턴으로 진행된다.
*   공은 마지막으로 바닥에 닿은 위치를 기준으로 다음 턴 발사 지점을 설정한다.

## 4. 블록 시스템
블록은 물리 엔진 부하를 줄이기 위해 **논리적 Grid(2D 배열)** 형태로 위치를 인덱싱하여 관리한다.

| 블록 종류 | 특징 및 기믹                                             | 엣지 케이스 (Edge Case) 방지 |
| :--- |:----------------------------------------------------| :--- |
| **일반 블록** | 표기된 HP만큼 피격 시 파괴                                    | - |
| **무적 블록** | 데미지를 받지 않고 공을 반사(장애물). 바닥 데드라인에 도달하면 파괴되며 게임오버 미유발  | 소프트 락 방지를 위해 스폰 시 1줄당 최대 1~2개로 개수 제한 |
| **회복 블록** | 턴 종료 시 살아있다면 맵 상의 모든 블록 HP를 `RecoveryAmount`만큼 회복   | - |
| **증식 블록** | 턴 종료 시 좌우 중 빈 공간이 있다면 현재 체력을 절반(예: 20 ➔ 10+10)으로 분열 | 무한 증식(갇힘) 방지를 위해 1줄당 최대 증식 개수 제한 적용. HP가 1이면 증식 불가. |

## 5. 난이도 시스템
게임이 진행될수록 생성되는 블록의 체력(HP)이 스케일링된다. 모든 수치는 하드코딩을 배제하고 `DifficultyData` (ScriptableObject)에서 관리한다.

| Turn 구간 | 초기 HP 범위                |
|:--------|:------------------------|
| 1 ~ 5   | 1 ~ 5                   |
| 6 ~ 20  | 5 ~ 50                  |
| 21 ~ 40 | 50 ~ 150                |
| 41 ~ 60 | 150 ~ 600               |
| 61+     | `DifficultyData` 설정값 적용 |

*추후 회복 블록 및 증식 블록의 등장 확률 또한 해당 데이터에서 관리한다.*\
*위 수치는 임시로 적용된 수치이며, 추후 난이도를 조정할 예정입니다.*

## 6. 공 시스템
*   **충돌 최적화:** 공(Ball)끼리는 절대 충돌하지 않는다. (Layer Collision Matrix 적용)
*   **패턴 적용:** 각 공의 공격 방식은 `IAttackBehavior` 인터페이스(Strategy Pattern)를 통해 구현한다.

| 공 종류 | 공격 범위 (Grid 인덱스 탐색) | 특징                    |
| :--- | :--- |:----------------------|
| **일반 공 (Normal)** | 충돌한 단일 블록 | 기본 데미지 1              |
| **폭발 공 (Explosion)** | 충돌 블록 중심 3x3 범위 (주변 1칸) | Grid 인덱스를 탐색하여 광역 데미지 |
| **십자 공 (Cross)** | 충돌 블록 기준 상하좌우 (십자 형태) | 주변 블록 타격              |
| **레이저 공 (Laser)** | 충돌 위치 기준 가로 1줄 전체 | 일반 십자공의 상위호환.         |

## 7. 액티브 스킬
*   블록 파괴 및 콤보 보너스로 `Skill Gauge`가 100% 충전되면 `Idle` 상태에서 사용 가능.
*   **스킬 발동 흐름:** 스킬 버튼 클릭 ➔ `SkillTargeting` 상태 진입 (시간 정지) ➔ 원하는 가로줄 화면 터치 ➔ 스킬 발동 ➔ `Idle` 복귀
*   **초기 스킬 (Line Strike):** 선택한 가로줄 전체에 고정 데미지(예: `1 + TurnBonus`) 부여.
*   모든 스킬 데이터는 `SkillData` (ScriptableObject)에서 관리.

## 8. 점수 시스템
*   점수는 블록을 파괴할 때가 아니라, **블록에게 가한 데미지(Damage)**를 기준으로 산정한다.
    *   `1 Damage = 1 Score`
*   블록 파괴 시 별도의 `Destroy Bonus`를 추가로 지급한다.
    *   총 획득 점수 = `(Damage Score) + (Destroy Bonus)`
*   이 방식을 통해 후반부 체력이 높은 블록을 때릴수록 자연스럽게 점수가 기하급수적으로 높아지는 쾌감을 제공한다.

## 9. 콤보 (Combo) 시스템
한 턴(Shooting 상태) 내에서 공이 블록과 충돌할 때마다 콤보 수치가 1씩 증가하며, 턴 종료 시 0으로 초기화된다.

| Combo | 획득 보너스              |
| :--- |:--------------------|
| 50 | 획득 Coin + 5% 증가     |
| 100 | 획득 Coin + 20% 증가    |
| 150 | 스킬 게이지 충전량 + 20% 증가 |

*   관련 밸런스 수치는 `ComboData` (ScriptableObject)에서 관리.

## 10. 재화 (Coin) 및 가챠 시스템
*   블록 파괴 시 인게임 재화인 **Coin**을 획득한다. (게임 오버 시 초기화되는 1회성 재화)
*   **UI 공통 규칙:** 구매, 가챠 결과 등 최종 승인 버튼 텍스트는 무조건 **"Confirm"**을 사용한다.
*   **가중치 랜덤 (Weighted Random):** 코인을 소모해 `GachaData`에 설정된 가중치에 따라 랜덤하게 공을 획득.
    *   (예: 일반 80%, 십자 10%, 폭발 10%)
*   **천장 시스템:** 가챠 누적 10회 도달 시, 다음 가챠는 무조건 특수 공(십자/폭발/레이저 중 1)을 확정 지급한다.

## 11. Save / Resume (진행 상태 저장)
모바일 환경 특성상 강제 종료(ApplicationQuit) 및 백그라운드 전환(ApplicationPause) 시 현재 진행 상태를 저장한다.
*   **라이브러리:** `Newtonsoft.Json` 사용.
*   **저장 위치:** 기기의 로컬 저장소 (`Application.persistentDataPath`).
*   **저장 데이터 목록:** 현재 Turn, Score, 보유 Coin, 현재 Board(Grid 2D 배열 인덱스/타입/HP), 보유 공 리스트, Skill Gauge, Combo 현황.
*   **자동 저장:** 비정상적인 접속 종료를 방지하기 위하여, 게임 도중 특정 시간마다 저장 로직을 실행시킨다.

## 12. 통계 시스템 (Statistics)
유저의 달성감을 위해 누적 기록을 로컬에 별도 저장한다.
*   최고 점수 (High Score) / 최고 도달 턴
*   총 플레이 횟수 / 총 획득 Coin / 총 가챠 횟수
*   총 파괴한 블록 수 / 총 가한 누적 Damage
*   역대 최고 Combo

## 13. 사용 예정 기술 스펙
핵심 기술 명세서.

*   **Design Pattern:** Model-View-Presenter (MVP UI 구조), Singleton (GameManager), Strategy (공 공격 로직 분리), Observer (이벤트 기반 스킬/점수 갱신), FSM (턴 상태 관리).
*   **Data Driven:** ScriptableObject (기획 데이터), Newtonsoft.Json (세이브 데이터).
*   **Optimization (성능 최적화):**
    *   Object Pool (공, 이펙트, 텍스트 팝업 재사용).
    *   논리적 Grid 기반 O(1) 범위 탐색 (Physics Overlap 대체).
    *   Addressables Asset System (동적 리소스/메모리 로딩 관리).
    *   Sprite Atlas (배칭을 통한 UI/인게임 Draw Call 최소화).

*위 기술들은 프로젝트를 진행함에 따라 변경될 수 있습니다*

## 14. 프로젝트 구조 (Architecture Directory)*

```text
GameManager (Entry Point)
│
├── Core
│   ├── TurnManager (FSM 기반 상태 제어)
│   ├── SaveManager (Newtonsoft.Json 데이터 관리)
│   └── ResourceManager (Addressables 메모리 로드)
│
├── In-Game
│   ├── BallManager (BallPool, AttackBehavior - Strategy)
│   ├── BlockManager (LogicalGrid 2D Array, BlockSpawner)
│   ├── SkillManager (Observer 구독, 타겟팅 상태 제어)
│   └── Score/ComboManager 
│
├── Out-Game
│   ├── GachaManager (가중치 랜덤, 천장 로직)
│   └── StatisticsManager (최고 기록 트래킹)
│
├── UI & Audio
│   ├── UIManager (MVP Pattern 기반 뷰 렌더링, "Confirm" 버튼 규격화)
│   └── AudioManager
│
└── ScriptableObjects (Data)
    ├── BallData / BlockData
    ├── DifficultyData / ComboData
    └── SkillData / GachaData