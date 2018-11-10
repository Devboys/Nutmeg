using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpMethodDump : MonoBehaviour {

    /*Standard jump with minimum jump-height. Cancel still needs to be made accurate. Jump cancel feels bad*/
    //private void HandleJump(bool jump)
    //{
    //    //make jump cancelable
    //    if (_velocity.y > 0 & Input.GetButtonUp("Jump"))
    //    {
    //        jumpCancelled = true;
    //    }

    //    if (currJumpTimer > 0)
    //    {
    //        //Only stop jump if minimum jump time has passed.
    //        if (jumpCancelled && (totalJumpTime - currJumpTimer) >= minimumJumpTime)
    //        {
    //            _velocity.y = 0;
    //            currJumpTimer = 0;
    //            jumpCancelled = false;
    //        }

    //        currJumpTimer -= Time.deltaTime;

    //    }

    //    if (jump)
    //    {
    //        //set upward speed to cancel out with gravity at jumpHeight;
    //        _velocity.y = Mathf.Sqrt(2f * maxJumpHeight * -gravity);

    //        //calculate the total time it will take for the jump to reach it's peak, and set the jumpTimer accordingly.
    //        totalJumpTime = (_velocity.y) / Mathf.Abs(gravity);
    //        currJumpTimer = totalJumpTime;
    //    }
    //}

    /*Half-finished per-frame controlled jump method*/
    //private void HandleFramePerfectJump(bool jump)
    //{

    //    if (jump)
    //    {
    //        currJumpTimer = jumpTime;
    //        currJumpSpeed = maxJumpHeight / jumpTime;

    //        jumpInitHeight = transform.position.y;
    //    }



    //    if(currJumpTimer > 0)
    //    {
    //        _velocity.y = ((currJumpTimer - Time.deltaTime) < 0) ? (currJumpSpeed * currJumpTimer) / Time.deltaTime : currJumpSpeed;
    //        currJumpTimer -= Time.deltaTime;

    //        if(currJumpTimer <= 0)
    //        {
    //            //Debug.Log(currJumpTimer);
    //            lastJumpFrame = true;
    //        }

    //    }

    //    else if (lastJumpFrame)
    //    {
    //        _velocity.y = 0;
    //        lastJumpFrame = false;
    //    }

    //    //debug
    //    float heightDif = transform.position.y - jumpInitHeight;

    //    if ((heightDif > jumpMaxHeight))
    //    {
    //        jumpMaxHeight = heightDif;
    //        Debug.Log(jumpMaxHeight);
    //    }

    //}
}
