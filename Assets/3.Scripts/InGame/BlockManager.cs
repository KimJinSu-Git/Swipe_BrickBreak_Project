using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bird.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Bird.InGame
{
    public class BlockManager : MonoBehaviour
    {
        [Header("Managers")]
        [SerializeField] private ScoreManager scoreManager;
        
        [Header("Grid Settings")] 
        [SerializeField] private int maxRows = 7; // 세로 개수
        [SerializeField] private int maxColumns = 7; // 가로 개수
        
        [Header("Coordinate Settings")]
        [SerializeField] private Vector2 topLeftPosition = new Vector2(-1.8f, 2.75f);
        [SerializeField] private Vector2 bottomRightPosition = new Vector2(1.8f, -2.75f);

        [Header("Polling Settings")]
        [SerializeField] private GameObject blockPrefab;
        [SerializeField] private int initialPoolSize = 30;

        private float cellSpacingX;
        private float cellSpacingY;

        private Queue<GameObject> blockPool = new Queue<GameObject>();
        private GameObject[,] blockGrid;
        
        public bool IsGameOverFlag { get; private set; }

        private void Awake()
        {
            CalculateCellSpacing();
            InitializeGrid();
            InitializePool();
        }
        
        /// <summary>
        /// 설정된 좌상단/우하단 좌표를 바탕으로 블록 간의 정확한 간격을 자동 계산합니다.
        /// </summary>
        private void CalculateCellSpacing()
        {
            // 칸과 칸 사이의 틈 개수는 전체 칸 수보다 1개 적습니다.
            cellSpacingX = (bottomRightPosition.x - topLeftPosition.x) / (maxColumns - 1);
            cellSpacingY = (topLeftPosition.y - bottomRightPosition.y) / (maxRows - 1);
        }

        /// <summary>
        /// 게임 시작 시 한 번 실행됩니다.
        /// </summary>
        private void InitializeGrid() => blockGrid = new GameObject[maxRows, maxColumns];

        private void InitializePool()
        {
            for (int i = 0; i < initialPoolSize; i++)
            {
                GameObject newBlock = Instantiate(blockPrefab, transform);
                newBlock.SetActive(false);
                blockPool.Enqueue(newBlock);
            }
        }

        public void SpawnTestBlock(int row, int col, int hp)
        {
            GameObject blockObj = GetBlockFormPool();
            
            blockObj.transform.position = GetWorldPosition(row, col);
            
            if (blockObj.TryGetComponent(out Block blockComponent))
            {
                blockComponent.Initialize(hp);
            }
            
            blockGrid[row, col] = blockObj;
        }

        public void DamageBlock(Vector2Int gridIndex, int damage)
        {
            if (gridIndex.x < 0 || gridIndex.x >= maxColumns || gridIndex.y < 0 || gridIndex.y >= maxRows) return;
            
            GameObject targetBlockObj = blockGrid[gridIndex.y, gridIndex.x];
            if (targetBlockObj == null || !targetBlockObj.activeInHierarchy) return;
            
            if (targetBlockObj.TryGetComponent(out Block targetBlock))
            {
                int earnedScore = targetBlock.TakeDamage(damage);
                bool isDestroyed = targetBlock.CurrentHp <= 0;

                if (isDestroyed)
                {
                    blockGrid[gridIndex.y, gridIndex.x] = null;
                }

                if (scoreManager != null)
                {
                    scoreManager.AddScore(earnedScore, isDestroyed);
                }
            }
        }
        
        // -- Object Pool 로직 --
        public GameObject GetBlockFormPool()
        {
            GameObject block = blockPool.Count > 0 ? blockPool.Dequeue() : Instantiate(blockPrefab, transform);
            block.SetActive(true);
            return block;
        }

        public void ReturnBlockToPool(GameObject block)
        {
            block.SetActive(false);
            blockPool.Enqueue(block);
        }
        
        // -- Async 기반 블록 하강 로직 --
        /// <summary>
        /// TurnEnd 상태일 때 호출되며, 블록 하강 로직 처리 후 대기 시간으 ㄹ가집니다.
        /// </summary>
        public async Task MoveBlocksDownAsync()
        {
            ShiftGridDataDown();
            SpawnNewRow();

            await Task.Delay(300);
            Debug.Log("블록 하강 및 신규 스폰 완료");
        }

        private void ShiftGridDataDown()
        {
            IsGameOverFlag = false;
            for (int row = maxRows - 2; row >= 0; row--)
            {
                for (int col = 0; col < maxColumns; col++)
                {
                    GameObject block = blockGrid[row, col];
                    if (block != null)
                    {
                        // 다음 칸(row + 1)이 맨 바닥(데드라인)이라면 게임 오버 플래그 작동
                        if (row + 1 == maxRows - 1) IsGameOverFlag = true; 

                        // 배열 데이터 이동
                        blockGrid[row + 1, col] = block;
                        blockGrid[row, col] = null;

                        // World 좌표 이동
                        block.transform.position = GetWorldPosition(row + 1, col);
                    }
                }
            }
        }

        private void SpawnNewRow()
        {
            // 맨 윗줄에 임시로 블록을 무작위 생성합니다.
            // TODO :: 추후 DifficultyData를 연동하여 체력 효과 적용
            for (int col = 0; col < maxColumns; col++)
            {
                if (Random.value < 1f)
                {
                    SpawnTestBlock(0, col, 10);
                }
            }
        }

        /// <summary>
        /// 논리적인 2D Grid 인덱스를 Unity World 좌표(Transform)로 변환합니다.
        /// </summary>
        public Vector2 GetWorldPosition(int row, int col)
        {
            float xPos = topLeftPosition.x + (col * cellSpacingX);
            float yPos = topLeftPosition.y - (row * cellSpacingY);
            return new Vector2(xPos, yPos);
        }

        /// <summary>
        /// 무거운 물리 연산을 대체하기 위해 World 좌표를 Grid 인덱스로 역산합니다.
        /// </summary>
        public Vector2Int GetGridIndex(Vector2 worldPosition)
        {
            int col = Mathf.RoundToInt((worldPosition.x - topLeftPosition.x) / cellSpacingX);
            int row = Mathf.RoundToInt((topLeftPosition.y - worldPosition.y) / cellSpacingY);

            col = Mathf.Clamp(col, 0, maxColumns - 1);
            row = Mathf.Clamp(row, 0, maxRows - 1);
            
            return new Vector2Int(col, row);
        }
    }
}
