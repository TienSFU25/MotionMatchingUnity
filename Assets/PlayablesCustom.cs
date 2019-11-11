using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayQueuePlayable : PlayableBehaviour
{
    private int m_CurrentClipIndex = -1;
    private float m_TimeToNextClip;
    private PlayableGraph playableGraph;
    private AnimationClip[] clips;
    private Animator anim;

    //private Playable mixer;

    public void Initialize(AnimationClip[] clipsToPlay, Playable scriptPlayable, PlayableGraph graph, Animator animmy)
    {
        playableGraph = graph;
        clips = clipsToPlay;
        anim = animmy;
        
        scriptPlayable.SetInputCount(clipsToPlay.Length);

        var clippy = clips[0];

        // Wrap the clip in a playable
        var clipPlayable = AnimationClipPlayable.Create(playableGraph, clippy);
        clipPlayable.CanChangeInputs();
        clipPlayable.SetTime(0);
        graph.Connect(clipPlayable, 0, scriptPlayable, 0);

        var c1 = clips[1];

        var c1Playable = AnimationClipPlayable.Create(playableGraph, clippy);
        //c1Playable.CanChangeInputs();
        c1Playable.SetTime(0);

        //scriptPlayable.AddInput(clipPlayable, 0);
        //graph.Connect(clipPlayable, 0, c1Playable, 0);
        //graph.Connect(c1Playable, 0, clipPlayable, 0);
        graph.Connect(c1Playable, 0, scriptPlayable, 1);

        // Connect the Playable to an output
        //var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", anim);
        //playableOutput.SetSourcePlayable(clipPlayable);
    }

    override public void PrepareFrame(Playable owner, FrameData info)
    {
        //if (mixer.GetInputCount() == 0)
        //    return;

        // Advance to next clip if necessary
        m_TimeToNextClip -= (float)info.deltaTime;

        if (m_TimeToNextClip <= 0.0f)
        {
            m_CurrentClipIndex++;

            var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", anim);
            var clippy = clips[m_CurrentClipIndex];

            // Wrap the clip in a playable
            var clipPlayable = AnimationClipPlayable.Create(playableGraph, clippy);
            clipPlayable.SetTime(0);

            // Connect the Playable to an output
            playableOutput.SetSourcePlayable(clipPlayable);

            m_TimeToNextClip = clippy.length;
        }

        // Adjust the weight of the inputs
        //for (int clipIndex = 0; clipIndex < mixer.GetInputCount(); ++clipIndex)
        //{
        //    if (clipIndex == m_CurrentClipIndex)
        //        mixer.SetInputWeight(clipIndex, 1.0f);

        //    else
        //        mixer.SetInputWeight(clipIndex, 0.0f);
        //}
    }
}

[RequireComponent(typeof(Animator))]
public class PlayablesCustom : MonoBehaviour
{
    public AnimationClip[] clipsToPlay;
    PlayableGraph playableGraph;

    void Start()
    {
        playableGraph = PlayableGraph.Create();

        var scriptPlayable = ScriptPlayable<PlayQueuePlayable>.Create(playableGraph);
        var playQueue = scriptPlayable.GetBehaviour();
        playQueue.Initialize(clipsToPlay, scriptPlayable, playableGraph, GetComponent<Animator>());

        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());

        playableOutput.SetSourcePlayable(scriptPlayable);
        playableOutput.SetSourceOutputPort(0);

        playableGraph.Play();
    }

    void OnDisable()
    {
        // Destroys all Playables and Outputs created by the graph.
        playableGraph.Destroy();
    }
}