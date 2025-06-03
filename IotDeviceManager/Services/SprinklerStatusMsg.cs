using Mqtt;

namespace Mqtt;
public class SprinklersStatusMsg : Message
{
    public SprinklersStatusMsg() : base("/iot/sprinklers/status") {}

    public override string GetPayload()
    {
        string msg = "";

        return msg;
    }
}