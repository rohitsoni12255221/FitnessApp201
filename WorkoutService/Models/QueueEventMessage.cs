using System.Text.Json;

namespace WorkoutService.Models
{
    
        public class QueueEventMessage
        {
            public string ActionType { get; set; }
            public JsonElement Payload { get; set; }
        }
    
}
