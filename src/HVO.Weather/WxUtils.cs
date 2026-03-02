using System;

namespace HVO.Weather
{
    /// <summary>
    /// Provides meteorological calculation utilities including pressure reduction algorithms,
    /// vapor pressure models, heat index, wind chill, dew point, and unit conversions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// All algorithms are based on published meteorological standards and references including:
    /// U.S. Standard Atmosphere (1976), Manual of Barometry (1963), Smithsonian Meteorological Tables,
    /// ASOS training documents, MADIS API, and WMO reports.
    /// </para>
    /// </remarks>
    public static class WxUtils
    {
        /// <summary>
        /// Sea-level pressure reduction algorithms.
        /// </summary>
        public enum SLPAlgorithm
        {
            /// <summary>Approximates the SLP calculation used by Davis Vantage Pro consoles.</summary>
            DavisVP,

            /// <summary>University of Vienna algorithm.</summary>
            /// <remarks>See http://www.univie.ac.at/IMG-Wien/daquamap/Parametergencom.html</remarks>
            Univie,

            /// <summary>Manual of Barometry (1963) algorithm.</summary>
            ManBar
        }

        /// <summary>
        /// Altimeter setting algorithms.
        /// </summary>
        public enum AltimeterAlgorithm
        {
            /// <summary>Formula from ASOS training documentation.</summary>
            ASOS,

            /// <summary>Metric formula likely used to derive the ASOS formula.</summary>
            ASOS2,

            /// <summary>MADIS algorithm by NOAA Forecast Systems Lab.</summary>
            /// <remarks>See http://madis.noaa.gov/madis_api.html</remarks>
            MADIS,

            /// <summary>NOAA algorithm (essentially the same as SMT with unit conversion differences).</summary>
            NOAA,

            /// <summary>Weather Observation Handbook algorithm.</summary>
            /// <remarks>See http://www.wxqa.com/archive/obsman.pdf</remarks>
            WOB,

            /// <summary>Smithsonian Meteorological Tables (1963) algorithm.</summary>
            SMT
        }

        /// <summary>
        /// Vapor pressure algorithms.
        /// </summary>
        public enum VapAlgorithm
        {
            /// <summary>Approximates the calculation used by Davis Vantage Pro weather stations.</summary>
            DavisVP,

            /// <summary>Buck (1996) algorithm.</summary>
            /// <remarks>See http://cires.colorado.edu/~voemel/vp.html</remarks>
            Buck,

            /// <summary>Buck (1981) algorithm.</summary>
            Buck81,

            /// <summary>Bolton (1980) algorithm.</summary>
            Bolton,

            /// <summary>Magnus-Teten as used by NWS.</summary>
            TetenNWS,

            /// <summary>Magnus-Teten (Murray 1967 variant).</summary>
            TetenMurray,

            /// <summary>Magnus-Teten algorithm.</summary>
            Teten
        }

        private const SLPAlgorithm DefaultSLPAlgorithm = SLPAlgorithm.ManBar;
        private const AltimeterAlgorithm DefaultAltimeterAlgorithm = AltimeterAlgorithm.MADIS;
        private const VapAlgorithm DefaultVapAlgorithm = VapAlgorithm.Bolton;

        // U.S. Standard Atmosphere (1976) constants
        private const double Gravity = 9.80665;                                  // g at sea level at latitude 45.5° in m/s²
        private const double UniversalGasConstant = 8.31432;                     // universal gas constant in J/(mol·K)
        private const double MoleAir = 0.0289644;                                // mean molecular mass of air in kg/mol
        private const double MoleWater = 0.01801528;                             // molecular weight of water in kg/mol
        private const double GasConstantAir = UniversalGasConstant / MoleAir;    // ≈287.053 gas constant for air in J/(kg·K)
        private const double StandardSLP = 1013.25;                              // standard sea level pressure in hPa
        private const double StandardSlpInHg = 29.921;                           // standard sea level pressure in inHg
        private const double StandardTempK = 288.15;                             // standard sea level temperature in K
        private const double EarthRadius45 = 6356.766;                           // radius of earth at latitude 45.5° in km

