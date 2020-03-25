namespace Dell_Fan_Control.Models
{
    public class Temperture
    {
        public Temperture(string message)
        {
            Message = message;
            var splitMessage = SplitMessage(message);

            Location = splitMessage[0].Trim();
            UnknownFieldA = splitMessage[1].Trim();
            Status = splitMessage[2].Trim();
            UnknownFieldB = splitMessage[3].Trim();

            var tempReading = splitMessage[4].Replace("degrees C", "").Trim();
            Temp = int.Parse(tempReading);
        }

        //Message
        //Inlet Temp       | 04h | ok  |  7.1 | 22 degrees C
        private string[] SplitMessage(string message) => message.Split("|");

        public string Message { get; private set; }
        public string Location { get; private set; }
        public string UnknownFieldA { get; private set; }
        public string Status { get; private set; }
        public string UnknownFieldB { get; private set; }
        public int Temp { get; private set; }
    }
}
