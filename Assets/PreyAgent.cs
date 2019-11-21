using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class PreyAgent : Agent
{
    public Transform Hunter;
    public Transform Food;
    public GameObject Arena;

    Rigidbody rBody;
    HunterAcademy m_Academy;
    RayPerception m_RayPer;

    private float Scale;

    public override void InitializeAgent()
    {
        agentParameters.maxStep = 2000;
        base.InitializeAgent();
        m_Academy = FindObjectOfType<HunterAcademy>();
        rBody = GetComponent<Rigidbody>();
        m_RayPer = GetComponent<RayPerception>();
    }

    public override void AgentReset()
    {
        Scale = m_Academy.resetParameters["arena_scale"];
        Arena.transform.localScale = new Vector3(1 * Scale, 1, 1 * Scale);

        // Move the target to a new spot
        RespawnObject(transform);
        RespawnObject(Food);
    }

    public override void CollectObservations()
    {
        var rayDistance = 12f;
        float[] rayAngles = { 20f, 60f, 90f, 120f, 160f, 200f, 240f };
        string[] detectableObjects = { "Hunter", "Arena", "Food" };
        AddVectorObs(m_RayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
        AddVectorObs(GetStepCount() / (float)agentParameters.maxStep);

        // Agent velocity
        AddVectorObs(rBody.velocity);

        Monitor.Log("Prey reward", GetCumulativeReward().ToString());
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

        transform.Rotate(rotateDir, Time.deltaTime * 150f * m_Academy.preyRotationSpeed);
        transform.Translate(dirToGo * m_Academy.preyRunSpeed * Time.deltaTime, Space.World);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        MoveAgent(vectorAction);

        float distanceToHunter = Vector3.Distance(this.transform.localPosition, Hunter.localPosition);
        float distanceToFood = Vector3.Distance(this.transform.localPosition, Food.localPosition);

        // Caught By Hunter
        if (distanceToHunter < 1.42f)
        {
            AddReward(-1.0f);
            RespawnObject(transform);
            RespawnObject(Food);
        }

        // Reached target
        if (distanceToFood < 1.0f)
        {
            AddReward(1.0f);
            RespawnObject(Food);
        }

        // Fell off platform
        if (this.transform.localPosition.y < 0)
        {
            SetReward(-1.0f);
            Done();
        }
    }

    public void RespawnObject(Transform item)
    {
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        item.localPosition = new Vector3(
            (Random.value * 8 - 4) * Scale,
            0.5f,
            (Random.value * 8 - 4) * Scale
        );
    }
}