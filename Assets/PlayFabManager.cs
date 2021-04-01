using UnityEngine;
using PlayFab;
using PlayFab.ClientModels;
using UnityEngine.UI;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using TMPro;

public class PlayFabManager : MonoBehaviour
{
    [SerializeField]
    public Button gameCenterButton;
    public TextMeshProUGUI currencyText;

    public List<Button> storeButtons;
    
    public List<Button> inventoryButtons;

    public int playerCurrency;

    public List<CatalogItem> catalog;
    public List<ItemInstance> inventory;

    private static string CustomIdPrefsKey = "CustomId";

    // TODO: I think we eventually want to log in with Game Center instead of anonymously,
    // on Game Center logins are properly validated with the public key
    void Start() {
        #if !UNITY_IOS
            gameCenterButton.gameObject.SetActive(false);
        #endif
        
        var CustomId = PlayerPrefs.GetString(CustomIdPrefsKey);
        if (CustomId == "") {
            Debug.Log("No CustomId, generating one");
            CustomId = System.Guid.NewGuid().ToString();
            PlayerPrefs.SetString(CustomIdPrefsKey, CustomId);
            PlayerPrefs.Save();
        }

        Debug.Log($"CustomId: {CustomId.ToString()}");
        LogInWithCustomID(CustomId.ToString());
    }

    void LogInWithCustomID(string CustomId)
    {
     var request = new LoginWithCustomIDRequest { 
            CustomId = CustomId, 
            CreateAccount = true,
            InfoRequestParameters = new GetPlayerCombinedInfoRequestParams {
                GetUserVirtualCurrency = true,
                GetUserInventory = true,
            }
        };
        PlayFabClientAPI.LoginWithCustomID(request, (loginResult) => {
            Debug.Log($"returned playfabid: {loginResult.PlayFabId}");
            Debug.Log(JsonConvert.SerializeObject(loginResult));

            UpdatePlayerCurrency(loginResult.InfoResultPayload.UserVirtualCurrency["CC"]);

            PlayFabClientAPI.GetCatalogItems(new GetCatalogItemsRequest(), (catalogResult) => {
                Debug.Log("Fetched catalog");
                Debug.Log(JsonConvert.SerializeObject(catalogResult));

                catalog = catalogResult.Catalog;
                inventory = loginResult.InfoResultPayload.UserInventory;
                UpdateInventoryDisplay();
                UpdateCatalogDisplay();

            }, (error) => { 
                Debug.Log("Failed to fetch catalog");
                Debug.Log(error.GenerateErrorReport()); });
        }, (error) => { Debug.Log(error.GenerateErrorReport());});
    
    }

    private void BuyQuest(CatalogItem item) {
        Debug.Log($"Buying quest: {item.DisplayName}");
        // TODO: Move this to the server
        // We want to validate that players only have 1 of each quest
        // in their inventory, which requires custom server code
        var request = new PurchaseItemRequest {
            ItemId = item.ItemId,
            Price = (int)item.VirtualCurrencyPrices["CC"],
            VirtualCurrency = "CC"
        };
        PlayFabClientAPI.PurchaseItem(request, (result) => {
            Debug.Log(JsonConvert.SerializeObject(result));
            var items = result.Items;
            inventory.InsertRange (0, items);
            UpdateInventoryDisplay();
            UpdatePlayerCurrency(playerCurrency - request.Price);
            UpdateCatalogDisplay();
        }, (error) => { Debug.Log(error.GenerateErrorReport());});
    }

    private void UpdatePlayerCurrency(int currency) {
        playerCurrency = currency;
        currencyText.text = $"${currency}";
    }

    public void LogInWithGameCenter() {
        // TODO: Replace this with our own Obj-C Game Center flow
        // so we can properly pass along public-key verification data
        Social.localUser.Authenticate(ProcessAuthentication);
    }

    private void UpdateCatalogDisplay() {
        // TODO: Handle item expiration
        var filteredCatalog = catalog.FindAll(c => {
           return inventory.Find(inv => inv.ItemId == c.ItemId) == null;
        });

        foreach (var button in storeButtons) {
            button.gameObject.SetActive(false);
            button.onClick.RemoveAllListeners();
        }

        for (var i = 0; i < Math.Min(3, filteredCatalog.Count); i++) {
            var item = filteredCatalog[i];
            storeButtons[i].GetComponentsInChildren<TextMeshProUGUI>()[0].text = $"{item.DisplayName}: {item.Description} (${item.VirtualCurrencyPrices["CC"]})";
            storeButtons[i].gameObject.SetActive(true);
            storeButtons[i].onClick.AddListener(() => BuyQuest(item));
        };
    }
    private void UpdateInventoryDisplay() {
        foreach (var button in inventoryButtons) {
            button.gameObject.SetActive(false);
        }

        // var filteredItems = items.FindAll((item) => item)

        for (var i = 0; i < Math.Min(3, inventory.Count); i++) {
            var item = inventory[i];
            var catalogItem = catalog.Find((i) => i.ItemId == item.ItemId);

            // TODO: Show how much time is left
            var time = HumanReadableTime((DateTime)item.Expiration);
            var button = inventoryButtons[i].GetComponentsInChildren<TextMeshProUGUI>()[0];
            button.text = $"{item.DisplayName}: {catalogItem.Description} (expires in {time})";
            inventoryButtons[i].gameObject.SetActive(true);
        };
    }

    void ProcessAuthentication (bool success) {
        if (success) {
            Debug.Log($"User ID: {Social.localUser.id}");
            var request = new LinkGameCenterAccountRequest { GameCenterId = Social.localUser.id };
            PlayFabClientAPI.LinkGameCenterAccount(request, OnGameCenterLinkSucess, OnFailure);
        } else {
            Debug.Log ("Failed to authenticate");
        }
    }

    void OnFailure(PlayFabError error) {
        Debug.Log(error.GenerateErrorReport());
    }

    void OnGameCenterLinkSucess(LinkGameCenterAccountResult result) {
        Debug.Log("Linked game center successfully");
    }

    private string HumanReadableTime(DateTime time) {
        const int SECOND = 1;
        const int MINUTE = 60 * SECOND;
        const int HOUR = 60 * MINUTE;
        const int DAY = 24 * HOUR;
        const int MONTH = 30 * DAY;

        var ts = new TimeSpan(time.Ticks - DateTime.UtcNow.Ticks);
        double delta = Math.Abs(ts.TotalSeconds);

        if (delta < 1 * MINUTE)
        return ts.Seconds == 1 ? "one second ago" : ts.Seconds + " seconds";

        if (delta < 45 * MINUTE)
        return ts.Minutes + "min";


        if (delta < 24 * HOUR)
        return ts.Hours + "hr";

        if (delta < 30 * DAY)
        return ts.Days + "d";

        return time.ToString();
    }

    public Task<U> AsyncPlayFabRequest<T, U>(Action<T, Action<U>, Action<PlayFabError>, object, Dictionary<string, string>> fn, T request, object customData = null, Dictionary<string, string> extraHeaders = null)
    {
        return Task.Run(() =>
        {
            var t = new TaskCompletionSource<U>();


            fn(request, 
                result => t.TrySetResult(result),
                error => t.TrySetException(new Exception(error.GenerateErrorReport())),
                customData,
                extraHeaders
            );

            return t.Task;
        });
    }
}
