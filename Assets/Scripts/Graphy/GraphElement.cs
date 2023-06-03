using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GraphElement : MonoBehaviour
{
    private enum PlotStyle
    {
        POINT,
        LINE
    }

    public enum Axis
    {
        X,
        Y
    }

    [SerializeField] private PlotStyle _plotStyle;

    [SerializeField] private GraphAxis _yAxis;
    [SerializeField] private GraphAxis _xAxis;
    [SerializeField] private GraphBody _graphBody;
    [SerializeField] private GraphGrid _graphGrid;

    private GraphData _graphData;

    // private void Start()
    // {
    //     Initialise("Test Graph", "X Axis", 5, "Y Axis", 32, new List<Vector2>
    //     {
    //         new Vector2(0, 0),
    //         new Vector2(1, 1),
    //         new Vector2(2, 2),
    //         new Vector2(3, 4),
    //         new Vector2(4, 8),
    //         new Vector2(5, 16),
    //         new Vector2(6, 32),
    //         new Vector2(7, 64),
    //         new Vector2(8, 128),
    //         new Vector2(9, 256),
    //         new Vector2(10, 512),
    //         new Vector2(11, 256),
    //         new Vector2(12, 128),
    //         new Vector2(13, 64),
    //         new Vector2(14, 32),
    //         new Vector2(15, 16),
    //         new Vector2(16, 8),
    //         new Vector2(17, 4),
    //         new Vector2(18, 2),
    //         new Vector2(19, 1),
    //         new Vector2(20, 0),
    //     });
    // }

    public void Initialise(string graphLabel, string xAxisLabel, int xAxisStep, string yAxisLabel, int yAxisStep, List<Vector2> initialData = null)
    {
        _graphData = new GraphData
        {
            graphLabel = graphLabel,
            xAxisTitle = xAxisLabel,
            yAxisTitle = yAxisLabel,
            xAxisStep = xAxisStep,
            yAxisStep = yAxisStep,
        };

        if (initialData != null)
        {
            SetBodyData(initialData);
        }
        else
        {
            SetAxesData();
        }
    }

    public void SetBodyData(List<Vector2> initialData)
    {
        if (_graphData == null)
            return;

        _graphData.dataPoints = initialData;

        _graphData.xAxisStartValue = _graphData.dataPoints.Min(dataPoint => dataPoint.x);
        _graphData.yAxisStartValue = _graphData.dataPoints.Min(dataPoint => dataPoint.y);
        _graphData.xAxisEndValue = _graphData.dataPoints.Max(dataPoint => dataPoint.x);
        _graphData.yAxisEndValue = _graphData.dataPoints.Max(dataPoint => dataPoint.y);

        _graphBody.SetBodyData(_graphData);
        _graphGrid.SetGridData(_graphData);

        SetAxesData();
    }

    private void SetAxesData()
    {
        _xAxis.SetAxisData(_graphData);
        _yAxis.SetAxisData(_graphData);
    }
}

public class GraphData
{
    public string graphLabel;

    public string xAxisTitle;
    public string yAxisTitle;

    public float xAxisStartValue;
    public float xAxisEndValue;
    public int xAxisStep;

    public float yAxisStartValue;
    public float yAxisEndValue;
    public int yAxisStep;

    public bool RoundLabels;

    public List<Vector2> dataPoints;

    public Vector2 GetNormalisedDataPoint(Vector2 dataPoint)
    {
        float xAxisDelta = xAxisEndValue - xAxisStartValue;
        float yAxisDelta = yAxisEndValue - yAxisStartValue;

        return new Vector2((dataPoint.x - xAxisStartValue) / xAxisDelta, (dataPoint.y - yAxisStartValue) / yAxisDelta);
    }
}