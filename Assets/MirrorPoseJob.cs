using UnityEngine;

public static class AnimationHumanStreamExtensions
{
    public static void MultMuscle(this UnityEngine.Animations.AnimationHumanStream stream, UnityEngine.Animations.MuscleHandle h, float f)
    {
        stream.SetMuscle(h, f * stream.GetMuscle(h));
    }

    public static void SwapMuscles(this UnityEngine.Animations.AnimationHumanStream stream, UnityEngine.Animations.MuscleHandle a, UnityEngine.Animations.MuscleHandle b)
    {
        var t = stream.GetMuscle(a);
        stream.SetMuscle(a, stream.GetMuscle(b));
        stream.SetMuscle(b, t);
    }
}

public struct MirrorPoseJob : UnityEngine.Animations.IAnimationJob
{
    private static readonly float[] BodyDoFMirror = {
        +1.0f,  // BodyDof.SpineFrontBack,
        -1.0f,  // BodyDof.SpineLeftRight,
        -1.0f,  // BodyDof.SpineRollLeftRight,
        +1.0f,  // BodyDof.ChestFrontBack,
        -1.0f,  // BodyDof.ChestLeftRight,
        -1.0f,  // BodyDof.ChestRollLeftRight,
        +1.0f,  // BodyDof.UpperChestFrontBack,
        -1.0f,  // BodyDof.UpperChestLeftRight,
        -1.0f   // BodyDof.UpperChestRollLeftRight,
    };

    private static readonly float[] HeadDoFMirror = {
        +1.0f,  // HeadDof.NeckFrontBack,
        -1.0f,  // HeadDof.NeckLeftRight,
        -1.0f,  // HeadDof.NeckRollLeftRight,
        +1.0f,  // HeadDof.HeadFrontBack,
        -1.0f,  // HeadDof.HeadLeftRight,
        -1.0f,  // HeadDof.HeadRollLeftRight,
        +1.0f,  // HeadDof.LeftEyeDownUp,
        -1.0f,  // HeadDof.LeftEyeLeftRight,
        +1.0f,  // HeadDof.RightEyeDownUp,
        -1.0f,  // HeadDof.RightEyeLeftRight,
        +1.0f,  // HeadDof.JawDownUp,
        -1.0f   // HeadDof.JawLeftRight,
    };

