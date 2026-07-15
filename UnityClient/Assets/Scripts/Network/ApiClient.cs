using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using CuocDuaKyThu.Utilities;

namespace CuocDuaKyThu.Network
{
    public class ApiClient : MonoBehaviour
    {
        public static ApiClient Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        // ════════════════════════════════════════
        // HTTP VERB WRAPPERS
        // ════════════════════════════════════════

        public void Get(string endpoint, Action<string> onSuccess, Action<string> onError)
        {
            StartCoroutine(SendRequest(endpoint, UnityWebRequest.kHttpVerbGET, null, onSuccess, onError));
        }

        public void Post(string endpoint, string jsonPayload, Action<string> onSuccess, Action<string> onError)
        {
            StartCoroutine(SendRequest(endpoint, UnityWebRequest.kHttpVerbPOST, jsonPayload, onSuccess, onError));
        }

        public void Delete(string endpoint, Action<string> onSuccess, Action<string> onError)
        {
            StartCoroutine(SendRequest(endpoint, UnityWebRequest.kHttpVerbDELETE, null, onSuccess, onError));
        }

        private IEnumerator SendRequest(string endpoint, string method, string jsonPayload, Action<string> onSuccess, Action<string> onError)
        {
            string url = Constants.DefaultServerUrl + endpoint;
            using var request = new UnityWebRequest(url, method);

            if (!string.IsNullOrEmpty(jsonPayload))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonPayload);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            }
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Accept", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onSuccess?.Invoke(request.downloadHandler.text);
            }
            else
            {
                onError?.Invoke(request.error + " | " + request.downloadHandler.text);
            }
        }
    }
}
