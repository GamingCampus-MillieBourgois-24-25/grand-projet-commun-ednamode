using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManagerLogin : MonoBehaviour
{
    public static UIManagerLogin Instance;

    [SerializeField]
    private FirebaseAuthManager firebaseAuthManager;

    [SerializeField]
    private GameObject StartingScreenPanel;

    [SerializeField]
    private GameObject loginPanel;

    [SerializeField]
    private GameObject registrationPanel;

    [SerializeField]
    private GameObject emailVerifPanel;
    public Text EmailVerifText;

    [SerializeField]
    private GameObject EnterGamePanel;

    [SerializeField]
    private GameObject SignedInPanel;

    [SerializeField] 
    private GameObject transitionCoverPanel;

    [SerializeField] private RectTransform startingScreenTransform;
    [SerializeField] private float startingTransitionDuration = 1f;
    [SerializeField] private float panelTransitionDuration = 0.5f;


    private void Awake()
    {
        CreateInstance();
     

        if (firebaseAuthManager == null)
        {
            Debug.LogError("FirebaseAuthManager n'a pas été trouvé dans la scène.");
        }
    }

    private void CreateInstance()
    {
        if(Instance == null)
        {
            Instance = this;
        }
    }

   


    public void OpenGameLogin()
    {
        OpenLoginPanel();
        firebaseAuthManager.StartGameLoginProcess();
    }

    public void OpenGameRegistration()
    {
        OpenRegistrationPanel();
        firebaseAuthManager.StartGameLoginProcess();
    }

    public void ClearUI()
    {
        StartingScreenPanel.SetActive(false);
        loginPanel.SetActive(false);
        registrationPanel.SetActive(false);
        emailVerifPanel.SetActive(false);
        EnterGamePanel.SetActive(false);
        SignedInPanel.SetActive(false);
    }

    public void OpenLoginPanel()
    {
        ClearUI();
        UITransitionManager.Instance.AnimatePanelIn(loginPanel);
    }

    public void OpenRegistrationPanel()
    {
        ClearUI();
        UITransitionManager.Instance.AnimatePanelIn(registrationPanel);
    }

    public void ShowVerificationResponse(bool isEmailSent, string emailId, string errorMessage)
    {
        ClearUI();
        UITransitionManager.Instance.AnimatePanelIn(emailVerifPanel);

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

    public void OpenSignedInPanel()
    {
        ClearUI();

    }
}
