using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour {
    Vector3 Velocity;
    public float moveSpeed = 6;
    public float MaxjumpHeight = 4f;
    public float MinJumpHeight = 1f;
    //time to get to the top
    public float timeToJumpApex = .4f;

    float MaxJumpVelocity;
    float MinJumpVelocity;
    float gravity;

    float velocityXSmoothing;
    public float accelerationTimeAirborn = .2f;
    public float accelerationTimeGrounded = .1f; 
    Controller2D controller;
	void Start () {
        controller = GetComponent<Controller2D>();
        //This makes the gravity negativ and feel more natural
        gravity = (-2 * MaxjumpHeight) / Mathf.Pow(timeToJumpApex,2);
        //We use ABS to make gravity positive. 
        MaxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        MinJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * MinJumpHeight);
        
    }
	
	// Update is called once per frame
	void Update () {
       
        //This is setup for a controller (movement only so far) and keyboard
        //TODO add a dash because we need one for the sick dodges. Add full controller support not just movement. 
        //TODo ADd a c
        if (Input.GetKeyDown(KeyCode.W) && controller.collisions.below)
        {
            Velocity.y = MaxJumpVelocity;
        }
        if (Input.GetKeyUp(KeyCode.W))
        {
            if (Velocity.y > MinJumpVelocity)
            {
                Velocity.y = MinJumpVelocity;
            }
        }
        
        Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        float targetVelocityX = input.x * moveSpeed;
        //This does smooth acceleration for changing direction and changes depending on if we are in the air or not.
        //Im not sure if I like it so we can take it out later if we decide to. 
        Velocity.x = Mathf.SmoothDamp(Velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below)?accelerationTimeGrounded:accelerationTimeAirborn);
        Velocity.y += gravity * Time.deltaTime;
        controller.move(Velocity * Time.deltaTime, input);
        //call this after because controller.move modifies it. 
        //if we are on the ground we don't accumulate gravity
        if (controller.collisions.above || controller.collisions.below)
        {
            Velocity.y = 0;
        }
    }

}