        private const double StandardLapseRate = 0.0065;                         // standard lapse rate (6.5°C/1000m)
        private const double StandardLapseRateFt = StandardLapseRate * 0.3048;   // ≈0.0019812 standard lapse rate per foot
        private const double VpLapseRateUS = 0.00275;                            // lapse rate used by Davis VP (2.75°F/1000ft)
        private const double ManBarLapseRate = 0.0117;                           // Manual of Barometry lapse rate (11.7°F/1000m = 6.5°C/1000m)

        /// <summary>
        /// Converts station pressure to sensor pressure using the ASOS formula.
        /// </summary>
        /// <param name="pressureHPa">Station pressure in hPa.</param>
        /// <param name="sensorElevationM">Sensor elevation in meters.</param>
        /// <param name="stationElevationM">Station elevation in meters.</param>
        /// <param name="temperature">The current temperature.</param>
        /// <returns>The sensor pressure in hPa.</returns>
        public static double StationToSensorPressure(double pressureHPa, double sensorElevationM, double stationElevationM, Temperature temperature)
        {
            var barometer = BarometricPressure.FromMillibars(pressureHPa);
            return BarometricPressure.FromInchesHg(barometer.InchesHg / Power10(0.00813 * MToFt(sensorElevationM - stationElevationM) / temperature.Rankine)).Millibars;
        }

        /// <summary>
        /// Converts station pressure to altimeter setting using the specified algorithm.
        /// </summary>
        /// <param name="pressureHPa">Station pressure in hPa.</param>
        /// <param name="elevationM">Station elevation in meters.</param>
        /// <param name="algorithm">The altimeter algorithm to use (default: MADIS).</param>
        /// <returns>The altimeter setting in hPa.</returns>
        public static double StationToAltimeter(double pressureHPa, double elevationM, AltimeterAlgorithm algorithm = DefaultAltimeterAlgorithm)
        {
            var barometer = BarometricPressure.FromMillibars(pressureHPa);

            switch (algorithm)
            {
                case AltimeterAlgorithm.ASOS:
                    return BarometricPressure.FromInchesHg(Power(Power(barometer.InchesHg, 0.1903) + (1.313E-5 * MToFt(elevationM)), 5.255)).Millibars;

                case AltimeterAlgorithm.ASOS2:
                    {
                        var geopEl = GeopotentialAltitude(elevationM);
                        var k1 = StandardLapseRate * GasConstantAir / Gravity; // ≈0.190263
                        var k2 = 8.41728638E-5; // (StandardLapseRate / StandardTempK) * Power(StandardSLP, k1)
                        return Power(Power(pressureHPa, k1) + (k2 * geopEl), 1 / k1);
                    }

                case AltimeterAlgorithm.MADIS:
                    {
                        var k1 = 0.190284;
                        var k2 = 8.4184960528E-5; // (StandardLapseRate / StandardTempK) * Power(StandardSLP, k1)
                        return Power(Power(pressureHPa - 0.3, k1) + (k2 * elevationM), 1 / k1);
                    }

                case AltimeterAlgorithm.NOAA:
                    {
                        var k1 = 0.190284;
                        var k2 = 8.42288069E-5; // (StandardLapseRate / 288) * Power(StandardSLP, k1)
                        return (pressureHPa - 0.3) * Power(1 + (k2 * (elevationM / Power(pressureHPa - 0.3, k1))), 1 / k1);
                    }

                case AltimeterAlgorithm.WOB:
                    {
                        var k1 = StandardLapseRate * GasConstantAir / Gravity; // ≈0.190263
                        var k2 = 1.312603E-5; // (StandardLapseRateFt / StandardTempK) * Power(StandardSlpInHg, k1)
                        return BarometricPressure.FromInchesHg(Power(Power(barometer.InchesHg, k1) + (k2 * MToFt(elevationM)), 1 / k1)).Millibars;
                    }

                case AltimeterAlgorithm.SMT:
                    {
                        var k1 = 0.190284;
                        var k2 = 4.30899E-5; // (StandardLapseRate / 288) * Power(StandardSlpInHg, k1)
                        var geopEl = GeopotentialAltitude(elevationM);
                        return BarometricPressure.FromInchesHg((barometer.InchesHg - 0.01) * Power(1 + (k2 * (geopEl / Power(barometer.InchesHg - 0.01, k1))), 1 / k1)).Millibars;
                    }

                default:
                    return pressureHPa;
            }
        }

