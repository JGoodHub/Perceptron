using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpriteRandomiser : MonoBehaviour
{

    [SerializeField] private SpriteRenderer _targetImage;
    [SerializeField] private Sprite[] _sourceSprites;

    private void Start()
    {
        _targetImage.sprite = _sourceSprites[Random.Range(0, _sourceSprites.Length)];
    }
    
}
