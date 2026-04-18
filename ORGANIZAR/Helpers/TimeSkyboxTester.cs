using UnityEngine;

public class TimeSkyboxTester : MonoBehaviour
{
    [ContextMenu("Definir 08:00")]
    public void SetMorning()
    {
        if (GameTimeSystem.Instance == null) return;
        GameTimeSystem.Instance.SetTime(8, 0);
    }

    [ContextMenu("Definir 14:00")]
    public void SetAfternoon()
    {
        if (GameTimeSystem.Instance == null) return;
        GameTimeSystem.Instance.SetTime(14, 0);
    }

    [ContextMenu("Definir 18:00")]
    public void SetNight()
    {
        if (GameTimeSystem.Instance == null) return;
        GameTimeSystem.Instance.SetTime(18, 0);
    }
}