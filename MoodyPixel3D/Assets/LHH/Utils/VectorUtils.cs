using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class VectorUtils
{

    #region Random
    /// <summary>
    /// Get a random Vector3 between -vec and vec.
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector3 RandomRange(this Vector3 vec)
    {
        return new Vector3(
            Random.Range(-vec.x, vec.x), 
            Random.Range(-vec.y, vec.y), 
            Random.Range(-vec.z, vec.z)
            );
    }

    /// <summary>
    /// Get a random Vector3 between 0 and vec.
    /// </summary>
    /// <param name="vec"></param>
    /// <returns></returns>
    public static Vector3 RandomRangeSingle(this Vector3 vec)
    {
        return new Vector3(
            Random.Range(0f, vec.x),
            Random.Range(0f, vec.y),
            Random.Range(0f, vec.z)
            );
    }

    public static Vector2 RandomRange(this Vector2 vec)
    {
        return new Vector2(
            Random.Range(-vec.x, vec.x),
            Random.Range(-vec.y, vec.y)
            );
    }

    public static Vector2 RandomRangeSingle(this Vector2 vec)
    {
        return new Vector2(
            Random.Range(0f, vec.x),
            Random.Range(0f, vec.y)
        );
    }

    #endregion

    #region Mathf
    public static Vector2 Clamp(this Vector2 v, float min, float max)
    {
        float mag = v.sqrMagnitude;
        if (mag < (min*min)) return v.normalized * min;
        else if (mag > (max*max)) return v.normalized * max;
        return v;
    }

    public static Vector2 Clamp(this Vector2 v, float max)
    {
        float mag = v.sqrMagnitude;
        if (mag > (max * max)) return v.normalized * max;
        return v;
    }

    public static Vector3 Clamp(this Vector3 v, float min, float max)
    {
        float mag = v.sqrMagnitude;
        if (mag < (min * min)) return v.normalized * min;
        else if (mag > (max * max)) return v.normalized * max;
        return v;
    }

    public static Vector3 Clamp(this Vector3 v, float max)
    {
        float mag = v.sqrMagnitude;
        if (mag > (max * max)) return v.normalized * max;
        return v;
    }

    public static Vector3 Abs(this Vector3 v)
    {
        return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z));
    }

    public static Vector2 Abs(this Vector2 v)
    {
        return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y));
    }

    #endregion

}
