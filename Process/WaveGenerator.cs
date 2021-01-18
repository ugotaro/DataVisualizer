using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UGO.Process
{
    public class SPASIM_WaveGenerator
    {
        public static double[] GenerateLaserPulse(double th, double td, double tw, double period, int repetetion, double vMax, double vMin, double deviceDelay)
        {
            int length = (int)(period * repetetion / th);
            double[] returnArray = new double[length];
            for (int n = 0; n < length; n++)
            {
                returnArray[n] = vMin;
            }

            int tdLength = (int)(td / th);
            int twLength = (int)(tw / th);
            int periodLength = (int)(period / th);
            int deviceDelayLength = (int)(deviceDelay / th);
            for (int n = 0; n < repetetion; n++)
            {
                for (int m = 0; m <= twLength; m++)
                {
                    int pos = periodLength * n + tdLength + m - deviceDelayLength;
                    if(pos >= 0 && pos < returnArray.Length)
                        returnArray[pos] = vMax;
                }
            }

            return returnArray;
        }

        public class NLSIMPulseConfiguration
        {
            public int ONRep { get; set; }  // repetitions
            public double ONTime { get; set; }
            public int DEPRep { get; set; }  // repetitions
            public double DEPTime { get; set; }
            public int ExposureRep { get; set; }  // repetitions
            public double ExposureTime { get; set; }
            public int OFFRep { get; set; }  // repetitions
            public double OFFTime { get; set; }
            public double CameraDelay { get; set; }
            public double DMD1Delay { get; set; }
            public double DMD2Delay { get; set; }
            public double DMD3Delay { get; set; }
            public double CameraReadout { get; set; }

            public bool ON_Camera { get; set; }
            public bool DEP_Camera { get; set; }
            public bool Ex_Camera { get; set; }
            public bool OFF_Camera { get; set; }

            public bool ON_DMD1 { get; set; }
            public bool DEP_DMD1 { get; set; }
            public bool EX_DMD1 { get; set; }
            public int EX_DMD1_PulseNumber { get; set; }
            public bool OFF_DMD1 { get; set; }

            public bool ON_DMD2 { get; set; }
            public bool DEP_DMD2 { get; set; }
            public bool EX_DMD2 { get; set; }
            public bool OFF_DMD2 { get; set; }

            public bool ON_DMD3 { get; set; }
            public bool DEP_DMD3 { get; set; }
            public bool EX_DMD3 { get; set; }
            public bool OFF_DMD3 { get; set; }

            public double SamplingRate { get; set; }
            public double PulseWidth1 { get; set; }
            public double PulseWidth2 { get; set; }
            public double TTLVoltage { get; set; }

            public double InitialOffset
            {
                get
                {
                    return Math.Max(CameraDelay, Math.Max(DMD1Delay, DMD2Delay));
                }
            }

            public double TotalExposure
            {
                get
                {
                    return ONTime*ONRep + DEPTime * DEPRep + ExposureTime * ExposureRep + OFFTime*OFFRep;
                }
            }

            public NLSIMPulseConfiguration()
            {
                TTLVoltage = 3.3;
                PulseWidth1 = 1e-3;
                PulseWidth2 = 0;// 1e-3;//(if the value != 0, line2 become pulse)
                SamplingRate = 10e4;
            }

            public int RepeatNumber { get; set; }
        }

        public static double[,] GenerateNLSIMPulse(NLSIMPulseConfiguration configuration, bool cameraOnce = false)
        {
            double totalTime = 0;
            double oneTime = 0;
            double onTime = 0;
            double depTime = 0;
            double exTime = 0;
            double offTime = 0;
            //OnTime = OnTime * Repetition (カメラも撮像の場合は、(OnTime+Readout) * Repetition）
            onTime += configuration.ONTime * configuration.ONRep;
            if (configuration.ON_Camera)
            {
                onTime += configuration.CameraReadout * configuration.ONRep;
            }

            //DepTimeも同上
            depTime += configuration.DEPTime * configuration.DEPRep;
            if (configuration.DEP_Camera)
            {
                depTime += configuration.CameraReadout * configuration.DEPRep;
            }

            //ExTimeも同上
            exTime += configuration.ExposureTime * configuration.ExposureRep;
            if (configuration.Ex_Camera)
            {
                exTime += configuration.CameraReadout * configuration.ExposureRep;
            }

            //OffTimeも同上
            offTime += configuration.OFFTime * configuration.OFFRep;
            if (configuration.OFF_Camera)
            {
                offTime += configuration.CameraReadout * configuration.OFFRep;
            }
            oneTime = onTime + depTime + exTime + offTime;
            totalTime = configuration.InitialOffset + oneTime * configuration.RepeatNumber;
            int arrayLength = (int)(totalTime * configuration.SamplingRate);

            double[,] returnArray = new double[4, arrayLength];

            int offset = (int)(configuration.InitialOffset * configuration.SamplingRate);
            int oneTimeLength = (int)(oneTime * configuration.SamplingRate);

            if (cameraOnce)
            {
                for(int n = offset;n < offset + (int)(100e-3 * configuration.SamplingRate); n++)
                {
                    returnArray[0, n] = configuration.TTLVoltage;
                }
            }

            for (int n = 0; n < configuration.RepeatNumber; n++)
            {
                int currentOffset = offset + oneTimeLength * n;
                //Camera Trigger
                if(cameraOnce == false)
                {
                    if (configuration.ON_Camera && configuration.ONRep>0)
                    {
                        for (int pulseNumber = 0; pulseNumber < configuration.ONRep; pulseNumber++)
                        {
                            double pulseWidth = (onTime) / (configuration.ONRep); // - configuration.CameraReadout;
                            int startPos = (int)(currentOffset+(- configuration.CameraDelay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                            int endPos = startPos + (int)(configuration.ONTime * configuration.SamplingRate );
                            for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                            {
                            returnArray[0, pos] = configuration.TTLVoltage;
                            }
                        }
                    }
                    if (configuration.DEP_Camera && configuration.DEPRep > 0)
                    {
                        for (int pulseNumber = 0; pulseNumber < configuration.DEPRep; pulseNumber++)
                        {
                            double pulseWidth = (depTime) / (configuration.DEPRep); // - configuration.CameraReadout;
                            int startPos = (int)(currentOffset + (onTime - configuration.CameraDelay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                            int endPos = startPos + (int)(configuration.DEPTime * configuration.SamplingRate);
                            for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                            {
                                returnArray[0, pos] = configuration.TTLVoltage;
                            }
                        }
                    }
                    if (configuration.Ex_Camera && configuration.ExposureRep > 0)
                    {
                        for (int pulseNumber = 0; pulseNumber < configuration.ExposureRep; pulseNumber++)
                        {
                            double pulseWidth = exTime / configuration.ExposureRep; // - configuration.CameraReadout;
                            int startPos = (int)(currentOffset + (onTime + depTime - configuration.CameraDelay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                            int endPos = startPos + (int)(configuration.ExposureTime * configuration.SamplingRate);
                            for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                            {
                                returnArray[0, pos] = configuration.TTLVoltage;
                            }
                        }
                    }
                    if (configuration.OFF_Camera && configuration.OFFRep > 0)
                    {
                        for (int pulseNumber = 0; pulseNumber < configuration.OFFRep; pulseNumber++)
                        {
                            double pulseWidth = (offTime) / (configuration.OFFRep); // - configuration.CameraReadout;
                            int startPos = (int)(currentOffset + (onTime + depTime + exTime - configuration.CameraDelay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                            int endPos = startPos + (int)(configuration.OFFTime * configuration.SamplingRate);
                            for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                            {
                                returnArray[0, pos] = configuration.TTLVoltage;
                            }
                        }
                    }
                }
                int triggerLength1 = (int)(configuration.PulseWidth1 * configuration.SamplingRate);
                int triggerLength2 = (int)(configuration.PulseWidth2 * configuration.SamplingRate);

                //DMD1 Trigger
                if (configuration.ON_DMD1 && configuration.ONRep > 0)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.ONRep; pulseNumber++)
                    {
                        double pulseWidth = (onTime ) / (configuration.ONRep); //  - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (-configuration.DMD1Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength1;
                        if (triggerLength1 == 0)
                        {
                            endPos = startPos + (int)(configuration.ONTime * configuration.SamplingRate);
                        }
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[1, pos] = configuration.TTLVoltage;
                        }
                    }
                }
                if (configuration.DEP_DMD1 && configuration.DEPRep > 0)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.DEPRep; pulseNumber++)
                    {
                        double pulseWidth = (depTime ) / (configuration.DEPRep); //  - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (onTime - configuration.DMD1Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength1;
                        if (triggerLength1 == 0)
                        {
                            endPos = startPos + (int)(configuration.DEPTime * configuration.SamplingRate);
                        }
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[1, pos] = configuration.TTLVoltage;
                        }
                    }
                }
                if (configuration.EX_DMD1 && configuration.ExposureRep > 0)
                {
                    if (configuration.EX_DMD1_PulseNumber* configuration.ExposureRep == 1)
                    {
                        int startPos = (int)(currentOffset + (onTime + depTime - configuration.DMD1Delay) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength1;
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[1, pos] = configuration.TTLVoltage;
                        }
                    }
                    else
                    {
                        for(int pulseNumber = 0; pulseNumber < configuration.EX_DMD1_PulseNumber*configuration.ExposureRep; pulseNumber++)
                        {
                            double pulseWidth = (exTime ) / (configuration.EX_DMD1_PulseNumber*configuration.ExposureRep); //  - configuration.CameraReadout;
                            int startPos = currentOffset + (int)((onTime + depTime - configuration.DMD1Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                            int endPos = startPos + triggerLength1;
                            for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                            {
                                returnArray[1, pos] = configuration.TTLVoltage;
                            }
                        }
                    }
                }
                if (configuration.OFF_DMD1 && configuration.OFFRep > 0)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.OFFRep; pulseNumber++)
                    {
                        double pulseWidth = (offTime ) / (configuration.OFFRep); //  - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (onTime + depTime + exTime - configuration.DMD1Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength1;
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[1, pos] = configuration.TTLVoltage;
                        }
                    }
                }

                //DMD2 Trigger
                if (configuration.ON_DMD2)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.ONRep; pulseNumber++)
                    {
                        double pulseWidth = (onTime ) / (configuration.ONRep); //  - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (-configuration.DMD2Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength2;
                        if(triggerLength2 == 0)
                        {
                            endPos = startPos + (int)(configuration.ONTime * configuration.SamplingRate);
                        }
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[2, pos] = configuration.TTLVoltage;
                        }
                    }
                }
                if (configuration.DEP_DMD2 && configuration.DEPRep > 0)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.DEPRep; pulseNumber++)
                    {
                        double pulseWidth = (depTime) / (configuration.DEPRep); //  - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (onTime - configuration.DMD2Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength2;
                        if(triggerLength2 == 0)
                        {
                            endPos = startPos + (int)(configuration.DEPTime * configuration.SamplingRate);
                        }
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[2, pos] = configuration.TTLVoltage;
                        }
                    }
                }
                if (configuration.EX_DMD2 && configuration.ExposureRep > 0)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.EX_DMD1_PulseNumber * configuration.ExposureRep; pulseNumber++)
                    {
                        double pulseWidth = (exTime) / (configuration.EX_DMD1_PulseNumber * configuration.ExposureRep); //  - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (onTime + depTime - configuration.DMD2Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength2;
                        if(triggerLength2 == 0)
                        {
                            endPos = startPos + (int)(configuration.ExposureTime * configuration.SamplingRate);
                        }
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[2, pos] = configuration.TTLVoltage;
                        }
                    }
                }
                if (configuration.OFF_DMD2 && configuration.OFFRep > 0)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.OFFRep; pulseNumber++)
                    {
                        double pulseWidth = (offTime) / (configuration.OFFRep); // - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (onTime + depTime + exTime - configuration.DMD2Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength2;
                        if(triggerLength2 == 0)
                        {
                            endPos = startPos + (int)(configuration.OFFTime * configuration.SamplingRate);
                        }
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[2, pos] = configuration.TTLVoltage;
                        }
                    }
                }

                //DMD3 Trigger
                if (configuration.ON_DMD3)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.ONRep; pulseNumber++)
                    {
                        double pulseWidth = (onTime) / (configuration.ONRep); //  - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (-configuration.DMD3Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength2;
                        if (triggerLength2 == 0)
                        {
                            endPos = startPos + (int)(configuration.ONTime * configuration.SamplingRate);
                        }
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[3, pos] = configuration.TTLVoltage;
                        }
                    }
                }
                if (configuration.DEP_DMD3 && configuration.DEPRep > 0)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.DEPRep; pulseNumber++)
                    {
                        double pulseWidth = (depTime) / (configuration.DEPRep); //  - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (onTime - configuration.DMD3Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength2;
                        if (triggerLength2 == 0)
                        {
                            endPos = startPos + (int)(configuration.DEPTime * configuration.SamplingRate);
                        }
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[3, pos] = configuration.TTLVoltage;
                        }
                    }
                }
                if (configuration.EX_DMD3 && configuration.ExposureRep > 0)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.EX_DMD1_PulseNumber * configuration.ExposureRep; pulseNumber++)
                    {
                        double pulseWidth = (exTime) / (configuration.EX_DMD1_PulseNumber * configuration.ExposureRep); //  - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (onTime + depTime - configuration.DMD3Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength2;
                        if (triggerLength2 == 0)
                        {
                            endPos = startPos + (int)(configuration.ExposureTime * configuration.SamplingRate);
                        }
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[3, pos] = configuration.TTLVoltage;
                        }
                    }
                }
                if (configuration.OFF_DMD3 && configuration.OFFRep > 0)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.OFFRep; pulseNumber++)
                    {
                        double pulseWidth = (offTime) / (configuration.OFFRep); // - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (onTime + depTime + exTime - configuration.DMD3Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength2;
                        if (triggerLength2 == 0)
                        {
                            endPos = startPos + (int)(configuration.OFFTime * configuration.SamplingRate);
                        }
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[3, pos] = configuration.TTLVoltage;
                        }
                    }
                }
            }

            return returnArray;
        }
    }


    public class WaveGenerator
    {
        public static double[] GenerateLaserPulse(double th, double td, double tw, double period, int repetetion, double vMax, double vMin, double deviceDelay)
        {
            int length = (int)(period * repetetion / th);
            double[] returnArray = new double[length];
            for (int n = 0; n < length; n++)
            {
                returnArray[n] = vMin;
            }

            int tdLength = (int)(td / th);
            int twLength = (int)(tw / th);
            int periodLength = (int)(period / th);
            int deviceDelayLength = (int)(deviceDelay / th);
            for (int n = 0; n < repetetion; n++)
            {
                for (int m = 0; m <= twLength; m++)
                {
                    int pos = periodLength * n + tdLength + m - deviceDelayLength;
                    if (pos >= 0 && pos < returnArray.Length)
                        returnArray[pos] = vMax;
                }
            }

            return returnArray;
        }

        public class NLSIMPulseConfiguration
        {
            public int ONRep { get; set; }  // repetitions
            public double ONTime { get; set; }
            public int DEPRep { get; set; }  // repetitions
            public double DEPTime { get; set; }
            public int ExposureRep { get; set; }  // repetitions
            public double ExposureTime { get; set; }
            public int OFFRep { get; set; }  // repetitions
            public double OFFTime { get; set; }
            public double CameraDelay { get; set; }
            public double DMD1Delay { get; set; }
            public double DMD2Delay { get; set; }
            public double CameraReadout { get; set; }

            public bool ON_Camera { get; set; }
            public bool DEP_Camera { get; set; }
            public bool EX_Camera { get; set; }
            public bool OFF_Camera { get; set; }

            public bool ON_DMD1 { get; set; }
            public bool DEP_DMD1 { get; set; }
            public bool EX_DMD1 { get; set; }
            public int EX_DMD1_PulseNumber { get; set; }
            public bool OFF_DMD1 { get; set; }

            public bool ON_DMD2 { get; set; }
            public bool DEP_DMD2 { get; set; }
            public bool EX_DMD2 { get; set; }
            public bool OFF_DMD2 { get; set; }

            public bool ON_laser { get; set; }
            public bool DEP_laser { get; set; }
            public bool EX_laser { get; set; }
            public bool OFF_laser { get; set; }

            public double SamplingRate { get; set; }
            public double PulseWidth { get; set; }
            public double TTLVoltage { get; set; }

            public double InitialOffset
            {
                get
                {
                    return Math.Max(CameraDelay, Math.Max(DMD1Delay, DMD2Delay));
                }
            }

            public double TotalExposure
            {
                get
                {
                    return ONTime * ONRep + DEPTime * DEPRep + ExposureTime * ExposureRep + OFFTime * OFFRep;
                }
            }

            public NLSIMPulseConfiguration()
            {
                TTLVoltage = 3.3;
                PulseWidth = 1e-3;
                SamplingRate = 10e4;
            }

            public int RepeatNumber { get; set; }
        }

        public static double[,] GenerateNLSIMPulse(NLSIMPulseConfiguration configuration, bool cameraOnce = false)
        {
            double totalTime = 0;
            double oneTime = 0;
            double onTime = 0;
            double depTime = 0;
            double exTime = 0;
            double offTime = 0;
            //OnTime = OnTime * Repetition (カメラも撮像の場合は、(OnTime+Readout) * Repetition）
            onTime += configuration.ONTime * configuration.ONRep;
            if (configuration.ON_Camera)
            {
                onTime += configuration.CameraReadout * configuration.ONRep;
            }

            //DepTimeも同上
            depTime += configuration.DEPTime * configuration.DEPRep;
            if (configuration.DEP_Camera)
            {
                depTime += configuration.CameraReadout * configuration.DEPRep;
            }

            //ExTimeも同上
            exTime += configuration.ExposureTime * configuration.ExposureRep;
            if (configuration.EX_Camera)
            {
                exTime += configuration.CameraReadout * configuration.ExposureRep;
            }

            //OffTimeも同上
            offTime += configuration.OFFTime * configuration.OFFRep;
            if (configuration.OFF_Camera)
            {
                offTime += configuration.CameraReadout * configuration.OFFRep;
            }
            oneTime = onTime + depTime + exTime + offTime;
            totalTime = configuration.InitialOffset + oneTime * configuration.RepeatNumber;
            int arrayLength = (int)(totalTime * configuration.SamplingRate);

            double[,] returnArray = new double[4, arrayLength];

            int offset = (int)(configuration.InitialOffset * configuration.SamplingRate);
            int oneTimeLength = (int)(oneTime * configuration.SamplingRate);

            if (cameraOnce)
            {
                for (int n = offset; n < offset + (int)(100e-3 * configuration.SamplingRate); n++)
                {
                    returnArray[0, n] = configuration.TTLVoltage;
                }
            }

            for (int n = 0; n < configuration.RepeatNumber; n++)
            {
                int currentOffset = offset + oneTimeLength * n;
                //Camera Trigger
                if (cameraOnce == false)
                {
                    if (configuration.ON_Camera && configuration.ONRep > 0)
                    {
                        for (int pulseNumber = 0; pulseNumber < configuration.ONRep; pulseNumber++)
                        {
                            double pulseWidth = (onTime) / (configuration.ONRep); // - configuration.CameraReadout;
                            int startPos = (int)(currentOffset + (-configuration.CameraDelay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                            int endPos = startPos + (int)(configuration.ONTime * configuration.SamplingRate);
                            for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                            {
                                returnArray[0, pos] = configuration.TTLVoltage;
                            }
                        }
                    }
                    if (configuration.DEP_Camera && configuration.DEPRep > 0)
                    {
                        for (int pulseNumber = 0; pulseNumber < configuration.DEPRep; pulseNumber++)
                        {
                            double pulseWidth = (depTime) / (configuration.DEPRep); // - configuration.CameraReadout;
                            int startPos = (int)(currentOffset + (onTime - configuration.CameraDelay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                            int endPos = startPos + (int)(configuration.DEPTime * configuration.SamplingRate);
                            for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                            {
                                returnArray[0, pos] = configuration.TTLVoltage;
                            }
                        }
                    }
                    if (configuration.EX_Camera && configuration.ExposureRep > 0)
                    {
                        for (int pulseNumber = 0; pulseNumber < configuration.ExposureRep; pulseNumber++)
                        {
                            double pulseWidth = exTime / configuration.ExposureRep; // - configuration.CameraReadout;
                            int startPos = (int)(currentOffset + (onTime + depTime - configuration.CameraDelay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                            int endPos = startPos + (int)(configuration.ExposureTime * configuration.SamplingRate);
                            for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                            {
                                returnArray[0, pos] = configuration.TTLVoltage;
                            }
                        }
                    }
                    if (configuration.OFF_Camera && configuration.OFFRep > 0)
                    {
                        for (int pulseNumber = 0; pulseNumber < configuration.OFFRep; pulseNumber++)
                        {
                            double pulseWidth = (offTime) / (configuration.OFFRep); // - configuration.CameraReadout;
                            int startPos = (int)(currentOffset + (onTime + depTime + exTime - configuration.CameraDelay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                            int endPos = startPos + (int)(configuration.OFFTime * configuration.SamplingRate);
                            for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                            {
                                returnArray[0, pos] = configuration.TTLVoltage;
                            }
                        }
                    }
                }
                int triggerLength = (int)(configuration.PulseWidth * configuration.SamplingRate);
                //DMD1 Trigger
                if (configuration.ON_DMD1 && configuration.ONRep > 0)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.ONRep; pulseNumber++)
                    {
                        double pulseWidth = (onTime) / (configuration.ONRep); //  - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (-configuration.DMD1Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength;
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[1, pos] = configuration.TTLVoltage;
                        }
                    }
                }
                if (configuration.DEP_DMD1 && configuration.DEPRep > 0)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.DEPRep; pulseNumber++)
                    {
                        double pulseWidth = (depTime) / (configuration.DEPRep); //  - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (onTime - configuration.DMD1Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength;
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[1, pos] = configuration.TTLVoltage;
                        }
                    }
                }
                if (configuration.EX_DMD1 && configuration.ExposureRep > 0)
                {
                    if (configuration.EX_DMD1_PulseNumber * configuration.ExposureRep == 1)
                    {
                        int startPos = (int)(currentOffset + (onTime + depTime - configuration.DMD1Delay) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength;
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[1, pos] = configuration.TTLVoltage;
                        }
                    }
                    else
                    {
                        for (int pulseNumber = 0; pulseNumber < configuration.EX_DMD1_PulseNumber * configuration.ExposureRep; pulseNumber++)
                        {
                            double pulseWidth = (exTime) / (configuration.EX_DMD1_PulseNumber * configuration.ExposureRep); //  - configuration.CameraReadout;
                            int startPos = currentOffset + (int)((onTime + depTime - configuration.DMD1Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                            int endPos = startPos + triggerLength;
                            for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                            {
                                returnArray[1, pos] = configuration.TTLVoltage;
                            }
                        }
                    }
                }
                if (configuration.OFF_DMD1 && configuration.OFFRep > 0)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.OFFRep; pulseNumber++)
                    {
                        double pulseWidth = (offTime) / (configuration.OFFRep); //  - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (onTime + depTime + exTime - configuration.DMD1Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength;
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[1, pos] = configuration.TTLVoltage;
                        }
                    }
                }

                //DMD2 Trigger
                if (configuration.ON_DMD2)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.ONRep; pulseNumber++)
                    {
                        double pulseWidth = (onTime) / (configuration.ONRep); //  - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (-configuration.DMD2Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength;
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[2, pos] = configuration.TTLVoltage;
                        }
                    }
                }
                if (configuration.DEP_DMD2 && configuration.DEPRep > 0)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.DEPRep; pulseNumber++)
                    {
                        double pulseWidth = (depTime) / (configuration.DEPRep); //  - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (onTime - configuration.DMD2Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength;
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[2, pos] = configuration.TTLVoltage;
                        }
                    }
                }
                if (configuration.EX_DMD2 && configuration.ExposureRep > 0)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.EX_DMD1_PulseNumber * configuration.ExposureRep; pulseNumber++)
                    {
                        double pulseWidth = (exTime) / (configuration.EX_DMD1_PulseNumber * configuration.ExposureRep); //  - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (onTime + depTime - configuration.DMD2Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength;
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[2, pos] = configuration.TTLVoltage;
                        }
                    }
                }
                if (configuration.OFF_DMD2 && configuration.OFFRep > 0)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.OFFRep; pulseNumber++)
                    {
                        double pulseWidth = (offTime) / (configuration.OFFRep); // - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (onTime + depTime + exTime - configuration.DMD2Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + triggerLength;
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[2, pos] = configuration.TTLVoltage;
                        }
                    }
                }
                //405nm laser Trigger
                if (configuration.ON_laser && configuration.ONRep > 0)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.ONRep; pulseNumber++)
                    {
                        double pulseWidth = (onTime) / (configuration.ONRep); // - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (-configuration.DMD1Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + (int)(configuration.ONTime * configuration.SamplingRate);
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[3, pos] = configuration.TTLVoltage;
                        }
                    }
                }
                if (configuration.DEP_laser && configuration.DEPRep > 0)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.DEPRep; pulseNumber++)
                    {
                        double pulseWidth = (depTime) / (configuration.DEPRep); // - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (onTime - configuration.DMD1Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + (int)(configuration.DEPTime * configuration.SamplingRate);
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[3, pos] = configuration.TTLVoltage;
                        }
                    }
                }
                if (configuration.EX_laser && configuration.ExposureRep > 0)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.ExposureRep; pulseNumber++)
                    {
                        double pulseWidth = exTime / configuration.ExposureRep; // - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (onTime + depTime - configuration.DMD1Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + (int)(configuration.ExposureTime * configuration.SamplingRate);
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[3, pos] = configuration.TTLVoltage;
                        }
                    }
                }
                if (configuration.OFF_laser && configuration.OFFRep > 0)
                {
                    for (int pulseNumber = 0; pulseNumber < configuration.OFFRep; pulseNumber++)
                    {
                        double pulseWidth = (offTime) / (configuration.OFFRep); // - configuration.CameraReadout;
                        int startPos = (int)(currentOffset + (onTime + depTime + exTime - configuration.DMD1Delay + pulseNumber * pulseWidth) * configuration.SamplingRate);
                        int endPos = startPos + (int)(configuration.OFFTime * configuration.SamplingRate);
                        for (int pos = Math.Max(0, startPos); pos <= Math.Min(endPos, arrayLength - 1); pos++)
                        {
                            returnArray[3, pos] = configuration.TTLVoltage;
                        }
                    }
                }
                triggerLength = (int)(configuration.PulseWidth * configuration.SamplingRate);
            }

            return returnArray;
        }
    }
}
