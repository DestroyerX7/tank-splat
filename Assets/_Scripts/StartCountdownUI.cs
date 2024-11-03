using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class StartCountdownUI : MonoBehaviour
{
    private Animator _anim;
    private TextMeshProUGUI _countdownText;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _countdownSound;

    private void Awake()
    {
        _anim = GetComponent<Animator>();
        _countdownText = GetComponent<TextMeshProUGUI>();
    }

    public async Task WaitForCountdown(float startTime)
    {
        float countdownTimer = startTime;
        float previousTime = 0;
        gameObject.SetActive(true);
        _audioSource.PlayOneShot(_countdownSound);

        while (countdownTimer > 0 && this != null)
        {
            int roundedTime = Mathf.CeilToInt(countdownTimer);
            _countdownText.text = roundedTime.ToString();

            if (roundedTime != previousTime)
            {
                _anim.SetTrigger("Bounce");
            }

            previousTime = roundedTime;
            countdownTimer -= Time.deltaTime;
            await Task.Yield();
        }

        if (this != null)
        {
            gameObject.SetActive(false);
        }
    }
}
