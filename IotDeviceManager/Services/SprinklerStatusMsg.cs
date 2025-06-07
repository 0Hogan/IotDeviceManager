using System.Collections.Generic;
using Mqtt;
using Newtonsoft.Json;

namespace Mqtt;

public class SprinklersStatusMsg : Message
{
    class JobsList
    {
        public List<SprinklerJob> Jobs { get; set; } = new List<SprinklerJob>();
    }

    public SprinklersStatusMsg() : base("/iot/sprinklers/status") { }
    public SprinklersStatusMsg(string payload) : base("/iot/sprinklers/status")
    {
        Jobs = JsonConvert.DeserializeObject<List<SprinklerJob>>(payload) ?? new List<SprinklerJob>();
    }

    public override string GetPayload()
    {
        string msg = JsonConvert.SerializeObject(this);

        return msg;
    }

    public List<SprinklerJob> Jobs { get; set; } = new List<SprinklerJob>();
}