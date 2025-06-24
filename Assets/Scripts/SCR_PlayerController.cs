using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

public class SCR_PlayerController : MonoBehaviour
{
    private Rigidbody2D playerRB;
    [SerializeField] float gravity = -9.81f;
    public float moveSpeed = 5f;
    public float runSpeed = 12f;
    public float jumpHeight = 2f;
    private Vector3 velocity;
    private float ySpeed;

    // Start is called before the first frame update
    void Start()
    {
        playerRB = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        Move();
    }

    private void Move()
    {
    }
}
