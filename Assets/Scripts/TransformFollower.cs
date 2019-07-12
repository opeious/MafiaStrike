using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransformFollower : MonoBehaviour
{
    [SerializeField] private bool FollowPosition;

    [SerializeField] private Transform TransformToFollow;

    [SerializeField] private Rigidbody RotateToDirectionOfRb;

    private void FixedUpdate()
    {
        if (FollowPosition)
        {
            gameObject.transform.position = TransformToFollow.position;   
        }

        if (RotateToDirectionOfRb != null)
        {
//            Debug.LogError(RotateToDirectionOfRb.velocity);
        }
    }
}
