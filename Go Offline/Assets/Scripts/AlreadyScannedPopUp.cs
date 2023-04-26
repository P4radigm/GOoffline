using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AlreadyScannedPopUp : MonoBehaviour
{
    private float timer;
    [SerializeField] private TextMeshProUGUI notificationText;

    public void StartPopUp(string text, float time)
    {
        notificationText.text = text;
        timer = time;
    }

    private void Update()
    {
        if(timer > 0)
        {
            timer -= Time.deltaTime;
            if(timer <= 0) { gameObject.SetActive(false); }
        }
    }
}
