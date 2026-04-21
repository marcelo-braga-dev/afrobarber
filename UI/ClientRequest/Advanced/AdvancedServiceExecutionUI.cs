using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AdvancedServiceExecutionUI : MonoBehaviour
{
    [SerializeField] private ServiceExecutionSystem executionSystem;
    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text actionNameText;
    [SerializeField] private TMP_Text toolNameText;
    [SerializeField] private Slider progressSlider;
    [SerializeField] private TMP_Text resultText;

    [Header("Fechamento automático")]
    [SerializeField] private float closeDelayAfterFinish = 3f;
    [SerializeField] private bool enableDebugLogs = true;

    private Coroutine closeRoutine;

    private void Awake()
    {
        if (executionSystem == null)
            executionSystem = FindFirstObjectByType<ServiceExecutionSystem>();

        if (root == null)
            root = gameObject;

        Hide();
    }

    private void OnEnable()
    {
        if (executionSystem == null)
            executionSystem = FindFirstObjectByType<ServiceExecutionSystem>();

        if (executionSystem == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[AdvancedServiceExecutionUI] ServiceExecutionSystem não encontrado.");

            return;
        }

        executionSystem.OnActionProgress += HandleProgress;
        executionSystem.OnActionCompleted += HandleCompleted;
    }

    private void OnDisable()
    {
        if (executionSystem == null)
            return;

        executionSystem.OnActionProgress -= HandleProgress;
        executionSystem.OnActionCompleted -= HandleCompleted;
    }

    private void HandleProgress(ServiceActionPlanStep step, float progress)
    {
        Show();

        if (closeRoutine != null)
        {
            StopCoroutine(closeRoutine);
            closeRoutine = null;
        }

        if (actionNameText != null)
            actionNameText.text = $"Ação: {step.actionType}";

        if (toolNameText != null)
            toolNameText.text = string.IsNullOrWhiteSpace(step.productId)
                ? "Ferramenta: nenhuma"
                : $"Ferramenta: {step.productId}";

        if (progressSlider != null)
            progressSlider.value = progress;

        if (resultText != null && progress < 1f)
            resultText.text = "";
    }

    private void HandleCompleted(ServiceActionExecutionResult result)
    {
        Show();

        if (resultText != null)
            resultText.text = $"Resultado: {result.rating} ({result.score:0.0})";
    }

    public void HideAfterDelay()
    {
        HideAfterDelay(closeDelayAfterFinish, null);
    }

    public void HideAfterDelay(float delay)
    {
        HideAfterDelay(delay, null);
    }

    public void HideAfterDelay(float delay, Action onHidden)
    {
        if (enableDebugLogs)
            Debug.Log($"[AdvancedServiceExecutionUI] HideAfterDelay chamado. Fechando em {delay:0.0}s.");

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogWarning("[AdvancedServiceExecutionUI] Este GameObject está desativado. Coloque este script em um objeto ativo, como AdvancedServiceUIRoot.");
            onHidden?.Invoke();
            return;
        }

        if (closeRoutine != null)
            StopCoroutine(closeRoutine);

        closeRoutine = StartCoroutine(HideAfterDelayRoutine(delay, onHidden));
    }

    private IEnumerator HideAfterDelayRoutine(float delay, Action onHidden)
    {
        yield return new WaitForSeconds(delay);

        if (enableDebugLogs)
            Debug.Log("[AdvancedServiceExecutionUI] Fechando painel agora.");

        Hide();

        closeRoutine = null;

        onHidden?.Invoke();
    }

    public void Show()
    {
        if (root != null && !root.activeSelf)
            root.SetActive(true);
    }

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);

        if (progressSlider != null)
            progressSlider.value = 0f;

        if (actionNameText != null)
            actionNameText.text = "";

        if (toolNameText != null)
            toolNameText.text = "";

        if (resultText != null)
            resultText.text = "";
    }
}