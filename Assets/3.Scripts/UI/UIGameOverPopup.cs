using Bird.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Bird.UI
{
    public class UIGameOverPopup : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private TurnManager turnManager;

        [Header("UI Components")]
        [SerializeField] private GameObject popupRoot;
        [SerializeField] private Button buttonRestart;
        
        private void Start()
        {
            popupRoot.SetActive(false);

            if (buttonRestart != null) buttonRestart.onClick.AddListener(OnRestartClicked);
            
            if (turnManager != null) turnManager.OnGameOver += ShowPopup;
        }

        private void OnDestroy()
        {
            if (turnManager != null) turnManager.OnGameOver -= ShowPopup;
        }

        private void ShowPopup() => popupRoot.SetActive(true);

        private void OnRestartClicked()
        {
            if (gameManager != null)
            {
                gameManager.RestartGame();
            }
        }
    }
}
