using UnityEngine;

public class BoatCameraFollow : MonoBehaviour
{
    public Transform boat; // Assign the boat object here
    public Vector3 offset = new Vector3(0, 5, -10); // Adjust height & distance
    public float smoothSpeed = 5f;

    void LateUpdate()
    {
        if (boat == null) return;

        // Target position for smooth follow
        Vector3 targetPosition = boat.position + boat.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);

        // Look slightly upward to see the horizon
        Vector3 lookDirection = boat.position + (boat.forward * 10f);
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDirection - transform.position), smoothSpeed * Time.deltaTime);
    }
}