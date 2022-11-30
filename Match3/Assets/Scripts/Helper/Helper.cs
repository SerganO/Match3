using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;


public delegate void VoidFunc();
public delegate void IntFunc(int value);
public delegate void FloatFunc(float value);
public delegate void StringFunc(string value);

public static class Helper
{
    public static T DeepClone<T>(T obj)
    {
        using (var ms = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;

            return (T)formatter.Deserialize(ms);
        }
    }
    public static IEnumerator Wait(float time, VoidFunc action)
    {
        yield return new WaitForSeconds(time);
        action();
    }

    public static void Wait(MonoBehaviour monoBehaviour, float time, VoidFunc action)
    {
        monoBehaviour.StartCoroutine(Wait(time, action));
    }
}

public static class StringHelper
{
    public static string ColorText(this string text, string color)
    {
        return "<color=" + color + ">" + text + "</color>";
    }

    public static string RedText(this string text)
    {
        return text.ColorText("red");
    }

    public static string BlueText(this string text)
    {
        return text.ColorText("blue");
    }

    public static string GreenText(this string text)
    {
        return text.ColorText("green");
    }

    public static string YellowText(this string text)
    {
        return text.ColorText("yellow");
    }
}

public static class TransformHelper
{
    public static void DestroyAllChilds(this Transform transform)
    {
        foreach (Transform child in transform)
        {
            Object.Destroy(child.gameObject);
        }
    }

    public static void DestroyImmediateAllChilds(this Transform transform)
    {
        int childCount = transform.childCount;
        for (int i = 0; i < childCount; i++)
        {
            var child = transform.GetChild(0);
            Object.DestroyImmediate(child.gameObject);
        }
    }
}

public static class Vector3Helper
{
    public static Vector3 MovedByX(this Vector3 vector, float offset)
    {
        return new Vector3(vector.x + offset, vector.y, vector.z);
    }

    public static Vector3 MovedByY(this Vector3 vector, float offset)
    {
        return new Vector3(vector.x, vector.y + offset, vector.z);
    }

    public static Vector3 MovedByZ(this Vector3 vector, float offset)
    {
        return new Vector3(vector.x, vector.y, vector.z + offset);
    }

    public static Vector3 MovedByXYZ(this Vector3 vector, float offsetX = 0, float offsetY = 0, float offsetZ = 0)
    {
        return new Vector3(vector.x + offsetX, vector.y + offsetY, vector.z + offsetZ);
    }

}

