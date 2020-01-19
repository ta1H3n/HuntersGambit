using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class HunterAgent : Agent
{
    public Transform Prey;
    public GameObject Arena;

    public float RunSpeed = 3F;
    public float RotationSpeed = 1F;
    public float SprintSpeedMultiplier = 1.5F;
    public float MaxStamina = 500F;

    private float Stamina = 0F;

    RayPerception h_RayPer;
    Rigidbody p_rBody;
    Rigidbody h_rBody;
    HunterAcademy m_Academy;
    public override void InitializeAgent()
    {
        agentParameters.maxStep = 2000;
        base.InitializeAgent();
        m_Academy = FindObjectOfType<HunterAcademy>();
        h_RayPer = GetComponent<RayPerception>();
        h_rBody = GetComponent<Rigidbody>();
        p_rBody = Prey.GetComponent<Rigidbody>();
    }

    public override void AgentReset()
    {
        // If the Agent fell, zero its momentum
        this.transform.localPosition = new Vector3(0, 0.5f, 0);

        Stamina = MaxStamina;
    }

    public override void CollectObservations()
    {
        var rayDistance = 16f;
        float[] rayAngles = { 40f, 50f, 60f, 70f, 80f, 90f, 100f, 110f, 120f, 130f, 140f };
        string[] detectableObjects = { "Prey", "Arena", "Food" };
        AddVectorObs(h_RayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));

        // Steps remaining until environment reset
        AddVectorObs(GetStepCount() / (float)agentParameters.maxStep);

        // Agent stamina
        AddVectorObs(Stamina / MaxStamina);

        // Disable sprint if low stamina
        if (Stamina <= 2)
            SetActionMask(0, 3);

        Monitor.Log("Hunter reward", GetCumulativeReward().ToString());
        Monitor.Log("Stamina", Stamina / MaxStamina, transform);
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
                case 0:
                    Stamina += 3;
                    break;
                case 1:
                    dirToGo = transform.forward * 1f;
                    Stamina += 1;
                    break;
                case 2:
                    dirToGo = transform.forward * -0.5f;
                    Stamina += 1;
                    break;
                case 3:
                    dirToGo = transform.forward * 1f * SprintSpeedMultiplier;
                    Stamina -= 3;
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

        if (Stamina > MaxStamina)
            Stamina = MaxStamina;
        if (Stamina < 0)
            Stamina = 0;

        transform.Rotate(rotateDir, Time.deltaTime * 150f * RotationSpeed);
        transform.Translate(dirToGo * RunSpeed * Time.deltaTime, Space.World);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        //AddReward(-1f / agentParameters.maxStep);
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