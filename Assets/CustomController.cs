using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;

using static dbConnector;

// Create a menu item that causes a new controller and statemachine to be created.

public class CustomController : MonoBehaviour
{
    private dbConnector dbcon;
    private PathReader pathReader;
    private MocapLoader mocapLoader;

    // top n matches
    public const int NumPoseMatch = 10;
    public const int NumTrajMatch = 10;
    public const int FPS = 30;

    private const string IdleFile = "Idle2Run_AllAngles";

    // initialize these to be the idle file
    private int currentClipIndex;
    private int currentFrameIndex = 0;
    private float timeSinceLastUpdate = 0.0f;

    Animator anim;

    AnimatorController controller;
    private System.Random rng = new System.Random();

    private bool hasInit = false;

    void Start()
    {
        anim = GetComponent<Animator>();

        // Creates the controller
        controller = new AnimatorController();
        controller.AddParameter("Go to A2", AnimatorControllerParameterType.Trigger);
        controller.AddParameter("Go to A1", AnimatorControllerParameterType.Trigger);
        controller.AddLayer("nakedLayer");

        // Add StateMachines
        var rootStateMachine = controller.layers[0].stateMachine;
        //var stateMachineA = rootStateMachine.AddStateMachine("smA");

        anim.runtimeAnimatorController = controller;

        dbcon = ScriptableObject.CreateInstance("dbConnector") as dbConnector;
        dbcon.LoadStuff();

        pathReader = ScriptableObject.CreateInstance("PathReader") as PathReader;
        pathReader.Initialize();

        mocapLoader = ScriptableObject.CreateInstance("MocapLoader") as MocapLoader;
        mocapLoader.Initialize(rootStateMachine);
    }

    // gg unity
    private void LateUpdate()
    {
        if (!hasInit && anim.isActiveAndEnabled && anim.runtimeAnimatorController != null) { 
            // play the idle file
            currentClipIndex = mocapLoader.clipNames.FindIndex(a => a == IdleFile);
            anim.Play(IdleFile, 0, 0);
            hasInit = true;
        }
    }

    private void OnEnable()
    {
        //MotionMatch();
    }


    private float MotionMatch()
    {
        float newTime = Time.fixedTime;
        float timeClipRunning = newTime - timeSinceLastUpdate;

        AnimatorStateInfo animationInfo = anim.GetCurrentAnimatorStateInfo(0);
        //AnimatorClipInfo[] animationClip = anim.GetCurrentAnimatorClipInfo(0);

        var currentClip = mocapLoader.getClipAtIndex(currentClipIndex);
        float fn = 0;

        if (currentClip != null)
        {
            fn = currentClip.length * animationInfo.normalizedTime;
        }

        Debug.Log("last clip ran for " + timeClipRunning + " seconds, ending at frame " + (int)fn);

        currentFrameIndex = (int)fn;

        // skip this pose?
        var poseMatches = dbcon.diffIndexMatrix[currentClipIndex];
        List<int> bestPoseMatches = poseMatches.Take(NumPoseMatch).ToList();

        Debug.Log("best pose matches" + string.Join(",", bestPoseMatches));

        // ignore pose cost for now, just match the best trajectory
        var nextTrajectory = pathReader.sample(newTime);
        int bestClipNumber = 0;
        int bestFrameNumber = 0;
        float bestTrajectoryCost = 1000000;

        for (int i = 0; i < bestPoseMatches.Count; i++)
        {
            int poseIndex = bestPoseMatches[i];
            (int clipNumber, int frameNumber) = dbcon.belongsTo[poseIndex];

            // change trajectory matrix later for edge case
            if (frameNumber > dbcon.trajectoryMatrix[clipNumber].Count - 1) continue;

            List<Point> trajectory = dbcon.trajectoryMatrix[clipNumber][frameNumber];
            float trajectoryCost = pathReader.trajectoryDifference(nextTrajectory, trajectory);

            if (trajectoryCost < bestTrajectoryCost)
            {
                bestTrajectoryCost = trajectoryCost;
                bestClipNumber = clipNumber;
                bestFrameNumber = frameNumber;
            }
        }

        currentClipIndex = bestClipNumber;
        currentFrameIndex = bestFrameNumber;

        return bestTrajectoryCost;
    }

