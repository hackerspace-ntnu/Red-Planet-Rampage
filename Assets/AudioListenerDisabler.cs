using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class AudioListenerDisabler : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        DisableAudioListeners(SceneManager.GetActiveScene(), SceneManager.GetActiveScene());
        SceneManager.activeSceneChanged += DisableAudioListeners;
    }
    private void DisableAudioListeners(Scene current, Scene next)
    {
        if (GetComponent<PlayerInput>().user.id == 1)
        {

            // Disable other Audio listeners
            foreach (AudioListener listener in FindObjectsOfType<AudioListener>(true))
            {
                listener.enabled = false;
            }

            GetComponentInChildren<AudioListener>().enabled = true;
        }
    }
}
