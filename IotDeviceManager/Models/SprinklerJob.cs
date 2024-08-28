using System;

public class SprinklerJob
{
    public SprinklerJob() {}
    public SprinklerJob(int zoneNumber, UInt64 duration_s)
    {
        ZoneNumber = zoneNumber;
        Duration_s = duration_s;
    }

    public int ZoneNumber { get; set; }
    public UInt64 Duration_s {get; set; }
}