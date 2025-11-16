using UnityEngine;

public class SafeZone : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player reached safe zone!");
            //GameManager.Instance.FinishGame();
        }
    }
}
        