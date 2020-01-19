using System.Collections.Generic;
using UnityEngine;
using MLAgents;

public class PreyAgent : Agent
{
    public Transform Hunter;
    public GameObject Arena;
    public Transform[] Food;

    public float RunSpeed = 3F;
    public float RotationSpeed = 1F;
    public float SprintSpeedMultiplier = 1.5F;
    public float MaxStamina = 500F;

    private float Stamina = 0F;
    private int Eaten;

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
        foreach (var food in Food)
            RespawnObject(food);

        Stamina = MaxStamina;
    }

    public override void CollectObservations()
    {
        var rayDistance = 20f;
        float[] rayAngles = { 10f, 30f, 40f, 50f, 60f, 70f, 80f, 90f, 100f, 110f, 120f, 130f, 140f, 150f, 170f };
        string[] detectableObjects = { "Hunter", "Arena", "Food" };
        AddVectorObs(m_RayPer.Perceive(rayDistance, rayAngles, detectableObjects, 0f, 0f));

        // Steps remaining until environment reset
        AddVectorObs(GetStepCount() / (float)agentParameters.maxStep);

        // Disable sprint if low stamina
        if (Stamina <= 2)
            SetActionMask(0, 3);

        // Agent stamina
        AddVectorObs(Stamina / MaxStamina);

        Monitor.Log("Prey reward", GetCumulativeReward().ToString());
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
        MoveAgent(vectorAction);

        foreach (var food in Food)
        {
            float distanceToFood = Vector3.Distance(this.transform.localPosition, food.localPosition);

            // Ate Food
            if (distanceToFood < 1.0f)
            {
                Eaten++;
                AddReward(1.0f / Food.Length);
                food.position = new Vector3(0, -1000, 0);
                if (Eaten == Food.Length)
                {
                    Eaten = 0;
                    Done();
                }
            }
        }

        float distanceToHunter = Vector3.Distance(transform.localPosition, Hunter.localPosition);

        // Caught By Hunter
        if (distanceToHunter < 1.42f)
        {
            AddReward(-1.0f / Food.Length);
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

    public void Broadcasttest()
    {
        Debug.Log("broadsvaes");
    }
}