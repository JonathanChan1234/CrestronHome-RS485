using Crestron.RAD.Common.Interfaces;
using Crestron.RAD.Common.Transports;
using Crestron.RAD.DeviceTypes.Gateway;
using Crestron.RAD.ProTransports;
using System;

namespace RS485Gateway
{
    public class ARS485Gateway : AGateway, ISimpl, ISerialComport
    {
        private SimplTransport _transport;

        public SimplTransport Initialize(Action<string, object[]> send)
        {
            _transport = new SimplTransport { Send = send };
            ConnectionTransport = _transport;
            Protocol = new CallLiftSerialProtocol(ConnectionTransport, Id)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger
            };
            Protocol.RxOut += SendRxOut;
            Connected = true;
            return _transport;
        }

        public void Initialize(IComPort comPort)
        {
            ConnectionTransport = new CommonSerialComport(comPort)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger,
                EnableRxDebug = InternalEnableRxDebug,
                EnableTxDebug = InternalEnableTxDebug
            };
            Protocol = new CallLiftSerialProtocol(ConnectionTransport, Id)
            {
                EnableLogging = InternalEnableLogging,
                CustomLogger = InternalCustomLogger
            };
            Protocol.RxOut += SendRxOut;
            Connected = true;
        }

        public override void Connect()
        {
            base.Connect();
            ((CallLiftSerialProtocol)Protocol).Connect();
        }

        public override void Disconnect()
        {
            base.Disconnect();
            ((CallLiftSerialProtocol)Protocol).Disconnect();
        }


    }
}
