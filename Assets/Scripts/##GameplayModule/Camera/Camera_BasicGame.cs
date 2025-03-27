// using Unity.Netcode;
// using UnityEngine;
// using UnityEngine.EventSystems;

// public class Camera_BasicGame : NetworkBehaviour
// {
//     Camera cam;
//     ClientHero_Holder holder = null;
//     ClientHero_Holder Move_Holder = null;
//     string HostAndClient = "";

//     private void Start()
//     {
//         cam = Camera.main;
//         HostAndClient = NetUtils.LocalID() == 0 ? "HOST" : "CLIENT";
//     }

//     // Update is called once per frame
//     private void Update()
//     {
//         if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject()) 
//         {
//             MouseButtonDown();
//         }

//         if (Input.GetMouseButton(0) && !EventSystem.current.IsPointerOverGameObject())
//         {
//             MouseButton();

//         }
//         if (Input.GetMouseButtonUp(0) && !EventSystem.current.IsPointerOverGameObject())
//         {
//             MouseButtonUp();
//         }

//     }

//     private void MouseButtonDown()
//     {
//         Ray ray = cam.ScreenPointToRay(Input.mousePosition);
//         RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

//         if (holder != null)
//         {
//             holder.ReturnRange();
//             holder = null;
//         }

//         if (hit.collider != null)
//         {
//             holder = hit.collider.GetComponent<ClientHero_Holder>();
//             Debug.Log(holder);
//             if(holder.Holder_Name == "" || holder.Holder_Name == null)
//             {
//                 holder = null;
//                 return;
//             }

//             bool CanGet = false;
//             int value = (int)NetworkManager.Singleton.LocalClientId;

//             if (value == 0) CanGet = holder.Holder_Part_Name.Contains("HOST");
//             else if (value == 1) CanGet = holder.Holder_Part_Name.Contains("CLIENT");

//             if (!CanGet) holder = null;
//         }
//     }

//     // ���콺�� ������ �ִ� ����
//     private void MouseButton()
//     {
//         if (holder != null)
//         {
//             holder.G_GetClick(true);
//             Ray ray = cam.ScreenPointToRay(Input.mousePosition);
//             RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);




//             if(hit.collider != null && hit.collider.transform != holder.transform)
//             {
//                 if(hit.collider.GetComponent<ClientHero_Holder>() == null) return;
//                 if(hit.collider.GetComponent<ClientHero_Holder>().Holder_Part_Name.Contains(HostAndClient) == false)
//                 {
//                     return;
//                 }

//                 if (Move_Holder != null)
//                 {
//                     Move_Holder.S_SetClick(false);
//                 }

//                 Move_Holder = hit.collider.GetComponent<ClientHero_Holder>();
//                 Move_Holder.S_SetClick(true);
//             }

//         }
//     }

//     private void MouseButtonUp()
//     {
//         if (holder == null) return;

//         if(Move_Holder == null)
//         {
//             Ray ray = cam.ScreenPointToRay(Input.mousePosition);
//             RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

//             if (hit.collider != null)
//             {
//                 if (holder.transform == hit.collider.transform)
//                 {
//                     holder.GetRange();

//                 }
//             }
//         }
//         else
//         {

//             Move_Holder.S_SetClick(false);

//             ClientSpawner.instance.Holder_Position_Set(holder.Holder_Part_Name, Move_Holder.Holder_Part_Name);
//         }
//         if (holder != null)
//         {   
//             holder.G_GetClick(false);
//         }

//         Move_Holder = null;

//     }
// }
