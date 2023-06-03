using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GraphAxisLayoutGroup : MonoBehaviour
{
    
    [SerializeField] private GraphElement.Axis _axis;

    private List<Transform> _children;

    private void OnValidate()
    {
        RefreshLayout();
    }

    private void Reset()
    {
        RefreshLayout();
    }

    public void RefreshLayout()
    {
        RectTransform rectTransform = (RectTransform)transform;
        float width = rectTransform.rect.width;
        float height = rectTransform.rect.height;

        int childCounter = 0;
        float childTotal = transform.childCount - 1;

        foreach (RectTransform childRect in rectTransform)
        {
            switch (_axis)
            {
                case GraphElement.Axis.X:
                    childRect.anchoredPosition = new Vector2((width * (childCounter / childTotal)), 0);
                    break;
                case GraphElement.Axis.Y:
                    childRect.anchoredPosition = new Vector2(0, (height * (childCounter / childTotal)));
                    break;
            }

            childCounter++;
        }
    }
}