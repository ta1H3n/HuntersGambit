using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class PreyAgent : Agent
{
    public Transform Hunter;
    public Transform[] Food;
    //public GameObject Arena;

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
        //if (Arena != null)
        //    Arena.transform.localScale = new Vector3(1 * Scale, 1, 1 * Scale);

        // Move the target to a new spot
        RespawnObject(transform);
        foreach (var food in Food)
            RespawnObject(food);
    }

    public override void CollectObservations()
    {
        var rayDistance = Academy.preyVisionRange;
        float[] rayAngles = { 10f, 30f, 40f, 50f, 60f, 70f, 80f, 90f, 100f, 110f, 120f, 130f, 140f, 150f, 170f };
        string[] detectableObjects = { "Hunter", "Arena", "Food" };
        AddVectorObs(RayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
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
                AddReward(1.0f);
                RespawnObject(food);
            }
        }

        // Caught By Hunter
        if (distanceToHunter < 1.42f)
        {
            AddReward(-1.0f);
            RespawnObject(transform);
            foreach (var food in Food)
                RespawnObject(food);
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

    //public static Vector3 PolarToCartesian(float radius, float angle)
    //{
    //    var x = radius * Mathf.Cos(RayPerception.DegreeToRadian(angle));
    //    var z = radius * Mathf.Sin(RayPerception.DegreeToRadian(angle));
    //    return new Vector3(x, 0f, z);
    //}

    //public void DrawVision(float[] rayAngles, float distance)
    //{
    //    var start = rBody.transform.localPosition;
    //    foreach (var angle in rayAngles)
    //    {
    //        var end = PolarToCartesian(distance, angle);
    //        Debug.DrawRay(start, end, Color.green);
    //    }
    //}
}