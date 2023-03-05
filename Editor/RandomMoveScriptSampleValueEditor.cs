#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

namespace PogapogaEditor.Component
{
    
    [CustomEditor(typeof(RandomMoveScriptSampleValue))]
    public class RandomMoveScriptSampleValueEditor : Editor
    {
        RandomMoveScriptSampleValue sampleValue;
        private void OnEnable()
        {
            sampleValue = (RandomMoveScriptSampleValue)target;
            sampleValue.SetGenerator();
        }

        public override void OnInspectorGUI()
        {
            for (int i = 0; i < sampleValue.sampleValues.Count; i++)
            {
                if (GUILayout.Button($"Sample{i + 1}"))
                {
                    if (sampleValue.generator == null)
                    {
                        Debug.LogError("RandomMoveAnimationGeneratorをセットしてください");
                        return;
                    }
                    Undo.RecordObject(sampleValue.generator, "SampleValue");
                    sampleValue.SetSampleSetting(i);
                    Debug.Log("RandomMoveAnimationGeneratorへサンプル値をセット");
                }
            }           
        }
    }
}
#endif