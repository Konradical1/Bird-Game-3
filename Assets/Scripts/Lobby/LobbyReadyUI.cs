using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple UI bridge that forwards button/toggle changes to the LobbyManager.
/// Add this to a UI GameObject in the lobby scene and hook up the references.
/// </summary>
public class LobbyReadyUI : MonoBehaviour
{
    [SerializeField] private LobbyManager lobbyManager;
    [SerializeField] private Toggle readyToggle;

    private void Awake()
    {
        if (readyToggle)
        {
            readyToggle.onValueChanged.AddListener(OnToggleChanged);
        }
    }

    private void OnDestroy()
    {
        if (readyToggle)
        {
            readyToggle.onValueChanged.RemoveListener(OnToggleChanged);
        }
    }

    public void SetLobbyManager(LobbyManager manager)
    {
        lobbyManager = manager;
    }

    public void SetReady(bool ready)
    {
        if (readyToggle)
        {
            readyToggle.isOn = ready;
        }

        OnToggleChanged(ready);
    }

    private void OnToggleChanged(bool ready)
    {
        if (lobbyManager)
        {
            lobbyManager.RequestSetReady(ready);
        }
    }
}
