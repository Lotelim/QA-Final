using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowThePath : MonoBehaviour {

    [HideInInspector] public Transform [] path; 
    [HideInInspector] public float speed;
    [HideInInspector] public bool rotationByPath;   
    [HideInInspector] public bool loop;         
    float currentPathPercent;               
    Vector3[] paddedPathPositions;          
    [HideInInspector] public bool movingIsActive;  

    public void SetPath()
    {
        currentPathPercent = 0;
        Vector3[] pathPositions = new Vector3[path.Length];       
        for (int i = 0; i < pathPositions.Length; i++)
        {
            pathPositions[i] = path[i].position;
        }
        paddedPathPositions = SplineUtility.PadForCatmullRom(pathPositions);
        transform.position = SplineUtility.Interpolate(paddedPathPositions, 0); 
        if (!rotationByPath)
            transform.rotation = Quaternion.identity;
        movingIsActive = true;
    }

    private void Update()
    {
        if (movingIsActive)
        {
            currentPathPercent += speed / 100 * Time.deltaTime;     
            transform.position = SplineUtility.Interpolate(paddedPathPositions, currentPathPercent); 
            if (rotationByPath)                            
            {
                transform.right = SplineUtility.Interpolate(paddedPathPositions, currentPathPercent + 0.01f) - transform.position;
                transform.Rotate(Vector3.forward * 90);
            }
            if (currentPathPercent > 1)                    
            {
                if (loop)                                   
                    currentPathPercent = 0;
                else
                {
                    Destroy(gameObject);
                }
            }
        }
    }
}
