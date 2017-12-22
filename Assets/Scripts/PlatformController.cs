using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlatformController : RaycastController
{
    List<PassengerMovement> passengerMovement;
    public LayerMask passengerMask;

    public float speed;
    int fromwaypointIndex;
    float percentBetweenWaypoints;

    //Remember this when we set up the bomb, super useful :D
    public float waitTime;
    float nextmovetime;
    public bool Cyclic;
    //Limit the range between 1 and 3 because above that will not be useful, will discuse with designers. 
    [Range(0,2)]
    public float easeAmount;
    //This stors positions of waypoints.
    public Vector3[] localWaypoints;
    
    public Vector3[] globalWaypoints;
    Dictionary<Transform, Controller2D> passengerDictionary = new Dictionary<Transform, Controller2D>();
    // Use this for initialization
    public override void Start()
    {
        base.Start();

        globalWaypoints = new Vector3[localWaypoints.Length];
        //This sets up the waypoints that we will use and then makes sure that they don't move when the platforms move.
        for (int i = 0; i <localWaypoints.Length; i++)
        {
            globalWaypoints[i] = localWaypoints[i] + transform.position;
        }

    }

    // Update is called once per frame
    void Update()
    {
        UpdateRaycastOrigins();

        Vector3 velocity = CalculatePlatformMovement();

        CalculatePassengerMovement(velocity);

        MovePassengers(true);
        transform.Translate(velocity);
        MovePassengers(false);
    }
    float Ease(float x)
    {
        float a = easeAmount + 1; // This will mean that if the ease amount is 0 then it will be defaulted to 1 
        //This is done because 1 should always be the defualt. 

        //This is the equation used for easing
        return (Mathf.Pow(x, a) / (Mathf.Pow(x,a)) + Mathf.Pow(1-x,a));
    }
    Vector3 CalculatePlatformMovement()
    {
        //setting up a wait timer
        if (Time.time < nextmovetime)
        {
            return Vector3.zero;
        }
        //This resets it to 0 when it reaches the length of the array.
        fromwaypointIndex %= globalWaypoints.Length;
        //get the next point in the index
        int toWaypointIndex = (fromwaypointIndex + 1) % globalWaypoints.Length;
        //get the distance between the points
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromwaypointIndex], globalWaypoints[toWaypointIndex]);
        //Find the percentage of distance and do it over time. 
        percentBetweenWaypoints += Time.deltaTime * speed/distanceBetweenWaypoints;
        //This is just to protect the ease function from giving wierd results
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);
        //Move between the points
        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromwaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);
        //This tells the platform what to do when it reaches the end of the array
        if (percentBetweenWaypoints >= 1)
        {
            //this resets the percentage to 0 so that it can move again
            percentBetweenWaypoints = 0;
            //this goes to the next point in the array
            fromwaypointIndex++;
            if (!Cyclic)
            {
                //if it reaches the end of the array we reset and reverse the array.
                if (fromwaypointIndex >= globalWaypoints.Length - 1)
                {
                    fromwaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextmovetime = Time.time + waitTime;
        }
        //calculate the amount left to move
        return newPos - transform.position;
    }

    void MovePassengers(bool beforMovePlatform)
    {
        foreach (PassengerMovement passenger in passengerMovement)
        {
            //This makes sure we only get one component call.
            if (!passengerDictionary.ContainsKey(passenger.transform))
            {
                passengerDictionary.Add(passenger.transform, passenger.transform.GetComponent<Controller2D>());
            }
            if (passenger.moveBeforePlatform == beforMovePlatform)
            {
                passengerDictionary[passenger.transform].move(passenger.velovity, passenger.standingOnPlatform);
            }
        }
    }

    void CalculatePassengerMovement(Vector3 velocity)
    {
        HashSet<Transform> movedPassengers = new HashSet<Transform>();
        passengerMovement = new List<PassengerMovement>();

        float directionX = Mathf.Sign(velocity.x);
        float directionY = Mathf.Sign(velocity.y);

        //vertically moving platform
        if (velocity.y != 0)
        {
            //doing the collision 
            float rayLength = Mathf.Abs(velocity.y) + skinWidth;
            for (int i = 0; i < verticalRayCount; i++)
            {

                Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (VerticalRaySpacing * i);
                //this creates the variable hit which checks if the raycast hits any objects.
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, passengerMask);
                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float pushX = (directionY == 1) ? velocity.x : 0;
                        float pushY = velocity.y - (hit.distance - skinWidth) * directionY;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), directionY == 1, true));
                    }
                }
            }


        }
        //Horizontally Moving Platform
        if (velocity.x != 0)
        {
            float rayLength = Mathf.Abs(velocity.x) + skinWidth;

            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (HorizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, passengerMask);
                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float pushX = velocity.x - (hit.distance - skinWidth) * directionX;
                        float pushY = -skinWidth;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), false, true));
                    }
                }
            }
        }
        //passenger is on top of a horizontally or downward moving platform Wont bounce and will move with it 
        if (directionY == -1 || velocity.y == 0 && velocity.x != 0)
        {
            float rayLength = skinWidth * 2;

            for (int i = 0; i < horizontalRayCount; i++)
            {
                Vector2 rayOrigin = raycastOrigins.topLeft + Vector2.right * (VerticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, passengerMask);
                if (hit)
                {
                    if (!movedPassengers.Contains(hit.transform))
                    {
                        movedPassengers.Add(hit.transform);
                        float pushX = velocity.x;
                        float pushY = velocity.y;

                        passengerMovement.Add(new PassengerMovement(hit.transform, new Vector3(pushX, pushY), true, false));
                    }
                }
            }
            }
    }
    struct PassengerMovement
    {
        public Transform transform;
        public Vector3 velovity;
        public bool standingOnPlatform;
        public bool moveBeforePlatform;

        public PassengerMovement(Transform _transform, Vector3 _velocity, bool _standingOnPlatform, bool _moveBeforePlatform)
        {
            transform = _transform;
            velovity = _velocity;
            standingOnPlatform = _standingOnPlatform;
            moveBeforePlatform = _moveBeforePlatform;
        }
    }
    //This is just some code so that we can show the waypoints in the sceene and place them accordingly
    void OnDrawGizmos()
    {
        if (localWaypoints != null)
        {
            Gizmos.color = Color.green;
            float size = .3f;
            for (int i = 0; i < localWaypoints.Length; i++)
            {
                //change local position to global position to display it
                //Checking if the aplication is playing, if it is we use the global waypoints otherwise we use the local ones
                Vector3 globalWaypointPos = (Application.isPlaying)?globalWaypoints[i]: localWaypoints[i] + transform.position;
                //Draw the gizmo at the local position
                Gizmos.DrawLine(globalWaypointPos - Vector3.up * size, globalWaypointPos + Vector3.up * size);//This will draw a cross + <---LIKE DIS
                Gizmos.DrawLine(globalWaypointPos - Vector3.left * size, globalWaypointPos + Vector3.left * size);
            }
        }
    }
}
