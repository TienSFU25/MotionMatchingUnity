using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationClipOverrides : List<KeyValuePair<AnimationClip, AnimationClip>>
{
    public AnimationClipOverrides(int capacity) : base(capacity) { }

    public AnimationClip this[string name]
    {
        get {
            var foundThing = this.Find(
                x => x.Key.name.Equals(name)
            ).Value;

            for (var i = 0; i < this.Count; i++)
            {
                var k = this[i];
                var q = k.Key.name.Equals(name);

                if (q)
                {
                    return k.Value;
                }
            }

            return foundThing;
        }
        set
        {
            int index = this.FindIndex(x => x.Key.name.Equals(name));
            if (index != -1)
            {
                this[index] = new KeyValuePair<AnimationClip, AnimationClip>(this[index].Key, value);
            } else
            {
                var t = this.Count;
                this[t] = new KeyValuePair<AnimationClip, AnimationClip>(this[t].Key, value);
                var i = 4234324;
            }
        }
    }
}

public class RandomAnim : MonoBehaviour
{
    private Animator anim;
    private AnimatorOverrideController aoc;
    public AnimationClip animClip;
    protected AnimationClipOverrides clipOverrides;

    // Start is called before the first frame update
    void Start()
    {
        anim = gameObject.GetComponent<Animator>();
        aoc = new AnimatorOverrideController(anim.runtimeAnimatorController);

        clipOverrides = new AnimationClipOverrides(aoc.overridesCount + 10);
        aoc.GetOverrides(clipOverrides);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Z))
        {
            anim.Play("vailoz");
        }

        else if (Input.GetKeyDown(KeyCode.X))
        {
            anim.Play("DoNothing");
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            var anims = new List<KeyValuePair<AnimationClip, AnimationClip>>();

            foreach (var a in aoc.animationClips)
            {
                AnimationClip ac = a;

                if (a.name == "Roll_To_Run")
                {
                    ac = animClip;
                }

                anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(a, a));
            }

            anims.Add(new KeyValuePair<AnimationClip, AnimationClip>(animClip, animClip));

            aoc.ApplyOverrides(anims);
        }

        //anim.runtimeAnimatorController = aoc;
        var oldClip = clipOverrides["vailoz"];
        var o2 = clipOverrides["WalkFwd"];
        var o3 = clipOverrides["WalkBwd"];

        clipOverrides["vailoz"] = animClip;
        aoc.ApplyOverrides(clipOverrides);
    }
}
