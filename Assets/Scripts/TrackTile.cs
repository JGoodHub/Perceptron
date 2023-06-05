using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackTile : MonoBehaviour
{

    [SerializeField] private Vector2 _forwardDirection;
    [SerializeField] private Vector2 _bounds;


    private void OnValidate()
    {
        if (_forwardDirection == Vector2.zero)
            _forwardDirection = Vector2.up;

        _forwardDirection.Normalize();
    }

    public bool IsAlignedWithTrack(Transform target)
    {
        Vector2 localDirection = transform.TransformDirection(_forwardDirection);
        return Vector2.Dot(localDirection, target.up) > -0.25f;
    }

    public bool InsideBounds(Vector2 position)
    {
        Vector2 min = (Vector2)(transform.position) - (_bounds / 2);
        Vector2 max = (Vector2)(transform.position) + (_bounds / 2);

        return position.x > min.x && position.y > min.y &&
            position.x < max.x && position.y < max.y;
    }

    private void OnDrawGizmos()
    {

        return;

        Gizmos.color = Color.cyan;
        Vector2 min = (Vector2)(transform.position) - (_bounds / 2);
        Vector2 max = (Vector2)(transform.position) + (_bounds / 2);

        Gizmos.DrawWireCube(transform.position, _bounds);
        Gizmos.DrawSphere(min, 0.075f);
        Gizmos.DrawSphere(max, 0.075f);


        Gizmos.color = Color.magenta;
        Vector2 localDirection = transform.TransformDirection(_forwardDirection);
        Gizmos.DrawRay(transform.position, localDirection);
        Gizmos.DrawSphere(transform.position, 0.1f);
    }

}
