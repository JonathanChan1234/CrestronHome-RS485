using System;
using System.Collections.Generic;
using System.Text;
using Crestron.RAD.Common.Enums;
using Crestron.RAD.Common.Interfaces.ExtensionDevice;
using Crestron.RAD.DeviceTypes.ExtensionDevice;
using Crestron.RAD.DeviceTypes.Gateway;
using Crestron.SimplSharp;
using Crestron.SimplSharp.CrestronIO;

namespace RS485CrestronHome
{
    public class CallLiftExtensionDevice : AExtensionDevice
    {

        private const string CallLiftCommand = "CallLift";

        private readonly string LiftIconLabel = "LiftIcon";
        private readonly string LiftStatusLabel = "LiftStatusLabel";
        private const string DataReceivedProperty = "DataReceivedProperty";

        private const string LiftArriveIcon = "icGateOpenDisabled";
        private const string LiftNotArriveIcon = "icGateClosedDisabled";


        private PropertyValue<string> _liftStatusProperty;
        private PropertyValue<string> _liftIconProperty;
        private PropertyValue<string> _dataReceivedProperty;
        private string _command = "";

        public event EventHandler<string> CommandSentHandler;
        private GatewayPairedDeviceInformation _pairedDeviceInfo;

        public GatewayPairedDeviceInformation PairedDeviceInformation
        {
            get { return _pairedDeviceInfo; }
        }

        public CallLiftExtensionDevice(string id, string name)
        {
            _pairedDeviceInfo = new GatewayPairedDeviceInformation(
                id,
                name,
                Description,
                Manufacturer,
                BaseModel,
                DriverData.CrestronSerialDeviceApi.GeneralInformation.DeviceType,
                string.Empty);
            _liftStatusProperty = CreateProperty<string>(new PropertyDefinition(LiftStatusLabel, null, DevicePropertyType.String));
            _liftIconProperty = CreateProperty<string>(new PropertyDefinition(LiftIconLabel, null, DevicePropertyType.String));
            _dataReceivedProperty = CreateProperty<string>(new PropertyDefinition(DataReceivedProperty, null, DevicePropertyType.String));
            InitUI();
        }

        private void InitUI()
        {
            _liftStatusProperty.Value = "0";
            _liftIconProperty.Value = LiftNotArriveIcon;
            Connected = true;
            Commit();
        }

        public void SetLiftStatus(string status)
        {
            _liftStatusProperty.Value = status == "1" ? "Arrived" : "Not Arrived";
            _liftIconProperty.Value = status == "1" ? LiftArriveIcon : LiftNotArriveIcon;
            _dataReceivedProperty.Value = status;
            Commit();
        }
        protected override IOperationResult DoCommand(string command, string[] parameters)
        {
            switch (command)
            {
                case CallLiftCommand:
                    Log("Call Lift Command called");
                    if (parameters.Length == 1) _command = parameters[0];
                    CommandSentHandler?.Invoke(this, _command.Equals("") ? "1" : _command);
                    break;
                default:
                    ErrorLog.Error($"Invalid Command {command}");
                    return new OperationResult(OperationResultCode.Error);
            }
            return new OperationResult(OperationResultCode.Success);
        }

        protected override string GetUiDefinition(string uiFolderPath)
        {
            var uiFilePath = Path.Combine(uiFolderPath, "UIDefinition.xml");

            if (!File.Exists(uiFilePath))
            {
                Log(string.Format("ERROR: Ui Definition file not found. Path: {0}", uiFilePath));
                return null;
            }

            if (EnableLogging)
                Log(string.Format("UI Definition file found. Path: '{0}'", uiFilePath));

            return File.ReadToEnd(uiFilePath, Encoding.UTF8);
        }

        protected override IOperationResult SetDriverPropertyValue<T>(string propertyKey, T value)
        {
            if (EnableLogging) Log($"key: {propertyKey}, value: {value}");
            return new OperationResult(OperationResultCode.Success);
        }

        protected override IOperationResult SetDriverPropertyValue<T>(string objectId, string propertyKey, T value)
        {
            return new OperationResult(OperationResultCode.Success);
        }

        public void SetConnectionStatus(bool connected)
        {
            Connected = connected;
            Commit();
        }

    }
}
