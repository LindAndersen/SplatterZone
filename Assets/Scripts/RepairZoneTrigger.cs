using UnityEngine;

public class RepairZoneTrigger : MonoBehaviour
{
    [Header("References")]
    public Breakable breakable;

    void OnTriggerEnter(Collider other)
    {
        if (breakable == null || !other.gameObject.CompareTag("Player"))
            return;

        breakable.SetPlayerInRepairZone(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (breakable == null || !other.gameObject.CompareTag("Player"))
            return;

        breakable.SetPlayerInRepairZone(false);
    }
}
