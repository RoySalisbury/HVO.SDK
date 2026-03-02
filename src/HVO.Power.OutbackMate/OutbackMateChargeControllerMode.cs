namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// Charge stage of an Outback charge controller.
    /// </summary>
    public enum OutbackMateChargeControllerMode
    {
        /// <summary>Silent mode (not charging).</summary>
        Silent = 0,

        /// <summary>Float charging stage.</summary>
        Floating = 1,

        /// <summary>Bulk charging stage.</summary>
        Bulk = 2,

        /// <summary>Absorb charging stage.</summary>
        Absorbing = 3,

        /// <summary>Equalization charging stage.</summary>
        Equalizing = 4
    }
}
