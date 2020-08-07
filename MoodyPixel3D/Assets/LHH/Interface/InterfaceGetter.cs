using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;



[System.Serializable]
public abstract class GetterFromGameObjects
{
    [SerializeField]
    private GameObject[] toGetFrom;

    protected GameObject[] Objects
    {
        get
        {
            return toGetFrom;
        }
    }

    public abstract int GetAmountOfLinesToReport();

    public abstract string ReportFromObject();
}

public abstract class SingleInterfaceGetter<T> : GetterFromGameObjects where T:class
{
    protected T _what;

    private bool _got;
    private void Initialize()
    {
        if (!_got)
        {
            _got = true;
            CaptureFromObjects();
        }
    }

    public override string ReportFromObject()
    {
        CaptureFromObjects();
        if (_what != null)
        {
            return ReportObject(_what);
        }
        else return string.Empty;
    }


    public override int GetAmountOfLinesToReport()
    {
        CaptureFromObjects();
        if (_what != null) return 1;
        else return 0;
    }

    protected abstract void CaptureFromObjects();

    public string ReportObject(T obj)
    {
        return obj.ToString();
    }

    public T Interface
    {
        get
        {
            Initialize();
            if (_what == null || _what.Equals(null)) return null;
            else return _what;
        }
    }

    public static implicit operator T(SingleInterfaceGetter<T> instance)
    {
        instance.Initialize();
        return instance._what;
    }

    public override bool Equals(object obj)
    {
        if (obj is SingleInterfaceGetter<T>) return this == (obj as SingleInterfaceGetter<T>);
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(SingleInterfaceGetter<T> person1, SingleInterfaceGetter<T> person2)
    {
        if (ReferenceEquals(person1, null))
        {
            return EqualsNull(person2);
        }
        else if(ReferenceEquals(person2, null))
        {
            return EqualsNull(person1);
        }
        return person1.Equals(person2);
    }

    public static bool operator !=(SingleInterfaceGetter<T> person1, SingleInterfaceGetter<T> person2)
    {
        if (ReferenceEquals(person1, null))
        {
            return !EqualsNull(person2);
        }
        else if (ReferenceEquals(person2, null))
        {
            return !EqualsNull(person1);
        }
        return !person1.Equals(person2);
    }

    public static bool EqualsNull(SingleInterfaceGetter<T> a)
    {
        if (a.Equals(null)) return true;
        a.Initialize();
        return a._what == null || a._what.Equals(null);
    }
}

public class FirstInterfaceGetter<T> : SingleInterfaceGetter<T>  where T:class
{
    protected override void CaptureFromObjects()
    {
        if (Objects != null)
        {
            foreach (var o in Objects)
            {
                _what = o.GetComponent<T>();
                if (_what != null) return;
            }
        }
        else
        {
            _what = null;
        }
    }
}

public class LastInterfaceGetter<T> : SingleInterfaceGetter<T> where T : class
{
    protected override void CaptureFromObjects()
    {
        if (Objects != null)
        {
            foreach (var o in Objects)
            {
                T thing = o.GetComponent<T>();
                if(thing != null)
                    _what = thing;
            }
        }
        else
        {
            _what = null;
        }
    }
}

public class InterfaceGetter<T> : GetterFromGameObjects, IEnumerable<T>
{
    private List<T> interfaces;

    private bool _got;
    private void Initialize()
    {
        if(!_got)
        {
            _got = true;
            CaptureFromObjects();
        }
    }

    private void CaptureFromObjects()
    {
        if(Objects != null)
        {
            interfaces = new List<T>(Objects.Length * 2);
            foreach (GameObject obj in Objects)
            {
                CaptureFromObject(obj, ref interfaces);
            }
        }
        else
        {
            interfaces = new List<T>(0);
        }
    }

    private void CaptureFromObject(GameObject obj, ref List<T> list)
    {
        if (obj != null)
        {
            if (list == null)
            {
                list = new List<T>(obj.GetComponents<T>());
            }
            else
            {
                list.AddRange(obj.GetComponents<T>());
            }
        }
    }

    public IEnumerator<T> GetEnumerator()
    {
        Initialize();
        if (interfaces != null)
            return interfaces.GetEnumerator();
        else return null;
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        Initialize();
        if (interfaces != null)
            return interfaces.GetEnumerator();
        else return null;
    }

    public string ReportObject(T obj)
    {
        return obj.ToString();
    }

    public override string ReportFromObject()
    {
        CaptureFromObjects();
        if (interfaces != null)
        {
            string str = "";
            foreach (T face in this)
            {
                str += ReportObject(face) + '\n';
            }
            return str;
        }
        else return string.Empty;
    }

    public override int GetAmountOfLinesToReport()
    {
        CaptureFromObjects();
        if (interfaces != null) return interfaces.Count;
        else return 0;
    }

    public IEnumerable<T> All
    {
        get
        {
            Initialize();
            return interfaces;
        }
    }

    public T First
    {
        get
        {
            Initialize();
            return interfaces[0];
        }
    }

    public T Last
    {
        get
        {
            Initialize();
            return interfaces[interfaces.Count - 1];
        }
    }

    public int Length
    {
        get
        {
            return interfaces.Count;
        }
    }


    public T this[int index]
    {
        get
        {
            return interfaces[index];
        }
    }
}
