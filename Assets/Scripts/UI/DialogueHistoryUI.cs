using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DialogueHistoryUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private DialogueHistoryManager historyManager;

    [SerializeField]
    private GameObject historyPanel;

    [SerializeField]
    private Button openHistoryButton;

    [SerializeField]
    private Button closeHistoryButton;

    [SerializeField]
    private ScrollRect scrollRect;

    [SerializeField]
    private RectTransform contentRoot;

    [SerializeField]
    private DialogueHistoryItemUI entryPrefab;


    private void Awake()
    {
        if (historyPanel != null)
        {
            historyPanel.SetActive(false);
        }

        if (openHistoryButton != null)
        {
            openHistoryButton.onClick.AddListener(
                OpenHistory
            );
        }

        if (closeHistoryButton != null)
        {
            closeHistoryButton.onClick.AddListener(
                CloseHistory
            );
        }
    }


    private void OnEnable()
    {
        if (historyManager != null)
        {
            historyManager.HistoryChanged +=
                HandleHistoryChanged;
        }
    }


    private void OnDisable()
    {
        if (historyManager != null)
        {
            historyManager.HistoryChanged -=
                HandleHistoryChanged;
        }
    }


    private void OnDestroy()
    {
        if (openHistoryButton != null)
        {
            openHistoryButton.onClick.RemoveListener(
                OpenHistory
            );
        }

        if (closeHistoryButton != null)
        {
            closeHistoryButton.onClick.RemoveListener(
                CloseHistory
            );
        }
    }


    // 打开历史记录
    public void OpenHistory()
    {
        if (historyPanel == null)
            return;

        historyPanel.SetActive(true);

        RefreshHistory();

        ScrollToBottom();
    }


    // 关闭历史记录
    public void CloseHistory()
    {
        if (historyPanel != null)
        {
            historyPanel.SetActive(false);
        }
    }


    // 更新历史记录显示
    public void RefreshHistory()
    {
        Debug.Log(
    $"[History UI] Refreshing. Entries: {historyManager.Entries.Count}"
);

        if (historyManager == null ||
            contentRoot == null ||
            entryPrefab == null)
        {
            Debug.LogWarning(
                "DialogueHistoryUI: References are missing."
            );

            return;
        }

        // 清除旧 UI
        for (int i = contentRoot.childCount - 1;
             i >= 0;
             i--)
        {
            Destroy(contentRoot.GetChild(i).gameObject);
        }

        // 重新生成历史记录
        foreach (DialogueHistoryEntry entry
                 in historyManager.Entries)
        {
            DialogueHistoryItemUI item =
                Instantiate(
                    entryPrefab,
                    contentRoot
                );

            item.Setup(entry);
        }

        Canvas.ForceUpdateCanvases();

        if (contentRoot is RectTransform rect)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
        }
    }


    private void HandleHistoryChanged()
    {
        if (historyPanel != null &&
            historyPanel.activeSelf)
        {
            RefreshHistory();

            ScrollToBottom();
        }
    }


    private void ScrollToBottom()
    {
        if (scrollRect == null)
            return;

        Canvas.ForceUpdateCanvases();

        scrollRect.verticalNormalizedPosition = 0f;
    }
}