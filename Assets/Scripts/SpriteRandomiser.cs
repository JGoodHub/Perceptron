using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class SpriteRandomiser : MonoBehaviour
{

    [SerializeField] private SpriteRenderer _targetImage;
    [SerializeField] private Sprite[] _sourceSprites;

    public void SelectSpriteFromSeed(int seed)
    {
        _targetImage.sprite = _sourceSprites[seed % _sourceSprites.Length];
    }

}
