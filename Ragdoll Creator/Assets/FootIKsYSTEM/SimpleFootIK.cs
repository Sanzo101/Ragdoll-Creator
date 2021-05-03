using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FootIK
{
	public class SimpleFootIK : MonoBehaviour
	{
		// Declare properties
		[HideInInspector] public Vector3 leftFootPosition;
		[HideInInspector] public Vector3 rightFootPosition;
		public Transform ragdoll;
		public GameObject Armature,Slave;

		Animator animator;
		public LayerMask layerMask;
		public string[] ignoreLayers = { "Ragdoll","UI","Water","Ignore Raycast", "Slope", "TransparentFX " };
		public float deltaTime;

		public RaycastHit raycastHitLeftFoot;
		public RaycastHit raycastHitRightFoot;
		public RaycastHit raycastHitToe;
		[Range(4f, 20f)] public float raycastLength = 20f; // Character must not be higher above ground than this.
		[Range(.2f, .9f)] public float maxStepHeight = .5f;

		[Range(0f, 1f)] public float footIKWeight = 1f;

		[Range(1f, 100f)] public float footNormalLerp = 40f; // Lerp smoothing of foot normals
		[Range(1f, 100f)] public float footTargetLerp = 40f; // Lerp smoothing of foot position
		[Range(1f, 100f)] public float transformYLerp = 20f; // Lerp smoothing of transform following terrain
		[HideInInspector] public float extraYLerp = 1f; // Used by ragdollControl

		[Range(0f, 1f)] public float maxIncline = .8f; // Foot IK not aktiv on inclines steeper than arccos(maxIncline);

		public bool followTerrain = true;
		[HideInInspector] public bool userNeedsToFixStuff = false;
		public float footHeight; // Set manually in inspector


		public Transform leftToe; // Set manually in inspector
		public Transform leftFoot;
		public Transform leftCalf;
		public Transform leftThigh;
		public Transform rightToe;
		public Transform rightFoot;
		public Transform rightCalf;
		public Transform rightThigh;


		public Quaternion leftFootRotation;
		public Quaternion rightFootRotation;

		public Vector3 leftFootTargetPos;
		public Vector3 leftFootTargetNormal;
		public Vector3 lastLeftFootTargetPos;
		public Vector3 lastLeftFootTargetNormal;
		public Vector3 rightFootTargetPos;
		public Vector3 rightFootTargetNormal;
		public Vector3 lastRightFootTargetPos;
		public Vector3 lastRightFootTargetNormal;

		public Vector3 footForward;

		public float leftLegTargetLength;
		public float rightLegTargetLength;
		public float thighLength;
		public float thighLengthSquared;
		public float calfLength;
		public float calfLengthSquared;
		public float reciDenominator;

		public float leftKneeAngle;
		public float leftThighAngle;
		public float rightKneeAngle;
		public float rightThighAngle;

	

	void Awake()
		{
			Awake2();
			
		}

		void FixedUpdate()
		{
			deltaTime = Time.fixedDeltaTime;
			DoSimpleFootIK();
		}

		void DoSimpleFootIK()
		{
			ShootIKRays();
			PositionFeet();
			Slave.GetComponent<ConfigurableJoints>().FollowAnimation();
			
		}

		public void ShootIKRays()
		{
			layerMask = LayerMask.GetMask("Ground");
			leftFootPosition = new Vector3(leftFoot.position.x, leftFoot.position.y, leftFoot.position.z);
			rightFootPosition = new Vector3(rightFoot.position.x, rightFoot.position.y, rightFoot.position.z);

			// Shoot ray to determine where the feet should be placed.
						Debug.DrawRay(rightFootPosition + Vector3.up * maxStepHeight, Vector3.down * raycastLength, Color.green);
			if (!Physics.Raycast(rightFootPosition + Vector3.up * maxStepHeight, Vector3.down, out raycastHitRightFoot, raycastLength, layerMask))
			{
				raycastHitRightFoot.normal = Vector3.up;
				raycastHitRightFoot.point = rightFoot.position - raycastLength * Vector3.up;
			}
			footForward = rightFoot.position - rightCalf.position;
			footForward = new Vector3(footForward.x, 0f, footForward.z);
			footForward = Quaternion.FromToRotation(Vector3.up, raycastHitRightFoot.normal) * footForward;
			if (!Physics.Raycast(rightFootPosition + footForward + Vector3.up * maxStepHeight, Vector3.down, out raycastHitToe, maxStepHeight * 2f, layerMask))
			{
				raycastHitToe.normal = raycastHitRightFoot.normal;
				raycastHitToe.point = raycastHitRightFoot.point + footForward;
			}
			else
			{
				if (raycastHitRightFoot.point.y < raycastHitToe.point.y - footForward.y)
					raycastHitRightFoot.point = new Vector3(raycastHitRightFoot.point.x, raycastHitToe.point.y , raycastHitRightFoot.point.z);

				// Put avgNormal in foot normal
				raycastHitRightFoot.normal = (raycastHitRightFoot.normal + raycastHitToe.normal).normalized;
			}

						Debug.DrawRay(leftFootPosition + Vector3.up * maxStepHeight, Vector3.down * raycastLength , Color.red);
			if (!Physics.Raycast(leftFootPosition + Vector3.up * maxStepHeight, Vector3.down, out raycastHitLeftFoot, raycastLength, layerMask))
			{
				raycastHitLeftFoot.normal = Vector3.up;
				raycastHitLeftFoot.point = leftFoot.position - raycastLength * Vector3.up;
			}
			footForward = leftFoot.position - leftCalf.position;
			footForward = new Vector3(footForward.x, 0f, footForward.z);
			footForward = Quaternion.FromToRotation(Vector3.up, raycastHitLeftFoot.normal) * footForward;
			if (!Physics.Raycast(leftFootPosition + footForward + Vector3.up * maxStepHeight, Vector3.down, out raycastHitToe, maxStepHeight * 2f, layerMask))
			{
				raycastHitToe.normal = raycastHitLeftFoot.normal;
				raycastHitToe.point = raycastHitLeftFoot.point + footForward;
			}
			else
			{
				if (raycastHitLeftFoot.point.y < raycastHitToe.point.y - footForward.y)
					raycastHitLeftFoot.point = new Vector3(raycastHitLeftFoot.point.x, raycastHitToe.point.y - footForward.y, raycastHitLeftFoot.point.z);

				// Put avgNormal in foot normal
				raycastHitLeftFoot.normal = (raycastHitLeftFoot.normal + raycastHitToe.normal).normalized;
			}

			// Do not tilt feet if on to steep an angle
			if (raycastHitLeftFoot.normal.y < maxIncline)
			{
				raycastHitLeftFoot.normal = Vector3.RotateTowards(Vector3.up, raycastHitLeftFoot.normal, Mathf.Acos(maxIncline), 0f);
			}
			if (raycastHitRightFoot.normal.y < maxIncline)
			{
				raycastHitRightFoot.normal = Vector3.RotateTowards(Vector3.up, raycastHitRightFoot.normal, Mathf.Acos(maxIncline), 0f);
			}

			if (followTerrain)
			{
				Armature.transform.position = new Vector3(Armature.transform.position.x, Mathf.Lerp(Armature.transform.position.y, Mathf.Min(raycastHitLeftFoot.point.y, raycastHitRightFoot.point.y), transformYLerp * extraYLerp * deltaTime), Armature.transform.position.z);
								Debug.DrawLine(raycastHitLeftFoot.point, raycastHitRightFoot.point);
			}
		}

		public void Awake2()
		{
			foreach (string ignoreLayer in ignoreLayers)
			{
				layerMask = layerMask | (1 << LayerMask.NameToLayer(ignoreLayer)); // Use to avoid IK raycasts to hit colliders on the character (ragdoll must be on an ignored layer)
			}
			layerMask = ~layerMask;

			if (!ragdoll)
			{
				Debug.LogWarning("ragdoll not assigned in SimpleFootIK script on " + this.name + "\nThis Foot IK is for use with an AnimFollow system" + "\n");
				userNeedsToFixStuff = true;
			}
			else
			{
				

				bool ragdollOnIgnoredLayer = false;
				foreach (string ignoreLayer in ignoreLayers)
				{
					if (ragdoll.gameObject.layer.Equals(LayerMask.NameToLayer(ignoreLayer)))
					{
						ragdollOnIgnoredLayer = true;
						break;
					}
				}

				if (!ragdollOnIgnoredLayer)
				{
					Debug.LogWarning("Layer for " + ragdoll.name + " and its children must be set to an ignored layer" + "\n");
					userNeedsToFixStuff = true;
				}
			}


			
			Transform[] characterTransforms = Armature.GetComponentsInChildren<Transform>();
			for (int n = 0; n < characterTransforms.Length; n++)
			{
				if ((characterTransforms[n].name.ToLower().Contains("Foot.L")))
				{
					leftToe = characterTransforms[n + 1];
					leftFoot = characterTransforms[n];
					leftCalf = characterTransforms[n - 1];
					leftThigh = characterTransforms[n - 2];
					if (rightFoot)
						break;
				}
				if (characterTransforms[n].name.ToLower().Contains("Foot.R") )
				{
					rightToe = characterTransforms[n + 1];
					rightFoot = characterTransforms[n];
					rightCalf = characterTransforms[n - 1];
					rightThigh = characterTransforms[n - 2];
					if (leftFoot)
						break;
				}
			}
			if (!(leftToe && rightToe))
			{
				Debug.LogWarning("Auto assigning of legs failed. Look at lines 32-57 in script IK_Setup" + "\n");
				userNeedsToFixStuff = true;
				return;
			}


			thighLength = (rightThigh.position - rightCalf.position).magnitude;
			thighLengthSquared = (rightThigh.position - rightCalf.position).sqrMagnitude;
			calfLength = (rightCalf.position - rightFoot.position).magnitude;
			calfLengthSquared = (rightCalf.position - rightFoot.position).sqrMagnitude;
			reciDenominator = -.5f / calfLength / thighLength;


			// Character should be spawned upright (line from feets to head points as vector3.up)
			footHeight = (rightFoot.position.y + leftFoot.position.y) * .5f - Armature.transform.position.y;

			if (footHeight == 0f)
				footHeight = .132f;

		}


		public void PositionFeet()
		{
			float leftLegTargetLength;
			float rightLegTargetLength;
			float leftKneeAngle;
			float rightKneeAngle;

			// Save before PositionFeet
			//Quaternion leftFootRotation = leftFoot.rotation;
			//Quaternion rightFootRotation = rightFoot.rotation;

			float leftFootElevationInAnim = Vector3.Dot(leftFoot.position - transform.position, transform.up) - footHeight;
			float rightFootElevationInAnim = Vector3.Dot(rightFoot.position - transform.position, transform.up) - footHeight;

			// maths			
			leftFootTargetNormal = Vector3.Lerp(Vector3.up, raycastHitLeftFoot.normal, footIKWeight);
			leftFootTargetNormal = Vector3.Lerp(lastLeftFootTargetNormal, leftFootTargetNormal, footNormalLerp * deltaTime);
			lastLeftFootTargetNormal = leftFootTargetNormal;
			rightFootTargetNormal = Vector3.Lerp(Vector3.up, raycastHitRightFoot.normal, footIKWeight);
			rightFootTargetNormal = Vector3.Lerp(lastRightFootTargetNormal, rightFootTargetNormal, footNormalLerp * deltaTime);
			lastRightFootTargetNormal = rightFootTargetNormal;

			leftFootTargetPos = raycastHitLeftFoot.point;
			leftFootTargetPos = Vector3.Lerp(lastLeftFootTargetPos, leftFootTargetPos, footTargetLerp * deltaTime);
			lastLeftFootTargetPos = leftFootTargetPos;
			leftFootTargetPos = Vector3.Lerp(leftFoot.position, leftFootTargetPos + leftFootTargetNormal * footHeight + leftFootElevationInAnim * Vector3.up, footIKWeight);

			rightFootTargetPos = raycastHitRightFoot.point;
			rightFootTargetPos = Vector3.Lerp(lastRightFootTargetPos, rightFootTargetPos, footTargetLerp * deltaTime);
			lastRightFootTargetPos = rightFootTargetPos;
			rightFootTargetPos = Vector3.Lerp(rightFoot.position, rightFootTargetPos + rightFootTargetNormal * footHeight + rightFootElevationInAnim * Vector3.up, footIKWeight);


			leftLegTargetLength = Mathf.Min((leftFootTargetPos - leftThigh.position).magnitude, calfLength + thighLength - .01f);
			leftLegTargetLength = Mathf.Max(leftLegTargetLength, .2f);
			leftKneeAngle = Mathf.Acos((Mathf.Pow(leftLegTargetLength, 2f) - calfLengthSquared - thighLengthSquared) * reciDenominator);
			leftKneeAngle *= Mathf.Rad2Deg;
			float currKneeAngle;
			Vector3 currKneeAxis;
			Quaternion currKneeRotation = Quaternion.FromToRotation(leftCalf.position - leftThigh.position, leftFoot.position - leftCalf.position);
			currKneeRotation.ToAngleAxis(out currKneeAngle, out currKneeAxis);
			if (currKneeAngle > 180f)
			{
				currKneeAngle = 360f - currKneeAngle;
				currKneeAxis *= -1f;
			}
			leftCalf.Rotate(currKneeAxis, 180f - leftKneeAngle - currKneeAngle, Space.World);
			leftThigh.rotation = Quaternion.FromToRotation(leftFoot.position - leftThigh.position, leftFootTargetPos - leftThigh.position) * leftThigh.rotation;

			rightLegTargetLength = Mathf.Min((rightFootTargetPos - rightThigh.position).magnitude, calfLength + thighLength - .01f);
			rightLegTargetLength = Mathf.Max(rightLegTargetLength, .2f);
			rightKneeAngle = Mathf.Acos((Mathf.Pow(rightLegTargetLength, 2f) - calfLengthSquared - thighLengthSquared) * reciDenominator);
			rightKneeAngle *= Mathf.Rad2Deg;
			currKneeRotation = Quaternion.FromToRotation(rightCalf.position - rightThigh.position, rightFoot.position - rightCalf.position);
			currKneeRotation.ToAngleAxis(out currKneeAngle, out currKneeAxis);
			if (currKneeAngle > 180f)
			{
				currKneeAngle = 360f - currKneeAngle;
				currKneeAxis *= -1f;
			}
			rightCalf.Rotate(currKneeAxis, 180f - rightKneeAngle - currKneeAngle, Space.World);
			rightThigh.rotation = Quaternion.FromToRotation(rightFoot.position - rightThigh.position, rightFootTargetPos - rightThigh.position) * rightThigh.rotation;

			leftFootPosition = leftFoot.position - leftFootTargetNormal * footHeight;
			rightFootPosition = rightFoot.position - rightFootTargetNormal * footHeight;

			//leftFoot.rotation = Quaternion.FromToRotation(Armature.transform.up, leftFootTargetNormal) * leftFootRotation;
			//rightFoot.rotation = Quaternion.FromToRotation(Armature.transform.up, rightFootTargetNormal) * rightFootRotation;
		}
	}
}