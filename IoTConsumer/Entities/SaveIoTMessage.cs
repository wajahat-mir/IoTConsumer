using System;
using System.Collections.Generic;
using System.Text;

namespace IoTConsumer.Entities
{
    class SaveIoTMessage
    {
        public string DeviceId { get; set; }
        public DateTime TimeStamp { get; set; }
        public IoTStatus Status { get; set; }
        public string message { get; set; }

        public SaveIoTMessage(string DeviceId, DateTime TimeStamp, IoTStatus Status, string message)
        {
            this.DeviceId = DeviceId;
            this.TimeStamp = TimeStamp;
            this.Status = Status;
            this.message = message;
        }
    }

    enum IoTStatus
    {
        Failure = 0,
        Alive = 1,
        ShuttingDown = 2
    }
}
