using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Firebase;
using Firebase.Auth;
using System;

public class FirebaseAuthManager : MonoBehaviour
{
    // Firebase variable
    [Header("Firebase")]
    public DependencyStatus dependencyStatus;
    [NonSerialized]
    public FirebaseAuth auth;
    [NonSerialized]
    public FirebaseUser user;
    [TextArea]
    public string jsonConfig;

    // Login Variables
    [Space]
    [Header("Login")]
    public InputField emailLoginField;
    public InputField passwordLoginField;

    // Registration Variables
    [Space]
    [Header("Registration")]
    public InputField nameRegisterField;
    public InputField emailRegisterField;
    public InputField passwordRegisterField;
    public InputField confirmPasswordRegisterField;

    // UI pour afficher les erreurs
    [Space]
    [Header("Error Display")]
    [SerializeField]
    private Text errorDisplayText; // Utilisez TMP_Text si vous utilisez TextMeshPro, sinon remplacez par Text

    private void Awake()
    {
        InitializeFirebase();
    }

    public void StartGameLoginProcess()
    {
        StartCoroutine(CheckAndFixDependenciesAsync());
    }

    private IEnumerator CheckAndFixDependenciesAsync()
    {
        var dependencyTask = FirebaseApp.CheckAndFixDependenciesAsync();

        yield return new WaitUntil(() => dependencyTask.IsCompleted);

        dependencyStatus = dependencyTask.Result;

        if (dependencyStatus == DependencyStatus.Available)
        {
            yield return new WaitForEndOfFrame();
            StartCoroutine(CheckForAutoLogin());

        }
        else
        {
            Debug.LogError("Could not resolve all firebase dependencies: " + dependencyStatus);
        }
    }

    private void DisplayError(string message)
    {
        if (errorDisplayText != null)
        {
            errorDisplayText.color = Color.red; // Définit la couleur en rouge
            errorDisplayText.text = message;
            StartCoroutine(HideErrorAfterDelay(5f)); // Masque l'erreur après 5 secondes
        }
        else
        {
            Debug.LogError("Error Display Text is not assigned in the Inspector.");
        }
    }

