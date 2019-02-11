using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdvancedTransform : MonoBehaviour
{
#if UNITY_EDITOR

    private Transform _transform;

    [Header("Position")]
    public Vector3 m_localPosition = Vector3.zero;
    public Vector3 m_globalPosition = Vector3.zero;

    [Header("Rotation")]
    public Vector3 m_localRotation = Vector3.zero;
    public Vector3 m_globalRotation = Vector3.zero;

    private void Start()
    {
        _transform = transform;

    }
    private void Update()
    {
        if (Application.isPlaying)
        {
            m_localPosition = _transform.localPosition;
            m_globalPosition = _transform.position;

            m_localRotation = _transform.localRotation.eulerAngles;
            m_globalRotation = _transform.rotation.eulerAngles;
        }
    }
    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            m_localPosition = transform.localPosition;
            m_globalPosition = transform.position;

            m_localRotation = transform.localRotation.eulerAngles;
            m_globalRotation = transform.rotation.eulerAngles;

        }
    }

#endif

}