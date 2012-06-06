namespace SignalR.AppFabric
{
    using System;

    public class IIDGeneratorException : Exception
    {
        public IIDGeneratorException()
        {
        }
        public IIDGeneratorException(String Message)
        {
            exceptionMessage = Message;
        }
        public override string Message
        {
            get
            {
                return this.exceptionMessage;
            }
        }

        private string exceptionMessage { get; set; }
    }
}
