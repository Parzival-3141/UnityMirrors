using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Mirror : MonoBehaviour
{
    private MeshRenderer mirror;
    private Camera playerCam;
    private Transform playerCamTrans;
    private Camera mirrorCam;
    private RenderTexture viewTexture;

    public float nearClipOffset = 0.05f;
    public float nearClipLimit  = 0.2f;


    private void Awake()
    {
        mirror = GetComponent<MeshRenderer>();
        mirror.enabled = false;
        
        playerCam = Camera.main;
        playerCamTrans = playerCam.transform;
        mirrorCam = GetComponentInChildren<Camera>();
        mirrorCam.fieldOfView = playerCam.fieldOfView;

        mirror.material.shader = Shader.Find("Custom/Mirror");
        mirror.material.SetInt("displayMask", 1);
    }

    private void CreateViewTexture()
    {
        if(viewTexture == null || viewTexture.width != Screen.width || viewTexture.height != Screen.height)
        {
            if (viewTexture != null)
                viewTexture.Release();

            viewTexture = new RenderTexture(Screen.width, Screen.height, 0);
            mirrorCam.targetTexture = viewTexture;

            mirror.material.SetTexture("_MainTex", viewTexture);
        }
    }

    public void Render()
    {
        if(!IsVisibleFrom(mirror, playerCam))
        {
            var testTex = new Texture2D(1, 1);
            testTex.SetPixel(0, 0, Color.red);
            testTex.Apply();
            mirror.material.SetTexture("_MainTex", testTex); // Debug

            return;
        }

        mirror.material.SetTexture("_MainTex", viewTexture);
        mirror.enabled = false;
        CreateViewTexture();

        ReflectCamera();
        SetNearClipPlane();
        mirrorCam.Render();
        mirror.enabled = true;
    }

    private void ReflectCamera()
    {
        // FINALLY, it works!! hell yeah B)
        Vector3 mirrorNormal = transform.forward;
        Vector3 worldToPlane = Vector3.ProjectOnPlane(playerCamTrans.position - transform.position, transform.forward) + transform.position;
        mirrorCam.transform.position = 2 * worldToPlane - playerCamTrans.position;

        mirrorCam.transform.rotation = ReflectRotation(playerCamTrans.rotation, mirrorNormal);
    }

    private Quaternion ReflectRotation(Quaternion source, Vector3 normal)
    {
        return Quaternion.LookRotation(Vector3.Reflect(source * Vector3.forward, normal), Vector3.Reflect(source * Vector3.up, normal));
    }

    private Vector3 Intersect(Vector3 planePoint, Vector3 planeNormal, Vector3 rayOrigin, Vector3 rayEnd)
    {
        var d = Vector3.Dot(planePoint, -planeNormal);
        var t = -(d + rayOrigin.z * planeNormal.z + rayOrigin.y * planeNormal.y + rayOrigin.x * planeNormal.x) / (rayEnd.z * planeNormal.z + rayEnd.y * planeNormal.y + rayEnd.x * planeNormal.x);
        return rayOrigin + t * rayEnd;
    }

    // http://wiki.unity3d.com/index.php/IsVisibleFrom
    private bool IsVisibleFrom(Renderer renderer, Camera camera)
    {
        Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(camera);
        return GeometryUtility.TestPlanesAABB(frustumPlanes, renderer.bounds);
    }

    // https://github.com/SebLague/Portals/blob/master/Assets/Scripts/Core/Portal.cs Thanks Seb Lague!
    private void SetNearClipPlane()
    {
        // Learning resource:
        // http://www.terathon.com/lengyel/Lengyel-Oblique.pdf
        Transform clipPlane = transform;
        int dot = System.Math.Sign(Vector3.Dot(clipPlane.forward, transform.position - mirrorCam.transform.position));

        Vector3 camSpacePos = mirrorCam.worldToCameraMatrix.MultiplyPoint(clipPlane.position);
        Vector3 camSpaceNormal = mirrorCam.worldToCameraMatrix.MultiplyVector(clipPlane.forward) * dot;
        float camSpaceDst = -Vector3.Dot(camSpacePos, camSpaceNormal) + nearClipOffset;

        // Don't use oblique clip plane if very close to portal as it seems this can cause some visual artifacts
        if (Mathf.Abs(camSpaceDst) > nearClipLimit)
        {
            Vector4 clipPlaneCameraSpace = new Vector4(camSpaceNormal.x, camSpaceNormal.y, camSpaceNormal.z, camSpaceDst);

            // Update projection based on new clip plane
            // Calculate matrix with player cam so that player camera settings (fov, etc) are used
            mirrorCam.projectionMatrix = playerCam.CalculateObliqueMatrix(clipPlaneCameraSpace);
        }
        else
        {
            mirrorCam.projectionMatrix = playerCam.projectionMatrix;
        }
    }

	private void OnDrawGizmos()
	{	
		Gizmos.DrawWireMesh(Resources.GetBuiltinResource<Mesh>("Quad.fbx"), transform.position, transform.rotation, transform.lossyScale);
	}

    private void OnDrawGizmosSelected()
    {
        if (mirrorCam != null && playerCam != null)
        {
            var worldToPlane = Vector3.ProjectOnPlane(playerCamTrans.position - transform.position, transform.forward) + transform.position;
            Gizmos.DrawLine(Vector3.zero, worldToPlane);

            Gizmos.color = Color.red;
            Gizmos.DrawLine(playerCamTrans.position, worldToPlane);
            
            Gizmos.color = Color.green;
            Vector3 mirrorCam = 2 * worldToPlane - playerCamTrans.position;
            Gizmos.DrawLine(worldToPlane, mirrorCam);
        }
    }
}
