using UnityEngine;
using UnityEngine.SceneManagement;
using EZCameraShake;

public class Rocket : MonoBehaviour {

    Rigidbody rb;
    AudioSource audioSource;

    [SerializeField] float mainThrust = 100f;
    [SerializeField] float rcsThrust = 100f;
    [SerializeField] float loadLevelDelay = 2f;

    [SerializeField] AudioClip mainEngine;
    [SerializeField] AudioClip nextLevel;
    [SerializeField] AudioClip explosion;

    [SerializeField] ParticleSystem rocketJet;
    [SerializeField] ParticleSystem success;
    [SerializeField] ParticleSystem crush;

    bool isTouchDevice = false;
    bool isTranscending = false;
    bool collisionsDisabled = false;

    void Start()
    {
        if (SystemInfo.deviceType == DeviceType.Handheld)
        {
            isTouchDevice = true;
        }

        rb = GetComponent<Rigidbody>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        if (!isTranscending)
        {
            if (isTouchDevice)
            {
                RespondToTouchThrustInput();
                RespondToTouchRotateInput();
            }
            else
            {
                RespondToThrustInput();
                RespondToRotateInput();
            }
        }

        if (Debug.isDebugBuild)
        {
            RespondToDebugKeys();
        }
    }

    private void RespondToDebugKeys()
    {
        if (Input.GetKeyDown(KeyCode.L))
        {
            LoadNextLevel();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            collisionsDisabled = !collisionsDisabled;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (!isTranscending && !collisionsDisabled)
        {
            switch (collision.gameObject.tag)
            {
                case "Friendly":
                    // do nothing
                    break;

                case "Finish":
                    StartSuccesSequence();
                    break;

                default:
                    StartDeathSequence();
                    break;
            }
        }
    }

    private void StartSuccesSequence()
    {
        isTranscending = true;
        audioSource.Stop();
        audioSource.PlayOneShot(nextLevel);
        success.Play();
        Invoke("LoadNextLevel", loadLevelDelay);
    }

    private void StartDeathSequence()
    {
        isTranscending = true;
        CameraShaker.Instance.ShakeOnce(4f, 7f, 0f, 3f);
        audioSource.Stop();
        audioSource.PlayOneShot(explosion);
        crush.Play();
        Invoke("LoadFirstLevel", loadLevelDelay);
    }

    private void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (currentSceneIndex == SceneManager.sceneCountInBuildSettings - 1)
        {
            nextSceneIndex = 0;
        }
        
        SceneManager.LoadScene(nextSceneIndex);
    }

    private void LoadFirstLevel()
    {
        SceneManager.LoadScene(0);
    }

    private void RespondToThrustInput()
    {  
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyThrusting();
        }
        else
        {
            StopApplyingThrust();
        }
    }

    private void RespondToTouchThrustInput()
    {
        foreach (Touch touch in Input.touches)
        {
            if (touch.position.x > Screen.width / 2)
            {
                if (touch.phase != TouchPhase.Ended)
                {
                    ApplyThrusting();
                }
                else
                {
                    StopApplyingThrust();
                }
            }
        }
    }

    private void StopApplyingThrust()
    {
        audioSource.Stop();
        rocketJet.Stop();
    }

    private void ApplyThrusting()
    {
        rb.AddRelativeForce(Vector3.up * mainThrust * Time.deltaTime); // add force relatively to coordinates
        if (!audioSource.isPlaying)
        {
            audioSource.PlayOneShot(mainEngine);
        }
        rocketJet.Play();
    }

    private void RespondToRotateInput()
    {
        rb.angularVelocity = Vector3.zero;

        float rotationThisFrame = rcsThrust * Time.deltaTime;

        if (Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D))
        {
            transform.Rotate(Vector3.forward * rotationThisFrame);
        }
        else if (Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.A))
        {
            transform.Rotate(-Vector3.forward * rotationThisFrame);
        }
    }
    private void RespondToTouchRotateInput()
    {
        rb.angularVelocity = Vector3.zero;

        float rotationThisFrame = rcsThrust * Time.deltaTime;

        foreach (Touch touch in Input.touches)
        {
            if (touch.position.x > Screen.width / 2 - Screen.width / 4 && touch.position.x < Screen.width / 2)
            {
                transform.Rotate(-Vector3.forward * rotationThisFrame);
            }
            else if (touch.position.x < Screen.width / 4)
            {
                transform.Rotate(Vector3.forward * rotationThisFrame);
            }
        }
    }
}
