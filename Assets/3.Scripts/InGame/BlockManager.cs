using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bird.Core;
using Bird.Data;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Bird.InGame
{
    public class BlockManager : MonoBehaviour
    {
        [Serializable]
        public struct BlockPrefabMapping
        {
            public BlockType type;
            public GameObject prefab;
        }
        
        [Header("Managers")]
        [SerializeField] private ScoreManager scoreManager;
        [SerializeField] private ComboManager comboManager;
        [SerializeField] private SkillManager skillManager;
        [SerializeField] private VFXManager vfxManager;
        
        [Header("Grid Settings")] 
        [SerializeField] private int maxRows = 7; // 세로 개수
        [SerializeField] private int maxColumns = 7; // 가로 개수
        
        [Header("Coordinate Settings")]
        [SerializeField] private Vector2 topLeftPosition = new Vector2(-1.8f, 2.75f);
        [SerializeField] private Vector2 bottomRightPosition = new Vector2(1.8f, -2.75f);

        [Header("Polling Settings")]
        [SerializeField] private List<BlockPrefabMapping> blockPrefabs;
        [SerializeField] private int initialPoolSize = 10;
        
        [Header("Data")]
        [SerializeField] private DifficultyData difficultyData;

        [Header("Block Settings")] 
        [SerializeField] private int maxMultiplyPerRow = 1;

        private float _cellSpacingX;
        private float _cellSpacingY;

        private Dictionary<BlockType, Queue<GameObject>> _blockPools = new Dictionary<BlockType, Queue<GameObject>>();
        private GameObject[,] _blockGrid;
        private List<int> _availableColumns = new List<int>();
        private List<KeyValuePair<Block, Vector2Int>> _blocksToProcessCache = new List<KeyValuePair<Block, Vector2Int>>();
        
        public bool IsGameOverFlag { get; private set; }
        
        public int MaxColumns => maxColumns;
        public int MaxRows => maxRows;
        
        public event Action OnBlockDestroyed;
        public event Action<int> OnDamageDealt;

        private void Awake()
        {
            CalculateCellSpacing();
            InitializeGrid();
            InitializePool();

            for (int i = 1; i <= 3; i++)
            {
                ShiftGridDataDown();
                SpawnNewRow(i);
            }
            // _ = MoveBlocksDownAsync(1);
        }
        
        /// <summary>
        /// 설정된 좌상단/우하단 좌표를 바탕으로 블록 간의 정확한 간격을 자동 계산합니다.
        /// </summary>
        private void CalculateCellSpacing()
        {
            // 칸과 칸 사이의 틈 개수는 전체 칸 수보다 1개 적습니다.
            _cellSpacingX = (bottomRightPosition.x - topLeftPosition.x) / (maxColumns - 1);
            _cellSpacingY = (topLeftPosition.y - bottomRightPosition.y) / (maxRows - 1);
        }

        /// <summary>
        /// 게임 시작 시 한 번 실행됩니다.
        /// </summary>
        private void InitializeGrid() => _blockGrid = new GameObject[maxRows, maxColumns];

        private void InitializePool()
        {
            _blockPools.Clear();

            foreach (var mapping in blockPrefabs)
            {
                _blockPools[mapping.type] = new Queue<GameObject>();
                for (int i = 0; i < initialPoolSize; i++)
                {
                    GameObject newBlock = Instantiate(mapping.prefab, transform);
                    newBlock.SetActive(false);
                    _blockPools[mapping.type].Enqueue(newBlock);
                }
            }
        }
        
        // -- Block 관련 로직 --

        public void DamageBlock(Vector2Int gridIndex, int damage)
        {
            if (gridIndex.x < 0 || gridIndex.x >= maxColumns || gridIndex.y < 0 || gridIndex.y >= maxRows) return;
            
            GameObject targetBlockObj = _blockGrid[gridIndex.y, gridIndex.x];
            if (targetBlockObj == null || !targetBlockObj.activeInHierarchy) return;
            
            if (targetBlockObj.TryGetComponent(out Block targetBlock))
            {
                if (comboManager != null) comboManager.AddCombo();
                
                int earnedScore = targetBlock.TakeDamage(damage);
                
                OnDamageDealt?.Invoke(damage);
                
                bool isDestroyed = targetBlock.CurrentHp <= 0;

                if (isDestroyed)
                {
                    _blockGrid[gridIndex.y, gridIndex.x] = null;
                    
                    if (vfxManager != null)
                    {
                        vfxManager.PlayVFX(VFXType.BlockDestroy, targetBlockObj.transform.position);
                    }
                    
                    OnBlockDestroyed?.Invoke();
                    
                    if (comboManager != null)
                    {
                        comboManager.AddPendingCoin(20);
                    }
                }

                if (scoreManager != null)
                {
                    scoreManager.AddScore(earnedScore, isDestroyed);
                    skillManager.AddGauge(1f);
                }
            }
        }
        
        private void SpawnBlock(int row, int col, int hp, BlockType type = BlockType.Normal)
        {
            GameObject blockObj = GetBlockFormPool(type);
            
            blockObj.transform.position = GetWorldPosition(row, col);
            
            if (blockObj.TryGetComponent(out SpriteRenderer spriteRenderer))
            {
                float originalWidth = spriteRenderer.sprite.bounds.size.x;
                float originalHeight = spriteRenderer.sprite.bounds.size.y;

                float scaleX = _cellSpacingX / originalWidth;
                float scaleY = _cellSpacingY / originalHeight;

                blockObj.transform.localScale = new Vector3(scaleX, scaleY, 1f);
            }
            
            if (blockObj.TryGetComponent(out Block blockComponent))
            {
                blockComponent.Initialize(hp, this);
            }
            
            _blockGrid[row, col] = blockObj;
        }
        
        /// <summary>
        /// 액티브 스킬(Line Strike) 사용 시 특정 가로줄을 일괄 타격합니다.
        /// </summary>
        public void DamageRows(int startRow, int endRow, int damage)
        {
            for (int row = startRow; row <= endRow; row++)
            {
                for (int col = 0; col < maxColumns; col++)
                {
                    DamageBlock(new Vector2Int(col, row), damage);
                }
            }
        }
        
        // -- Special Block 로직 --

        public void ExecuteTurnEndEffects()
        {
            _blocksToProcessCache.Clear();
            
            for (int row = 0; row < maxRows; row++)
            {
                for (int col = 0; col < maxColumns; col++)
                {
                    GameObject blockObj = _blockGrid[row, col];
                    if (blockObj != null && blockObj.activeInHierarchy)
                    {
                        if (blockObj.TryGetComponent(out Block block))
                        {
                            _blocksToProcessCache.Add(new KeyValuePair<Block, Vector2Int>(block, new Vector2Int(col, row)));
                        }
                    }
                }
            }
            
            foreach (var kvp in _blocksToProcessCache)
            {
                kvp.Key.OnTurnEnd(this, kvp.Value);
            }
        }

        /// <summary>
        /// 맵 상에 활성화된 모든 블록의 체력을 회복시킵니다.
        /// </summary>
        public void HealAllBlocks(int healAmount)
        {
            for (int row = 0; row < maxRows; row++)
            {
                for (int col = 0; col < maxColumns; col++)
                {
                    GameObject blockObj = _blockGrid[row, col];
                    if (blockObj != null && blockObj.activeInHierarchy)
                    {
                        if (blockObj.TryGetComponent(out Block block))
                        {
                            block.Heal(healAmount);
                        }
                    }
                }
            }
        }
        
        // -- Object Pool 로직 --
        private GameObject GetBlockFormPool(BlockType type)
        {
            if (_blockPools.ContainsKey(type) && _blockPools[type].Count > 0)
            {
                GameObject block = _blockPools[type].Dequeue();
                block.SetActive(true);
                return block;
            }

            GameObject prefab = blockPrefabs.Find(x => x.type == type).prefab;
            GameObject newBlock = Instantiate(prefab, transform);
            newBlock.SetActive(true);
            return newBlock;
        }

        public void ReturnBlockToPool(GameObject block)
        {
            if (block.TryGetComponent(out Block blockComponent))
            {
                block.SetActive(false);
                _blockPools[blockComponent.Type].Enqueue(block);
            }
        }
        
        // -- Async 기반 블록 하강 로직 --
        /// <summary>
        /// TurnEnd 상태일 때 호출되며, 블록 하강 로직 처리 후 대기 시간으 ㄹ가집니다.
        /// </summary>
        public async Task MoveBlocksDownAsync(int currentTurn)
        {
            ShiftGridDataDown();
            SpawnNewRow(currentTurn);

            await Task.Delay(300);
        }

        private void ShiftGridDataDown()
        {
            IsGameOverFlag = false;
            
            for (int col = 0; col < maxColumns; col++)
            {
                GameObject bottomBlockObj = _blockGrid[maxRows - 1, col];
                if (bottomBlockObj != null)
                {
                    if (bottomBlockObj.TryGetComponent(out Block block))
                    {
                        if (!block.CausesGameOver)
                        {
                            block.ForceDestroy();
                            _blockGrid[maxRows - 1, col] = null;
                            Debug.Log($"[무적 블록] 바닥에 도달하여 안전하게 파괴되었습니다. (Col: {col})");
                        }
                        else
                        {
                            IsGameOverFlag = true; 
                            return;
                        }
                    }
                }
            }
            
            for (int row = maxRows - 2; row >= 0; row--)
            {
                for (int col = 0; col < maxColumns; col++)
                {
                    GameObject block = _blockGrid[row, col];
                    if (block != null)
                    {
                        _blockGrid[row + 1, col] = block;
                        _blockGrid[row, col] = null;
                        
                        block.transform.position = GetWorldPosition(row + 1, col);
                        
                        if (block.TryGetComponent(out Block blockComponent))
                        {
                            blockComponent.SyncBaselinePosition();
                        }
                    }
                }
            }
        }

        private void SpawnNewRow(int currentTurn)
        {
            if (difficultyData == null || IsGameOverFlag) return;
            
            DifficultyStage currentStage = difficultyData.GetStageData(currentTurn);
            
            int targetSpawnCount = Random.Range(currentStage.minSpawnCount, currentStage.maxSpawnCount + 1);
            targetSpawnCount = Mathf.Clamp(targetSpawnCount, 1, maxColumns - 1);
            
            _availableColumns.Clear();
            for (int i = 0; i < maxColumns; i++)
            {
                _availableColumns.Add(i);
            }
            
            for (int i = 0; i < targetSpawnCount; i++)
            {
                int randomIndex = Random.Range(0, _availableColumns.Count);
                int selectedCol = _availableColumns[randomIndex];

                _availableColumns.RemoveAt(randomIndex);
                
                BlockType selectedType = GetRandomBlockType(currentStage.spawnRates);

                // 랜덤 HP 부여
                int randomHp = Random.Range(currentStage.minHp, currentStage.maxHp + 1);
                SpawnBlock(0, selectedCol, randomHp, selectedType);
            }
        }
        
        /// <summary>
        /// 블록의 타입을 결정하는 가중치 랜덤 알고리즘입니다.
        /// </summary>
        private BlockType GetRandomBlockType(List<BlockSpawnRate> rates)
        {
            float totalWeight = 0;
            foreach (var rate in rates) totalWeight += rate.weight;

            float randomValue = Random.Range(0, totalWeight);
            foreach (var rate in rates)
            {
                randomValue -= rate.weight;
                if (randomValue <= 0) return rate.blockType;
            }
            return BlockType.Normal;
        }
        
        // -- Grid 기반 블록 위치 관련 로직 --

        /// <summary>
        /// 논리적인 2D Grid 인덱스를 Unity World 좌표(Transform)로 변환합니다.
        /// </summary>
        public Vector2 GetWorldPosition(int row, int col)
        {
            float xPos = topLeftPosition.x + (col * _cellSpacingX);
            float yPos = topLeftPosition.y - (row * _cellSpacingY);
            return new Vector2(xPos, yPos);
        }

        /// <summary>
        /// 무거운 물리 연산을 대체하기 위해 World 좌표를 Grid 인덱스로 역산합니다.
        /// </summary>
        public Vector2Int GetGridIndex(Vector2 worldPosition)
        {
            int col = Mathf.RoundToInt((worldPosition.x - topLeftPosition.x) / _cellSpacingX);
            int row = Mathf.RoundToInt((topLeftPosition.y - worldPosition.y) / _cellSpacingY);

            col = Mathf.Clamp(col, 0, maxColumns - 1);
            row = Mathf.Clamp(row, 0, maxRows - 1);
            
            return new Vector2Int(col, row);
        }

        public bool TryMultiplyBlock(Vector2Int sourceIndex, int sourceHp)
        {
            int row = sourceIndex.y;
            int col = sourceIndex.x;

            if (col > 0 && _blockGrid[row, col - 1] == null)
            {
                SpawnBlock(row, col - 1, sourceHp / 2, BlockType.Multiply);
                return true;
            }
            
            if (col < maxColumns - 1 && _blockGrid[row, col + 1] == null)
            {
                SpawnBlock(row, col + 1, sourceHp / 2, BlockType.Multiply);
                return true;
            }
            
            return false;
        }
        
        // -- Save & Load 관련 로직
        
        public List<BlockSaveData> GetBoardSaveData()
        {
            List<BlockSaveData> boardData = new List<BlockSaveData>();
            for (int y = 0; y < maxRows; y++)
            {
                for (int x = 0; x < maxColumns; x++)
                {
                    GameObject blockObj = _blockGrid[y, x];
                    
                    if (blockObj != null && blockObj.activeInHierarchy && blockObj.TryGetComponent(out Block block))
                    {
                        boardData.Add(new BlockSaveData { gridX = x, gridY = y, type = block.Type, hp = block.CurrentHp });
                    }
                }
            }
            return boardData;
        }
        
        public void RestoreBoardState(List<BlockSaveData> savedBoard)
        {
            // 기존에 깔려있던 블록이 있다면 전부 초기화
            for (int y = 0; y < MaxRows; y++)
            {
                for (int x = 0; x < MaxColumns; x++)
                {
                    if (_blockGrid[y, x] != null)
                    {
                        ReturnBlockToPool(_blockGrid[y, x]);
                        _blockGrid[y, x] = null;
                    }
                }
            }

            // 저장된 데이터를 바탕으로 지정된 위치에 블록 스폰
            foreach (var blockData in savedBoard)
            {
                // 인덱스 안전성 검사
                if (blockData.gridX >= 0 && blockData.gridX < MaxColumns &&
                    blockData.gridY >= 0 && blockData.gridY < MaxRows)
                {
                    // 저장된 HP와 타입 그대로 스폰합니다.
                    SpawnBlock(blockData.gridY, blockData.gridX, blockData.hp, blockData.type);
                }
            }
            Debug.Log($"[BlockManager] {savedBoard.Count}개의 블록 복구 완료!");
        }
    }
}
