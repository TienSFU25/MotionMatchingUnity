using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

public class PlayQueuePlayable2 : PlayableBehaviour
{
    private int m_CurrentClipIndex = -1;
    private float m_TimeToNextClip;
    private Playable mixer;

    public void Initialize(AnimationClip[] clipsToPlay, Playable owner, PlayableGraph nakedGraph)
    {
        owner.SetInputCount(1);
        mixer = AnimationMixerPlayable.Create(nakedGraph, clipsToPlay.Length);

        nakedGraph.Connect(mixer, 0, owner, 0);
        owner.SetInputWeight(0, 1);

        for (int clipIndex = 0; clipIndex < mixer.GetInputCount(); ++clipIndex)
        {
            nakedGraph.Connect(AnimationClipPlayable.Create(nakedGraph, clipsToPlay[clipIndex]), 0, mixer, clipIndex);
            mixer.SetInputWeight(clipIndex, 1.0f);
        }
    }

    override public void PrepareFrame(Playable owner, FrameData info)
    {
        if (mixer.GetInputCount() == 0)
            return;

        // Advance to next clip if necessary
        m_TimeToNextClip -= (float)info.deltaTime;

        if (m_TimeToNextClip <= 0.0f)
        {
            m_CurrentClipIndex++;
            if (m_CurrentClipIndex >= mixer.GetInputCount())
                m_CurrentClipIndex = 0;

            var currentClip = (AnimationClipPlayable)mixer.GetInput(m_CurrentClipIndex);

            // Reset the time so that the next clip starts at the correct position
            currentClip.SetTime(0);

            m_TimeToNextClip = currentClip.GetAnimationClip().length;
        }

        // Adjust the weight of the inputs
        for (int clipIndex = 0; clipIndex < mixer.GetInputCount(); ++clipIndex)
        {
            if (clipIndex == m_CurrentClipIndex)
                mixer.SetInputWeight(clipIndex, 1.0f);

            else
                mixer.SetInputWeight(clipIndex, 0.0f);
        }
    }
}

[RequireComponent(typeof(Animator))]
public class PlayablesCustomMixer : MonoBehaviour
{
    public AnimationClip[] clipsToPlay;
    PlayableGraph playableGraph;

    void Start()
    {
        playableGraph = PlayableGraph.Create();

        var playQueuePlayable = ScriptPlayable<PlayQueuePlayable2>.Create(playableGraph);

        var playQueue = playQueuePlayable.GetBehaviour();

        playQueue.Initialize(clipsToPlay, playQueuePlayable, playableGraph);

        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());

        playableOutput.SetSourcePlayable(playQueuePlayable);
        playableOutput.SetSourceInputPort(0);

        playableGraph.Play();
    }

    void OnDisable()
    {
        // Destroys all Playables and Outputs created by the graph.
        playableGraph.Destroy();
    }
}