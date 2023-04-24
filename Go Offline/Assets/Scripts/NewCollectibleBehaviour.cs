using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewCollectibleBehaviour : CollectibleVisualManager
{
    [Header("Settings")]
    [SerializeField] private float maxSize;

    private void OnEnable()
    {
        GetComponent<RectTransform>().localScale = Vector3.one * maxSize;
    }
}
