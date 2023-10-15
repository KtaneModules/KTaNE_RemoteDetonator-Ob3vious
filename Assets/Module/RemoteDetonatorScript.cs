using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using UnityEngine;

public class RemoteDetonatorScript : MonoBehaviour
{
    [SerializeField]
    private KMBombInfo _bombInfo;
    [SerializeField]
    private DetonatorSwitch _retryOnDeto;
    [SerializeField]
    private DetonatorSwitch _detoOnSolve;

    void Start()
    {
        _bombInfo.OnBombSolved += () =>
        {
            if (_detoOnSolve.GetState())
                StartCoroutine(DelayDetonate(2f));
        };
        _bombInfo.OnBombExploded += () =>
        {
            if (_retryOnDeto.GetState())
                StartCoroutine(AutoRetry());
        };

        GetComponent<KMSelectable>().OnDefocus += () =>
        {
            DetonatorSwitch.LastFlip = null;
        };

        ModSettings settings = GetSettings();

        _retryOnDeto.SetInitialState(settings.RetryOnDetonation);
        _detoOnSolve.SetInitialState(settings.DetonateOnSolve);
    }

    void OnDestroy()
    {
        UpdateSettings();
    }

    public void TriggerDetonate()
    {
        StartCoroutine(DelayDetonate(1f));
    }

    public IEnumerator DelayDetonate(float time)
    {
        yield return new WaitForSeconds(time);
        Detonate();
    }

    private bool _wantsRetrying = true;
    public IEnumerator AutoRetry()
    {
        if (!_wantsRetrying)
            yield break;
        _wantsRetrying = false;

        Type type = Type.GetType("SceneManager, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

        PropertyInfo currentState = type.GetProperty("CurrentState", BindingFlags.Instance | BindingFlags.Public);
        object instance = type.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static).GetValue(null, new object[0]);

        //because the holdable will no longer be present at the point where you can restart
        new Thread(() =>
        {
            while ((int)currentState.GetValue(instance, new object[0]) != 2)
                Thread.Sleep(8);

            Thread.Sleep(500);

            Retry();
        }).Start();
    }

    public static void Detonate()
    {
        Type recordManager = Type.GetType("Assets.Scripts.Records.RecordManager, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        Type gameRecord = Type.GetType("Assets.Scripts.Records.GameRecord, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");

        object currentRecord = recordManager
            .GetMethod("GetCurrentRecord", BindingFlags.Instance | BindingFlags.Public)
            .Invoke(recordManager
                .GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
                .GetValue(null, new object[0]), new object[0]);

        FieldInfo strikeField = gameRecord.GetField("Strikes", BindingFlags.Public | BindingFlags.Instance);
        IList strikes = (IList)strikeField.GetValue(currentRecord);

        Type strikeSource = Type.GetType("Assets.Scripts.Records.StrikeSource, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        object source = strikeSource.GetConstructors().First().Invoke(new object[0]);
        strikeSource.GetField("ComponentType", BindingFlags.Public | BindingFlags.Instance).SetValue(source, 17);
        strikeSource.GetField("ComponentName", BindingFlags.Public | BindingFlags.Instance).SetValue(source, "Suicidal Intent");
        strikeSource.GetField("InteractionType", BindingFlags.Public | BindingFlags.Instance).SetValue(source, 6);
        strikeSource.GetField("Time", BindingFlags.Public | BindingFlags.Instance).SetValue(source, 0);

        strikes[strikes.Count - 1] = source;

        gameRecord.GetField("Result", BindingFlags.Public | BindingFlags.Instance).SetValue(currentRecord, 1);

        Type bomb = Type.GetType("Bomb, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        bomb.GetMethod("Detonate", BindingFlags.Instance | BindingFlags.Public).Invoke(FindObjectOfType(bomb), new object[0]);
    }

    public static void Retry()
    {
        Type type = Type.GetType("SceneManager, Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null");
        type
            .GetMethod("EnterGameplayState", BindingFlags.Instance | BindingFlags.Public)
            .Invoke(type
                .GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)
                .GetValue(null, new object[0]), new object[] { true });
    }

    public static void Crash()
    {
        Application.Quit();
    }

    public class ModSettings
    {
        public bool RetryOnDetonation = false;
        public bool DetonateOnSolve = false;
    }

    public ModSettings GetSettings()
    {
        KMModSettings settingManager = GetComponent<KMModSettings>();
        try
        {
            ModSettings settings = JsonConvert.DeserializeObject<ModSettings>(settingManager.Settings);
            return settings;
        }
        catch
        {
            ModSettings settings = new ModSettings();
            File.WriteAllText(settingManager.SettingsPath, JsonConvert.SerializeObject(settings));
            return settings;
        }
    }

    public void UpdateSettings()
    {
        KMModSettings settingManager = GetComponent<KMModSettings>();
        ModSettings settings = new ModSettings();
        settings.RetryOnDetonation = _retryOnDeto.GetState();
        settings.DetonateOnSolve = _detoOnSolve.GetState();
        File.WriteAllText(settingManager.SettingsPath, JsonConvert.SerializeObject(settings));
    }
}
