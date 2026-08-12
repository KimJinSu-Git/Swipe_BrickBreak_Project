#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Bird.AI.UIBuilder
{
    public class GachaUIBuilder : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField] private TMP_FontAsset font; // 생성되는 모든 텍스트에 적용할 폰트

        private const string RootName = "Main UI";
        private readonly Vector2 ReferenceResolution = new Vector2(1440, 3088);

        [ContextMenu("1. Build UI")]
        public void BuildUI()
        {
            // 중복 생성 방지 체크
            GameObject existingRoot = GameObject.Find(RootName);
            if (existingRoot != null)
            {
                Debug.LogWarning($"[UIBuilder] '{RootName}'이(가) 이미 존재합니다. 겹쳐서 만들지 않고 중단합니다.");
                return;
            }

            // EventSystem 존재 여부 확인
            if (FindObjectOfType<EventSystem>() == null)
            {
                Debug.LogWarning("[UIBuilder] 씬에 EventSystem이 없습니다. UI 상호작용을 위해 EventSystem을 씬에 추가해주세요.");
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

            // 2. 가챠 결과 팝업 영역 생성
            CreateGachaPopupUI(rootObj.transform);

            // 변경사항 저장 플래그
            EditorSceneManager.MarkSceneDirty(rootObj.scene);
            Debug.Log($"[UIBuilder] '{RootName}' 생성 완료. (가챠 팝업 - 피라미드 5개 배치 적용)");
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

        #region Create UI Areas

        private void CreateGachaPopupUI(Transform canvas)
        {
            // 팝업 전체 루트
            GameObject popup = NewUI("Panel_GachaResultPopup", canvas);
            Stretch(popup.GetComponent<RectTransform>(), 0f);

            // 딤(어두운) 배경 버튼
            Button dimBtn = NewButton("Button_DimBg", popup.transform, "", out TMP_Text dimTxt);
            Stretch(dimBtn.GetComponent<RectTransform>(), 0f);
            dimBtn.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.8f);
            dimTxt.gameObject.SetActive(false); // 라벨 텍스트 숨김

            // 팝업 중앙 컨텐츠 패널
            GameObject content = NewUI("Panel_Content", popup.transform);
            RectTransform contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0.5f, 0.5f);
            contentRt.anchorMax = new Vector2(0.5f, 0.5f);
            contentRt.pivot = new Vector2(0.5f, 0.5f);
            contentRt.sizeDelta = new Vector2(1200, 1600);
            contentRt.anchoredPosition = Vector2.zero;

            // 상단 타이틀
            TMP_Text title = NewText("Text_Title", content.transform, "✦ 공 획득! ✦", 75f, TextAlignmentOptions.Center);
            title.rectTransform.anchorMin = new Vector2(0.5f, 1f);
            title.rectTransform.anchorMax = new Vector2(0.5f, 1f);
            title.rectTransform.pivot = new Vector2(0.5f, 1f);
            title.rectTransform.sizeDelta = new Vector2(800, 120);
            title.rectTransform.anchoredPosition = new Vector2(0, -150);
            title.color = new Color(1f, 0.9f, 0.4f);
            title.fontStyle = FontStyles.Bold;

            // 점선 (구분선)
            GameObject separator = NewPanel("Image_Separator", content.transform, new Vector2(1000, 10));
            RectTransform sepRt = separator.GetComponent<RectTransform>();
            sepRt.anchorMin = new Vector2(0.5f, 1f);
            sepRt.anchorMax = new Vector2(0.5f, 1f);
            sepRt.pivot = new Vector2(0.5f, 1f);
            sepRt.anchoredPosition = new Vector2(0, -280);
            separator.GetComponent<Image>().color = new Color(0.7f, 0.8f, 0.4f, 0.8f);

            // ---------------------------------------------------------
            // 아이템 래퍼 (피라미드 5개 배치를 위한 Vertical + Horizontal 조합)
            // ---------------------------------------------------------
            GameObject itemsWrapper = NewUI("Layout_ItemsWrapper", content.transform);
            RectTransform wrapRt = itemsWrapper.GetComponent<RectTransform>();
            wrapRt.anchorMin = new Vector2(0.5f, 0.5f);
            wrapRt.anchorMax = new Vector2(0.5f, 0.5f);
            wrapRt.pivot = new Vector2(0.5f, 0.5f);
            wrapRt.sizeDelta = new Vector2(960, 790); // 300*3 + 30*2 넓이 / 380*2 + 30 높이
            wrapRt.anchoredPosition = new Vector2(0, 50);

            VerticalLayoutGroup vlg = itemsWrapper.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 30f;
            vlg.childAlignment = TextAnchor.MiddleCenter;
            vlg.childControlWidth = false;
            vlg.childControlHeight = false;
            vlg.childForceExpandWidth = false;
            vlg.childForceExpandHeight = false;

            // 상단 행 (3개)
            GameObject topRow = NewUI("Row_Top", itemsWrapper.transform);
            topRow.GetComponent<RectTransform>().sizeDelta = new Vector2(960, 380);
            HorizontalLayoutGroup topHlg = topRow.AddComponent<HorizontalLayoutGroup>();
            topHlg.spacing = 30f;
            topHlg.childAlignment = TextAnchor.MiddleCenter;
            topHlg.childControlWidth = false;
            topHlg.childControlHeight = false;
            topHlg.childForceExpandWidth = false;
            topHlg.childForceExpandHeight = false;

            // 하단 행 (2개)
            GameObject bottomRow = NewUI("Row_Bottom", itemsWrapper.transform);
            bottomRow.GetComponent<RectTransform>().sizeDelta = new Vector2(960, 380);
            HorizontalLayoutGroup bottomHlg = bottomRow.AddComponent<HorizontalLayoutGroup>();
            bottomHlg.spacing = 30f;
            bottomHlg.childAlignment = TextAnchor.MiddleCenter;
            bottomHlg.childControlWidth = false;
            bottomHlg.childControlHeight = false;
            bottomHlg.childForceExpandWidth = false;
            bottomHlg.childForceExpandHeight = false;

            // 색상 프리셋
            Color colorR = new Color(0.4f, 0.75f, 1f);
            Color colorSR = new Color(0.85f, 0.45f, 1f);
            Color colorSSR = new Color(1f, 0.75f, 0.2f);

            // 상단 배치 (3개)
            CreateItemCard(topRow.transform, "Item_Card_R_1", "R", "기본공", colorR);
            CreateItemCard(topRow.transform, "Item_Card_R_2", "R", "기본공", colorR);
            CreateItemCard(topRow.transform, "Item_Card_R_3", "R", "기본공", colorR);

            // 하단 배치 (2개) - 5개 피라미드 모양 적용
            CreateItemCard(bottomRow.transform, "Item_Card_SR_1", "SR", "감전공", colorSR);
            CreateItemCard(bottomRow.transform, "Item_Card_SSR_1", "SSR", "세로 범위공", colorSSR);

            // ---------------------------------------------------------

            // 하단 종료 안내 텍스트
            TMP_Text guide = NewText("Text_CloseGuide", content.transform, "빈 곳을 터치해서 종료", 45f, TextAlignmentOptions.Center);
            guide.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            guide.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            guide.rectTransform.pivot = new Vector2(0.5f, 0f);
            guide.rectTransform.sizeDelta = new Vector2(800, 80);
            guide.rectTransform.anchoredPosition = new Vector2(0, 150);
            guide.color = new Color(0.9f, 0.9f, 0.9f);
        }

        // 아이템 카드를 생성하는 로컬 헬퍼 (배열용)
        private void CreateItemCard(Transform parent, string objName, string rank, string itemName, Color topColor)
        {
            GameObject card = NewPanel(objName, parent, new Vector2(300, 380));
            card.GetComponent<Image>().color = Color.white; // 카드 바탕 (흰색)

            // 카드 상단 색상 영역
            GameObject topBg = NewPanel("Image_TopColor", card.transform, new Vector2(300, 240));
            RectTransform topRt = topBg.GetComponent<RectTransform>();
            topRt.anchorMin = new Vector2(0, 1);
            topRt.anchorMax = new Vector2(1, 1);
            topRt.pivot = new Vector2(0.5f, 1);
            topRt.sizeDelta = new Vector2(0, 240); // 좌우 Stretch
            topRt.anchoredPosition = Vector2.zero;
            topBg.GetComponent<Image>().color = topColor;

            // 좌측 상단 랭크 (R, SR, SSR)
            TMP_Text rankTxt = NewText("Text_Rank", card.transform, rank, 45f, TextAlignmentOptions.Left);
            rankTxt.rectTransform.anchorMin = new Vector2(0, 1);
            rankTxt.rectTransform.anchorMax = new Vector2(0, 1);
            rankTxt.rectTransform.pivot = new Vector2(0, 1);
            rankTxt.rectTransform.sizeDelta = new Vector2(100, 60);
            rankTxt.rectTransform.anchoredPosition = new Vector2(20, -20);
            rankTxt.fontStyle = FontStyles.Bold;
            rankTxt.color = rank == "SSR" ? new Color(1f, 0.9f, 0.1f) : Color.white; // SSR만 노란색 하이라이트

            // 중앙 아이콘
            GameObject icon = NewPanel("Image_Icon", card.transform, new Vector2(130, 130));
            RectTransform iconRt = icon.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0.5f, 1f);
            iconRt.anchorMax = new Vector2(0.5f, 1f);
            iconRt.pivot = new Vector2(0.5f, 1f);
            iconRt.anchoredPosition = new Vector2(0, -60);
            icon.GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.9f); // 아이콘 목업 (연회색)

            // 하단 아이템 명칭
            TMP_Text nameTxt = NewText("Text_Name", card.transform, itemName, 36f, TextAlignmentOptions.Center);
            nameTxt.rectTransform.anchorMin = new Vector2(0.5f, 0f);
            nameTxt.rectTransform.anchorMax = new Vector2(0.5f, 0f);
            nameTxt.rectTransform.pivot = new Vector2(0.5f, 0f);
            nameTxt.rectTransform.sizeDelta = new Vector2(280, 80);
            nameTxt.rectTransform.anchoredPosition = new Vector2(0, 30);
            nameTxt.color = Color.black;
            nameTxt.fontStyle = FontStyles.Bold;
        }

        #endregion

        #region Helper Methods (요청된 필수 구조)

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
            GameObject go = NewPanel(name, parent, new Vector2(200, 100));
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
    }
}
#endif