using System;
using System.Collections.Generic;
using System.Text;

namespace IoTConsumer.Entities
{
    class SaveIoTMessage
    {
        public string DeviceId { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Status { get; set; }
        public string message { get; set; }

        public SaveIoTMessage(string DeviceId, DateTime TimeStamp, string Status, string message)
        {
            this.DeviceId = DeviceId;
            this.TimeStamp = TimeStamp;
            this.Status = Status;
            this.message = message;
        }
    }
}
