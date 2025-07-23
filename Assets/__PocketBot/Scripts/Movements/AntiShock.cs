using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using HyrphusQ.Events;
using LatteGames;
using UnityEngine;

public class AntiShock : MonoBehaviour
{
//     const float MAX_DISTANCE_PER_FRAME = 30f;
//     const float AXIS_PART_MAX_DISTANCE = 0.2f;
//     const float ANTI_PART_STUCK_DURATION = 0.05f;

//     Vector3 previousPos;
//     Quaternion previousRot;
//     Vector3 previousCamPos;
//     bool isStarted;
//     List<PBChassis.PartContainer> partContainers;
//     PBChassis chassis;

//     Rigidbody rigidbody => chassis.CarPhysics.CarRb;
//     Dictionary<Rigidbody, Coroutine> partCoroutines = new Dictionary<Rigidbody, Coroutine>();
//     Dictionary<Rigidbody, Vector3> partPositions = new Dictionary<Rigidbody, Vector3>();
//     CinemachineBrain mainCamBrain;

//     private void Awake()
//     {
//         var levelController = ObjectFindCache<PBLevelController>.Get();
//         if (levelController)
//         {
//             StartCoroutine(CommonCoroutine.WaitUntil(() => levelController.IsLevelStarted, () =>
//             {
//                 HandleLevelStarted();
//             }));
//         }
//     }

//     void HandleLevelStarted()
//     {
//         chassis = transform.GetComponentInParent<PBChassis>();
//         mainCamBrain = MainCameraFindCache.Get().GetComponent<CinemachineBrain>();
//         previousPos = rigidbody.position;
//         partContainers = chassis.PartContainers.FindAll(x => x.Containers[0].GetComponentInChildren<Rigidbody>() != null);
//         foreach (var partContainer in partContainers)
//         {
//             var partRBs = partContainer.Containers[0].GetComponentsInChildren<Rigidbody>();
//             foreach (var partRB in partRBs)
//             {
//                 partPositions.Add(partRB, chassis.CarPhysics.transform.InverseTransformPoint(partRB.transform.position));
//                 partCoroutines.Add(partRB, null);
//             }
//         }
//         isStarted = true;
//     }

// #if UNITY_EDITOR
//     // Update is called once per frame
//     void Update()
//     {
//         if (Input.GetKeyDown(KeyCode.B))
//         {
//             rigidbody.AddForce(Vector3.down * 1000000000, ForceMode.VelocityChange);
//             // UnityEditor.EditorApplication.isPaused = true;
//         }
//     }
// #endif

//     private void FixedUpdate()
//     {
//         if (isStarted)
//         {
//             if (chassis.Robot.IsDead)
//             {
//                 this.enabled = false;
//             }
//             if (!gameObject.activeInHierarchy)
//             {
//                 return;
//             }
//             if ((rigidbody.position - previousPos).magnitude > MAX_DISTANCE_PER_FRAME)
//             {
//                 rigidbody.transform.position = previousPos;
//                 rigidbody.transform.rotation = previousRot;
//                 rigidbody.velocity = Vector3.ClampMagnitude(rigidbody.velocity, 1);
//                 rigidbody.angularVelocity = Vector3.ClampMagnitude(rigidbody.angularVelocity, 1);
//                 var virtualCam = (CinemachineVirtualCamera)mainCamBrain.ActiveVirtualCamera;
//                 virtualCam.ForceCameraPosition(previousCamPos, virtualCam.transform.rotation);

//                 foreach (var partPosition in partPositions)
//                 {
//                     var partRB = partPosition.Key;
//                     partCoroutines[partRB] = StartCoroutine(CR_AntiPartStuck(partPosition));
//                 }
//             }
//             else
//             {
//                 previousCamPos = mainCamBrain.transform.position;
//                 previousPos = rigidbody.position + Vector3.up;
//                 previousRot = rigidbody.rotation;
//             }
//         }
//     }

//     IEnumerator CR_AntiPartStuck(KeyValuePair<Rigidbody, Vector3> keyValuePair)
//     {
//         var position = keyValuePair.Value;
//         var partRB = keyValuePair.Key;
//         var t = 0f;
//         var colliders = partRB.GetComponentsInChildren<Collider>().ToList().FindAll(x => x.enabled);
//         foreach (var collider in colliders)
//         {
//             collider.enabled = false;
//         }
//         while (t < 1)
//         {
//             t += Time.deltaTime / ANTI_PART_STUCK_DURATION;
//             partRB.transform.position = chassis.CarPhysics.transform.TransformPoint(position);
//             partRB.transform.rotation = chassis.CarPhysics.transform.rotation;
//             partRB.velocity = Vector3.zero;
//             partRB.angularVelocity = Vector3.zero;
//             yield return null;
//         }
//         foreach (var collider in colliders)
//         {
//             collider.enabled = true;
//         }
//         partCoroutines[partRB] = null;
//     }
}
