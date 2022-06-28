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

        private const string CallLiftOneCommand = "CallLiftOne";
        private const string CallLiftTwoCommand = "CallLiftTwo";
        private const string CallLiftThreeCommand = "CallLiftThree";

        private readonly string LiftIconLabel = "LiftIcon";
        private readonly string LiftStatusLabel = "LiftStatusLabel";
        private readonly string MainPageLabel = "MainPageLabel";

        private const string LiftArriveIcon = "icGateOpenDisabled";
        private const string LiftNotArriveIcon = "icGateClosedDisabled";

        private PropertyValue<string> _liftStatusProperty;
        private PropertyValue<string> _liftIconProperty;
        private PropertyValue<string> _mainPageLabelProperty;

        public event EventHandler<string> CallLiftOneHandler;
        public event EventHandler<string> CallLiftTwoHandler;
        public event EventHandler<string> CallLiftThreeHandler;
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
            _mainPageLabelProperty = CreateProperty<string>(new PropertyDefinition(MainPageLabel, null, DevicePropertyType.String));
            InitUI();
        }

        private void InitUI()
        {
            _liftStatusProperty.Value = "0";
            _liftIconProperty.Value = LiftNotArriveIcon;
            _mainPageLabelProperty.Value = "Main Page";
            Commit();
        }

        public void SetLiftStatus(string status)
        {
            _liftStatusProperty.Value = status;
            _liftIconProperty.Value = status == "1" ? LiftArriveIcon : LiftNotArriveIcon;
            Commit();
        }
        protected override IOperationResult DoCommand(string command, string[] parameters)
        {
            switch (command)
            {
                case CallLiftOneCommand:
                    if (EnableLogging) Log("Call Lift One Called");
                    CallLiftOneHandler?.Invoke(this, "1");
                    break;
                case CallLiftTwoCommand:
                    if (EnableLogging) Log("Call Lift Two Called");
                    CallLiftTwoHandler?.Invoke(this, "1");
                    break;
                case CallLiftThreeCommand:
                    if (EnableLogging) Log("Call Lift One Called");
                    CallLiftThreeHandler?.Invoke(this, "1");
                    break;
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
        }

    }
}
