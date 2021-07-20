using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using EasyMobile;

public class UIManager : MonoBehaviour {

	[Header("GUI Components")]
	public GameObject mainMenuGui;
	public GameObject pauseGui, gameplayGui, gameOverGui;

	public GameState gameState;
	[SerializeField] Image ImgMuted;
    [SerializeField] Image ImgUnMuted;
	private bool isMuted;
	bool clicked;

	// Use this for initialization
	void Start () {
		mainMenuGui.SetActive(true);
		pauseGui.SetActive(false);
		gameplayGui.SetActive(false);
		gameOverGui.SetActive(false);
		gameState = GameState.MENU;
	}

    void Update()
    {
		if (Input.GetMouseButtonDown(0) && gameState == GameState.MENU && !clicked)
		{
			if (IsButton())
				return;

			AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
			ShowGameplay();
		}
		else if (Input.GetMouseButtonUp(0) && clicked && gameState == GameState.MENU)
			clicked = false;
	}


    //show main menu
    public void ShowMainMenu()
	{
		Advertising.ShowBannerAd(BannerAdPosition.Bottom);
		ScoreManager.Instance.ResetCurrentScore();
		clicked = true;
		mainMenuGui.SetActive(true);
		pauseGui.SetActive(false);
		gameplayGui.SetActive(false);
		gameOverGui.SetActive(false);
		if (gameState == GameState.PAUSED)
			Time.timeScale = 1;

		gameState = GameState.MENU;
		AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
		GameManager.Instance.ClearScene();
	}

	public void OnMuteButtonPress(){
        if(isMuted==false){
            isMuted = true;
            AudioListener.pause = true;
        }
        else {
            isMuted = false;     
            AudioListener.pause = false;
        }
        UpdateMuteButton();
    }
    private void UpdateMuteButton() {
        if (isMuted == false){
            ImgMuted.enabled = true;
            ImgUnMuted.enabled = false;
        }
        else {
            ImgMuted.enabled = false;
            ImgUnMuted.enabled = true;
        }

    }

    //show pause menu
    public void ShowPauseMenu()
	{
		if (gameState == GameState.PAUSED)
			return;

		pauseGui.SetActive(true);
		Time.timeScale = 0;
		gameState = GameState.PAUSED;
		AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
	}

	//hide pause menu
	public void HidePauseMenu()
	{
		pauseGui.SetActive(false);
		Time.timeScale = 1;
		gameState = GameState.PLAYING;
		AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
	}

	//show gameplay gui
	public void ShowGameplay()
	{
		Advertising.HideBannerAd();
		mainMenuGui.SetActive(false);
		pauseGui.SetActive(false);
		gameplayGui.SetActive(true);
		gameOverGui.SetActive(false);
		gameState = GameState.PLAYING;
		AudioManager.Instance.PlayEffects(AudioManager.Instance.buttonClick);
		AudioManager.Instance.PlayMusic(AudioManager.Instance.gameMusic);
	}

	public void OnSharingButtonPressed(){
        Sharing.ShareURL("https://apps.apple.com/us/app/queenst-match/id1569178122");
        Debug.Log("Shared");
    }

	public void OnRatingButtonPressed(){
        StoreReview.RequestRating();
        Debug.Log("Button Rating Request");
    }
	//show game over gui
	public void ShowGameOver()
	{
		bool isReady = Advertising.IsInterstitialAdReady();

// Show it if it's ready
		if (isReady)
		{
    		Advertising.ShowInterstitialAd();
		}
		mainMenuGui.SetActive(false);
		pauseGui.SetActive(false);
		gameplayGui.SetActive(false);
		gameOverGui.SetActive(true);
		gameState = GameState.GAMEOVER;
		AudioManager.Instance.PlayMusic(AudioManager.Instance.menuMusic);
	}

	//check if user click any menu button
	public bool IsButton()
	{
		bool temp = false;

		PointerEventData eventData = new PointerEventData(EventSystem.current)
		{
			position = Input.mousePosition
		};

		List<RaycastResult> results = new List<RaycastResult>();
		EventSystem.current.RaycastAll(eventData, results);

		foreach (RaycastResult item in results)
		{
			temp |= item.gameObject.GetComponent<Button>() != null;
		}

		return temp;
	}
}
