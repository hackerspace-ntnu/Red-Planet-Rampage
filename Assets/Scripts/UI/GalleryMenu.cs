using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Newtonsoft.Json;
using Unity.VisualScripting;
using UnityEditor;
namespace UI
{
    public class GalleryMenu : MonoBehaviour
    {

        private List<RectTransform> gridElements;
        private List<GameObject> unlockedElements;
        private int pageIndex = 0;

        public Transform grid;
        public GameObject gridElement;
        private void Start()
        {
            for(int i = 0; i < 15; i++)
                Instantiate(gridElement, grid);
            
            gridElements = grid.GetComponentsInChildren<RectTransform>().ToList();
            gridElements.RemoveAt(0);

            DisplayIndex(pageIndex);
            
            gridElements[0].transform.Translate(new Vector3(0,1,0 ));
            DisplayIndex(pageIndex);
        }
        
        /// <summary>
        /// Update the grid elements
        /// </summary>
        void DisplayIndex(int index)
        {
            int firstElement = gridElements.Count * index;
            for (int i = 0; i < gridElements.Count; i++)
            {
                gridElements[i].GetComponentInChildren<TMP_Text>().text = (i).ToString();
            }
            
        }
    }
}
