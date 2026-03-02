namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// Identifies the protocol version used by the Outback Mate controller.
    /// </summary>
    public enum OutbackMateProtocolVersion
    {
        /// <summary>
        /// Mate1 protocol: record type is identified by the first CSV field character
        /// (A-K = charge controller, a-j = FlexNet, 0-9/; = inverter/charger).
        /// </summary>
        Mate1,

        /// <summary>
        /// Mate2 protocol: record type is identified by the second CSV field
        /// (2 = inverter/charger, 3 = charge controller, 4 = FlexNet).
        /// </summary>
        Mate2
    }
}
