using Unity.Netcode;
using UnityEngine;

public class SimplePlayerController : NetworkBehaviour
{
    public NetworkVariable<ulong> PlayerID = new NetworkVariable<ulong>();
    public float speed;
    public float jumpForce = 5f;
    public LayerMask groundLayer;
    private Rigidbody rb;

    private PlayerHealth playerHealth;

    private float originalSpeed;
    private float buffExpireTime = 0f;
    private bool hasSpeedBuff = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        originalSpeed = speed;
        playerHealth = GetComponent<PlayerHealth>();
    }

    void Update()
    {
        if (!IsOwner) return;
        if (!IsSpawned) return;

        float x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        float y = Input.GetAxis("Vertical") * speed * Time.deltaTime;

        if (x != 0 || y != 0)
        {
            MoveServerRpc(x, y);
        }

        if (IsGrounded() && Input.GetKeyDown(KeyCode.Space))
        {
            JumpServerRpc();
        }

        CheckBuffExpiration();
    }

    private void CheckBuffExpiration()
    {
        if (hasSpeedBuff && Time.time > buffExpireTime)
        {
            speed = originalSpeed;
            hasSpeedBuff = false;
            Debug.Log("Speed buff expired");
        }
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
    }

    public void ApplySpeedBuff(float multiplier, float duration)
    {
        speed = originalSpeed * multiplier;
        buffExpireTime = Time.time + duration;
        hasSpeedBuff = true;
        Debug.Log("Speed buff applied! New speed: " + speed);
    }

    public void TakeDamage(int damage)
    {
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(damage);
        }
        Debug.Log("Player took " + damage + " damage. Current health: " +
                 (playerHealth != null ? playerHealth.currentHealth.Value.ToString() : "N/A"));
    }

    [ServerRpc]
    private void MoveServerRpc(float x, float y)
    {
        if (rb != null && IsSpawned)
        {
            rb.MovePosition(rb.position + new Vector3(x, 0, y));
        }
    }

    [ServerRpc]
    private void JumpServerRpc()
    {
        if (rb != null && IsGrounded() && IsSpawned)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}