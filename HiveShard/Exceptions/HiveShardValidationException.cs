using System;

namespace HiveShard.Exceptions;

public class HiveShardValidationException: Exception
{
    public HiveShardValidationExceptionCase Case { get; }

    public HiveShardValidationException(string message, HiveShardValidationExceptionCase @case) : base(message)
    {
        Case = @case;
    }   
}

public enum HiveShardValidationExceptionCase
{
    NoEvents,
    EmitterWithoutEvents
}