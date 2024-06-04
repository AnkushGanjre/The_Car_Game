public interface ICarControl
{
    // float
    public float AccelerationFactor { get; set; }
    public float BrakingFactor { get; set; }
    public float SteeringValue { get; set; }
    public float TopSpeedValue { get; set; }
    public float SuspensionValue { get; set; }

    // bools
    public bool IsBraking { get; set; }
    public bool IsBoosting { get; set; }
    public bool IsDrifting { get; set; }
}