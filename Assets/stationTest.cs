
using UdonSharp;
using UnityEngine;
using VRC.SDKBase;
using VRC.Udon;

public class stationTest : UdonSharpBehaviour
{
    public override void Interact()
    {
        Networking.LocalPlayer.UseAttachedStation();
    }
}
