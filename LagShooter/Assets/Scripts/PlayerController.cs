using UnityEngine;
using Mirror;
 
public class PlayerController : NetworkBehaviour
{
    CharacterController characterController;
    public float MovementSpeed =1;
    public float Gravity = 9.8f;
    private float velocity = 0;
 
    private void Start()
    {
        characterController = GetComponent<CharacterController>();
    }
 
    void Update()
    {
       // if (!isLocalPlayer) { return; }
        // player movement - forward, backward, left, right
        float horizontal = Input.GetAxis("Horizontal") * MovementSpeed;
        float vertical = Input.GetAxis("Vertical") * MovementSpeed;
        characterController.Move((Vector3.right * horizontal + Vector3.forward * vertical) * Time.deltaTime);
 
        // Gravity
        if(characterController.isGrounded)
        {
            velocity = 0;
        }
        else
        {
            velocity -= Gravity * Time.deltaTime;
            characterController.Move(new Vector3(0, velocity, 0));
        }
    }
}