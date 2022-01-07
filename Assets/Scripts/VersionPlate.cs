using UnityEngine;

public class VersionPlate : MonoBehaviour
{
    private TMPro.TMP_Text textContent;

    // Start is called before the first frame update
    void Start()
    {
        textContent = GetComponentInChildren<TMPro.TMP_Text>();
    }

    public void SetText(string information)
    {
        textContent.text = information;
    }
}
