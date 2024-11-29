using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RulesHandler : MonoBehaviour
{
    [SerializeField] private List<TextMeshProUGUI> textDisplays;
    [SerializeField] private TMP_InputField search;
    [SerializeField] private Button showNext;
    [SerializeField] private Button showPrevious;
    [SerializeField] private RectTransform contentHolder;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform mainRect;

    [SerializeField] private Color defaultColor;
    [SerializeField] private Color foundKeyWordColor;
    [SerializeField] private Color currentKeyWordColor;

    private List<RulesSearchResult> searchResults = new();
    private string searchedKeyword;
    private int currentSearchIndex = -1;
    private Dictionary<TextMeshProUGUI, string> originalTexts = new();

    private void OnEnable()
    {
        search.onDeselect.AddListener(Search);
        showNext.onClick.AddListener(ShowNext);
        showPrevious.onClick.AddListener(ShowPrevious);
    }

    private void OnDisable()
    {
        search.onDeselect.RemoveListener(Search);
        showNext.onClick.RemoveListener(ShowNext);
        showPrevious.onClick.RemoveListener(ShowPrevious);
    }

    private void Awake()
    {
        foreach (var _textDisplay in textDisplays)
        {
            originalTexts[_textDisplay] = _textDisplay.text;
        }
    }

    private void Search(string _text)
    {
        if (!string.IsNullOrEmpty(searchedKeyword))
        {
            MarkAllKeywords(searchedKeyword, defaultColor);
        }

        searchedKeyword = _text;

        if (string.IsNullOrEmpty(searchedKeyword))
        {
            return;
        }

        SearchKeywords(searchedKeyword);
    }

    private void SearchKeywords(string _keyWord)
    {
        searchResults.Clear();
        currentSearchIndex = -1;
        foreach (var _textDisplay in textDisplays)
        {
            var _foundWordInTextCounter = 0;
            int _index = _textDisplay.text.IndexOf(_keyWord, StringComparison.OrdinalIgnoreCase);
            while (_index != -1)
            {
                searchResults.Add(new RulesSearchResult { TextDisplay = _textDisplay, Index = _foundWordInTextCounter });
                _foundWordInTextCounter++;
                _index = _textDisplay.text.IndexOf(_keyWord, _index + _keyWord.Length, StringComparison.OrdinalIgnoreCase);
            }
        }

        if (searchResults.Count != 0)
        {
            ShowNext();
        }

        bool _showButtons = searchResults.Count > 1;
        showNext.gameObject.SetActive(_showButtons);
        showPrevious.gameObject.SetActive(_showButtons);
    }


    private void ShowNext()
    {
        if (searchResults.Count == 0)
        {
            return;
        }

        currentSearchIndex = (currentSearchIndex + 1) % searchResults.Count;
        SnapContentToResult(searchResults[currentSearchIndex]);
    }

    private void ShowPrevious()
    {
        if (searchResults.Count == 0)
        {
            return;
        }

        if (currentSearchIndex <= 0)
        {
            currentSearchIndex = searchResults.Count - 1;
        }
        else
        {
            currentSearchIndex--;
        }

        SnapContentToResult(searchResults[currentSearchIndex]);
    }

    private void SnapContentToResult(RulesSearchResult _result)
    {
        Vector2 _anchoredPosition = contentHolder.anchoredPosition;
        Vector2 _newAnchoredPosition = (Vector2)scrollRect.transform.InverseTransformPoint(contentHolder.position) -
                                       (Vector2)scrollRect.transform.InverseTransformPoint(_result.TextDisplay.transform.position);
        _newAnchoredPosition.x = _anchoredPosition.x;
        float _contentHeight = contentHolder.sizeDelta.y;
        float _viewportHeight = scrollRect.viewport.rect.height;
        float _lowerBound = 0f;
        float _upperBound = _contentHeight - _viewportHeight;
        _newAnchoredPosition.y = Mathf.Clamp(_newAnchoredPosition.y, _lowerBound, _upperBound);
        contentHolder.anchoredPosition = _newAnchoredPosition;

        ColorTheKeyWord(_result);
    }

    private void ColorTheKeyWord(RulesSearchResult _result)
    {
        MarkAllKeywords(searchedKeyword, foundKeyWordColor);

        TextMeshProUGUI _textDisplay = _result.TextDisplay;

        int _foundWordInTextCounter = 0;
        int _index = _textDisplay.text.IndexOf(searchedKeyword, StringComparison.OrdinalIgnoreCase);

        while (_index != -1)
        {
            if (_foundWordInTextCounter == _result.Index)
            {
                string _colorTagStartForCurrent = $"<color=#{ColorUtility.ToHtmlStringRGBA(currentKeyWordColor)}>";
                string _colorTagEnd = "</color>";

                string _beforeKeyword = _textDisplay.text.Substring(0, _index);
                string _keyword = _textDisplay.text.Substring(_index, searchedKeyword.Length);
                string _afterKeyword = _textDisplay.text.Substring(_index + searchedKeyword.Length);

                _textDisplay.text = _beforeKeyword + _colorTagStartForCurrent + _keyword + _colorTagEnd + _afterKeyword;
                break;
            }

            _foundWordInTextCounter++;
            _index = _textDisplay.text.IndexOf(searchedKeyword, _index + searchedKeyword.Length, StringComparison.OrdinalIgnoreCase);
        }
    }
    
    private void MarkAllKeywords(string _keyWord, Color _color)
    {
        string _colorTagStart = $"<color=#{ColorUtility.ToHtmlStringRGBA(_color)}>";
        string _colorTagEnd = "</color>";

        foreach (var _textDisplay in textDisplays)
        {
            string _originalText = originalTexts[_textDisplay];
            string _updatedText = _originalText;
            int _index = _updatedText.IndexOf(_keyWord, StringComparison.OrdinalIgnoreCase);

            while (_index != -1)
            {
                string _actualKeyword = _updatedText.Substring(_index, _keyWord.Length);
                string _coloredKeyword = _colorTagStart + _actualKeyword + _colorTagEnd;

                _updatedText = _updatedText.Substring(0, _index) + _coloredKeyword + _updatedText.Substring(_index + _keyWord.Length);
                _index = _updatedText.IndexOf(_keyWord, _index + _coloredKeyword.Length, StringComparison.OrdinalIgnoreCase);
            }

            _textDisplay.text = _updatedText;
        }
    }
}