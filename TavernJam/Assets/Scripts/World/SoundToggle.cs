using UnityEngine;
using UnityEngine.UI;

public class SoundToggle : MonoBehaviour
{
    public Toggle soundToggle;

    private void Start()
    {
        // Set the initial state based on PlayerPrefs (or any other method you prefer)
        soundToggle.isOn = PlayerPrefs.GetInt("SoundOn", 1) == 1;

        // Add listener for when the toggle value changes
        soundToggle.onValueChanged.AddListener(ToggleSound);
    }

    private void ToggleSound(bool isSoundOn)
    {
        // Set the global sound state based on the toggle value
        AudioListener.volume = isSoundOn ? 1 : 0;

        // Save the state to PlayerPrefs (or any other method you prefer)
        PlayerPrefs.SetInt("SoundOn", isSoundOn ? 1 : 0);
        PlayerPrefs.Save();
    }
}
