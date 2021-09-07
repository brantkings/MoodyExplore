using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LHH.Caster;

[ExecuteInEditMode]
public class FollowPlatformerGroundedPosition : MonoBehaviour
{
    const float VERY_BIG_VALUE = 1000000f;
    const float VERY_SMALL_VALUE = 0.00001f;

    public Transform mainPosition;
    public Transform groundedPosition;
    public KinematicPlatformer platformer;
    public Caster groundCaster;
    public float timeGroundedToLerp = 0.25f;

    [Space()]
    public float durationToY = 0.45f;
    public float delayToStartChangingY;
    public float offsetY;
#if UNITY_EDITOR
    [Space()]
    [SerializeField]
    [ReadOnly]
#endif

    private float _currentY;

#if UNITY_EDITOR
    [SerializeField]
    [ReadOnly]
#endif
    private float _targetY;
    private float _lastYDelta;
#if UNITY_EDITOR
    [SerializeField]
    [ReadOnly]
#endif
    private float _groundedTimeCounter;


    public Vector3 LocalFeetPosition
    {
        get
        {
            return groundedPosition.position - mainPosition.position;
        }
    }

    private void OnEnable()
    {
        platformer.Grounded.OnChanged += OnGroundedChange;
    }

    private void OnDisable()
    {
        platformer.Grounded.OnChanged -= OnGroundedChange;
    }

    private void OnGroundedChange(bool change)
    {
        if (change == true)
            StartCoroutine(OnGroundedRoutine());
    }

    private bool Cast(out RaycastHit hit, float range = VERY_BIG_VALUE)
    {
        return groundCaster.CastLength(-Vector3.up, VERY_BIG_VALUE, out hit);
    }

    private IEnumerator OnGroundedRoutine()
    {
        if(Cast(out RaycastHit hit))
        {
            yield return new WaitForSeconds(timeGroundedToLerp);
            _targetY = hit.point.y;
        }
    }

    public void Start()
    {
        if (Cast(out RaycastHit hit))
        {
            _currentY = hit.point.y - LocalFeetPosition.y;
        }
    }

    public void LateUpdate()
    {
        Vector3 pos = transform.position;
        if(platformer.Grounded && Cast(out RaycastHit hit, 2f))
        {
            _targetY = hit.point.y;
        }
        UpdateY(_targetY);
        pos.y = GetCurrentY();
        transform.position = pos;
    }

    private float GetCurrentY()
    {
        return _currentY + offsetY;
    }

    private void UpdateY(float destination)
    {
        if (Mathf.Abs(_currentY - destination) < VERY_SMALL_VALUE)
            SetY(destination);
        _currentY = Mathf.SmoothDamp(_currentY, destination, ref _lastYDelta, durationToY);
    }

    private void SetY(float destination)
    {
        _currentY = destination;
        _lastYDelta = 0f;
    }
}
