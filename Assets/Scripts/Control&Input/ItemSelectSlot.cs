using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TransformExtensions;

public class ItemSelectSlot : MonoBehaviour
{
    [SerializeField]
    private AugmentType type;

    [SerializeField]
    private float spacing = 4;

    [SerializeField]
    private float itemModelScale = 50;

    [SerializeField]
    private Transform itemHolder;

    [SerializeField]
    private GameObject[] itemSlots;

    [SerializeField]
    private TMP_Text label;

    [SerializeField]
    private Image background;

    [SerializeField]
    private Color selectedColor;

    [SerializeField]
    private Color deselectedColor;

    [SerializeField]
    private GameObject missingObject;

    private List<Item> items;
    private List<(GameObject, Vector3 originalPosition)> augmentModels = new List<(GameObject, Vector3 originalPosition)>();

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
        {
            items.Insert(0, null);
        }
        int itemCount = items.Count;
        if (itemCount < 6)
        {
            for (int i = 0; i < 6 - itemCount; i++)
            {
                items.Insert(itemCount + i, items[Mod((itemCount + i), itemCount)]);
            }
        }

        // After lots of tweaking, this formula seems to translate size correctly
        var transformedSpacing = spacing * (itemModelScale / spacing) / 2f;


        // Instantiate all items at offset spacing down from holder
        for (var i = 0; i < items.Count; i++)
        {
            // Item is probably an empty extension slot!
            if (items[i] == null) 
            {
                var missing = Instantiate(missingObject, itemHolder.position + Vector3.down * spacing * i, itemHolder.rotation, itemHolder);
                missing.transform.position = itemHolder.position + Vector3.down * transformedSpacing * i;
                augmentModels.Add((missing, missing.transform.localPosition));
                continue;
            }
            var rotation = Quaternion.Euler(new Vector3(0, 90, -20));
            var instance = Instantiate(items[i].augment, itemHolder.position + Vector3.down * spacing * i, rotation, itemHolder);
            instance.transform.ScaleAndParent(itemModelScale, itemHolder);
            instance.transform.position = itemHolder.position + Vector3.down * transformedSpacing * i;

            Augment.DisableInstance(instance, type);
            augmentModels.Add((instance, instance.transform.localPosition));
        }

        selectedIndex = items.IndexOf(initialItem);

        // For extensions, we may not find any available item
        if (selectedIndex < 0) selectedIndex = 0;

        ChangeItemDisplayed(selectedIndex);
    }

    public void Select(out Item item)
    {
        background.color = selectedColor;
        item = SelectedItem;
    }

    public void Deselect()
    {
        background.color = deselectedColor;
    }

    public void Previous(out Item item)
    {
        // +count to avoid negative numbers from modulo (C# is an oddball here)
        ChangeItemDisplayed((selectedIndex - 1 + items.Count) % items.Count);
        item = SelectedItem;
    }

    public void Next(out Item item)
    {
        ChangeItemDisplayed((selectedIndex + 1) % items.Count);
        item = SelectedItem;
    }

    private void ChangeItemDisplayed(int nextIndex)
    {
        if (nextIndex < 0 || nextIndex >= items.Count)
        {
            return;
        }

        selectedIndex = nextIndex;

        label.text = SelectedItem != null ? SelectedItem.displayName : "None";
        for (int i = 0; i < augmentModels.Count; i++)
        {
            augmentModels[i].Item1.SetActive(false);
            if (i == selectedIndex || i == Mod((selectedIndex + 1), itemSlots.Length) || i == Mod((selectedIndex - 1), itemSlots.Length))
            {
                augmentModels[i].Item1.SetActive(true);
            }
            LeanTween.move(augmentModels[i].Item1, itemSlots[Mod((selectedIndex - i), itemSlots.Length)].transform.position, .2f).setEaseInOutBounce();
        }
    }

    public static int Mod(float a, float b)
    {
        float c = a % b;
        if ((c < 0 && b > 0) || (c > 0 && b < 0))
        {
            c += b;
        }
        return (int) c;
    }
}
