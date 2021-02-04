using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;

public class AuthenticationManager : MonoBehaviour
{
    private static string CustomIdPrefsKey = "CustomId";

    // Start is called before the first frame update
    void Start()
    {
        // check if we have a customID
        // if we do, login
        // if we don't, create one

        var CustomId = PlayerPrefs.GetString(CustomIdPrefsKey);
        if (CustomId == "") {
            Debug.Log("No CustomId, generating one");
            CustomId = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString(CustomIdPrefsKey, CustomId);
            PlayerPrefs.Save();
        }

        var request = new LoginWithCustomIDRequest { CustomId = CustomId.ToString(), CreateAccount = true};
        PlayFabClientAPI.LoginWithCustomID(request, OnLoginSuccess, OnLoginFailure);
        Debug.Log($"CustomId: {CustomId.ToString()}");
    }

    private void OnLoginSuccess(LoginResult result)
    {
        Debug.Log($"returned playfabid: {result.PlayFabId}");
    }

    private void OnLoginFailure(PlayFabError error)
    {
        Debug.LogWarning("Something went wrong with your first API call.  :(");
        Debug.LogError("Here's some debug information:");
        Debug.LogError(error.GenerateErrorReport());
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
