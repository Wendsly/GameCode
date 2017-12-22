using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent (typeof(BoxCollider2D))]
public class Controller2D : RaycastController {
    //Most of what this script does at the moment is controll the collision and speed at which the player falls. This is
    //all done with raycasting and velocity, its also done with a bit of math but that's just UNTIY MAGIC AS FAR AS I'M CONCERNED
    
    //This is set up so that we can apply a mask for the player to interact with
    public LayerMask collisionMask;

    float maxclimbangle = 80f;
    float maxDescendAngle = 75f;
    public collisionInfo collisions;
    // Use this for initialization
    public override void Start()
    {
        base.Start();
    }
    //This is the move function used in Player.cs 
    public void move(Vector3 velocity, bool StandingOnPlatform = false)
    {
        UpdateRaycastOrigins();
        //resets the collisions to false so that gravity doesn't constantly increase (obligotory Owen Wilson Wow)
        collisions.reset();
        collisions.velocityOld = velocity;
        if (velocity.y < 0)
        {
            descendSlope(ref velocity);
        }
        if (velocity.x != 0)
        {
            HorizontalCollisions(ref velocity);
        }
        if (velocity.y != 0)
        {
            VerticalCollisions(ref velocity);
        }
        transform.Translate(velocity);
        if (StandingOnPlatform)
        {
            collisions.below = true;
        }
    }
    void HorizontalCollisions(ref Vector3 velocity)
    {
        
        float directionX = Mathf.Sign(velocity.x);
        
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;
        
        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up*(HorizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.cyan);
            if (hit)
            {
                //This stops us getting stuck in stuff, it will use the next raycast to detect collisions. 
                if (hit.distance == 0)
                {
                    continue;
                }
                //get the angle of the surface
                //using global up and normal to get the angle 
                float slopeangle = Vector2.Angle(hit.normal, Vector2.up);
                //If we are climbing the slope we make it so that the velocity is moving up the slope
                if (i == 0 && slopeangle <= maxclimbangle)
                {
                    if (collisions.descendSlope)
                    {
                        collisions.descendSlope = false;
                        velocity = collisions.velocityOld;
                    }
                    float distanceToSlopeStart = 0;
                    //starting to climb a new slope
                    if (slopeangle != collisions.slopeAngleOld) {
                        distanceToSlopeStart = hit.distance - skinWidth; //this will make it so that if we go up on the slope it will touch the slope
                        velocity.x -= distanceToSlopeStart * directionX; //only uses the velocity it has once it reaches the slope 

                    }
                    climbSlope(ref velocity, slopeangle);
                    velocity.x += distanceToSlopeStart * directionX;
                }
                //if we are not climbing the slope then we change the collision so it knows we are on the ground. 
                if (!collisions.climbingSlope || slopeangle > maxclimbangle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;
                    if (collisions.climbingSlope)
                    {
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                    }
                    //if we've hit something and we are going left then collisions will be true
                    collisions.left = directionX == -1;
                    //same thing for right
                    collisions.right = directionX == 1;
                }
            }
        }
    }
    void VerticalCollisions(ref Vector3 velocity)
    {
        //This means if we are moving up velocity = 1 and if we are moving down velocity = -1
        float directionY = Mathf.Sign(velocity.y);
        //forces it to be posetive with absolute value, we are inset by skinwidth
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;
        //This checks the direction that we are moving in and draws the rays appropriatly
        for (int i = 0; i < verticalRayCount; i++)
        {
            //this means that if the direction in the y is -1 e.g. going down then the ray will be drawn in the bottom left 
            //other wise it is drawn in the top left.
            //? means if true
            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (VerticalRaySpacing * i + velocity.x);
            //this creates the variable hit which checks if the raycast hits any objects.
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);
            //this draws the raycasts in the debugger, we can take this out it's just to show how it works
            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.cyan);
            //
            if (hit)
            {
                //This sets the speed at which it will fall and then the direction, it will constantly gain speed over time
                //todo create max fall speed for those big drops were gonna be having :D
                velocity.y = (hit.distance - skinWidth) * directionY;
                //This stops it deciding to drop if it detects somthing underneath it that is further away. 
                //this sets the distance the ray can see to the distance that it is away from the object it is currently on. 
                
                rayLength = hit.distance;
                if (collisions.climbingSlope)
                {
                    velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);
                }
                //if we've hit something and we are falling then collisions will be true
                collisions.below = directionY == -1;
                //same thing for right
                collisions.above = directionY == 1;
            }
            //This stops us getting stuck in the slope, checks if there is another slope and then changes us to that rather than
            //continuing and then going too far into the slope
            if (collisions.climbingSlope)
            {
                float directionX = Mathf.Sign(velocity.x);
                rayLength = Mathf.Abs(velocity.x) + skinWidth;
                Vector2 NewRayOrigin = ((directionX == -1)?raycastOrigins.bottomLeft:raycastOrigins.bottomRight) + Vector2.up * velocity.y;
                RaycastHit2D hitSlope = Physics2D.Raycast(NewRayOrigin, Vector2.right * directionX, rayLength, collisionMask); 

                if (hitSlope)
                {
                    float slopeAngle = Vector2.Angle(hitSlope.normal, Vector2.up);
                    if (slopeAngle != collisions.slopeAngle)
                    {
                        velocity.x = (hitSlope.distance - skinWidth) * directionX;
                        collisions.slopeAngle = slopeAngle;
                    }
                }
            }
        }
    }
    void climbSlope(ref Vector3 velocity, float slopeAngle)
    {
        //trig time 
        //we need to solve for y so we need to do y = d*sin(theta) and same with cos but with x
        float moveDistance = Mathf.Abs(velocity.x);
        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        if (velocity.y <= climbVelocityY)
        {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
        }
    }
    void descendSlope(ref Vector3 velocity)
    {
        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle != 0 && slopeAngle<= maxDescendAngle)
            {
                if (Mathf.Sign(hit.normal.x) == directionX)
                {
                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {
                        float moveDistance = Mathf.Abs(velocity.x);
                        float desendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;
                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= desendVelocityY;

                        collisions.slopeAngle = slopeAngle;
                        collisions.descendSlope = true;
                        collisions.below = true;
                    }
                } 
            }

        }
    }

    

	//These are checks that I'm using to see where the player is and when the collisions need to be set 

	public struct collisionInfo
    {
        public bool above, below;
        public bool left,right;
        public bool climbingSlope;
        public bool descendSlope;
        public float slopeAngle, slopeAngleOld;
        public Vector3 velocityOld;
        public void reset()
        {
            //being able to set all of the bools to false. 
            above = below = false;
            left = right = false;
            climbingSlope = false;
            descendSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
        }
    }
}