    public void ProcessAnimation(UnityEngine.Animations.AnimationStream stream)
    {
        var humanStream = stream.AsHuman();

        //humanStream.bodyLocalPosition = Mirrored(humanStream.bodyLocalPosition);
        //humanStream.bodyLocalRotation = Mirrored(humanStream.bodyLocalRotation);

        //humanStream.bodyLocalPosition = Mirrored(humanStream.bodyLocalPosition);
        //humanStream.bodyLocalRotation = Quaternion.Euler(humanStream.bodyLocalRotation.eulerAngles.x, -humanStream.bodyLocalRotation.eulerAngles.y, humanStream.bodyLocalRotation.eulerAngles.z);


        humanStream.bodyPosition = Mirrored(humanStream.bodyPosition);
        humanStream.bodyRotation = Mirrored(humanStream.bodyRotation);

        // mirror body
        for (int i = 0; i < (int)BodyDof.LastBodyDof; i++)
        {
            humanStream.MultMuscle(new UnityEngine.Animations.MuscleHandle((BodyDof)i), BodyDoFMirror[i]);
        }

        // mirror head
        for (int i = 0; i < (int)HeadDof.LastHeadDof; i++)
        {
            humanStream.MultMuscle(new UnityEngine.Animations.MuscleHandle((HeadDof)i), HeadDoFMirror[i]);
        }

        // swap arms
        for (int i = 0; i < (int)ArmDof.LastArmDof; i++)
        {
            humanStream.SwapMuscles(
                new UnityEngine.Animations.MuscleHandle(HumanPartDof.LeftArm, (ArmDof)i),
                new UnityEngine.Animations.MuscleHandle(HumanPartDof.RightArm, (ArmDof)i));
        }

        // swap legs
        for (int i = 0; i < (int)LegDof.LastLegDof; i++)
        {
            humanStream.SwapMuscles(
                new UnityEngine.Animations.MuscleHandle(HumanPartDof.LeftLeg, (LegDof)i),
                new UnityEngine.Animations.MuscleHandle(HumanPartDof.RightLeg, (LegDof)i));
        }

        // swap fingers
        for (int i = 0; i < (int)FingerDof.LastFingerDof; i++)
        {
            humanStream.SwapMuscles(
                new UnityEngine.Animations.MuscleHandle(HumanPartDof.LeftThumb, (FingerDof)i),
                new UnityEngine.Animations.MuscleHandle(HumanPartDof.RightThumb, (FingerDof)i));
            humanStream.SwapMuscles(
                new UnityEngine.Animations.MuscleHandle(HumanPartDof.LeftIndex, (FingerDof)i),
                new UnityEngine.Animations.MuscleHandle(HumanPartDof.RightIndex, (FingerDof)i));
            humanStream.SwapMuscles(
                new UnityEngine.Animations.MuscleHandle(HumanPartDof.LeftMiddle, (FingerDof)i),
                new UnityEngine.Animations.MuscleHandle(HumanPartDof.RightMiddle, (FingerDof)i));
            humanStream.SwapMuscles(
                new UnityEngine.Animations.MuscleHandle(HumanPartDof.LeftRing, (FingerDof)i),
                new UnityEngine.Animations.MuscleHandle(HumanPartDof.RightRing, (FingerDof)i));
            humanStream.SwapMuscles(
                new UnityEngine.Animations.MuscleHandle(HumanPartDof.LeftLittle, (FingerDof)i),
                new UnityEngine.Animations.MuscleHandle(HumanPartDof.RightLittle, (FingerDof)i));
        }


        // swap ik
        Vector3[] goalLocalPositions = new Vector3[4];
        Quaternion[] goalLocalRotations = new Quaternion[4];
        Vector3[] goalPositions = new Vector3[4];
        Quaternion[] goalRotations = new Quaternion[4];
        float[] goalWeightPositons = new float[4];
        float[] goalWeightRotations = new float[4];
        Vector3[] hintPositions = new Vector3[4];
        float[] hintWeightPositions = new float[4];
        for (int i = 0; i < 4; i++)
        {
            goalLocalPositions[i] = humanStream.GetGoalLocalPosition(AvatarIKGoal.LeftFoot + i);
            goalLocalRotations[i] = humanStream.GetGoalLocalRotation(AvatarIKGoal.LeftFoot + i);

            goalPositions[i] = humanStream.GetGoalPosition(AvatarIKGoal.LeftFoot + i);
            goalRotations[i] = humanStream.GetGoalRotation(AvatarIKGoal.LeftFoot + i);

            goalWeightPositons[i] = humanStream.GetGoalWeightPosition(AvatarIKGoal.LeftFoot + i);
            goalWeightRotations[i] = humanStream.GetGoalWeightRotation(AvatarIKGoal.LeftFoot + i);
            hintPositions[i] = humanStream.GetHintPosition(AvatarIKHint.LeftKnee + i);
            hintWeightPositions[i] = humanStream.GetHintWeightPosition(AvatarIKHint.LeftKnee + i);
        }
        for (int i = 0; i < 4; i++)
        {
            int j = (i + 1) % 2 + (i / 2) * 2;                  // make [1, 0, 3, 2]
            //humanStream.SetGoalLocalPosition(AvatarIKGoal.LeftFoot + i, Mirrored(goalLocalPositions[j]));
            //humanStream.SetGoalLocalRotation(AvatarIKGoal.LeftFoot + i, Mirrored(goalLocalRotations[j]));

            humanStream.SetGoalPosition(AvatarIKGoal.LeftFoot + i, Mirrored(goalPositions[j]));
            humanStream.SetGoalRotation(AvatarIKGoal.LeftFoot + i, Mirrored(goalRotations[j]));

            humanStream.SetGoalWeightPosition(AvatarIKGoal.LeftFoot + i, goalWeightPositons[j]);
            humanStream.SetGoalWeightRotation(AvatarIKGoal.LeftFoot + i, goalWeightRotations[j]);

            humanStream.SetHintPosition(AvatarIKHint.LeftKnee + i, Mirrored(hintPositions[j]));
            humanStream.SetHintWeightPosition(AvatarIKHint.LeftKnee + i, hintWeightPositions[j]);
        }
        

    }

    public static Vector3 Mirrored(Vector3 value)
    {
        return new Vector3(-value.x, value.y, value.z);
    }

    public static Vector3 Mirrored_test(Vector3 value)
    {
        return new Vector3(-value.x, value.y, -value.z);
    }

    public static Quaternion Mirrored(Quaternion value)
    {
        return Quaternion.Euler(value.eulerAngles.x, -value.eulerAngles.y, -value.eulerAngles.z);
    }

    public static Quaternion Mirrored_test(Quaternion value)
    {
        return Quaternion.Euler(value.eulerAngles.x, -value.eulerAngles.y, value.eulerAngles.z);
    }

    public void ProcessRootMotion(UnityEngine.Animations.AnimationStream stream) { }
}
