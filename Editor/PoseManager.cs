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
                if (GUILayout.Button("Preview"))
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

        private void SavePose()
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null) return;

            PoseData newPose = new PoseData { poseName = newPoseName };
            foreach (Transform child in selected.GetComponentsInChildren<Transform>())
            {
                newPose.boneTransforms.Add(new BoneTransform
                {
                    boneName = child.name,
                    position = child.localPosition,
                    rotation = child.localRotation
                });
            }

            poseStorage.savedPoses.Add(newPose);
            AssetDatabase.SaveAssets();
        }

        private void ApplyPose(PoseData pose)
        {
            GameObject selected = Selection.activeGameObject;
            if (selected == null) return;

            Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
            foreach (Transform child in selected.GetComponentsInChildren<Transform>())
            {
                boneMap[child.name] = child;
            }

            foreach (BoneTransform bone in pose.boneTransforms)
            {
                if (boneMap.TryGetValue(bone.boneName, out Transform t))
                {
                    t.localPosition = bone.position;
                    t.localRotation = bone.rotation;
                }
            }
        }

        private void OnSceneGUI(SceneView sceneView)
        {
            if (poseStorage == null || poseStorage.previewIndex == -1 || poseStorage.savedPoses.Count == 0) return;

            Handles.color = new Color(1, 0, 0, 0.5f); // Red transparent color for preview
            PoseData previewPose = poseStorage.savedPoses[poseStorage.previewIndex];
            GameObject selected = Selection.activeGameObject;
            if (selected == null) return;

            Dictionary<string, Transform> boneMap = new Dictionary<string, Transform>();
            foreach (Transform child in selected.GetComponentsInChildren<Transform>())
            {
                boneMap[child.name] = child;
            }

            foreach (BoneTransform bone in previewPose.boneTransforms)
            {
                if (boneMap.TryGetValue(bone.boneName, out Transform t))
                {
                    Handles.SphereHandleCap(0, t.position + (t.rotation * bone.position), Quaternion.identity, 0.02f, EventType.Repaint);
                }
            }
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
