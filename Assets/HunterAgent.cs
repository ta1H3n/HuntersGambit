﻿using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class HunterAgent : Agent
{
    public Transform Prey;
    public GameObject Arena;

    RayPerception h_RayPer;
    Rigidbody p_rBody;
    Rigidbody h_rBody;
    HunterAcademy m_Academy;
    public override void InitializeAgent()
    {
        agentParameters.maxStep = 5000;
        base.InitializeAgent();
        m_Academy = FindObjectOfType<HunterAcademy>();
        h_RayPer = GetComponent<RayPerception>();
        h_rBody = GetComponent<Rigidbody>();
        p_rBody = Prey.GetComponent<Rigidbody>();
    }

    public override void AgentReset()
    {
        if (this.transform.localPosition.y < 0)
        {
            // If the Agent fell, zero its momentum
            this.h_rBody.angularVelocity = Vector3.zero;
            this.h_rBody.velocity = Vector3.zero;
            this.transform.localPosition = new Vector3(0, 0.5f, 0);
        }
    }

    public override void CollectObservations()
    {
        var rayDistance = 12f;
        float[] rayAngles = { 20f, 60f, 90f, 120f, 160f };
        string[] detectableObjects = { "Prey", "Arena" };
        AddVectorObs(h_RayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
        AddVectorObs(GetStepCount() / (float)agentParameters.maxStep);

        // Agent velocity
        AddVectorObs(h_rBody.velocity);

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
            var action = Mathf.FloorToInt(act[0]);
            switch (action)
            {
                case 1:
                    dirToGo = transform.forward * 1f;
                    break;
                case 2:
                    dirToGo = transform.forward * -1f;
                    break;
                case 3:
                    rotateDir = transform.up * 1f;
                    break;
                case 4:
                    rotateDir = transform.up * -1f;
                    break;
            }
        }
        transform.Rotate(rotateDir, Time.deltaTime * 150f);
        h_rBody.AddForce(dirToGo * m_Academy.agentRunSpeed, ForceMode.VelocityChange);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        AddReward(-1f / agentParameters.maxStep);
        MoveAgent(vectorAction);

        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Prey.localPosition);

        // Reached target
        if (distanceToTarget < 1.42f)
        {
            SetReward(1.0f);
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