#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PogapogaEditor.Component
{

    public class RandomMoveAnimationGenerator : MonoBehaviour
    {
        // 検索用
        public string targetTag = "Untagged";
        public string targetName = "○○";

        // Seed値
        [Tooltip("Seed値")] public int seedValue = 1;
        [Tooltip("Seed値の利用")] public bool seedValueEnable = true;

        // 時間設定
        [Tooltip("アニメーション時間")] public float animationTime = 10f;
        [Tooltip("移動時間")] public float movementTime = 0.5f;
        [Tooltip("ランダムにずらす")] public bool fluctuationFlag = false;
        [Tooltip("移動時間のゆらぎ")] public Vector2 fluctuationTime = new Vector2(0f, 0f);

        [Tooltip("RootObjectと同じ方向を向かせる")] public bool parallelToRootFlag = false;

        [Tooltip("回転フラグ")] public bool rotationFlag = true;
        [Tooltip("回転時間（秒）")] public float rotationTime = 0.5f;
        [Tooltip("ランダムにずらす")] public bool rotationFluctuationFlag = true;
        [Tooltip("回転時間のゆらぎ")] public Vector2 rotationfluctuationTime = new Vector2(0f, 0f);

        [Tooltip("静止フラグ")] public bool waitFlag = false;
        [Tooltip("静止時間")] public float waitTime = 0.5f;
        [Tooltip("ランダムにずらす")] public bool waitFluctuationFlag = true;
        [Tooltip("静止時間のゆらぎ")] public Vector2 waitfluctuationTime = new Vector2(0f, 0f);

        // 必須項目
        [Tooltip("設定対象のAnimation")] public AnimationClip targetAnimationClip;
        [Tooltip("RootのObject")] public GameObject animationRootObject;
        [Tooltip("移動対象のGameObject")] public List<GameObject> targetObjectList = new List<GameObject>();

        // 移動範囲
        public Vector3 moveAreaCenterPosition;
        public Color movePositionBoundsColor = new Color(0, 255, 0, 0.5f);
        public Bounds movePositionBounds = new Bounds()
        {
            center = new Vector3(0f, 2.25f, 0f),
            size = new Vector3(5f, 1f, 3f),
        };

        // 時間の格納用
        const int TimeListCapacity = 3;
        List<float> _moveTimeList = new List<float>();
        List<float> _rotationStartTimeList = new List<float>();
        List<float> _rotationEndTimeList = new List<float>();
        List<float> _waitStartTimeList = new List<float>();
        List<float> _waitEndTimeList = new List<float>();

        #region // Object取得用関数
        public void SearchTagObject()
        {
            targetObjectList = animationRootObject.GetComponentsInChildren<Transform>(true)
                        .Where(t => t.gameObject.tag == targetTag)
                        .Select(t => t.gameObject)
                        .ToList();
        }
        public void SearchNameObject()
        {
            targetObjectList = animationRootObject.GetComponentsInChildren<Transform>(true)
                        .Where(t => t.gameObject.name.Contains(targetName))
                        .Select(t => t.gameObject)
                        .ToList();
        }
        #endregion

        /// <summary>
        /// 移動範囲のScene上での表示
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (animationRootObject == null) { return; }
            moveAreaCenterPosition = movePositionBounds.center + animationRootObject.transform.position;
            Gizmos.color = movePositionBoundsColor;
            Gizmos.DrawCube(moveAreaCenterPosition, movePositionBounds.size);
        }

        /// <summary>
        /// AnimationClipへランダムな動きを登録する
        /// </summary>
        /// <param name="animationClip">設定対象のAnimationClip</param>
        /// <returns></returns>
        public AnimationClip GenerateRandomMoveAnimation(AnimationClip animationClip)
        {
            #region // 変数宣言
            bool singleAnimation;
            float animationTargetTime;
            List<Vector3> positionList;
            Vector3 firstPosition;
            Vector3 secondPosition;
            Quaternion moveRotation;
            Keyframe[] keyframesPosition = new Keyframe[3];
            for (int num = 0; num < keyframesPosition.Length; num++) { keyframesPosition[num] = new Keyframe(); }
            Keyframe[] keyframesRotation = new Keyframe[4];
            for (int num = 0; num < keyframesRotation.Length; num++) { keyframesRotation[num] = new Keyframe(); }
            #endregion

            #region // 乱数のSeed値の設定
            if (seedValueEnable == true) { Random.InitState(seedValue); }
            #endregion

            #region // Objectごとの処理
            for (int objectNum = 0; objectNum < targetObjectList.Count; objectNum++)
            {
                if (targetObjectList[objectNum] == null) { continue; }
                Transform objectTransform = targetObjectList[objectNum].transform;
                #region //初期化
                singleAnimation = false;
                positionList = new List<Vector3>();
                animationTargetTime = 0;
                AnimationCurve[] curvePosition = new AnimationCurve[3];
                AnimationCurve[] curveRotation = new AnimationCurve[4];
                for (int num = 0; num < curvePosition.Length; num++) { curvePosition[num] = new AnimationCurve(); }
                for (int num = 0; num < curveRotation.Length; num++) { curveRotation[num] = new AnimationCurve(); }

                _moveTimeList.Clear();
                _rotationStartTimeList.Clear();
                _rotationEndTimeList.Clear();
                _waitStartTimeList.Clear();
                _waitEndTimeList.Clear();
                #endregion

                //アニメーション用パスの取得
                string hierarchyPath = GetHierarchyPath(targetObjectList[objectNum], animationRootObject);
                
                // Positionの決定
                positionList.Add(RandomPosition());
                positionList[positionList.Count - 1] = objectTransform.InverseTransformPoint(positionList[positionList.Count - 1]);
                positionList.Add(RandomPosition());
                positionList[positionList.Count - 1] = objectTransform.InverseTransformPoint(positionList[positionList.Count - 1]);
                firstPosition = positionList[0]; // 最後に最初のポジションに戻すため
                secondPosition = positionList[1];
                

                // 0フレームのポジション
                SetPositionKeyframes(ref keyframesPosition, positionList[0], animationTargetTime);
                for (int num = 0; num < curvePosition.Length; num++) { curvePosition[num].AddKey(keyframesPosition[num]); }
                // 0フレームのローテーション
                moveRotation = NextLookRotation(positionList[1], positionList[0]);

                if (parallelToRootFlag == true)
                {
                    // RootObjectの向きにあわせる
                    Quaternion parallelDirection = Quaternion.Euler(animationRootObject.transform.rotation.eulerAngles - objectTransform.rotation.eulerAngles);
                    SetRotationKeyframes(ref keyframesRotation, parallelDirection, animationTargetTime);
                    for (int num = 0; num < curveRotation.Length; num++) { curveRotation[num].AddKey(keyframesRotation[num]); }
                }
                else
                {
                    SetRotationKeyframes(ref keyframesRotation, moveRotation, animationTargetTime);
                    for (int num = 0; num < curveRotation.Length; num++) { curveRotation[num].AddKey(keyframesRotation[num]); }
                }

                // 待機
                if (waitFlag == true && waitFluctuationFlag == true) { animationTargetTime = Random.Range(waitfluctuationTime.x, waitfluctuationTime.y); } 
                if (animationTargetTime > 0)
                {
                    SetPositionKeyframes(ref keyframesPosition, positionList[0], animationTargetTime);
                    for (int num = 0; num < curvePosition.Length; num++) { curvePosition[num].AddKey(keyframesPosition[num]); }
                    if (rotationFlag == true)
                    {
                        moveRotation = NextLookRotation(positionList[1], positionList[0]);
                        SetRotationKeyframes(ref keyframesRotation, moveRotation, animationTargetTime);
                        for (int num = 0; num < curveRotation.Length; num++) { curveRotation[num].AddKey(keyframesRotation[num]); }
                    }
                }

                // 次回の移動先Positionの決定
                positionList.RemoveAt(0);
                positionList.Add(RandomPosition());
                positionList[positionList.Count - 1] = objectTransform.InverseTransformPoint(positionList[positionList.Count - 1]);
                
                animationTargetTime = CalculationTime(animationTargetTime);
                animationTargetTime = CalculationTime(animationTargetTime);


                // すでにAnimationTimeを超えている場合 
                if (animationTargetTime >= animationTime)
                {
                    singleAnimation = true;
                }

                // 移動Animationの作成
                float lastAnimationTargetTime = animationTargetTime;
                float loopCount = (movementTime == 0) ? animationTime : animationTime / movementTime;
                for (int moveCount = 0; moveCount < loopCount && singleAnimation == false; moveCount++)
                {
                    bool lastLoopFlag = false;

                    // 時間計算処理
                    animationTargetTime = CalculationTime(animationTargetTime);

                    if (animationTargetTime > animationTime)
                    {
                        lastLoopFlag = true;
                    }
                    if (lastAnimationTargetTime == animationTargetTime)
                    {
                        break; // 無限ループ対策
                    }

                    // Positionを動かす
                    SetPositionKeyframes(ref keyframesPosition, positionList[0], _moveTimeList[0]);
                    for (int num = 0; num < curvePosition.Length; num++) { curvePosition[num].AddKey(keyframesPosition[num]); }

                    #region // 回転処理
                    if (rotationFlag == true)
                    {
                        SetPositionKeyframes(ref keyframesPosition, positionList[0], _rotationStartTimeList[0]);
                        for (int num = 0; num < curvePosition.Length; num++) { curvePosition[num].AddKey(keyframesPosition[num]); }
                        SetRotationKeyframes(ref keyframesRotation, moveRotation, _rotationStartTimeList[0]);
                        for (int num = 0; num < curveRotation.Length; num++) { curveRotation[num].AddKey(keyframesRotation[num]); }

                        // 次のポジションへ回転させる
                        if (lastLoopFlag == false)
                        {
                            moveRotation = NextLookRotation(positionList[1], positionList[0]);
                        }
                        else
                        {
                            moveRotation = NextLookRotation(firstPosition, positionList[0]);
                        }                       
                        SetPositionKeyframes(ref keyframesPosition, positionList[0], _rotationEndTimeList[0]);
                        for (int num = 0; num < curvePosition.Length; num++) { curvePosition[num].AddKey(keyframesPosition[num]); } 
                        SetRotationKeyframes(ref keyframesRotation, moveRotation, _rotationEndTimeList[0]);
                        for (int num = 0; num < curveRotation.Length; num++) { curveRotation[num].AddKey(keyframesRotation[num]); }
                    }
                    #endregion

                    #region // 静止時間
                    if (waitFlag == true)
                    {
                        SetPositionKeyframes(ref keyframesPosition, positionList[0], _waitStartTimeList[0]);
                        for (int num = 0; num < curvePosition.Length; num++) { curvePosition[num].AddKey(keyframesPosition[num]); }

                        SetPositionKeyframes(ref keyframesPosition, positionList[0], _waitEndTimeList[0]);
                        for (int num = 0; num < curvePosition.Length; num++) { curvePosition[num].AddKey(keyframesPosition[num]); }
                    }
                    #endregion
                    lastAnimationTargetTime = animationTargetTime;

                    // ループを繰り返すか残り時間の判定
                    if (lastLoopFlag == true) { break; }

                    // 次回の移動先Positionの決定
                    positionList.RemoveAt(0);
                    positionList.Add(RandomPosition());
                    positionList[positionList.Count - 1] = objectTransform.InverseTransformPoint(positionList[positionList.Count - 1]);
                }
                #region // スタート地点へ
                // ポジション処理
                SetPositionKeyframes(ref keyframesPosition, firstPosition, _moveTimeList[1]);
                for (int num = 0; num < curvePosition.Length; num++) { curvePosition[num].AddKey(keyframesPosition[num]); }
                // 回転処理
                if (rotationFlag == true)
                {
                    //
                    SetPositionKeyframes(ref keyframesPosition, firstPosition, _rotationStartTimeList[1]);
                    for (int num = 0; num < curvePosition.Length; num++) { curvePosition[num].AddKey(keyframesPosition[num]); }
                    SetRotationKeyframes(ref keyframesRotation, moveRotation, _rotationStartTimeList[1]);
                    for (int num = 0; num < curveRotation.Length; num++) { curveRotation[num].AddKey(keyframesRotation[num]); }

                    // 次のポジションへ回転させる
                    moveRotation = NextLookRotation(secondPosition, firstPosition);

                    SetPositionKeyframes(ref keyframesPosition, firstPosition, _rotationEndTimeList[1]);
                    for (int num = 0; num < curvePosition.Length; num++) { curvePosition[num].AddKey(keyframesPosition[num]); }
                    SetRotationKeyframes(ref keyframesRotation, moveRotation, _rotationEndTimeList[1]);
                    for (int num = 0; num < curveRotation.Length; num++) { curveRotation[num].AddKey(keyframesRotation[num]); }
                }
                if (waitFlag == true)
                {
                    SetPositionKeyframes(ref keyframesPosition, firstPosition, _waitStartTimeList[1]);
                    for (int num = 0; num < curvePosition.Length; num++) { curvePosition[num].AddKey(keyframesPosition[num]); }
                    SetPositionKeyframes(ref keyframesPosition, firstPosition, _waitEndTimeList[1]);
                    for (int num = 0; num < curvePosition.Length; num++) { curvePosition[num].AddKey(keyframesPosition[num]); }
                }
                #endregion

                #region //アニメーションへの登録
                SetAnimationCurveToClip(animationClip, hierarchyPath, curvePosition, curveRotation);
                #endregion
            }
            return animationClip;
            #endregion
        }

        private void SetAnimationCurveToClip(AnimationClip animationClip, string hierarchyPath, AnimationCurve[] positionAnimationCurves, AnimationCurve[] rotationAnimationCurves)
        {
            #region //アニメーションへの登録
            animationClip.SetCurve(hierarchyPath, typeof(Transform), "m_LocalPosition.x", positionAnimationCurves[0]);
            animationClip.SetCurve(hierarchyPath, typeof(Transform), "m_LocalPosition.y", positionAnimationCurves[1]);
            animationClip.SetCurve(hierarchyPath, typeof(Transform), "m_LocalPosition.z", positionAnimationCurves[2]);

            if (rotationFlag == true || parallelToRootFlag == true)
            {
                animationClip.SetCurve(hierarchyPath, typeof(Transform), "m_LocalRotation.x", rotationAnimationCurves[0]);
                animationClip.SetCurve(hierarchyPath, typeof(Transform), "m_LocalRotation.y", rotationAnimationCurves[1]);
                animationClip.SetCurve(hierarchyPath, typeof(Transform), "m_LocalRotation.z", rotationAnimationCurves[2]);
                animationClip.SetCurve(hierarchyPath, typeof(Transform), "m_LocalRotation.w", rotationAnimationCurves[3]);
            }
            #endregion
        }

        private float CalculationTime(float animationTargetTime)
        {
            // 時間計算処理
            animationTargetTime += movementTime;

            // 移動時間のゆらぎ
            if (fluctuationFlag == true)
            {
                animationTargetTime += Random.Range(fluctuationTime.x, fluctuationTime.y);
            }
            _moveTimeList.Add(animationTargetTime);
            if (_moveTimeList.Count > TimeListCapacity) { _moveTimeList.RemoveAt(0); }
            if (rotationFlag == true)
            {
                _rotationStartTimeList.Add(animationTargetTime);
                animationTargetTime += rotationTime;
                if (rotationFluctuationFlag == true)
                {
                    animationTargetTime += Random.Range(rotationfluctuationTime.x, rotationfluctuationTime.y);
                }
                _rotationEndTimeList.Add(animationTargetTime);
                if (_rotationStartTimeList.Count > TimeListCapacity) { _rotationStartTimeList.RemoveAt(0); }
                if (_rotationEndTimeList.Count > TimeListCapacity) { _rotationEndTimeList.RemoveAt(0); }
            }
            if (waitFlag == true)
            {
                _waitStartTimeList.Add(animationTargetTime);
                animationTargetTime += waitTime;
                if (waitFluctuationFlag == true)
                {
                    animationTargetTime += Random.Range(waitfluctuationTime.x, waitfluctuationTime.y);
                }               
                _waitEndTimeList.Add(animationTargetTime);
                if (_waitStartTimeList.Count > TimeListCapacity) { _waitStartTimeList.RemoveAt(0); }
                if (_waitEndTimeList.Count > TimeListCapacity) { _waitEndTimeList.RemoveAt(0); }
            }

            return animationTargetTime;
        }

        #region // Keyframe
        private void SetPositionKeyframes(ref Keyframe[] keyframes, Vector3 position, float keyFrameNum)
        {
            keyframes[0] = new Keyframe(keyFrameNum, position.x);
            keyframes[1] = new Keyframe(keyFrameNum, position.y);
            keyframes[2] = new Keyframe(keyFrameNum, position.z);
        }

        private void SetRotationKeyframes(ref Keyframe[] keyframes, Quaternion rotation, float keyFrameNum)
        {
            keyframes[0] = new Keyframe(keyFrameNum, rotation.x);
            keyframes[1] = new Keyframe(keyFrameNum, rotation.y);
            keyframes[2] = new Keyframe(keyFrameNum, rotation.z);
            keyframes[3] = new Keyframe(keyFrameNum, rotation.w);
        }
        #endregion

        #region // Position,Rotation
        private Vector3 RandomPosition()
        {
            Vector3 resultPosition = new Vector3
            {
                x = Random.Range(movePositionBounds.min.x, movePositionBounds.max.x),
                y = Random.Range(movePositionBounds.min.y, movePositionBounds.max.y),
                z = Random.Range(movePositionBounds.min.z, movePositionBounds.max.z),
            };
            return resultPosition;
        }

        private Quaternion NextLookRotation(Vector3 nextPosition, Vector3 currentPosition)
        {
            // ターゲットへの向きベクトル計算
            Vector3 direction = nextPosition - currentPosition;
            // ターゲットの方向への回転
            Quaternion lookAtRotation = Quaternion.LookRotation(direction, Vector3.up);
            return lookAtRotation;
        }
        #endregion

        #region // 親子関係の確認
        private bool CheckParentChildRelationship(GameObject parentObject, GameObject childObject)
        {
            GameObject tmpObject = childObject;
            while (tmpObject.transform.parent != null)
            {
                if (parentObject.transform == tmpObject.transform.parent)
                {
                    return true;
                }
                tmpObject = tmpObject.transform.parent.gameObject;
            }
            return false;
        }

        public string CheckParentChildRelationship(GameObject parentObject, List<GameObject> childObjects)
        {
            string resultText = "";
            for (int i = 0; i < childObjects.Count; i++)
            {
                if (childObjects[i] == null) { continue; }
                if (CheckParentChildRelationship(animationRootObject, childObjects[i]) == false)
                {
                    resultText += childObjects[i].name + System.Environment.NewLine;
                }
            }
            return resultText;
        }
        #endregion

        #region
        private string GetHierarchyPath(GameObject targetObject, GameObject rootObject)
        {
            string resultPath = targetObject.name;
            Transform parentTransform = targetObject.transform.parent;

            if (targetObject == rootObject)
            {
                return "";
            }

            // parentのObjectがなくなるまでループする
            while (parentTransform != null)
            {
                // rootObjectまでの到達
                if (parentTransform == rootObject.transform)
                {
                    break;
                }

                // pathとparentの更新
                resultPath = $"{parentTransform.name}/{resultPath}";
                parentTransform = parentTransform.parent;
            }
            return resultPath;
        }
        #endregion
    }
}
#endif