        /// <summary>
        /// Reduces station pressure to sea-level pressure using the specified algorithm.
        /// </summary>
        /// <param name="pressureHPa">Station pressure in hPa.</param>
        /// <param name="elevationM">Station elevation in meters.</param>
        /// <param name="currentTemperature">The current temperature at the station.</param>
        /// <param name="meanTemperature">The 12-hour mean temperature at the station.</param>
        /// <param name="humidity">Relative humidity (0–100).</param>
        /// <param name="algorithm">The SLP algorithm to use (default: ManBar).</param>
        /// <returns>The sea-level pressure in hPa.</returns>
        public static double StationToSeaLevelPressure(double pressureHPa, double elevationM, Temperature currentTemperature, Temperature meanTemperature, byte humidity, SLPAlgorithm algorithm = DefaultSLPAlgorithm)
        {
            return pressureHPa * PressureReductionRatio(pressureHPa, elevationM, currentTemperature, meanTemperature, humidity, algorithm);
        }

        /// <summary>
        /// Converts sensor pressure to station pressure using the ASOS formula.
        /// </summary>
        /// <param name="pressureHPa">Sensor pressure in hPa.</param>
        /// <param name="sensorElevationM">Sensor elevation in meters.</param>
        /// <param name="stationElevationM">Station elevation in meters.</param>
        /// <param name="temperature">The current temperature.</param>
        /// <returns>The station pressure in hPa.</returns>
        public static double SensorToStationPressure(double pressureHPa, double sensorElevationM, double stationElevationM, Temperature temperature)
        {
            var barometer = BarometricPressure.FromMillibars(pressureHPa);
            return BarometricPressure.FromInchesHg(barometer.InchesHg * Power10(0.00813 * MToFt(sensorElevationM - stationElevationM) / temperature.Rankine)).Millibars;
        }

        /// <summary>
        /// Converts sea-level pressure to station pressure using the specified algorithm.
        /// </summary>
        /// <param name="pressureHPa">Sea-level pressure in hPa.</param>
        /// <param name="elevationM">Station elevation in meters.</param>
        /// <param name="currentTemperature">The current temperature at the station.</param>
        /// <param name="meanTemperature">The 12-hour mean temperature at the station.</param>
        /// <param name="humidity">Relative humidity (0–100).</param>
        /// <param name="algorithm">The SLP algorithm to use (default: ManBar).</param>
        /// <returns>The station pressure in hPa.</returns>
        public static double SeaLevelToStationPressure(double pressureHPa, double elevationM, Temperature currentTemperature, Temperature meanTemperature, byte humidity, SLPAlgorithm algorithm = DefaultSLPAlgorithm)
        {
            return pressureHPa / PressureReductionRatio(pressureHPa, elevationM, currentTemperature, meanTemperature, humidity, algorithm);
        }

        /// <summary>
        /// Computes the pressure reduction ratio for converting between station and sea-level pressure.
        /// </summary>
        /// <param name="pressureHPa">Pressure in hPa.</param>
        /// <param name="elevationM">Station elevation in meters.</param>
        /// <param name="currentTemperature">The current temperature.</param>
        /// <param name="meanTemperature">The 12-hour mean temperature.</param>
        /// <param name="humidity">Relative humidity (0–100).</param>
        /// <param name="algorithm">The SLP algorithm to use (default: ManBar).</param>
        /// <returns>The pressure reduction ratio.</returns>
        public static double PressureReductionRatio(double pressureHPa, double elevationM, Temperature currentTemperature, Temperature meanTemperature, byte humidity, SLPAlgorithm algorithm = DefaultSLPAlgorithm)
        {
            switch (algorithm)
            {
                case SLPAlgorithm.DavisVP:
                    {
                        double hCorr = 0;
                        if (humidity > 0)
                        {
                            // BUG FIX: Legacy code used (9 / 5) which is integer division = 1.
                            // Corrected to (9.0 / 5.0) = 1.8 for proper °C to °F conversion factor.
                            hCorr = (9.0 / 5.0) * HumidityCorrection(currentTemperature, elevationM, humidity, VapAlgorithm.DavisVP);
                        }

                        return Power(10, (MToFt(elevationM) / (122.8943111 * (meanTemperature.Fahrenheit + 460 + (MToFt(elevationM) * VpLapseRateUS / 2) + hCorr))));
                    }

                case SLPAlgorithm.Univie:
                    {
                        double geopElevationM = GeopotentialAltitude(elevationM);
                        return Math.Exp(((Gravity / GasConstantAir) * geopElevationM) / (VirtualTempK(pressureHPa, meanTemperature, humidity) + (geopElevationM * StandardLapseRate / 2)));
                    }

                case SLPAlgorithm.ManBar:
                    {
                        double hCorr = 0;
                        if (humidity > 0)
                        {
                            // BUG FIX: Legacy code used (9 / 5) which is integer division = 1.
                            // Corrected to (9.0 / 5.0) = 1.8 for proper °C to °F conversion factor.
                            hCorr = (9.0 / 5.0) * HumidityCorrection(currentTemperature, elevationM, humidity, VapAlgorithm.Buck);
                        }

                        double geopElevationM = GeopotentialAltitude(elevationM);
                        return Math.Exp(geopElevationM * 6.1454E-2 / (meanTemperature.Fahrenheit + 459.7 + (geopElevationM * ManBarLapseRate / 2) + hCorr));
                    }

                default:
                    return 1;
            }
        }

