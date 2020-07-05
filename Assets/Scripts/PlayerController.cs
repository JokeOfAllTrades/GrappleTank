//DONE: start checking code in :)

//TODO: add bounce to fast moving hovertanks going around corners
//QUESTION: find out LevelTankOnMove and SetFlushOnMove should be called in Awake and if they should be skipped in the  
// movement handler if there is no movement

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour {

	// The speed at which the tank moves
	public float tankSpeed;

	// Rate at which Cannon tilts
	public float verticalCannonSpeed;

    // Rate at which Cannon turns left and right
    public float horizontalCannonSpeed;

    public float tractorBeamMaxDistance = 20f;

    Collider tankBaseCollider;
	Rigidbody tankRigidBody;
	Transform tankTurret;
	Transform tankChamber;
    RaycastHit hitInfo;
    List<Vector3> hoverSurface;
    Vector3 center;

    //TODO: remove these objects, they are for testing
    Vector3 mypoint = new Vector3(-1.191f, 5, 0);
    GameObject firstPerson;
    GameObject thirdPerson;
    bool cameraSwitch = true;
    int factor = 10000;
    float distanceOld = 0;

    // Tank related constants
    //TODO: get the next two constants from the physical objects
    const float TANK_RADIUS = 1;
	const float BASE_HEIGHT = -1;
	const float CAST_GAP = .01F;
    // This makes sure that the TankCannon doesn't move through the rest of the tank
	const float VERTICAL_CANNON_ROTATION_CLAMP = 16.368546490040347337177997155454f;
    const float FIND_NORMAL_DISTANCE = 2.5f;

	//ANSWER: hoverSurface should be initialized in Awake() instead of the consructor 
	void Awake()
	{
        //ANSWER: the hover surface should be set in a function
    }

    List<Vector3> GetHoverSurface()
    {
        //DONE: use an angle and a for loop to create list
        List<Vector3> hoverSurface = new List<Vector3>();
        float angle;
        Vector3 vector;

        //TODO: use constant instead of 8 for more granular hover surface
        for (int x = 0; x < 8; x++)
        {
            angle = x * Mathf.PI / 4;
            vector = transform.TransformVector(Mathf.Cos(angle) * TANK_RADIUS, BASE_HEIGHT, Mathf.Sin(angle) * TANK_RADIUS);
            hoverSurface.Add(transform.position + vector);
        }

        hoverSurface.Add(transform.position + transform.TransformVector(0, BASE_HEIGHT, 0));

        return hoverSurface;
    }

    void OnDrawGizmos()
	{
        List<Vector3> hoverSurface = GetHoverSurface();
        Gizmos.color = Color.blue;
        
        //DONE: change to a loop
        for (int x = 0; x < 8; x++)
        {
            Gizmos.DrawLine(hoverSurface[x], x == 7 ? hoverSurface[0] : hoverSurface[x + 1]);
        }

        //DONE: change to a loop
        for (int x = 0; x < 8; x++)
        {
            Gizmos.DrawLine(hoverSurface[x], hoverSurface[8]);
        }
	}

    void Start () 
	{
        tankBaseCollider = GetComponentsInChildren<Collider>()[0];
		tankRigidBody = GetComponent<Rigidbody>();
		tankTurret = transform.GetChild (1);
		tankChamber = tankTurret.GetChild (0);

		firstPerson = GameObject.Find ("FP Camera");
		thirdPerson = GameObject.Find ("3P Camera");


        thirdPerson.SetActive(cameraSwitch);
        //QUESTION: should the tank be rotated before it is set flush?
        SetFlush(GetCenter());
	}

	void Update () 
	{

		CameraSwitch ();
        //QUESTION: should MovementHandler be in fixed update or update?
        //QUESTION: should rotate and leveling be pulled out of the movement handler
        MovementHandler();
        //CannonSwivleHandler ();

    }

    void FixedUpdate()
	{
        
    }

    Vector3 GetCenter()
    {
        return transform.position + transform.TransformVector(0, BASE_HEIGHT, 0);
    }

	// Switches the camera from first person to 3rd person
	void CameraSwitch()
	{
		if(Input.GetKeyDown(KeyCode.Space)) 
        {
			cameraSwitch = !cameraSwitch;
			firstPerson.SetActive (cameraSwitch);
			thirdPerson.SetActive (!cameraSwitch);
		}
	}

	// Takes care of the moving logic
	void MovementHandler()
	{
		// Input on x (straffing)
		float hMovement = Input.GetAxis("Horizontal");

		// Input on z (forward/backward)
		float vMovement = Input.GetAxis("Vertical");

        Vector3 direction = Vector3.Normalize(new Vector3(hMovement, 0, vMovement));

		// Check if we are moving
		if(direction != Vector3.zero)
		{
            //DONE: make diagonal speed one instead of 2^(1/2); normalize ensures that direction has a magnitude of 1
            //QUESTION: should rotation and flush logic be brought into moving logic
            // Move
            transform.position += tankTurret.TransformVector(direction * tankSpeed * Time.deltaTime);
		}
        //QUESTION: are there times when the tank needs to be set fushed before rotation?
        //TODO: Pull out rotation logic
        LevelTankOnMove();
	}

	// Takes care of swivling the cannon
	void CannonSwivleHandler()
	{
		// Input on x (left and right)
		float hPlaneRotation = Input.GetAxis("Mouse X");

		// Input on y (up and down)
		float vPlaneRotation = -Input.GetAxis("Mouse Y");

		// Check if we are Rotating
		if (hPlaneRotation != 0 || vPlaneRotation != 0) {
			tankTurret.RotateAround(tankTurret.position,tankTurret.up,hPlaneRotation * horizontalCannonSpeed * Time.deltaTime);

			Vector3 angles = tankChamber.localRotation.eulerAngles;
			angles.x = angles.x + vPlaneRotation * verticalCannonSpeed * Time.deltaTime;
			if (angles.x <= 180f)
            {
				angles.x = Mathf.Clamp (angles.x, -VERTICAL_CANNON_ROTATION_CLAMP, VERTICAL_CANNON_ROTATION_CLAMP);
			}
			tankChamber.localRotation = Quaternion.Euler (angles);
		}
		//TODO: set hud reticle from beam status
	}

	// This function handles when the player shoots their tractor beam 
	void ShootTractorBeamHandler()
	{
		/* get input on beam axis
		 * if (input)
		 * {
		 *
		 * //check beam status
		 * if miss then just graphic and sound
		 * if hit
		 */
	}
 
	void FindBeamStatus()
	{
		
	}

    //QUESTION: does this function work if the tank landed off-level on a surface?

    // This function makes sure the tank is always oriented perpendicular to the surface it is on 
    void LevelTankOnMove()
	{

        //QUESTION: should function writen as a x rotation plus a z rotation?
        //TODO: create a function called next to wall to handle surfices that bend up called NextToWall();
        //TODO: pull out code into a function called OverHang();
        
        List<Vector3> normals = new List<Vector3>();        

        foreach (Vector3 point in GetHoverSurface())
        {
            FindNormal(point, ref normals);
        }

        List<Vector3> disctinctNormals = normals.Distinct().ToList();
        Vector3 rotation = transform.up;
        Vector3 rotationPoint = GetCenter();
        float distance = 0;
        float rotationAngle;
        Vector3 rotationDirection;

        //TODO: set condition to look for tilt on flat surface; usually a rotation only needs to be done when the tank is over two or more surfaces; see TODO labeled "PARALLEL"
        if (disctinctNormals.Count > 1)
        {
            //TODO: pull this part out into a function
            List<Vector3> projections = new List<Vector3>();
            foreach (Vector3 surfaceNormal in disctinctNormals)
            {

                //ANSWER: projection needs to be normalized and then set to have the same magnitude as a side vector
                //TODO: get projection to work when the surface normal is parallel to transform up; PARALLEL
                Vector3 upProjection = Vector3.Project(surfaceNormal, transform.up);
                Vector3 normalProjection = TANK_RADIUS * Vector3.Normalize(surfaceNormal - upProjection);
                //QUESTION: should projections be an ordered list?
                projections.Add(normalProjection);
                if (projections.Count > 1)
                {
                    int current = projections.Count - 1;
                    //TODO: define comparison in comment or otherwise rename it
                    for (int comparison = 0; comparison < current; comparison++)
                    {
                        //QUESTION: should I compare magnitude and direction instead of using '=='; ANSWER: no, '==' compares the actual vector values
                        //QUESTION: under what circumstances would two normalProjection be equal?; this apparently can happen right when a second surface is, but not always.  This may be a bug related to the incorrectly calculated rotation point

                        if (projections[comparison] == projections[current])
                        {
                            //QUESTION: will disctinctNormals[comparison] always corespond with projections[comparison]?
                            if (upProjection.magnitude > Vector3.Project(disctinctNormals[comparison], transform.up).magnitude)
                            {
                                projections[current] = -projections[current];
                            }
                            else
                            {
                                projections[comparison] = -projections[comparison];

                            }
                        }
                    }
                }
            }

            int associated = 0;
            foreach (Vector3 projection in projections)
            {
                Debug.Log("surfaceNormal: " + disctinctNormals[associated]* factor);                  
                Debug.Log("upProjection: " + Vector3.Project(disctinctNormals[associated], transform.up) * factor);
                Debug.Log("projection: " + projection * factor);
                if (projection.magnitude > 0)
                {
                    distance = DistancePastSeem(projection + center);
                    Debug.Log("distance: " + distance * factor);
                    rotation += disctinctNormals[associated] * distance;
                    //ISSUE: rotation point does not stay static
                    //FIXED: did not combine rotationPoint with +=
                    //TODO: find better fix
                    //TODO: figure out if this is the right solution regardless of facing of tank and rotation over 3-4 surfaces
                    if (distance < 1)
                    {
                        rotationPoint += projection * (TANK_RADIUS - distance);
                    }
                }
                Debug.Log("-----------------");
                associated++;
            }

            //ISSUE: tank doesn't rotate correctly
            //FIXES: moved movement handler into FixedUpdate
            //FIXED: disabled SetFlush
            Quaternion.FromToRotation(transform.up, rotation).ToAngleAxis(out rotationAngle, out rotationDirection);
            transform.RotateAround(rotationPoint, rotationDirection, rotationAngle);
            
            Debug.Log("rotation: " + rotation * factor);
            Debug.Log("Rotation point: " + rotationPoint * factor);
            Debug.Log("Center point before flush with rotation: " + GetCenter() * factor);
            SetFlush(rotationPoint);
        }
        else
        {
            //Quaternion.FromToRotation(transform.up, disctinctNormals[0]).ToAngleAxis(out rotationAngle, out rotationDirection);
            //transform.RotateAround(GetCenter(), rotationDirection, rotationAngle);
            Debug.Log("Center point before flush without rotation: " + GetCenter() * factor);
            SetFlush(GetCenter());
        }
        Debug.Log("Center point after flush: " + GetCenter() * factor);
        Debug.Log("------------------------------------------------------");
    }

    // A ray is cast directly downward for from the input vector + transform.up * CAST_GAP, the normal to any surface it finds is added to normals
    //TODO: Change so that normals are searched for in the negative difference direction for all inputs
    void FindNormal(Vector3 input, ref List<Vector3> normals)
    {
        //TODO: figure out a way not to use a center variable
        center = GetCenter();

        RaycastHit output = new RaycastHit();

        //QUESTION: should these raycasts look for the ground that the player is on via some layer mechanism?
        if (Physics.Raycast(input + transform.up * CAST_GAP, -transform.up, out output, FIND_NORMAL_DISTANCE))
        {
            //ANSWER: normals are already normalized
            normals.Add(output.normal);
        }
        else
        {
            // For cases when the player is partly over a cliff
            //TODO: fix surface gap issue
            //TODO: figure out a way to not use a the center variable
            Vector3 centerGap = center - transform.TransformVector(0, CAST_GAP * 2, 0);
            Vector3 inputGap = input - transform.TransformVector(0, CAST_GAP * 2, 0);
            Vector3 difference = centerGap - inputGap;

            if (Physics.Raycast(inputGap, difference, out output, FIND_NORMAL_DISTANCE))
            {
                //ANSWER: normals do not need to be normalized
                normals.Add(output.normal);
            }
        }
    }

    float DistancePastSeem(Vector3 directionOfOverHang)
    {
        center = GetCenter();

        RaycastHit output = new RaycastHit();

        if (Physics.Raycast(directionOfOverHang, -transform.up, out output, FIND_NORMAL_DISTANCE))
        {
            //ANSWER: we need to convert the output of Vector3.Angle to radians
            return (output.distance - CAST_GAP) / Mathf.Tan(Vector3.Angle(output.normal,transform.up) * Mathf.Deg2Rad);
        }
        else
        {
            //TODO: fix surface gap issue
            Vector3 centerGap = center - transform.TransformVector(0, CAST_GAP * 2, 0);
            Vector3 inputGap = directionOfOverHang - transform.TransformVector(0, CAST_GAP * 2, 0);
            Vector3 difference = centerGap - inputGap;

            //QUESTION: should this be in if statement?
            Physics.Raycast(inputGap, difference, out output, FIND_NORMAL_DISTANCE);


            return output.distance - CAST_GAP * 2 * Mathf.Tan(Vector3.Angle(output.normal, difference) * Mathf.Deg2Rad);
        }
    }

    //This function makes sure that the tank is always the correct distance above the surface
    //ANSWER: The ray is cast from the CAST_GAP instead of the surface
    void SetFlush(Vector3 rotationPoint)
    {
        RaycastHit closest;
        float distanceChange;
        if (Physics.Raycast(rotationPoint + transform.up * CAST_GAP, -transform.up, out closest))
        {
            
            //TODO: create transform for when tank is too close to surface
            
            Debug.Log("closest.distance * 100000: " + closest.distance * 100000);
            distanceChange = closest.distance * 100000 - distanceOld;
            distanceOld = closest.distance * 100000;
            distanceChange = Mathf.Abs(distanceChange);
            Debug.Log("distanceChange: " + distanceChange);
            if (distanceChange > 100)
            {
                //Debug.Break();
            }
            if (closest.distance > CAST_GAP)
            {

                transform.position -= transform.up * (closest.distance - CAST_GAP);
            }
        }   
        
    }

}