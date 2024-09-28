using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class RebindSaveLoad : MonoBehaviour
{
    public InputActionAsset actions;

    public void OnEnable()
    {
        var rebinds = PlayerPrefs.GetString("rebinds");
        if (!string.IsNullOrEmpty(rebinds))
            actions.LoadBindingOverridesFromJson(rebinds);
        GetComponentsInChildren<KeyMappingButton>().ToList().ForEach(button => button.UpdateBindingDisplay());
    }

    public void OnDisable()
    {
        var rebinds = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("rebinds", rebinds);
    }
}
