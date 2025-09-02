using Unity.Netcode;
using UnityEngine;

public class SimplePlayerController : NetworkBehaviour
{
    public NetworkVariable<ulong> PlayerID = new NetworkVariable<ulong>();
    public float speed;
    public float jumpForce = 5f;
    public LayerMask groundLayer;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (!IsOwner) return;

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
    }

    private bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer);
    }

    [ServerRpc]
    private void MoveServerRpc(float x, float y)
    {
        if (rb != null)
        {
            rb.MovePosition(rb.position + new Vector3(x, 0, y));
        }
    }

    [ServerRpc]
    private void JumpServerRpc()
    {
        if (rb != null && IsGrounded())
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}