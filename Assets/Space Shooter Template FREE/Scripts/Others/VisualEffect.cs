using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualEffect : MonoBehaviour {

    [Tooltip("the time after object will be destroyed")]
    public float destructionTime;

    private void OnEnable()
    {
        StartCoroutine(Destruction()); 
    }

    IEnumerator Destruction() 
    {
        yield return new WaitForSeconds(destructionTime); 
        Destroy(gameObject);
    }
}
