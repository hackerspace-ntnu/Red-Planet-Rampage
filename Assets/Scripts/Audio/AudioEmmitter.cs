using UnityEngine;

public class AudioEmmitter : MonoBehaviour
{
    [SerializeField]
    private AudioSource source;


    private void Awake()
    {
#if UNITY_EDITOR
        Debug.Assert(source != null, "An AudioSource is needed!");
#endif
    }
    
    public void EmmitSound(AudioGroup audioType)
    {
        audioType.PlayRandomFrom(source);
    }
}
