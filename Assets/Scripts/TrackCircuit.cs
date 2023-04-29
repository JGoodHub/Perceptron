using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackCircuit : MonoBehaviour
{
    public static TrackCircuit Instance;

    private void Awake() => Instance = this;

    [SerializeField] private TrackTile[] _trackTiles;

    public bool IsAlignedToTrack(Transform target)
    {
        for (int i = 0; i < _trackTiles.Length; i++)
            if (_trackTiles[i].InsideBounds(target.position))
                return _trackTiles[i].IsAlignedWithTrack(target);

        return false;
    }
    
    public int GetCurrentTrackTile(Transform target)
    {
        for (int i = 0; i < _trackTiles.Length; i++)
            if (_trackTiles[i].InsideBounds(target.position))
                return i;

        return -1;
    }
}
