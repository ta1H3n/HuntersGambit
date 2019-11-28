using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class PreyAgent : Agent
{
    public Transform Hunter;
    public Transform Arena;
    private Transform[] Food { get { return Academy.Food; } }

    Rigidbody rBody;
    HunterAcademy Academy;
    RayPerception RayPer;

    private float Scale;

    public override void InitializeAgent()
    {
        agentParameters.maxStep = 2000;
        base.InitializeAgent();
        Academy = FindObjectOfType<HunterAcademy>();
        rBody = GetComponent<Rigidbody>();
        RayPer = GetComponent<RayPerception>();
    }

    public override void AgentReset()
    {
        Scale = Academy.resetParameters["arena_scale"];
        Arena.transform.localScale = new Vector3(1 * Scale, 1, 1 * Scale);
        if (Academy.resetParameters["arena"] == 0)
            Arena.transform.localPosition = new Vector3(0, -10, 0);

        // Move the target to a new spot
        RespawnObject(transform);
        foreach (var food in Food)
            RespawnObject(food);
    }

    public override void CollectObservations()
    {
        var range = Academy.preyVisionRange;
        var angle = Academy.preyVisionAngle;
        var radius = Academy.preyAwarenessRadius;
        foreach (var food in Food)
        {
            AddVectorObs(HunterAcademy.GetDetection(transform, food, range, radius, angle, Color.yellow));
        }
        AddVectorObs(HunterAcademy.GetDetection(transform, Hunter, range, radius, angle, Color.yellow));

        // Agent velocity
        AddVectorObs(rBody.velocity);
        AddVectorObs(GetStepCount() / (float)agentParameters.maxStep);
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

        transform.Rotate(rotateDir, Time.deltaTime * 150f * Academy.preyRotationSpeed);
        transform.Translate(dirToGo * Time.deltaTime * Academy.preyRunSpeed, Space.World);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        MoveAgent(vectorAction);

        float distanceToHunter = Vector3.Distance(this.transform.localPosition, Hunter.localPosition);

        foreach (var food in Food)
        {
            float distanceToFood = Vector3.Distance(this.transform.localPosition, food.localPosition);
            
            // Reached target
            if (distanceToFood < 1.0f)
            {
                AddReward(0.5f);
                food.position = new Vector3(0, -1000, 0);
            }
        }

        // Caught By Hunter
        if (distanceToHunter < 1.42f)
        {
            AddReward(-1.0f);
            Done();
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