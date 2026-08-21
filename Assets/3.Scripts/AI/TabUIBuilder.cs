#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

namespace Bird.AI.UIBuilder
{
    public class TabUIBuilder : MonoBehaviour
    {
        [Header("UI Settings")]
        [SerializeField] private TMP_FontAsset font;

        private const string RootName = "Main UI";
        private readonly Vector2 ReferenceResolution = new Vector2(1440, 3088);

        [ContextMenu("1. Build UI")]
        public void BuildUI()
        {
            // 1. 중복 생성 방지
            GameObject existingRoot = GameObject.Find(RootName);
            if (existingRoot != null)
            {
                Debug.LogWarning($"[UIBuilder] '{RootName}'이(가) 이미 존재합니다. 겹쳐서 만들지 않고 중단합니다.");
                return;
            }

            // 2. EventSystem 확인
            if (FindObjectOfType<EventSystem>() == null)
            {
                Debug.LogWarning("[UIBuilder] 씬에 EventSystem이 없습니다. UI 상호작용을 위해 EventSystem을 추가해주세요.");
            }

            // 3. Root & Canvas 설정
            GameObject rootObj = new GameObject(RootName);
            Undo.RegisterCreatedObjectUndo(rootObj, "Build UI");

            Canvas canvas = rootObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = rootObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = ReferenceResolution;
            scaler.matchWidthOrHeight = 0.5f;

            rootObj.AddComponent<GraphicRaycaster>();

            // 4. 팝업 UI 생성
            CreatePopupUI(rootObj.transform);

            // 5. 변경사항 저장
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

        #region Create UI Areas

        private void CreatePopupUI(Transform canvas)
        {
            // 팝업 루트
            GameObject popup = NewUI("Panel_TabPopup", canvas);
            Stretch(popup.GetComponent<RectTransform>(), 0f);

            // 외부 터치 시 닫기용 딤(Dim) 배경 버튼
            Button dimBtn = NewButton("Button_DimBackground", popup.transform, "", out TMP_Text dimTxt);
            Stretch(dimBtn.GetComponent<RectTransform>(), 0f);
            dimBtn.GetComponent<Image>().color = new Color(0f, 0f, 0f, 0.7f);
            dimTxt.gameObject.SetActive(false); // 텍스트 컴포넌트 비활성화

            // 중앙 메인 팝업 패널
            GameObject mainPanel = NewPanel("Panel_MainContent", popup.transform, new Vector2(1200, 2200));
            RectTransform mainRt = mainPanel.GetComponent<RectTransform>();
            mainRt.anchorMin = new Vector2(0.5f, 0.5f);
            mainRt.anchorMax = new Vector2(0.5f, 0.5f);
            mainRt.pivot = new Vector2(0.5f, 0.5f);
            mainRt.anchoredPosition = Vector2.zero;
            mainPanel.GetComponent<Image>().color = new Color(0.9f, 0.9f, 0.95f);

            // 상단 탭 그룹 생성
            CreateTopTabGroup(mainPanel.transform);
            
            // 중앙 스크롤 리스트 생성
            CreateScrollView(mainPanel.transform);
            
            // 하단 버튼 영역 생성
            CreateBottomGroup(mainPanel.transform);
        }

        private void CreateTopTabGroup(Transform parent)
        {
            GameObject topGroup = NewUI("TopTabGroup", parent);
            RectTransform topRt = topGroup.GetComponent<RectTransform>();
            topRt.anchorMin = new Vector2(0, 1);
            topRt.anchorMax = new Vector2(1, 1);
            topRt.pivot = new Vector2(0.5f, 1);
            topRt.sizeDelta = new Vector2(0, 200);
            topRt.anchoredPosition = new Vector2(0, -50);

            HorizontalLayoutGroup hlg = topGroup.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 50f;
            hlg.padding = new RectOffset(50, 50, 0, 0);
            hlg.childControlWidth = true;
            hlg.childControlHeight = true;

            // Ball 탭 (기본 활성화 상태 표출을 위해 밝은 색상 적용)
            Button ballTab = NewButton("Button_Tab_Ball", topGroup.transform, "Ball", out TMP_Text ballTxt);
            ballTab.GetComponent<Image>().color = new Color(1f, 0.85f, 0.6f);
            ballTxt.color = Color.black;
            ballTxt.fontStyle = FontStyles.Bold;

            // Block 탭 (비활성화 상태 표출을 위해 어두운 색상 적용)
            Button blockTab = NewButton("Button_Tab_Block", topGroup.transform, "Block", out TMP_Text blockTxt);
            blockTab.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f);
            blockTxt.color = Color.white;
        }

