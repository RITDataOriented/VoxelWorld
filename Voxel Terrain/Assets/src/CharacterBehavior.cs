using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBehavior : MonoBehaviour {
    public Transform Testing;

	// Use this for initialization
	void Start () {
        Testing = Transform.Instantiate(Resources.Load<Transform>("TestingCube"));
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButton(0))
        {
            Testing.transform.GetComponent<MeshRenderer>().enabled = true;
            RaycastHit hit;
            Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            // Does the ray intersect any objects excluding the player layer
            if (Physics.Raycast(ray, out hit))
            {
                Vector3 rawPosition = hit.point + hit.normal;
                Vector3 roundedPosition = new Vector3(Mathf.RoundToInt(rawPosition.x), Mathf.RoundToInt(rawPosition.y)-1, Mathf.RoundToInt(rawPosition.z));
                Testing.transform.position = roundedPosition;
                //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.yellow, 20.0f);
                //hit.transform.position
                //Debug.Log("Did Hit");
            }
            else
            {
                //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * 1000, Color.white, 20.0f);
                //Debug.Log("Did not Hit");
            }
        }
	}
}
