using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class PlayablesState : MonoBehaviour
{
    public AnimationClip clip0;
    public AnimationClip clip1;
    PlayableGraph playableGraph;
    AnimationMixerPlayable mixerPlayable;

    AnimationClipPlayable clipPlayable0;
    AnimationClipPlayable clipPlayable1;
    Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();

        // Creates the graph, the mixer and binds them to the Animator.
        playableGraph = PlayableGraph.Create();
        var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation", animator);
        mixerPlayable = AnimationMixerPlayable.Create(playableGraph, 2);
        playableOutput.SetSourcePlayable(mixerPlayable);

        // Creates AnimationClipPlayable and connects them to the mixer.
        clipPlayable0 = AnimationClipPlayable.Create(playableGraph, clip0);
        clipPlayable1 = AnimationClipPlayable.Create(playableGraph, clip1);

        playableGraph.Connect(clipPlayable0, 0, mixerPlayable, 0);
        playableGraph.Connect(clipPlayable1, 0, mixerPlayable, 1);

        // nope
        //playableGraph.Connect(clipPlayable0, 1, clipPlayable1, 1);

        //clipPlayable1.SetPlayState(PlayState.Paused);
        //clipPlayable0.CanChangeInputs
        //clipPlayable0.AddInput(clipPlayable1, 1);
        //clipPlayable0.ConnectInput(0, clipPlayable1, 1);

        clipPlayable0.Pause();
        clipPlayable1.Pause();

        // Plays the Graph.
        playableGraph.Play();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            clipPlayable1.Pause();

            mixerPlayable.SetInputWeight(0, 1.0f);
            mixerPlayable.SetInputWeight(1, 0.0f);

            clipPlayable0.SetTime(0);
            clipPlayable1.SetTime(0);

            clipPlayable0.Play();
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            clipPlayable0.Pause();

            mixerPlayable.SetInputWeight(0, 0.0f);
            mixerPlayable.SetInputWeight(1, 1.0f);

            clipPlayable0.SetTime(0);
            clipPlayable1.SetTime(0);

            clipPlayable1.Play();
        }
    }

    void OnDisable()
    {
        // Destroys all Playables and Outputs created by the graph.
        playableGraph.Destroy();
    }
}