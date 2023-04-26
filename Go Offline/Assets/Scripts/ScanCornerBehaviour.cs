using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Shapes;

public class ScanCornerBehaviour : MonoBehaviour
{
    private Gyroscope gyro;

    //private float xRotation;
    //private float yRotation;
    private float zRotation;

    //[SerializeField] private TextMeshProUGUI xRotDebugTest;
    //[SerializeField] private TextMeshProUGUI yRotDebugTest;
    //[SerializeField] private TextMeshProUGUI zRotDebugTest;
    //[SerializeField] private TextMeshProUGUI vertRotDebugTest;
    //[SerializeField] private TextMeshProUGUI horzRotDebugTest;

    [SerializeField] private Polyline[] vertLines;
    [SerializeField] private Polyline[] horzLines;

    [Range(0, 0.5f)] [SerializeField] private float transitionStart;

    void Start()
    {
        if (SystemInfo.supportsGyroscope)
        {
            gyro = Input.gyro;
            gyro.enabled = true;
        }
        else
        {
            Debug.Log("Gyro not supported on this device.");
        }
    }

    void Update()
    {
        if (gyro != null)
        {
            Quaternion deviceRotation = gyro.attitude;
            Quaternion cameraRotation = Quaternion.Euler(90, 0, 0) * deviceRotation;
            //xRotation = cameraRotation.eulerAngles.x;
            //yRotation = cameraRotation.eulerAngles.y;
            zRotation = cameraRotation.eulerAngles.z;

            // Use the xRotation value to adjust the hue of your UI element
            // For example:
            // GetComponent<Image>().color = Color.HSVToRGB(xRotation / 360f, 1, 1);

            //xRotDebugTest.text = $"xRot = {xRotation}";
            //yRotDebugTest.text = $"yRot = {yRotation}";
            //zRotDebugTest.text = $"zRot = {zRotation}";

            float sineValueVert = Mathf.Sin(zRotation * Mathf.Deg2Rad);
            float sineValueHorz = Mathf.Sin((zRotation+90) * Mathf.Deg2Rad);

            float remappedAngleVert = Mathf.Clamp01((Mathf.Abs(sineValueVert) - transitionStart) * (1 / (1 - (2 * transitionStart))));
            float remappedAngleHorz = Mathf.Clamp01((Mathf.Abs(sineValueHorz) - transitionStart) * (1 / (1 - (2 * transitionStart))));

            AdjustAlphaLineGroup(vertLines, remappedAngleVert);
            AdjustAlphaLineGroup(horzLines, remappedAngleHorz);

            //vertRotDebugTest.text = $"using zRot, Vert value = {remappedAngleVert}";
            //horzRotDebugTest.text = $"using zRot, Horz value = {remappedAngleHorz}";
        }
    }

    private void AdjustAlphaLineGroup(Polyline[] group, float alpha)
    {
        for (int i = 0; i < group.Length; i++)
        {
            Color currentColor = group[i].Color;
            group[i].Color = new Color(currentColor.r, currentColor.g, currentColor.b, alpha);
        }
    }
}
