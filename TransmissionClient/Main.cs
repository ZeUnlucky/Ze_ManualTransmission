using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using RAGE;
using RAGE.Elements;


namespace TransmissionClient
{
    public class Main : Events.Script
    {
        readonly float[] gearInfo = new float[] { 3.77778f, 12.33333f, 20.8889f, 30.4444f, 37.7778f, 55.1111f, 75.2222f, 100.8889f };
        public static int gear = 0;
        int maxGears = -1;
        float speed;
        bool isManual = false;

        public Main()
        {
            Events.Tick += CheckForCarAndKeyPress;
            Events.OnPlayerLeaveVehicle += LeaveVehicle;
            Events.Add("ToggleManual", ToggleManual);
        }

        public void CheckForCarAndKeyPress(List<Events.TickNametagData> nametags)
        {
            if (isManual)
            {
                Vehicle veh = Player.LocalPlayer.Vehicle;
                if (veh != null)
                {
                    if (veh.GetPedInSeat(-1, 0) == Player.LocalPlayer.GetPedIndexFromIndex())
                    {
                        if (maxGears == -1)
                            maxGears = veh.GetHandlingInt("nInitialDriveGears");


                        if (RAGE.Game.Pad.IsControlPressed(27, (int)RAGE.Game.Control.CharacterWheel))
                        {
                            RAGE.Game.Pad.DisableControlAction(27, (int)RAGE.Game.Control.VehicleAccelerate, true);
                            RAGE.Game.Pad.DisableControlAction(27, (int)RAGE.Game.Control.VehicleBrake, true);
                            if (RAGE.Game.Pad.IsControlJustPressed(27, (int)RAGE.Game.Control.PhoneUp))
                            {
                                gear++;
                                if (gear > maxGears) gear = maxGears;
                            }
                            if (RAGE.Game.Pad.IsControlJustPressed(27, (int)RAGE.Game.Control.PhoneDown))
                            {
                                gear--;
                                if (gear < -1) gear = -1;
                            }
                        }
                        speed = veh.GetSpeed();
                        if (gear > 0)
                        {
                            if (speed <= 0.5f)
                                RAGE.Game.Pad.DisableControlAction(27, (int)RAGE.Game.Control.VehicleBrake, true);
                            else
                                ChangeGear(veh);

                        }
                        else if (gear == 0)
                        {
                            if (speed < 0.5f && speed > -0.5f)
                            {
                                RAGE.Game.Pad.DisableControlAction(27, (int)RAGE.Game.Control.VehicleBrake, true);
                                RAGE.Game.Pad.DisableControlAction(27, (int)RAGE.Game.Control.VehicleAccelerate, true);
                            }
                            else if (speed > 0.5f)
                                RAGE.Game.Pad.DisableControlAction(27, (int)RAGE.Game.Control.VehicleAccelerate, true);
                            else
                                RAGE.Game.Pad.DisableControlAction(27, (int)RAGE.Game.Control.VehicleBrake, true);
                        }
                        else if (gear == -1)
                        {
                            if (speed >= 0)
                                RAGE.Game.Pad.DisableControlAction(27, (int)RAGE.Game.Control.VehicleAccelerate, true);
                        }
                    }
                }
                else
                {
                    gear = 0;
                    maxGears = -1;
                }
            }
        }

        public void ChangeGear(Vehicle veh)
        {
            speed = veh.GetSpeed();
            float torque = speed / (1.1f * gearInfo[gear - 1]);
            if (speed < 2)
                torque = 1 - (gear - 1) / maxGears;
            if (torque > 1f)
                torque = 1 - speed / (1.1f * gearInfo[gear]);
            veh.SetEngineTorqueMultiplier(torque);
        }

        public void LeaveVehicle(Vehicle veh, int seat)
        {
            if (veh != null)
                veh.SetEngineTorqueMultiplier(1.0f);
        }

        public void ToggleManual(object[] args)
        {
            isManual = !isManual;
            if (!isManual)
            {
                Vehicle veh = Player.LocalPlayer.Vehicle;
                if (veh != null)
                    veh.SetEngineTorqueMultiplier(1.0f);

            }
            Chat.Output("Manual transmission mode is now turned " + (isManual ? "on" : "off"));
        }
    }
}
