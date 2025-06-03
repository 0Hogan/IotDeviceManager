using System;
using Mqtt;
using Newtonsoft.Json;

namespace Mqtt;
public class SprinklersCmdMsg : Message
{
    public SprinklersCmdMsg() : base("/iot/sprinklers/cmd") {}

    public override string GetPayload()
    {
        return JsonConvert.SerializeObject(this);
    }

    public enum Command : UInt16
    {
        MissingCommandError=0, ///< Indicates that a command was missing
        EnqueueJob, ///< Adds a job to the queue
        DequeueJobByIndex, ///< Removes a zone from the queue
        PauseQueueExecution, ///< Pauses execution of the job queue
        ResumeQueueExecution, ///< Resumes execution of the job queue
        StopQueueExecution, ///< Cancels all jobs currently in the queue
        RequestQueueStatus, ///< Requests the current status of the queue
        DeserializeError, ///< Indicates an error while deserializing the message
        InvalidCommandError, ///< Indicates that the specified command is invalid
    }

    public Command Cmd { get; set; } = Command.MissingCommandError;
    public UInt16 ZoneNumber { get; set; } = 0xff;
    public UInt64 Duration_s { get; set; } = 0;
}