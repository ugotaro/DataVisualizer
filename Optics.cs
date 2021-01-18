using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UGO.Process
{
    public class Optics
    {
        public static double PositionToGalvanoVoltage(double position, double centerOffset, double radPerV, double magnification, double focalLength)
        {
            double positionOnPupil = (position - centerOffset) * magnification;
            double radian = Math.Atan2(positionOnPupil,focalLength);
            double voltage = radian / radPerV;
            return voltage;
        }

        public static double VoltageToGalvanoPosition(double voltage, double centerOffset, double radPerV, double magnification, double focalLength)
        {
            double radian = voltage * radPerV;
            double positionOnPupil = focalLength * Math.Tan(radian);
            double position = positionOnPupil / magnification + centerOffset;
            return position;
        }
    }
}
