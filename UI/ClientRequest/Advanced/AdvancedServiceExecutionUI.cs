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

    [Header("Textos principais")]
    [SerializeField] private string actionLabelPrefix = "Ação";
    [SerializeField] private string toolLabelPrefix = "Ferramenta";
    [SerializeField] private string resultLabelPrefix = "Resultado";
    [SerializeField] private string labelSeparator = ": ";
    [SerializeField] private string emptyToolText = "nenhuma";

    [Header("Exibir prefixos")]
    [SerializeField] private bool showActionPrefix = true;
    [SerializeField] private bool showToolPrefix = true;
    [SerializeField] private bool showResultPrefix = true;

    [Header("Nomes das ações")]
    [SerializeField] private string washActionText = "Lavar";
    [SerializeField] private string combActionText = "Pentear";
    [SerializeField] private string cutActionText = "Cortar";
    [SerializeField] private string razorActionText = "Navalha";
    [SerializeField] private string finishActionText = "Acabamento";
    [SerializeField] private string defineActionText = "Definir";
    [SerializeField] private string beardActionText = "Barba";
    [SerializeField] private string finalizeActionText = "Finalizar";

    [Header("Nomes das avaliações por etapa")]
    [SerializeField] private string horribleRatingText = "Horrível";
    [SerializeField] private string badRatingText = "Ruim";
    [SerializeField] private string mediumRatingText = "Mais ou menos";
    [SerializeField] private string goodRatingText = "Bom";
    [SerializeField] private string greatRatingText = "Ótimo";
    [SerializeField] private string perfectRatingText = "Perfeito";

    [Header("Formato do resultado")]
    [SerializeField] private bool showResultScore = true;
    [SerializeField] private string scoreOpenText = " (";
    [SerializeField] private string scoreCloseText = ")";
    [SerializeField] private string scoreFormat = "0.0";

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
            actionNameText.text = BuildLabel(showActionPrefix, actionLabelPrefix, GetActionDisplayName(step.actionType));

        if (toolNameText != null)
            toolNameText.text = BuildLabel(showToolPrefix, toolLabelPrefix, GetToolDisplayName(step));

        if (progressSlider != null)
            progressSlider.value = progress;

        if (resultText != null && progress < 1f)
            resultText.text = "";
    }

    private void HandleCompleted(ServiceActionExecutionResult result)
    {
        Show();

        if (resultText != null)
            resultText.text = BuildResultText(result);
    }

    private string BuildResultText(ServiceActionExecutionResult result)
    {
        if (result == null)
            return "";

        string ratingText = GetRatingDisplayName(result.rating);
        string value = ratingText;

        if (showResultScore)
            value += $"{scoreOpenText}{result.score.ToString(scoreFormat)}{scoreCloseText}";

        return BuildLabel(showResultPrefix, resultLabelPrefix, value);
    }

    private string BuildLabel(bool showPrefix, string prefix, string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            value = "";

        if (!showPrefix || string.IsNullOrWhiteSpace(prefix))
            return value;

        return $"{prefix}{labelSeparator}{value}";
    }

    private string GetToolDisplayName(ServiceActionPlanStep step)
    {
        if (step == null)
            return emptyToolText;

        if (string.IsNullOrWhiteSpace(step.productId))
            return emptyToolText;

        if (InventoryManager.Instance != null)
        {
            ProductData product = InventoryManager.Instance.GetProductDataById(step.productId);

            if (product != null && !string.IsNullOrWhiteSpace(product.productName))
                return product.productName;
        }

        return step.productId;
    }

    private string GetActionDisplayName(ServiceActionType actionType)
    {
        return actionType switch
        {
            ServiceActionType.Wash => washActionText,
            ServiceActionType.Comb => combActionText,
            ServiceActionType.Cut => cutActionText,
            ServiceActionType.Razor => razorActionText,
            ServiceActionType.Finish => finishActionText,
            ServiceActionType.Define => defineActionText,
            ServiceActionType.Beard => beardActionText,
            ServiceActionType.Finalize => finalizeActionText,
            _ => actionType.ToString()
        };
    }

    private string GetRatingDisplayName(ServiceActionRating rating)
    {
        return rating switch
        {
            ServiceActionRating.Horrivel => horribleRatingText,
            ServiceActionRating.Ruim => badRatingText,
            ServiceActionRating.MaisOuMenos => mediumRatingText,
            ServiceActionRating.Bom => goodRatingText,
            ServiceActionRating.Otimo => greatRatingText,
            ServiceActionRating.Perfeito => perfectRatingText,
            _ => rating.ToString()
        };
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