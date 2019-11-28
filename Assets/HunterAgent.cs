﻿using System.Collections.Generic;
using UnityEngine;
using MLAgents;

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
        // If the Agent fell, zero its momentum
        this.rBody.angularVelocity = Vector3.zero;
        this.rBody.velocity = Vector3.zero;
        this.transform.localPosition = new Vector3(0, 0.5f, 0);
    }

    public override void CollectObservations()
    {
        var range = Academy.hunterVisionRange;
        var angle = Academy.hunterVisionAngle;
        var radius = Academy.hunterAwarenessRadius;
        foreach (var food in Food)
        {
            AddVectorObs(HunterAcademy.GetDetection(transform, food, range, radius, angle, Color.red));
        }
        AddVectorObs(HunterAcademy.GetDetection(transform, Prey, range, radius, angle, Color.red));

        // Agent velocity
        AddVectorObs(rBody.velocity);
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