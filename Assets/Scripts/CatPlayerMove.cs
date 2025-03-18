using UnityEngine;
using UnityEngine.InputSystem;

public class CatPlayerMove : MonoBehaviour
{
    [SerializeField] private int speed;
    private Rigidbody2D rg;
    private Vector2 _movement;

    void Awake()
    {
        rg = GetComponent<Rigidbody2D>();
    }


    void FixedUpdate()
    {
        rg.linearVelocity = _movement * speed;
    }

    private void OnMove(InputValue input)
    {
        _movement = input.Get<Vector2>();
    }
}