        /// <summary>
        /// Computes the actual vapor pressure from temperature and relative humidity.
        /// </summary>
        /// <param name="temperature">The air temperature.</param>
        /// <param name="humidity">Relative humidity (0–100).</param>
        /// <param name="algorithm">The vapor pressure algorithm to use (default: Bolton).</param>
        /// <returns>The actual vapor pressure in hPa.</returns>
        public static double ActualVaporPressure(Temperature temperature, byte humidity, VapAlgorithm algorithm = DefaultVapAlgorithm)
        {
            return (humidity * SaturationVaporPressure(temperature, algorithm)) / 100;
        }

        /// <summary>
        /// Computes the saturation vapor pressure for a given temperature using the specified algorithm.
        /// </summary>
        /// <param name="temperature">The air temperature.</param>
        /// <param name="algorithm">The vapor pressure algorithm to use (default: Bolton).</param>
        /// <returns>The saturation vapor pressure in hPa.</returns>
        /// <remarks>
        /// Algorithm comparison available at http://cires.colorado.edu/~voemel/vp.html
        /// </remarks>
        public static double SaturationVaporPressure(Temperature temperature, VapAlgorithm algorithm = DefaultVapAlgorithm)
        {
            switch (algorithm)
            {
                case VapAlgorithm.DavisVP:
                    return 6.112 * Math.Exp((17.62 * temperature.Celsius) / (243.12 + temperature.Celsius));

                case VapAlgorithm.Buck:
                    return 6.1121 * Math.Exp((18.678 - (temperature.Celsius / 234.5)) * temperature.Celsius / (257.14 + temperature.Celsius));

                case VapAlgorithm.Buck81:
                    return 6.1121 * Math.Exp((17.502 * temperature.Celsius) / (240.97 + temperature.Celsius));

                case VapAlgorithm.Bolton:
                    return 6.112 * Math.Exp(17.67 * temperature.Celsius / (temperature.Celsius + 243.5));

                case VapAlgorithm.TetenNWS:
                    return 6.112 * Power(10, (7.5 * temperature.Celsius / (temperature.Celsius + 237.7)));

                case VapAlgorithm.TetenMurray:
                    return Power(10, (7.5 * temperature.Celsius / (237.5 + temperature.Celsius)) + 0.7858);

                case VapAlgorithm.Teten:
                    return 6.1078 * Power(10, (7.5 * temperature.Celsius / (temperature.Celsius + 237.3)));

                default:
                    return 0;
            }
        }

        /// <summary>
        /// Computes the mixing ratio (mass of water vapor per mass of dry air).
        /// </summary>
        /// <param name="pressureHPa">Atmospheric pressure in hPa.</param>
        /// <param name="temperature">The air temperature.</param>
        /// <param name="humidity">Relative humidity (0–100).</param>
        /// <returns>The mixing ratio in g/kg.</returns>
        public static double MixingRatio(double pressureHPa, Temperature temperature, byte humidity)
        {
            double vapPres = ActualVaporPressure(temperature, humidity, VapAlgorithm.Buck);
            return 1000 * (((MoleWater / MoleAir) * vapPres) / (pressureHPa - vapPres));
        }

