using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Animations;

[RequireComponent(typeof(Animator))]
public class PlayablesAnim : MonoBehaviour
{
    public AnimationClip clipZ;
    public AnimationClip clipX;
    private int i = 0;
    bool initOnce = false;

    PlayableGraph playableGraph;

    void Start()
    {
        //playableGraph = PlayableGraph.Create();
    }

    private void Update()
    {
        AnimationClip clippy = null;

        if (Input.GetKeyDown(KeyCode.Z))
        {
            clippy = clipZ;
        }
        else if (Input.GetKeyDown(KeyCode.X))
        {
            clippy = clipX;
        }

        if (clippy != null)
        {
            if (initOnce)
            {
                playableGraph.Destroy();
            } else
            {
                initOnce = true;
            }

            playableGraph = PlayableGraph.Create();

            var playableOutput = AnimationPlayableOutput.Create(playableGraph, "Animation" + i, GetComponent<Animator>());
            i += 1;

            // Wrap the clip in a playable
            var clipPlayable = AnimationClipPlayable.Create(playableGraph, clippy);

            // IN SECONDS (normalized??)
            clipPlayable.SetTime(0.5);

            // Connect the Playable to an output
            playableOutput.SetSourcePlayable(clipPlayable);

            // Plays the Graph.
            playableGraph.Play();
        }
    }

    void OnDisable()

    {
        // Destroys all Playables and PlayableOutputs created by the graph.
        playableGraph.Destroy();
    }
}