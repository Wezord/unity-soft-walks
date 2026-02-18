using System;
using System.Reflection;
using UnityEngine;

[ExecuteAlways]
public class CopyTemplateJoints : MonoBehaviour{

    public bool hasRun = true;

    public BotJoint templateAgent;

    private void Update(){
        if(hasRun) return;
        CopyComponent<Rigidbody>();
        CopyComponent<ConfigurableJoint>();
        CopyComponent<CapsuleCollider>();
        CopyComponent<BoxCollider>();
        CopyComponent<BotJoint>();
        hasRun = true;
        FixCapsuleCollider();
    }

    public static T GetCopyOf<T>(Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Default | BindingFlags.DeclaredOnly;
        PropertyInfo[] pinfos = type.GetProperties(flags);
        foreach (var pinfo in pinfos) {
            if (pinfo.CanWrite) {
                try {
                    pinfo.SetValue(comp, pinfo.GetValue(other, null), null);
                }
                catch { } // In case of NotImplementedException being thrown. For some reason specifying that exception didn't seem to catch it, so I didn't catch anything specific.
            }
        }
        FieldInfo[] finfos = type.GetFields(flags);
        foreach (var finfo in finfos) {
            finfo.SetValue(comp, finfo.GetValue(other));
        }
        return comp as T;
    }

    private void CopyComponent<T>() where T : Component{
        if(!templateAgent.GetComponent<T>()){
            return;
        }
        var cg = gameObject.GetComponent<T>();
        if(!cg){
            cg = gameObject.AddComponent<T>();
        }
        cg = GetCopyOf(cg, templateAgent.GetComponent<T>());
    }

    private void FixCapsuleCollider(){
        var oldCollider = templateAgent.GetComponent<CapsuleCollider>();
        if(!oldCollider){
            return;
        }
        var oldCenterWs = oldCollider.transform.TransformPoint(oldCollider.center) - oldCollider.transform.position;
        var newCenterLs = transform.InverseTransformPoint(transform.position + oldCenterWs);
        transform.GetComponent<CapsuleCollider>().center = newCenterLs;

        Vector3 oldDirection = Vector3.zero;
        switch(oldCollider.direction){
            case 0:
                oldDirection = Vector3.right;
                break;
            case 1:
                oldDirection = Vector3.up;
                break;
            case 2:
                oldDirection = Vector3.forward;
                break;
        }

        Vector3 oldDirectionWs = oldCollider.transform.TransformVector(oldDirection);
        Vector3 newDirectionWs = transform.InverseTransformVector(oldDirectionWs);
        if(Mathf.Abs(newDirectionWs.x) >= Mathf.Abs(newDirectionWs.y) && Mathf.Abs(newDirectionWs.x) >= Mathf.Abs(newDirectionWs.z)){
            transform.GetComponent<CapsuleCollider>().direction = 0;
            transform.GetComponent<ConfigurableJoint>().axis = Vector3.right;
            transform.GetComponent<ConfigurableJoint>().secondaryAxis = Vector3.up;
        }
        else if(Mathf.Abs(newDirectionWs.y) >= Mathf.Abs(newDirectionWs.x) && Mathf.Abs(newDirectionWs.y) >= Mathf.Abs(newDirectionWs.z)){
            transform.GetComponent<CapsuleCollider>().direction = 1;
            transform.GetComponent<ConfigurableJoint>().axis = Vector3.up;
            transform.GetComponent<ConfigurableJoint>().secondaryAxis = Vector3.forward;
        }
        else{
            transform.GetComponent<CapsuleCollider>().direction = 2;
            transform.GetComponent<ConfigurableJoint>().axis = Vector3.forward;
            transform.GetComponent<ConfigurableJoint>().secondaryAxis = Vector3.right;
        }
        Debug.Log(newDirectionWs);

    }
}