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

        private UserAuth userAuth;

        private bool isValid;

        private void Start()
        {
            isValid = false;
            userAuth = GetComponent<UserAuth>();
        }

        private void Update()
        {
            CheckInputFields();

            if (isValid)
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
                    isValid = false;
                    return;
                }
            }

            // all the input fields were NOT null or empty, therefore valid
            isValid = true;
            return;
        }

        public void SendForm()
        {
            if (isValid)
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

                userAuth.RequestAuth(username, email, gender, age, language, password);
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
            isValid = false;
        }

    }
}

