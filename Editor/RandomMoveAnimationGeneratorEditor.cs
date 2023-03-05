#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace PogapogaEditor.Component
{
    [CustomEditor(typeof(RandomMoveAnimationGenerator))]
    public class RandomMoveAnimationGeneratorEditor : Editor
    {
        private SerializedProperty targetTag;
        private RandomMoveAnimationGenerator generator;
        private BoxBoundsHandle boundsHandle;

        private bool _animationIsOpen = true;
        private bool _searchIsOpen = true;

        private const float MaxAnimationTime = 3600f;
        private const float LimitedRange = 1000f;
        //private bool debugFlag = false;

        private void OnEnable()
        {
            targetTag = serializedObject.FindProperty(nameof(targetTag));
            generator = (RandomMoveAnimationGenerator)target;
            boundsHandle = new BoxBoundsHandle();
        }

        public override void OnInspectorGUI()
        {
            //debugFlag = EditorGUILayout.Toggle("DebugMode", debugFlag);
            //if (debugFlag) { DrawDefaultInspector(); }

            bool setuplockFlag = false;

            serializedObject.Update();
            EditorGUI.BeginChangeCheck();

            #region // 必須項目のチェック
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            if (RequiredObject(generator.targetAnimationClip, "空のAnimationClip") == false) { setuplockFlag = true; };
            EditorObjectField<AnimationClip>("Animationを設定するClip", ref generator.targetAnimationClip);

            if (RequiredObject(generator.animationRootObject, "AnimationのRootObject") == false) { setuplockFlag = true; };
            EditorObjectField<GameObject>("AnimationのRootObject", ref generator.animationRootObject);

            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            #endregion
            #region // targetObjectList
            bool searchIsOpen = EditorGUILayout.Foldout(_searchIsOpen, "GameObjectを検索してListに格納する");
            if (searchIsOpen != _searchIsOpen)
            {
                _searchIsOpen = searchIsOpen;
            }
            if (searchIsOpen == true)
            {
                using (new EditorGUI.DisabledScope(generator.animationRootObject == null))
                {
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        targetTag.stringValue = EditorGUILayout.TagField("検索対象のTag", targetTag.stringValue);
                        if (GUILayout.Button($"Tagが{generator.targetTag}のObjectを取得する"))
                        {
                            Undo.RecordObject(generator, "Objectの検索");
                            generator.SearchTagObject();
                        }
                    }
                    using (new EditorGUILayout.HorizontalScope())
                    {
                        generator.targetName = EditorGUILayout.TextField("検索対象の文字列", generator.targetName);
                        if (GUILayout.Button($"名前に{generator.targetName}を含むObjectを取得する"))
                        {
                            Undo.RecordObject(generator, "Objectの検索");
                            generator.SearchNameObject();
                        }
                    }
                }
            }
            GUILayout.Label("移動対象のGameObject");
            SerializedProperty targetProperty = serializedObject.FindProperty(nameof(generator.targetObjectList));            
            if (generator.targetObjectList.Count == 0)
            {
                EditorGUILayout.HelpBox($"TargetObjectListを設定してください", MessageType.Warning);
                setuplockFlag = true;
            }
            EditorGUILayout.PropertyField(targetProperty);
            #endregion

            #region // Animationの設定
            bool animationIsOpen = EditorGUILayout.Foldout(_animationIsOpen, "Animationの設定");
            if (animationIsOpen != _animationIsOpen)
            {
                _animationIsOpen = animationIsOpen;
            }
            if (animationIsOpen == true)
            {
                EditorGUILayout.LabelField("移動設定");
                EditorGUI.indentLevel++;
                generator.animationTime = EditorGUILayout.FloatField("Animation時間(秒)", Mathf.Clamp(generator.animationTime, 0, MaxAnimationTime));
                generator.movementTime = EditorGUILayout.FloatField("移動時間の間隔(秒)", Mathf.Clamp(generator.movementTime, 0, generator.animationTime));
                generator.fluctuationFlag = EditorGUILayout.Toggle("移動時間の間隔にゆらぎを与える", generator.fluctuationFlag);
                if (generator.fluctuationFlag == true)
                {
                    generator.fluctuationTime = EditorGUILayout.Vector2Field("ゆらぎ間隔", 
                        new Vector2(Mathf.Clamp(generator.fluctuationTime.x, generator.movementTime * -1, generator.fluctuationTime.y),
                        Mathf.Clamp(generator.fluctuationTime.y, generator.fluctuationTime.x, generator.animationTime)));
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.LabelField("回転設定");
                EditorGUI.indentLevel++;  
                bool _parallelToRootFlag = EditorGUILayout.Toggle("RootObjectにあわせる", generator.parallelToRootFlag);
                bool _rotationFlag = EditorGUILayout.Toggle("次の着地点を見る", generator.rotationFlag);
                if (_parallelToRootFlag != generator.parallelToRootFlag) 
                { 
                    generator.parallelToRootFlag = _parallelToRootFlag;
                    if (generator.parallelToRootFlag == true) { generator.rotationFlag = false; }
                }
                else if (_rotationFlag != generator.rotationFlag)
                {
                    generator.rotationFlag = _rotationFlag;
                    if (generator.rotationFlag == true) { generator.parallelToRootFlag = false; }
                }

                if (generator.rotationFlag == true)
                {
                    generator.rotationTime = EditorGUILayout.FloatField("回転時間(秒)", Mathf.Clamp(generator.rotationTime, 0, generator.animationTime));
                    generator.rotationFluctuationFlag = EditorGUILayout.Toggle("回転時間の間隔にゆらぎを与える", generator.rotationFluctuationFlag);
                    if (generator.rotationFluctuationFlag == true)
                    {
                        generator.rotationfluctuationTime =
                            EditorGUILayout.Vector2Field("静止時間のゆらぎ間隔",
                            new Vector2(Mathf.Clamp(generator.rotationfluctuationTime.x, generator.rotationTime * -1, generator.rotationfluctuationTime.y),
                            Mathf.Clamp(generator.rotationfluctuationTime.y, generator.rotationfluctuationTime.x, generator.animationTime)));
                    }

                }
                EditorGUI.indentLevel--;

                EditorGUILayout.LabelField("静止設定");
                EditorGUI.indentLevel++;
                generator.waitFlag = EditorGUILayout.Toggle("静止設定", generator.waitFlag);
                if (generator.waitFlag == true)
                {
                    generator.waitTime = EditorGUILayout.FloatField("静止時間(秒)", Mathf.Clamp(generator.waitTime, 0, generator.animationTime));
                    generator.waitFluctuationFlag = EditorGUILayout.Toggle("静止時間の間隔にゆらぎを与える", generator.waitFluctuationFlag);
                    if (generator.waitFluctuationFlag == true)
                    {
                        generator.waitfluctuationTime =
                            EditorGUILayout.Vector2Field("静止時間のゆらぎ間隔",
                            new Vector2(Mathf.Clamp(generator.waitfluctuationTime.x, generator.waitTime * -1, generator.waitfluctuationTime.y),
                            Mathf.Clamp(generator.waitfluctuationTime.y, generator.waitfluctuationTime.x, generator.animationTime)));
                    }
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.LabelField("Seed値設定");
                EditorGUI.indentLevel++;
                generator.seedValueEnable = EditorGUILayout.Toggle("Seed値の利用", generator.seedValueEnable);
                if (generator.seedValueEnable == true)
                {
                    generator.seedValue = EditorGUILayout.IntField("Seed値の設定", generator.seedValue);
                }
                EditorGUI.indentLevel--;

                EditorGUILayout.LabelField("移動範囲の設定");
                EditorGUI.indentLevel++;
                generator.movePositionBounds = EditorGUILayout.BoundsField(generator.movePositionBounds);
                // 最大値・最小値の確認
                Vector3 limitedCenter = generator.movePositionBounds.center;
                limitedCenter.x = Mathf.Clamp(limitedCenter.x, LimitedRange * -1, LimitedRange);
                limitedCenter.y = Mathf.Clamp(limitedCenter.y, LimitedRange * -1, LimitedRange);
                limitedCenter.z = Mathf.Clamp(limitedCenter.z, LimitedRange * -1, LimitedRange);
                generator.movePositionBounds.center = limitedCenter;
                Vector3 limitedExtents = generator.movePositionBounds.extents;
                limitedExtents.x = Mathf.Clamp(limitedExtents.x, LimitedRange * -1, LimitedRange);
                limitedExtents.y = Mathf.Clamp(limitedExtents.y, LimitedRange * -1, LimitedRange);
                limitedExtents.z = Mathf.Clamp(limitedExtents.z, LimitedRange * -1, LimitedRange);
                generator.movePositionBounds.extents = limitedExtents;
                EditorGUI.indentLevel--;
            }
            #endregion
            GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.Height(1));
            EditorGUILayout.Space();

            #region // Animationの作成ボタン
            using (new EditorGUI.DisabledScope(setuplockFlag))
            {
                if (GUILayout.Button("Animationの作成"))
                {

                    // Animationの再生中の確認
                    if (AnimationMode.InAnimationMode())
                    {
                        EditorUtility.DisplayDialog("Animationclipの確認", $"AnimationClipが再生中または編集中です{System.Environment.NewLine}Animationを停止してから実行してください", "OK");
                        AnimationMode.StopAnimationMode();
                        return;
                    }
                        
                    // 空アニメーションではないとき
                    if (generator.targetAnimationClip.empty == false)
                    {
                        bool initialization = EditorUtility.DisplayDialog("Animationclipの確認",
                            $"Animationclipが空ではありません{System.Environment.NewLine}初期化してよろしいですか？", "OK", "Cancel");
                        if (initialization == true)
                        {
                            Undo.RecordObject(generator.targetAnimationClip, "AnimationClipの初期化");
                            generator.targetAnimationClip.ClearCurves();
                        }
                        else
                        {
                            return;
                        }
                    }
                    // 親子関係の確認
                    string checkText = generator.CheckParentChildRelationship(generator.animationRootObject, generator.targetObjectList);
                    if (checkText != "")
                    {
                        EditorUtility.DisplayDialog("TargetObjectListの確認",
                            $"次のObjectはRootObjectに含まれていません{System.Environment.NewLine}" +
                            $"親子関係を確認してください{System.Environment.NewLine}" +
                            $"{checkText}", "OK");
                        return;
                    }
                    // 有効なObjectの確認
                    for (int i = 0; i < generator.targetObjectList.Count; i++)
                    {
                        if (generator.targetObjectList[i] != null)
                        {
                            break;
                        }
                        if (i == generator.targetObjectList.Count - 1)
                        {
                            EditorUtility.DisplayDialog("TargetObjectListの確認",
                            $"TargetObjectListに有効なObjectが設定されていません{System.Environment.NewLine}" +
                            $"移動対象のObjectを設定してください{System.Environment.NewLine}" +
                            $"{checkText}", "OK");
                            return;
                        }
                    }
                    Undo.RecordObject(generator.targetAnimationClip, "Animationの作成");
                    generator.GenerateRandomMoveAnimation(generator.targetAnimationClip);
                    EditorUtility.DisplayDialog("Animationclipの確認", "Animationの作成が完了しました", "OK");
                    Debug.Log("Animationの作成完了");
                }
            }
            #endregion
            if (EditorGUI.EndChangeCheck())
            {
                EditorUtility.SetDirty(generator);
                serializedObject.ApplyModifiedProperties();
            }   
        }

        /// <summary>
        /// Scene上での表示
        /// </summary>
        private void OnSceneGUI()
        {
            if (generator.animationRootObject == null) { return; }

            serializedObject.Update();
            Bounds collider = generator.movePositionBounds;
            Tool currentTool = Tools.current;

            // 現在のツールモードにあわせる
            if (currentTool == Tool.Scale)
            {
                generator.movePositionBounds.size = Handles.ScaleHandle(collider.size, collider.center, Quaternion.identity, HandleUtility.GetHandleSize(collider.center));
            }
            if (currentTool == Tool.Move)
            {
                generator.movePositionBounds.center = Handles.PositionHandle(collider.center + generator.animationRootObject.transform.position, Quaternion.identity) - generator.animationRootObject.transform.position;
            }

            //BoxHandle
            boundsHandle.center = generator.movePositionBounds.center + generator.animationRootObject.transform.position;
            boundsHandle.size = generator.movePositionBounds.size;

            using (EditorGUI.ChangeCheckScope check = new EditorGUI.ChangeCheckScope())
            {
                boundsHandle.DrawHandle();

                if (check.changed)
                {
                    generator.movePositionBounds.center = boundsHandle.center - generator.animationRootObject.transform.position;
                    generator.movePositionBounds.size = boundsHandle.size;
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private bool RequiredObject(Object targetObject, string settingMessage)
        {
            if (targetObject == null)
            {
                EditorGUILayout.HelpBox($"{settingMessage}を設定してください", MessageType.Warning);
                return false;
            }
            return true;
        }

        private void EditorObjectField<T>(string objectText, ref T targetObject) where T : Object
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                targetObject = EditorGUILayout.ObjectField(objectText, targetObject, typeof(T), true) as T;
            }
        }
    }
}
#endif