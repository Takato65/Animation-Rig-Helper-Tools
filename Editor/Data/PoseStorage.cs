using System.Collections.Generic;
using UnityEngine;
namespace Takato.AnimationRiggingHelperTools.Data
{
    public class PoseStorage : ScriptableObject
    {
        public Color humanColor = new(255, 0, 0, 60); // Red
        public Color otherColor = new(0, 255, 0, 60); // Green
        public float humanSize = 0.02f;
        public float otherSize = 0.02f;
        public List<PoseData> savedPoses = new List<PoseData>();
        public string[] PoseNames { get { return savedPoses.ConvertAll(pose => pose.poseName).ToArray(); } }
        public int previewIndex = -1;
    }
}
