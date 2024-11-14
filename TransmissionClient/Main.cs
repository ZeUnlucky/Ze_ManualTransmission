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
        readonly float[] gearInfo = new float[] { 7, 15, 24, 33, 44, 55, 66, 77};
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
                        if (!VehicleIsCorrectType(veh))
                            return;
                        if (maxGears == -1)
                            maxGears = veh.GetHandlingInt("nInitialDriveGears");

                        veh.Gear = gear;
                        if (RAGE.Game.Pad.IsControlPressed(27, (int)RAGE.Game.Control.CharacterWheel))
                        {
                            RAGE.Game.Pad.DisableControlAction(27, (int)RAGE.Game.Control.VehicleAccelerate, true);
                            RAGE.Game.Pad.DisableControlAction(27, (int)RAGE.Game.Control.VehicleBrake, true);
                            if (RAGE.Game.Pad.IsControlJustPressed(27, (int)RAGE.Game.Control.PhoneUp) && gear != maxGears)
                            {
                                gear++;                           
                                Chat.Show(true);
                                Chat.Output($"Gear is now {gear}");
                            }
                            if (RAGE.Game.Pad.IsControlJustPressed(27, (int)RAGE.Game.Control.PhoneDown) && gear != -1)
                            {
                                gear--;
                                Chat.Show(true);
                                Chat.Output($"Gear is now {gear}");  
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
                    ResetCar();
                }
            }
        }

        public bool VehicleIsCorrectType(Vehicle veh)
        {
            int vClass = veh.GetClass();
            return (vClass <= 12 || vClass == 18) && maxGears >= 4;
        }

        public void ChangeGear(Vehicle veh)
        {
            speed = veh.GetSpeed();
            float torque = speed / gearInfo[gear-1];
            if (speed < gearInfo[0])
            {
                if (gear == 1)
                    torque = 1;
                else
                {
                    torque = 1.25f - speed / 10;
                    if (torque < 0.25f)
                        torque = 0.25f;
                }
            }
            if (speed > gearInfo[gear - 1] && gear == maxGears)
            {
                torque = 1;
            }
            if (torque > 1.25f)
            {
                torque = 1.25f - speed / gearInfo[gear];
                veh.Rpm = 1;
            }
            else
                veh.Rpm = torque/1.25f;
            if (speed > RAGE.Game.Vehicle.GetVehicleModelMaxSpeed(veh.Model) + veh.GetMod(11)*0.8f)
                torque = 1;
            veh.SetEngineTorqueMultiplier(torque);           
        }

        public void LeaveVehicle(Vehicle veh, int seat)
        {
            if (seat == -1)
                veh.SetEngineTorqueMultiplier(1.0f);
        }

        public void ToggleManual(object [] args)
        {
            isManual = !isManual;
            if (!isManual)
            {
                ResetCar();
                Vehicle veh = Player.LocalPlayer.Vehicle;
                if (veh != null)
                    veh.SetEngineTorqueMultiplier(1.0f);
            }
        }

        void ResetCar()
        {
            gear = 0;
            maxGears = -1;
        }
    }
}
