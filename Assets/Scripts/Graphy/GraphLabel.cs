using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;

public class GraphLabel : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _valueText;

    public void SetValue(float value, int decimalPlaces = -1)
    {
        switch (decimalPlaces)
        {
            case 0:
            {
                _valueText.text = Mathf.RoundToInt(value).ToString(CultureInfo.InvariantCulture);
                break;
            }
            case > 0:
            {
                float multiplier = 10 * decimalPlaces;
                _valueText.text = (Mathf.Round(value * multiplier) / multiplier).ToString(CultureInfo.InvariantCulture);
                break;
            }
            default:
            {
                _valueText.text = value.ToString(CultureInfo.InvariantCulture);
                break;
            }
        }
    }

    public void SetValue(int value)
    {
        _valueText.text = value.ToString(CultureInfo.InvariantCulture);
    }
}