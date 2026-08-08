using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bird.Core
{
    [Serializable]
    public struct DifficultyStage
    {
        public int minTurn;
        public int maxTurn;
        public int minHp;
        public int maxHp;

        public int minSpawnCount;
        public int maxSpawnCount;
    }

    [CreateAssetMenu(fileName = "DifficultyData", menuName = "Bird/DifficultyData")]
    public class DifficultyData : ScriptableObject
    {
        [SerializeField] private List<DifficultyStage> stages;

        /// <summary>
        /// 현재 턴을 입력받아 해당하는 구간의 난이도 데이터를 반환합니다.
        /// </summary>
        /// <param name="currnetTurn"></param>
        /// <returns></returns>
        public DifficultyStage GetStageData(int currentTurn)
        {
            foreach (var stage in stages)
            {
                if (currentTurn >= stage.minTurn && currentTurn <= stage.maxTurn)
                {
                    return stage;
                }
            }
            
            // 기획된 턴을 초과하게 되면(SO 스테이지보다 더 높이 올라가게 되면) 가장 마지막 난이도를 반환합니다.
            return stages[stages.Count - 1];
        }
    }
}
