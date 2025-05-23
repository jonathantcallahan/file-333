using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine.UIElements;

public class MoveToBallAgent: Agent
{
    [SerializeField] private Transform targetTransform;
    [SerializeField] private Material winMaterial;
    [SerializeField] private Material loseMaterial;
    [SerializeField] private MeshRenderer floorMeshRenderer;
    public override void OnEpisodeBegin()
    {
        transform.localPosition = new Vector3(Random.Range(-3f, +3f), 0, Random.Range(-3f, +3f));
        targetTransform.localPosition = new Vector3(Random.Range(-3f, +3f), 0, Random.Range(-3f, +3f));
    }
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(transform.localPosition);
        sensor.AddObservation(targetTransform.localPosition);
    }
    public override void OnActionReceived(ActionBuffers actions)
    {

        float moveX = actions.ContinuousActions[0];
        float moveZ = actions.ContinuousActions[1];
        float rotateY = actions.ContinuousActions[2];
        int shoot = actions.DiscreteActions[0];

        Debug.Log("Rotation is: " + actions.ContinuousActions[2]);


        float moveSpeed = 2f;
        transform.localPosition += new Vector3(moveX, 0, moveZ) * Time.deltaTime * moveSpeed;

        float rotationSpeed = 180f;
        float rotationAmount = rotateY * rotationSpeed * Time.deltaTime;
        transform.Rotate(0f, rotationAmount, 0f);

        if (Mathf.Abs(rotateY) > 0)
        {
            SetReward(-0.1f);
        }

        if (shoot == 1)
        {
            float raycastDistance = 20f;
            string targetTag = "Enemy";
            
            RaycastHit hit;
            bool hasHit = Physics.Raycast(
                transform.position,
                transform.forward,
                out hit,
                raycastDistance
            );
            Debug.DrawRay(transform.position, transform.forward * raycastDistance, Color.green);

            if (hasHit && hit.collider.CompareTag(targetTag))
            {
                SetReward(+1f);
                floorMeshRenderer.material = winMaterial;
                EndEpisode();
            }
            else
            {
                SetReward(-0.5f);
            }

        }

        SetReward(-0.01f);

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        ActionSegment<float> continuousActions = actionsOut.ContinuousActions;
        ActionSegment<int> discreteActions = actionsOut.DiscreteActions;
        continuousActions[0] = Input.GetAxisRaw("Horizontal");
        continuousActions[1] = Input.GetAxisRaw("Vertical");
        float rotateInput = 0f;
        if (Input.GetKey(KeyCode.Q)) rotateInput = -1f;
        if (Input.GetKey(KeyCode.E)) rotateInput = 1f;
        continuousActions[2] = rotateInput;

        if (Input.GetKey(KeyCode.Space))
        {
            discreteActions[0] = 1;
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if ( other.gameObject.tag == "Goal")
        {
            
        }
        if (other.gameObject.tag == "Wall")
        {
            SetReward(-1f);
            floorMeshRenderer.material = loseMaterial;
            EndEpisode();
        }
    }

}
