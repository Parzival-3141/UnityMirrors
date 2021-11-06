using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomRotate : MonoBehaviour
{
    public static bool sync = false;
    [SerializeField] private bool synchronizeAll = false;

    private bool lastSyncAll = false;
    private bool firstSync = true;
    private const float syncSpinRate = 12f;

    private float time;
    private float rate;

    private void Start()
    {
        rate = syncSpinRate;
    }

    private void Update()
    {
        HandleSync();

        time += Time.deltaTime / rate;
        var rotX = Mathf.Cos(time) * 360;
        var rotY = Mathf.Sin(time) * 360;

        transform.rotation = Quaternion.Euler(rotX, rotY, 0);
    }

    private void HandleSync()
    {
        if (synchronizeAll != sync || firstSync) // changed
        {
            if (synchronizeAll != lastSyncAll) // this changed
                sync = synchronizeAll;
            else // other component changed
                synchronizeAll = sync;

            time = 0;
            rate = sync ? syncSpinRate : Random.Range(syncSpinRate * 0.333f, syncSpinRate * 1.f);
        }

        lastSyncAll = synchronizeAll;

        if (firstSync)
            firstSync = false;
    }
}
