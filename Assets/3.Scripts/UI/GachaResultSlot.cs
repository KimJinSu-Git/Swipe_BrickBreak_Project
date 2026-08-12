using Bird.OutGame;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bird.UI
{
    public class GachaResultSlot : MonoBehaviour
    {
        [Header("Slot Settings")] 
        [SerializeField] private Image topColorBg;
        [SerializeField] private TextMeshProUGUI textRank;
        [SerializeField] private TextMeshProUGUI textName;

        public void SetData(BallType type)
        {
            textName.text = GetNameText(type);
            textRank.text = GetRankText(type);
            topColorBg.color = GetRankColor(type);
        }

        private string GetNameText(BallType type) => type switch
        {
            BallType.Normal => "기본 공",
            BallType.Cross => "십자 공",
            BallType.Explosion => "폭발 공",
            BallType.Laser => "레이저 공",
            _ => "기본 공"
        };
        
        private string GetRankText(BallType type) => type switch
        {
            BallType.Normal => "N",
            BallType.Cross => "R",
            BallType.Explosion => "SR",
            BallType.Laser => "SSR",
            _ => "N"
        };
        
        private Color GetRankColor(BallType type) => type switch
        {
            BallType.Normal => new Color(0.7f, 0.7f, 0.7f), 
            BallType.Cross => new Color(0.4f, 0.75f, 1f), 
            BallType.Explosion => new Color(0.85f, 0.45f, 1f), 
            BallType.Laser => new Color(1f, 0.75f, 0.2f),
            _ => Color.white
        };
    }
}