    private IEnumerator HideErrorAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (errorDisplayText != null)
        {
            errorDisplayText.text = "";
        }
    }

    void InitializeFirebase()
    {
        AppOptions options = AppOptions.LoadFromJsonConfig(jsonConfig);
        FirebaseApp app = FirebaseApp.Create(options, "DripOrDrop");
        auth = FirebaseAuth.GetAuth(app);

        auth.StateChanged += AuthStateChanged;
        AuthStateChanged(this, null);
    }


    private IEnumerator CheckForAutoLogin()
    {
        // Check if the user is already logged in
        if (user != null)
        {
            var reloadUserTask = user.ReloadAsync();
            yield return new WaitUntil(() => reloadUserTask.IsCompleted);
            AutoLogin();
        }
        else
        {
            UIManager.Instance.OpenLoginPanel();
        }
        yield return null;
    }

    private void AutoLogin()
    {
        // Check if the user is already logged in
        if (user != null)
        {
            if (user.IsEmailVerified)
            {
                Debug.Log("Auto Login Success");
                References.userName = user.DisplayName;
                UIManager.Instance.OpenSignedInPanel();
            }
            else
            {
                SendEmailForVerfication();
            }
        }
        else
        {
            UIManager.Instance.OpenLoginPanel();
        }
    }

    // Track state changes of the auth object.
    void AuthStateChanged(object sender, System.EventArgs eventArgs)
    {
        if (auth.CurrentUser != user)
        {
            bool signedIn = user != auth.CurrentUser && auth.CurrentUser != null;

            if (!signedIn && user != null)
            {
                Debug.Log("Signed out " + user.UserId);
                UIManager.Instance.OpenLoginPanel();
                ClearLoginFieldInputText();
            }

            user = auth.CurrentUser;

            if (signedIn)
            {
                Debug.Log("Signed in " + user.UserId);
            }
        }
    }

    private void ClearLoginFieldInputText()
    {
        emailLoginField.text = "";
        passwordLoginField.text = "";
    }

    public void Logout()
    {
        if (user != null && auth != null)
        {
            auth.SignOut();

        }
    }

    public void Login()
    {
        if (emailLoginField == null || passwordLoginField == null)
        {
            Debug.LogError("Les champs emailLoginField ou passwordLoginField ne sont pas assignés.");
            return;
        }

        StartCoroutine(LoginAsync(emailLoginField.text, passwordLoginField.text));
    }

    private IEnumerator LoginAsync(string email, string password)
    {
        if (auth == null)
        {
            DisplayError("FirebaseAuth n'est pas initialisé.");
            yield break;
        }

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            DisplayError("Email ou mot de passe est vide.");
            yield break;
        }

        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => loginTask.IsCompleted);

        if (loginTask.Exception != null)
        {
            FirebaseException firebaseException = loginTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;

            string failedMessage = "Échec de la connexion : ";

            switch (authError)
            {
                case AuthError.InvalidEmail:
                    failedMessage += "Email invalide.";
                    break;
                case AuthError.WrongPassword:
                    failedMessage += "Mot de passe incorrect.";
                    break;
                case AuthError.MissingEmail:
                    failedMessage += "Email manquant.";
                    break;
                case AuthError.MissingPassword:
                    failedMessage += "Mot de passe manquant.";
                    break;
                default:
                    failedMessage += "Erreur inconnue.";
                    break;
            }

            DisplayError(failedMessage);
        }
        else
        {
            user = loginTask.Result.User;
            Debug.LogFormat("{0} Vous êtes connecté avec succès.", user.DisplayName);

            if (user.IsEmailVerified)
            {
                References.userName = user.DisplayName;
                UIManager.Instance.OpenSignedInPanel();
            }
            else
            {
                SendEmailForVerfication();
            }
        }
    }


    public void Register()
    {
        StartCoroutine(RegisterAsync(nameRegisterField.text, emailRegisterField.text, passwordRegisterField.text, confirmPasswordRegisterField.text));
    }

    private IEnumerator RegisterAsync(string name, string email, string password, string confirmPassword)
    {
        if (string.IsNullOrEmpty(name))
        {
            DisplayError("Le nom d'utilisateur est vide.");
            yield break;
        }

        if (string.IsNullOrEmpty(email))
        {
            DisplayError("Le champ email est vide.");
            yield break;
        }

        if (password != confirmPassword)
        {
            DisplayError("Les mots de passe ne correspondent pas.");
            yield break;
        }

        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);

        yield return new WaitUntil(() => registerTask.IsCompleted);

        if (registerTask.Exception != null)
        {
            FirebaseException firebaseException = registerTask.Exception.GetBaseException() as FirebaseException;
            AuthError authError = (AuthError)firebaseException.ErrorCode;

            string failedMessage = "Échec de l'inscription : ";

            switch (authError)
            {
                case AuthError.InvalidEmail:
                    failedMessage += "Email invalide.";
                    break;
                case AuthError.MissingEmail:
                    failedMessage += "Email manquant.";
                    break;
                case AuthError.MissingPassword:
                    failedMessage += "Mot de passe manquant.";
                    break;
                default:
                    failedMessage += "Erreur inconnue.";
                    break;
            }

            DisplayError(failedMessage);
        }
        else
        {
            user = registerTask.Result.User;

            UserProfile userProfile = new UserProfile { DisplayName = name };

            var updateProfileTask = user.UpdateUserProfileAsync(userProfile);

            yield return new WaitUntil(() => updateProfileTask.IsCompleted);

            if (updateProfileTask.Exception != null)
            {
                user.DeleteAsync();
                DisplayError("Échec de la mise à jour du profil. Veuillez réessayer.");
            }
            else
            {
                Debug.Log("Inscription réussie. Bienvenue " + user.DisplayName);
                if (user.IsEmailVerified)
                {
                    UIManager.Instance.OpenLoginPanel();
                }
                else
                {
                    SendEmailForVerfication();
                }
            }
        }
    }

    public void SendEmailForVerfication()
    {
        StartCoroutine(SendEmailVerificationAsync());
    }

    private IEnumerator SendEmailVerificationAsync()
    {
        if (user != null)
        {
            var sendEmailTask = user.SendEmailVerificationAsync();
            yield return new WaitUntil(() => sendEmailTask.IsCompleted);

            if (sendEmailTask.Exception != null)
            {
                FirebaseException firebaseException = sendEmailTask.Exception.GetBaseException() as FirebaseException;
                AuthError authError = (AuthError)firebaseException.ErrorCode;

                string failedMessage = "Erreur lors de l'envoi de l'email de vérification : ";

                switch (authError)
                {
                    case AuthError.Cancelled:
                        failedMessage += "L'envoi a été annulé.";
                        break;
                    case AuthError.TooManyRequests:
                        failedMessage += "Trop de requêtes. Veuillez réessayer plus tard.";
                        break;
                    case AuthError.InvalidRecipientEmail:
                        failedMessage += "L'email saisi est invalide.";
                        break;
                    default:
                        failedMessage += "Erreur inconnue.";
                        break;
                }

                DisplayError(failedMessage);
            }
            else
            {
                Debug.Log("Email de vérification envoyé à " + user.Email);
                UIManager.Instance.ShowVerificationResponse(true, user.Email, null);
            }
        }
    }

    public void OpenGameScene()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("GameSceneExample");
    }

}


    
