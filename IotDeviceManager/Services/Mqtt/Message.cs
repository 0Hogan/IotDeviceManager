namespace Mqtt;

public abstract class Message
{
    public Message(string topicName)
    {
        topic = topicName;
    }

    public abstract string GetPayload();

    public string GetTopic() { return topic; }
    private string topic;
}