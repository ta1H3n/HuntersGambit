using UnityEngine;
using MLAgents;

public class PreyAgent : Agent
{
    public Transform Hunter;
    public GameObject Arena;
    public GameObject[] Walls;

    Rigidbody rBody;
    HunterAcademy m_Academy;
    RayPerception m_RayPer;

    public override void InitializeAgent()
    {
        agentParameters.maxStep = 3000;
        base.InitializeAgent();
        m_Academy = FindObjectOfType<HunterAcademy>();
        rBody = GetComponent<Rigidbody>();
        m_RayPer = GetComponent<RayPerception>();

        foreach (var wall in Walls)
        {
            wall.transform.localPosition = new Vector3(
                (Random.value * 6 - 3) - 0.5f,
                -1f,
                (Random.value * 6 - 3) - 0.5f
            );
        }
    }

    public override void AgentReset()
    {
        float scale = m_Academy.resetParameters["arena_scale"];
        Debug.Log("scale: " + scale);
        Arena.transform.localScale = new Vector3(scale, 1, scale);

        int walls = (int) m_Academy.resetParameters["walls"];
        walls = walls > Walls.Length ? Walls.Length : walls;
        for (int i = 0; i < walls; i++)
        {
            var wall = Walls[i];
            wall.transform.localScale = new Vector3(0.2f * scale, 1.5f, 3 * scale);
            wall.transform.localPosition = new Vector3(
                (Random.value * 6 - 3) * scale - 0.5f,
                0.75f,
                (Random.value * 6 - 3) * scale - 0.5f
            );
        }

        // Move the target to a new spot
        rBody.angularVelocity = Vector3.zero;
        rBody.velocity = Vector3.zero;
        transform.localPosition = new Vector3(
            (Random.value * 8 - 4) * scale,
            0.5f,
            (Random.value * 8 - 4) * scale
        );
    }

    public override void CollectObservations()
    {
        var rayDistance = 18f;
        float[] rayAngles = { 20f, 60f, 90f, 120f, 160f, 200f, 240f };
        string[] detectableObjects = { "Hunter", "Arena", "Terrain" };
        AddVectorObs(m_RayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));
        AddVectorObs(GetStepCount() / (float)agentParameters.maxStep);

        // Agent velocity
        //AddVectorObs(rBody.velocity);
        //AddVectorObs(rBody.transform.localPosition);
        //AddVectorObs(rBody.rotation.y);

        Monitor.Log("Prey reward", GetCumulativeReward().ToString());
    }

    public void MoveAgent(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        if (brain.brainParameters.vectorActionSpaceType == SpaceType.Continuous)
        {
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
                    rotateDir = transform.up * 1f;
                    break;
                case 2:
                    rotateDir = transform.up * -1f;
                    break;
            }
            switch (turn)
            {
                case 1:
                    dirToGo = transform.forward * 1f;
                    break;
                case 2:
                    dirToGo = transform.forward * -1f;
                    break;
            }
        }
        transform.Rotate(rotateDir, Time.deltaTime * 150f);
        //rBody.AddForce(dirToGo * m_Academy.preyRunSpeed, ForceMode.VelocityChange);
        transform.Translate(dirToGo * m_Academy.preyRunSpeed * Time.deltaTime, Space.World);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        AddReward(+1f / agentParameters.maxStep);
        MoveAgent(vectorAction);

        float distanceToTarget = Vector3.Distance(this.transform.localPosition, Hunter.localPosition);

        // Reached target
        if (distanceToTarget < 1.42f)
        {
            SetReward(-1.0f);
            MyDone();
        }

        // Fell off platform
        if (this.transform.localPosition.y < 0)
        {
            SetReward(-1.0f);
            MyDone();
        }
    }

    public void MyDone()
    {
        Debug.Log("Reward: " + GetCumulativeReward().ToString() + "\n" +
            "MaxStep: " + agentParameters.maxStep);

        Done();
    }
}