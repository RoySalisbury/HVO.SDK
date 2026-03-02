namespace HVO.Power.OutbackMate
{
    /// <summary>
    /// Identifies the type of rotating extra value reported by a FlexNet DC battery monitor.
    /// Each status report includes one extra value that cycles through these types.
    /// </summary>
    public enum OutbackMateFlexNetExtraValueType
    {
        /// <summary>Unknown or unrecognized extra value type.</summary>
        Unknown = 0,

        /// <summary>Accumulated amp-hours through shunt A.</summary>
        AccumulatedShuntAAmpHours = 1,

        /// <summary>Accumulated watt-hours through shunt A.</summary>
        AccumulatedShuntAWattHours = 2,

        /// <summary>Accumulated amp-hours through shunt B.</summary>
        AccumulatedShuntBAmpHours = 3,

        /// <summary>Accumulated watt-hours through shunt B.</summary>
        AccumulatedShuntBWattHours = 4,

        /// <summary>Accumulated amp-hours through shunt C.</summary>
        AccumulatedShuntCAmpHours = 5,

        /// <summary>Accumulated watt-hours through shunt C.</summary>
        AccumulatedShuntCWattHours = 6,

        /// <summary>Number of days since the battery was last fully charged.</summary>
        DaysSinceFull = 7,

        /// <summary>Today's minimum state of charge (percent).</summary>
        TodaysMinimumSOC = 8,

        /// <summary>Today's net input amp-hours.</summary>
        TodaysNetInputAmpHours = 9,

        /// <summary>Today's net output amp-hours.</summary>
        TodaysNetOutputAmpHours = 10,

        /// <summary>Today's net input watt-hours.</summary>
        TodaysNetInputWattHours = 11,

        /// <summary>Today's net output watt-hours.</summary>
        TodaysNetOutputWattHours = 12,

        /// <summary>Charge-factor-corrected net amp-hours.</summary>
        ChargeFactorCorrectedNetAmpHours = 13,

        /// <summary>Charge-factor-corrected net watt-hours.</summary>
        ChargeFactorCorrectedNetWattHours = 14
    }
}
