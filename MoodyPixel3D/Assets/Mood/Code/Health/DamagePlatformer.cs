using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamagePlatformer : Damage
{
    private static Dictionary<Health, IPlatformer> _cache = new Dictionary<Health, IPlatformer>(5);

    private Health _healthCache;
    private IPlatformer _platformer;
    [Space()]
    public bool hitWhenGrounded = true;
    public bool hitWhenAerial = false;
    public bool hitWhenNeither = true;
    public bool hitWhenNull = true;

    public override void DealDamage(Health health)
    {
        if (CanDamage(GetFrom(health)))
            base.DealDamage(health);
    }

    IPlatformer GetFrom(Health health)
    {
        if (health != _healthCache)
        {
            _healthCache = health;
            _platformer = GetPlatformerFrom(_healthCache);
        }
        return _platformer;
    }

    IPlatformer GetPlatformerFrom(Health health)
    {
        if (!_cache.ContainsKey(health))
        {
            _cache.Add(health, health.GetComponentInParent<IPlatformer>());
        }

        return _cache[health];
    }

    bool CanDamage(IPlatformer plat)
    {
        if (plat == null || plat.Equals(null)) return hitWhenNull;
        switch (plat.IsGrounded())
        {
            case GroundedState.Neither:
                return hitWhenNeither;
            case GroundedState.Grounded:
                return hitWhenGrounded;
            case GroundedState.Aerial:
                return hitWhenAerial;
            default:
                return hitWhenNull;
        }

    }
}
