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
    private TMP_Text label;

    [SerializeField]
    private Image background;

    [SerializeField]
    private Color selectedColor;

    [SerializeField]
    private Color deselectedColor;

    private Vector3 originalItemHolderPosition;

    private List<Item> items;

    private int selectedIndex = 0;
    public Item SelectedItem => items.Count > 0 ? items[selectedIndex] : null;

    private void Start()
    {
        originalItemHolderPosition = itemHolder.localPosition;
        background.color = deselectedColor;
    }

    public void SetItems(List<Item> items, Item initialItem)
    {
        this.items = items;

        if (type == AugmentType.Extension)
        {
            items.Insert(0, null);
        }

        // After lots of tweaking, this formula seems to translate size correctly
        var transformedSpacing = spacing * (itemModelScale / spacing) / 2f;


        // Instantiate all items at offset spacing down from holder
        for (var i = 0; i < items.Count; i++)
        {
            if (items[i] == null) continue;

            var rotation = Quaternion.Euler(new Vector3(0, 90, -20));

            var instance = Instantiate(items[i].augment, itemHolder.position + Vector3.down * spacing * i, rotation, itemHolder);
            instance.transform.ScaleAndParent(itemModelScale, itemHolder);
            instance.transform.position = itemHolder.position + Vector3.down * transformedSpacing * i;

            Augment.DisableInstance(instance, type);
        }

        selectedIndex = items.IndexOf(initialItem);

        // For extensions, we may not find any available item
        if (selectedIndex < 0) selectedIndex = 0;

        ChangeItemDisplayed(selectedIndex, false);
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
        // +count to avoid negative numbers from modulo (C# is an oddball here)
        ChangeItemDisplayed((selectedIndex - 1 + items.Count) % items.Count);
    }

    public void Next()
    {
        ChangeItemDisplayed((selectedIndex + 1) % items.Count);
    }

    private void ChangeItemDisplayed(int nextIndex, bool animate = true)
    {
        if (nextIndex < 0 || nextIndex >= items.Count)
        {
            return;
        }

        selectedIndex = nextIndex;

        label.text = SelectedItem != null ? SelectedItem.displayName : "None";

        var position = originalItemHolderPosition + Vector3.up * selectedIndex * spacing;
        if (animate)
        {
            LeanTween.moveLocal(itemHolder.gameObject, position, .2f).setEaseInOutBounce();
        }
        else
        {
            itemHolder.localPosition = position;
        }
    }
}
