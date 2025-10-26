using Ditzelgames;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class WaterFloat : MonoBehaviour
{
    //public properties
    public float AirDrag = 1;
    public float WaterDrag = 10;
    public bool AffectDirection = true;
    public bool AttachToSurface = false;
    public Transform[] FloatPoints;

    //used components
    protected Rigidbody Rigidbody;
    protected Waves Waves;

    //water line
    protected float WaterLine;
    protected Vector3[] WaterLinePoints;

    //help Vectors
    protected Vector3 smoothVectorRotation;
    protected Vector3 TargetUp;
    protected Vector3 centerOffset;

    public Vector3 Center { get { return transform.position + centerOffset; } }

    // Start is called before the first frame update
    void Start()
    {
        // validate float points
        if (FloatPoints == null || FloatPoints.Length == 0)
        {
            Debug.LogError("WaterFloat: FloatPoints is null or empty. Assign float point transforms in the Inspector.", this);
            return;
        }

        // get Waves (prefer inspector assignment; fallback to safe Find)
#if UNITY_2023_1_OR_NEWER
        if (Waves == null) Waves = FindObjectOfType<Waves>(true);
#else
        if (Waves == null) Waves = FindObjectOfType<Waves>();
#endif
        if (Waves == null)
            Debug.LogWarning("WaterFloat: no Waves instance found in scene. Water height queries will be skipped.", this);

        // get Rigidbody (fail fast so analyzer knows it's not null)
        Rigidbody = GetComponent<Rigidbody>() ?? throw new System.InvalidOperationException("WaterFloat requires a Rigidbody on the same GameObject.");
        Rigidbody.useGravity = false;

        // compute center and allocate arrays
        WaterLinePoints = new Vector3[FloatPoints.Length];
        for (int i = 0; i < FloatPoints.Length; i++)
            WaterLinePoints[i] = FloatPoints[i] != null ? FloatPoints[i].position : transform.position;
        try
        {
            centerOffset = PhysicsHelper.GetCenter(WaterLinePoints) - transform.position;
        }
        catch
        {
            centerOffset = Vector3.zero;
        }
    }

    // FixedUpdate is used for physics
    void FixedUpdate()
    {
        if (FloatPoints == null || FloatPoints.Length == 0 || Rigidbody == null)
            return;

        // default water surface
        var newWaterLine = 0f;
        var pointUnderWater = false;

        // set WaterLinePoints and WaterLine
        for (int i = 0; i < FloatPoints.Length; i++)
        {
            if (FloatPoints[i] == null) continue;

            // get sample world position
            WaterLinePoints[i] = FloatPoints[i].position;

            // if Waves is missing, skip height sampling
            if (Waves != null)
                WaterLinePoints[i].y = Waves.GetHeight(FloatPoints[i].position);
            else
                WaterLinePoints[i].y = transform.position.y; // fallback to object's y

            newWaterLine += WaterLinePoints[i].y / FloatPoints.Length;

            if (WaterLinePoints[i].y > FloatPoints[i].position.y)
                pointUnderWater = true;
        }

        var waterLineDelta = newWaterLine - WaterLine;
        WaterLine = newWaterLine;

        // compute up vector (guard PhysicsHelper)
        try
        {
            TargetUp = PhysicsHelper.GetNormal(WaterLinePoints);
        }
        catch
        {
            TargetUp = Vector3.up;
        }

        // apply drag
        Rigidbody.drag = pointUnderWater ? WaterDrag : AirDrag;

        // movement: attach or move towards surface using physics-friendly calls
        if (WaterLine > Center.y)
        {
            // under/at surface
            if (AttachToSurface)
            {
                var targetPos = new Vector3(Rigidbody.position.x, WaterLine - centerOffset.y, Rigidbody.position.z);
                Rigidbody.MovePosition(targetPos);
            }
            else
            {
                // go up relative to body using MovePosition (respecting physics)
                var upDelta = Vector3.up * waterLineDelta * 0.9f;
                var targetPos = Rigidbody.position + upDelta;
                Rigidbody.MovePosition(targetPos);
            }
        }

        // apply gravity-like force (use acceleration so mass doesn't matter)
        var gravity = Physics.gravity;
        if (WaterLine > Center.y)
        {
            gravity = AffectDirection ? TargetUp * -Physics.gravity.y : -Physics.gravity;
        }
        var forceAmount = Mathf.Clamp(Mathf.Abs(WaterLine - Center.y), 0f, 1f);
        Rigidbody.AddForce(gravity * forceAmount, ForceMode.Acceleration);

        // rotation: align to water normal (use MoveRotation)
        if (pointUnderWater)
        {
            TargetUp = Vector3.SmoothDamp(transform.up, TargetUp, ref smoothVectorRotation, 0.2f);
            // compute rotation that maps object's up to TargetUp
            var fromTo = Quaternion.FromToRotation(transform.up, TargetUp);
            var desiredRot = fromTo * Rigidbody.rotation;
            Rigidbody.MoveRotation(desiredRot);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        if (FloatPoints == null)
            return;

        for (int i = 0; i < FloatPoints.Length; i++)
        {
            if (FloatPoints[i] == null)
                continue;

            if (Waves != null && WaterLinePoints != null && i < WaterLinePoints.Length)
            {
                //draw cube
                Gizmos.color = Color.red;
                Gizmos.DrawCube(WaterLinePoints[i], Vector3.one * 0.3f);
            }

            //draw sphere
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(FloatPoints[i].position, 0.1f);
        }

        //draw center
        if (Application.isPlaying)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(new Vector3(Center.x, WaterLine, Center.z), Vector3.one * 1f);
            Gizmos.DrawRay(new Vector3(Center.x, WaterLine, Center.z), TargetUp * 1f);
        }
    }
}