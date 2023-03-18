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

    }

    private void CreateTabs(int pages)
    {
        for(int i = 1; i < pages; i++)
        {
            GameObject content = Instantiate(gridBase.gameObject, transform);
            PopulateGridAtIndex(i, content);
            TabsButton tab = Instantiate(tabPrefab, tabGroup.transform).GetComponent<TabsButton>();
            tab.GetComponentInChildren<TMP_Text>().text = i.ToString();
            tabGroup.Subscribe(tab);
        }
    }

    /// <summary>
    /// Update the grid elements
    /// </summary>
    void PopulateGridAtIndex(int index, GameObject grid)
    {
        FlexibleGridLayout gridLayout = gridBase.GetComponent<FlexibleGridLayout>();
        List<RectTransform> gridElements = grid.GetComponentsInChildren<Image>().Select(x => x.GetComponent<RectTransform>()).ToList();
        gridElements.RemoveAt(0);

        int firstElement = gridElements.Count * index;

        for (int i = firstElement; i < gridElements.Count + firstElement; i++)
        {
            if (i < unlockedElements.Length)
            {
                Weapon weapon = unlockedElements[i];
                GameObject gun = GunFactory.InstantiateGun(weapon.body.augment, weapon.barrel.augment, weapon.extension.augment, gridElements[i]);
                //gun = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //gun.transform.SetParent(gridElements[i].transform);
                //gun.transform.localPosition = Vector3.zero + Vector3.up * gridLayout.cellSize.y / 4f;
                float renderedSize = default;
                foreach (var renderer in gun.GetComponentsInChildren<Renderer>())
                {
                    renderer.gameObject.layer = LayerMask.NameToLayer("UI");
                    renderedSize += Mathf.Max(renderer.localBounds.size.x, renderer.localBounds.size.y, renderer.localBounds.size.z);
                }

                // Scale the weapon so it fits within the UI 
                float maxSize = (gridLayout.cellSize.x - gridLayout.spacing.x) / 4;
                float sizeFactor = maxSize / renderedSize;
                gun.transform.localScale = Vector3.one * sizeFactor;

                Debug.Log("MaxSize: " + maxSize.ToString());
                Debug.Log("RederedSize: " + renderedSize.ToString());
                Debug.Log("SizeFactor: " + sizeFactor.ToString());

                // Center the weapon on the UI element
                //gun.transform.Translate(Vector3.left * (renderedSize.x + renderedSize.y) / 2);

                // Rotate the weapon to the correct angle
                gun.transform.Rotate(Vector3.up * 90);

                // Add the weapon name
                string name = GunFactory.GetGunName(weapon.body, weapon.barrel, weapon.extension);
                gun.name = name;
                gridElements[i].GetComponentInChildren<TMP_Text>().text = name;
            } else
            {
                // We are out of displayable weapons, disable objects
                gridElements[i].gameObject.SetActive(false);
            }
            
        }

    }
}

