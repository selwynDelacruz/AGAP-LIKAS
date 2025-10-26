using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class BuoyancyObject : MonoBehaviour
{
    public Transform[] floaters;
    public float underWaterDrag = 3f;
    public float underWaterAngularDrag = 1f;
    public float airDrag = 0f;
    public float airAngularDrag = 0.05f;
    public float floatingPower = 15f;

    WaterManager waterManager;
    Rigidbody m_Rigidbody;

    int floatersUnderwater;
    bool underwater;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        waterManager = Object.FindAnyObjectByType<WaterManager>();

        if (floaters == null || floaters.Length == 0)
        {
            Debug.LogError("BuoyancyObject: no floaters assigned. Please assign transforms in the inspector.", this);
            enabled = false;
            return;
        }

        if (m_Rigidbody == null)
        {
            Debug.LogError("BuoyancyObject: no Rigidbody found.", this);
            enabled = false;
            return;
        }
    }

    void FixedUpdate()
    {
        if (floaters == null || floaters.Length == 0 || m_Rigidbody == null) return;

        floatersUnderwater = 0;
        for (int i = 0; i < floaters.Length; i++)
        {
            if (floaters[i] == null) continue;

            float waterY = (waterManager != null) ? waterManager.WaterHeightAtPosition(floaters[i].position) : 0f;
            float difference = floaters[i].position.y - waterY;

            if (difference < 0f)
            {
                // Under water — apply buoyancy at this floater position
                m_Rigidbody.AddForceAtPosition(Vector3.up * floatingPower * Mathf.Abs(difference), floaters[i].position, ForceMode.Force);
                floatersUnderwater += 1;
                if (!underwater)
                {
                    underwater = true;
                    SwitchState(true);
                }
            }
        }

        // if previously underwater but no floaters now submerged
        if (underwater && floatersUnderwater == 0)
        {
            underwater = false;
            SwitchState(false);
        }
    }

    void SwitchState(bool isUnderwater)
    {
        if (m_Rigidbody == null) return;

        if (isUnderwater)
        {
            m_Rigidbody.linearDamping = underWaterDrag;
            m_Rigidbody.angularDamping = underWaterAngularDrag;
        }
        else
        {
            m_Rigidbody.linearDamping = airDrag;
            m_Rigidbody.angularDamping = airAngularDrag;
        }
    }
}
