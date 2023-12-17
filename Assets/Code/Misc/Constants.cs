using UnityEngine;
public struct Constants
{
    public const int COROUTINE_LOOP_BATCHSIZE = 10000;
    public const int NETWORKED_BURN_CURVES_SIZE = 10;
    public const float GAS_CONSTANT = 8.31446261815324f;
    public const float VELOCITY_INCREASE_SENSIBILITY = 100f;
    public const float DISTANCE_FACTOR = 1e+6f;
    public const float SECONDS_TO_DECILMAL_HOUR = 1f / 3600f;
    public const float GRAVITATIONAL_CONSTANT = 6.67408e-11f;
    public const float STANDARD_GRAVITY = 9.80665f;
    public const float DEG2RAD = 0.0174533f;
    public const float DESYNC_MARGIN_OF_ERROR = 0.1f;
    public const float TRAJECTORY_TIME_INTERVAL = 60f;
    public const float TRAJECTORY_MAX_TIME_AHEAD = 86400f;
    public const float MOUSE_HOVER_SCREEN_DISTANCE = 10f;
    public const float TRAJECTORY_LINE_THICKNESS = 0.001f;
    public const float BURN_SIMULATION_TIMESTEP = 1f;
}