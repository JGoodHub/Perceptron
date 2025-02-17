using GoodHub.Core.Runtime;
using UnityEngine;

public class ConnectionElement : MonoBehaviour
{
    [SerializeField] private CanvasPolyLineRenderer _lineRenderer;

    [SerializeField] private Color _positiveColour = Color.green;
    [SerializeField] private Color _negativeColour = Color.red;
    [SerializeField] private float _weightThicknessScaler = 1f;

    private NeuronElement _sourceNeuronElement;
    private NeuronElement _targetNeuronElement;
    private float _weight;

    public void Initialise(NeuronElement sourceNeuronElement, NeuronElement targetNeuronElement, float weight)
    {
        _sourceNeuronElement = sourceNeuronElement;
        _targetNeuronElement = targetNeuronElement;
        _weight = weight;

        InvokeRepeating(nameof(RefreshLineRenderer), 0.05f, 1f);
    }

    private void RefreshLineRenderer()
    {
        Vector2 sourceScreenPoint = RectTransformUtility.WorldToScreenPoint(null, _sourceNeuronElement.ConnectionsOutput.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform) transform, sourceScreenPoint, null, out Vector2 sourceAnchoredPosition);

        Vector2 targetScreenPoint = RectTransformUtility.WorldToScreenPoint(null, _targetNeuronElement.ConnectionsInput.position);
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform) transform, targetScreenPoint, null, out Vector2 targetAnchoredPosition);

        Vector2 offset = ((RectTransform) transform).rect.size / 2f;

        _lineRenderer.SetPositions(new[] {sourceAnchoredPosition + offset, targetAnchoredPosition + offset});
        _lineRenderer.Thickness = _weight * _weightThicknessScaler;
        _lineRenderer.color = _weight >= 0f ? _positiveColour : _negativeColour;
    }
}