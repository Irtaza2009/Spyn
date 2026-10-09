using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class MatchManager : MonoBehaviour
{
    [Header("Players")]
    [SerializeField] private SpynerController player1;
    [SerializeField] private SpynerController player2;

    [Header("Player 1 UI")]
    [SerializeField] private TMP_Text p1WinsText;
    [SerializeField] private TMP_Text p1SpinText;
    [SerializeField] private TMP_Text p1SpeedText;
    [SerializeField] private TMP_Text p1AttackText;

    [Header("Player 2 UI")]
    [SerializeField] private TMP_Text p2WinsText;
    [SerializeField] private TMP_Text p2SpinText;
    [SerializeField] private TMP_Text p2SpeedText;
    [SerializeField] private TMP_Text p2AttackText;

    [Header("Round UI")]
    [SerializeField] private TMP_Text winnerText;
    [SerializeField] private Button restartButton;

    private int p1Wins;
    private int p2Wins;
    private bool roundOver;

    public bool IsRoundOver => roundOver;

    private Vector3 p1StartPosition;
    private Quaternion p1StartRotation;
    private Vector3 p2StartPosition;
    private Quaternion p2StartRotation;

    private Rigidbody p1Rb;
    private Rigidbody p2Rb;

    private void Awake()
    {
        p1Rb = player1.GetComponent<Rigidbody>();
        p2Rb = player2.GetComponent<Rigidbody>();

        p1StartPosition = new Vector3(0,1,2);
        p1StartRotation = Quaternion.Euler(0f, 0f, 0f);
        p2StartPosition = new Vector3(0,1,-2);
        p2StartRotation = Quaternion.Euler(0f, 0f, 0f);

        restartButton.gameObject.SetActive(false);
        winnerText.gameObject.SetActive(false);

        UpdateWinsUI();
    }

    private void Update()
    {
        UpdatePlayerUI();

        if (roundOver)
            return;

        // If a spinner runs out of spin and topples, it loses.
        bool p1Dead = player1.IsDead();
        bool p2Dead = player2.IsDead();

        if (p1Dead && !p2Dead)
            EndRound(2);
        else if (p2Dead && !p1Dead)
            EndRound(1);
        else if (p1Dead && p2Dead)
            EndRound(0);
    }

    private void UpdatePlayerUI()
    {
        p1SpinText.text = $"Spin: {player1.GetSpinSpeed():F1}";
        p2SpinText.text = $"Spin: {player2.GetSpinSpeed():F1}";

        p1SpeedText.text = $"Speed: {player1.GetMovementSpeed():F1}";
        p2SpeedText.text = $"Speed: {player2.GetMovementSpeed():F1}";

        p1AttackText.text = $"Attack: {player1.GetAttackMultiplier():F1}x";
        p2AttackText.text = $"Attack: {player2.GetAttackMultiplier():F1}x";
    }

    private void UpdateWinsUI()
    {
        p1WinsText.text = $"Wins: {p1Wins}";
        p2WinsText.text = $"Wins: {p2Wins}";
    }

    public void PlayerFell(SpynerController fallenPlayer)
    {
        if (roundOver || fallenPlayer == null)
            return;

        if (fallenPlayer == player1)
            EndRound(2);
        else if (fallenPlayer == player2)
            EndRound(1);
    }

    private void EndRound(int winner)
    {
        if (roundOver)
            return;

        roundOver = true;
        Time.timeScale = 0f;

        if (winner == 1)
        {
            p1Wins++;
            winnerText.text = "Player 1 Wins!";
        }
        else if (winner == 2)
        {
            p2Wins++;
            winnerText.text = "Player 2 Wins!";
        }
        else
        {
            winnerText.text = "Draw!";
        }

        UpdateWinsUI();
        winnerText.gameObject.SetActive(true);
        restartButton.gameObject.SetActive(true);
    }

    public void RestartRound()
    {
        // Ensure physics is running, even if the round was paused.
        Time.timeScale = 1f;
        roundOver = false;

        ResetPlayer(player1, p1Rb, p1StartPosition, p1StartRotation);
        ResetPlayer(player2, p2Rb, p2StartPosition, p2StartRotation);

        winnerText.gameObject.SetActive(false);
        restartButton.gameObject.SetActive(false);
    }

    private void ResetPlayer(
        SpynerController player,
        Rigidbody rb,
        Vector3 position,
        Quaternion rotation)
    {
        
        rb.isKinematic = true;

        player.transform.SetPositionAndRotation(position, rotation);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.position = position;
        rb.rotation = rotation;

        rb.linearDamping = 1f;
        rb.angularDamping = 0f;

        rb.isKinematic = false;

        player.Launch();
    }
}