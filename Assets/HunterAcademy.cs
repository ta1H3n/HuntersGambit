using UnityEngine;
using MLAgents;

public class HunterAcademy : Academy
{
    public float hunterRunSpeed = 1.5F;
    public float hunterRotationSpeed = 1F;
    public float hunterVisionRange = 15F;
    public float hunterVisionAngle = 90F;
    public float hunterAwarenessRadius = 5F;
    public float preyRunSpeed = 1.5F;
    public float preyRotationSpeed = 1F;
    public float preyVisionRange = 15F;
    public float preyVisionAngle = 90F;
    public float preyAwarenessRadius = 5F;
    public float gravityMultiplier = 3F; // use ~3 to make things less floaty

    public override void InitializeAcademy()
    {
        Physics.gravity *= gravityMultiplier;
        Monitor.SetActive(true);
    }

    public override void AcademyReset()
    {
    }

    public static float[] GetDetection(Transform from, Transform to, float range, float radius, float angle, Color color)
    {
        float[] res = new float[2];

        var heading = to.position - from.position;
        float distance = heading.sqrMagnitude;
        if (distance < range * range)
        {
            var signedAngle = Vector3.SignedAngle(from.forward, heading.normalized, Vector3.up);
            var viewAngle = signedAngle >= 0 ? signedAngle : signedAngle * -1;
            //signedAngle = (viewAngle / 360) + 0.5f;
            signedAngle = signedAngle / 360;
            if (distance < radius * radius || viewAngle < angle)
            {
                Physics.Raycast(from.position, heading.normalized, out var hit, Mathf.Infinity);
                if (hit.transform != null && to.Equals(hit.transform))
                {
                    Debug.DrawRay(from.position, heading, color, 0.01F);
                    res[0] = heading.magnitude;
                    res[1] = signedAngle;
                    return res;
                }
                else
                {
                    Debug.DrawRay(from.position, heading, Color.black, 0.01F);
                }
            }
        }

        res[0] = 0f;
        res[1] = 0f;
        return res;
    }

    public static bool IsDetected(Transform from, Transform to, float range, float radius, float angle, Color color)
    {

        var heading = to.position - from.position;
        float distance = heading.sqrMagnitude;
        if (distance < range * range)
        {
            var signedAngle = Vector3.SignedAngle(from.forward, heading.normalized, Vector3.up);
            var viewAngle = signedAngle >= 0 ? signedAngle : signedAngle * -1;
            //signedAngle = (signedAngle / 360) + 0.5f;
            if (distance < radius * radius || viewAngle < angle)
            {
                Physics.Raycast(from.position, heading.normalized, out var hit, Mathf.Infinity);
                if (hit.transform != null && to.Equals(hit.transform))
                {
                    Debug.DrawRay(from.position, heading, color, 0.01F);
                    return true;
                }
                else
                {
                    Debug.DrawRay(from.position, heading, Color.black, 0.01F);
                }
            }
        }

        return false;
    }

    //public static float[] GetDetection(Transform from, Transform to, float range, float radius, float angle, Color color)
    //{
    //    float[] res = new float[4];

    //    var heading = to.position - from.position;
    //    float distance = heading.sqrMagnitude;
    //    if (distance < range * range)
    //    {
    //        var viewAngle = Vector3.Angle(from.forward, heading.normalized);
    //        if (distance < radius * radius || Vector3.Angle(from.forward, heading) < angle)
    //        {
    //            RaycastHit hit;
    //            Vector3 normalised = heading.normalized;
    //            Physics.Raycast(from.position, normalised, out hit, Mathf.Infinity);
    //            if (hit.transform != null && to.Equals(hit.transform))
    //            {
    //                Debug.DrawRay(from.position, heading, color, 0.1F);
    //                var direction = (from.forward + heading.normalized) / 2;
    //                res[0] = direction.x >= 0 ? direction.x : direction.x * -1;
    //                res[1] = direction.y >= 0 ? direction.x : direction.y * -1;
    //                res[2] = direction.z >= 0 ? direction.x : direction.z * -1;
    //                res[3] = heading.magnitude;
    //                return res;
    //            }
    //            else
    //            {
    //                Debug.DrawRay(from.position, heading, Color.black, 0.1F);

    //            }
    //        }
    //    }

    //    res[0] = -1;
    //    res[1] = -1;
    //    res[2] = -1;
    //    res[3] = -1;
    //    return res;
    //}
}
