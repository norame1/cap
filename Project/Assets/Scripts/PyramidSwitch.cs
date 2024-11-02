using UnityEngine;

public class PyramidSwitch : MonoBehaviour
{
    public bool switchOn = false;

    public void ResetSwitch(Vector3 localPosition)
    {
        transform.localPosition = localPosition;
        switchOn = false;
        gameObject.SetActive(true);
    }

    public bool GetState()
    {
        return switchOn;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("player"))
        {
            switchOn = true;
            gameObject.SetActive(false);
            other.gameObject.GetComponent<PyramidAgent>().OnSwitchActivated();
        }
    }
}
