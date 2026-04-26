using UnityEngine;

public class BarbershopManagementSaveSystem : MonoBehaviour
{
    private const string KeyOpeningHour = "AFROBARBER_MANAGEMENT_OPENING_HOUR";
    private const string KeyOpeningMinute = "AFROBARBER_MANAGEMENT_OPENING_MINUTE";
    private const string KeyClosingHour = "AFROBARBER_MANAGEMENT_CLOSING_HOUR";
    private const string KeyClosingMinute = "AFROBARBER_MANAGEMENT_CLOSING_MINUTE";

    private const string KeyMonday = "AFROBARBER_MANAGEMENT_MONDAY";
    private const string KeyTuesday = "AFROBARBER_MANAGEMENT_TUESDAY";
    private const string KeyWednesday = "AFROBARBER_MANAGEMENT_WEDNESDAY";
    private const string KeyThursday = "AFROBARBER_MANAGEMENT_THURSDAY";
    private const string KeyFriday = "AFROBARBER_MANAGEMENT_FRIDAY";
    private const string KeySaturday = "AFROBARBER_MANAGEMENT_SATURDAY";
    private const string KeySunday = "AFROBARBER_MANAGEMENT_SUNDAY";

    private const string KeyAdjustmentPrefix = "AFROBARBER_MANAGEMENT_PRICE_ADJUSTMENT_";

    [Header("Auto save/load")]
    [SerializeField] private bool loadOnStart = true;
    [SerializeField] private bool saveOnApplicationPause = true;
    [SerializeField] private bool saveOnApplicationQuit = true;

    private void Start()
    {
        if (loadOnStart)
            Load();
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause && saveOnApplicationPause)
            Save();
    }

    private void OnApplicationQuit()
    {
        if (saveOnApplicationQuit)
            Save();
    }

    public void Save()
    {
        if (GlobalGameplayManagement.Instance == null)
            return;

        GlobalGameplayManagement management = GlobalGameplayManagement.Instance;

        PlayerPrefs.SetInt(KeyOpeningHour, management.GetOpeningHour());
        PlayerPrefs.SetInt(KeyOpeningMinute, management.GetOpeningMinute());
        PlayerPrefs.SetInt(KeyClosingHour, management.GetClosingHour());
        PlayerPrefs.SetInt(KeyClosingMinute, management.GetClosingMinute());

        PlayerPrefs.SetInt(KeyMonday, BoolToInt(management.GetSuggestedMonday()));
        PlayerPrefs.SetInt(KeyTuesday, BoolToInt(management.GetSuggestedTuesday()));
        PlayerPrefs.SetInt(KeyWednesday, BoolToInt(management.GetSuggestedWednesday()));
        PlayerPrefs.SetInt(KeyThursday, BoolToInt(management.GetSuggestedThursday()));
        PlayerPrefs.SetInt(KeyFriday, BoolToInt(management.GetSuggestedFriday()));
        PlayerPrefs.SetInt(KeySaturday, BoolToInt(management.GetSuggestedSaturday()));
        PlayerPrefs.SetInt(KeySunday, BoolToInt(management.GetSuggestedSunday()));

        ServiceType[] allTypes = (ServiceType[])System.Enum.GetValues(typeof(ServiceType));
        for (int i = 0; i < allTypes.Length; i++)
        {
            ServiceType serviceType = allTypes[i];
            PlayerPrefs.SetFloat(GetAdjustmentKey(serviceType), management.GetAdjustmentForService(serviceType));
        }

        PlayerPrefs.Save();
    }

    public void Load()
    {
        if (GlobalGameplayManagement.Instance == null)
            return;

        GlobalGameplayManagement management = GlobalGameplayManagement.Instance;

        if (PlayerPrefs.HasKey(KeyOpeningHour) &&
            PlayerPrefs.HasKey(KeyOpeningMinute) &&
            PlayerPrefs.HasKey(KeyClosingHour) &&
            PlayerPrefs.HasKey(KeyClosingMinute))
        {
            management.SetSuggestedBusinessHours(
                PlayerPrefs.GetInt(KeyOpeningHour, management.GetOpeningHour()),
                PlayerPrefs.GetInt(KeyOpeningMinute, management.GetOpeningMinute()),
                PlayerPrefs.GetInt(KeyClosingHour, management.GetClosingHour()),
                PlayerPrefs.GetInt(KeyClosingMinute, management.GetClosingMinute())
            );
        }

        bool monday = GetSavedBoolOrDefault(KeyMonday, management.GetSuggestedMonday());
        bool tuesday = GetSavedBoolOrDefault(KeyTuesday, management.GetSuggestedTuesday());
        bool wednesday = GetSavedBoolOrDefault(KeyWednesday, management.GetSuggestedWednesday());
        bool thursday = GetSavedBoolOrDefault(KeyThursday, management.GetSuggestedThursday());
        bool friday = GetSavedBoolOrDefault(KeyFriday, management.GetSuggestedFriday());
        bool saturday = GetSavedBoolOrDefault(KeySaturday, management.GetSuggestedSaturday());
        bool sunday = GetSavedBoolOrDefault(KeySunday, management.GetSuggestedSunday());

        management.SetSuggestedWorkDays(monday, tuesday, wednesday, thursday, friday, saturday, sunday);

        ServiceType[] allTypes = (ServiceType[])System.Enum.GetValues(typeof(ServiceType));
        for (int i = 0; i < allTypes.Length; i++)
        {
            ServiceType serviceType = allTypes[i];
            string key = GetAdjustmentKey(serviceType);
            if (!PlayerPrefs.HasKey(key))
                continue;

            management.SetAdjustmentForService(serviceType, PlayerPrefs.GetFloat(key, 0f));
        }

        management.ForceNotifyBusinessSettingsChanged();
    }

    private static string GetAdjustmentKey(ServiceType serviceType)
    {
        return KeyAdjustmentPrefix + serviceType;
    }

    private static int BoolToInt(bool value)
    {
        return value ? 1 : 0;
    }

    private static bool GetSavedBoolOrDefault(string key, bool defaultValue)
    {
        if (!PlayerPrefs.HasKey(key))
            return defaultValue;

        return PlayerPrefs.GetInt(key, BoolToInt(defaultValue)) == 1;
    }
}