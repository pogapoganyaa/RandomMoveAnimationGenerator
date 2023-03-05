#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEngine;

namespace PogapogaEditor.Component
{
    public class RandomMoveScriptSampleValue : MonoBehaviour
    {
        public struct SampleValues
        {
            public bool seedValueEnable;
            public int seedValue;
            public float animationTime;
            public float movementTime;
            public bool fluctuationFlag;
            public Vector2 fluctuationTime;
            public bool parallelToRootFlag;
            public bool rotationFlag;
            public float rotationTime;
            public bool rotationFluctuationFlag;
            public Vector2 rotationfluctuationTime;
            public bool waitFlag;
            public float waitTime;
            public bool waitFluctuationFlag;
            public Vector2 waitfluctuationTime;
            public Bounds movePositionBounds;
        }

        public RandomMoveAnimationGenerator generator;

        public List<SampleValues> sampleValues = new List<SampleValues>()
        {
            new SampleValues()
            {
                seedValueEnable = true,
                seedValue = 1,
                animationTime = 10,
                movementTime = 0.5f,
                fluctuationFlag = false,
                fluctuationTime = new Vector2(0, 0),
                parallelToRootFlag = false,
                rotationFlag = true,
                rotationTime = 0.5f,
                rotationFluctuationFlag = false,
                rotationfluctuationTime = new Vector2(0, 0),
                waitFlag = false,
                waitTime = 0.5f,
                waitFluctuationFlag = false,
                waitfluctuationTime = new Vector2(0, 0),
                movePositionBounds = new Bounds()
                {
                    center = new Vector3(0f, 2.25f, 0f),
                    size = new Vector3(5f, 1f, 3f),
                },
            },
            new SampleValues()
            {
                seedValueEnable = true,
                seedValue = 1,
                animationTime = 10,
                movementTime = 0.5f,
                fluctuationFlag = false,
                fluctuationTime = new Vector2(0, 0),
                parallelToRootFlag = true,
                rotationFlag = false,
                rotationTime = 0.5f,
                rotationFluctuationFlag = false,
                rotationfluctuationTime = new Vector2(0, 0),
                waitFlag = true,
                waitTime = 0.5f,
                waitFluctuationFlag = false,
                waitfluctuationTime = new Vector2(0, 0),
                movePositionBounds = new Bounds()
                {
                    center = new Vector3(0f, 2.25f, 0f),
                    size = new Vector3(5f, 1f, 3f),
                },
            },
            new SampleValues()
            {
                seedValueEnable = true,
                seedValue = 1,
                animationTime = 10,
                movementTime = 0.5f,
                fluctuationFlag = true,
                fluctuationTime = new Vector2(-0.2f, 0.1f),
                parallelToRootFlag = false,
                rotationFlag = true,
                rotationTime = 0.5f,
                rotationFluctuationFlag = false,
                rotationfluctuationTime = new Vector2(0, 0),
                waitFlag = true,
                waitTime = 0.5f,
                waitFluctuationFlag = true,
                waitfluctuationTime = new Vector2(-0.2f, 0.1f),
                movePositionBounds = new Bounds()
                {
                    center = new Vector3(0f, 2.25f, 0f),
                    size = new Vector3(5f, 1f, 3f),
                },
            },
            new SampleValues()
            {
                seedValueEnable = true,
                seedValue = 1,
                animationTime = 10,
                movementTime = 0.5f,
                fluctuationFlag = true,
                fluctuationTime = new Vector2(-0.2f, 0.1f),
                parallelToRootFlag = true,
                rotationFlag = false,
                rotationTime = 0.5f,
                rotationFluctuationFlag = false,
                rotationfluctuationTime = new Vector2(0, 0),
                waitFlag = true,
                waitTime = 0.5f,
                waitFluctuationFlag = true,
                waitfluctuationTime = new Vector2(-0.2f, 0.1f),
                movePositionBounds = new Bounds()
                {
                    center = new Vector3(0f, 2.25f, 0f),
                    size = new Vector3(5f, 1f, 3f),
                },
            },
        };
        
        public void SetGenerator()
        {
            generator = this.gameObject.GetComponent<RandomMoveAnimationGenerator>();
        }

        public void SetSampleSetting(int sampleNum)
        {
            generator.seedValueEnable = sampleValues[sampleNum].seedValueEnable;
            generator.seedValue = sampleValues[sampleNum].seedValue;
            generator.animationTime = sampleValues[sampleNum].animationTime;
            generator.movementTime = sampleValues[sampleNum].movementTime;
            generator.fluctuationFlag = sampleValues[sampleNum].fluctuationFlag;
            generator.fluctuationTime = sampleValues[sampleNum].fluctuationTime;
            generator.parallelToRootFlag = sampleValues[sampleNum].parallelToRootFlag;
            generator.rotationFlag = sampleValues[sampleNum].rotationFlag;
            generator.rotationTime = sampleValues[sampleNum].rotationTime;
            generator.rotationFluctuationFlag = sampleValues[sampleNum].rotationFluctuationFlag;
            generator.rotationfluctuationTime = sampleValues[sampleNum].rotationfluctuationTime;
            generator.waitFlag = sampleValues[sampleNum].waitFlag;
            generator.waitTime = sampleValues[sampleNum].waitTime;
            generator.waitfluctuationTime = sampleValues[sampleNum].waitfluctuationTime;
            generator.movePositionBounds = sampleValues[sampleNum].movePositionBounds;
        }
    }
}

#endif