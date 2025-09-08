using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;
using Unity.Netcode.Components;

public class PLayer : NetworkBehaviour
{
    private InputSystem_Actions action;
    public Animator animator;
    public Rigidbody rb;
    public LayerMask groundLayer;
    public GameObject projectilePrefab;
    public Transform firePoint;

    public float jumpForce;
    public float speed;
    public bool OnMove = false;
    public Vector2 moveInput = Vector2.zero;
    public void OnEnable()
    {
        action.Enable();
        action.Player.Move.performed += OnMovePerformed;
        action.Player.Move.canceled += OnMoveCanceled;
        action.Player.Jump.performed += OnJump;
        action.Player.Attack.performed += OnAttack;
    }



    public void OnDisable()
    {

        action.Player.Move.performed -= OnMovePerformed;
        action.Player.Move.canceled -= OnMoveCanceled;
        action.Player.Jump.performed -= OnJump;
        action.Disable();
    }
    private void Awake()
    {
        action = new InputSystem_Actions();
    }
    void Start()
    {

    }
    void Update()
    {
        if (!IsOwner) return;

        Movement();
        GroundCheckRpc();
    }

    public void Movement()
    {
        if (OnMove) MovementRpc(moveInput);

    }

    [Rpc(SendTo.Server)]
    public void MovementRpc(Vector2 moveInput)
    {
        Vector3 move = new Vector3(moveInput.x, rb.linearVelocity.y, moveInput.y);
        rb.linearVelocity = move * speed;

    }
    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
        OnMove = true;
    }
    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        OnMove = false;
    }
    private void OnJump(InputAction.CallbackContext context)
    {
        PerformJumpRpc();
    }
    private void OnAttack(InputAction.CallbackContext context)
    {
        if (!IsOwner) return;
        ShootRpc();
    }
    [Rpc(SendTo.Server)]
    public void PerformJumpRpc()
    {
        transform.GetComponent<Rigidbody>().AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        animator.SetTrigger("Jump");
    }
    [Rpc(SendTo.Server)]
    public void GroundCheckRpc()
    {
        if (Physics.Raycast(transform.position, Vector3.down, 1.1f, groundLayer))
        {
            animator.SetBool("Grounded", true);
            animator.SetBool("FreeFall", false);
        }
        else
        {
            animator.SetBool("Grounded", false);
            animator.SetBool("FreeFall", true);
        }
    }

    [Rpc(SendTo.Server)]
    public void ShootRpc()
    {
        GameObject proj = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        proj.GetComponent<NetworkObject>().Spawn(true);

        proj.GetComponent<Rigidbody>().AddForce(Vector3.forward * 5, ForceMode.Impulse);
    }
}
