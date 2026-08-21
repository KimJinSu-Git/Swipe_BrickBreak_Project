using System;
using System.Collections.Generic;
using System.Linq;
using Bird.Ball;
using Bird.Core;
using Bird.Data;
using Bird.InGame;
using UnityEngine;
using UnityEngine.UI;

namespace Bird.UI
{
    public class UIInfoPopup : MonoBehaviour
    {
        [Serializable]
        public struct BallInfoData
        {
            public BallType type;
            public Sprite icon;
            public string name;
            [TextArea] public string description;
        }
        
        [Serializable]
        public struct BlockInfoData
        {
            public BlockType type;
            public Sprite icon;
            public string name;
            [TextArea] public string description;
        }
        
        [Header("Managers")]
        [SerializeField] private BallManager ballManager;
        [SerializeField] private TurnManager turnManager;
        [SerializeField] private DifficultyData difficultyData;

        [Header("UI Dependencies")]
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private Button buttonConfirm;
        [SerializeField] private Button buttonCloseBg;
        
        [Header("Tab System")]
        [SerializeField] private Button buttonTabBall;
        [SerializeField] private Button buttonTabBlock;
        [SerializeField] private Transform contentArea;
        [SerializeField] private UIInfoSlot slotPrefab;

        [Header("Mock Data (임시 데이터)")]
        [SerializeField] private List<BallInfoData> ballInfoDatabase;
        [SerializeField] private List<BlockInfoData> blockInfoDatabase;
        
        private List<UIInfoSlot> _slotPool = new List<UIInfoSlot>();
        
        private void Start()
        {
            popupRoot.SetActive(false);

            if (buttonConfirm != null) buttonConfirm.onClick.AddListener(ClosePopup);
            if (buttonCloseBg != null) buttonCloseBg.onClick.AddListener(ClosePopup);

            if (buttonTabBall != null) buttonTabBall.onClick.AddListener(ShowBallTab);
            if (buttonTabBlock != null) buttonTabBlock.onClick.AddListener(ShowBlockTab);
        }
        
        public void OpenPopup()
        {
            popupRoot.SetActive(true);
            ShowBallTab();
        }
        
        public void ClosePopup() => popupRoot.SetActive(false);
        
        private void ShowBallTab()
        {
            if (ballManager == null) return;

            var deckCounts = ballManager.PlayerDeck
                .GroupBy(ball => ball)
                .ToDictionary(group => group.Key, group => group.Count());

            int slotIndex = 0;

            foreach (var kvp in deckCounts)
            {
                BallType type = kvp.Key;
                int count = kvp.Value;

                BallInfoData info = ballInfoDatabase.Find(x => x.type == type);
                
                UIInfoSlot slot = GetOrCreateSlot(slotIndex);
                slot.SetData(info.icon, info.name, info.description, $"x{count}");
                
                slotIndex++;
            }

            HideUnusedSlots(slotIndex);
            UpdateTabVisuals(isBallTabActive: true);
        }
        
        private void ShowBlockTab()
        {
            if (turnManager == null || difficultyData == null) return;
            
            int currentTurn = turnManager.CurrentTurn; 
            DifficultyStage currentStage = difficultyData.GetStageData(currentTurn);
            int slotIndex = 0;

            foreach (var rate in currentStage.spawnRates)
            {
                if (rate.weight <= 0) continue;

                BlockInfoData info = blockInfoDatabase.Find(x => x.type == rate.blockType);

                UIInfoSlot slot = GetOrCreateSlot(slotIndex);
                
                slot.SetData(info.icon, info.name, info.description, ""); 
                
                slotIndex++;
            }

            HideUnusedSlots(slotIndex);
            UpdateTabVisuals(isBallTabActive: false);
        }
        
        private UIInfoSlot GetOrCreateSlot(int index)
        {
            if (index < _slotPool.Count)
            {
                _slotPool[index].gameObject.SetActive(true);
                return _slotPool[index];
            }

            UIInfoSlot newSlot = Instantiate(slotPrefab, contentArea);
            newSlot.gameObject.SetActive(true);
            _slotPool.Add(newSlot);
            
            return newSlot;
        }

        private void HideUnusedSlots(int startIndex)
        {
            for (int i = startIndex; i < _slotPool.Count; i++)
            {
                _slotPool[i].gameObject.SetActive(false);
            }
        }
        
        private void UpdateTabVisuals(bool isBallTabActive)
        {
            if (buttonTabBall != null) buttonTabBall.interactable = !isBallTabActive;
            if (buttonTabBlock != null) buttonTabBlock.interactable = isBallTabActive;
        }
    }
}

