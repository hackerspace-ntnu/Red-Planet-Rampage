using System.Collections.Generic;
using UnityEngine;
using TMPro;
using TransformExtensions;
using System.Collections;
using System.Linq;
using OperatorExtensions;

public class ItemSelectSlot : MonoBehaviour
{
    [SerializeField]
    private AugmentType type;

    [SerializeField]
    private float itemModelScale = 50;

    [SerializeField]
    private Transform itemHolder;

    [SerializeField]
    private GameObject[] itemSlots;

    [SerializeField]
    private TMP_Text label;

    [SerializeField]
    private SpriteRenderer background;

    [SerializeField]
    private Color selectedColor;

    [SerializeField]
    private Color deselectedColor;

    [SerializeField]
    private GameObject missingObject;

    [SerializeField]
    private GameObject tileHolder;

    private List<Item> items;
    private List<(GameObject instance, Vector3 originalPosition, Vector3 offset)> augmentModels = new List<(GameObject instance, Vector3 originalPosition, Vector3 offset)>();

    private int selectedIndex = 0;
    public Item SelectedItem => items.Count > 0 ? items[selectedIndex] : null;

    private void Start()
    {
        background.color = deselectedColor;
    }

    public void SetItems(List<Item> items, Item initialItem)
    {
        this.items = items;

        if (type == AugmentType.Extension)
            items.Insert(0, null);

        int itemCount = items.Count;
        if (itemCount < 6)
            for (int i = 0; i < 6 - itemCount; i++)
                items.Insert(itemCount + i, items[(itemCount + i).Mod(itemCount)]);

        // Instantiate all items at offset spacing down from holder
        for (var i = 0; i < items.Count; i++)
        {
            // Item is probably an empty extension slot!
            if (items[i] == null)
            {
                var missing = Instantiate(missingObject, Vector3.zero, itemHolder.rotation, itemHolder);
                missing.transform.localPosition = Vector3.zero;
                augmentModels.Add((missing, missing.transform.localPosition, Vector3.zero));
                continue;
            }

            var scale = itemModelScale;
            // Items are tiiiny when we are at 3-4 splitscreens, so scale them up.
            if (MatchController.Singleton.HumanPlayers.Count() > 2)
                scale *= 3;

            // Item is an augment
            var rotation = Quaternion.Euler(new Vector3(0, 90, -20));
            var instance = Instantiate(items[i].augment, Vector3.zero, rotation, itemHolder);
            instance.transform.ScaleAndParent(scale, itemHolder);
            Transform itemCenter;
            if (items[i].augmentType == AugmentType.Body)
            {
                itemCenter = instance.GetComponent<GunBody>().midpoint;
            }
            else
            {
                itemCenter = instance.GetComponent<Augment>().midpoint;
            }
            var offset = instance.transform.position - itemCenter.transform.position;
            instance.transform.localPosition = itemSlots[i].transform.position + offset / scale;

            Augment.DisableInstance(instance, type);
            augmentModels.Add((instance, itemSlots[i].transform.position, offset / scale));
        }

        selectedIndex = items.IndexOf(initialItem);

        // For extensions, we may not find any available item
        if (selectedIndex < 0) selectedIndex = 0;

        StartCoroutine(WaitAndSetWeapons());
    }

    private IEnumerator WaitAndSetWeapons()
    {
        // Skip a frame to let the splitscreen change UI aspect ratio and scale first
        yield return null;
        ChangeItemDisplayed(selectedIndex);
    }

    public void Select()
    {
        background.color = selectedColor;
    }

    public void Deselect()
    {
        background.color = deselectedColor;
    }

    public void Previous()
    {
        ChangeItemDisplayed((selectedIndex - 1).Mod(items.Count));
        LeanTween.moveLocal(tileHolder, itemSlots[5].transform.localPosition, .2f)
            .setEaseInOutBounce()
            .setOnComplete(() => tileHolder.transform.localPosition = Vector3.zero);
    }

    public void Next()
    {
        ChangeItemDisplayed((selectedIndex + 1).Mod(items.Count));
        LeanTween.moveLocal(tileHolder, itemSlots[3].transform.localPosition, .2f)
            .setEaseInOutBounce()
            .setOnComplete(() => tileHolder.transform.localPosition = Vector3.zero);
    }

    private void ChangeItemDisplayed(int nextIndex)
    {
        if (nextIndex < 0 || nextIndex >= items.Count)
            return;


        selectedIndex = nextIndex;

        label.text = SelectedItem != null ? SelectedItem.displayName : "None";
        for (int i = 0; i < augmentModels.Count; i++)
        {
            var model = augmentModels[i];
            model.instance.SetActive(false);
            if (i == selectedIndex || i == (selectedIndex + 1).Mod(itemSlots.Length) || i == (selectedIndex - 1).Mod(itemSlots.Length))
                model.instance.SetActive(true);

            LeanTween.move(augmentModels[i].Item1, itemSlots[(selectedIndex - i).Mod(itemSlots.Length)].transform.position + model.offset, .2f).setEaseInOutBounce();
        }
    }
}
