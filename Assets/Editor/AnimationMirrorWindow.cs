using PlasticGui.Configuration.CloudEdition.Welcome;
using PlasticPipe.PlasticProtocol.Messages;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;
//using UnityEngine.Animations;
//using UnityEngine.Playables;

public class AnimationMirrorWindow : EditorWindow
{
    ObjectField tpose;
    ObjectField originAnimation;
    ObjectField destinyAnimation;


    [MenuItem("PurpleTree/Animation Editor")]
    public static void ShowExample()
    {
        AnimationMirrorWindow wnd = GetWindow<AnimationMirrorWindow>();
        wnd.titleContent = new GUIContent("Animation Editor");
    }

    public void CreateGUI()
    {
        VisualElement root = rootVisualElement;

        Label label = new Label("");
        root.Add(label);

        Label label2 = new Label("Invertir Animacion");
        StyleEnum<TextAnchor> align = new StyleEnum<TextAnchor>();
        align.value = TextAnchor.MiddleCenter;
        label2.style.unityTextAlign = align;
        root.Add(label2);

        Label label3 = new Label("");
        root.Add(label3);

        originAnimation = new ObjectField();
        originAnimation.objectType = typeof(AnimationClip);
        originAnimation.name = "animacionOrigen";
        originAnimation.label = "Animación origen";
        root.Add(originAnimation);

        destinyAnimation = new ObjectField();
        destinyAnimation.objectType = typeof(AnimationClip);
        destinyAnimation.name = "animacionDestino";
        destinyAnimation.label = "Animación destino";
        root.Add(destinyAnimation);

        tpose = new ObjectField();
        tpose.objectType = typeof(GameObject);
        tpose.RegisterValueChangedCallback((obj) => {
            if (PrefabUtility.GetPrefabAssetType(obj.newValue) == PrefabAssetType.Regular)
            {
                if (PrefabUtility.IsPartOfPrefabInstance(obj.newValue))
                    tpose.value = PrefabUtility.GetOutermostPrefabInstanceRoot(obj.newValue);
                else
                    tpose.value = obj.newValue;
            }
            else
                tpose.value = null;
        });
        tpose.name = "tpose";
        tpose.label = "Prefab T-Pose";
        root.Add(tpose);

        Button button = new Button();
        button.name = "invertir";
        button.text = "Invertir";
        button.clicked += InvertAnimation;
        root.Add(button);

    }



    public void SetKeysValue(ref AnimationCurve destinyCurve, AnimationCurve originCurve, int signo) 
    {
            for (int k = 0; k < originCurve.keys.Length; k++)
            {
                destinyCurve.AddKey(originCurve.keys[k].time, originCurve.keys[k].value * signo);
            }

    }


    private void InvertAnimation()
    {
        EditorCurveBinding[] originInfo = AnimationUtility.GetCurveBindings((AnimationClip)originAnimation.value);
        

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(((AnimationClip)originAnimation.value));
        AnimationUtility.SetAnimationClipSettings((AnimationClip)destinyAnimation.value, settings);

        string previousPath = "";
        for (int i = 0; i < originInfo.Length; i++)
        {
            if (((GameObject)tpose.value).transform.Find(originInfo[i].path) != null)
            {
                AnimationCurve originCurve = AnimationUtility.GetEditorCurve((AnimationClip)originAnimation.value, originInfo[i]);
                AnimationCurve destinyCurve = new AnimationCurve();

                previousPath = originInfo[i].path;
                if (originInfo[i].path.Contains("Left") || originInfo[i].path.Contains("Right"))
                {
                    
                    if (originInfo[i].path.Contains("Left"))
                    {
                        originInfo[i].path = originInfo[i].path.Replace("Left", "Right");
                    }
                    else if (originInfo[i].path.Contains("Right"))
                    {
                        originInfo[i].path = originInfo[i].path.Replace("Right", "Left");
                    }
                }
                
                
                if (originInfo[i].propertyName == "m_LocalPosition.x")
                {
                    SetKeysValue(ref destinyCurve, originCurve, -1);

                }
                else if (originInfo[i].propertyName == "m_LocalPosition.y")
                {

                    SetKeysValue(ref destinyCurve, originCurve, 1);

                }
                else if (originInfo[i].propertyName == "m_LocalPosition.z")
                {
                    SetKeysValue(ref destinyCurve, originCurve,  1);

                }
                else if (originInfo[i].propertyName == "m_LocalRotation.x")
                {
                    SetKeysValue(ref destinyCurve, originCurve,  1);
                }
                else if (originInfo[i].propertyName == "m_LocalRotation.y")
                {
                    SetKeysValue(ref destinyCurve, originCurve, -1);
                }
                else if (originInfo[i].propertyName == "m_LocalRotation.z")
                { 
                    SetKeysValue(ref destinyCurve, originCurve, -1);
                }
                else if (originInfo[i].propertyName == "m_LocalRotation.w")
                {
                    SetKeysValue(ref destinyCurve, originCurve, 1);
                }
            
               ((AnimationClip)destinyAnimation.value).SetCurve(originInfo[i].path, typeof(Transform), originInfo[i].propertyName, destinyCurve);
               }//if
            }//for
        }
}
        

    

