using Crestron.RAD.Common.Interfaces;
using Crestron.RAD.Common.Transports;
using Crestron.RAD.DeviceTypes.Gateway;
using Crestron.RAD.ProTransports;
using System;

namespace RS485Gateway
{
    public class ARS485Gateway : AGateway, ISimpl, ISerialComport
    {

        public void SendCommandViaTransport(Object sender, string msg)
        {
            ConnectionTransport.Send("1", null);
        }

        public SimplTransport Initialize(Action<string, object[]> send)
        {
            ConnectionTransport = new CallLiftSerialTransport
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
            (Protocol as CallLiftSerialProtocol).TransportSendHandler += SendCommandViaTransport;
            return ConnectionTransport as SimplTransport;
        }

        public void Initialize(IComPort comPort)
        {
            ConnectionTransport = new CallLiftSerialTransport
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
            (Protocol as CallLiftSerialProtocol).TransportSendHandler += SendCommandViaTransport;
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