    private int GetCurrentClipFrameNumber()
    {
        AnimatorStateInfo animationInfo = anim.GetCurrentAnimatorStateInfo(0);
        AnimatorClipInfo[] animationClip = anim.GetCurrentAnimatorClipInfo(0);

        if (animationClip != null && animationClip.Length > 0)
        {
            var a = animationClip[0].clip;
            var b = Mathf.Clamp(animationInfo.normalizedTime, 0, 1);
            //var fn = currentClip.length * animationInfo.normalizedTime;
            var frameNumber = a.length * b * FPS;

            Debug.Log("playing clip " + a.name + " at frame " + (int)frameNumber);
            return (int)frameNumber;
        }

        return -1;
    }

    private void ChangeRandomClip()
    {
        var nextClipIndex = currentClipIndex;
        while (nextClipIndex == currentClipIndex)
        {
            nextClipIndex = rng.Next(mocapLoader.clipNames.Count - 1);
        }

        currentClipIndex = nextClipIndex;
        AnimationClip clip = mocapLoader.getClipAtIndex(nextClipIndex);
        currentFrameIndex = rng.Next(0, (int)(clip.length * FPS - 2));
    }

    private void PlayClip(int cost)
    {
        //if (bestClipNumber != CurrentPoseClipNumber && bestFrameNumber != CurrentPoseFrameNumber)
        //{

        var matchingMocap = dbcon.mocapOrder[currentClipIndex];
        var matchingMocapFileName = Path.GetFileNameWithoutExtension(matchingMocap.Item1);
        var matchingMocapNumFrames = matchingMocap.Item2;

        var normalizedFrameNum = (float)currentFrameIndex / (float)matchingMocapNumFrames;

        Debug.Log("Matched best clip " + matchingMocapFileName + ", at frame " + currentFrameIndex + ", with cost " + cost);
        //anim.CrossFade(matchingMocapFileName, 0.5f, 0, normalizedFrameNum);
        anim.Play(matchingMocapFileName, 0, normalizedFrameNum);

        //}
    }

    private void PoseMatch()
    {
        AnimatorStateInfo animationInfo = anim.GetCurrentAnimatorStateInfo(0);
        AnimatorClipInfo[] animationClip = anim.GetCurrentAnimatorClipInfo(0);

        var a = animationClip[0].clip;
        var b = Mathf.Clamp(animationInfo.normalizedTime, 0, 1);
        //var fn = currentClip.length * animationInfo.normalizedTime;
        currentFrameIndex = (int)(a.length * b * FPS);

        var poseMatches = dbcon.diffIndexMatrix[currentClipIndex];
        int numMatches = 0;
        int idx = 0;

        List<int> bestPoseMatches = new List<int>();

        while (numMatches < NumPoseMatch)
        {
            var poseIndex = poseMatches[idx];
            idx += 1;
            (int clipNumber, int frameNumber) = dbcon.belongsTo[poseIndex];

            if (clipNumber != currentClipIndex || frameNumber < currentFrameIndex - margin || frameNumber > currentFrameIndex + margin)
            {
                numMatches += 1;
                bestPoseMatches.Add(poseIndex);
            }
        }

        var randomGoodPoseIndex = rng.Next(numMatches - 1);
        (currentClipIndex, currentFrameIndex) = dbcon.belongsTo[randomGoodPoseIndex];

        Debug.Log("switching to clip " + mocapLoader.getClipAtIndex(currentClipIndex).name + " at frame " + currentFrameIndex);
    }

    private void Update()
    {
        //GetCurrentClipFrameNumber();

        if (Input.GetKeyDown(KeyCode.Z))
        {
            ChangeRandomClip();
            PlayClip(0);
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            PoseMatch();
            PlayClip(0);
        }
        else if (Input.GetKeyDown(KeyCode.C))
        {
            MotionMatch();
        }
    }
}