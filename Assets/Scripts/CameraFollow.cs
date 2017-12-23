using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour {
    //What the camera will follow
    public Controller2D target;
    //The area around the player we want to focus on
    public Vector2 focusAreaSize;

    public float verticalOffset;
    public float LookAheadDistanceX;
    public float LookSmoothTimeX;
    public float verticalSmoothTime;

    focusArea FocusArea;

    float currentLookAheadX;
    float targetLookAheadX;
    float lookAheadDirectionX;
    float smoothLookVelocityX;
    float smoothVelocityY;

    bool lookAheadStopped;

    void Start()
    {
        FocusArea = new focusArea(target.collider.bounds, focusAreaSize);
        
        
    }
    void LateUpdate()
    {
        Vector2 focusPosition = FocusArea.centre + Vector2.up * verticalOffset;
        FocusArea.Update(target.collider.bounds);
        if (FocusArea.velocity.x != 0)
        {
            lookAheadDirectionX = Mathf.Sign(FocusArea.velocity.x);
            if (Mathf.Sign(target.playerInput.x) == Mathf.Sign(FocusArea.velocity.x) && target.playerInput.x != 0)
            {
                lookAheadStopped = false;
                targetLookAheadX = lookAheadDirectionX * LookAheadDistanceX;
            } 
            else
            {
                if (!lookAheadStopped)
                {
                    lookAheadStopped = true;
                    targetLookAheadX = currentLookAheadX + (lookAheadDirectionX * LookAheadDistanceX - currentLookAheadX) / 4f;
                }
            }
        }
        
        currentLookAheadX = Mathf.SmoothDamp(currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, LookSmoothTimeX);
        focusPosition.y = Mathf.SmoothDamp(transform.position.y, focusPosition.y, ref smoothVelocityY, verticalSmoothTime);
        focusPosition += Vector2.right * currentLookAheadX;
        transform.position = (Vector3)focusPosition + Vector3.forward * -10;

    }


    struct focusArea
    {
        public Vector2 velocity;
        public Vector2 centre;
        float left, right;
        float top, bottom;
        public focusArea(Bounds targetBounds, Vector2 Size)
        {
            left = targetBounds.center.x - Size.x/2;
            right = targetBounds.center.x + Size.x / 2;
            bottom = targetBounds.min.y;
            top = targetBounds.min.y + Size.y;
            velocity = Vector2.zero;
            centre = new Vector2((left + right) / 2, (top + bottom) / 2);
        }
        public void Update(Bounds TargetBounds)
        {
            float shiftX = 0;
            if (TargetBounds.min.x < left)
            {
                shiftX = TargetBounds.min.x - left;
            } 
            else if (TargetBounds.max.x > right)
            {
                shiftX = TargetBounds.max.x - right;
            }
            left += shiftX;
            right += shiftX;

            float shiftY = 0;
            if (TargetBounds.min.y < bottom)
            {
                shiftY = TargetBounds.min.y - bottom;
            }
            else if (TargetBounds.max.x >top)
            {
                shiftY = TargetBounds.max.y - top;
            }
            bottom += shiftY;
            top += shiftY;
            centre = new Vector2((left + right) / 2, (top + bottom) / 2);
            velocity = new Vector2(shiftX, shiftY);
        }
    }
	
	
	// Update is called once per frame
	void Update () {
		
	}
}
