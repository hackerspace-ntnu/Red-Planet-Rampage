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
    private List<(GameObject instance, Vector3 originalPosition, Vector3 offset)> augmentModels = new();

    private int slotIndexOffset = 2;

    private int selectedIndex = 0;
    public Item SelectedItem => items.Count > 0 ? items[selectedIndex] : null;

    private void Awake()
    {
        slotIndexOffset = Mathf.FloorToInt(itemSlots.Length / 2);
        background.color = deselectedColor;
    }

    public void SetItems(IEnumerable<Item> availableItems, Item initialItem)
    {
        items = availableItems.ToList();

        if (type == AugmentType.Extension)
            items.Insert(0, null);

        int itemCount = items.Count;
        // Repeat until we have enough to fill all slots
        while (items.Count < itemSlots.Length)
            items.AddRange(items.GetRange(0, itemCount));


        selectedIndex = items.IndexOf(initialItem);
        // For extensions, we may not find any available item
        if (selectedIndex < 0) selectedIndex = 0;


        // Instantiate all items at offset spacing down from holder
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            // Item is probably an empty extension slot!
            if (item == null)
            {
                var missing = Instantiate(missingObject, Vector3.zero, itemHolder.rotation, itemHolder);
                missing.transform.localPosition = Vector3.zero;
                augmentModels.Add((missing, missing.transform.localPosition, Vector3.zero));
                continue;
            }

            var scale = itemModelScale;
            // Items are tiiiny when we are at 3-4 splitscreens, so scale them up.
            if (PlayerInputManagerController.Singleton.LocalPlayerInputs.Count > 2)
                scale *= 3;

            // Item is an augment, so it needs to be instantiated
            var rotation = Quaternion.Euler(new Vector3(0, 90, -20));
            var instance = Instantiate(item.augment, Vector3.zero, rotation, itemHolder);
            instance.transform.ScaleAndParent(scale, itemHolder);

            Transform itemCenter;
            if (item.augmentType == AugmentType.Body)
            {
                itemCenter = instance.GetComponent<GunBody>().midpoint;
            }
            else
            {
                itemCenter = instance.GetComponent<Augment>().midpoint;
            }

            var offset = instance.transform.position - itemCenter.transform.position;
            var slotPosition = itemSlots[SlotIndexForItem(i)].transform.position;
            instance.transform.localPosition = slotPosition + offset / scale;

            Augment.DisableInstance(instance, type);
            augmentModels.Add((instance, slotPosition, offset / scale));
        }

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
        ChangeItemDisplayedByDelta(-1);
    }

    public void Next()
    {
        ChangeItemDisplayedByDelta(+1);
    }

    private void ChangeItemDisplayedByDelta(int delta)
    {
        ChangeItemDisplayed((selectedIndex + delta).Mod(items.Count));
        LeanTween.moveLocal(tileHolder, itemSlots[slotIndexOffset - delta].transform.localPosition, .2f)
            .setEaseInOutBounce()
            .setOnComplete(() => tileHolder.transform.localPosition = Vector3.zero);
    }

    private int SlotIndexForItem(int i) =>
        Mathf.Clamp((i - selectedIndex + slotIndexOffset).Mod(items.Count), 0, itemSlots.Length - 1);

    private void ChangeItemDisplayed(int nextIndex)
    {
        if (nextIndex < 0 || nextIndex >= items.Count)
            return;

        selectedIndex = nextIndex;

        label.text = SelectedItem != null ? SelectedItem.displayName : "None";

        for (int i = 0; i < items.Count; i++)
        {
            var (instance, _, offset) = augmentModels[i];

            var isSelectedOrNeighbour = i == selectedIndex || i == (selectedIndex + 1).Mod(items.Count) || i == (selectedIndex - 1).Mod(items.Count);
            instance.SetActive(isSelectedOrNeighbour);

            var slotIndex = SlotIndexForItem(i);
            LeanTween.move(instance, itemSlots[slotIndex].transform.position + offset, .2f).setEaseInOutBounce();
        }
    }
}
