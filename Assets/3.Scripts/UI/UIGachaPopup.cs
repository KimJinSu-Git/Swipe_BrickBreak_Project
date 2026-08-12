using System.Collections.Generic;
using Bird.OutGame;
using UnityEngine;
using UnityEngine.UI;

namespace Bird.UI
{
    public class UIGachaPopup : MonoBehaviour
    {
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private Button confirmButton;
        [SerializeField] private List<GachaResultSlot> slotPool;
        
        private void Start()
        {
            popupRoot.SetActive(false);
            
            if (confirmButton != null)
            {
                confirmButton.onClick.AddListener(() => popupRoot.SetActive(false));
            }
        }
        
        /// <summary>
        /// GachaManager로부터 결과를 전달받아 UI를 갱신합니다.
        /// </summary>
        public void ShowResults(List<BallType> results)
        {
            popupRoot.SetActive(true);

            // 전달받은 결과(1개 or 5개)만큼만 카드를 켜고, 나머지는 끕니다.
            for (int i = 0; i < slotPool.Count; i++)
            {
                if (i < results.Count)
                {
                    slotPool[i].SetData(results[i]);
                    slotPool[i].gameObject.SetActive(true);
                }
                else
                {
                    slotPool[i].gameObject.SetActive(false); 
                }
            }
        }
    }
}
