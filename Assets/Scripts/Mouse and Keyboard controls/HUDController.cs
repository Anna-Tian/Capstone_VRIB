using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SickscoreGames.HUDNavigationSystem;

public class HUDController : MonoBehaviour
{
    private HUDNavigationSystem _HUDNavigationSystem;
    // Start is called before the first frame update
    void Start()
    {
        _HUDNavigationSystem = HUDNavigationSystem.Instance;
    }

    // Update is called once per frame
    void Update()
    {
        if (ExperimentController.DEBUG)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
                _HUDNavigationSystem.EnableRadar(!_HUDNavigationSystem.useRadar);
            if (Input.GetKeyDown(KeyCode.Alpha2))
                _HUDNavigationSystem.EnableCompassBar(!_HUDNavigationSystem.useCompassBar);
            if (Input.GetKeyDown(KeyCode.Alpha3))
                _HUDNavigationSystem.EnableIndicators(!_HUDNavigationSystem.useIndicators);
            if (Input.GetKeyDown(KeyCode.Alpha4))
                _HUDNavigationSystem.EnableARC4(!_HUDNavigationSystem.useARC4);
            /*if (Input.GetKeyDown(KeyCode.Alpha5))
                _HUDNavigationSystem.radarMode = (_HUDNavigationSystem.radarMode == RadarModes.RotateRadar) ? RadarModes.RotatePlayer : RadarModes.RotateRadar;
            if (Input.GetKeyDown(KeyCode.Alpha6))
                if (_HUDNavigationSystem.RotationReference == 0) _HUDNavigationSystem.RotationReference = (_RotationReference)1;
                else _HUDNavigationSystem.RotationReference = 0;*/
        }
    }
}
