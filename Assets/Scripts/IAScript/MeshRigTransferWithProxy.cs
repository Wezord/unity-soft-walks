using UnityEngine;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MeshRigTransferWithProxy : MonoBehaviour
{
    [Header("Source")]
    public SkinnedMeshRenderer sourceMeshRenderer;
    public Transform sourceRootBone;
    
    [Header("Target")]
    public Transform targetRootBone;
    public string targetBonePrefix = "mixamorig:";
    
    [Header("Options")]
    public bool createNewGameObject = true;
    public bool debugMode = true;

    private Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
    private Dictionary<Transform, Transform> proxyBones = new Dictionary<Transform, Transform>();
    
    public void TransferMesh()
    {
        if (sourceMeshRenderer == null || sourceRootBone == null)
        {
            Debug.LogError("Source Mesh Renderer or Root Bone is not assigned!");
            return;
        }

        if (targetRootBone == null)
        {
            Debug.LogError("Target Root Bone is not assigned!");
            return;
        }
        
        // Clear any previous mappings
        boneMap.Clear();
        proxyBones.Clear();
        
        // Map all bones in the target rig
        MapBones(targetRootBone);
        
        if (debugMode)
            Debug.Log($"Found {boneMap.Count} bones in target rig");
        
        // Create proxy bones for each target bone
        CreateProxyBones();
        
        // Create a new object for the remapped mesh if needed
        GameObject newMeshObject;
        SkinnedMeshRenderer newRenderer;
        
        if (createNewGameObject)
        {
            newMeshObject = new GameObject(sourceMeshRenderer.gameObject.name + "_Remapped");
            newMeshObject.transform.position = sourceMeshRenderer.transform.position;
            newMeshObject.transform.rotation = sourceMeshRenderer.transform.rotation;
            newMeshObject.transform.localScale = sourceMeshRenderer.transform.localScale;
            newRenderer = newMeshObject.AddComponent<SkinnedMeshRenderer>();
        }
        else
        {
            newRenderer = sourceMeshRenderer;
        }
        
        // Clone the mesh to avoid modifying the original
        Mesh clonedMesh = Instantiate(sourceMeshRenderer.sharedMesh);
        
        // Map bones from source to proxy bones
        Transform[] newBones = new Transform[sourceMeshRenderer.bones.Length];
        
        int matchedBones = 0;
        for (int i = 0; i < sourceMeshRenderer.bones.Length; i++)
        {
            string originalName = sourceMeshRenderer.bones[i].name;
            
            // Try different name variations to find a match
            Transform mappedBone = FindMatchingBone(originalName);
            
            if (mappedBone != null && proxyBones.TryGetValue(mappedBone, out Transform proxyBone))
            {
                newBones[i] = proxyBone;
                matchedBones++;
            }
            else
            {
                if (debugMode)
                    Debug.LogWarning($"No matching bone found for '{originalName}'");
                
                // Fall back to the root bone if no match
                newBones[i] = proxyBones[targetRootBone];
            }
        }
        
        // Apply the new bones to the renderer
        newRenderer.bones = newBones;
        newRenderer.rootBone = proxyBones[targetRootBone];
        newRenderer.sharedMesh = clonedMesh;
        newRenderer.materials = sourceMeshRenderer.materials;
        
        if (debugMode)
            Debug.Log($"Mesh transferred successfully. Matched {matchedBones} out of {sourceMeshRenderer.bones.Length} bones.");
    }
    
    private void CreateProxyBones()
    {
        // Start by analyzing the source skeleton to get original bone orientations
        Dictionary<string, Transform> sourceBones = new Dictionary<string, Transform>();
        Dictionary<string, Vector3> sourceLocalPositions = new Dictionary<string, Vector3>();
        Dictionary<string, Quaternion> sourceLocalRotations = new Dictionary<string, Quaternion>();
        
        // Collect all source bones
        Transform[] allSourceBones = sourceRootBone.GetComponentsInChildren<Transform>();
        foreach (Transform bone in allSourceBones)
        {
            sourceBones[bone.name] = bone;
            sourceLocalPositions[bone.name] = bone.localPosition;
            sourceLocalRotations[bone.name] = bone.localRotation;
        }
        
        // Create proxy for the root bone first
        GameObject rootProxy = new GameObject(targetRootBone.name.Replace(targetBonePrefix, ""));
        rootProxy.transform.SetParent(targetRootBone);
        rootProxy.transform.localPosition = Vector3.zero;
        rootProxy.transform.localRotation = Quaternion.identity;
        rootProxy.transform.localScale = Vector3.one;
        proxyBones[targetRootBone] = rootProxy.transform;
        
        // Then create proxies for all other bones
        foreach (KeyValuePair<string, Transform> entry in boneMap)
        {
            Transform targetBone = entry.Value;
            
            // Skip if we already created a proxy for this bone
            if (proxyBones.ContainsKey(targetBone))
                continue;
                
            // Create the proxy GameObject
            string cleanName = targetBone.name.Replace(targetBonePrefix, "");
            GameObject proxyObj = new GameObject(cleanName);
            proxyObj.transform.SetParent(targetBone);
            
            // Try to match source bone orientations if available
            if (sourceBones.TryGetValue(cleanName, out Transform sourceBone))
            {
                // Use the source bone's local transform
                proxyObj.transform.position = sourceBone.position;
                
                // Calculate the rotation difference between target and source
                // This is a simplified approach - for complex rigs you may need more advanced matching
                proxyObj.transform.rotation = sourceBone.rotation;
            }
            else
            {
                // Default to identity if no source bone match
                proxyObj.transform.localPosition = Vector3.zero;
                proxyObj.transform.localRotation = Quaternion.identity;
            }
            
            proxyObj.transform.localScale = Vector3.one;
            proxyBones[targetBone] = proxyObj.transform;
        }
    }
    
    private void MapBones(Transform root)
    {
        // Get all bones in the target skeleton
        Transform[] allBones = root.GetComponentsInChildren<Transform>();
        
        foreach (Transform bone in allBones)
        {
            // Store with and without prefix for flexible matching
            string nameWithoutPrefix = bone.name;
            
            if (bone.name.StartsWith(targetBonePrefix))
                nameWithoutPrefix = bone.name.Substring(targetBonePrefix.Length);
            
            // Store bone under multiple possible names for easier lookup
            boneMap[bone.name] = bone;
            boneMap[nameWithoutPrefix] = bone;
            
            // Also try lowercase
            boneMap[bone.name.ToLower()] = bone;
            boneMap[nameWithoutPrefix.ToLower()] = bone;
        }
    }
    
    private Transform FindMatchingBone(string boneName)
    {
        // Try to find an exact match first
        if (boneMap.TryGetValue(boneName, out Transform bone))
            return bone;
        
        // Try lowercase
        if (boneMap.TryGetValue(boneName.ToLower(), out bone))
            return bone;
        
        // Try with prefix
        if (boneMap.TryGetValue(targetBonePrefix + boneName, out bone))
            return bone;
        
        // Try with prefix and lowercase
        if (boneMap.TryGetValue((targetBonePrefix + boneName).ToLower(), out bone))
            return bone;
        
        // No match found
        return null;
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(MeshRigTransferWithProxy))]
public class MeshRigTransferWithProxyEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        MeshRigTransferWithProxy transferScript = (MeshRigTransferWithProxy)target;
        
        EditorGUILayout.Space();
        
        if (GUILayout.Button("Transfer Mesh with Proxy Bones", GUILayout.Height(30)))
        {
            transferScript.TransferMesh();
        }
    }
}
#endif