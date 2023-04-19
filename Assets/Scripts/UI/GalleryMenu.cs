using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

[System.Serializable]
public struct Weapon
{
    public Item body;
    public Item barrel;
    public Item extension;

    public Weapon(Item Body, Item Barrel, Item Extension)
    {
        body = Body;
        barrel = Barrel;
        extension = Extension;
    }
    public Weapon(Item Body, Item Barrel)
    {
        body = Body;
        barrel = Barrel;
        extension = null;
    }
}
public class GalleryMenu : MonoBehaviour
{
    [SerializeField]
    private Weapon[] unlockedElements;
    private int pageIndex = 0;
    private int maxPages = 0;
    private List<GameObject> spawnedWeapons = new List<GameObject>();
    private List<RectTransform> gridElements = new List<RectTransform>();

    [Header("Dependencies")]
    public Transform gridBase;
    public Transform navigation;
    public TabsButton tabPrefab;

    [Header("Tabs")]
    public Color disabledColor;
    public Color selectedColor;
    private TabsButton selectedTab;
    private List<TabsButton> tabs = new List<TabsButton>();

    private MainMenuController mainMenuController;
    private void Start()
    {
        unlockedElements = CreateAllWeapons();
        mainMenuController= GetComponentInParent<MainMenuController>();

        gridElements = gridBase.GetComponentsInChildren<Image>(true).Select(x => x.GetComponent<RectTransform>()).ToList();
        gridElements.RemoveAt(0); // GetComponentInChildren returns this element as well, which we don't want

        maxPages = Mathf.CeilToInt((float)unlockedElements.Length / gridElements.Count);

        CreateTabs(maxPages);
        PopulateGrid(0);
    }

    public void SetPlayerInput(InputManager inputManager)
    {
        inputManager.onLeftTab += PrevPage;
        inputManager.onRightTab += NextPage;
        inputManager.onCancel += Back;
    }

    private void CreateTabs(int pages)
    {
        for (int i = 1; i <= pages; i++)
        {
            TabsButton tab = Instantiate(tabPrefab, navigation.transform).GetComponent<TabsButton>();
            tab.GetComponentInChildren<TMP_Text>().text = (i).ToString();
            tabs.Add(tab);
            tab.GetComponent<Image>().color = disabledColor;
        }

        selectedTab = tabs[0];
        selectedTab.GetComponent<Image>().color = selectedColor;
    }

    /// <summary>
    /// Update the grid items with the corresponding page of items
    /// </summary>
    void PopulateGrid(int page)
    {
        // Remove previous guns
        foreach (GameObject g in spawnedWeapons)
        {
            GameObject.Destroy(g);
        }
        spawnedWeapons.Clear();

        FlexibleGridLayout gridLayout = gridBase.GetComponent<FlexibleGridLayout>();

        int unlockedElementsOffset = gridElements.Count * page;
        Vector2 gridCellSize = new Vector2((gridLayout.cellSize.x - gridLayout.spacing.x) / 2, (gridLayout.cellSize.y - gridLayout.spacing.y) / 2);

        for (int i = unlockedElementsOffset; i < gridElements.Count + unlockedElementsOffset; i++)
        {
            RectTransform gridElement = gridElements[i%gridElements.Count];
            gridElement.gameObject.SetActive(true);

            if (i < unlockedElements.Length)
            { 
                Weapon weapon = unlockedElements[i];
                GameObject gun = GunFactory.InstantiateGun(weapon.body, weapon.barrel, weapon.extension, null);
                gun.transform.localScale = Vector3.one;
                gun.transform.SetParent(gridElement);

                spawnedWeapons.Add(gun);

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
            } 
            else
            {
                // We are out of displayable weapons, disable objects
                gridElement.gameObject.SetActive(false);
            }    
        }
    }

    public void NextPage(InputAction.CallbackContext ctx)
    {
        if(pageIndex == maxPages - 1)
        {
            pageIndex = 0;
        }
        else
        {
            pageIndex++;
        }

        selectedTab.GetComponent<Image>().color = disabledColor;
        selectedTab = tabs[pageIndex];
        selectedTab.GetComponent<Image>().color = selectedColor;

        PopulateGrid(pageIndex);
    }

    public void PrevPage(InputAction.CallbackContext ctx)
    {
        if (pageIndex == 0)
        {
            pageIndex = maxPages - 1;
        }
        else
        {
            pageIndex--;
        }

        selectedTab.GetComponent<Image>().color = disabledColor;
        selectedTab = tabs[pageIndex];
        selectedTab.GetComponent<Image>().color = selectedColor;

        PopulateGrid(pageIndex);
    }

    public void Back(InputAction.CallbackContext ctx)
    {
        mainMenuController.ReturnToMainMenu();
    }

    /// <summary>
    /// Creates and returns an array of all possible weapon combinations
    /// </summary>
    private Weapon[] CreateAllWeapons()
    {
        StaticInfo staticInfo = StaticInfo.Singleton;

        List<Weapon> weaponList = new List<Weapon>();
        for(int body = 0; body < staticInfo.Bodies.Count; body++)
        {
            for(int barrel = 0; barrel < staticInfo.Barrels.Count; barrel++)
            {
                // Add the extensionless variant of the weapon
                weaponList.Add(new Weapon(staticInfo.Bodies[body], staticInfo.Barrels[barrel]));

                for (int extension = 0; extension < staticInfo.Extensions.Count; extension++)
                {
                    // Add the weapon
                    weaponList.Add(new Weapon(staticInfo.Bodies[body], staticInfo.Barrels[barrel], staticInfo.Extensions[extension]));
                }
            }
        }
        return weaponList.ToArray();
    }
}

