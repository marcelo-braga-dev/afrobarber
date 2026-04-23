using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BarbershopRatingUI : MonoBehaviour
{
    [Header("Painel")]
    [SerializeField] private GameObject panel;

    [Header("Resumo geral")]
    [SerializeField] private TMP_Text globalRatingText;
    [SerializeField] private TMP_Text totalReviewsText;
    [SerializeField] private Slider globalRatingSlider;

    [Header("Avaliações agrupadas")]
    [SerializeField] private TMP_Text attendanceRatingText;
    [SerializeField] private Slider attendanceRatingSlider;

    [SerializeField] private TMP_Text structureRatingText;
    [SerializeField] private Slider structureRatingSlider;

    [SerializeField] private TMP_Text experienceRatingText;
    [SerializeField] private Slider experienceRatingSlider;

    [Header("Comentários")]
    [SerializeField] private TMP_Text globalCommentText;
    [SerializeField] private TMP_Text attendanceCommentText;
    [SerializeField] private TMP_Text structureCommentText;
    [SerializeField] private TMP_Text experienceCommentText;

    [Header("Configuração")]
    [SerializeField] private bool hidePanelOnStart = true;
    [SerializeField] private bool refreshOnStart = true;
    [SerializeField] private bool enableDebugLogs = true;

    private readonly CultureInfo brazilCulture = new CultureInfo("pt-BR");

    private void Awake()
    {
        if (enableDebugLogs)
        {
            Debug.Log("[BarbershopRatingUI] Awake chamado.");
            DebugReferenciasInternas();
        }
    }

    private void Start()
    {
        ConfigureSlider(globalRatingSlider);
        ConfigureSlider(attendanceRatingSlider);
        ConfigureSlider(structureRatingSlider);
        ConfigureSlider(experienceRatingSlider);

        if (hidePanelOnStart && panel != null)
            panel.SetActive(false);

        if (refreshOnStart)
            RefreshUI();
    }

    private void OnEnable()
    {
        if (BarbershopRatingManager.Instance != null)
        {
            BarbershopRatingManager.Instance.OnRatingChanged.AddListener(OnRatingChanged);
            RefreshUI();
        }
        else if (enableDebugLogs)
        {
            Debug.LogWarning("[BarbershopRatingUI] OnEnable: BarbershopRatingManager.Instance não encontrado.");
        }
    }

    private void OnDisable()
    {
        if (BarbershopRatingManager.Instance != null)
            BarbershopRatingManager.Instance.OnRatingChanged.RemoveListener(OnRatingChanged);
    }

    private void OnRatingChanged(float newRating)
    {
        if (enableDebugLogs)
            Debug.Log($"[BarbershopRatingUI] OnRatingChanged: {newRating:0.0}");

        RefreshUI();
    }

    public void Show()
    {
        if (panel != null)
            panel.SetActive(true);

        RefreshUI();
    }

    public void Hide()
    {
        if (panel != null)
            panel.SetActive(false);
    }

    public void Toggle()
    {
        if (panel == null)
        {
            RefreshUI();
            return;
        }

        bool newState = !panel.activeSelf;
        panel.SetActive(newState);

        if (newState)
            RefreshUI();
    }

    public void RefreshUI()
    {
        if (BarbershopRatingManager.Instance == null)
        {
            if (enableDebugLogs)
                Debug.LogWarning("[BarbershopRatingUI] RefreshUI: BarbershopRatingManager.Instance não encontrado.");

            SetEmptyState();
            return;
        }

        BarbershopRatingManager rating = BarbershopRatingManager.Instance;

        float global = Mathf.Clamp(rating.GlobalRating, 0f, 5f);
        float attendance = Mathf.Clamp(rating.AttendanceRating, 0f, 5f);
        float structure = Mathf.Clamp(rating.StructureRating, 0f, 5f);
        float experience = Mathf.Clamp(rating.ExperienceRating, 0f, 5f);

        if (enableDebugLogs)
        {
            Debug.Log(
                $"[BarbershopRatingUI] RefreshUI | " +
                $"Global={global:0.0} | Atendimento={attendance:0.0} | Estrutura={structure:0.0} | Experiência={experience:0.0}"
            );
        }

        SetText(globalRatingText, $"{Format(global)}/5");
        SetText(totalReviewsText, BuildTotalReviewsText(rating.TotalReviews));

        SetText(attendanceRatingText, $"{Format(attendance)}/5");
        SetText(structureRatingText, $"{Format(structure)}/5");
        SetText(experienceRatingText, $"{Format(experience)}/5");

        SetSlider(globalRatingSlider, global);
        SetSlider(attendanceRatingSlider, attendance);
        SetSlider(structureRatingSlider, structure);
        SetSlider(experienceRatingSlider, experience);

        SetText(globalCommentText, GetGlobalComment(global, rating.TotalReviews));
        SetText(attendanceCommentText, GetAttendanceComment(attendance));
        SetText(structureCommentText, GetStructureComment(structure));
        SetText(experienceCommentText, GetExperienceComment(experience));
    }

    private void SetEmptyState()
    {
        SetText(globalRatingText, "0/5");
        SetText(totalReviewsText, "0 avaliações");

        SetText(attendanceRatingText, "0/5");
        SetText(structureRatingText, "0/5");
        SetText(experienceRatingText, "0/5");

        SetSlider(globalRatingSlider, 0f);
        SetSlider(attendanceRatingSlider, 0f);
        SetSlider(structureRatingSlider, 0f);
        SetSlider(experienceRatingSlider, 0f);

        SetText(globalCommentText, "Nenhuma avaliação disponível.");
        SetText(attendanceCommentText, "Sem dados de atendimento.");
        SetText(structureCommentText, "Sem dados de estrutura.");
        SetText(experienceCommentText, "Sem dados de experiência.");
    }

    private string BuildTotalReviewsText(int totalReviews)
    {
        if (totalReviews == 1)
            return "1 avaliação";

        return $"{totalReviews} avaliações";
    }

    private string GetGlobalComment(float score, int totalReviews)
    {
        if (totalReviews <= 0)
            return "A barbearia ainda não recebeu avaliações.";

        if (score >= 4.5f) return "Reputação excelente. A barbearia está se destacando.";
        if (score >= 3.5f) return "Boa reputação. Os clientes estão satisfeitos.";
        if (score >= 2.5f) return "Reputação razoável. Ainda há pontos para melhorar.";
        if (score >= 1.5f) return "Reputação baixa. Os clientes estão percebendo problemas.";
        return "Reputação muito baixa. A barbearia precisa de melhorias urgentes.";
    }

    private string GetAttendanceComment(float score)
    {
        if (score >= 4.5f) return "Atendimento excelente.";
        if (score >= 3.5f) return "Atendimento bom.";
        if (score >= 2.5f) return "Atendimento razoável.";
        if (score >= 1.5f) return "Atendimento precisa melhorar.";
        return "Atendimento ruim.";
    }

    private string GetStructureComment(float score)
    {
        if (score >= 4.5f) return "Estrutura excelente, confortável e estilosa.";
        if (score >= 3.5f) return "Boa estrutura da barbearia.";
        if (score >= 2.5f) return "Estrutura aceitável, mas pode evoluir.";
        if (score >= 1.5f) return "Estrutura fraca para os clientes.";
        return "Estrutura muito ruim.";
    }

    private string GetExperienceComment(float score)
    {
        if (score >= 4.5f) return "Experiência muito agradável e memorável.";
        if (score >= 3.5f) return "Boa experiência geral.";
        if (score >= 2.5f) return "Experiência mediana.";
        if (score >= 1.5f) return "Experiência desconfortável.";
        return "Experiência ruim.";
    }

    [ContextMenu("Debug Referencias RatingText")]
    private void DebugReferenciasRatingText()
    {
        Debug.Log($"globalRatingText: {(globalRatingText != null ? globalRatingText.name : "NULL")}");
        Debug.Log($"attendanceRatingText: {(attendanceRatingText != null ? attendanceRatingText.name : "NULL")}");
        Debug.Log($"structureRatingText: {(structureRatingText != null ? structureRatingText.name : "NULL")}");
        Debug.Log($"experienceRatingText: {(experienceRatingText != null ? experienceRatingText.name : "NULL")}");
    }

    private void DebugReferenciasInternas()
    {
        Debug.Log($"globalRatingText => {(globalRatingText != null ? globalRatingText.name : "NULL")}");
        Debug.Log($"attendanceRatingText => {(attendanceRatingText != null ? attendanceRatingText.name : "NULL")}");
        Debug.Log($"structureRatingText => {(structureRatingText != null ? structureRatingText.name : "NULL")}");
        Debug.Log($"experienceRatingText => {(experienceRatingText != null ? experienceRatingText.name : "NULL")}");
        Debug.Log($"totalReviewsText => {(totalReviewsText != null ? totalReviewsText.name : "NULL")}");
    }

    private void ConfigureSlider(Slider slider)
    {
        if (slider == null)
            return;

        slider.minValue = 0f;
        slider.maxValue = 5f;
        slider.wholeNumbers = false;
        slider.interactable = false;
    }

    private void SetSlider(Slider slider, float value)
    {
        if (slider == null)
            return;

        slider.value = Mathf.Clamp(value, 0f, 5f);
    }

    private void SetText(TMP_Text target, string value)
    {
        if (target == null)
            return;

        target.text = value;
    }

    private string Format(float value)
    {
        return value.ToString("0.#", brazilCulture);
    }
}