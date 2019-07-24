using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraLerp : MonoBehaviour

{

    public static CameraLerp Instance;

    private Vector3 backupPos;
    private Quaternion backupRot;

    Transform target;
    Vector3 targetPos;
    Transform targetRot;

    bool currentlyLerping = false;

    bool currentlyReturning = false;

    private void Awake() {
        StartCoroutine(PostStartup());
        Instance = this;
    }

    IEnumerator PostStartup() {
        yield return new WaitForSeconds(8);
        GetComponent<Cinemachine.CinemachineBrain>().enabled = false;
        backupPos = transform.position;
        backupRot = transform.rotation;
    }

    private void OnDestroy() {
        Instance = null;
    }

    public void StartReturning() {
        currentlyLerping = false;
        currentlyReturning = true;
    }

    public void StartLerping(Transform t, Vector3 pos, Transform rot) {
        target = t;
        targetPos = pos;
        targetRot = rot;
        currentlyLerping = true;
    }

    // Update is called once per frame
    void Update()
    {
        if(currentlyLerping) {
            transform.position = Vector3.Lerp(transform.position, target.position, 0.1f);
            transform.rotation = Quaternion.Lerp(transform.rotation, target.rotation, 0.1f);
        }
        if(currentlyReturning) {
            gameObject.transform.position = Vector3.Lerp(transform.position, backupPos, 0.1f);
            gameObject.transform.rotation = Quaternion.Lerp(transform.rotation, backupRot, 0.1f);
        }
    }

}