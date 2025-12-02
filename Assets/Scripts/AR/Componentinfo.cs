using UnityEngine;
using UnityEngine.UI;

public class ComponentInfo : MonoBehaviour
{
    public static ComponentInfo Instance;

    public GameObject panel;
    public Text titleText;
    public Text descriptionText;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void ShowInfo(ARManager.ComponentData data)
    {
        panel.SetActive(true);
        titleText.text = data.name;
        descriptionText.text = data.description;
    }

    public void HideInfo()
    {
        panel.SetActive(false);
    }
}
