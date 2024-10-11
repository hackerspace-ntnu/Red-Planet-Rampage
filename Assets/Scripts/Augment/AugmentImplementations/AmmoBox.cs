using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.VFX;

public class AmmoBox : MonoBehaviour
{
    [SerializeField] private bool ignoredByAI = false;
    [SerializeField] private float respawnTime = 30;
    [SerializeField] private GameObject boxModel;
    [SerializeField] private VisualEffect effect;

    [SerializeField]
    private AudioGroup soundEffect;
    private AudioSource audioSource;

    private Collider collider;
    private MeshRenderer renderer;

    private static readonly List<AmmoBox> ammoBoxes = new();

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        collider = GetComponent<Collider>();
        renderer = boxModel.GetComponent<MeshRenderer>();

        // Animate spin and bounce
        LeanTween.sequence()
            .append(LeanTween.moveLocalY(boxModel, 0.4f, 0.5f)
                .setLoopPingPong().setEaseInOutCubic())
            .insert(LeanTween.rotateAroundLocal(boxModel, Vector3.forward, boxModel.transform.eulerAngles.y + 360, 1.5f)
                .setLoopType(LeanTweenType.easeInOutCubic).setLoopCount(-1));
    }

    private void OnEnable()
    {
        ammoBoxes.Add(this);
    }

    private void OnDisable()
    {
        ammoBoxes.Remove(this);
    }

    public static AmmoBox GetClosestAmmoBox(Vector3 from) =>
        GetClosestAmmoBox(ammoBoxes, from);

    public static AmmoBox GetClosestAmmoBoxForAI(Vector3 from) =>
        GetClosestAmmoBox(ammoBoxes.Where(b => !b.ignoredByAI).ToList(), from);

    private static AmmoBox GetClosestAmmoBox(List<AmmoBox> boxes, Vector3 from)
    {
        if (boxes.Count == 0)
            return null;
        return boxes.Aggregate(
            (ammoBox, next) =>
            Vector3.Distance(from, next.transform.position) < Vector3.Distance(from, ammoBox.transform.position) ? next : ammoBox);
    }

    private IEnumerator RespawnAfterTimeout()
    {
        yield return new WaitForSeconds(respawnTime);
        collider.enabled = true;
        renderer.enabled = true;
        effect.enabled = true;
        ammoBoxes.Add(this);
    }

    private void OnTriggerEnter(Collider intruder) =>
        HandleCollision(intruder);

    private void OnTriggerStay(Collider intruder) =>
        HandleCollision(intruder);

    private void HandleCollision(Collider intruder)
    {
        if (!intruder.gameObject.TryGetComponent<AmmoBoxCollector>(out var collector))
            return;

        if (!collector.CanReload || collector.HasFullMagazine)
            return;

        collector.Reload();
        soundEffect.Play(audioSource);

        collider.enabled = false;
        renderer.enabled = false;
        effect.enabled = false;
        ammoBoxes.Remove(this);
        StartCoroutine(RespawnAfterTimeout());

        if (intruder.gameObject.TryGetComponent<AIManager>(out var ai))
            ai.DestinationTarget = null;
    }
}
