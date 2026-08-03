using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Bird.InGame
{
    public class BlockManager : MonoBehaviour
    {
        [Header("Grid Settings")] 
        [SerializeField] private int maxRows = 10;
        [SerializeField] private int maxColumns = 7;
        [SerializeField] private float cellSize = 1.0f;

        [Header("Polling Settings")]
        [SerializeField] private GameObject blockPrefab;
        [SerializeField] private int initialPoolSize = 30;

        private Queue<GameObject> blockPool = new Queue<GameObject>();
        private GameObject[,] blockGrid;

        private void Awake()
        {
            InitializeGrid();
            InitializePool();
        }

        private void Start()
        {
            SpawnTestBlock(3, 0, 10);
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
            
            // 현재는 테스트를 위해 GetComponent를 사용합니다.
            Block targetBlock = targetBlockObj.GetComponent<Block>();
            
            if (targetBlock != null)
            {
                int earnedScore = targetBlock.TakeDamage(damage);
                // TODO: ScoreManager에 earnedScore 전달하여 점수 증가 처리
                Debug.Log($"인덱스 [{gridIndex.x}, {gridIndex.y}] 타격! 획득 점수: {earnedScore}");
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
        public async Task MoveBlocksDownAsync()
        {
            ShiftGridDataDown();

            await Task.Delay(300);
            
            Debug.Log("블록 하강 및 신규 스폰 완료");
        }

        private void ShiftGridDataDown()
        {
            // TODO :: 2차원 배열(blockGrid)의 데이터를 밑으로 한 칸씩 이동시키고, 바닥에 닿은 블록이 있는지 검사하는 로직 추가 예정
        }
        
        /// <summary>
        /// 논리적인 2D Grid 인덱스를 Unity World 좌표(Transform)로 변환합니다.
        /// 표현식 본문 멤버(=>)를 사용하여 코드를 간결하게 유지합니다.
        /// </summary>
        public Vector2 GetWorldPosition(int row, int col) => new Vector2(col * cellSize, -row * cellSize);

        /// <summary>
        /// 공이 특정 위치에 도달했을 때, 해당 World 좌표를 Grid 인덱스로 역산합니다.
        /// 이 메서드를 통해 Physics2D.Overlap 등 무거운 물리 연산을 대체합니다.
        /// </summary>
        public Vector2Int GetGridIndex(Vector2 worldPosition)
        {
            int col = Mathf.RoundToInt(worldPosition.x / cellSize);
            int row = Mathf.RoundToInt(-worldPosition.y / cellSize);

            col = Mathf.Clamp(col, 0, maxColumns - 1);
            row = Mathf.Clamp(row, 0, maxRows - 1);
            
            return new Vector2Int(col, row);
        }
    }
}
