using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class DialogueUI : MonoBehaviour, IPointerClickHandler
{
    [Header("Dialogue References")]
    public TMP_Text nameText;
    public TMP_Text dialogueText;
    public Transform choiceContainer;
    public DialogueManager dialogueManager;

    [Header("Visibility")]
    [SerializeField] private CanvasGroup dialogueCanvasGroup;

    // Tab 已被 DimensionSwitcher 使用，默认使用 H
    [SerializeField] private KeyCode toggleVisibilityKey = KeyCode.H;

    private bool isDialogueActive;
    private bool isVisible;

    private void Awake()
    {
        if (dialogueCanvasGroup == null)
            dialogueCanvasGroup = GetComponent<CanvasGroup>();

        if (dialogueCanvasGroup == null)
            dialogueCanvasGroup = gameObject.AddComponent<CanvasGroup>();

        ApplyVisibility(false);
    }

    private void Update()
    {
        if (isDialogueActive &&
            Input.GetKeyDown(toggleVisibilityKey))
        {
            ToggleVisibility();
        }
    }

    // 开始显示对话框
    public void ShowPanel()
    {
        isDialogueActive = true;
        ApplyVisibility(true);
    }

    // 结束对话并隐藏
    public void HidePanel()
    {
        isDialogueActive = false;
        ApplyVisibility(false);
    }

    // 手动切换显示状态
    public void ToggleVisibility()
    {
        if (!isDialogueActive)
            return;

        ApplyVisibility(!isVisible);
    }

    private void ApplyVisibility(bool visible)
    {
        isVisible = visible;

        if (dialogueCanvasGroup == null)
            return;

        dialogueCanvasGroup.alpha = visible ? 1f : 0f;
        dialogueCanvasGroup.interactable = visible;
        dialogueCanvasGroup.blocksRaycasts = visible;
    }

    // 保留旧接口，兼容原有调用
    public void ShowDialogue(string speaker, string text)
    {
        UpdateDialogueContent(speaker, text);
        ShowPanel();
    }

    // 更新文字，但不改变 UI 显示状态
    public void UpdateDialogueContent(string speaker, string text)
    {
        if (nameText != null)
            nameText.text = speaker;

        if (dialogueText != null)
            dialogueText.text = text;
    }

    public void ShowChoices()
    {
        if (choiceContainer != null)
            choiceContainer.gameObject.SetActive(true);
    }

    public void HideChoices()
    {
        if (choiceContainer != null)
            choiceContainer.gameObject.SetActive(false);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDialogueActive || !isVisible)
            return;

        if (dialogueManager == null)
        {
            Debug.LogWarning(
                "DialogueUI: DialogueManager is not assigned."
            );
            return;
        }

        dialogueManager.AdvanceDialogue();
    }
}