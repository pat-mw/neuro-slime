using System.Collections.Generic;
using UnityEngine;
using Sirenix.Serialization;

namespace RetinaNetworking
{
    public class JSONHandler
    {
        /// <summary>
        /// decodes a byte array containing serialized JSON data
        /// </summary>
        public static Data DecodeByteArray(byte[] encodedData)
        {
            Data result;

            result = SerializationUtility.DeserializeValue<Data>(encodedData, DataFormat.JSON);

            return result;
        }

        public static NestedData DecodeNestedByteArray(byte[] encodedData)
        {
            NestedData result;

            result = SerializationUtility.DeserializeValue<NestedData>(encodedData, DataFormat.JSON);

            return result;
        }
        
        public static Data DecodeString(string encodedData)
        {
            Data result = new Data();

            JsonUtility.FromJsonOverwrite(encodedData, result);

            return result;
        }

        public static NestedData DecodeNestedString(string encodedData)
        {
            NestedData result = new NestedData();

            JsonUtility.FromJsonOverwrite(encodedData, result);

            return result;
        }


        /// <summary>
        /// returns a byte array containing the serialized JSON for any given object
        /// </summary>
        public static byte[] EncodeByteArray(Data dataToEncode)
        {
            byte[] result;
            
            result = SerializationUtility.SerializeValue(dataToEncode, DataFormat.JSON);
            
            return result;
        }

        public static byte[] EncodeByteArray(NestedData dataToEncode)
        {
            byte[] result;

            result = SerializationUtility.SerializeValue(dataToEncode, DataFormat.JSON);

            return result;
        }

        public static string EncodeString(Data dataToEncode)
        {
            string result;

            result = JsonUtility.ToJson(dataToEncode);

            return result;
        }

        public static string EncodeString(NestedData dataToEncode)
        {
            string result;

            result = JsonUtility.ToJson(dataToEncode);

            return result;
        }
    }
}