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
    [SerializeField] private float movementAcceleration = 14f;
    [SerializeField] private float maxMovementSpeed = 7f;
    [SerializeField] private float movementDrag = 0.3f;
    [SerializeField] private float spinControlThreshold = 3f;
    [SerializeField] private float fullControlSpin = 20f;
    [SerializeField] private float minimumControl = 0.4f;
    [SerializeField] private float movementSpinCost = 1.5f;
    [SerializeField] private float extraBowlPull = 4f;
    [SerializeField] private float maxPlanarSpeed = 14f;
    [SerializeField] private float overspeedBrake = 40f;
    [SerializeField] private LayerMask arenaLayer;

    [Header("Stability")]
    [SerializeField] private float uprightSpeed = 8f;
    [SerializeField] private float tiltResponse = 15f;
    [SerializeField] private float noStabilitySpin = 3f;
    [SerializeField] private float fullStabilitySpin = 25f;
    [SerializeField] private float minimumStability = 0.35f;

    [Header("Friction")]
    [SerializeField] private bool applyPhysicsMaterial = true;
    [SerializeField] private float slideFriction = 0.05f;
    [SerializeField] private float bounciness = 0.15f;

    [Header("Combat")]
    [SerializeField] private float knockbackForce = 3f;
    [SerializeField] private float minimumImpactSpeed = 1f;
    [SerializeField] private float maxImpactDamage = 15f;

    [Header("Combat Effects")]
    [SerializeField] private ParticleSystem impactParticles;
    [SerializeField] private CameraShake cameraShake;
    [SerializeField] private float shakeThreshold = 3f;
    [SerializeField] private float maxShakeIntensity = 0.18f;
    [SerializeField] private float maxShakeDuration = 0.12f;

    [Header("Death")]
    [SerializeField] private float deadLinearDamping = 3f;
    [SerializeField] private float toppleSpeed = 3f;

    [Header("Debug")]
    [SerializeField] private bool showDebug = false;

    [Header("Player")]
    [Range(1, 2)]
    public int player = 1;

    [Header("Components")]
    [SerializeField] private SpynerTip tip;

    private Rigidbody rb;
    private Vector3 movementInput;
    private float currentSpin;
    private bool isDead;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        RefreshTip();
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
            bounceCombine = PhysicsMaterialCombine.Minimum
        };

        foreach (Collider col in GetComponentsInChildren<Collider>(true))
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
        {
            ApplyBowlPull();
            return;
        }

        float dt = Time.fixedDeltaTime;

        float tipSpinLoss = tip != null
            ? tip.spinLossMultiplier
            : 1f;

        float loss = spinLossPerSecond * tipSpinLoss;

        if (movementInput.sqrMagnitude > 0.01f)
            loss += movementSpinCost * tipSpinLoss;

        Vector3 up = transform.up;

        float spin = Vector3.Dot(rb.angularVelocity, up);
        float newSpin = Mathf.MoveTowards(spin, 0f, loss * dt);

        rb.angularVelocity += up * (newSpin - spin);
        currentSpin = newSpin;

        float rawStability = Mathf.InverseLerp(
            noStabilitySpin,
            fullStabilitySpin,
            Mathf.Abs(newSpin)
        );

        float stability = Mathf.Lerp(
            minimumStability,
            1f,
            rawStability
        );

        Vector3 uprightTorque = Vector3.Cross(up, Vector3.up) * uprightSpeed;

        rb.AddTorque(
            uprightTorque * stability,
            ForceMode.Acceleration
        );

        Vector3 angularVelocity = rb.angularVelocity;
        Vector3 tiltVelocity = angularVelocity - Vector3.Project(angularVelocity, up);

        rb.AddTorque(
            -tiltVelocity * tiltResponse * stability,
            ForceMode.Acceleration
        );

        if (Mathf.Abs(newSpin) <= stopSpinThreshold)
        {
            Die();
            return;
        }

        ApplyBowlPull();
        Move(Mathf.Abs(newSpin));
        LimitPlanarSpeed(dt);
    }

    private void ReadMovementInput()
    {
        float horizontal = 0f;
        float vertical = 0f;

        if (player == 1)
        {
            if (Input.GetKey(KeyCode.A)) horizontal -= 1f;
            if (Input.GetKey(KeyCode.D)) horizontal += 1f;
            if (Input.GetKey(KeyCode.S)) vertical -= 1f;
            if (Input.GetKey(KeyCode.W)) vertical += 1f;
        }
        else
        {
            if (Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;
            if (Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
            if (Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;
            if (Input.GetKey(KeyCode.UpArrow)) vertical += 1f;
        }

        Vector3 input = new Vector3(horizontal, 0f, vertical);
        input = Vector3.ClampMagnitude(input, 1f);

        if (cameraTransform != null)
        {
            input = Quaternion.Euler(
                0f,
                cameraTransform.eulerAngles.y,
                0f
            ) * input;
        }

        movementInput = input;
    }

    private bool TryGetGround(out RaycastHit hit)
    {
        Vector3 rayOrigin = transform.position + Vector3.up * 2f;

        return Physics.Raycast(
            rayOrigin,
            Vector3.down,
            out hit,
            8f,
            arenaLayer,
            QueryTriggerInteraction.Ignore
        );
    }

    private void ApplyBowlPull()
    {
        if (!TryGetGround(out RaycastHit hit))
            return;

        Vector3 downhill = Vector3.ProjectOnPlane(Vector3.down, hit.normal);

        rb.AddForce(downhill * extraBowlPull, ForceMode.Acceleration);
    }

    private void Move(float spinSpeed)
    {
        if (movementInput.sqrMagnitude < 0.01f)
            return;

        if (!TryGetGround(out RaycastHit hit))
        {
            rb.AddForce(
                movementInput * movementAcceleration * 0.75f,
                ForceMode.Acceleration
            );
            return;
        }

        float control = Mathf.InverseLerp(
            spinControlThreshold,
            fullControlSpin,
            spinSpeed
        );

        control = Mathf.Lerp(minimumControl, 1f, control);

        float tipMovement = tip != null
            ? tip.movementMultiplier
            : 1f;

        Vector3 direction = Vector3.ProjectOnPlane(movementInput, hit.normal);

        if (direction.sqrMagnitude < 0.001f)
            return;

        direction.Normalize();

        Vector3 surfaceVelocity = Vector3.ProjectOnPlane(rb.linearVelocity, hit.normal);

        float targetSpeed = maxMovementSpeed * control * tipMovement;
        float speedAlongInput = Vector3.Dot(surfaceVelocity, direction);

        if (speedAlongInput < targetSpeed)
        {
            rb.AddForce(
                direction * movementAcceleration * control * tipMovement,
                ForceMode.Acceleration
            );
        }
    }

    private void LimitPlanarSpeed(float dt)
    {
        Vector3 velocity = rb.linearVelocity;
        Vector3 planar = new Vector3(velocity.x, 0f, velocity.z);
        float speed = planar.magnitude;

        if (speed <= maxPlanarSpeed)
            return;

        float newSpeed = Mathf.MoveTowards(speed, maxPlanarSpeed, overspeedBrake * dt);
        Vector3 limited = planar / speed * newSpeed;

        rb.linearVelocity = new Vector3(limited.x, velocity.y, limited.z);
    }

    private void OnCollisionEnter(Collision collision)
    {
        SpynerController opponent =
            collision.gameObject.GetComponentInParent<SpynerController>();

        if (opponent == null || opponent == this || isDead)
            return;

        if (AudioManager.Instance != null)
            AudioManager.Instance.PlayClash();

        float impactSpeed = collision.relativeVelocity.magnitude;

        if (impactSpeed < minimumImpactSpeed)
            return;

        ContactPoint contact = collision.GetContact(0);

        if (GetEntityId() < opponent.GetEntityId())
            PlayImpactEffects(contact.point, impactSpeed);

        SpynerTip opponentTip = opponent.tip;

        float attackMultiplier = tip != null
            ? tip.impactMultiplier
            : 1f;

        float resistanceMultiplier = opponentTip != null
            ? opponentTip.resistanceMultiplier
            : 1f;

        float damage = Mathf.Min(
            impactSpeed * collisionSpinLoss
                * attackMultiplier * resistanceMultiplier,
            maxImpactDamage
        );

        opponent.ApplySpinDamage(damage);

        Vector3 direction =
            opponent.transform.position - transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.001f)
        {
            direction.Normalize();

            float knockback = knockbackForce * attackMultiplier;

            Rigidbody opponentRb = opponent.GetComponent<Rigidbody>();

            if (opponentRb != null)
            {
                opponentRb.AddForce(
                    direction * knockback,
                    ForceMode.Impulse
                );
            }
        }
    }

    private void Die()
    {
        if (isDead)
            return;

        isDead = true;
        movementInput = Vector3.zero;

        rb.linearDamping = deadLinearDamping;

        Vector3 axis = Vector3.Cross(
            Vector3.up,
            Random.insideUnitSphere
        );

        if (axis.sqrMagnitude > 0.0001f)
        {
            rb.AddTorque(
                axis.normalized * toppleSpeed,
                ForceMode.VelocityChange
            );
        }
    }

    private void OnGUI()
    {
        if (!showDebug || rb == null)
            return;

        GUI.Label(
            new Rect(10, 10, 400, 100),
            $"Player: {player}\n" +
            $"Spin: {currentSpin:F1} rad/s\n" +
            $"Tilt: {Vector3.Angle(transform.up, Vector3.up):F1} deg\n" +
            $"Speed: {GetMovementSpeed():F1} m/s\n" +
            $"Dead: {isDead}"
        );
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
        currentSpin = newSpin;
    }

    public void AddSpin(float amount)
    {
        if (isDead)
            return;

        float direction = currentSpin < 0f ? -1f : 1f;

        rb.angularVelocity += transform.up * direction * amount;
    }

    public void RefreshTip()
    {
        tip = GetComponentInChildren<SpynerTip>();
    }

    public float GetSpinSpeed() =>
        Mathf.Abs(currentSpin);

    public float GetSpinNormalized() =>
        launchSpin > 0f
            ? Mathf.Clamp01(Mathf.Abs(currentSpin) / launchSpin)
            : 0f;

    public bool IsDead() => isDead;

    public float GetAttackMultiplier()
    {
        return tip != null ? tip.impactMultiplier : 1f;
    }

    public float GetMovementSpeed()
    {
        Vector3 velocity = rb.linearVelocity;

        return new Vector3(
            velocity.x, 0f, velocity.z
        ).magnitude;
    }

    private void PlayImpactEffects(Vector3 hitPoint, float impactSpeed)
    {
        if (impactParticles != null)
        {
            ParticleSystem particles = Instantiate(
                impactParticles,
                hitPoint,
                Quaternion.identity
            );

            particles.Play();

            Destroy(particles.gameObject, 2f);
        }

        if (cameraShake != null && impactSpeed >= shakeThreshold)
        {
            float strength = Mathf.InverseLerp(
                shakeThreshold,
                12f,
                impactSpeed
            );

            cameraShake.Shake(
                Mathf.Lerp(0.03f, maxShakeIntensity, strength),
                Mathf.Lerp(0.04f, maxShakeDuration, strength)
            );
        }
    }
}