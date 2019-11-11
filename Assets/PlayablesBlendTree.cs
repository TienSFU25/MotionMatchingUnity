using UnityEngine;

using UnityEngine.Playables;

using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]

public class PlayablesBlendTree : MonoBehaviour
{
    public AnimationClip clip0;
    public AnimationClip clip1;
    PlayableGraph playableGraph;
    AnimationMixerPlayable mixerPlayable;

    void InitShit()
    {
        // Creates the graph, the mixer and binds them to the Animator.

        playableGraph = PlayableGraph.Create();

        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", GetComponent<Animator>());

        mixerPlayable = AnimationMixerPlayable.Create(playableGraph, 2);

        playableOutput.SetSourcePlayable(mixerPlayable);

        // Creates AnimationClipPlayable and connects them to the mixer.

        var clipPlayable0 = AnimationClipPlayable.Create(playableGraph, clip0);

        var clipPlayable1 = AnimationClipPlayable.Create(playableGraph, clip1);

        playableGraph.Connect(clipPlayable0, 0, mixerPlayable, 0);

        playableGraph.Connect(clipPlayable1, 0, mixerPlayable, 1);
    }

    void Start()
    {
        InitShit();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            InitShit();

            float weight = 0.0f;
            mixerPlayable.SetInputWeight(0, 1.0f - weight);
            mixerPlayable.SetInputWeight(1, weight);
            playableGraph.Play();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            InitShit();
            float weight = 1.0f;
            mixerPlayable.SetInputWeight(0, 1.0f - weight);
            mixerPlayable.SetInputWeight(1, weight);

            playableGraph.Play();
        }
    }

    void OnDisable()

    {

        // Destroys all Playables and Outputs created by the graph.

        playableGraph.Destroy();

    }

}