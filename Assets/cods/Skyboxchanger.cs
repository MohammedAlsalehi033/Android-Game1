using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skyboxchanger : MonoBehaviour
{
      public float RotationPerSecond = 1;
    private bool _rotate;

protected void Start (){
  Application.targetFrameRate = 120;
}
    protected void Update()
    { RenderSettings.skybox.SetFloat("_Rotation", Time.time * RotationPerSecond);
    }


    // Update is called once per frame

}
