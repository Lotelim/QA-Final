using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This script moves the ‘Enemy’ along the defined path.
/// </summary>
public class FollowThePath : MonoBehaviour {

    [HideInInspector] public Transform [] path; //path points which passes the 'Enemy'
    [HideInInspector] public float speed;
    [HideInInspector] public bool rotationByPath;   //whether 'Enemy' rotates in path direction or not
    [HideInInspector] public bool loop;         //if loop is true, 'Enemy' returns to the path starting point after completing the path
    float currentPathPercent;               //current percentage of completing the path
    Vector3[] paddedPathPositions;           //path points padded for Catmull-Rom, cached once per SetPath() instead of rebuilt every frame
    [HideInInspector] public bool movingIsActive;   //whether 'Enemy' moves or not

    //setting path parameters for the 'Enemy' and sending the 'Enemy' to the path starting point
    public void SetPath()
    {
        currentPathPercent = 0;
        Vector3[] pathPositions = new Vector3[path.Length];       //transform path points to vector3
        for (int i = 0; i < pathPositions.Length; i++)
        {
            pathPositions[i] = path[i].position;
        }
        paddedPathPositions = SplineUtility.PadForCatmullRom(pathPositions);
        transform.position = SplineUtility.Interpolate(paddedPathPositions, 0); //sending the enemy to the path starting point
        if (!rotationByPath)
            transform.rotation = Quaternion.identity;
        movingIsActive = true;
    }

    private void Update()
    {
        if (movingIsActive)
        {
            currentPathPercent += speed / 100 * Time.deltaTime;     //every update calculating current path percentage according to the defined speed

            transform.position = SplineUtility.Interpolate(paddedPathPositions, currentPathPercent); //moving the 'Enemy' to the path position
            if (rotationByPath)                            //rotating the 'Enemy' in path direction, if set 'rotationByPath'
            {
                transform.right = SplineUtility.Interpolate(paddedPathPositions, currentPathPercent + 0.01f) - transform.position;
                transform.Rotate(Vector3.forward * 90);
            }
            if (currentPathPercent > 1)                    //when the path is complete
            {
                if (loop)                                   //when loop is set, moving to the path starting point; if not, destroying or deactivating the 'Enemy'
                    currentPathPercent = 0;
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
