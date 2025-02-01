using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GoodHub.Core.Runtime;
using UnityEngine;
using UnityEngine.UI;

public class PerceptronWindow : Window
{
    [SerializeField] private GameObject _layerElementPrefab;

    public override void Initialise(WindowsManager windowsManager, WindowConfiguration windowConfig, WindowIcon windowIcon)
    {
        base.Initialise(windowsManager, windowConfig, windowIcon);
    }
}