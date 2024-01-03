using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AmmoBox : MonoBehaviour
{
    [SerializeField] private float respawnTime = 30;
    [SerializeField] private GameObject boxModel;
    [SerializeField] private bool shouldAlwaysSpawn = false;

    private Collider collider;
    private MeshRenderer renderer;
    private static List<AmmoBox> ammoBoxes = new List<AmmoBox>();

    private void Start()
    {
        collider = GetComponent<Collider>();
        renderer = boxModel.GetComponent<MeshRenderer>();
        if (!shouldAlwaysSpawn)
            StartCoroutine(CheckForCollectors());

        // Animate spin and bounce
        LeanTween.sequence()
            .append(LeanTween.moveLocalY(boxModel, 0.08f, 0.5f)
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

    public static AmmoBox GetClosestAmmoBox(Vector3 from)
    {
        return ammoBoxes.Aggregate(
            (ammoBox, next) =>
            Vector3.Distance(from, next.transform.position) < Vector3.Distance(from, ammoBox.transform.position) ? next : ammoBox);
    }

    private IEnumerator CheckForCollectors()
    {
        // Wait for two frames before checking, since the collectors have to wait one frame first
        yield return null;
        yield return null;
        var collectors = FindObjectsOfType<AmmoBoxCollector>();
        var noAmmoBoxBodiesArePresent = !collectors.Any(c => c.CanReload);
        if (noAmmoBoxBodiesArePresent)
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator RespawnAfterTimeout()
    {
        yield return new WaitForSeconds(respawnTime);
        collider.enabled = true;
        renderer.enabled = true;
        ammoBoxes.Add(this);
    }

    private void OnTriggerStay(Collider intruder)
    {
        if (!intruder.gameObject.TryGetComponent<AmmoBoxCollector>(out var collector))
            return;

        if (!collector.CanReload || collector.HasFullMagazine)
            return;

        collector.Reload();

        collider.enabled = false;
        renderer.enabled = false;
        ammoBoxes.Remove(this);
        StartCoroutine(RespawnAfterTimeout());
    }
}
