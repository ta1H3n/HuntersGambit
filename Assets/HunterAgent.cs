using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class HunterAgent : Agent
{
    public Transform Prey;
    //public GameObject? Arena;

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
        // If the Agent fell, zero its momentum
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.localPosition = new Vector3(0, 0.5f, 0);
    }

    public override void CollectObservations()
    {
        var rayDistance = Academy.hunterVisionRange;
        float[] rayAngles = { 50f, 60f, 70f, 80f, 90f, 100f, 110f, 120f, 130f };
        string[] detectableObjects = { "Prey", "Arena", "Food" };
        var perception = RayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f);
        AddVectorObs(perception);
        AddVectorObs(GetStepCount() / (float)agentParameters.maxStep);

        // Agent velocity
        AddVectorObs(rBody.velocity);

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
        }

        // Fell off platform
        if (this.transform.localPosition.y < 0)
        {
            SetReward(-1.0f);
            Done();
        }
    }

    //public static Vector3 PolarToCartesian(float radius, float angle)
    //{
    //    var x = radius * Mathf.Cos(RayPerception.DegreeToRadian(angle));
    //    var z = radius * Mathf.Sin(RayPerception.DegreeToRadian(angle));
    //    return new Vector3(x, 0f, z);
    //}

    //public void DrawVision(float[] rayAngles, float distance)
    //{
    //    foreach (var angle in rayAngles)
    //    {
    //        var end = transform.TransformDirection(
    //            PolarToCartesian(distance, angle));
    //        end.y = transform.position.y;
    //        var start = transform.position;
    //        Debug.DrawRay(start, end, Color.white, 1f, false);
    //    }
    //}
}