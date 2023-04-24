using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Shapes;

[RequireComponent(typeof(Rectangle))]
[RequireComponent(typeof(RectTransform))]
public class ShapesFillRectTransform : MonoBehaviour
{
    private RectTransform rectTransform;
    private Rectangle rect;

    private void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        rect = GetComponent<Rectangle>();
    }

    private void Update()
    {
        rect.Height = rectTransform.rect.height;
        rect.Width = rectTransform.rect.width;
    }
}
