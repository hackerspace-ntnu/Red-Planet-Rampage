using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.IO;
using Unity.VisualScripting;

[System.Serializable]
    public struct Weapon
    {
        public Item body;
        public Item barrel;
        public Item extension;
    }
public class GalleryMenu : MonoBehaviour
{
    [SerializeField]
    private Weapon[] unlockedElements;
    private int pageIndex = 0;
    private int maxPages = 0;

    public Transform gridBase;
    public TabGroup tabGroup;
    public TabsButton tabPrefab;

    private void Start()
    {
        List<RectTransform> gridElements = gridBase.GetComponentsInChildren<Image>().Select(x => x.GetComponent<RectTransform>()).ToList();
        maxPages = Mathf.CeilToInt(unlockedElements.Length /gridElements.Count);

        CreateTabs(maxPages);
        PopulateGrid(0);
    }

    private void CreateTabs(int pages)
    {
        for(int i = 1; i < pages; i++)
        { 
            TabsButton tab = Instantiate(tabPrefab, tabGroup.transform).GetComponent<TabsButton>();
            tab.GetComponentInChildren<TMP_Text>().text = i.ToString();
            tabGroup.Subscribe(tab);
        }
    }

    /// <summary>
    /// Update the grid elements
    /// </summary>
    void PopulateGrid(int page)
    {
        FlexibleGridLayout gridLayout = gridBase.GetComponent<FlexibleGridLayout>();
        List<RectTransform> gridElements = gridBase.GetComponentsInChildren<Image>().Select(x => x.GetComponent<RectTransform>()).ToList();
        gridElements.RemoveAt(0); // GetComponentInChildren returns this element as well, which we don't want

        int unlockedElementsOffset = gridElements.Count * page;
        Vector2 gridCellSize = new Vector2((gridLayout.cellSize.x - gridLayout.spacing.x) / 2, (gridLayout.cellSize.y - gridLayout.spacing.y) / 2);

        for (int i = unlockedElementsOffset; i < gridElements.Count + unlockedElementsOffset; i++)
        {
            RectTransform gridElement = gridElements[i%gridElements.Count];
            if (i < unlockedElements.Length)
            {
                gridElement.gameObject.SetActive(true);

                Weapon weapon = unlockedElements[i];
                GameObject gun = GunFactory.InstantiateGun(weapon.body, weapon.barrel, weapon.extension, gridElement, Vector3.one);

                Bounds bounds = new Bounds();
                foreach (var renderer in gun.GetComponentsInChildren<Renderer>())
                {
                    renderer.gameObject.layer = LayerMask.NameToLayer("UI");
                    bounds.Encapsulate(renderer.bounds);
                }

                // Scale the weapon so it fits within the UI 
                float scaleFactor = gridCellSize.x / bounds.size.z;

                gun.transform.localScale = new Vector3(scaleFactor / gun.transform.lossyScale.x, scaleFactor / gun.transform.lossyScale.y, scaleFactor / gun.transform.lossyScale.z);

                // Recalculate bounds
                foreach (var renderer in gun.GetComponentsInChildren<Renderer>())
                {
                    renderer.gameObject.layer = LayerMask.NameToLayer("UI");
                    bounds.Encapsulate(renderer.bounds);
                }

                // Center the gun on the grid cell
                gun.transform.localPosition = new Vector3(0 - bounds.center.z * gridCellSize.x / 4, gridCellSize.y / 3, -bounds.extents.x);

                // Rotate the weapon to the correct angle
                gun.transform.Rotate(Vector3.up * 90);

                // Add the weapon name
                string name = GunFactory.GetGunName(weapon.body, weapon.barrel, weapon.extension);
                gun.name = name;
                gridElement.GetComponentInChildren<TMP_Text>().text = name;
            } else
            {
                // We are out of displayable weapons, disable objects
                gridElement.gameObject.SetActive(false);
            }    
        }
    }
}

