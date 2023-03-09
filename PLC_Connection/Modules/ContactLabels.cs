using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PLC_Connection.Modules
{
    public static class ContactLabels
    {
        public static String supplyStationState = "supplyStation_State";
        public static String visualStationState = "visualInspection_Status";
        public static String functionStationState = "functionStation_State";
        public static String functionStationSensor = "functionStation_Sensor";
        public static String assemblyStationState = "assemblyStation_State";
        public static String visualStationSensor = "visualStation_Sensor";
        public static String systemOperation = "systemOperation";

        public static String visualInspectionState = "visualInspection_Status";
        public static String visualInspectionResult = "visualInspection_Result";
        public static String functionInspectionVoltage = "functionInspection_Voltage";
        public static String functionInspectionFrequency = "functionInspection_Frequency";
    }
}