using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoodHub.Core.Runtime.Curves;
using UnityEngine;

public class TrackCircuit : MonoBehaviour, ITrainingEnvironment
{
    [SerializeField] private Transform _startTransform;
    [SerializeField] private List<TrackTile> _trackTiles;

    private CurvedPath _trackPolyLine;

    public Transform StartTransform => _startTransform;

    private void Awake()
    {
        Vector3[] controlPoints = _trackTiles.Select(tile => tile.transform.position).ToArray();

        _trackPolyLine = new CurvedPath(controlPoints, false);
    }

    public bool IsAlignedToTrack(Transform target)
    {
        for (int i = 0; i < _trackTiles.Count; i++)
            if (_trackTiles[i].InsideBounds(target.position))
                return _trackTiles[i].IsAlignedWithTrack(target);

        return false;
    }

    public int GetCurrentTrackTile(Transform target)
    {
        for (int i = 0; i < _trackTiles.Count; i++)
            if (_trackTiles[i].InsideBounds(target.position))
                return i;

        return -1;
    }

    public float GetDistanceAlongTrack(Transform target)
    {
        return _trackPolyLine.GetDistanceAlongEdge(target.position);
    }

    public List<Vector3> GetTrackPath()
    {
        return _trackTiles.Select(tile => tile.transform.position).ToList();
    }

}