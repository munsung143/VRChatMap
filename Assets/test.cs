
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class test : UdonSharpBehaviour
{
    void Update()
    {
        transform.Rotate(Vector3.up, 90f * Time.deltaTime);
    }
}
