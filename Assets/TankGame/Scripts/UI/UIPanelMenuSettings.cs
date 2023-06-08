using FishNet;
using FishNet.Managing.Scened;
#if EOS
using FishNet.Plugins.FishyEOS.Util;
#endif
using System;
namespace EOSLobbyTest
{
    public class UIPanelMenuSettings : UIPanelSettings
    {
        public void Back()
        {
            UIPanelManager.Instance.HidePanel<UIPanelMenuSettings>(false);
        }
    }
}