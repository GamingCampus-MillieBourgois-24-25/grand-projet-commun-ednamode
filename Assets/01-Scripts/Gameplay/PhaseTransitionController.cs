using Unity.Netcode;
using UnityEngine;

public class PhaseTransitionController : NetworkBehaviour
{
    public void TransitionToPhase(string panelToHide, string panelToShow)
    {
        TransitionClientRpc(panelToHide, panelToShow);
    }

    [ClientRpc]
    private void TransitionClientRpc(string hidePanel, string showPanel)
    {
        UIManager.Instance.HidePanelByName(hidePanel);
        UIManager.Instance.ShowPanel(showPanel);
    }
}