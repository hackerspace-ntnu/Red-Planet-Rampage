using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterMenuLoadout : MonoBehaviour
{
    [SerializeField]
    private TMP_Text toptext;

    [SerializeField]
    private Image background;

    [SerializeField]
    private GameObject model;

    public float rotationSpeed = 5f;

    public void SetupPreview(string name, Color color)
    {
        // Change the panel color
        background.color = color;

        // Apply player name
        toptext.text = name;

        // Change Character color
        model.GetComponentInChildren<SkinnedMeshRenderer>().material.color = color;
    }

    public void Update()
    {
        model.transform.Rotate(new Vector3(0, rotationSpeed, 0) * Time.deltaTime);
    }
}
