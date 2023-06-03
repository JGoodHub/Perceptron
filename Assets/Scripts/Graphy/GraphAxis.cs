using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GraphAxis : MonoBehaviour
{
    [SerializeField] private GraphElement.Axis _axis;
    [SerializeField] private TextMeshProUGUI _title;
    [SerializeField] private GameObject _labelPrefab;
    [SerializeField] private RectTransform _labelsContainer;
    [SerializeField] private GraphAxisLayoutGroup _layoutGroup;

    private List<GraphLabel> _labels = new List<GraphLabel>();

    private GraphData _graphData;

    private RectTransform _rectTransform;
    private float _axisLength;

    private void Start()
    {
        _rectTransform = (RectTransform)transform;
        _axisLength = _axis == GraphElement.Axis.X ? _rectTransform.rect.size.x : _rectTransform.rect.size.y;
    }

    public void SetAxisData(GraphData graphData)
    {
        _graphData = graphData;

        foreach (GraphLabel label in _labels)
        {
            Destroy(label.gameObject);
        }

        _labels.Clear();

        switch (_axis)
        {
            case GraphElement.Axis.X:
            {
                _title.text = _graphData.xAxisTitle;
                break;
            }
            case GraphElement.Axis.Y:
            {
                _title.text = _graphData.yAxisTitle;
                break;
            }
        }

        CreateDivisors(_axis);

        //_layoutGroup.RefreshLayout();
    }

    private void CreateDivisors(GraphElement.Axis axis)
    {
        float startValue = 0;
        float endValue = 0;
        float stepValue = 0;

        switch (axis)
        {
            case GraphElement.Axis.X:
                startValue = _graphData.xAxisStartValue;
                endValue = _graphData.xAxisEndValue;
                stepValue = _graphData.xAxisStep;
                break;
            case GraphElement.Axis.Y:
                startValue = _graphData.yAxisStartValue;
                endValue = _graphData.yAxisEndValue;
                stepValue = _graphData.yAxisStep;
                break;
        }

        for (float labelValue = startValue; labelValue <= endValue; labelValue += stepValue)
        {
            GraphLabel label = Instantiate(_labelPrefab, _labelsContainer).GetComponent<GraphLabel>();

            if (_graphData.RoundLabels)
                label.SetValue(Mathf.RoundToInt(labelValue));
            else
                label.SetValue(labelValue);

            RectTransform labelRect = (RectTransform)label.transform;

            switch (_axis)
            {
                case GraphElement.Axis.X:
                    labelRect.anchoredPosition = new Vector2(AxisToAnchoredPositionX(labelValue), 0f);
                    break;
                case GraphElement.Axis.Y:
                    labelRect.anchoredPosition = new Vector2(0f, AxisToAnchoredPositionX(labelValue));
                    break;
            }

            _labels.Add(label);
        }
    }

    public float AxisToAnchoredPositionX(float dataPoint)
    {
        switch (_axis)
        {
            case GraphElement.Axis.X:
                float xDelta = _graphData.xAxisEndValue - _graphData.xAxisStartValue;
                return (dataPoint / xDelta) * _axisLength;
            case GraphElement.Axis.Y:
                float yDelta = _graphData.yAxisEndValue - _graphData.yAxisStartValue;
                return (dataPoint / yDelta) * _axisLength;
        }

        return 0f;
    }
}