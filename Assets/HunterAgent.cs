using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class HunterAgent : Agent
{
    public Transform Prey;
    public GameObject Arena;
    public float MaxStamina = 100;

    private RayPerception h_RayPer;
    private Rigidbody p_rBody;
    private Rigidbody h_rBody;
    private HunterAcademy m_Academy;
    private Vector3 LastKnownTargetPosition = Vector3.zero;
    private float Stamina;
    public override void InitializeAgent()
    {
        agentParameters.maxStep = 3000;
        base.InitializeAgent();
        m_Academy = FindObjectOfType<HunterAcademy>();
        h_RayPer = GetComponent<RayPerception>();
        h_rBody = GetComponent<Rigidbody>();
        p_rBody = Prey.GetComponent<Rigidbody>();
    }

    public override void AgentReset()
    {
        Stamina = MaxStamina;

        // If the Agent fell, zero its momentum
        this.h_rBody.angularVelocity = Vector3.zero;
        this.h_rBody.velocity = Vector3.zero;
        this.transform.localPosition = new Vector3(0, 0.5f, 0);
    }

    public override void CollectObservations()
    {
        var rayDistance = 24f;
        float[] rayAngles = { 20f, 60f, 90f, 120f, 160f };
        string[] detectableObjects = { "Prey", "Arena", "Terrain" };
        AddVectorObs(h_RayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));

        AddVectorObs(GetStepCount() / (float)agentParameters.maxStep);
        AddVectorObs(Stamina);

        // Agent velocity and position
        //AddVectorObs(h_rBody.velocity);
        //AddVectorObs(h_rBody.transform.localPosition);
        //AddVectorObs(h_rBody.rotation.y);

        // Last known target position
        // AddVectorObs(LastKnownTargetPosition);

        Monitor.Log("Hunter reward", GetCumulativeReward().ToString());
    }

    public void MoveAgent(float[] act)
    {
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        if (brain.brainParameters.vectorActionSpaceType == SpaceType.Continuous)
        {
            float speed = act[0];
            if (speed < 0)
                speed *= -1;

            if (speed > 0.5)
            {
                float staminaNeeded = speed - 0.3f;
                if (Stamina > staminaNeeded)
                    Stamina -= staminaNeeded;
                else
                    speed = 0.5f;
            }
            else
            {
                RegenStamina();
            }

            dirToGo = transform.forward * Mathf.Clamp(speed, -1f, 1f);
            rotateDir = transform.up * Mathf.Clamp(act[1], -1f, 1f);
        }
        else
        {
            var run = Mathf.FloorToInt(act[0]);
            var turn = Mathf.FloorToInt(act[1]);
            switch (run)
            {
                case 0:
                    RegenStamina();
                    break;
                case 1:
                    dirToGo = transform.forward * 0.5f;
                    RegenStamina();
                    break;
                case 2:
                    if (Stamina < 1)
                    {
                        dirToGo = transform.forward * 0.5f;
                    }
                    else
                    {
                        dirToGo = transform.forward * 1f;
                        Stamina -= 1;
                    }
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
        Monitor.Log("Nitro", Stamina / MaxStamina, transform);

        transform.Rotate(rotateDir, Time.deltaTime * 150f);
        //h_rBody.AddForce(dirToGo * m_Academy.hunterRunSpeed * Time.deltaTime, ForceMode.VelocityChange);
        transform.Translate(dirToGo * m_Academy.hunterRunSpeed * Time.deltaTime, Space.World);
    }

    private void RegenStamina(float multiplier = 1)
    {
        float staminaRegen = multiplier;
        if (Stamina > MaxStamina - staminaRegen)
            Stamina = MaxStamina;
        else
            Stamina += staminaRegen;
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