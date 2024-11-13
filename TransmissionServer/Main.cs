using System;
using GTANetworkAPI;

namespace TransmissionServer
{
    public class Main : Script
    {
        [Command("mt", "Manual transmission toggle")]
        public void ManualTransmissionToggle(Player player)
        {
            NAPI.ClientEvent.TriggerClientEvent(player, "ToggleManual");
        }
    }
}
