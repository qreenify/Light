using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ImageEffectAllowedInSceneView]
public class SphericalMaskPpController : MonoBehaviour
{
    public Material orgMaterial;
    [Serializable]
    public class ColourSphere
    {
        public Material mat = null;
        public Vector3 spherePosition;
        public float radius = 1;
        public float softness = 1;
    }
    
    public List<ColourSphere> colourSpheres;

    public Camera CameraTest => GetComponent<Camera>();
    
    void OnEnable()
    {
        UpdateSphereMaterial();
        CameraTest.depthTextureMode = DepthTextureMode.Depth;
    }

    private void UpdateSphereMaterial()
    {
        foreach (var sphere in colourSpheres.Where(sphere => sphere.mat == null))
        {
            sphere.mat = Instantiate(orgMaterial);
        }
    }

    void OnRenderImage (RenderTexture src, RenderTexture dest) {
        if (orgMaterial == null || colourSpheres == null) {
            return;
        }
        var p = GL.GetGPUProjectionMatrix (CameraTest.projectionMatrix, false);
        p[2, 3] = p[3, 2] = 0.0f;
        p[3, 3] = 1.0f;
        var clipToWorld = Matrix4x4.Inverse (p * CameraTest.worldToCameraMatrix) * Matrix4x4.TRS (new Vector3 (0, 0, -p[2, 2]), Quaternion.identity, Vector3.one);
        
        
        
        colourSpheres[0].mat.SetMatrix ("_ClipToWorld", clipToWorld);
        colourSpheres[0].mat.SetVector ("_Position", colourSpheres[0].spherePosition);
        colourSpheres[0].mat.SetFloat ("_Radius", colourSpheres[0].radius);
        colourSpheres[0].mat.SetFloat ("_Softness", colourSpheres[0].softness);
        Graphics.Blit (src, dest, colourSpheres[0].mat);
    }
}

#if UNITY_EDITOR
[CustomEditor (typeof (SphericalMaskPpController))]
public class SphericalMaskPpControllerEditor : Editor {
    private void OnSceneGUI() {
        SphericalMaskPpController controller = target as SphericalMaskPpController;
        
        if (controller is null) return;
        
        foreach (var sphere in controller.colourSpheres)
        {
            Vector3 spherePosition = sphere.spherePosition;
            
            EditorGUI.BeginChangeCheck();
            spherePosition = Handles.DoPositionHandle(spherePosition, Quaternion.identity);
            if (EditorGUI.EndChangeCheck())
            {
                Undo.RecordObject(controller, "Move sphere pos");
                EditorUtility.SetDirty(controller);
                sphere.spherePosition = spherePosition;
            }

            Handles.DrawWireDisc(sphere.spherePosition, Vector3.up, sphere.radius);
            Handles.DrawWireDisc(sphere.spherePosition, Vector3.forward, sphere.radius);
            Handles.DrawWireDisc(sphere.spherePosition, Vector3.right, sphere.radius);
        }
    }
}
#endif



