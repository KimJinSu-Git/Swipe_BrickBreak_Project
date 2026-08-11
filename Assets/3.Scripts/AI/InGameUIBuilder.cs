#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Bird.AI.UIBuilder
{
    public class InGameUIBuilder : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField] private TMP_FontAsset font;

        private const string RootName = "Main UI";
        private readonly Vector2 ReferenceResolution = new Vector2(1440, 3088);

        [ContextMenu("1. Build UI")]
        public void BuildUI()
        {
            // 중복 생성 방지
            GameObject existingRoot = GameObject.Find(RootName);
            if (existingRoot != null)
            {
                Debug.LogWarning($"[UIBuilder] '{RootName}'이(가) 이미 존재합니다. 겹쳐서 만들지 않고 중단합니다.");
                return;
            }

            // EventSystem 확인
            if (FindObjectOfType<EventSystem>() == null)
            {
                Debug.LogWarning("[UIBuilder] 씬에 EventSystem이 없습니다. UI 상호작용을 위해 EventSystem을 추가해주세요.");
            }

            // 1. Root & Canvas 설정
            GameObject rootObj = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(rootObj, "Build UI");

            Canvas canvas = rootObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = rootObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;

            rootObj.AddComponent<GraphicRaycaster>();

            // 2. 영역별 UI 생성
            CreateTopUI(rootObj.transform);
            CreateCenterUI(rootObj.transform);
            CreateBottomUI(rootObj.transform);

            // 변경사항 저장
            EditorSceneManager.MarkSceneDirty(rootObj.scene);
            Debug.Log($"[UIBuilder] '{RootName}' 생성 완료.");
        }

        [ContextMenu("2. Clear UI")]
        public void ClearUI()
        {
            GameObject existingRoot = GameObject.Find(RootName);
            if (existingRoot != null)
            {
                Undo.DestroyObjectImmediate(existingRoot);
                Debug.Log($"[UIBuilder] '{RootName}'을(를) 삭제했습니다.");
            }
            else
            {
                Debug.Log($"[UIBuilder] 삭제할 '{RootName}'을(를) 찾을 수 없습니다.");
            }
        }

        #region UI Creation Parts

        private void CreateTopUI(Transform canvas)
        {
            GameObject topUI = NewUI("TopUI", canvas);
            Stretch(topUI.GetComponent<RectTransform>(), 0f);

            // Left Group (Score & Wave)
            GameObject topLeftGroup = NewUI("TopLeftGroup", topUI.transform);
            RectTransform tlRect = topLeftGroup.GetComponent<RectTransform>();
            SetRect(tlRect, new Vector2(0, 1), new Vector2(0, 1), new Vector2(0, 1), new Vector2(50, -50), new Vector2(500, 250));
            VerticalLayoutGroup vlg = topLeftGroup.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 20f;
            vlg.childControlWidth = true;
            vlg.childControlHeight = true;

            // Score Panel
            GameObject scorePanel = NewPanel("ScorePanel", topLeftGroup.transform, new Vector2(500, 100));
            scorePanel.GetComponent<Image>().color = new Color(1f, 0.95f, 0.9f);
            
            TMP_Text scoreLabel = NewText("Text_ScoreLabel", scorePanel.transform, "SCORE", 45f, TextAlignmentOptions.Left);
            SetRect(scoreLabel.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(30, 0), new Vector2(200, 100));
            scoreLabel.color = new Color(0.9f, 0.4f, 0.1f);

            TMP_Text scoreVal = NewText("Text_ScoreValue", scorePanel.transform, "0", 50f, TextAlignmentOptions.Right);
            SetRect(scoreVal.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-30, 0), new Vector2(200, 100));
            scoreVal.color = Color.black;

            // Wave Panel
            GameObject wavePanel = NewPanel("WavePanel", topLeftGroup.transform, new Vector2(500, 100));
            wavePanel.GetComponent<Image>().color = new Color(0.8f, 0.95f, 1f);

            TMP_Text waveLabel = NewText("Text_WaveLabel", wavePanel.transform, "WAVE", 45f, TextAlignmentOptions.Left);
            SetRect(waveLabel.rectTransform, new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(30, 0), new Vector2(150, 100));
            waveLabel.color = new Color(0.2f, 0.4f, 0.8f);

            TMP_Text waveVal = NewText("Text_WaveValue", wavePanel.transform, "1", 50f, TextAlignmentOptions.Right);
            SetRect(waveVal.rectTransform, new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-180, 0), new Vector2(100, 100));
            waveVal.color = Color.black;

            Button skipBtn = NewButton("Button_Skip", wavePanel.transform, "▶ SKIP\nW", out TMP_Text skipTxt);
            SetRect(skipBtn.GetComponent<RectTransform>(), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-10, 0), new Vector2(160, 90));
            skipBtn.GetComponent<Image>().color = new Color(1f, 0.6f, 0.1f);
            skipTxt.color = Color.white;
            skipTxt.fontSize = 35f;

            // Right Group (TAB / ESC Buttons)
            GameObject topRightGroup = NewUI("TopRightGroup", topUI.transform);
            RectTransform trRect = topRightGroup.GetComponent<RectTransform>();
            SetRect(trRect, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-50, -50), new Vector2(350, 150));
            HorizontalLayoutGroup hlg = topRightGroup.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 30f;
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            Button tabBtn = NewButton("Button_Element_TAB", topRightGroup.transform, "TAB", out TMP_Text tabTxt);
            tabBtn.GetComponent<Image>().color = new Color(0.95f, 0.8f, 0.6f);
            tabTxt.alignment = TextAlignmentOptions.Bottom;
            tabTxt.margin = new Vector4(0, 0, 0, 10);

            Button escBtn = NewButton("Button_Pause_ESC", topRightGroup.transform, "ESC\n| |", out TMP_Text escTxt);
            escBtn.GetComponent<Image>().color = new Color(0.95f, 0.8f, 0.6f);
        }

        private void CreateCenterUI(Transform canvas)
        {
            GameObject centerUI = NewUI("CenterUI", canvas);
            Stretch(centerUI.GetComponent<RectTransform>(), 0f);

            // Play Area Outline Frame
            GameObject playFrame = NewPanel("PlayAreaFrame", centerUI.transform, new Vector2(1200, 1800));
            SetRect(playFrame.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 50), new Vector2(1200, 1800));
            Image frameImg = playFrame.GetComponent<Image>();
            frameImg.color = new Color(1f, 1f, 1f, 0.3f); // Semi-transparent for mockup

            // Mockup Blocks
            GameObject blockGroup = NewUI("BlockGroup", centerUI.transform);
            SetRect(blockGroup.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, 600), new Vector2(400, 150));
            HorizontalLayoutGroup blockHlg = blockGroup.AddComponent<HorizontalLayoutGroup>();
            blockHlg.spacing = 20f;
            blockHlg.childControlWidth = true;
            blockHlg.childControlHeight = true;

            NewPanel("Block_Water", blockGroup.transform, new Vector2(150, 150)).GetComponent<Image>().color = new Color(0.3f, 0.7f, 1f);
            NewPanel("Block_Jellyfish", blockGroup.transform, new Vector2(150, 150)).GetComponent<Image>().color = new Color(0.5f, 0.6f, 0.9f);

            // Multiplier Text
            TMP_Text multiTxt = NewText("Text_Multiplier", centerUI.transform, "x1", 60f, TextAlignmentOptions.Center);
            SetRect(multiTxt.rectTransform, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -800), new Vector2(200, 100));
            multiTxt.color = Color.white;
            multiTxt.fontStyle = FontStyles.Bold;
        }

        private void CreateBottomUI(Transform canvas)
        {
            GameObject bottomUI = NewUI("BottomUI", canvas);
            RectTransform botRect = bottomUI.GetComponent<RectTransform>();
            SetRect(botRect, new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(1350, 400));

            // Main Wood Board
            GameObject board = NewPanel("Panel_Board", bottomUI.transform, new Vector2(1350, 400));
            SetRect(board.GetComponent<RectTransform>(), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1350, 400));
            board.GetComponent<Image>().color = new Color(0.9f, 0.85f, 0.7f); // Wood color mockup

            // --- Left Group (Skills & Extra Chances) ---
            GameObject leftGroup = NewUI("LeftGroup", board.transform);
            SetRect(leftGroup.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(20, 0), new Vector2(600, 360));

            // Extra Chance Panel
            GameObject extraPanel = NewPanel("Panel_ExtraChance", leftGroup.transform, new Vector2(580, 100));
            SetRect(extraPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(580, 100));
            extraPanel.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.8f);
            NewText("Text_ExtraLabel", extraPanel.transform, "추가 기회", 40f, TextAlignmentOptions.Center).color = Color.black;

            // Skill Panel
            GameObject skillPanel = NewPanel("Panel_Skill", leftGroup.transform, new Vector2(580, 200));
            SetRect(skillPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 110), new Vector2(580, 200));
            skillPanel.GetComponent<Image>().color = new Color(0.95f, 0.95f, 0.9f);
            
            GameObject skillIcon = NewPanel("Image_SkillIcon", skillPanel.transform, new Vector2(150, 150));
            SetRect(skillIcon.GetComponent<RectTransform>(), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(90, 0), new Vector2(150, 150));
            skillIcon.GetComponent<Image>().color = Color.gray;

            TMP_Text skillLabel = NewText("Text_SkillName", skillPanel.transform, "전용 스킬", 35f, TextAlignmentOptions.Center);
            SetRect(skillLabel.rectTransform, new Vector2(1, 1), new Vector2(1, 1), new Vector2(1, 1), new Vector2(-200, -50), new Vector2(300, 50));
            skillLabel.color = Color.black;

            GameObject progressBar = NewPanel("ProgressBar_BG", skillPanel.transform, new Vector2(350, 60));
            SetRect(progressBar.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(-200, 60), new Vector2(350, 60));
            progressBar.GetComponent<Image>().color = new Color(0.4f, 0.3f, 0.2f);
            
            TMP_Text progressTxt = NewText("Text_Progress", progressBar.transform, "0.00%", 35f, TextAlignmentOptions.Center);
            Stretch(progressTxt.rectTransform, 0f);
            progressTxt.color = Color.white;


            // --- Right Group (Currency & Buy) ---
            GameObject rightGroup = NewUI("RightGroup", board.transform);
            SetRect(rightGroup.GetComponent<RectTransform>(), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-20, 0), new Vector2(700, 360));

            // Currency Panel
            GameObject currencyPanel = NewPanel("Panel_Currency", rightGroup.transform, new Vector2(650, 100));
            SetRect(currencyPanel.GetComponent<RectTransform>(), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -60), new Vector2(650, 100));
            currencyPanel.GetComponent<Image>().color = new Color(0.85f, 0.6f, 0.4f);
            TMP_Text curTxt = NewText("Text_Amount", currencyPanel.transform, "330", 50f, TextAlignmentOptions.Center);
            curTxt.color = Color.white;

            // Buy Buttons Layout
            GameObject buyGroup = NewUI("BuyButtonGroup", rightGroup.transform);
            SetRect(buyGroup.GetComponent<RectTransform>(), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 110), new Vector2(650, 180));
            HorizontalLayoutGroup buyHlg = buyGroup.AddComponent<HorizontalLayoutGroup>();
            buyHlg.spacing = 20f;
            buyHlg.childControlWidth = true;
            buyHlg.childControlHeight = true;

            Button buy1Btn = NewButton("Button_Buy1", buyGroup.transform, "1회 구매\n40", out TMP_Text b1Txt);
            buy1Btn.GetComponent<Image>().color = new Color(0.9f, 0.7f, 0.5f);
            b1Txt.color = Color.black;

            Button buy6Btn = NewButton("Button_Buy6", buyGroup.transform, "6회 구매\n220", out TMP_Text b6Txt);
            buy6Btn.GetComponent<Image>().color = new Color(0.5f, 0.8f, 0.95f);
            b6Txt.color = Color.black;


            // --- Action Button (Bottom Right Overlap) ---
            Button actionBtn = NewButton("Button_Action_SPC", bottomUI.transform, "SPC", out TMP_Text actionTxt);
            SetRect(actionBtn.GetComponent<RectTransform>(), new Vector2(1, 0), new Vector2(1, 0), new Vector2(1, 0), new Vector2(30, -30), new Vector2(250, 250));
            actionBtn.GetComponent<Image>().color = new Color(0.8f, 0.8f, 0.75f);
            actionTxt.alignment = TextAlignmentOptions.Bottom;
            actionTxt.margin = new Vector4(0, 0, 0, 30);
            actionTxt.fontSize = 40f;
            actionTxt.color = Color.black;
        }

        #endregion

        #region Helper Methods (필수 규격)

        private GameObject NewUI(string name, Transform parent)
        {
            GameObject go = new GameObject(name);
            RectTransform rt = go.AddComponent<RectTransform>();
            rt.SetParent(parent, false);
            return go;
        }

        private GameObject NewPanel(string name, Transform parent, Vector2 size)
        {
            GameObject go = NewUI(name, parent);
            go.AddComponent<Image>();
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.sizeDelta = size;
            return go;
        }

        private TMP_Text NewText(string name, Transform parent, string text, float size, TextAlignmentOptions align)
        {
            GameObject go = NewUI(name, parent);
            TMP_Text tmp = go.AddComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = align;
            if (font != null)
            {
                tmp.font = font;
            }
            return tmp;
        }

        private Button NewButton(string name, Transform parent, string label, out TMP_Text labelText)
        {
            GameObject go = NewPanel(name, parent, new Vector2(200, 100)); // Default size
            Button btn = go.AddComponent<Button>();
            
            labelText = NewText("Text_Label", go.transform, label, 40f, TextAlignmentOptions.Center);
            Stretch(labelText.rectTransform, 0f);
            labelText.color = Color.black;

            return btn;
        }

        private void Stretch(RectTransform rect, float padding)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(padding, padding);
            rect.offsetMax = new Vector2(-padding, -padding);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }

        #endregion

        #region Extra Utilities

        // 앵커, 피벗, 위치, 크기를 한번에 세팅하기 위한 추가 유틸리티
        private void SetRect(RectTransform rt, Vector2 anchorMin, Vector2 anchorMax, Vector2 pivot, Vector2 anchoredPosition, Vector2 sizeDelta)
        {
            rt.anchorMin = anchorMin;
            rt.anchorMax = anchorMax;
            rt.pivot = pivot;
            rt.sizeDelta = sizeDelta;
            rt.anchoredPosition = anchoredPosition;
        }

        #endregion
    }
}
#endif