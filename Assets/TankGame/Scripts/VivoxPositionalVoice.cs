using FishNet.Object;
#if Vivox
using VivoxUnity;
#endif

namespace EOSLobbyTest
{
    public class VivoxPositionalVoice : NetworkBehaviour
    {
#if Vivox
        private void Update()
        {
            if (IsOwner && VivoxManager.Instance != null && VivoxManager.Instance.TransmittingSession != null && VivoxManager.Instance.TransmittingSession.AudioState == ConnectionState.Connected)
            {
                VivoxManager.Instance.TransmittingSession.Set3DPosition(transform.position, transform.position, transform.forward, transform.up);
            }
        }
#endif
    }
}