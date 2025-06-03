using System;

public class SprinklerJob
{
    public SprinklerJob() {}
    public SprinklerJob(UInt16 zoneNumber, UInt64 duration_s)
    {
        ZoneNumber = zoneNumber;
        Duration_s = duration_s;
    }

    public UInt16 ZoneNumber { get; set; }
    public UInt64 Duration_s {get; set; }
}