using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField]
    private GameObject loginPanel;

    [SerializeField]
    private GameObject registrationPanel;

    [SerializeField]
    private GameObject emailVerifPanel;
    public Text EmailVerifText;

    private void Awake()
    {
        CreateInstance();

    }

    private void Start()
    {
        OpenLoginPanel();
    }

    private void CreateInstance()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

    public void ClearUI()
    {
        loginPanel.SetActive(false);
        registrationPanel.SetActive(false);
        emailVerifPanel.SetActive(false);
    }

    public void OpenLoginPanel()
    {
        ClearUI();
        loginPanel.SetActive(true);
    }

    public void OpenRegistrationPanel()
    {
        ClearUI();  
        registrationPanel.SetActive(true);
    }

    public void ShowVerificationResponse(bool isEmailSent, string emailId, string errorMessage)
    {
        ClearUI();
        emailVerifPanel.SetActive(true);

        if (isEmailSent)
        {
            EmailVerifText.text = $"Please verify your email address: {emailId}.\n" +
                "A verification link has been sent to your email. Please check your inbox and click the link to verify your account.";
        }
        else
        {
            EmailVerifText.text = $"Error: {errorMessage}.\n" +
                "Couldn't sent email";
        }

    }
}
