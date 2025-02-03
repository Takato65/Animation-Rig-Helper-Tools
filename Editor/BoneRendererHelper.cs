using UnityEngine;
using UnityEditor;
using UnityEngine.Animations.Rigging;
using System.Collections.Generic;
using System;

public class BoneRendererUtility
{
    [MenuItem("CONTEXT/BoneRenderer/Replace with only Human Bones")]
    private static void GetHumanBones(MenuCommand command)
    {
        BoneRenderer boneRenderer = (BoneRenderer)command.context;
        Animator animator = boneRenderer.GetComponentInParent<Animator>();
        SetHumanBones(boneRenderer, animator);
        
    }
    public static void BoneRendererSetup(Transform transform)
    {
        if (transform == null)
            return;

        var boneRenderer = transform.GetComponent<BoneRenderer>();
        if (boneRenderer == null)
            boneRenderer = Undo.AddComponent<BoneRenderer>(transform.gameObject);
        else
            Undo.RecordObject(boneRenderer, "Bone renderer setup.");

        var animator = transform.GetComponent<Animator>();
        if (animator == null || animator.avatar == null || !animator.isHuman)
            return; // Exit if not a humanoid rig

        var humanBones = new List<Transform>();

        // Loop through all HumanBodyBones and collect valid ones
        foreach (HumanBodyBones bone in Enum.GetValues(typeof(HumanBodyBones)))
        {
            if (bone == HumanBodyBones.LastBone) // Ignore the LastBone enum entry
                continue;

            var boneTransform = animator.GetBoneTransform(bone);
            if (boneTransform != null && !humanBones.Contains(boneTransform))
                humanBones.Add(boneTransform);
        }

        boneRenderer.transforms = humanBones.ToArray();

        if (PrefabUtility.IsPartOfPrefabInstance(boneRenderer))
            EditorUtility.SetDirty(boneRenderer);
    }

    [MenuItem("Animation Rigging/Takato/Bone Renderer Setup (Humanoid Only)", false, 11,secondaryPriority = 1)]
    static void BoneRendererSetup()
    {
        var selection = Selection.activeTransform;
        if (selection == null)
            return;

        BoneRendererSetup(selection);
    }
    private static void SetHumanBones(BoneRenderer boneRenderer, Animator animator)
    {
        if (animator == null || animator.avatar == null || !animator.avatar.isHuman)
        {
            Debug.LogWarning("No valid humanoid Animator found.");
            return;
        }

        HumanBodyBones[] humanBones = (HumanBodyBones[])System.Enum.GetValues(typeof(HumanBodyBones));
        System.Collections.Generic.List<Transform> boneTransforms = new System.Collections.Generic.List<Transform>();

        foreach (HumanBodyBones bone in humanBones)
        {
            if (bone == HumanBodyBones.LastBone) continue; // Ignore the LastBone enum value
            Transform boneTransform = animator.GetBoneTransform(bone);
            if (boneTransform != null)
            {
                boneTransforms.Add(boneTransform);
            }
        }

        boneRenderer.transforms = boneTransforms.ToArray();
        EditorUtility.SetDirty(boneRenderer);
        Debug.Log("Assigned " + boneTransforms.Count + " human bones to BoneRenderer.");
    }
    
}
