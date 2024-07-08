using UnityEngine;
using UnityEngine.VFX;

public class CollectableChip : MonoBehaviour
{
    [SerializeField]
    private GameObject chipModel;

    [SerializeField]
    private VisualEffect effect;

    [SerializeField]
    private AudioGroup soundEffect;
    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        // Animate spin and bounce
        LeanTween.sequence()
            .append(LeanTween.moveLocalY(chipModel, 0.08f, 0.5f)
                .setLoopPingPong().setEaseInOutCubic())
            .insert(LeanTween.rotateAroundLocal(chipModel, Vector3.up, chipModel.transform.eulerAngles.y + 360, 1.5f)
                .setLoopType(LeanTweenType.easeInOutCubic).setLoopCount(-1));
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.gameObject.TryGetComponent<PlayerManager>(out var player))
            return;

        player.identity.UpdateChip(1);

        soundEffect.Play(audioSource);
        GetComponent<Collider>().enabled = false;
        chipModel.GetComponent<Renderer>().enabled = false;
        effect.enabled = false;
        MatchController.Singleton?.RemoveChip(this);
        Destroy(gameObject, audioSource.clip.length);
    }
}
