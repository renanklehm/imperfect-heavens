using UnityEngine;
public struct Constants
{
    public const int COROUTINE_LOOP_BATCHSIZE = 10000;
    public const float DISTANCE_FACTOR = 1e+6f;
    public const float SECONDS_TO_DECILMAL_HOUR = 1f / 3600f;
    public const float GRAVITATIONAL_CONSTANT = 6.67408e-11f;
    public const float DEG2RAD = 0.0174533f;
    public const float DESYNC_MARGIN_OF_ERROR = 0.1f;
    public const float TRAJECTORY_TIME_INTERVAL = 60f;
    public const float TRAJECTORY_MAX_TIME_AHEAD = 84600f;
    public const float TRAJECTORY_MIN_TIME_AHEAD = 42300f;
}