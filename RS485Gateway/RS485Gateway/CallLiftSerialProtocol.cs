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

        private string[] _buffer = new string[1000];
        private int _byteCount = 0;

        #region Initialization

        public CallLiftSerialProtocol(ISerialTransport transport, byte id) : base(transport, id)
        {
            ValidateResponse = GatewayValidateResponse;
            var device = new CallLiftExtensionDevice(DeviceID, "Test");
            device.SetConnectionStatus(true);
            device.CommandSentHandler += CommandSentHandler;
            AddPairedDevice(device);
        }

        private void ClearBuffer()
        {
            for (int i = 0; i < 1000; ++i)
            {
                _buffer[i] = "";
            }
            _byteCount = 0;
        }

        private string GroupBuffer()
        {
            string res = "";
            for (int i = 0; i < 1000; ++i)
            {
                if (_buffer[i].Equals("\n")) break;
                res += _buffer[i];
            }
            return res;
        }

        private void CommandSentHandler(object sender, string command)
        {
            if (EnableLogging) Log($"Send Command {command}");
            ClearBuffer();
            var commandSet = new CommandSet(
                "CallLift",
               command,
               CommonCommandGroupType.Other,
               null,
               false,
               CommandPriority.Normal,
               StandardCommandsEnum.NotAStandardCommand)
            {
                CommandPrepared = true
            };
            SendCommand(commandSet);
        }

        #endregion

        #region Base Members
        protected override void ChooseDeconstructMethod(ValidatedRxData validatedData)
        {
            if (EnableLogging)
            {
                string hex = string.Format("0x{0:X2}", (int)validatedData.Data[0]);
                Log($"incoming data: {hex}, End of Frame: {validatedData.Data.Equals("\n")}");
            }
            // ignore the incoming data if it exceeds the buffer size
            if (_byteCount > 999) return;
            _buffer[_byteCount] = validatedData.Data;
            _byteCount++;
            if (validatedData.Data.Equals("\n"))
            {
                CallLiftExtensionDevice device;
                string data = GroupBuffer();
                if (EnableLogging) Log($"Received Data: {data}");
                ClearBuffer();
                if (_pairedDevices.TryGetValue(DeviceID, out device))
                {
                    device.SetLiftStatus(data);
                }
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
