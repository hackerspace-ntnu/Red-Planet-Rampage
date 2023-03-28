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

        for (int i = unlockedElementsOffset; i < gridElements.Count + unlockedElementsOffset; i++)
        {
            RectTransform gridElement = gridElements[i%gridElements.Count];
            if (i < unlockedElements.Length)
            {
                gridElement.gameObject.SetActive(true);

                Weapon weapon = unlockedElements[i];
                GameObject gun = GunFactory.InstantiateGun(weapon.body, weapon.barrel, weapon.extension, gridElement);

                //gun = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //gun.transform.SetParent(gridElement.transform);
                //gun.transform.localPosition = Vector3.zero + Vector3.up * gridLayout.cellSize.y / 4f;

                Bounds bounds = new Bounds();
                foreach (var renderer in gun.GetComponentsInChildren<Renderer>())
                {
                    renderer.gameObject.layer = LayerMask.NameToLayer("UI");
                    bounds.Encapsulate(renderer.bounds);
                    //TODO This does not really work figure out why.
                }

                // Scale the weapon so it fits within the UI 
                float maxSize = (gridLayout.cellSize.x - gridLayout.spacing.x) / 2;
                float sizeFactor = maxSize / bounds.size.x /  10 ;
                gun.transform.localScale = new Vector3(sizeFactor / gun.transform.lossyScale.x, sizeFactor / gun.transform.lossyScale.y, sizeFactor / gun.transform.lossyScale.z);

                Debug.Log("MaxSize: " + maxSize.ToString());
                Debug.Log("MinExtents: " + bounds.min);
                Debug.Log("MaxExtents: " + bounds.max);
                Debug.Log("SizeFactor: " + sizeFactor.ToString());

                // Center the weapon on the UI element
                gun.transform.Translate(Vector3.left * (bounds.size.x) / 20);

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

