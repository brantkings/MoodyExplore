using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public enum DamageTeam
{
    Ally,
    Enemy,
    Neutral
}

public class Health : MonoBehaviour {

    [SerializeField]
    private int maxLife = 10;

    [SerializeField]
    [ReadOnly]
    private int _lifeNow;

    public DamageTeam damageFrom;

    public GameObject toDestroy;

    public delegate void DelHealthFeedback(Health health);
    public delegate void DelDamageFeedback(int amount, Health health);
    public event DelDamageFeedback OnDamage;
    public event DelHealthFeedback OnDeath;



    private void Awake()
    {
        _lifeNow = maxLife;
    }

    public bool IsAlive()
    {
        return _lifeNow > 0;
    }

    public virtual bool Damage(int amount, DamageTeam team, GameObject origin = null)
    {
        Debug.LogErrorFormat("Damage {0} with {1},{2}. Can damage? {3}", this, amount, team, CanDamage(team));
        if (CanDamage(team))
        {
            _lifeNow = Mathf.Clamp(_lifeNow - amount, 0, maxLife);
            OnDamage?.Invoke(amount, this);
            Debug.LogErrorFormat("{0} is dead? {1} <= 0 so {2}", this, _lifeNow, _lifeNow <= 0);
            if (_lifeNow <= 0) Die();
            return true;
        }
        return false;
    }

    public void Kill(DamageTeam team = DamageTeam.Neutral, GameObject origin = null)
    {
        Damage(maxLife, team, origin);
    }

    private bool CanDamage(DamageTeam from)
    {
        switch (from)
        {
            case DamageTeam.Neutral:
                return true;
            default:
                return from == damageFrom;
        }
    }

    public void Die()
    {
        Debug.LogErrorFormat("{0} is dead!", this);
        OnDeath?.Invoke(this);
        if (toDestroy == null) toDestroy = gameObject;
        toDestroy.SetActive(false);
        Destroy(toDestroy);
    }
}
