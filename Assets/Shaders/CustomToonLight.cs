using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CustomToonLight : MonoBehaviour
{
    private Light light = null;
    private void OnEnable()
{
light = this.GetComponent<Light>();
}

private void Update()
{
Shader.SetGlobalVector("Toon", -this.transform.forward);
}

}
