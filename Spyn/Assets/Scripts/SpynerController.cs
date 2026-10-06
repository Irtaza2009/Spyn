using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class SpynerController : MonoBehaviour
{
    [Header("Launch")]
    [SerializeField] private float launchSpin = 40f;

    [Header("Spin")]
    [SerializeField] private float spinLossPerSecond = 1.2f;
    [SerializeField] private float collisionSpinLoss = 0.8f;
    [SerializeField] private float stopSpinThreshold = 1f;

    [Header("Movement")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float movementAcceleration = 20f;
    [SerializeField] private float maxMovementSpeed = 8f;
    [SerializeField] private float movementDrag = 1f;
    [SerializeField] private float spinControlThreshold = 3f;
    [SerializeField] private float fullControlSpin = 20f;
    [SerializeField] private float movementSpinCost = 1.5f;

    [Header("Stability")]
    [SerializeField] private float uprightSpeed = 8f;
    [SerializeField] private float tiltResponse = 15f;
    [SerializeField] private float noStabilitySpin = 3f;
    [SerializeField] private float fullStabilitySpin = 25f;

    [Header("Friction")]
    [SerializeField] private bool applyPhysicsMaterial = true;
    [SerializeField] private float slideFriction = 0.1f;
    [SerializeField] private float bounciness = 0.4f;

    [Header("Death")]
    [SerializeField] private float deadLinearDamping = 3f;
    [SerializeField] private float toppleSpeed = 3f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = true;

    private Rigidbody rb;
    private Vector3 movementInput;
    private float currentSpin;
    private bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        RefreshPhysics();
    }

    private void Start()
    {
        Launch();
    }

    public void RefreshPhysics()
    {
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.angularDamping = 0f;
        rb.maxAngularVelocity = 200f;

        if (!applyPhysicsMaterial)
            return;

        PhysicsMaterial material = new PhysicsMaterial("SpynSlide")
        {
            dynamicFriction = slideFriction,
            staticFriction = slideFriction,
            bounciness = bounciness,
            frictionCombine = PhysicsMaterialCombine.Minimum,
            bounceCombine = PhysicsMaterialCombine.Maximum
        };

        foreach (Collider col in GetComponentsInChildren<Collider>())
            col.sharedMaterial = material;
    }

    private void Update()
    {
        if (isDead)
        {
            movementInput = Vector3.zero;
            return;
        }

        ReadMovementInput();
    }

    private void FixedUpdate()
    {
        if (isDead)
            return;

        float dt = Time.fixedDeltaTime;
        Vector3 up = transform.up;

        Vector3 angularVelocity = rb.angularVelocity;
        float spin = Vector3.Dot(angularVelocity, up);
        Vector3 tilt = angularVelocity - up * spin;

        float loss = spinLossPerSecond;
        if (movementInput.sqrMagnitude > 0.01f)
            loss += movementSpinCost;

        spin = Mathf.MoveTowards(spin, 0f, loss * dt);
        currentSpin = spin;

        float stability = Mathf.InverseLerp(noStabilitySpin, fullStabilitySpin, Mathf.Abs(spin));
        Vector3 desiredTilt = Vector3.Cross(up, Vector3.up) * uprightSpeed;
        tilt = Vector3.Lerp(tilt, desiredTilt, 1f - Mathf.Exp(-tiltResponse * stability * dt));

        rb.angularVelocity = tilt + up * spin;

        if (Mathf.Abs(spin) <= stopSpinThreshold)
        {
            Die();
            return;
        }

        Move(Mathf.Abs(spin));
    }

    private void ReadMovementInput()
    {
        Vector3 input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));
        input = Vector3.ClampMagnitude(input, 1f);

        if (cameraTransform != null)
            input = Quaternion.Euler(0f, cameraTransform.eulerAngles.y, 0f) * input;

        movementInput = input;
    }

    private void Move(float spinSpeed)
    {
        if (movementInput.sqrMagnitude < 0.01f)
            return;

        float control = Mathf.InverseLerp(spinControlThreshold, fullControlSpin, spinSpeed);
        if (control <= 0f)
            return;

        Vector3 velocity = rb.linearVelocity;
        Vector3 flat = new Vector3(velocity.x, 0f, velocity.z);
        float speedAlongInput = Vector3.Dot(flat, movementInput.normalized);

        if (speedAlongInput < maxMovementSpeed)
            rb.AddForce(movementInput * movementAcceleration * control, ForceMode.Acceleration);
    }

    private void OnCollisionEnter(Collision collision)
    {
        Rigidbody otherBody = collision.rigidbody;
        if (otherBody == null || otherBody.GetComponent<SpynerController>() == null)
            return;

        ApplySpinDamage(collision.relativeVelocity.magnitude * collisionSpinLoss);
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        movementInput = Vector3.zero;
        rb.linearDamping = deadLinearDamping;

        Vector3 axis = Vector3.Cross(Vector3.up, Random.insideUnitSphere);
        if (axis.sqrMagnitude > 0.0001f)
            rb.AddTorque(axis.normalized * toppleSpeed, ForceMode.VelocityChange);
    }

    private void OnGUI()
    {
        if (!showDebug || rb == null)
            return;

        GUI.Label(new Rect(10, 10, 400, 100),
            $"spin: {currentSpin:F1} rad/s\n" +
            $"tilt: {Vector3.Angle(transform.up, Vector3.up):F1} deg\n" +
            $"speed: {rb.linearVelocity.magnitude:F1} m/s\n" +
            $"dead: {isDead}");
    }

    public void Launch()
    {
        isDead = false;
        movementInput = Vector3.zero;
        currentSpin = launchSpin;

        rb.linearDamping = movementDrag;
        rb.angularVelocity = transform.up * launchSpin;
    }

    public void ApplySpinDamage(float amount)
    {
        if (isDead)
            return;

        Vector3 up = transform.up;
        float spin = Vector3.Dot(rb.angularVelocity, up);
        float newSpin = Mathf.MoveTowards(spin, 0f, amount);
        rb.angularVelocity += up * (newSpin - spin);
    }

    public void AddSpin(float amount)
    {
        if (isDead)
            return;

        float direction = currentSpin < 0f ? -1f : 1f;
        rb.angularVelocity += transform.up * direction * amount;
    }

    public float GetSpinSpeed() => Mathf.Abs(currentSpin);
    public float GetSpinNormalized() => Mathf.Clamp01(Mathf.Abs(currentSpin) / launchSpin);
    public bool IsDead() => isDead;
}