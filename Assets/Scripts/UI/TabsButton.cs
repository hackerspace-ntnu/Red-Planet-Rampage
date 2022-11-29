using UnityEngine;
using UnityEngine.UI;


[RequireComponent(typeof(Image))]
public class TabsButton : MonoBehaviour
{
    public TabGroup tabGroup;

    public Image background;

    public GameObject tabContent;

    public Selectable firstItem;
}
