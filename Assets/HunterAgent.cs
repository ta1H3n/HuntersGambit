using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using System.Linq;

public class HunterAgent : Agent
{
    public Transform Prey;
    private Transform[] Food { get { return Academy.Food; } }

    RayPerception RayPer;
    Rigidbody rBody;
    HunterAcademy Academy;
    public override void InitializeAgent()
    {
        agentParameters.maxStep = 2000;
        base.InitializeAgent();
        Academy = FindObjectOfType<HunterAcademy>();
        RayPer = GetComponent<RayPerception>();
        rBody = GetComponent<Rigidbody>();
    }

    public override void AgentReset()
    {
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.localPosition = new Vector3(0, 0.5f, 0);
    }

    public override void CollectObservations()
    {
        var range = Academy.hunterVisionRange;
        var angle = Academy.hunterVisionAngle;
        var radius = Academy.hunterAwarenessRadius;
        float[] detections = new float[2 + Food.Length * 2];

        var detection = HunterAcademy.GetDetection(transform, Prey, range, radius, angle, Color.red);
        detections[0] = detection[0];
        detections[1] = detection[1];

        for (int i = 0; i < Food.Length; i++)
        {
            detection = HunterAcademy.GetDetection(transform, Food[i], range, radius, angle, Color.red);
            detections[2 + 2 * i] = detection[0];
            detections[2 + 2 * i + 1] = detection[1];
        }
        AddVectorObs(detections);

        Monitor.Log("Distance", detections[0].ToString("n2"), transform);
        Monitor.Log("Angle", detections[1].ToString("n2"), transform);
        Monitor.Log("Hunter", string.Join(" ", detections.Select(x => x.ToString("n2"))));

        //var facing = transform.forward;
        //Monitor.Log("x", facing.x.ToString("n2"), transform);
        //Monitor.Log("z", facing.z.ToString("n2"), transform);

        //var heading = Prey.position - transform.position;
        //var heading2 = (Prey.position - transform.position).normalized;
        //Monitor.Log("xh", heading2.x.ToString("n2"), transform);
        //Monitor.Log("zh", heading2.z.ToString("n2"), transform);

        //var direction = (facing + heading2) / 2;
        //direction.z = direction.z >= 0 ? direction.z : direction.z * -1;
        //direction.x = direction.x >= 0 ? direction.x : direction.x * -1;
        //Monitor.Log("xd", direction.x.ToString("n2"), transform);
        //Monitor.Log("zd", direction.z.ToString("n2"), transform);

        //Monitor.Log("Angle", Vector3.SignedAngle(transform.forward, heading, Vector3.up).ToString());
        //Monitor.Log("AngleNorm", Vector3.SignedAngle(transform.forward, heading.normalized, Vector3.up).ToString());

        // Agent position
        AddVectorObs(transform.localPosition);
        AddVectorObs(transform.forward);
        AddVectorObs(GetStepCount() / (float)agentParameters.maxStep);
        Monitor.Log("Hunter reward", GetCumulativeReward().ToString());
    }

    public void MoveAgent(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        if (brain.brainParameters.vectorActionSpaceType == SpaceType.Continuous)
        {
            if (act[0] < 0)
            {
                act[0] = act[0] / 2;
            }

            dirToGo = transform.forward * Mathf.Clamp(act[0], -1f, 1f);
            rotateDir = transform.up * Mathf.Clamp(act[1], -1f, 1f);
        }
        else
        {
            var run = Mathf.FloorToInt(act[0]);
            var turn = Mathf.FloorToInt(act[1]);
            switch (run)
            {
                case 1:
                    dirToGo = transform.forward * 1f;
                    break;
                case 2:
                    dirToGo = transform.forward * -0.5f;
                    break;
            }
            switch (turn)
            {
                case 1:
                    rotateDir = transform.up * 1f;
                    break;
                case 2:
                    rotateDir = transform.up * -1f;
                    break;
            }
        }

        transform.Rotate(rotateDir, Time.deltaTime * 150f * Academy.hunterRotationSpeed);
        transform.Translate(dirToGo * Academy.hunterRunSpeed * Time.deltaTime, Space.World);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        AddReward(-1f / agentParameters.maxStep);
        MoveAgent(vectorAction);

        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Prey.localPosition);

        // Reached target
        if (distanceToTarget < 1.42f)
        {
            AddReward(1.0f);
            Done();
        }

        // Fell off platform
        if (this.transform.localPosition.y < 0)
        {
            SetReward(-1.0f);
            Done();
        }
    }
}