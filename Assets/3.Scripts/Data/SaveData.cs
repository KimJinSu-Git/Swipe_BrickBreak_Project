using System;
using System.Collections.Generic;
using Bird.Ball;
using Bird.InGame;
using UnityEngine;

namespace Bird.Data
{
    [Serializable]
    public struct BlockSaveData
    {
        public int gridX;
        public int gridY;
        public BlockType type;
        public int hp;
    }
    [Serializable]
    public class SaveData
    {
        // 기본 진행 정보
        public int currentTurn = 1;
        public int score = 0;
        public int coin = 0;
        
        // 게이지 및 콤보 정보
        public float skillGauge = 0f;
        public int currentCombo = 0;
        
        // 보유 공 리스트
        public List<BallType> playerDeck = new List<BallType>();
        
        // 현재 보드에 남아있는 블록들 상태
        public List<BlockSaveData> boardState = new List<BlockSaveData>();
    }
}
