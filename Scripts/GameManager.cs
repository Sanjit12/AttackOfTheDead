using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum gameStatus{
	next,play,gameover,win
};

public class GameManager : Singleton<GameManager> {

	[SerializeField]
	private int totalWaves = 10;
	[SerializeField]
	private Text totalMoneyLbl;
	[SerializeField]
	private Text currentWaveLbl;
	[SerializeField]
	private Text totalEscapedLbl;
	[SerializeField]
	private GameObject spawnPoint;
	[SerializeField]
	private Enemy[] enemies;
	[SerializeField]
	private int maxEnemiesOnScreen;
	[SerializeField]
	private int totalEnemies=1;
	[SerializeField]
	private int enemiesPerSpawn;
	[SerializeField]
	private Text playButtonLbl;
	[SerializeField]
	private Button playButton;

	private int waveNummber = 0;
	private int totalMoney = 10;
	private int totalEscaped = 0;
	private int roundEscaped = 0;
	private int totalKilled = 0;
	private int enemiesToSpawn = 0;
	private gameStatus currentState = gameStatus.play;
	private AudioSource audioSource;

	public List<Enemy> EnemyList = new List<Enemy> ();

	const float spawnDelay = 0.5f;

	public int TotalEscaped{
		get{
			return totalEscaped;
		}
		set{
			totalEscaped = value;
		}
	}

	public int RoundEscaped{
		get{
			return roundEscaped;
		}
		set{
			roundEscaped = value;
		}
	}

	public int TotalKilled{
		get{
			return totalKilled;
		}
		set{
			totalKilled = value;
		}
	}

	public int TotalMoney{
		get{
			return totalMoney;
		}
		set{
			totalMoney = value;
			totalMoneyLbl.text = totalMoney.ToString();
		}
	}

	public AudioSource AudioSource{
		get{
			return audioSource;
		}
	}

	void Start () {
		//spawnEnemy ();
		//StartCoroutine (spawn ());
		playButton.gameObject.SetActive (false);
		audioSource = GetComponent<AudioSource> ();
		showMenu ();
		Time.timeScale = 0.0f;
	}
	void Update(){
		handleEscape ();
	}
	void spawnEnemy(){
		if (enemiesPerSpawn > 0 && EnemyList.Count < totalEnemies) {
			for (int i=0;i < enemiesPerSpawn;i++){
				if ( EnemyList.Count < maxEnemiesOnScreen){
					Enemy newEnemy = Instantiate(enemies[Random.Range(0,enemiesToSpawn)]) as Enemy;
					newEnemy.transform.position = spawnPoint.transform.position;
				}
			}
		}
	}

	IEnumerator spawn(){
		if (enemiesPerSpawn > 0 && EnemyList.Count < totalEnemies) {
			for (int i=0;i < enemiesPerSpawn;i++){
				if (EnemyList.Count < maxEnemiesOnScreen){
					Enemy newEnemy = Instantiate(enemies[Random.Range(0,enemiesToSpawn)]) as Enemy;
					newEnemy.transform.position = spawnPoint.transform.position;
				}
			}
			yield return new WaitForSeconds(spawnDelay);
			StartCoroutine(spawn());
		}
	}

	public void RegisterEnemy(Enemy enemy){
		EnemyList.Add (enemy);
	}

	public void DestroyAllEnemy(){
		foreach(Enemy enemy in EnemyList){
			Destroy (enemy.gameObject);
		}
		EnemyList.Clear ();
	}

	public void UnregisterEnemy(Enemy enemy){
		EnemyList.Remove (enemy);
		Destroy (enemy.gameObject);
	}

	public void addMoney(int amount){
		TotalMoney += amount;
	}

	public void subtractMoney(int amount){
		TotalMoney -= amount;
	}

	public void isWaveOver(){
		totalEscapedLbl.text = "Escaped " + TotalEscaped +"/10";
		if ((RoundEscaped + TotalKilled) == totalEnemies) {
			if(waveNummber <= enemies.Length){
				enemiesToSpawn = waveNummber;
			}
			setCurrentGameState();
			showMenu();
		}
		Debug.Log (RoundEscaped + " " + TotalKilled+" "+totalEnemies);
	}

	public void setCurrentGameState(){
		if (TotalEscaped >= 10) {
			currentState = gameStatus.gameover;
		} else if (waveNummber == 0 && (TotalKilled + RoundEscaped) == 0) {
			currentState = gameStatus.play;
		} else if (waveNummber >= totalWaves) {
			currentState = gameStatus.win;
		} else {
			currentState = gameStatus.next;
		}
	}

	public void showMenu(){
		switch (currentState) {
		case gameStatus.gameover:
			playButtonLbl.text = "Play Again!";
			audioSource.PlayOneShot(SoundManager.Instance.GameOver);
			break;
		case gameStatus.next:
			playButtonLbl.text = "Next Wave";
			break;
		case gameStatus.play:
			playButtonLbl.text = "Play";
			break;
		case gameStatus.win:
			playButtonLbl.text = "Play";
			break;
		}
		playButton.gameObject.SetActive (true);
	}

	public void playButtonPressed(){
		switch (currentState) {
		case gameStatus.next:
			waveNummber +=1;
			totalEnemies = waveNummber+1;
			break;
		default:
			totalEnemies = 1;
			TotalEscaped = 0;
			TotalMoney = 10;
			enemiesToSpawn=0;
			waveNummber=0;
			DestroyAllEnemy();
			TowerManager.Instance.DestroyAllTower();
			TowerManager.Instance.renameTagsBuildSites();
			totalMoneyLbl.text = TotalMoney.ToString();
			totalEscapedLbl.text = "Escaped "+TotalEscaped +"/10";
			audioSource.PlayOneShot(SoundManager.Instance.NewGame);
			break;
		}
		DestroyAllEnemy ();
		TotalKilled = 0;
		RoundEscaped = 0;
		currentWaveLbl.text = "Wave " + (waveNummber+1);
		StartCoroutine (spawn ());
		playButton.gameObject.SetActive(false);
		Time.timeScale = 1f;
	}

	private void handleEscape(){
		if (Input.GetKeyDown (KeyCode.Escape)) {
			TowerManager.Instance.disableDragSprite();
			TowerManager.Instance.towerButtonPressed = null;
		}
	}
}


