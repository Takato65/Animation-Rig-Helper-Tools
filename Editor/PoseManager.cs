using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using Takato.AnimationRiggingHelperTools.Data;

namespace Takato.AnimationRiggingHelperTools
{
    public class PoseManager : EditorWindow
    {
        private PoseStorage poseStorage;
        private string newPoseName = "New Pose";
        private EPoseApplyMode applyMode = EPoseApplyMode.PositionAndRotation;
        private EPoseApplyBones applyBones = EPoseApplyBones.All;
        private bool colorFoldout = true;

        [MenuItem("Animation Rigging/Takato/Animation Rigging Pose Manager", false, 11, secondaryPriority = 0)]
        public static void ShowWindow()
        {
            GetWindow<PoseManager>("Pose Manager");
        }

        private void OnGUI()
        {
            if (poseStorage == null)
            {
                poseStorage = AssetDatabase.LoadAssetAtPath<PoseStorage>("Assets/PoseStorage.asset");
                if (poseStorage == null)
                {
                    if (GUILayout.Button("Create Pose Storage"))
                    {
                        poseStorage = CreateInstance<PoseStorage>();
                        AssetDatabase.CreateAsset(poseStorage, "Assets/PoseStorage.asset");
                        AssetDatabase.SaveAssets();
                    }
                    return;
                }
            }

            newPoseName = EditorGUILayout.TextField("Pose Name", newPoseName);
            applyMode = (EPoseApplyMode)EditorGUILayout.EnumPopup("Apply Mode", applyMode);
            applyBones = (EPoseApplyBones)EditorGUILayout.EnumPopup("Apply Bones", applyBones);
            colorFoldout = EditorGUILayout.Foldout(colorFoldout, "Preview Settings");
            if (colorFoldout)
            {
                EditorGUI.BeginChangeCheck();
                EditorGUI.indentLevel++;
                poseStorage.humanColor = EditorGUILayout.ColorField("Human Color", poseStorage.humanColor);
                poseStorage.humanSize = EditorGUILayout.Slider("Human Size", poseStorage.humanSize, 0.01f, 0.1f);
                poseStorage.otherColor = EditorGUILayout.ColorField("Other Color", poseStorage.otherColor);
                poseStorage.otherSize = EditorGUILayout.Slider("Other Size", poseStorage.otherSize, 0.01f, 0.1f);
                EditorGUI.indentLevel--;
                if (EditorGUI.EndChangeCheck())
                {
                    EditorUtility.SetDirty(poseStorage);
                    SceneView.RepaintAll();
                }
            }

            if (GUILayout.Button("Save Current Pose"))
            {
                SavePose();
            }

            GUILayout.Space(10);
            GUILayout.Label("Saved Poses", EditorStyles.boldLabel);
            for (int i = 0; i < poseStorage.savedPoses.Count; i++)
            {
                PoseData pose = poseStorage.savedPoses[i];
                GUILayout.BeginHorizontal();
                GUILayout.Label(pose.poseName);
                if (GUILayout.Button("Preview Effected Bones"))
                {
                    poseStorage.previewIndex = i;
                    SceneView.RepaintAll();
                }
                if (GUILayout.Button("Apply"))
                {
                    ApplyPose(pose);
                }
                if (GUILayout.Button("Delete"))
                {
                    poseStorage.savedPoses.RemoveAt(i);
                    poseStorage.previewIndex = -1;
                    AssetDatabase.SaveAssets();
                }
                GUILayout.EndHorizontal();
            }
        }

        //Try Get Human Bone of transform
        private bool TryGetHumanBone(Transform transform, Animator animator, out HumanBodyBones humanBone)
        {
            humanBone = HumanBodyBones.LastBone;
            if (transform == null || animator == null) return false;
            foreach (HumanBodyBones bone in System.Enum.GetValues(typeof(HumanBodyBones)))
            {
                if (bone == HumanBodyBones.LastBone) continue;
                if (animator.GetBoneTransform(bone) == transform)
                {
                    humanBone = bone;
                    return true;
                }
            }
            return false;
        }

        private static Animator GetHumanAnimatorUp(Transform transform)
        {
            if (transform == null) return null;

            Animator animator = transform.GetComponentInParent<Animator>();
            while (animator != null && !animator.isHuman)
            {
                Transform parentTransform = animator.transform.parent;
                if (parentTransform == null) break;

                animator = parentTransform.GetComponentInParent<Animator>();
            }

            return animator != null && animator.isHuman ? animator : null;
        }
        private static Animator GetHumanAnimatorDown(Transform transform)
        {
            if (transform == null) return null;

            Animator[] animators = transform.GetComponentsInChildren<Animator>();
            foreach (Animator animator in animators)
            {
                if (animator.isHuman)
                {
                    return animator;
                }
            }

            return null;
        }

