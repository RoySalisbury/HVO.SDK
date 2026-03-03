
using System;
using System.Collections.Generic;
using System.Text;

namespace HVO.Astronomy.TheSkyX
{
    public class CCDSoftImage
    {
        private readonly TheSkyXClient _theSkyXClient;

        internal CCDSoftImage(TheSkyXClient theSkyXClient)
        {
            this._theSkyXClient = theSkyXClient;
        }


        int Open(string pathToFile)
        {
            throw new NotImplementedException();
        }

        int Save(string pathToSave)
        {
            throw new NotImplementedException();
        }

        int Close()
        {
            throw new NotImplementedException();
        }

        int AttachToActive(string pathToFile)
        {
            throw new NotImplementedException();
        }

        int ApplyBackgroundRange()
        {
            throw new NotImplementedException();
        }

        int NewImage(int Width, int Height, int BitsPerPixel)
        {
            throw new NotImplementedException();
        }

        int Zoom(int Numerator, int Denominator)
        {
            throw new NotImplementedException();
        }

        int SetActive()
        {
            throw new NotImplementedException();
        }

        int AttachToActiveImager()
        {
            throw new NotImplementedException();
        }

        int AttachToActiveAutoguider()
        {
            throw new NotImplementedException();
        }

        object GetFITSKeyword(string qsKeyword)
        {
            throw new NotImplementedException();
        }

        void setFITSKeyword(string qsKeyword, object value)
        {
            throw new NotImplementedException();
        }


        int InsertWCS(bool RedoExistingSolution = false)
        {
            throw new NotImplementedException();
        }

        int XYToRADec(double X, double Y)
        {
            throw new NotImplementedException();
        }

        int RADecToXY(double RA, double Dec)
        {
            throw new NotImplementedException();
        }

        object WCSArray(int WCSIndex)
        {
            throw new NotImplementedException();
        }

        int ShowInventory()
        {
            throw new NotImplementedException();
        }

        object InventoryArray(int InventoryIndex)
        {
            throw new NotImplementedException();
        }

        object FindInventoryAtRADec(double RA, double Dec)
        {
            throw new NotImplementedException();
        }


        double averagePixelValue()
        {
            throw new NotImplementedException();
        }

        object scanLine(int i)
        {
            throw new NotImplementedException();
        }

        double XYToRADecResultRA()
        {
            throw new NotImplementedException();
        }

        double XYToRADecResultDec()
        {
            throw new NotImplementedException();
        }

        double RADecToXYResultX()
        {
            throw new NotImplementedException();
        }

        double RADecToXYResultY()
        {
            throw new NotImplementedException();
        }


        double JulianDay
        {
            get; set;
        }

        string Path
        {
            get; set;
        } = string.Empty;

        int DetachOnClose
        {
            get; set;
        }

        int Visible
        {
            get; set;
        }

        int WidthInPixels
        {
            get; set;
        }

        int HeightInPixels
        {
            get; set;
        }

        int ModifiedFlag
        {
            get; set;
        }

        int Background
        {
            get; set;
        }

        int Range
        {
            get; set;
        }

        double NorthAngle
        {
            get; set;
        }

        double ScaleInArcsecondsPerPixelNorthAngle
        {
            get; set;
        }

    }

    enum ccdsoftInventoryIndex
    {
        cdInventoryX, cdInventoryY, cdInventoryMagnitude, cdInventoryClass,
        cdInventoryFWHM, cdInventoryMajorAxis, cdInventoryMinorAxis, cdInventoryTheta,
        cdInventoryEllipticity
    }

    enum ccdsoftWCSIndex
    {
        cdWCSRA, cdWCSDec, cdWCSX, cdWCSY,
        cdWCSPositionError, cdWCSResidual, cdWCSCatalogID, cdActive
    }

    enum ccdsoftAutoContrastMethod
    {
        cdAutoContrastUseAppSetting = -1, cdAutoContrastSBIG, cdAutoContrastBjorn, cdAutoContrastDSS100X
    }

    enum ccdsoftBjornBackground
    {
        cdBgNone, cdBgWeak, cdBgMedium, cdBgStrong,
        cdBgVeryStrong
    }

    enum ccdsoftBjornHighlight
    {
        cdHLNone, cdHLWeak, cdHLMedium, cdHLStrong,
        cdHLVeryStrong, cdHLAdaptive, cdHLPlanetary
    }

    enum ccdsoftCoordinates
    {
        cdRA, cdDec
    }

    enum ccdsoftSaveAs
    {
        cdGIF, cdBMP, cdJPG, cd48BitTIF
    }


}
