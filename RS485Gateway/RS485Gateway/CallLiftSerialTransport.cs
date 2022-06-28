using Crestron.RAD.Common.Transports;

namespace RS485Gateway
{
    class CallLiftSerialTransport : SimplTransport
    {
        public override void SendMethod(string message, object[] paramaters)
        {
            Log($"Transport SendMethod: {message}");
            base.SendMethod(message, paramaters);
        }

        public override void Start()
        {
            base.Start();
            ConnectionChanged(true);
        }

        public override void Stop()
        {
            base.Stop();
            ConnectionChanged(false);
        }
    }
}
