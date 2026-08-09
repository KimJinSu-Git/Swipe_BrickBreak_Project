using System;
using System.Collections.Generic;
using UnityEngine;

namespace Bird.OutGame
{
    public enum BallType { Normal, Cross, Explosion, Laser}

    [Serializable]
    public struct GachaRate
    {
        public BallType ballType;
        [Range(0, 100)] public float weight;
    }
    
    [CreateAssetMenu(fileName = "GachaData", menuName = "Bird/GachaData")]
    public class GachaData : ScriptableObject
    {
        [Header("Price Settings")] 
        public int singlePullCost = 50;
        public int fivePullCost = 200;

        [Header("Normal Pull Pool")] 
        public List<GachaRate> normalRates;

        [Header("Pity Pull Pool")] 
        public List<GachaRate> pityRates;
    }
}
