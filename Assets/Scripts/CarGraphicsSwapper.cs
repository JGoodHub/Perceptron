using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class CarGraphicsSwapper : MonoBehaviour
{

    public enum DefinedColour
    {
        BLACK,
        RED,
        YELLOW,
        GREEN,
        BLUE,
    }

    [Serializable]
    public class ColourSettings
    {
        public DefinedColour definedColour;
        public Sprite sprite;
        public Color lineColour;
    }


    [SerializeField] private SpriteRenderer _targetImage;
    [SerializeField] private LineRenderer _lineRenderer;

    [SerializeField] private ColourSettings[] _colourSettings;

    private List<Vector3> _positionHistory;

    public void SelectColourFromSeed(int seed)
    {
        ColourSettings colourSettings = _colourSettings[seed % _colourSettings.Length];

        _targetImage.sprite = colourSettings.sprite;

        Material lineMat = _lineRenderer.sharedMaterial = new Material(_lineRenderer.sharedMaterial);
        lineMat.color = colourSettings.lineColour;
    }

    public void ResetPositionHistory()
    {
        _positionHistory = new List<Vector3>
        {
            transform.position
        };

        _lineRenderer.positionCount = 1;
        _lineRenderer.SetPositions(_positionHistory.ToArray());
    }

    public void LogPosition()
    {
        _positionHistory.Add(transform.position);

        _lineRenderer.positionCount = _positionHistory.Count;
        _lineRenderer.SetPositions(_positionHistory.ToArray());
    }



}
