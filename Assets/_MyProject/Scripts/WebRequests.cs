using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class WebRequests : MonoBehaviour
{
    public const string JSON_EXTENSION = ".json";

    public static WebRequests Instance;
    private string userIdToken;

    private void Awake()
    {
        if (Instance==null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void SetUserToken(string _token)
    {
        userIdToken = _token;
    }

    public void Get(string _uri, Action<string> _onSuccess=null, Action<string> _onError=null)
    {
        StartCoroutine(GetRoutine(_uri, _onSuccess, _onError));
    }

    public void Post(string _uri, string _jsonData, Action<string> _onSuccess=null, Action<string> _onError=null, bool _includeHeader = true)
    {
        StartCoroutine(PostRoutine(_uri, _jsonData, _onSuccess, _onError, _includeHeader));
    }

    public void Put(string _uri, string _jsonData, Action<string> _onSuccess=null, Action<string> _onError=null)
    {
        StartCoroutine(PutRoutine(_uri, _jsonData, _onSuccess, _onError));
    }

    public void Patch(string _uri, string _jsonData, Action<string> _onSuccess=null, Action<string> _onError=null)
    {
        StartCoroutine(PatchRoutine(_uri, _jsonData, _onSuccess, _onError));
    }

    public void Delete(string _uri, Action<string> _onSuccess=null, Action<string> _onError=null)
    {
        StartCoroutine(DeleteRoutine(_uri, _onSuccess, _onError));
    }

    private IEnumerator GetRoutine(string _uri, Action<string> _onSuccess, Action<string> _onError)
    {
        if (!string.IsNullOrEmpty(userIdToken))
        {
            _uri = $"{_uri}?auth={userIdToken}";
        }

        using UnityWebRequest _webRequest = UnityWebRequest.Get(_uri);
        yield return _webRequest.SendWebRequest();

        if (_webRequest.result == UnityWebRequest.Result.Success)
        {
            _onSuccess?.Invoke(_webRequest.downloadHandler.text);
        }
        else
        {
            Debug.Log(_webRequest.error);
            _onError?.Invoke(_webRequest.error);
        }

        _webRequest.Dispose();
    }

    private IEnumerator PostRoutine(string _uri, string _jsonData, Action<string> _onSuccess, Action<string> _onError, bool _includeHeader = true)
    {
        if (!string.IsNullOrEmpty(userIdToken))
        {
            if (_includeHeader)
            {
                _uri = $"{_uri}?auth={userIdToken}";
            }
        }

        using UnityWebRequest _webRequest = UnityWebRequest.Post(_uri, _jsonData);
        byte[] _jsonToSend = new System.Text.UTF8Encoding().GetBytes(_jsonData);
        _webRequest.uploadHandler = new UploadHandlerRaw(_jsonToSend);
        _webRequest.downloadHandler = new DownloadHandlerBuffer();
        _webRequest.SetRequestHeader("Content-Type", "application/json");

        yield return _webRequest.SendWebRequest();

        if (_webRequest.result == UnityWebRequest.Result.Success)
        {
            _onSuccess?.Invoke(_webRequest.downloadHandler.text);
        }
        else
        {
            Debug.Log(_uri);
            Debug.Log(_webRequest.error);
            _onError?.Invoke(_webRequest.error);
        }

        _webRequest.uploadHandler.Dispose();
        _webRequest.downloadHandler.Dispose();
        _webRequest.Dispose();
    }

    private IEnumerator PutRoutine(string _uri, string _jsonData, Action<string> _onSuccess, Action<string> _onError)
    {
        if (!string.IsNullOrEmpty(userIdToken))
        {
            _uri = $"{_uri}?auth={userIdToken}";
        }

        using UnityWebRequest _webRequest = UnityWebRequest.Put(_uri, _jsonData);
        byte[] _jsonToSend = new System.Text.UTF8Encoding().GetBytes(_jsonData);
        _webRequest.uploadHandler = new UploadHandlerRaw(_jsonToSend);
        _webRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return _webRequest.SendWebRequest();

        if (_webRequest.result == UnityWebRequest.Result.Success)
        {
            _onSuccess?.Invoke(_webRequest.downloadHandler.text);
        }
        else
        {
            Debug.Log(_webRequest.error);
            _onError?.Invoke(_webRequest.error);
        }

        _webRequest.uploadHandler.Dispose();
        _webRequest.downloadHandler.Dispose();
        _webRequest.Dispose();
    }

    private IEnumerator PatchRoutine(string _uri, string _jsonData, Action<string> _onSuccess, Action<string> _onError)
    {
        if (!string.IsNullOrEmpty(userIdToken))
        {
            _uri = $"{_uri}?auth={userIdToken}";
        }

        using UnityWebRequest _webRequest = new UnityWebRequest(_uri, "PATCH");
        byte[] _jsonToSend = new System.Text.UTF8Encoding().GetBytes(_jsonData);
        _webRequest.uploadHandler = new UploadHandlerRaw(_jsonToSend);
        _webRequest.downloadHandler = new DownloadHandlerBuffer();


        yield return _webRequest.SendWebRequest();

        if (_webRequest.result == UnityWebRequest.Result.Success)
        {
            _onSuccess?.Invoke(_webRequest.downloadHandler.text);
        }
        else
        {
            Debug.Log(_webRequest.uri);
            Debug.Log(_jsonData);
            _onError?.Invoke(_webRequest.error);
        }

        _webRequest.uploadHandler.Dispose();
        _webRequest.downloadHandler.Dispose();
        _webRequest.Dispose();
    }
    
    private IEnumerator DeleteRoutine(string _uri, Action<string> _onSuccess, Action<string> _onError)
    {
        if (!string.IsNullOrEmpty(userIdToken))
        {
            _uri = $"{_uri}?auth={userIdToken}";
        }

        using UnityWebRequest _webRequest = UnityWebRequest.Delete(_uri);
        _webRequest.downloadHandler = new DownloadHandlerBuffer();

        yield return _webRequest.SendWebRequest();

        if (_webRequest.result == UnityWebRequest.Result.Success)
        {
            _onSuccess?.Invoke(_webRequest.downloadHandler.text);
        }
        else
        {
            _onError?.Invoke(_webRequest.error);
        }

        _webRequest.downloadHandler.Dispose();
        _webRequest.Dispose();
    }
}
