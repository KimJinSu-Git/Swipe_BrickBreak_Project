using Bird.Core;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Bird.UI
{
    public class UIOptionPopup : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private GameManager gameManager;

        [Header("UI Components")]
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private Button buttonOpenOption;
        [SerializeField] private Button buttonClose;
        [SerializeField] private Button buttonBgClose;
        [SerializeField] private Button buttonRestart;

        private void Start()
        {
            popupRoot.SetActive(false);

            if (buttonOpenOption != null) buttonOpenOption.onClick.AddListener(OpenPopup);
            if (buttonClose != null) buttonClose.onClick.AddListener(ClosePopup);
            if (buttonBgClose != null) buttonBgClose.onClick.AddListener(ClosePopup);
            if (buttonRestart != null) buttonRestart.onClick.AddListener(OnRestartClicked);
        }

        public void OpenPopup()
        {
            popupRoot.SetActive(true);
        }

        public void ClosePopup()
        {
            popupRoot.SetActive(false);
        }

        private void OnRestartClicked()
        {
            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
        }
    }
}