        private static Animator GetHumanAnimator(Transform transform)
        {
            if (transform == null) return null;
            Animator animator = GetHumanAnimatorUp(transform);
            if (animator != null) return animator;
            return GetHumanAnimatorDown(transform);
        }

        private void SavePose()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null) return;
            var uniquePoseName = ObjectNames.GetUniqueName(poseStorage.PoseNames, newPoseName);
            PoseData newPose = new PoseData { poseName = uniquePoseName };

            foreach (Transform child in selected.GetComponentsInChildren<Transform>())
            {
                BoneTransform boneTransform = new BoneTransform
                {
                    boneName = child.name,
                    position = child.localPosition,
                    rotation = child.localRotation
                };
                var animator = GetHumanAnimatorUp(child);
                if (TryGetHumanBone(child, animator, out var bone))
                {
                    boneTransform.humanBodyBone = bone;
                    newPose.HumanBoneTransforms.Add(boneTransform);
                }
                else
                {
                    newPose.otherBoneTransforms.Add(boneTransform);
                }
            }

            poseStorage.savedPoses.Add(newPose);
            AssetDatabase.SaveAssetIfDirty(poseStorage);
            //AssetDatabase.SaveAssets();
        }

        private void ApplyPose(PoseData pose)
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null) return;


            if (applyBones == EPoseApplyBones.Other || applyBones == EPoseApplyBones.All)
            {
                Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
                foreach (Transform child in selected.GetComponentsInChildren<Transform>())
                {
                    boneMap[child.name] = child;
                }
                foreach (BoneTransform bone in pose.otherBoneTransforms)
                {
                    if (boneMap.TryGetValue(bone.boneName, out Transform t))
                    {
                        ApplyBoneTransform(bone, t);
                    }
                }
            }
            if (applyBones == EPoseApplyBones.Humanoid || applyBones == EPoseApplyBones.All)
            {
                var animator = GetHumanAnimator(selected.transform);
                if (animator == null) return;
                foreach (BoneTransform bone in pose.HumanBoneTransforms)
                {
                    if (bone.humanBodyBone == HumanBodyBones.LastBone) continue;

                    var t = animator.GetBoneTransform(bone.humanBodyBone);

                    if (t != null && t.IsChildOf(selected.transform))
                    {
                        ApplyBoneTransform(bone, t);
                    }
                }
            }
        }

        private void ApplyBoneTransform(BoneTransform bone, Transform t)
        {
            if (applyMode == EPoseApplyMode.Position || applyMode == EPoseApplyMode.PositionAndRotation)
            {
                t.localPosition = bone.position;
            }
            if (applyMode == EPoseApplyMode.Rotation || applyMode == EPoseApplyMode.PositionAndRotation)
            {
                t.localRotation = bone.rotation;
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (poseStorage == null || poseStorage.previewIndex == -1 || poseStorage.savedPoses.Count == 0) return;

            
            PoseData previewPose = poseStorage.savedPoses[poseStorage.previewIndex];
            GameObject selected = Selection.activeGameObject;
            if (selected == null) return;



            if (applyBones == EPoseApplyBones.Other || applyBones == EPoseApplyBones.All)
            {
                Handles.color = poseStorage.otherColor;
                Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
                foreach (Transform child in selected.GetComponentsInChildren<Transform>())
                {
                    boneMap[child.name] = child;
                }
                foreach (BoneTransform bone in previewPose.otherBoneTransforms)
                {
                    if (boneMap.TryGetValue(bone.boneName, out Transform t))
                    {
                        Handles.CubeHandleCap(0, t.position, t.rotation, poseStorage.otherSize, EventType.Repaint);
                    }
                }
            }
            if (applyBones == EPoseApplyBones.Humanoid || applyBones == EPoseApplyBones.All)
            {
                Handles.color = poseStorage.humanColor;
                var animator = GetHumanAnimator(selected.transform);
                foreach (BoneTransform bone in previewPose.HumanBoneTransforms)
                {
                    if (bone.humanBodyBone == HumanBodyBones.LastBone) continue;

                    var t = animator.GetBoneTransform(bone.humanBodyBone);
                    if (t != null && t.IsChildOf(selected.transform))
                    {
                        Handles.SphereHandleCap(0, t.position, Quaternion.identity, poseStorage.humanSize, EventType.Repaint);
                    }
                }
            }
/*            Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
            foreach (Transform child in selected.GetComponentsInChildren<Transform>())
            {
                boneMap[child.name] = child;
            }

            foreach (BoneTransform bone in previewPose.otherBoneTransforms)
            {
                if (boneMap.TryGetValue(bone.boneName, out Transform t))
                {
                    Handles.SphereHandleCap(0, t.position + (t.rotation * bone.position), Quaternion.identity, 0.02f, EventType.Repaint);
                }
            }*/
        }

        private void OnEnable()
        {
            SceneView.duringSceneGui += OnSceneGUI;
        }

        private void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGUI;
        }
    }
}
