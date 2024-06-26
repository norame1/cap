using UnityEngine;

public class PyramidSwitch : MonoBehaviour
{
    public Material onMaterial;
    public Material offMaterial;
    public GameObject myButton;
    private bool m_State;
    private GameObject m_Area;
    private PyramidArea m_AreaComponent;

    public bool GetState()
    {
        return m_State;
    }

    void Start()
    {
        m_Area = gameObject.transform.parent.gameObject;
        m_AreaComponent = m_Area.GetComponent<PyramidArea>();
    }

    public void ResetSwitch(int spawnAreaIndex)
    {
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
