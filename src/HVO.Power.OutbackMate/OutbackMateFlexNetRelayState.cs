namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// Physical state of the FlexNet DC battery monitor relay.
    /// </summary>
    public enum OutbackMateFlexNetRelayState
    {
        /// <summary>Relay contacts are closed (energized).</summary>
        Closed,

        /// <summary>Relay contacts are open (de-energized).</summary>
        Open
    }
}
