using UnityEngine;
using MLAgents;

public class HunterAcademy : Academy
{
    public float gravityMultiplier = 3F; // use ~3 to make things less floaty

    public override void InitializeAcademy()
    {
        Physics.gravity *= gravityMultiplier;
        Monitor.SetActive(true);
    }

    public override void AcademyReset()
    {
    }
}