        /// <summary>
        /// Computes the virtual temperature in Kelvin.
        /// </summary>
        /// <param name="pressureHPa">Atmospheric pressure in hPa.</param>
        /// <param name="temperature">The air temperature.</param>
        /// <param name="humidity">Relative humidity (0–100).</param>
        /// <returns>The virtual temperature in Kelvin.</returns>
        public static double VirtualTempK(double pressureHPa, Temperature temperature, byte humidity)
        {
            double epsilon = 1 - (MoleWater / MoleAir);
            double vapPres = ActualVaporPressure(temperature, humidity, VapAlgorithm.Buck);
            return temperature.Kelvin / (1 - (epsilon * (vapPres / pressureHPa)));
        }

        /// <summary>
        /// Computes the humidity correction factor for pressure reduction calculations.
        /// </summary>
        /// <param name="temperature">The air temperature.</param>
        /// <param name="elevationM">Station elevation in meters.</param>
        /// <param name="humidity">Relative humidity (0–100).</param>
        /// <param name="algorithm">The vapor pressure algorithm to use (default: Bolton).</param>
        /// <returns>The humidity correction factor.</returns>
        public static double HumidityCorrection(Temperature temperature, double elevationM, byte humidity, VapAlgorithm algorithm = DefaultVapAlgorithm)
        {
            double vapPress = ActualVaporPressure(temperature, humidity, algorithm);
            return vapPress * ((2.8322E-9 * (elevationM * elevationM)) + (2.225E-5 * elevationM) + 0.10743);
        }

        /// <summary>
        /// Computes the dew point temperature.
        /// </summary>
        /// <param name="temperature">The air temperature.</param>
        /// <param name="humidity">Relative humidity (0–100).</param>
        /// <param name="algorithm">The vapor pressure algorithm to use (default: Bolton).</param>
        /// <returns>A <see cref="Temperature"/> representing the dew point.</returns>
        public static Temperature DewPoint(Temperature temperature, byte humidity, VapAlgorithm algorithm = DefaultVapAlgorithm)
        {
            double lnVapor = Math.Log(ActualVaporPressure(temperature, humidity, algorithm));

            switch (algorithm)
            {
                case VapAlgorithm.DavisVP:
                    return Temperature.FromCelsius(((243.12 * lnVapor) - 440.1) / (19.43 - lnVapor));

                default:
                    return Temperature.FromCelsius(((237.7 * lnVapor) - 430.22) / (19.08 - lnVapor));
            }
        }

        /// <summary>
        /// Computes the wind chill temperature using the JAG/TI model.
        /// </summary>
        /// <param name="temperature">The air temperature.</param>
        /// <param name="windSpeedKmph">Wind speed in km/h.</param>
        /// <returns>A <see cref="Temperature"/> representing the wind chill. Returns the original
        /// temperature if conditions are outside the valid range (≥10°C or wind ≤4.8 km/h).</returns>
        /// <remarks>
        /// See http://www.msc.ec.gc.ca/education/windchill/science_equations_e.cfm
        /// </remarks>
        public static Temperature WindChill(Temperature temperature, double windSpeedKmph)
        {
            double result;

            if (temperature.Celsius >= 10.0 || windSpeedKmph <= 4.8)
            {
                result = temperature.Celsius;
            }
            else
            {
                double windPow = Power(windSpeedKmph, 0.16);
                result = 13.12 + (0.6215 * temperature.Celsius) - (11.37 * windPow) + (0.3965 * temperature.Celsius * windPow);
            }

            if (result > temperature.Celsius)
            {
                result = temperature.Celsius;
            }

            return Temperature.FromCelsius(result);
        }

