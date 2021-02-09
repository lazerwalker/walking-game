using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class AuthenticationManager : MonoBehaviour
{
    private static string CustomIdPrefsKey = "CustomId";

    // TODO: I think we eventually want to log in with Game Center instead of anonymously,
    // on Game Center logins are properly validated with the public key
    void Start() {
        var CustomId = PlayerPrefs.GetString(CustomIdPrefsKey);
        if (CustomId == "") {
            Debug.Log("No CustomId, generating one");
            CustomId = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString(CustomIdPrefsKey, CustomId);
            PlayerPrefs.Save();
        }

        var request = new LoginWithCustomIDRequest { CustomId = CustomId.ToString(), CreateAccount = true};
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnFailure);
        Debug.Log($"CustomId: {CustomId.ToString()}");
    }

    public void LogInWithGameCenter() {
        // TODO: Replace this with our own Obj-C Game Center flow
        // so we can properly pass along public-key verification data
        Social.localUser.Authenticate(ProcessAuthentication);
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log($"returned playfabid: {result.PlayFabId}");
    }

    private void OnFailure(PlayFabError error)
    {
        Debug.LogWarning("API call failure");
        Debug.LogError(error.GenerateErrorReport());
    }

    void ProcessAuthentication (bool success) {
        if (success) {
            Debug.Log($"User ID: {Social.localUser.id}");
            var request = new LinkGameCenterAccountRequest { GameCenterId = Social.localUser.id, Social.localUser. };
        PlayFabClientAPI.LinkGameCenterAccountRequest(request, OnGameCenterLinkSucess, OnLoginFailure);
        } else
            Debug.Log ("Failed to authenticate");
        }
    }

    void OnGameCenterLinkSucess(LinkGameCenterAccountResult result) {
        Debug.Log("Linked game center successfully");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
