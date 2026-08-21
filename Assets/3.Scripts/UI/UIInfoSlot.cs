using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Bird.UI
{
    public class UIInfoSlot : MonoBehaviour
    {
        [Header("UI Components")] 
        [SerializeField] private Image imageIcon;
        [SerializeField] private TextMeshProUGUI textName;
        [SerializeField] private TextMeshProUGUI textDescription;
        [SerializeField] private TextMeshProUGUI textCount;
        
        /// <summary>
        /// 풀링된 슬롯의 데이터를 갱신합니다.
        /// </summary>
        public void SetData(Sprite icon, string name, string desc, string countText = "")
        {
            if (imageIcon != null && icon != null) imageIcon.sprite = icon;
            if (textName != null) textName.text = name;
            if (textDescription != null) textDescription.text = desc;
            
            if (textCount != null)
            {
                textCount.text = countText;
                textCount.gameObject.SetActive(!string.IsNullOrEmpty(countText));
            }
        }
    }
}
