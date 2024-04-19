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
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        Label label = new Label("");
        root.Add(label);

        // VisualElements objects can contain other VisualElement following a tree hierarchy
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

        // Create button
        Button button = new Button();
        button.name = "invertir";
        button.text = "Invertir";
        button.clicked += InvertAnimation;
        root.Add(button);

    }


    private void AddAllKeysToArray(int quaternionpos, int keyIndex, ref Dictionary<string, Dictionary<int, float[]>> rotationskeys, EditorCurveBinding[] curvesinfo, AnimationCurve animCurve)
    {
        float[] array = new float[animCurve.keys.Length];
        for (int j = 0; j < animCurve.keys.Length; j++)
            array[j] = animCurve.keys[j].value;
        rotationskeys[curvesinfo[keyIndex].path].Add(quaternionpos, array);
    }
    private Dictionary<string, Dictionary<int, float[]>> CollectAllRotations(out Dictionary<string, bool> rotationSetted) 
    {
        EditorCurveBinding[] curvesinfo = AnimationUtility.GetCurveBindings((AnimationClip)originAnimation.value);
        Dictionary<string, Dictionary<int, float[]>> rotationskeys = new Dictionary<string, Dictionary<int, float[] >>();
        rotationSetted = new Dictionary<string, bool>();
        for (int i = 0; i < curvesinfo.Length; i++)
        {
            if (!rotationskeys.ContainsKey(curvesinfo[i].path))
            { 
                rotationskeys.Add(curvesinfo[i].path, new Dictionary<int, float[] >());
                rotationSetted.Add(curvesinfo[i].path, false);
            }
            if (curvesinfo[i].propertyName == "m_LocalRotation.x" || curvesinfo[i].propertyName == "m_LocalRotation.y" || curvesinfo[i].propertyName == "m_LocalRotation.z" || curvesinfo[i].propertyName == "m_LocalRotation.w")
            {
                AnimationCurve animCurve = AnimationUtility.GetEditorCurve((AnimationClip)originAnimation.value, curvesinfo[i]);
                if (curvesinfo[i].propertyName == "m_LocalRotation.x")
                {
                    AddAllKeysToArray(0, i,ref rotationskeys, curvesinfo, animCurve);
                }
                else if (curvesinfo[i].propertyName == "m_LocalRotation.y")
                {
                    AddAllKeysToArray(1, i, ref rotationskeys, curvesinfo, animCurve);

                }
                if (curvesinfo[i].propertyName == "m_LocalRotation.z")
                {
                    AddAllKeysToArray(2, i, ref rotationskeys, curvesinfo, animCurve);

                }
                if (curvesinfo[i].propertyName == "m_LocalRotation.w")
                {
                    AddAllKeysToArray(3, i, ref rotationskeys, curvesinfo, animCurve);

                }
            }
        }
        return rotationskeys;
    }

    private Quaternion ConvertArrayToQuaternion(Dictionary<string, Dictionary<int, float[]>> rotationskeys, string path, int index) 
    {
        Quaternion quat = new Quaternion(rotationskeys.GetValueOrDefault(path).GetValueOrDefault(0)[index],
        rotationskeys.GetValueOrDefault(path).GetValueOrDefault(1)[index],
        rotationskeys.GetValueOrDefault(path).GetValueOrDefault(2)[index],
        rotationskeys.GetValueOrDefault(path).GetValueOrDefault(3)[index]);

        return quat;
    }

    private Vector3 ConvertArrayToEuler(Dictionary<string, Dictionary<int, float[]>> rotationskeys, string path, int index)
    {
        Vector3 vec = new Vector3(rotationskeys.GetValueOrDefault(path).GetValueOrDefault(0)[index],
        rotationskeys.GetValueOrDefault(path).GetValueOrDefault(1)[index],
        rotationskeys.GetValueOrDefault(path).GetValueOrDefault(2)[index]);

        return vec;
    }
    public void SetKeysValue(string previousPath, string destinyPath, ref AnimationCurve destinyCurve, AnimationCurve originCurve,float tPoseOrigin, float tPoseDestiny, int signo, bool isQuaternion = false, Dictionary<string, Dictionary<int, float[]>> originRotation = null, Dictionary<string, Dictionary<int, float[]>> destinyRotation = null, char rotationpos = 'x') {

        tPoseDestiny = 0;
        tPoseOrigin = 0;

        if (!previousPath.Contains("Left") && !previousPath.Contains("Right"))
        {
            tPoseOrigin = 0;
            tPoseDestiny = 0;
        }

        if (isQuaternion)
        {
            for (int k = 0; k < originCurve.keys.Length; k++)
            {
                if(rotationpos == 'x')
                    destinyCurve.AddKey(originCurve.keys[k].time, destinyRotation[destinyPath][0][k]);
                if (rotationpos == 'y')
                    destinyCurve.AddKey(originCurve.keys[k].time, destinyRotation[destinyPath][1][k]);
                if (rotationpos == 'z')
                    destinyCurve.AddKey(originCurve.keys[k].time, destinyRotation[destinyPath][2][k]);
                if (rotationpos == 'w')
                    destinyCurve.AddKey(originCurve.keys[k].time, destinyRotation[destinyPath][3][k]);
            }
        }
        else 
        {
            for (int k = 0; k < originCurve.keys.Length; k++)
            {
                destinyCurve.AddKey(originCurve.keys[k].time, tPoseDestiny + ((originCurve.keys[k].value  - tPoseOrigin) * signo));
            }
        }
        
    }

    private void SavePositions(string previousPath,string destinyPath, AnimationCurve originCurve, Quaternion tPoseOrigin, Quaternion tPoseDestiny, Dictionary<string, Dictionary<int, float[]>> originRotation, ref Dictionary<string, Dictionary<int, float[]>> destinyRotation)
    {
        if (!destinyRotation.ContainsKey(destinyPath))
        {
            destinyRotation.Add(destinyPath, new Dictionary<int, float[]>());
        }

       // Debug.Log(originRotation[previousPath][0].Length + " " + originRotation[previousPath][3].Length);

        for (int i = 0; i < 4; i++)
        {
            int signo = 1;
            float[] array = new float[originRotation[previousPath][i].Length];

            for (int k = 0; k < originRotation[previousPath][i].Length; k++)
            {
                Quaternion quat = ConvertArrayToQuaternion(originRotation, previousPath, k);

                float originAnimKey = 0;
                float origin = 0;
                float destiny = 0;
                if (i == 0) 
                {
                    origin = tPoseOrigin.x;
                    destiny = 0;//tPoseDestiny.x;
                    signo = 1;
                    originAnimKey = quat.x;
                }
                if (i == 1)
                {
                    origin = tPoseOrigin.y;
                    destiny = 0;
                    signo = 1;
                    originAnimKey = quat.y;
                }
                if (i == 2)
                {
                    origin = tPoseOrigin.z;
                    destiny = 0;
                    signo = 1;
                    originAnimKey = quat.z;
                }
                if (i == 3)
                {
                    origin = tPoseOrigin.w;
                    destiny = 0;
                    signo = 1;
                    originAnimKey = quat.w;
                }
                //if (!previousPath.Contains("Left") && !previousPath.Contains("Right"))
                {
                    origin = 0;
                    destiny = 0;
                }

                array[k] = destiny + ((originAnimKey - origin) * signo);
            }
            destinyRotation[destinyPath].Add(i, array);
        }
    }



    private void InvertAnimation()
    {
        EditorCurveBinding[] originInfo = AnimationUtility.GetCurveBindings((AnimationClip)originAnimation.value);
        Dictionary<string, bool> rotationSaved;
        Dictionary<string, Dictionary<int, float[]>> rotationOrigin = CollectAllRotations(out rotationSaved);
        Dictionary<string, Dictionary<int, float[]>> rotationDestiny = new Dictionary<string, Dictionary<int, float[]>>();

        AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(((AnimationClip)originAnimation.value));
        AnimationUtility.SetAnimationClipSettings((AnimationClip)destinyAnimation.value, settings);

        string previousPath = "";
        string destinyPath = "";
        int wcount = 0;
        int xcount = 0;
        for (int i = 0; i < originInfo.Length; i++)
        {
            if (((GameObject)tpose.value).transform.Find(originInfo[i].path) != null)
            {
                AnimationCurve originCurve = AnimationUtility.GetEditorCurve((AnimationClip)originAnimation.value, originInfo[i]);
                AnimationCurve destinyCurve = new AnimationCurve();

                previousPath = originInfo[i].path;
                destinyPath = originInfo[i].path;
                if (originInfo[i].path.Contains("Left") || originInfo[i].path.Contains("Right"))
                {
                    
                    if (originInfo[i].path.Contains("Left"))
                    {
                        //originInfo[i].path = originInfo[i].path.Replace("Left", "Right");
                        //destinyPath = originInfo[i].path.Replace("Left", "Right");
                    }
                    else if (originInfo[i].path.Contains("Right"))
                    {
                        //originInfo[i].path = originInfo[i].path.Replace("Right", "Left");
                        //destinyPath = originInfo[i].path.Replace("Left", "Right");
                    }
                }
                
                
                if (originInfo[i].propertyName == "m_LocalPosition.x")
                {
                    SetKeysValue(previousPath, originInfo[i].path, ref destinyCurve, originCurve, ((GameObject)tpose.value).transform.Find(previousPath).localPosition.x, ((GameObject)tpose.value).transform.Find(originInfo[i].path).localPosition.x, 1);

                }
                else if (originInfo[i].propertyName == "m_LocalPosition.y")
                {

                    SetKeysValue(previousPath, originInfo[i].path, ref destinyCurve, originCurve, ((GameObject)tpose.value).transform.Find(previousPath).localPosition.y, ((GameObject)tpose.value).transform.Find(originInfo[i].path).localPosition.y, 1);

                }
                else if (originInfo[i].propertyName == "m_LocalPosition.z")
                {
                    //Debug.Log(previousPath + ((GameObject)tpose.value).transform.Find(previousPath).localPosition.z + " " + ((GameObject)tpose.value).transform.Find(curvesinfo[i].path).localPosition.z);

                    SetKeysValue(previousPath, originInfo[i].path, ref destinyCurve, originCurve, ((GameObject)tpose.value).transform.Find(previousPath).localPosition.z, ((GameObject)tpose.value).transform.Find(originInfo[i].path).localPosition.z, 1);

                }
                else if (originInfo[i].propertyName == "m_LocalRotation.x" 
                      || originInfo[i].propertyName == "m_LocalRotation.y" 
                      || originInfo[i].propertyName == "m_LocalRotation.z" 
                      || originInfo[i].propertyName == "m_LocalRotation.w")
                {
                    if (!rotationSaved[previousPath]) 
                    { 
                        rotationSaved[previousPath] = true;
                        SavePositions(previousPath, originInfo[i].path, originCurve, ((GameObject)tpose.value).transform.Find(previousPath).localRotation, ((GameObject)tpose.value).transform.Find(originInfo[i].path).localRotation, rotationOrigin, ref rotationDestiny);
                    
                    }

                    if (originInfo[i].propertyName == "m_LocalRotation.x")
                    {
                        xcount++;
                        SetKeysValue(previousPath, originInfo[i].path, ref destinyCurve, originCurve, 0, 0, -1, true, rotationOrigin, rotationDestiny,'x');
                    }
                    else if (originInfo[i].propertyName == "m_LocalRotation.y")
                        SetKeysValue(previousPath, originInfo[i].path, ref destinyCurve, originCurve, 0, 0, -1, true, rotationOrigin, rotationDestiny, 'y');
                    if (originInfo[i].propertyName == "m_LocalRotation.z")
                        SetKeysValue(previousPath, originInfo[i].path, ref destinyCurve, originCurve, 0,0, -1, true, rotationOrigin, rotationDestiny, 'z');
                    if (originInfo[i].propertyName == "m_LocalRotation.w")
                    {
                        wcount++;
                        SetKeysValue(previousPath, originInfo[i].path, ref destinyCurve, originCurve,0,0, -1, true, rotationOrigin, rotationDestiny, 'w');
                    }

                }
                //estudiar cada hueso para rotacion
                //estudiar hips y spine y neck
                ((AnimationClip)destinyAnimation.value).SetCurve(originInfo[i].path, typeof(Transform), originInfo[i].propertyName, destinyCurve);
                //AnimationUtility.SetEditorCurve((AnimationClip)destinyAnimation.value, originInfo[i], destinyCurve);
                //Debug.Log(previousPath + " " + originInfo[i].path);
            }//if
        }//for
        //Debug.Log(wcount + " " + xcount);
    }
}
        

    

