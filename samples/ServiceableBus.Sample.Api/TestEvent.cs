namespace ServiceableBus.Sample.Api
{
    public class TestEvent : ServiceableBusEvent
    {
        private const string Topic = "test-event-topic";

        public TestEvent(string field1, DateTime field2, int field3)
        {
            TopicName = Topic;
            MessageTypeName = typeof(TestEvent).Name;
            CreatedAt = DateTime.UtcNow;
            Payload = new TestEventPayload(field1, field2, field3);
        }
    }

    internal class TestEventPayload : IServiceableBusPayload
    {
        public TestEventPayload(string field1, DateTime field2, int field3)
        {
            Field1 = field1;
            Field2 = field2;
            Field3 = field3;
        }

        string Field1 { get; set; }
        DateTime Field2 { get; set; }
        int Field3 { get; set; }    
    }
}
