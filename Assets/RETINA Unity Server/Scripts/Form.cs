using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

namespace RetinaNetworking.Server
{
    [RequireComponent(typeof(UserAuth))]
    public class Form : MonoBehaviour
    {
        [SerializeField] List<TMP_InputField> inputFields;

        [SerializeField] Button button;

        [SerializeField] TextMeshProUGUI debugText;

        [SerializeField] UserAuth webPost;

        private bool isActive;

        private void Start()
        {
            isActive = false;
        }

        private void Update()
        {
            CheckInputFields();

            if (isActive)
            {
                //enable button
                button.interactable = true;
            }
            else
            {
                //disable button
                button.interactable = false;
            }
        }

        private void CheckInputFields()
        {
            foreach (TMP_InputField field in inputFields)
            {
                if (string.IsNullOrEmpty(field.text))
                {
                    isActive = false;
                    return;
                }
            }

            // all the input fields were NOT null or empty, therefore valid
            isActive = true;
            return;
        }

        public void SendForm()
        {
            if (isActive)
            {
                string username = inputFields[0].text;
                string email = inputFields[1].text;
                string gender = inputFields[2].text;
                int age = 0;
                try
                {
                    int.TryParse(inputFields[3].text, out age);
                }
                catch (SystemException ex)
                {
                    Debug.LogError($"Age input was not a valid: {ex}");
                }
                string language = inputFields[4].text;
                string password = inputFields[5].text;

                webPost.RequestAuth(username, email, gender, age, language, password);
            }
            else
            {
                Debug.LogError("Button not active, something has gone wrong");
            }
        }

        public void ResetForm()
        {
            foreach(TMP_InputField field in inputFields)
            {
                field.text = "";
            }
        }

    }
}

