using UnityEngine;

public class PyramidSwitch : MonoBehaviour
{
    public Material onMaterial;
    public Material offMaterial;
    public GameObject myButton;
    private bool m_State;
    private GameObject m_Area;
    private PyramidArea m_AreaComponent;

    private Vector3 defaultPosition;
    private Quaternion defaultRotation;

    void Start()
    {
        m_Area = gameObject.transform.parent.gameObject;
        m_AreaComponent = m_Area.GetComponent<PyramidArea>();

        // Store default position and rotation
        defaultPosition = transform.position;
        defaultRotation = transform.rotation;
    }

    public bool GetState()
    {
        return m_State;
    }

    public void ResetSwitchToDefault()
    {
        // Reset to default position and rotation
        transform.position = defaultPosition;
        transform.rotation = defaultRotation;
        m_State = false;
        tag = "switchOff";
        myButton.GetComponent<Renderer>().material = offMaterial;
    }

    public void ResetSwitch(int spawnAreaIndex)
    {
        // Use this method if you want to keep the random placement functionality
        m_AreaComponent.PlaceObject(gameObject, spawnAreaIndex);
        m_State = false;
        tag = "switchOff";
        transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        myButton.GetComponent<Renderer>().material = offMaterial;
    }

    void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("agent") && !m_State)
        {
            myButton.GetComponent<Renderer>().material = onMaterial;
            m_State = true;
            tag = "switchOn";
            other.gameObject.GetComponent<PyramidAgent>().OnSwitchActivated();
        }
    }
}
