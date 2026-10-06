using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpynerController : MonoBehaviour
{
    [Header("Spin")]
    [SerializeField] private float startingSpin = 50f;
    [SerializeField] private float spinAcceleration = 500f;
    [SerializeField] private float spinDrag = 0.15f;

    [Header("Stability")]
    [SerializeField] private float uprightForce = 15f;
    [SerializeField] private float uprightTorque = 10f;

    private Rigidbody rb;
    private float currentSpin;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        // physics based spinning (note for self: search stack overflow for thread to overcome unity rb rotation limitation)
        rb.maxAngularVelocity = 100f;
        currentSpin = startingSpin;
    }

    private void Start()
    {
        Launch();
    }

    private void FixedUpdate()
    {
        Spin();
        Stabilize();
        DrainSpin();
    }

    private void Spin()
    {
        if (currentSpin <= 0f)
        {
            return;
        }

        Vector3 spinAxis = transform.up; 

        rb.AddTorque(spinAxis * spinAcceleration, ForceMode.Force);
    }

    private void Stabilize()
    {
        Vector3 tiltAxis = Vector3.Cross(transform.up, Vector3.up);

        rb.AddTorque(tiltAxis * uprightTorque, ForceMode.Force);
    }

    private void DrainSpin()
    {
        currentSpin -= spinDrag * Time.fixedDeltaTime;
        currentSpin = Mathf.Max(currentSpin, 0f);
    }

    public void Launch()
    {
        currentSpin = startingSpin;

        // initial spin
        rb.AddTorque(transform.up * startingSpin, ForceMode.Impulse);
    }

    public float GetSpin()
    {
        return currentSpin;
    }
}

