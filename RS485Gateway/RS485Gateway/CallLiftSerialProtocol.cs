using Crestron.RAD.Common.BasicDriver;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Transports;
using Crestron.RAD.DeviceTypes.Gateway;
using Crestron.SimplSharp;
using RS485CrestronHome;
using System;
using System.Text;
using System.Collections.Generic;

namespace RS485Gateway
{
    class CallLiftSerialProtocol : AGatewayProtocol
    {
        #region Fields
        private readonly Dictionary<string, CallLiftExtensionDevice> _pairedDevices = new Dictionary<string, CallLiftExtensionDevice>();

        private CCriticalSection _pairedDevicesLock = new CCriticalSection();
        private const string DeviceID = "1";

        #endregion

        public EventHandler<string> TransportSendHandler;

        #region Initialization

        public CallLiftSerialProtocol(ISerialTransport transport, byte id) : base(transport, id)
        {
            ValidateResponse = GatewayValidateResponse;
            var device = new CallLiftExtensionDevice(DeviceID, "call 10/F");
            device.CallLiftOneHandler += Device_CallLiftOneHandler;
            device.CallLiftTwoHandler += Device_CallLiftTwoHandler;
            device.CallLiftThreeHandler += Device_CallLiftThreeHandler;
            AddPairedDevice(device);

            // AddCustomCommand("test", "1", null);

            SendCommand(new CommandSet(
                "DeviceDiscovery",
                "DeviceDiscovery",
               CommonCommandGroupType.Other,
               null,
               false,
               CommandPriority.High,
               StandardCommandsEnum.NotAStandardCommand));
        }

        // Method 1: Using Send Command
        private void Device_CallLiftOneHandler(object sender, string e)
        {
            if (EnableLogging) Log($"Calling SendCommand");
            var commandSet = new CommandSet(
                "CallLift",
               "\x1",
               CommonCommandGroupType.Other,
               null,
               true,
               CommandPriority.High,
               StandardCommandsEnum.NotAStandardCommand)
            {
                CommandPrepared = true
            };
            SendCommand(commandSet);
        }

        // Method 2: Using SendCustomCommandByName
        private void Device_CallLiftTwoHandler(object sender, string e)
        {
            if (EnableLogging) Log($"Calling SendCustomCommandByname");
            try
            {
                SendCustomCommandByName("test");
            }
            catch (Exception error)
            {
                Log(error.Message);
            }
        }

        // Method 3: Using Transport.SendMethod
        private void Device_CallLiftThreeHandler(object sender, string e)
        {
            if (EnableLogging) Log($"Calling Transport.SendMethod");
            try
            {
                Transport.SendMethod("1", null);
            }
            catch (Exception error)
            {
                Log(error.Message);
            }
            try
            {
                TransportSendHandler?.Invoke(this, "1");
            }
            catch (Exception error)
            {
                Log(error.Message);
            }
        }
        #endregion

        #region Base Members
        protected override bool PrepareStringThenSend(CommandSet commandSet)
        {
            if (EnableLogging) Log($"PrepareStringThenSend: {commandSet.Command}");
            commandSet.CommandPrepared = true;
            var sent = base.PrepareStringThenSend(commandSet);
            if (EnableLogging)
            {
                Log($"PrepareStringThenSend: sent: {sent}");
                Log($"Commandset prepared: {commandSet.CommandPrepared}");
            }
            return true;
        }

        protected override bool CanQueueCommand(CommandSet commandSet, bool powerOnCommandInQueue)
        {
            var canQueue = base.CanQueueCommand(commandSet, powerOnCommandInQueue);
            if (EnableLogging) Log($"CanQueueCommand: {canQueue}");
            return canQueue;
        }

        protected override bool CanSendCommand(CommandSet commandSet)
        {
            var canSend = base.CanSendCommand(commandSet);
            if (EnableLogging) Log($"CanSendCommand: {canSend}");
            return canSend;
        }

        protected override void ChooseDeconstructMethod(ValidatedRxData validatedData)
        {
            // if (EnableLogging) Log($"Received from serial: {validatedData.Data}");
            CallLiftExtensionDevice device;
            if (_pairedDevices.TryGetValue(DeviceID, out device))
            {
                device.SetLiftStatus(validatedData.Data);
            }
        }

        protected override void ConnectionChangedEvent(bool connection)
        {
            base.ConnectionChangedEvent(connection);
            foreach (var samplePairedDevice in _pairedDevices.Values)
                samplePairedDevice.SetConnectionStatus(connection);
        }

        #endregion

        #region Public Members
        public void Connect()
        {
        }

        public void Disconnect()
        {
        }

        public override void Dispose()
        {
            try
            {
                _pairedDevicesLock.Enter();

                foreach (var pairedDevice in _pairedDevices.Values)
                {
                    if (pairedDevice is IDisposable disposable)
                        disposable.Dispose();
                }
                _pairedDevices.Clear();
            }
            finally
            {
                _pairedDevicesLock.Leave();
            }

            base.Dispose();
        }
        #endregion

        #region Private Members
        private void AddPairedDevice(CallLiftExtensionDevice pairedDevice)
        {
            pairedDevice.SetConnectionStatus(IsConnected);
            AddPairedDevice(pairedDevice.PairedDeviceInformation, pairedDevice);
            try
            {
                _pairedDevicesLock.Enter();
                _pairedDevices[pairedDevice.PairedDeviceInformation.Id] = pairedDevice;
            }
            finally
            {
                _pairedDevicesLock.Leave();
            }
        }

        private void UpdateSamplePairedDevice(CallLiftExtensionDevice updatedPairedDevice)
        {
            updatedPairedDevice.SetConnectionStatus(IsConnected);
            UpdatePairedDevice(updatedPairedDevice.PairedDeviceInformation.Id, updatedPairedDevice.PairedDeviceInformation);

            //if the updated paired device is a different instance than the current paired device in the cache
            //update the cache and dispose of the old paired device
            CallLiftExtensionDevice oldPairedDevice;
            bool oldDeviceNeedsDisposal = false;
            try
            {
                _pairedDevicesLock.Enter();

                if (_pairedDevices.TryGetValue(updatedPairedDevice.PairedDeviceInformation.Id, out oldPairedDevice))
                {
                    if (oldPairedDevice == updatedPairedDevice)
                        return;

                    //Values are different, need to update the cache
                    _pairedDevices[updatedPairedDevice.PairedDeviceInformation.Id] = updatedPairedDevice;

                    oldDeviceNeedsDisposal = true;
                }
            }
            finally
            {
                _pairedDevicesLock.Leave();
            }

            //Dispose of the old device if necessary
            if (oldDeviceNeedsDisposal && oldPairedDevice is IDisposable)
                ((IDisposable)oldPairedDevice).Dispose();
        }

        private void RemovePairedDevice(CallLiftExtensionDevice pairedDevice)
        {
            try
            {
                _pairedDevicesLock.Enter();

                if (_pairedDevices.ContainsKey(pairedDevice.PairedDeviceInformation.Id))
                {
                    RemovePairedDevice(pairedDevice.PairedDeviceInformation.Id);

                    //Remove the paired device from the local collection
                    _pairedDevices.Remove(pairedDevice.PairedDeviceInformation.Id);
                }
            }
            finally
            {
                _pairedDevicesLock.Leave();
            }

            //Dispose of the paired device
            if (pairedDevice is IDisposable)
                ((IDisposable)pairedDevice).Dispose();

        }

        private ValidatedRxData GatewayValidateResponse(string response, CommonCommandGroupType commandGroup)
        {
            return new ValidatedRxData(true, response);
        }
        #endregion
    }
}
