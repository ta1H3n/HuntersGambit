using UnityEngine;
using MLAgents;

public class HunterAcademy : Academy
{
    public float hunterRunSpeed = 1.5F;
    public float hunterRotationSpeed = 1F;
    public float preyRunSpeed = 1.5F;
    public float preyRotationSpeed = 1F;
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
