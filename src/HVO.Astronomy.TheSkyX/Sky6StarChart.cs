using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace HVO.Astronomy.TheSkyX
{
    public sealed class Sky6StarChart
    {
        private readonly TheSkyXClient _theSkyXClient;

        internal Sky6StarChart(TheSkyXClient theSkyXClient)
        {
            this._theSkyXClient = theSkyXClient;
        }

        public IEnumerable<(Sk6ObjectInformationProperty id, string name, string value)> ClickFind(int x, int y)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            string script = string.Format(Properties.Resources.TheSkyXScript_StarChartClickFindXY, x, y);
            var model = this._theSkyXClient.SendToTheSkyX(script, 1024, out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var result = JsonSerializer.Deserialize<List<ObjectInfoRecord>>(model, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            return result.Select(z => ((Sk6ObjectInformationProperty)z.Id, z.Name, z.Value)).ToArray();
        }

        public (double X, double Y) EquatorialToStarChartXY(double rightAscension, double declination)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6StarChart.EquatorialToStarChartXY({rightAscension}, {declination});");
            script.AppendLine("var objResult = { x: sky6StarChart.dOut0, y: sky6StarChart.dOut1 };");
            script.AppendLine("JSON.stringify(objResult);");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var result = JsonSerializer.Deserialize<XYPoint>(model, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
            return (result.X, result.Y);
        }

        public IEnumerable<(Sk6ObjectInformationProperty id, string name, string value)> Find(string objectName)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            string script = string.Format(Properties.Resources.TheSkyXScript_StarChartFindByName, objectName);
            var model = this._theSkyXClient.SendToTheSkyX(script, 10240, out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var result = JsonSerializer.Deserialize<List<ObjectInfoRecord>>(model, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
            return result.Select(x => ((Sk6ObjectInformationProperty)x.Id, x.Name, x.Value)).ToArray();
        }

        public void OpenStarChartSettings(string starChartSettingsPath)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6StarChart.Path = '{starChartSettingsPath?.Trim()}';");
            script.AppendLine($"sky6StarChart.Open();");

            this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
        }

        public void Refresh()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this._theSkyXClient.SendToTheSkyX("sky6StarChart.Refresh();", out var errorMessage);
        }

        public void SaveStarChartSettings(string starChartSettingsPath)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6StarChart.Path = '{starChartSettingsPath?.Trim()}';");
            script.AppendLine($"sky6StarChart.Save();");

            this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
        }

        public (double RightAscension, double Declination) StarChartXYToEquatorial(int x, int y)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6StarChart.StarChartXYToEquatorial({x}, {y});");
            script.AppendLine("var objResult = { rightAscension: sky6StarChart.dOut0, declination: sky6StarChart.dOut1 };");
            script.AppendLine("JSON.stringify(objResult);");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var result = JsonSerializer.Deserialize<RaDec>(model, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
            return (result.RightAscension, result.Declination);
        }

        public void UpdateSolarSystem()
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this._theSkyXClient.SendToTheSkyX("sky6StarChart.UpdateSolarSystem();", out var errorMessage);
        }

        public string GetDocumentProperty(Sk6DocumentProperty which)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6StarChart.DocumentProperty({(int)which});");
            script.AppendLine("sky6StarChart.DocPropOut;");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            return model;
        }

        public void SetDocumentProperty(Sk6DocumentProperty which, object value)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this._theSkyXClient.SendToTheSkyX($"sky6StarChart.SetDocumentProperty({(int)which}, '{value?.ToString()}');", out var errorMessage);
        }

        public object GetDisplayProperty(Sk6DisplayPropertyObjectType index, Sk6DisplayPropertySkyMode mode, Sk6DisplayProperty prop, Sk6DisplayPropertyItem item)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6StarChart.DisplayProperty({(int)index}, {(int)mode}, {(int)prop}, {(int)item});");
            script.AppendLine("sky6StarChart.DispPropOut;");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            return model;
        }

        public void SetDisplayProperty(Sk6DisplayPropertyObjectType index, Sk6DisplayPropertySkyMode mode, Sk6DisplayProperty prop, Sk6DisplayPropertyItem item, object value)
        {
            this._theSkyXClient.ThrowIfNotAttached();
            this._theSkyXClient.SendToTheSkyX($"sky6StarChart.SetDisplayProperty({(int)index}, {(int)mode}, {(int)prop}, {(int)item}, '{value?.ToString()}');", out var errorMessage);
        }

        public ImageSize GetStarChartSize()
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine("var objResult = { height: sky6StarChart.HeightInPixels, width: sky6StarChart.WidthInPixels };");
            script.AppendLine("JSON.stringify(objResult)");

            var model = this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new InvalidDataException("No response received.");
            }

            var result = JsonSerializer.Deserialize<ImageSize>(model, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true })!;
            return result;
        }

        public void SetStarChartSize(int height, int width)
        {
            this._theSkyXClient.ThrowIfNotAttached();

            var script = new StringBuilder();
            script.AppendLine($"sky6StarChart.HeightInPixels = {height};");
            script.AppendLine($"sky6StarChart.WidthInPixels = {width};");

            this._theSkyXClient.SendToTheSkyX(script.ToString(), out var errorMessage);
        }

        public double StarChartFieldOfView
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();

                var model = this._theSkyXClient.SendToTheSkyX("sky6StarChart.FieldOfView", out var errorMessage);
                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                double.TryParse(model, out var result);
                return result;
            }
            set
            {
                this._theSkyXClient.ThrowIfNotAttached();
                this._theSkyXClient.SendToTheSkyX($"sky6StarChart.FieldOfView = {value}", out var errorMessage);
            }
        }

        public double StarChartRotation
        {
            get
            {
                this._theSkyXClient.ThrowIfNotAttached();

                var model = this._theSkyXClient.SendToTheSkyX("sky6StarChart.Rotation", out var errorMessage);
                if (string.IsNullOrWhiteSpace(model))
                {
                    throw new InvalidDataException("No response received.");
                }

                double.TryParse(model, out var result);
                return result;
            }
            set
            {
                this._theSkyXClient.ThrowIfNotAttached();
                this._theSkyXClient.SendToTheSkyX($"sky6StarChart.Rotation = {value}", out var errorMessage);
            }
        }


        // Private helper class to aid in deserialization of XY coordinate results.
        private class XYPoint
        {
            public double X { get; set; }
            public double Y { get; set; }
        }

        // This is just a private helper class to aid in deserialization.  The MS provided deserialization does not yet support dynamic objects (ExpandoObject).
        private class RaDec
        {
            public double RightAscension
            {
                get; set;
            }
            public double Declination
            {
                get; set;
            }
        }

        // This is just a private helper class to aid in deserialization.  The MS provided deserialization does not yet support dynamic objects (ExpandoObject).
        private class AltAz
        {
            public double Altitude
            {
                get; set;
            }
            public double Azimuth
            {
                get; set;
            }
        }
        private class ObjectInfoRecord
        {
            public Sk6ObjectInformationProperty Id
            {
                get; set;
            }
            public string Name
            {
                get; set;
            } = string.Empty;
            public string Value
            {
                get; set;
            } = string.Empty;
        }
    }

    public enum Sk6DocumentProperty
    {
        sk6DocProp_Latitude = 0,
        sk6DocProp_Longitude = 1,
        sk6DocProp_Time_Zone = 2,
        sk6DocProp_ElevationInMeters = 3,
        sk6DocProp_DaylightSavingTimeIndex = 4,
        sk6DocProp_UseComputerClock = 5,
        sk6DocProp_StellarRecomputeInterval = 6,
        sk6DocProp_TimeSkipInterval = 7,
        sk6DocProp_PlanetRecomputeInterval = 8,
        sk6DocProp_JulianDateNow = 9,
        sk6DocProp_OrientationIndex = 10,
        sk6DocProp_PlotGSC = 11,
        sk6DocProp_PlotHipTyc = 12,
        sk6DocProp_PlotUCAC = 13,
        sk6DocProp_PlotUSNOA = 14,
        sk6DocProp_PlotUSNOSA = 15,
        sk6DocProp_PlotUSNOBFull = 16,
        sk6DocProp_PlotUSNOBSubset = 17,
        sk6DocProp_labels_on = 18,
        sk6DocProp_ext_opts = 19,
        sk6DocProp_ext_opts_comet = 20,
        sk6DocProp_NorthEastIndicator = 21,
        sk6DocProp_Mirror = 22,
        sk6DocProp_status_bar_opts = 23,
        sk6DocProp_nSpectralPercent = 24,
        sk6DocProp_NightVisionStarsRed = 25,
        sk6DocProp_nstell_gsc_opt = 26,
        sk6DocProp_ShowSpectralColors = 27,
        sk6DocProp_ScreenPopulationIndex = 28,
        sk6DocProp_ComputeProperMotion = 29,
        sk6DocProp_ProperMotionVectorLength = 30,
        sk6DocProp_TransparentMoon = 31,
        sk6DocProp_VirtualDisplayIndex = 32,
        sk6DocProp_ShowEarthShadow = 33,
        sk6DocProp_common_labels = 34,
        sk6DocProp_3DSolarSystemView = 35,
        sk6DocProp_LockedIndex = 36,
        sk6DocProp_ViewFromIndex = 37,
        sk6DocProp_TrailTypeIndex = 38,
        sk6DocProp_StartEq = 39,
        sk6DocProp_StartHz = 40,
        sk6DocProp_StartFW = 41,
        sk6DocProp_OrthographicProjection = 42,
        sk6DocProp_ShowRefractedHoizonToo = 43,
        sk6DocProp_cMWType = 44,
        sk6DocProp_ShowConstellations = 45,
        sk6DocProp_cShowHzPoleup = 46,
        sk6DocProp_nNSTGradient = 47,
        sk6DocProp_GridSpacingIndex = 48,
        sk6DocProp_RealModeHorizononDisplayTypeIndex = 49,
        sk6DocProp_GridRAIncrement = 50,
        sk6DocProp_GridDecIncrement = 51,
        sk6DocProp_extended_labels = 52,
        sk6DocProp_m_page_opts = 53,
        sk6DocProp_SAOPrefix = 54,
        sk6DocProp_NGCPrefix = 55,
        sk6DocProp_ICPrefix = 56,
        sk6DocProp_UGCPrefix = 57,
        sk6DocProp_CGCGPrefix = 58,
        sk6DocProp_GCVSPrefix = 59,
        sk6DocProp_ShowOverlappingCatalogs = 60,
        sk6DocProp_SkyDisplayPropertiesFileName = 61,
        sk6DocProp_ShowAtmosphericBackground = 62,
        sk6DocProp_LocationDescription = 63,
        sk6DocProp_UserRotateDegrees = 64,
        sk6DocProp_RealModeHorizononFileName = 65,
        sk6DocProp_AltitudeLimitFileName = 66,
        sk6DocProp_DecLimitFileName = 67,
        sk6DocProp_csLocationFileFileName = 68,
        sk6DocProp_plot_nst = 69,
        sk6DocProp_Recording = 70,
        sk6DocProp_bUSNORawUseVersion1 = 71,
        sk6DocProp_bFillScreen = 72,
        sk6DocProp_bHollowDeepSky = 73,
        sk6DocProp_AuxData = 74,
        sk6DocProp_nWideFieldProj = 75,
        sk6DocProp_bSSOnly = 76,
        sk6DocProp_SSOriginx = 77,
        sk6DocProp_SSOriginy = 78,
        sk6DocProp_SSOriginz = 79,
        sk6DocProp_SSdRho = 80,
        sk6DocProp_SSdTheta = 81,
        sk6DocProp_SSdPhi = 82,
        sk6DocProp_SSdDistanceAU = 83,
        sk6DocProp_SSbLineToEcliptic = 84,
        sk6DocProp_SScsLockedName = 85,
        sk6DocProp_SScsViewFromName = 86,
        sk6DocProp_StarPlotBrtCon = 87,
        sk6DocProp_RealModeHorizonIsVisible = 88,
        sk6DocProp_RealModeHorizonRPercent = 89,
        sk6DocProp_RealModeHorizonGPercent = 90,
        sk6DocProp_RealModeHorizonBPercent = 91,
        sk6DocProp_RealModeHorizonSilhouette = 92,
        sk6DocProp_RealModeHorizonGlowIsVisible = 93,
        sk6DocProp_RealModeHorizonRGlowOffset = 94,
        sk6DocProp_RealModeHorizonGGlowOffset = 95,
        sk6DocProp_RealModeHorizonBGlowOffset = 96,
        sk6DocProp_RealModeHorizondGlowAltitudeTop = 97,
        sk6DocProp_RealModeHorizondGlowAltitudeBottom = 98,
        sk6DocProp_RealModeConstellationGroupVisible = 99,
        sk6DocProp_RealModeConstellationROffset = 100,
        sk6DocProp_RealModeConstellationGOffset = 101,
        sk6DocProp_RealModeConstellationBOffset = 102,
        sk6DocProp_RealModeConstellationbOnlyNearCenter = 103,
        sk6DocProp_RealModeConstellationbBrightenNearCenter = 104,
        sk6DocProp_RealModeMilkyWayIsVisible = 105,
        sk6DocProp_RealModeMilkyWayROffset = 106,
        sk6DocProp_RealModeMilkyWayGOffset = 107,
        sk6DocProp_RealModeMilkyWayBOffset = 108,
        sk6DocProp_RealModeAtmosphereSunHaloVisible = 109,
        sk6DocProp_RealModeAtmosphereRSunHalo = 110,
        sk6DocProp_RealModeAtmosphereGSunHalo = 111,
        sk6DocProp_RealModeAtmosphereBSunHalo = 112,
        sk6DocProp_RealModeAtmosphereSunHaloDegrees = 113,
        sk6DocProp_RealModeAtmosphereMoonHaloVisible = 114,
        sk6DocProp_RealModeAtmosphereMoonHaloDegrees = 115,
        sk6DocProp_RealModeAtmosphereSunHzGlowVisible = 116,
        sk6DocProp_RealModeAtmosphereRSunHzGlow = 117,
        sk6DocProp_RealModeAtmosphereGSunHzGlow = 118,
        sk6DocProp_RealModeAtmosphereBSunHzGlow = 119,
        sk6DocProp_RealModeAtmospheredSunHzGlowDegrees = 120,
        sk6DocProp_RealModeAtmosphereCloudsVisible = 121,
        sk6DocProp_RealModeAtmosphereRCloud = 122,
        sk6DocProp_RealModeAtmosphereGCloud = 123,
        sk6DocProp_RealModeAtmosphereBCloud = 124,
        sk6DocProp_RealModeAtmospherebBackgroundIsVisible = 125,
        sk6DocProp_RealModeAtmospherenBackgroundR = 126,
        sk6DocProp_RealModeAtmospherenBackgroundG = 127,
        sk6DocProp_RealModeAtmospherenBackgroundB = 128,
        sk6DocProp_InPlaceImages = 129,
        sk6DocProp_ClipInPlace = 130,
        sk6DocProp_InPlaceBorders = 131,
        sk6DocProp_DefaultFrameSize = 132,
        sk6DocProp_ExtendLabels = 133,
        sk6DocProp_CatalogActive = 134,
        sk6DocProp_SmoothedStars = 135,
        sk6DocProp_bUseHalos = 136,
        sk6DocProp_nFntAdjust = 137,
        sk6DocProp_nSatAdjust = 138,
        sk6DocProp_nMaxBinAdjust = 139,
        sk6DocProp_dBrtAdjust = 140,
        sk6DocProp_dConAdjust = 141,
        sk6DocProp_dNarrowBrt = 142,
        sk6DocProp_dNarrowCon = 143,
        sk6DocProp_bDrawPMVectors = 144,
        sk6DocProp_bHorizonInPoleUp = 145,
        sk6DocProp_MosaicParms = 146,
        sk6DocProp_bUserDataVisible = 147,
        sk6DocProp_bConstlDrawings = 148,
        sk6DocProp_dFocusScale = 149,
        sk6DocProp_bOutline = 150,
        sk6DocProp_bLabel = 151,
        sk6DocProp_bGrid = 152,
        sk6DocProp_bClemHigh = 153,
        sk6DocProp_nMaskOpacity = 154,
        sk6DocProp_LayerInfo = 155,
        sk6DocProp_nLayerResX = 156,
        sk6DocProp_nLayerResY = 157,
        sk6DocProp_bResReset = 158,
        sk6DocProp_csLayerPrefix = 159,
        sk6DocProp_bDrawPaths = 160,
        sk6DocProp_nTimeCnt = 161,
        sk6DocProp_dTimeIncr = 162,
        sk6DocProp_dBrtAdjustChartMode = 163,
        sk6DocProp_dConAdjustChartMode = 164,
        sk6DocProp_nFntAdjustChartMode = 165,
        sk6DocProp_nSatAdjustChartMode = 166,
        sk6DocProp_dFocusScaleChartMode = 167,
        sk6DocProp_bRotateFOVToIlink = 168,
        sk6DocProp_m_dILinkPercent = 169,
        sk6DocProp_m_nILinkImageStatus = 170,
        sk6DocProp_m_bILinkImageKnowLink = 171,
        sk6DocProp_m_nILinkSearchType = 172,
        sk6DocProp_m_dILinkSearchScale = 173,
        sk6DocProp_m_bILinkShowImageFrame = 174,
        sk6DocProp_m_bILinkShowTelescopeFrame = 175,
        sk6DocProp_m_nILinkSearchRadius = 176,
        sk6DocProp_m_dILinkMatchPerc = 177,
        sk6DocProp_m_dILinkScaleToler = 178,
        sk6DocProp_m_bILinkAllowMirror = 179,
        sk6DocProp_SDPIDLast = 180
    }

    public enum Sk6DisplayPropertyObjectType
    {
        OT6_NOTKNOWN = -1,
        OT6_STAR = 0,
        OT6_VARIABLE_STAR = 1,
        OT6_SUSPECTED_VAR = 2,
        OT6_DOUBLE_STAR = 3,
        OT6_GALAXY = 4,
        OT6_GAL_C = 5,
        OT6_GAL_ELLIPTICAL = 6,
        OT6_GAL_LENTICULAR = 7,
        OT6_GAL_SPIRAL = 8,
        OT6_GAL_IRREGULAR = 9,
        OT6_GAL_CLUSTER = 10,
        OT6_OPEN_CLUSTER = 11,
        OT6_GLOB_CLUSTER = 12,
        OT6_CLUSTER_NEB = 13,
        OT6_NEBULA = 14,
        OT6_BRIGHT_NEB = 15,
        OT6_DARK_NEB = 16,
        OT6_PLAN_NEBULA = 17,
        OT6_ACTUAL_STAR = 18,
        OT6_OTHER_NGC = 19,
        OT6_MIXEDDEEPSKY = 20,
        OT6_NST_GSC = 21,
        OT6_QUASAR = 22,
        OT6_XRAY_SOURCE = 23,
        OT6_RADIO_SOURCE = 24,
        OT6_SUN = 25,
        OT6_MERCURY = 26,
        OT6_VENUS = 27,
        OT6_EARTH = 28,
        OT6_MARS = 29,
        OT6_JUPITER = 30,
        OT6_SATURN = 31,
        OT6_URANUS = 32,
        OT6_NEPTUNE = 33,
        OT6_PLUTO = 34,
        OT6_MOON = 35,
        OT6_COMET = 36,
        OT6_ASTEROID = 37,
        OT6_EXTCOMET = 38,
        OT6_EXTASTEROID = 39,
        OT6_SPACECRAFT = 40,
        OT6_IMAGE = 41,
        OT6_VIDEO = 42,
        OT6_SOUND = 43,
        OT6_NOTES = 44,
        OT6_CONSTELLFIGURE = 45,
        OT6_CONSTELLATION = 46,
        OT6_ECLIPTIC = 47,
        OT6_HORIZON_GRID = 48,
        OT6_HORIZON_LINE = 49,
        OT6_MERIDIAN = 50,
        OT6_EQUATORIALGRID = 51,
        OT6_GALACTICEQ = 52,
        OT6_MILKYWAY = 53,
        OT6_REFERENCELINE = 54,
        OT6_REFERENCEPOINT = 55,
        OT6_EYEPIECE = 56,
        OT6_CHART = 57,
        OT6_LEGEND = 58,
        OT6_SCOPE_LIMIT = 59,
        OT6_SUPER_NOVAE = 60,
        OT6_NE_INDICATOR = 61,
        OT6_NE_MOSAIC = 62,
        OT6_ZOOMBOX = 63,
        OT6_CONSTELL_ART = 64,
        OT6_SBRESERVED1 = 65,
        OT6_SBRESERVED2 = 66,
        OT6_SBRESERVED3 = 67,
        OT6_SBRESERVED4 = 68,
        OT6_SBRESERVED5 = 69,
        OT6_USER0 = 70,
        OT6_USER1 = 71,
        OT6_USER2 = 72,
        OT6_USER3 = 73,
        OT6_USER4 = 74,
        OT6_USER5 = 75,
        OT6_USER6 = 76,
        OT6_USER7 = 77,
        OT6_USER8 = 78,
        OT6_USER9 = 79,
        OT6_USER10 = 80,
        OT6_USER11 = 81,
        OT6_USER12 = 82,
        OT6_USER13 = 83,
        OT6_USER14 = 84,
        OT6_USER15 = 85,
        OT6_USER16 = 86,
        OT6_USER17 = 87,
        OT6_USER18 = 88,
        OT6_USER19 = 89,
        OT6_LAST = 90
    }

    public enum Sk6DisplayPropertySkyMode
    {
        sk6DisplayPropertySkyModeVirtualSky = 0,
        sk6DisplayPropertySkyModeChartMode = 1
    }

    public enum Sk6DisplayProperty
    {
        sk6DisplayPropertyVisible = 0,
        sk6DisplayPropertyFont = 1,
        sk6DisplayPropertyLine = 2,
        sk6DisplayPropertyFill = 3,
        sk6DisplayPropertyMagnitudeLimit = 4
    }

    public enum Sk6DisplayPropertyItem
    {
        sk6DisplayPropertyItemVisibleValue = 0,
        sk6DisplayPropertyItemFontName = 1,
        sk6DisplayPropertyItemFontWeight = 2,
        sk6DisplayPropertyItemFontSize = 3,
        sk6DisplayPropertyItemFontColor = 4,
        sk6DisplayPropertyItemFontItalic = 5,
        sk6DisplayPropertyItemLineStyle = 6,
        sk6DisplayPropertyItemLineColor = 7,
        sk6DisplayPropertyItemLineWidth = 8,
        sk6DisplayPropertyItemMagnitudeLimitBrightest = 9,
        sk6DisplayPropertyItemMagnitudeLimitDimmest = 10,
        sk6DisplayPropertyItemFillColor = 11
    }

    public enum Sk6DisplayPropertyFontWeight
    {
        sk6FontWeightNormal = 400,
        sk6FontWeightBold = 700
    }

    public enum Sk6DisplayPropertyLineStyle
    {
        sk6LineStyleSolid = 0,
        sk6LineStyleDash = 1,
        sk6LineStyleDot = 2,
        sk6LineStyleDashDot = 3,
        sk6LineStyleDashDotDot = 4
    }

    public enum Sk6DisplayPropertyLineWidth
    {
        sk6LineWidthOnePixel = 0,
        sk6LineWidthTwoPixels = 1,
        sk6LineWidthThreePixels = 2,
        sk6LineWidthFourPixels = 3,
        sk6LineWidthFivePixels = 4
    }

    public enum Sk6ObjectInformationProperty
    {
        sk6ObjInfoProp_NAME1 = 0,
        sk6ObjInfoProp_NAME2 = 1,
        sk6ObjInfoProp_NAME3 = 2,
        sk6ObjInfoProp_NAME4 = 3,
        sk6ObjInfoProp_NAME5 = 4,
        sk6ObjInfoProp_NAME6 = 5,
        sk6ObjInfoProp_NAME7 = 6,
        sk6ObjInfoProp_NAME8 = 7,
        sk6ObjInfoProp_NAME9 = 8,
        sk6ObjInfoProp_NAME10 = 9,
        sk6ObjInfoProp_CATALOGID = 10,
        sk6ObjInfoProp_ALL_INFO = 11,
        sk6ObjInfoProp_CO_NAME = 12,
        sk6ObjInfoProp_RISE_SET_INFO = 13,
        sk6ObjInfoProp_STAR_SPECTRAL = 14,
        sk6ObjInfoProp_STAR_BAYER_FLAMSTEED = 15,
        sk6ObjInfoProp_SATELLITE_NAME = 16,
        sk6ObjInfoProp_SATELLITE_TLE1 = 17,
        sk6ObjInfoProp_SATELLITE_TLE2 = 18,
        sk6ObjInfoProp_SOURCE_CATALOG = 19,
        sk6ObjInfoProp_DB_FIELD_0 = 20,
        sk6ObjInfoProp_DB_FIELD_1 = 21,
        sk6ObjInfoProp_DB_FIELD_2 = 22,
        sk6ObjInfoProp_DB_FIELD_3 = 23,
        sk6ObjInfoProp_DB_FIELD_4 = 24,
        sk6ObjInfoProp_DB_FIELD_5 = 25,
        sk6ObjInfoProp_DB_FIELD_6 = 26,
        sk6ObjInfoProp_DB_FIELD_7 = 27,
        sk6ObjInfoProp_DB_FIELD_8 = 28,
        sk6ObjInfoProp_DB_FIELD_9 = 29,
        sk6ObjInfoProp_DB_FIELD_10 = 30,
        sk6ObjInfoProp_DB_FIELD_11 = 31,
        sk6ObjInfoProp_DB_FIELD_12 = 32,
        sk6ObjInfoProp_DB_FIELD_13 = 33,
        sk6ObjInfoProp_DB_FIELD_14 = 34,
        sk6ObjInfoProp_DB_FIELD_15 = 35,
        sk6ObjInfoProp_TEXT_LINE = 36,
        sk6ObjInfoProp_DATE = 37, sk6ObjInfoProp_TIME = 38, sk6ObjInfoProp_OBSERVING_NOTES = 39,
        sk6ObjInfoProp_CATID = 40, sk6ObjInfoProp_OBJECTTYPE = 41, sk6ObjInfoProp_STAR_ID = 42, sk6ObjInfoProp_STAR_GSC_BLOCK = 43,
        sk6ObjInfoProp_STAR_GSC_NUM = 44, sk6ObjInfoProp_INDEX = 45, sk6ObjInfoProp_NGC_IC_NEG_ID = 46, sk6ObjInfoProp_SKIP_INDEX = 47,
        sk6ObjInfoProp_NST_FIELD_CNT = 48, sk6ObjInfoProp_PERINFO_TEXTPOSN = 49, sk6ObjInfoProp_ACTIVE = 50, sk6ObjInfoProp_SATELLITE_ECLIPSED = 51,
        sk6ObjInfoProp_SATELLITE_IS_EXT = 52, sk6ObjInfoProp_CATALOG = 53, sk6ObjInfoProp_RA_NOW = 54, sk6ObjInfoProp_DEC_NOW = 55,
        sk6ObjInfoProp_RA_2000 = 56, sk6ObjInfoProp_DEC_2000 = 57, sk6ObjInfoProp_AZM = 58, sk6ObjInfoProp_ALT = 59,
        sk6ObjInfoProp_MAJ_AXIS_MINS = 60, sk6ObjInfoProp_MIN_AXIS_MINS = 61, sk6ObjInfoProp_EARTH_DIST_KM = 62, sk6ObjInfoProp_SUN_DIST_AU = 63,
        sk6ObjInfoProp_PA = 64, sk6ObjInfoProp_MAG = 65, sk6ObjInfoProp_PHASE_PERC = 66, sk6ObjInfoProp_RISE_TIME = 67,
        sk6ObjInfoProp_TRANSIT_TIME = 68, sk6ObjInfoProp_SET_TIME = 69, sk6ObjInfoProp_HA_HOURS = 70, sk6ObjInfoProp_AIR_MASS = 71,
        sk6ObjInfoProp_STAR_MAGB = 72, sk6ObjInfoProp_STAR_MAGV = 73, sk6ObjInfoProp_STAR_MAGR = 74, sk6ObjInfoProp_SCREEN_X = 75,
        sk6ObjInfoProp_SCREEN_Y = 76, sk6ObjInfoProp_RA_RATE_ASPERSEC = 77, sk6ObjInfoProp_DEC_RATE_ASPERSEC = 78, sk6ObjInfoProp_ALT_RATE_ASPERSEC = 79,
        sk6ObjInfoProp_AZIM_RATE_ASPERSEC = 80, sk6ObjInfoProp_AZIM_RISE_DEGS = 81, sk6ObjInfoProp_AZIM_SET_DEGS = 82, sk6ObjInfoProp_MPL_ACTIVE = 83,
        sk6ObjInfoProp_MPL_EPOCH_M = 84, sk6ObjInfoProp_MPL_EPOCH_D = 85, sk6ObjInfoProp_MPL_EPOCH_Y = 86, sk6ObjInfoProp_MPL_MA = 87,
        sk6ObjInfoProp_MPL_ECCENT = 88, sk6ObjInfoProp_MPL_SEMIMAJOR = 89, sk6ObjInfoProp_MPL_INCLIN = 90, sk6ObjInfoProp_MPL_LAN = 91,
        sk6ObjInfoProp_MPL_LONG_PERI = 92, sk6ObjInfoProp_MPL_ECLIP = 93, sk6ObjInfoProp_MPL_MAGPARM1 = 94, sk6ObjInfoProp_MPL_MAGPARM2 = 95,
        sk6ObjInfoProp_COMET_PERIH_M = 96, sk6ObjInfoProp_COMET_PERIH_D = 97, sk6ObjInfoProp_COMET_PERIH_Y = 98, sk6ObjInfoProp_COMET_ECCENT = 99,
        sk6ObjInfoProp_COMET_PERIDIST = 100, sk6ObjInfoProp_COMET_INCLIN = 101, sk6ObjInfoProp_COMET_LAN = 102, sk6ObjInfoProp_COMET_LONG_PERI = 103,
        sk6ObjInfoProp_COMET_ECLIP = 104, sk6ObjInfoProp_COMET_MAGPARM1 = 105, sk6ObjInfoProp_COMET_MAGPARM2 = 106, sk6ObjInfoProp_PLANET_SHELIO_L = 107,
        sk6ObjInfoProp_PLANET_SHELIO_B = 108, sk6ObjInfoProp_PLANET_SHELIO_R = 109, sk6ObjInfoProp_PLANET_SGEO_L = 110, sk6ObjInfoProp_PLANET_SGEO_B = 111,
        sk6ObjInfoProp_PLANET_SGEO_R = 112, sk6ObjInfoProp_PLANET_SGEOMEAN_L = 113, sk6ObjInfoProp_PLANET_SGEOMEAN_B = 114, sk6ObjInfoProp_PLANET_SGEOMEAN_R = 115,
        sk6ObjInfoProp_PLANET_TRUE_RA = 116, sk6ObjInfoProp_PLANET_TRUE_DEC = 117, sk6ObjInfoProp_PLANET_ALTWREFRACT = 118, sk6ObjInfoProp_PLANET_APPMAG = 119,
        sk6ObjInfoProp_PLANET_APPANGDIAM = 120, sk6ObjInfoProp_MOON_TRUE_ECLIP_L = 121, sk6ObjInfoProp_MOON_TRUE_ECLIP_B = 122, sk6ObjInfoProp_MOON_TRUE_ECLIP_R = 123,
        sk6ObjInfoProp_MOON_PARALLAX = 124, sk6ObjInfoProp_MOON_ANGDIAM = 125, sk6ObjInfoProp_MOON_DIST_KM = 126, sk6ObjInfoProp_MOON_TRUE_EQ_RA = 127,
        sk6ObjInfoProp_MOON_TRUE_EQ_DEC = 128, sk6ObjInfoProp_MOON_TOPO_ANG_DIAM = 129, sk6ObjInfoProp_MOON_ALT_WREFRACT = 130, sk6ObjInfoProp_MOON_TOTAL_LIBR_L = 131,
        sk6ObjInfoProp_MOON_TOTAL_LIBR_B = 132, sk6ObjInfoProp_MOON_OPTICAL_LIBR_L = 133, sk6ObjInfoProp_MOON_OPTICAL_LIBR_B = 134, sk6ObjInfoProp_MOON_PHYS_LIBR_L = 135,
        sk6ObjInfoProp_MOON_PHYS_LIBR_B = 136, sk6ObjInfoProp_MOON_POS_ANGLE = 137, sk6ObjInfoProp_MOON_PHASE_ANGLE = 138, sk6ObjInfoProp_MOON_PABL = 139,
        sk6ObjInfoProp_SUN_POS_ANGLE = 140, sk6ObjInfoProp_SUN_HELIO_LONG = 141, sk6ObjInfoProp_SUN_HELIO_LAT = 142, sk6ObjInfoProp_DECL_SUN = 143,
        sk6ObjInfoProp_DECL_EARTH = 144, sk6ObjInfoProp_POLAR_DIAM = 145, sk6ObjInfoProp_LCM_I = 146, sk6ObjInfoProp_LCM_II = 147,
        sk6ObjInfoProp_MARS_DEFECT_ILLUM = 148, sk6ObjInfoProp_JUPITER_CRCT_PHASE = 149, sk6ObjInfoProp_SATURN_ARING_AXIS = 150, sk6ObjInfoProp_SATURN_BRING_AXIS = 151,
        sk6ObjInfoProp_SAT_JD = 152, sk6ObjInfoProp_SAT_LAT = 153, sk6ObjInfoProp_SAT_LON = 154, sk6ObjInfoProp_SAT_EARTH_ALT = 155,
        sk6ObjInfoProp_SAT_RANGE = 156, sk6ObjInfoProp_SAT_RANGE_RATE = 157, sk6ObjInfoProp_SAT_DEPTH_EC = 158, sk6ObjInfoProp_STAR_PARALLAX = 159,
        sk6ObjInfoProp_STAR_PM_RA = 160, sk6ObjInfoProp_STAR_PM_DEC = 161, sk6ObjInfoProp_STAR_POS_ERR_RA = 162, sk6ObjInfoProp_STAR_POS_ERR_DEC = 163,
        sk6ObjInfoProp_STAR_POS_ERR_PRLX = 164, sk6ObjInfoProp_STAR_PM_POS_ERR_RA = 165, sk6ObjInfoProp_STAR_PM_POS_ERR_DEC = 166, sk6ObjInfoProp_TWIL_CIVIL_START = 167,
        sk6ObjInfoProp_TWIL_CIVIL_END = 168, sk6ObjInfoProp_TWIL_NAUT_START = 169, sk6ObjInfoProp_TWIL_NAUT_END = 170, sk6ObjInfoProp_TWIL_ASTRON_START = 171,
        sk6ObjInfoProp_TWIL_ASTRON_END = 172, sk6ObjInfoProp_SIDEREAL = 173, sk6ObjInfoProp_JUL_DATE = 174, sk6ObjInfoProp_CLICK_DIST = 175,
        sk6ObjInfoProp_POINT3D_X = 176, sk6ObjInfoProp_POINT3D_Y = 177, sk6ObjInfoProp_POINT3D_Z = 178, sk6ObjInfoProp_FRAME_SIZE_MINS = 179,
        sk6ObjInfoProp_SUN_DIST_LY = 180, sk6ObjInfoProp_DIST_PARSEC = 181, sk6ObjInfoProp_SCALE_ASPIX = 182, sk6ObjInfoProp_HEIGHT = 183,
        sk6ObjInfoProp_WIDTH = 184, sk6ObjInfoProp_UMBRA_RAD = 185, sk6ObjInfoProp_PENUMBRA_RAD = 186, sk6ObjInfoProp_ANG_SEP_PRIOR = 187,
        sk6ObjInfoProp_PA_PRIOR = 188, sk6ObjInfoProp_COUNT = 189
    }
}