        /// <summary>
        /// Computes the heat index using the Rothfusz regression with NWS adjustments.
        /// </summary>
        /// <param name="temperature">The air temperature.</param>
        /// <param name="humidity">Relative humidity (0–100).</param>
        /// <returns>A <see cref="Temperature"/> representing the heat index. Returns the original
        /// temperature if below 80°F.</returns>
        public static Temperature HeatIndex(Temperature temperature, byte humidity)
        {
            double result;

            if (temperature.Fahrenheit < 80)
            {
                result = temperature.Fahrenheit;
            }
            else
            {
                double tSqrd = temperature.Fahrenheit * temperature.Fahrenheit;
                double hum = humidity;
                double hSqrd = hum * hum;

                result = -42.379 + (2.04901523 * temperature.Fahrenheit) + (10.14333127 * humidity)
                      - (0.22475541 * temperature.Fahrenheit * humidity) - (0.00683783 * tSqrd)
                      - (0.05481717 * hSqrd) + (0.00122874 * tSqrd * humidity)
                      + (0.00085282 * temperature.Fahrenheit * hSqrd) - (0.00000199 * tSqrd * hSqrd);

                // Rothfusz adjustments
                if (humidity < 13 && temperature.Fahrenheit >= 80 && temperature.Fahrenheit <= 112)
                {
                    result = result - ((13 - humidity) / 4.0) * Math.Sqrt((17 - Math.Abs(temperature.Fahrenheit - 95)) / 17.0);
                }
                else if (humidity > 85 && temperature.Fahrenheit >= 80 && temperature.Fahrenheit <= 87)
                {
                    result = result + ((humidity - 85) / 10.0) * ((87 - temperature.Fahrenheit) / 5.0);
                }
            }

            return Temperature.FromFahrenheit(result);
        }

        /// <summary>
        /// Computes the Canadian humidex (apparent temperature accounting for humidity).
        /// </summary>
        /// <param name="temperature">The air temperature.</param>
        /// <param name="humidity">Relative humidity (0–100).</param>
        /// <returns>A <see cref="Temperature"/> representing the humidex value.</returns>
        public static Temperature Humidex(Temperature temperature, byte humidity)
        {
            // BUG FIX: Legacy code used (5 / 9) which is integer division = 0.
            // Corrected to (5.0 / 9.0) ≈ 0.5556 for proper °F to °C conversion factor.
            return Temperature.FromCelsius(temperature.Celsius + ((5.0 / 9.0) * (ActualVaporPressure(temperature, humidity, VapAlgorithm.TetenNWS) - 10.0)));
        }

        /// <summary>
        /// Computes the geopotential altitude from geometric altitude using the simplified
        /// US Standard Atmosphere 1976 formula (assumes latitude 45.5°).
        /// </summary>
        /// <param name="geometricAltitudeM">The geometric altitude in meters.</param>
        /// <returns>The geopotential altitude in meters.</returns>
        public static double GeopotentialAltitude(double geometricAltitudeM)
        {
            return (EarthRadius45 * 1000 * geometricAltitudeM) / ((EarthRadius45 * 1000) + geometricAltitudeM);
        }

        /// <summary>Converts feet to meters.</summary>
        /// <param name="value">The value in feet.</param>
        /// <returns>The value in meters.</returns>
        public static double FtToM(double value) => value * 0.3048;

        /// <summary>Converts meters to feet.</summary>
        /// <param name="value">The value in meters.</param>
        /// <returns>The value in feet.</returns>
        public static double MToFt(double value) => value / 0.3048;

        /// <summary>Converts inches to millimeters.</summary>
        /// <param name="value">The value in inches.</param>
        /// <returns>The value in millimeters.</returns>
        public static double InToMm(double value) => value * 25.4;

        /// <summary>Converts millimeters to inches.</summary>
        /// <param name="value">The value in millimeters.</param>
        /// <returns>The value in inches.</returns>
        public static double MmToIn(double value) => value / 25.4;

        /// <summary>Converts miles to kilometers.</summary>
        /// <param name="value">The value in miles.</param>
        /// <returns>The value in kilometers.</returns>
        public static double MilesToKm(double value) => value * 1.609344;

        /// <summary>Converts kilometers to miles.</summary>
        /// <param name="value">The value in kilometers.</param>
        /// <returns>The value in miles.</returns>
        public static double KmToMiles(double value) => value / 1.609344;

        /// <summary>
        /// Raises a base to the given power using logarithmic computation.
        /// </summary>
        private static double Power(double b, double exponent)
        {
            if (exponent == 0.0)
                return 1.0;
            else if (b == 0.0 && exponent > 0.0)
                return 0.0;
            else
                return Math.Exp(exponent * Math.Log(b));
        }

        /// <summary>
        /// Computes 10 raised to the given power.
        /// </summary>
        private static double Power10(double exponent)
        {
            const double ln10 = 2.302585093; // Ln(10)

            if (exponent == 0.0)
                return 1.0;
            else
                return Math.Exp(exponent * ln10);
        }
    }
}
