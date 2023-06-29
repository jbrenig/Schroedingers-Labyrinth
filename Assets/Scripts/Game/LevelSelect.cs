using System.Collections.Generic;
using Lib;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.UIElements.Experimental;

namespace Game
{
    public class LevelSelect : MonoBehaviour
    {
        public Text levelText;
        public Button btnNext;
        public Button btnPrevious;
        
        public int pageOffset = 660;
        public float animationTime = 0.5f;
        public GameObject pageHolder;
        public GameObject prefab;

        public ScoreStarUi star1;
        public ScoreStarUi star2;
        public ScoreStarUi star3;

        private readonly List<GameObject> _pageList = new List<GameObject>();

        private int CurrentTargetX => pageOffset * _currentPage;
        private float _currentTimePassed = 0;
        private float _startingPosition = 0;
        private float _targetPosition = 0;
        private int _currentPage = 0;
        private bool _animationSettled = true;
        
        void Start()
        {
            _pageList.Clear();
            var levelCount = Levels.Story.LevelList.Length;


            for (int i = 0; i < levelCount; i++)
            {
                var page = GameObject.Instantiate(prefab, new Vector3 (pageOffset * i,0,0), Quaternion.identity);
                var entry = page.GetComponent<LevelSelectEntry>();
                entry.image.sprite = Resources.Load<Sprite>("levelselect/screenshot-level" + i);
                var button = page.GetComponent<Button>();
                button.onClick.AddListener(BtnStartPressed);
                button.interactable = false;
                _pageList.Add(page);

                page.transform.SetParent(pageHolder.transform, false);
            }
            
            UpdateUiToPage();
        }

        private void StartAnimation()
        {
            _animationSettled = false;
            _startingPosition = pageHolder.transform.localPosition.x;
            _targetPosition = -pageOffset * _currentPage;
            _currentTimePassed = 0;
        }

        public void BtnNextPressed()
        {
            _pageList[_currentPage].GetComponent<Button>().interactable = false;
            _currentPage++;
            _currentPage = Mathf.Min(_currentPage, _pageList.Count - 1);
            StartAnimation();
            UpdateUiToPage();
        }

        public void BtnPreviousPressed()
        {
            _pageList[_currentPage].GetComponent<Button>().interactable = false;
            _currentPage--;
            _currentPage = Mathf.Max(_currentPage, 0);
            StartAnimation();
            UpdateUiToPage();
        }

        public void BtnMainMenuPressed()
        {
            SceneManager.LoadSceneAsync(Levels.MainMenu);
        }
        
        public void BtnStartPressed()
        {
            SceneManager.LoadScene(Levels.Story.LevelList[_currentPage]);
        }

        private void UpdateUiToPage()
        {
            _pageList[_currentPage].GetComponent<Button>().interactable = true;
            btnPrevious.interactable = _currentPage != 0;
            btnNext.interactable = _currentPage < _pageList.Count - 1;

            if (_currentPage == 0)
            {
                levelText.text = "Intro";
            }
            else
            {
                levelText.text = "Level " + _currentPage;
            }

            var stars = UserPreferences.GetLevelStars(Levels.Story.GetStoryLevelSceneNumber(_currentPage));
            star1.SetVisibility(stars > 0 ? ScoreStarUi.Visibility.Enabled : ScoreStarUi.Visibility.Disabled);
            star2.SetVisibility(stars > 1 ? ScoreStarUi.Visibility.Enabled : ScoreStarUi.Visibility.Disabled);
            star3.SetVisibility(stars > 2 ? ScoreStarUi.Visibility.Enabled : ScoreStarUi.Visibility.Disabled);
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.RightArrow)) BtnNextPressed();
            if (Input.GetKeyDown(KeyCode.LeftArrow)) BtnPreviousPressed();
            if (Input.GetKeyDown(KeyCode.Return)) BtnStartPressed();
            if (Input.GetAxis("Submit") > 0.9f) BtnStartPressed();
            if (Input.GetKeyDown(KeyCode.Escape)) BtnMainMenuPressed();
            
            if (!_animationSettled)
            {
                _currentTimePassed += Time.deltaTime;
                var progress = _currentTimePassed / animationTime;
                var eased = Easing.OutQuad(progress);
                var pos = pageHolder.transform.localPosition;
                pos.x = Mathf.Lerp(_startingPosition, _targetPosition, eased);
                pageHolder.transform.localPosition = pos;

                if (_currentTimePassed >= animationTime)
                {
                    _animationSettled = true;
                }
            }
        }
    }
}
