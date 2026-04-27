using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServiceExecutionSystem : MonoBehaviour
{
    [SerializeField] private bool useRealSecondsForDemo = true;
    [SerializeField] private float secondsPerGameMinute = 1f;
    [SerializeField] private ServiceAudioController audioController;

    public event Action<ServiceActionExecutionResult> OnActionCompleted;
    public event Action<ServiceActionPlanStep, float> OnActionProgress;

    private Coroutine routine;

    public bool IsRunning => routine != null;

    private void Awake()
    {
        if (audioController == null)
            audioController = GetComponent<ServiceAudioController>();
    }

    public void StartExecution(ServicePlanData plan, ClientNPC client, Action<List<ServiceActionExecutionResult>, float> onFinish)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(ExecuteRoutine(plan, client, onFinish));
    }

    private IEnumerator ExecuteRoutine(ServicePlanData plan, ClientNPC client, Action<List<ServiceActionExecutionResult>, float> onFinish)
    {
        List<ServiceActionExecutionResult> results = new List<ServiceActionExecutionResult>();
        float totalMinutes = 0f;

        if (plan != null)
        {
            foreach (ServiceActionPlanStep step in plan.steps)
            {
                audioController?.PlayStepAudio(step);

                float gameMinutes = Mathf.Max(0.2f, step.estimatedMinutes);
                float durationSeconds = useRealSecondsForDemo ? gameMinutes * Mathf.Max(0.1f, secondsPerGameMinute) : gameMinutes;

                float elapsed = 0f;
                while (elapsed < durationSeconds)
                {
                    elapsed += Time.deltaTime;
                    float progress = Mathf.Clamp01(elapsed / durationSeconds);
                    OnActionProgress?.Invoke(step, progress);
                    yield return null;
                }

                audioController?.StopAudio();

                float actualMinutes = gameMinutes;
                totalMinutes += actualMinutes;

                ServiceActionExecutionResult result = ServiceActionEvaluationSystem.EvaluateStep(step, client, actualMinutes);
                results.Add(result);
                OnActionCompleted?.Invoke(result);
            }
        }

        routine = null;
        onFinish?.Invoke(results, totalMinutes);
    }
}
