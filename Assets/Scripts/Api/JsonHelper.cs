using System;
using UnityEngine;

/// <summary>
/// JsonUtility no puede parsear un JSON cuyo elemento raíz sea un arreglo
/// (por ejemplo la respuesta de GET /players, que llega como "[ {...}, {...} ]").
/// Esta clase envuelve el arreglo en un objeto temporal { "array": [...] }
/// para poder deserializarlo con JsonUtility.
/// </summary>
public static class JsonHelper
{
    public static T[] FromJson<T>(string json)
    {
        string wrappedJson = "{\"array\":" + json + "}";
        Wrapper<T> wrapper = JsonUtility.FromJson<Wrapper<T>>(wrappedJson);
        return wrapper.array;
    }

    [Serializable]
    private class Wrapper<T>
    {
        public T[] array;
    }
}
