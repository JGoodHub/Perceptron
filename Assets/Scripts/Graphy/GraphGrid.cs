using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GraphGrid : MonoBehaviour
{
    [SerializeField] private GameObject _gridLinePrefab;

    private List<GameObject> _gridLines = new List<GameObject>();
    
    private GraphData _graphData;

    private RectTransform _rectTransform;
    private Vector2 _size;

    private void Start()
    {
        _rectTransform = (RectTransform)transform;
        _size = _rectTransform.rect.size;
    }

    public void SetGridData(GraphData graphData)
    {
        _graphData = graphData;

        CreateLines();
    }

    private void CreateLines()
    {
        foreach (GameObject go in _gridLines)
        {
            Destroy(go);
        }

        _gridLines.Clear();
        
        float startValue = _graphData.xAxisStartValue;
        float endValue = _graphData.xAxisEndValue;
        float stepValue = _graphData.xAxisStep;
        float deltaValue = endValue - startValue;

        for (float lineValue = stepValue; lineValue <= endValue; lineValue += stepValue)
        {
            RectTransform lineRect = Instantiate(_gridLinePrefab, transform).GetComponent<RectTransform>();
            lineRect.anchoredPosition = new Vector2((lineValue / deltaValue) * _size.x, _size.y * 0.5f);
            lineRect.localScale = new Vector3(1f, _size.y, 1f);

            _gridLines.Add(lineRect.gameObject);
        }

        startValue = _graphData.yAxisStartValue;
        endValue = _graphData.yAxisEndValue;
        stepValue = _graphData.yAxisStep;
        deltaValue = endValue - startValue;

        for (float lineValue = stepValue; lineValue <= endValue; lineValue += stepValue)
        {
            RectTransform lineRect = Instantiate(_gridLinePrefab, transform).GetComponent<RectTransform>();
            lineRect.anchoredPosition = new Vector2(_size.x * 0.5f, (lineValue / deltaValue) * _size.y);
            lineRect.localScale = new Vector3(_size.x, 1f, 1f);
            
            _gridLines.Add(lineRect.gameObject);
        }
    }
}