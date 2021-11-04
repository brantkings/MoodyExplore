using LHH.Switchable;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BattleLog : Singleton<BattleLog>
{
    private struct LogInstance
    {
        internal RectTransform transf;
        internal Animator anim;
        internal Text text;
        internal Image bg;
        internal Switchable switchable;
    }


    private LinkedList<LogInstance> _instances;
    private bool _createdInstances;
    private LinkedList<LogInstance> Instances
    {
        get
        {
            if (!_createdInstances)
            {
                _instances = new LinkedList<LogInstance>();
                _createdInstances = true;
            }
            return _instances;
        }
    }

    [SerializeField]
    private GameObject prefab;
    [SerializeField]
    private RectTransform logParent;
    [SerializeField]
    private int maxIntances = 8;
    [SerializeField]
    private float durationInstance = 2f;
    [Space]
    public LHH.Unity.ColorValue importantColor;
    public LHH.Unity.ColorValue threateningColor;
    public LHH.Unity.ColorValue rewardColor;
    public LHH.Unity.ColorValue itemColor;
    public LHH.Unity.ColorValue thoughtColor;

    public enum LogType
    {
        Battle,
        Item,
        ThoughtSystem,
        Important
    }

    public LogType[] possibleLogTypes;

    public static void Log(string what, LogType type)
    {
        if(Instance != null)
        {
            Instance.Log(Instance.prefab, what, type);
        }
    }

    private string GetLogColor(LogType type)
    {
        switch (type)
        {
            case LogType.Battle:
                return "#B98888";
            case LogType.Item:
                return "#66B966";
            case LogType.ThoughtSystem:
                return "#7777C9";
            case LogType.Important:
                return "#AAAAAA";
            default:
                return "#767676";
        }
    }

    private void Log(GameObject prefab, string log, LogType type)
    {
        Debug.Log($"<color={GetLogColor(type)}>[{type}] {log}</color>", this);
        if(CanLog(log, type))
        {
            LogInstance newInst = GetNewInstance(prefab);
            newInst.text.text = log;
        }
    }


    public static string Paint(string what, Color c)
    {
        return $"<color=#{c.ToHexStringRGB()}>{what}</color>";
    }

    private bool CanLog(string log, LogType type)
    {
        for(int i = 0,len = possibleLogTypes.Length;i<len;i++)
        {
            if (type == possibleLogTypes[i]) return true;
        }
        return false;
    }


    private LogInstance GetNewInstance(GameObject prefab)
    {
        LogInstance instance = MakeInstance(prefab);
        Instances.AddLast(instance);
        while (Instances.Count > maxIntances)
        {
            DestroyInstance(Instances.First.Value);
            Instances.RemoveFirst();
        }
        OnListChange();
        return instance;
    }

    private LogInstance MakeInstance(GameObject prefab)
    {
        GameObject instance = Instantiate(prefab, logParent);
        instance.transform.localPosition = Vector3.zero;
        LogInstance inst = new LogInstance()
        {
            bg = instance.GetComponentInChildren<Image>(),
            text = instance.GetComponentInChildren<Text>(),
            transf = instance.GetComponentInChildren<RectTransform>(),
            anim = instance.GetComponentInChildren<Animator>(),
            switchable = instance.GetComponentInChildren<Switchable>(),
        };

        StartCoroutine(InstanceRoutine(inst, durationInstance));
        return inst;
    }

    private IEnumerator InstanceRoutine(LogInstance inst, float duration)
    { 
        inst.switchable?.Set(true);
        yield return new WaitForSecondsRealtime(duration);
        if(inst.switchable != null)
        {
            inst.switchable.Set(false);
            yield return new Switchable.WaitForSwitchable(inst.switchable, Switchable.SwitchState.Off);
        }
        DeactivateInstance(inst);
    }

    private void DeactivateInstance(LogInstance inst)
    {
        inst.transf.gameObject.SetActive(false);
    }

    private void DestroyInstance(LogInstance inst)
    {
        Destroy(inst.transf.gameObject);
    }

    private void ReutilizeInstance(LogInstance inst)
    {
        inst.transf.gameObject.SetActive(true);
        Transform parent = inst.transf.parent;
        inst.transf.SetSiblingIndex(0);
    }
    
    private void OnListChange()
    {

    }

}
