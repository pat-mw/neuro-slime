using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;
using Cysharp.Threading.Tasks;
using ScriptableObjectArchitecture;


[CreateAssetMenu(menuName = "MAPPINGS/Float Mapping")]
public class FloatMapping : SerializedScriptableObject, IMapping
{
    public MapType mapType;
    public FloatReference influencer;
    public FloatReference receiver;

    public void Map()
    {
        Mappings.ApplyMap(mapType, influencer.Value, ref receiver);
    }


    public static class Mappings
    {
        public static void ApplyMap(MapType type, float influencer, ref FloatReference receiver)
        {
            switch (type)
            {
                case MapType.Follow:
                    receiver.Value = influencer;
                    break;
                default:
                    receiver.Value = 0;
                    break;
            }
        }
    }
}


