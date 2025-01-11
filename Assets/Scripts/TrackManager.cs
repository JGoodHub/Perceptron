using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoodHub.Core.Runtime;
using UnityEngine;

public class TrackManager : SceneSingleton<TrackManager>
{
    [SerializeField] private List<TrackCircuit> _tracks;

    private int _currentTrackIndex = 0;
    private TrackCircuit _currentTrack;

    public TrackCircuit CurrentTrack => _currentTrack;

    private void Awake()
    {
        _currentTrack = _tracks[_currentTrackIndex];

        foreach (TrackCircuit trackCircuit in _tracks)
        {
            trackCircuit.gameObject.SetActive(trackCircuit == _currentTrack);
        }
    }

    public void IncrementTrack()
    {
        _currentTrack = _tracks[_currentTrackIndex++ % _tracks.Count];

        foreach (TrackCircuit trackCircuit in _tracks)
        {
            trackCircuit.gameObject.SetActive(trackCircuit == _currentTrack);
        }
    }
}