        private void CreateScrollView(Transform parent)
        {
            GameObject scrollView = NewUI("ScrollView_Items", parent);
            RectTransform scrollRt = scrollView.GetComponent<RectTransform>();
            scrollRt.anchorMin = new Vector2(0, 0);
            scrollRt.anchorMax = new Vector2(1, 1);
            scrollRt.pivot = new Vector2(0.5f, 0.5f);
            // 위쪽 탭 영역과 하단 영역을 피해 마진 설정
            scrollRt.offsetMin = new Vector2(50, 250); 
            scrollRt.offsetMax = new Vector2(-50, -300);

            Image scrollBg = scrollView.AddComponent<Image>();
            scrollBg.color = new Color(0.85f, 0.85f, 0.9f);

            ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
            scrollRect.horizontal = false;
            scrollRect.vertical = true;

            // Viewport
            GameObject viewport = NewUI("Viewport", scrollView.transform);
            viewport.AddComponent<Image>().color = Color.white;
            Mask mask = viewport.AddComponent<Mask>();
            mask.showMaskGraphic = false;
            Stretch(viewport.GetComponent<RectTransform>(), 0f);

            // Content
            GameObject content = NewUI("Content", viewport.transform);
            RectTransform contentRt = content.GetComponent<RectTransform>();
            contentRt.anchorMin = new Vector2(0, 1);
            contentRt.anchorMax = new Vector2(1, 1);
            contentRt.pivot = new Vector2(0.5f, 1);
            contentRt.sizeDelta = new Vector2(0, 0); // ContentSizeFitter가 제어
            contentRt.anchoredPosition = Vector2.zero;

            VerticalLayoutGroup vlg = content.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 30f;
            vlg.padding = new RectOffset(30, 30, 30, 30);
            vlg.childControlWidth = true;
            vlg.childControlHeight = false; // 아이템 고유 높이 사용
            vlg.childForceExpandHeight = false;

            ContentSizeFitter csf = content.AddComponent<ContentSizeFitter>();
            csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            scrollRect.viewport = viewport.GetComponent<RectTransform>();
            scrollRect.content = contentRt;

            // 목업 아이템 5개 생성
            for (int i = 1; i <= 5; i++)
            {
                CreateItemSlot(content.transform, $"Item_Slot_{i}", $"Ball Type {i}\n해당 볼의 특수 능력 및 설명이 이곳에 표시됩니다.");
            }
        }

        private void CreateItemSlot(Transform parent, string name, string descText)
        {
            GameObject slot = NewPanel(name, parent, new Vector2(0, 250)); // LayoutGroup에 의해 Width 결정됨
            slot.GetComponent<Image>().color = Color.white;

            // 좌측 아이콘
            GameObject icon = NewPanel("Image_Icon", slot.transform, new Vector2(180, 180));
            RectTransform iconRt = icon.GetComponent<RectTransform>();
            iconRt.anchorMin = new Vector2(0, 0.5f);
            iconRt.anchorMax = new Vector2(0, 0.5f);
            iconRt.pivot = new Vector2(0, 0.5f);
            iconRt.anchoredPosition = new Vector2(35, 0);
            icon.GetComponent<Image>().color = new Color(0.7f, 0.8f, 0.9f);

            // 우측 설명 텍스트
            TMP_Text desc = NewText("Text_Description", slot.transform, descText, 45f, TextAlignmentOptions.Left);
            desc.rectTransform.anchorMin = new Vector2(0, 0);
            desc.rectTransform.anchorMax = new Vector2(1, 1);
            desc.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            desc.rectTransform.offsetMin = new Vector2(250, 25);
            desc.rectTransform.offsetMax = new Vector2(-25, -25);
            desc.color = new Color(0.2f, 0.2f, 0.2f);
        }

        private void CreateBottomGroup(Transform parent)
        {
            GameObject bottomGroup = NewUI("BottomGroup", parent);
            RectTransform botRt = bottomGroup.GetComponent<RectTransform>();
            botRt.anchorMin = new Vector2(0, 0);
            botRt.anchorMax = new Vector2(1, 0);
            botRt.pivot = new Vector2(0.5f, 0);
            botRt.sizeDelta = new Vector2(0, 250);
            botRt.anchoredPosition = Vector2.zero;

            // 하단 스케치에 위치한 닫기/확인 버튼
            Button confirmBtn = NewButton("Button_Confirm", bottomGroup.transform, "Confirm", out TMP_Text confirmTxt);
            RectTransform btnRt = confirmBtn.GetComponent<RectTransform>();
            btnRt.anchorMin = new Vector2(0.5f, 0.5f);
            btnRt.anchorMax = new Vector2(0.5f, 0.5f);
            btnRt.pivot = new Vector2(0.5f, 0.5f);
            btnRt.sizeDelta = new Vector2(400, 120);
            btnRt.anchoredPosition = Vector2.zero;
            
            confirmBtn.GetComponent<Image>().color = new Color(0.6f, 0.6f, 0.6f);
            confirmTxt.color = Color.white;
        }

        #endregion

        #region Helper Methods

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
            // 기본 패널 생성 후 Button 부착
            GameObject go = NewPanel(name, parent, new Vector2(200, 100)); 
            Button btn = go.AddComponent<Button>();
            
            labelText = NewText("Text_Label", go.transform, label, 45f, TextAlignmentOptions.Center);
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