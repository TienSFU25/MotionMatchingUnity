using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections;
using System.Collections.Generic;

// Create a menu item that causes a new controller and statemachine to be created.

public class CustomController : MonoBehaviour
{
    Animator anim;
    public AnimationClip clipZ;

    private List<string> clipsGiveShit;
    private AnimatorState stateA1;
    private string assetFolder = "Assets/Animations/Walking/";
    private int index = 0;

    //[MenuItem("MyMenu/Create Controller")]
    void Start()
    {
        anim = GetComponent<Animator>();

        // Creates the controller
        var controller = new AnimatorController();
        
        // Add parameters
        controller.AddParameter("Go to A2", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Go to A1", AnimatorControllerParameterType.Trigger);
        controller.AddLayer("nakedLayer");

        // Add StateMachines
        var rootStateMachine = controller.layers[0].stateMachine;
        var stateMachineA = rootStateMachine.AddStateMachine("smA");

        // Add States
        stateA1 = stateMachineA.AddState("stateA1");
        var stateA2 = stateMachineA.AddState("stateA2");
        var trans1 = stateA1.AddTransition(stateA2);
        var trans2 = stateA2.AddTransition(stateA1);

        trans1.AddCondition(AnimatorConditionMode.If, 0, "Go to A2");
        trans2.AddCondition(AnimatorConditionMode.If, 0, "Go to A1");
        
        anim.runtimeAnimatorController = controller;

        //AssetDatabase.AddObjectToAsset(clipZ, AssetDatabase.GetAssetPath(this));

        var assetPath = "Assets/WalkingAnimations/WalkBackward_NtrlFaceFwd.fbx";
        ModelImporter modelImporter = (ModelImporter)AssetImporter.GetAtPath(assetPath);
        
        ModelImporterClipAnimation clip = modelImporter.clipAnimations[0]; // get first clip
        var thingy = AssetImporter.GetAtPath(assetPath);
        AnimationClip a = (AnimationClip)AssetDatabase.LoadAssetAtPath(assetPath, typeof(AnimationClip));

        stateA1.motion = clipZ;
        stateA2.motion = a;

        var allAssetPaths = AssetDatabase.GetAllAssetPaths();
        clipsGiveShit = new List<string>();
        foreach (var ap in allAssetPaths)
        {
            if (ap.Contains(assetFolder))
            {
                clipsGiveShit.Add(ap);
            }
        }
    }

    private void Update()
    {
        //AnimatorStateInfo animationInfo = anim.GetCurrentAnimatorStateInfo(0);
        //AnimatorClipInfo[] animationClip = anim.GetCurrentAnimatorClipInfo(0);

        //float time = animationClip[0].clip.length * animationInfo.normalizedTime;

        if (Input.GetKeyDown(KeyCode.Z))
        {
            anim.Play("stateA1", 0, 0.5f);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            //var assetPath = "Assets/Animations/Interacting/IdleGrab_FrontHigh.fbx";
            var assetPath = clipsGiveShit[index];
            index += 1;

            Debug.Log("trying to get shit at " + assetPath);
            ModelImporter modelImporter = (ModelImporter)AssetImporter.GetAtPath(assetPath);
            ModelImporterClipAnimation clip = modelImporter.clipAnimations[0]; // get first clip
            var thingy = AssetImporter.GetAtPath(assetPath);
            AnimationClip a = (AnimationClip)AssetDatabase.LoadAssetAtPath(assetPath, typeof(AnimationClip));

            stateA1.motion = a;
        }
    }
}