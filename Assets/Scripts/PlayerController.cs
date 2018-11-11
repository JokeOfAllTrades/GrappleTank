//TODO: start checking code in :)

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

    //for testing
    float movementTotal = 0;

    const float SIDE_VECTOR_MAGNITUDE = 1;
	readonly float DIAGONAL_VECTOR_MAGNITUDE_PROJECTION = Mathf.Cos (45 * Mathf.Deg2Rad);
	const float DOWN_VECTOR_MAGNITUDE = -1;
	const float SURFACE_GAP = .01F;
	const float VERTICAL_CANNON_ROTATION_CLAMP = 16.368546490040347337177997155454f;
    const float FIND_NORMAL_DISTANCE = 2.5f;

    Vector3 mypoint = new Vector3(-1.191f, 5, 0);

    //TODO: remove these objects
    GameObject firstPerson;
	GameObject thirdPerson;
	bool cameraSwitch = true;

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

        for (int x = 0; x < 8; x++)
        {
            angle = x * Mathf.PI / 4;
            vector = transform.TransformVector(Mathf.Cos(angle), DOWN_VECTOR_MAGNITUDE, Mathf.Sin(angle));
            hoverSurface.Add(transform.position + vector);
        }

        hoverSurface.Add(transform.position + transform.TransformVector(0, DOWN_VECTOR_MAGNITUDE, 0));

        return hoverSurface;
    }

    void OnDrawGizmos()
	{
        List<Vector3> hoverSurface = new List<Vector3>();
        float angle;
        Vector3 vector;

        for (int x = 0; x < 8; x++)
        {
            angle = x * Mathf.PI / 4;
            vector = transform.TransformVector(Mathf.Cos(angle), DOWN_VECTOR_MAGNITUDE, Mathf.Sin(angle));
            hoverSurface.Add(transform.position + vector);
        }

        

        //TODO: change to a loop
        Vector3 backLeft = hoverSurface[0];
		Vector3 backBack = hoverSurface[1];
        Vector3 backRight = hoverSurface[2];
        Vector3 rightRight = hoverSurface[3];
        Vector3 frontRight = hoverSurface[4];
        Vector3 frontFront = hoverSurface[5];
        Vector3 frontLeft = hoverSurface[6];
        Vector3 leftLeft = hoverSurface[7];
        Vector3 center = hoverSurface[8];

        Gizmos.color = Color.blue;
        
        //TODO: change to a loop
        Gizmos.DrawLine (backLeft, backBack);
		Gizmos.DrawLine (backBack, backRight);
		Gizmos.DrawLine (backRight, rightRight);
		Gizmos.DrawLine (rightRight, frontRight);
		Gizmos.DrawLine (frontRight, frontFront);
		Gizmos.DrawLine (frontFront, frontLeft);
		Gizmos.DrawLine (frontLeft, leftLeft);
		Gizmos.DrawLine (leftLeft, backLeft);

        hoverSurface.Add(transform.position + transform.TransformVector(0, DOWN_VECTOR_MAGNITUDE, 0));
        //TODO: change to a loop
        Gizmos.DrawLine (backLeft, center);
		Gizmos.DrawLine (backBack, center);
		Gizmos.DrawLine (backRight, center);
		Gizmos.DrawLine (rightRight, center);
		Gizmos.DrawLine (frontRight, center);
		Gizmos.DrawLine (frontFront, center);
		Gizmos.DrawLine (frontLeft, center);
		Gizmos.DrawLine (leftLeft, center);
        
	}
		
	void Start () 
	{
        tankBaseCollider = GetComponentsInChildren<Collider>()[0];
		tankRigidBody = GetComponent<Rigidbody>();
		tankTurret = transform.GetChild (1);
		tankChamber = tankTurret.GetChild (0);

		firstPerson = GameObject.Find ("FP Camera");
		thirdPerson = GameObject.Find ("3P Camera");
		
		thirdPerson.SetActive (cameraSwitch);
        SetFlushOnRotate();
	}

	void Update () 
	{
        //QUESTION: should MovementHandler be in fixed update or update?
		CameraSwitch ();
        MovementHandler();
        //CannonSwivleHandler ();

    }

    void FixedUpdate()
	{
        
    }

	// Switches the camera from first person to 3rd person
	void CameraSwitch()
	{
		if(Input.GetKeyDown(KeyCode.Space)) {
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
            //TODO: make diagonal speed one instead of 2^(1/2)
            //QUESTION: should rotation and flush logic be brought into moving logic
            // Move
            transform.position += tankTurret.TransformVector(direction * tankSpeed * Time.deltaTime);
		}
        //QUESTION: are there times when the tank needs to be set fushed before rotation?

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

    // This function makes sure the tank is always oriented perpendicular to the surface it is on 
    void LevelTankOnMoveOld()
    {
        float rotationAngle;
        Vector3 rotationDirection;

        center = transform.position + transform.TransformVector(0, DOWN_VECTOR_MAGNITUDE, 0);
        if (Physics.Raycast(GetHoverSurface()[0], -transform.up, out hitInfo))
        {
            Quaternion.FromToRotation(transform.up, hitInfo.normal).ToAngleAxis(out rotationAngle, out rotationDirection);
            transform.RotateAround(GetHoverSurface()[0], rotationDirection, rotationAngle);
        }
    }
    

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
        Vector3 rotationPoint = transform.position + transform.TransformVector(0, DOWN_VECTOR_MAGNITUDE, 0);
        float distanceOverEdge = 0;

        foreach (Vector3 surfaceNormal in disctinctNormals)
        {
            //ANSWER: projection needs to be normalized and then set to have the same magnitude as a side vector
            //TODO: get projection to work when the surface normal is parallel to transform up.
            Vector3 projection = SIDE_VECTOR_MAGNITUDE * Vector3.Normalize(surfaceNormal - Vector3.Project(surfaceNormal, transform.up));
            
            distanceOverEdge = FindDistanceOverEdge(projection + center);

            rotation += surfaceNormal * distanceOverEdge;
            //ISSUE: rotation point does not stay static
            //FIXED: did not combine rotationPoint with +=
            //TODO: figure out if this is the right solution regardless of facing of tank and rotation over 3-4 surfaces
            rotationPoint = projection * (SIDE_VECTOR_MAGNITUDE - distanceOverEdge);
           
        }

        rotationPoint += transform.position + transform.TransformVector(0, DOWN_VECTOR_MAGNITUDE, 0);

        float rotationAngle;
        Vector3 rotationDirection;
        
        //ISSUE: tank doesn't rotate correctly
        //FIXES: moved movement handler into FixedUpdate
        //FIXED: disabled SetFlushOnMove
        Quaternion.FromToRotation(transform.up, rotation).ToAngleAxis(out rotationAngle, out rotationDirection);
        transform.RotateAround(rotationPoint, rotationDirection, rotationAngle);
        //SetFlushOnRotate();
    }

    void FindNormal(Vector3 input, ref List<Vector3> normals)
    {
        //TODO: figure out a way not to use a center variable
        center = transform.position + transform.TransformVector(0, DOWN_VECTOR_MAGNITUDE, 0);

        RaycastHit output = new RaycastHit();

        //QUESTION: should these raycasts look for the ground that the player is on via some layer mechanism?
        if (Physics.Raycast(input, -transform.up, out output, FIND_NORMAL_DISTANCE))
        {
            if (Vector3.Angle(output.normal,transform.up) > .01)
            {
                //ANSWER: normals are already normalized
                normals.Add(output.normal);
            }
        }
        else
        {
            //TODO: figure out a way to not use a the center variable
            Vector3 centerGap = center - transform.TransformVector(0, SURFACE_GAP * 2, 0);
            Vector3 inputGap = input - transform.TransformVector(0, SURFACE_GAP * 2, 0);
            Vector3 difference = centerGap - inputGap;

            if (Physics.Raycast(inputGap, difference, out output, FIND_NORMAL_DISTANCE))
            {
                //ANSWER: normals do not need to be normalized
                normals.Add(output.normal);
            }
        }
    }

    float FindDistanceOverEdge(Vector3 directionOfOverHang)
    {
        center = transform.position + transform.TransformVector(0, DOWN_VECTOR_MAGNITUDE, 0);

        RaycastHit output = new RaycastHit();

        if (Physics.Raycast(directionOfOverHang, -transform.up, out output, FIND_NORMAL_DISTANCE))
        {
            //Debug.DrawRay(output.point, directionOfOverHang - center, Color.black);
            //ANSWER: we need to convert the output of Vector3.Angle to radians
            return output.distance / Mathf.Tan(Vector3.Angle(output.normal,transform.up) * Mathf.Deg2Rad);
        }
        else
        {
            Vector3 centerGap = center - transform.TransformVector(0, SURFACE_GAP * 2, 0);
            Vector3 inputGap = directionOfOverHang - transform.TransformVector(0, SURFACE_GAP * 2, 0);
            Vector3 difference = centerGap - inputGap;

            //QUESTION: should this be in if statement?
            Physics.Raycast(inputGap, difference, out output, FIND_NORMAL_DISTANCE);
            //Debug.DrawRay(inputGap, difference, Color.black);

            return output.distance - SURFACE_GAP * 2 * Mathf.Tan(Vector3.Angle(output.normal, difference) * Mathf.Deg2Rad);
        }
    }
    
    //This function makes sure that the tank is always the correct distance above the surface
    //TODO: rewrite for when any of the cardinal directions are not over the nearest point of a surface
    void SetFlushOnRotate()
	{
		float shortest = FindShortest ();
		if (shortest > SURFACE_GAP) {
			transform.position -= transform.up * (shortest - SURFACE_GAP);
		}
	}

    //This function finds which ever vector is closest to the ground
    //TODO: rewrite using arrays
    float FindShortest()
	{
		RaycastHit hitBackLeft;
		RaycastHit hitBackBack;
		RaycastHit hitBackRight;
		RaycastHit hitRightRight;
		RaycastHit hitFrontRight;
		RaycastHit hitFrontFront;
		RaycastHit hitFrontLeft;
		RaycastHit hitLeftLeft;
		RaycastHit hitCenter;

		Vector3 backLeft = transform.position + transform.TransformVector(-DIAGONAL_VECTOR_MAGNITUDE_PROJECTION, DOWN_VECTOR_MAGNITUDE, -DIAGONAL_VECTOR_MAGNITUDE_PROJECTION);
		Vector3 backBack = transform.position + transform.TransformVector(0, DOWN_VECTOR_MAGNITUDE, -SIDE_VECTOR_MAGNITUDE);
		Vector3 backRight = transform.position + transform.TransformVector(DIAGONAL_VECTOR_MAGNITUDE_PROJECTION, DOWN_VECTOR_MAGNITUDE, -DIAGONAL_VECTOR_MAGNITUDE_PROJECTION);
		Vector3 rightRight = transform.position + transform.TransformVector(SIDE_VECTOR_MAGNITUDE, DOWN_VECTOR_MAGNITUDE, 0);
		Vector3 frontRight = transform.position + transform.TransformVector(DIAGONAL_VECTOR_MAGNITUDE_PROJECTION, DOWN_VECTOR_MAGNITUDE, DIAGONAL_VECTOR_MAGNITUDE_PROJECTION);
		Vector3 frontFront = transform.position + transform.TransformVector(0, DOWN_VECTOR_MAGNITUDE, SIDE_VECTOR_MAGNITUDE);
		Vector3 frontLeft = transform.position + transform.TransformVector(-DIAGONAL_VECTOR_MAGNITUDE_PROJECTION, DOWN_VECTOR_MAGNITUDE, DIAGONAL_VECTOR_MAGNITUDE_PROJECTION);
		Vector3 leftLeft = transform.position + transform.TransformVector(-SIDE_VECTOR_MAGNITUDE, DOWN_VECTOR_MAGNITUDE, 0);
		Vector3 center = transform.position + transform.TransformVector(0, DOWN_VECTOR_MAGNITUDE, 0);

		List<float> distances = new List<float>();

		if(Physics.Raycast(backLeft, -transform.up, out hitBackLeft))
		{
			distances.Add (hitBackLeft.distance);
		}
		if(Physics.Raycast(backBack, -transform.up, out hitBackBack))
		{
			distances.Add (hitBackBack.distance);
		}
		if(Physics.Raycast(backRight, -transform.up, out hitBackRight))
		{
			distances.Add (hitBackRight.distance);
		}
		if(Physics.Raycast(rightRight, -transform.up, out hitRightRight))
		{
			distances.Add (hitRightRight.distance);
		}
		if(Physics.Raycast(frontRight, -transform.up, out hitFrontRight))
		{
			distances.Add (hitFrontRight.distance);
		}
		if(Physics.Raycast(frontFront, -transform.up, out hitFrontFront))
		{
			distances.Add (hitFrontFront.distance);
		}
		if(Physics.Raycast(frontLeft, -transform.up, out hitFrontLeft))
		{
			distances.Add (hitFrontLeft.distance);
		}
		if(Physics.Raycast(leftLeft, -transform.up, out hitLeftLeft))
		{
			distances.Add (hitLeftLeft.distance);
		}
		if(Physics.Raycast(center, -transform.up, out hitCenter))
		{
			distances.Add (hitCenter.distance);
		}

		if (distances.Count > 0) {
			distances.Sort ();
			return distances [0];
		} else
			return SURFACE_GAP;
	}
    
}