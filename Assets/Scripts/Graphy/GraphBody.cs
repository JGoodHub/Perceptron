using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphBody : MonoBehaviour
{
    [SerializeField] private GameObject _plotPointPrefab;
    [SerializeField] private GameObject _plotLinePrefab;

    private List<GameObject> _plotPointObjects = new List<GameObject>();
    private List<GameObject> _plotLineObjects = new List<GameObject>();

    
    private RectTransform _rectTransform;
    private Vector2 _size;

    private void Start()
    {
        _rectTransform = (RectTransform)transform;
        _size = _rectTransform.rect.size;
    }

    public void SetBodyData(GraphData _graphData)
    {
        foreach (GameObject plotPointObject in _plotPointObjects)
        {
            Destroy(plotPointObject);
        }
        
        foreach (GameObject plotLineObject in _plotLineObjects)
        {
            Destroy(plotLineObject);
        }

        _plotPointObjects.Clear();
        _plotLineObjects.Clear();
        
        for (int l = 1; l < _graphData.dataPoints.Count; l++)
        {
            GameObject plotLineObject = Instantiate(_plotLinePrefab, transform);
            RectTransform plotLineRect = plotLineObject.GetComponent<RectTransform>();

            Vector2 pointA = DataPointToAnchoredPosition(_graphData, _graphData.dataPoints[l - 1]);
            Vector2 pointB = DataPointToAnchoredPosition(_graphData, _graphData.dataPoints[l]);

            plotLineRect.anchoredPosition = (pointA + pointB) * 0.5f;
            plotLineRect.localScale = new Vector3(plotLineRect.localScale.x, (pointB - pointA).magnitude, plotLineRect.localScale.z);
            plotLineRect.rotation = Quaternion.Euler(0, 0, -Vector2.Angle(Vector2.up, (pointB - pointA)));
            
            _plotLineObjects.Add(plotLineObject);
        }

        foreach (Vector2 dataPoint in _graphData.dataPoints)
        {
            GameObject plotPointObject = Instantiate(_plotPointPrefab, transform);
            RectTransform plotPointRect = plotPointObject.GetComponent<RectTransform>();

            plotPointRect.anchoredPosition = DataPointToAnchoredPosition(_graphData, dataPoint);

            _plotPointObjects.Add(plotPointObject);
        }
    }

    public Vector2 DataPointToAnchoredPosition(GraphData graphData, Vector2 dataPoint)
    {
        Vector2 normalisedDataPoint = graphData.GetNormalisedDataPoint(dataPoint);
        return _size * normalisedDataPoint;
    }

    public Vector2 DataPointToAnchoredPosition(GraphData graphData, Vector2 dataPointA, Vector2 dataPointB)
    {
        Vector2 normalisedDataPoint = graphData.GetNormalisedDataPoint((dataPointA + dataPointB) * 0.5f);
        return _size * normalisedDataPoint;
    }
}