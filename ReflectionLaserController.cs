using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReflectionLaserController : MonoBehaviour
{
    [SerializeField] //Attribute to make private variables show in editor
    private int maxReflectionCount = 5; //Changed to private. Never use public unless a very specific use case
    [SerializeField]
    private float maxDistance = 200;
    [SerializeField]
    private LineRenderer lineRenderer;

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(transform.position, transform.position + transform.forward);
        Gizmos.DrawWireSphere(transform.position, 0.25f);

        List<Vector3> reflectionHitPositions = ReflectionHitPositions(transform.position + transform.forward * 0.75f, transform.forward, maxReflectionCount); //Get a list of reflection hit points and store them locally

        Gizmos.color = Color.blue;

        Vector3 lastPos = transform.position;
        foreach(Vector3 hitPos in reflectionHitPositions) //Foreach reflection hit point returned from the ReflectionHitPositions function, draw a line from the last point to the current point
        {
            Gizmos.DrawLine(lastPos, hitPos);
            lastPos = hitPos;
        }
    }

    //Input - Starting position and direction of the laser reflection that you want to calculate and how many reflections you want to calculate
    //Output - A list of Vector3 coordinates representing the reflection points
    private List<Vector3> ReflectionHitPositions(Vector3 position, Vector3 direction, int reflectionsRemaining)
    {

        List<Vector3> rayHitPoints = new List<Vector3>(); //Create a new empty local list 

        while (rayHitPoints.Count < reflectionsRemaining) //Checking if the amount of reflection points already in the list is less than the amount of reflections remaining
        {
            Ray ray = new Ray(position, direction);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, maxDistance))
            {
                direction = Vector3.Reflect(direction, hit.normal);
                position = hit.point;
            }

            if (hit.transform.tag == "Reflective") rayHitPoints.Add(hit.point); //If the ray hit a reflective surface, get the hit point in world space and add that coordiante to the total list of hit points

            else if (hit.transform.tag == "Detector") EventsManager.instance.StartLaserDetector(hit.transform);

            else //If nothing reflective is hit
            {
                rayHitPoints.Add(hit.point); //Adds the last calculated point to the list
                reflectionsRemaining = 0; //Don't reflect any more
            }
        }
        return rayHitPoints; //Return the list of Vector3 points
    }

    private void DrawLaser() 
    {
        List<Vector3> hitPointsList = ReflectionHitPositions(transform.position, transform.forward, maxReflectionCount);
            hitPointsList.Insert(0, transform.position);

        lineRenderer.SetPositions(hitPointsList.ToArray());

    }

    private void Update()
    {
        DrawLaser();
    }

    private void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }
}
