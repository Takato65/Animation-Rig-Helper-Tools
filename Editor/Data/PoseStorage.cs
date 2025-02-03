using UnityEngine;
using System.Collections.Generic;
namespace Takato.AnimationRiggingHelperTools.Data
{
    public class PoseStorage : ScriptableObject
    {
        public List<PoseData> savedPoses = new List<PoseData>();
        public int previewIndex = -1;
    }
}
