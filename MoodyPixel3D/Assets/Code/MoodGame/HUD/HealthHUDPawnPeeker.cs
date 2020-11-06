using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HealthHUDPawnPeeker : MonoBehaviour, IMoodPawnPeeker
{
    private Health _health;
    [SerializeField]
    private HealthHUDObject prefab;

    private int _maxHealth;
    private int _currentHealth;
    public int unitAmount = 1;
    private List<HealthHUDObject> _children;

    private void Start()
    {
        foreach (Transform t in transform) 
            Destroy(t.gameObject);
        CheckMaxHealth(_health.MaxLife);
        CheckHealth(_health.Life, false);
    }

    public void SetTarget(MoodPawn pawn)
    {
        _health = pawn.GetComponentInChildren<Health>();
        if(_health != null)
        {
            _health.OnDamage += OnDamage;
            _health.OnDeath += OnDeath;
        }
    }

    public void UnsetTarget(MoodPawn pawn)
    {
        if(_health != null)
        {
            _health.OnDamage -= OnDamage;
            _health.OnDeath -= OnDeath;
        }
        _health = null;
    }

    private void OnDamage(DamageInfo damage, Health damaged)
    {
        Debug.LogWarningFormat("{0} took notice that {1} took damage {2}", this, damaged, damage);
        CheckHealth(damaged.Life, true);
    }

    private void OnDeath(DamageInfo damage, Health damaged)
    {
        CheckHealth(0, true);
    }

    private void ChangeProportional(ref int num)
    {
        if (unitAmount != 0) num /= unitAmount;
    }

    private void CheckMaxHealth(int max)
    {
        ChangeProportional(ref max);
        if (max != _maxHealth)
        {
            if (_children == null) _children = new List<HealthHUDObject>(max);
            while(max > _maxHealth)
            {
                HealthHUDObject obj = Instantiate(prefab, transform);
                _children.Add(obj);
                _maxHealth++;
            }
            while(max < _maxHealth)
            {
                Destroy(_children.Last());
                _maxHealth--;
            }
        }
    }

    private void CheckHealth(int current,  bool feedback)
    {
        ChangeProportional(ref current);
        if (_currentHealth != current)
        {
            for (int i = 0, len = _children.Count; i < len; i++) 
            {
                _children[i].SetDamaged((i+1) > current, feedback);
                Debug.LogFormat("Setting {0} damaged? {1}", i, (i + 1) > current);
            }

            _currentHealth = current;
        }
    }
}
