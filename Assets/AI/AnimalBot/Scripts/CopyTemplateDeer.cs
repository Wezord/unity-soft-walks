using System;
using System.Collections.Generic;
using System.Reflection;
using Unity.MLAgents.Demonstrations;
using Unity.MLAgents.Policies;
using UnityEngine;
using UnityEngine.Assertions;

[ExecuteAlways]
public class CopyTemplateDeer : MonoBehaviour{
    public GameObject templateSrc;

    public bool isInit = true;
    public bool isUpdated = true;

    private void Update(){
        if(!templateSrc) return;
        if(!isInit){
            // init the components
            CopyProperties();
            CopyJoints();
            isInit = true;
            isUpdated = false;
        }

        if(!isUpdated){
            // update with correct values
            CopyComponent<HumanBotParameters>();
            UpdateBehaviorParams();
            GetComponent<DeerBodyMass>().hasRun = false;
            UpdateAnimator();
            isUpdated = true;
        }
    }

    public static T GetCopyOf<T>(Component comp, T other) where T : Component
    {
        Type type = comp.GetType();
        if (type != other.GetType()) return null; // type mis-match
        BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;// | BindingFlags.Default | BindingFlags.DeclaredOnly;
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
        if(!templateSrc.GetComponent<T>()){
            return;
        }
        var cg = gameObject.GetComponent<T>();
        if(!cg){
            cg = gameObject.AddComponent<T>();
        }
        cg = GetCopyOf(cg, templateSrc.GetComponent<T>());
    }


    private void CopyJoints(){
        var templateSrcJoints = templateSrc.GetComponent<TemplateDeer>().GetJointsToCopy();
        var templateDstJoints = GetComponent<TemplateDeer>().GetJointsToCopy();
        Assert.AreEqual(templateSrcJoints.Count, templateDstJoints.Count);
        for(int i=0; i<templateSrcJoints.Count; i++){
            var curJointSrc = templateSrcJoints[i].GetComponent<BotJoint>(); 
            var curJointDst = templateDstJoints[i];
            if(!curJointDst.GetComponent<CopyTemplateJoints>()){
                curJointDst.gameObject.AddComponent<CopyTemplateJoints>();
            }
            var copyJointScript = curJointDst.GetComponent<CopyTemplateJoints>();
            copyJointScript.templateAgent = curJointSrc;
            copyJointScript.hasRun = false;
            var parent = curJointDst.transform.parent;
            while(!parent.GetComponent<Rigidbody>()){
                parent = parent.transform.parent;
            }
            if(!curJointDst.GetComponent<ConfigurableJoint>()){
                curJointDst.gameObject.AddComponent<ConfigurableJoint>();
            }
            curJointDst.GetComponent<ConfigurableJoint>().connectedBody = parent.GetComponent<Rigidbody>();
        }
    }


    private void CopyProperties(){
        CopyComponent<BehaviorParameters>();
        CopyComponent<Rigidbody>();
        CopyComponent<HumanBotParameters>();
        CopyComponent<HumanBotAgent>();
        CopyComponent<DemonstrationRecorder>();
        CopyComponent<ImitationRecorder>();
        CopyComponent<DeerBodyMass>();
        CopyComponent<AgentEvents>();
    }


    private void UpdateBehaviorParams(){
        GetComponent<HumanBotParameters>().root = GetComponent<TemplateDeer>().root.GetComponent<BotJoint>();
        GetComponent<HumanBotParameters>().head = GetComponent<TemplateDeer>().neck.GetComponent<BotJoint>();
        // GetComponent<HumanBotParameters>().leftHand = GetComponent<TemplateDeer>().leftBackFoot.GetComponent<BotJoint>();
        // GetComponent<HumanBotParameters>().rightHand = GetComponent<TemplateDeer>().rightBackFoot.GetComponent<BotJoint>();

        // GetComponent<HumanBotParameters>().feet = GetComponent<TemplateDeer>().leftFrontFoot.GetComponent<BotJoint>();
    }

    private void UpdateAnimator(){
        var animator = GetComponent<Animator>();
        if(animator){
            GetComponent<ImitationRecorder>().controllers = new RuntimeAnimatorController[]{animator.runtimeAnimatorController};
            GetComponent<HumanBotParameters>().animator = animator;
        }
    }

}