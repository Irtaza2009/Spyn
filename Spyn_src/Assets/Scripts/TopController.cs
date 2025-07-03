using UnityEngine;

public class TopController : MonoBehaviour
{
    public float maxSpinTorque = 500f;
    public float spinDecayRate = 50f; // per second
    public float moveForce = 10f;

    private Rigidbody rb;
    private float currentSpin;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.maxAngularVelocity = 100f;
        currentSpin = maxSpinTorque;
    }

    void FixedUpdate()
    {
        // Apply spin if stamina remains
        if (currentSpin > 450f)
        {
            rb.AddTorque(transform.up * currentSpin * Time.fixedDeltaTime, ForceMode.VelocityChange);
            currentSpin -= spinDecayRate * Time.fixedDeltaTime;
        }

        // Player movement
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        Vector3 moveDir = new Vector3(h, 0f, v).normalized;
        if (moveDir.magnitude > 0.1f)
        {
            rb.AddForce(moveDir * moveForce, ForceMode.Acceleration);
        }
    }
}
