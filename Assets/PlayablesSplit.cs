//using System;
//using UnityEngine;
//using UnityEngine.Animations;
//using UnityEngine.Playables;
////using UnityEngine.Experimental.Director;

//public class PlayablesSplit : PlayableBehaviour
//{
//    private AnimControllerParams acp;
//    private AnimationClip clip;
//    private AnimationClipPlayable m_ClipPlayable;

//    public void SetAnimCtrlParams(ref AnimControllerParams animControllerParams)
//    {
//        if (clip != animControllerParams.Clip)
//        {
//            clip = animControllerParams.Clip;
//            if (m_ClipPlayable != null)
//            {
//                m_ClipPlayable.Dispose();
//            }
//            m_ClipPlayable = new AnimationClipPlayable(clip);
//            Connect(m_ClipPlayable, this);
//        }

//        acp = animControllerParams;
//    }


//    public override void PrepareFrame(FrameData info)
//    {
//        if (acp.Paused)
//        {

//        }
//        else if (acp.Scrubing)
//        {
//            acp.ClipTime = Mathf.Lerp(acp.StartTime, acp.EndTime, acp.NormalizedPosition);
//        }
//        else
//        {
//            acp.ClipTime += info.deltaTime * acp.Speed;

//            if (acp.Looping)
//            {
//                if (acp.ClipTime > acp.EndTime)
//                {
//                    acp.ClipTime = acp.StartTime;
//                }
//                else if (acp.ClipTime < acp.StartTime)
//                {
//                    acp.ClipTime = acp.EndTime;
//                }
//            }
//            else
//            {
//                acp.ClipTime = acp.EndTime;
//            }

//            acp.NormalizedPosition = Mathf.InverseLerp(acp.StartTime, acp.EndTime, acp.ClipTime);
//        }

//        // Set Time
//        m_ClipPlayable.time = acp.ClipTime;
//    }
//}

//[Serializable]
//public class AnimControllerParams
//{
//    public AnimationClip Clip;

//    public float StartFrame;
//    public float EndFrame;

//    public float Speed;

//    public bool Paused;
//    public bool Looping;
//    public bool Scrubing;

//    [Range(0f, 1f)]
//    public float NormalizedPosition;

//    public float ClipTime;


//    public float StartTime
//    {
//        get { return StartFrame / Clip.frameRate; }
//    }

//    public float EndTime
//    {
//        get { return EndFrame / Clip.frameRate; }
//    }
//}

//[RequireComponent(typeof(Animator))]
//public class PlaySplitClip : MonoBehaviour
//{
//    public AnimControllerParams AnimControllerParams;
//    private PlayablesSplit m_SplitClip;


//    private void Start()
//    {
//        m_SplitClip = new PlayablesSplit(); // Playable.Create<SplitClipPlayable>();
//        UpdateClip();

//        //GetComponent<Animator>().Play(m_SplitClip);
//    }

//    private void UpdateClip()
//    {
//        if (m_SplitClip == null)
//            return;

//        m_SplitClip.SetAnimCtrlParams(ref AnimControllerParams);
//    }

//    private void OnValidate()
//    {
//        UpdateClip();
//    }

//}