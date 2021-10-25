using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

namespace RetinaNetworking.Server
{
    [RequireComponent(typeof(UserAuth))]
    public class FormSimplified : MonoBehaviour
    {
        [SerializeField] List<TMP_InputField> inputFields;

        [SerializeField] List<Toggle> consentToggles;

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


            foreach (Toggle toggle in consentToggles)
            {
                if (toggle.isOn != true)
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
                string fullName = inputFields[0].text;
                string email = inputFields[1].text;
                string username = email;
                string gender = "n/a";
                int age = 0;
                string language = "n/a";
                string password = "password";


                webPost.connectionParams.SetName(fullName);

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

