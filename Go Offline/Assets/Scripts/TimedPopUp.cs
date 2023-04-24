using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimedPopUp : MonoBehaviour
{
    private float timer;

    public void StartDisableTimer(float time)
    {
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
