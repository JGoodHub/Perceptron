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


    [SerializeField] private Transform _root;
    
    [SerializeField] private SpriteRenderer _targetImage;
    [SerializeField] private LineRenderer _lineRenderer;

    [SerializeField] private GameObject _accelerationLight;
    [SerializeField] private GameObject _brakeLights;

    [SerializeField] private ColourSettings[] _colourSettings;

    private List<Vector3> _positionHistory;

    public void SetDepth(int depth)
    {
        _root.localPosition = new Vector3(0, -depth, 0);
    }

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

    public void UpdateLights(float throttleInput, float brakingInput)
    {
        _accelerationLight.SetActive(throttleInput >= 0.1f);
        _brakeLights.SetActive(brakingInput > 0.2f);
    }
}
