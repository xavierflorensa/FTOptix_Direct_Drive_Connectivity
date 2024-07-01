using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Serialization;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.IO;
using System.Globalization;
using FTOptix.Core;
using System.Data;
using System.Text.RegularExpressions;
using OpcUa = UAManagedCore.OpcUa;
using CGP_CS.CipDataTypesFactories;
using CGP_CS.Enums;
using Newtonsoft.Json;
using CGP_CS.Directives;
using System.Text;
using FTOptix.HMIProject;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UAManagedCore;
using System.ComponentModel;
using System.Collections.Concurrent;
using System.Reflection;
using CGP_CS.CipClasses;
using FTOptix.NetLogic;
using System;
using CGP_CS.CipDataTypes;
using DirectDriveConnectivity;

namespace DDC
{
    internal class Constants
    {
        internal const string CGPFolderName = "CGP";
        internal const string CGPExecutableNameWindows = "cgp.exe";
        internal const string NetSolutionFolderName = "NetSolution";
        internal const string PowerFlexModelsFolder = "pfModels";
        internal const string TypesFolderName = "Types";
        internal const string DeviceFolderName = "DeviceFolder";
    }
}

namespace DirectDriveConnectivity
{
    class FTOptixLog
    {
        public const string DDCCategory = "DirectDriveConnectivity";
    }
}

public class DriveConnectionDesignTimeNetLogic : BaseNetLogic
{
    [ExportMethod]
    public void GetPowerFlexModel()
    {
        try
        {
            var modelUtils = new ModelUtils(LogicObject);
            if (modelUtils.DoesDataPointFolderExists())
            {
                Log.Error(FTOptixLog.DDCCategory, "Data points folder already exists.");
                return;
            }

            var pathValidator = new TargetDevicePathValidator(modelUtils.GetIPAddressString());
            var pathValidatorResult = pathValidator.Validate();
            if (!pathValidatorResult.Test)
            {
                Log.Error(FTOptixLog.DDCCategory, pathValidatorResult.Message);
                return;
            }

            var dataPointsJsonReader = new DataPointsJsonReader(GetDataPointsFilesPath(modelUtils), modelUtils.GetIPAddressString());
            var dataPoints = dataPointsJsonReader.Read();

            var dataPointsModelCreator = new DataPointsModelCreator();
            dataPointsModelCreator.Create(dataPoints, modelUtils.CreateDataPointModelFolders());
        }
        catch (Exception ex)
        {
            Log.Error(FTOptixLog.DDCCategory, string.Format("Error during model creation: {0}.", ex.Message));
        }
    }

    private static string GetDataPointsFilesPath(ModelUtils modelUtils)
    {
        return Path.Combine(Project.Current.ProjectDirectory, DDC.Constants.NetSolutionFolderName, DDC.Constants.PowerFlexModelsFolder,
            modelUtils.GetPowerFlexFileName());
    }
}

namespace CGP_CS.CipClasses
{
    public class Datapoint
    {
        //---Deserialized properties
        public string Name { get; set; }
        public string Category { get; set; }

        public ExtractionDirective ExtractionDirective { get; set; }

        //---Additional properties
        public CipDataType DataTypeEnumVal { get; set; }

        internal DataType DataType { get; set; }

        public dynamic EngineeringUnitMultiplier { get; set; } = 1;

        internal dynamic Value { get; set; }

        private bool MultiplierIsInt()
        {
            bool isInt = Int32.TryParse(EngineeringUnitMultiplier.ToString(), out int number);
            return isInt;
        }

        private void SetDataType()
        {
            DataTypeEnumVal = GetCipDataTypeFromProfile();
            try
            {
                switch (DataTypeEnumVal)
                {
                    case CipDataType.UNSPECIFIED:
                        break;
                    case CipDataType.BOOL:
                        DataType = new BOOLFactory().GetSourceDataType();
                        break;
                    case CipDataType.SINT:
                        DataType = new SINTFactory().GetSourceDataType();
                        DataType.MultiplierIsInt = MultiplierIsInt();
                        break;
                    case CipDataType.INT:
                        DataType = new INTFactory().GetSourceDataType();
                        DataType.MultiplierIsInt = MultiplierIsInt();
                        break;
                    case CipDataType.LE_INT:
                        DataType = new LE_INTFactory().GetSourceDataType();
                        break;
                    case CipDataType.BE_INT:
                        DataType = new BE_INTFactory().GetSourceDataType();
                        DataType.MultiplierIsInt = MultiplierIsInt();
                        break;
                    case CipDataType.DINT:
                        DataType = new DINTFactory().GetSourceDataType();
                        DataType.MultiplierIsInt = MultiplierIsInt();
                        break;
                    case CipDataType.LE_DINT:
                        DataType = new LE_DINTFactory().GetSourceDataType();
                        break;
                    case CipDataType.BE_DINT:
                        DataType = new BE_DINTFactory().GetSourceDataType();
                        break;
                    case CipDataType.LINT:
                        DataType = new LINTFactory().GetSourceDataType();
                        break;
                    case CipDataType.LE_LINT:
                        DataType = new LE_LINTFactory().GetSourceDataType();
                        break;
                    case CipDataType.BE_LINT:
                        DataType = new BE_LINTFactory().GetSourceDataType();
                        break;
                    case CipDataType.USINT:
                        DataType = new USINTFactory().GetSourceDataType();
                        DataType.MultiplierIsInt = MultiplierIsInt();
                        break;
                    case CipDataType.UINT:
                        DataType = new UINTFactory().GetSourceDataType();
                        DataType.MultiplierIsInt = MultiplierIsInt();
                        break;
                    case CipDataType.LE_UINT:
                        DataType = new LE_UINTFactory().GetSourceDataType();
                        break;
                    case CipDataType.BE_UINT:
                        DataType = new BE_UINTFactory().GetSourceDataType();
                        DataType.MultiplierIsInt = MultiplierIsInt();
                        break;
                    case CipDataType.UDINT:
                        DataType = new UDINTFactory().GetSourceDataType();
                        break;
                    case CipDataType.LE_UDINT:
                        DataType = new LE_UDINTFactory().GetSourceDataType();
                        break;
                    case CipDataType.BE_UDINT:
                        DataType = new BE_UDINTFactory().GetSourceDataType();
                        break;
                    case CipDataType.ULINT:
                        DataType = new ULINTFactory().GetSourceDataType();
                        break;
                    case CipDataType.LE_ULINT:
                        DataType = new LE_ULINTFactory().GetSourceDataType();
                        break;
                    case CipDataType.BE_ULINT:
                        DataType = new BE_ULINTFactory().GetSourceDataType();
                        break;
                    case CipDataType.VL_UINT:
                        DataType = new VL_UINTFactory().GetSourceDataType();
                        break;
                    case CipDataType.VL_REAL:
                        DataType = new VL_REALFactory().GetSourceDataType();
                        break;
                    case CipDataType.REAL:
                        DataType = new REALFactory().GetSourceDataType();
                        break;
                    case CipDataType.LREAL:
                        DataType = new LREALFactory().GetSourceDataType();
                        break;
                    case CipDataType.BYTE:
                        DataType = new BYTEFactory().GetSourceDataType();
                        break;
                    case CipDataType.WORD:
                        DataType = new WORDFactory().GetSourceDataType();
                        break;
                    case CipDataType.DWORD:
                        DataType = new DWORDFactory().GetSourceDataType();
                        break;
                    case CipDataType.LWORD:
                        DataType = new LWORDFactory().GetSourceDataType();
                        break;
                    case CipDataType.STRING:
                        DataType = new STRINGFactory().GetSourceDataType();
                        break;
                    case CipDataType.STRING2:
                        DataType = new STRING2Factory().GetSourceDataType();
                        break;
                    case CipDataType.SHORT_STRING:
                        DataType = new SHORT_STRINGFactory().GetSourceDataType();
                        break;
                    case CipDataType.FLEX_STRING:
                        DataType = new FLEX_STRINGFactory().GetSourceDataType();
                        break;
                    case CipDataType.STRING16:
                        DataType = new STRING16Factory().GetSourceDataType();
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                string error = string.Format(CultureInfo.InvariantCulture,
                    "Specified CIP data type ({0}) is invalid for dataPoint {1}: {2}", DataTypeEnumVal.ToString(), Name, ex.Message);

                throw new ArgumentException(error);
            }
        }

        internal void Configure()
        {
            SetDataType();
            UpdateExtractionDirective();
        }

        private void UpdateExtractionDirective()
        {
            int additionalByteOffset = (int)Math.Truncate((decimal)(ExtractionDirective.BitOffset / 8));

            ExtractionDirective.ByteOffset += additionalByteOffset;
            ExtractionDirective.BitOffset -= additionalByteOffset * 8;
        }

        private CipDataType GetCipDataTypeFromProfile()
        {
            var cipDataType = this.ExtractionDirective.CipDataType;
            CipDataType dataType = EnumUtil.ParseEnum<CipDataType>(cipDataType.ToString());

            return dataType;
        }

        internal dynamic GetValueFromSourceData(byte[] sourceData)
        {
            dynamic dataPointValue = null;
            // byte length validation
            long byteOffset = ExtractionDirective.ByteOffset;
            long maxOffset = sourceData.Length - 1;

            if (byteOffset > maxOffset)
            {
                string sourceString = BitConverter.ToString(sourceData);
                if (DataType is STRING)
                {
                    return "N/A"; //This will result in N/A in the valueboxes
                }
                return null; //This will result in N/A in the gauges
            }

            try
            {
                dataPointValue = DataType.ExtractValue(sourceData, ExtractionDirective);
            }
            catch (Exception ex)
            {
                var message = $"Datapoint '{Name}' error extracting value: {ex.Message}";
                Globals.Logger.LogError(message);
            }

            return dataPointValue;
        }
    }
}
namespace CGP_CS.CipClasses
{
    public class DataSource
    {
        public string Name { get; set; }
        public AcquisitionDirective AcquisitionDirective { get; set; }
        public Datapoint[] DataPoints { get; set; }
    }

    public class AcquisitionDirective
    {
        public string Protocol { get; set; }
        public int DpiPort { get; set; }
        public int DpiParameter { get; set; }
        public bool DpiIsVirtual { get; set; }

        public string CipMode { get; set; }
        public int CipService { get; set; }
        public Cippath CipPath { get; set; }
    }

    public class Cippath
    {
        public Segment[] Segments { get; set; }
    }

    public class Segment
    {
        public int ClassId { get; set; }
        public int InstanceId { get; set; }
        public int AttributeId { get; set; }
    }

}

namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Signed 32–bit integer (Big Endian)
    /// </summary>
    class BE_DINT : DINT
    {
        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            dynamic dataPointValue = BitConverter.ToInt32(sourceData.Skip(extractionDirective.ByteOffset).Take(4).Reverse().ToArray(), 0);
            return dataPointValue;
        }
    }
}

namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Signed 16–bit integer (Big Endian)
    /// </summary>
    class BE_INT : INT
    {
        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            dynamic dataPointValue = BitConverter.ToInt16(sourceData.Skip(extractionDirective.ByteOffset).Take(2).Reverse().ToArray(), 0);
            return dataPointValue;
        }
    }
}

namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Signed 64–bit integer (Big Endian)
    /// </summary>
    class BE_LINT : LINT
    {
        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            dynamic dataPointValue = BitConverter.ToInt64(sourceData.Skip(extractionDirective.ByteOffset).Take(8).Reverse().ToArray(), 0);
            return dataPointValue;
        }
    }
}

namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Unsigned 32–bit integer (Big Endian)
    /// </summary>
    class BE_UDINT : UDINT
    {
        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            dynamic dataPointValue = BitConverter.ToUInt32(sourceData.Skip(extractionDirective.ByteOffset).Take(4).Reverse().ToArray(), 0);
            return dataPointValue;
        }
    }
}

namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Unsigned 16–bit integer (Big Endian)
    /// </summary>
    class BE_UINT : UINT
    {
        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            dynamic dataPointValue = BitConverter.ToUInt16(sourceData.Skip(extractionDirective.ByteOffset).Take(2).Reverse().ToArray(), 0);
            return dataPointValue;
        }
    }
}

namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Unsigned 64–bit integer (Big Endian)
    /// </summary>
    class BE_ULINT : ULINT
    {
        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            dynamic dataPointValue = BitConverter.ToUInt64(sourceData.Skip(extractionDirective.ByteOffset).Take(8).Reverse().ToArray(), 0);
            return dataPointValue;
        }
    }
}


namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Boolean
    /// </summary>
    internal class BOOL : DataType
    {
        internal override dynamic MinValue { get => false; }
        internal override dynamic MaxValue { get => true; }

        internal override uint Length => 1;

        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            BitArray bits = new BitArray(sourceData.Skip(extractionDirective.ByteOffset).Take(1).ToArray());
            dynamic dataPointValue = bits[(int)extractionDirective.BitOffset];
            return dataPointValue;
        }
    }
}


namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Single byte bit field
    /// </summary>
    class BYTE : DataType
    {
        internal override dynamic MinValue { get => byte.MinValue; }
        internal override dynamic MaxValue { get => byte.MaxValue; }
        internal override uint Length => 8;

        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            dynamic dataPointValue = sourceData[extractionDirective.ByteOffset];
            return dataPointValue;
        }
    }
}


namespace CGP_CS.CipDataTypes
{
    public abstract class DataType
    {
        internal abstract dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective);
        internal abstract dynamic MinValue { get; }
        internal abstract dynamic MaxValue { get; }
        internal abstract uint Length { get; }
        internal bool MultiplierIsInt { get; set; } = true;

        internal static string FinalizeStringResult(string result)
        {
            result = result.Trim();
            if (result.Length == 0)
            {
                result = NotAvailable;
            }
            else
            {
                result = IsStringOfQuestionMarks(result) ? NotAvailable : result;

            }
            return result;
        }

        private static bool IsStringOfQuestionMarks(string val)
        {
            for (int i = 0; i < val.Length; i++)
            {
                if (val[i] != '?')
                    return false;
            }
            return true;
        }

        private const string NotAvailable = "N/A";
    }
}


namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Signed 32–bit integer
    /// </summary>
    class DINT : DataType
    {
        internal override dynamic MinValue { get => int.MinValue; } //Int32
        internal override dynamic MaxValue { get => int.MaxValue; }
        internal override uint Length => 32;

        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            dynamic dataPointValue = BitConverter.ToInt32(sourceData, extractionDirective.ByteOffset);
            return dataPointValue;
        }
    }
}
namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Quad byte bit field
    /// </summary>
    class DWORD : UDINT
    {
    }
}

namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// String with optionally specified length
    /// </summary>
    class FLEX_STRING : STRING
    {
        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            dynamic dataPointValue;
            //Byte length of the string is defined in the profile, 1 byte per character
            if (extractionDirective.ByteLength > 0)
            {
                var result = Encoding.UTF8.GetString(sourceData, index: extractionDirective.ByteOffset,
                    count: extractionDirective.ByteLength);
                dataPointValue = FinalizeStringResult(result);
            }
            else
            {
                //Resize the byte array so it no longer contains extraneous characters used to fill the array
                int lastIndex = Array.FindLastIndex(sourceData, b => b != 0);
                Array.Resize(ref sourceData, lastIndex + 1);

                dataPointValue = FinalizeStringResult(Encoding.UTF8.GetString(sourceData));
            }
            return dataPointValue;
        }
    }
}


namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Signed 16–bit integer
    /// </summary>
    internal class INT : DataType
    {
        internal override dynamic MinValue { get => short.MinValue; } //Int16
        internal override dynamic MaxValue { get => short.MaxValue; }
        internal override uint Length => 16;

        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            dynamic dataPointValue = BitConverter.ToInt16(sourceData, extractionDirective.ByteOffset);
            return dataPointValue;
        }
    }
}
namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Signed 32–bit integer (Little Endian)
    /// </summary>
    class LE_DINT : DINT
    {
    }
}
namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Signed 16–bit integer (Little Endian)
    /// </summary>
    class LE_INT : INT
    {
    }
}
namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Signed 64–bit integer (Little Endian)
    /// </summary>
    class LE_LINT : LINT
    {
    }
}
namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Unsigned 32–bit integer (Little Endian)
    /// </summary>
    class LE_UDINT : UDINT
    {
    }
}
namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Unsigned 16–bit integer (Little Endian)
    /// </summary>
    class LE_UINT : UINT
    {
    }
}
namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Unsigned 64–bit integer (Little Endian)
    /// </summary>
    class LE_ULINT : ULINT
    {
    }
}


namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Signed 64–bit integer
    /// </summary>
    class LINT : DataType
    {
        internal override dynamic MinValue { get => long.MinValue; } //Int64
        internal override dynamic MaxValue { get => long.MaxValue; }
        internal override uint Length => 64;

        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            dynamic dataPointValue = BitConverter.ToInt64(sourceData, extractionDirective.ByteOffset);
            return dataPointValue;
        }
    }
}


namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Double precision 64-bit floating point (IEEE 754)
    /// </summary>
    class LREAL : DataType
    {
        internal override dynamic MinValue { get => double.MinValue; }
        internal override dynamic MaxValue { get => double.MaxValue; }
        internal override uint Length => 64;

        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            //64 bit floating point
            dynamic dataPointValue = BitConverter.ToDouble(sourceData, extractionDirective.ByteOffset);
            return dataPointValue;
        }
    }
}
namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Octa byte bit field
    /// </summary>
    class LWORD : ULINT
    {
    }
}


namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Single precision 32-bit floating point (IEEE 754)
    /// </summary>
    class REAL : DataType
    {
        internal override dynamic MinValue { get => float.MinValue; } //Single
        internal override dynamic MaxValue { get => float.MaxValue; }
        internal override uint Length => 32;

        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            //32 bit floating point
            dynamic dataPointValue = BitConverter.ToSingle(sourceData, extractionDirective.ByteOffset);
            return dataPointValue;
        }
    }
}


namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Length prefixed single byte character string
    /// </summary>
    class SHORT_STRING : STRING
    {
        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            //1 byte length indicator, 1 byte characters
            int byteOffset = extractionDirective.ByteOffset;
            int stringByteLength = sourceData[byteOffset];
            int availableByteLength = sourceData.Length - byteOffset;
            dynamic dataPointValue = string.Empty;
            if (stringByteLength < availableByteLength)
            {
                dataPointValue = FinalizeStringResult(Encoding.UTF8.GetString(sourceData, index: byteOffset + 1, count: stringByteLength));
            }
            return dataPointValue;
        }
    }
}


namespace CGP_CS.CipDataTypes
{
    //Signed 8–bit integer
    class SINT : DataType
    {
        internal override dynamic MinValue { get => char.MinValue; }
        internal override dynamic MaxValue { get => char.MaxValue; }
        internal override uint Length => 8;

        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            //Signed 8–bit integer
            dynamic dataPointValue = (sbyte)sourceData[extractionDirective.ByteOffset];
            return dataPointValue;
        }
    }
}


namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Single byte character string
    /// </summary>
    class STRING : DataType
    {
        //minVal/maxVal is not applicable to strings
        internal override dynamic MinValue { get => throw new NotImplementedException(); }
        internal override dynamic MaxValue { get => throw new NotImplementedException(); }

        //Variable
        internal override uint Length => 0;

        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            // 2 byte length indicator, 1 byte per character
            int bytesContainingLength = 2;
            int byteOffset = extractionDirective.ByteOffset;
            int stringByteLength = BitConverter.ToInt16(sourceData, byteOffset);
            dynamic dataPointValue = FinalizeStringResult(Encoding.UTF8.GetString(sourceData, index: byteOffset + bytesContainingLength,
                count: stringByteLength));
            return dataPointValue;
        }
    }
}

namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// STRING16
    /// </summary>
    class STRING16 : STRING
    {
        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            const int StringLength = 16;
            int byteOffset = extractionDirective.ByteOffset;
            dynamic dataPointValue = FinalizeStringResult(Encoding.UTF8.GetString(sourceData, index: byteOffset, count: StringLength));
            return dataPointValue;
        }
    }
}


namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Double byte character string
    /// </summary>
    class STRING2 : STRING
    {
        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            //2 byte length indicator, 2 bytes per character
            int byteOffset = extractionDirective.ByteOffset;
            int stringByteLength = 2 * BitConverter.ToInt16(sourceData, startIndex: byteOffset);
            dynamic dataPointValue = FinalizeStringResult(Encoding.Unicode.GetString(sourceData, index: byteOffset + 2, count: stringByteLength));
            return dataPointValue;
        }
    }
}


namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Unsigned 32–bit integer
    /// </summary>
    class UDINT : DataType
    {
        internal override dynamic MinValue { get => uint.MinValue; } //UInt32
        internal override dynamic MaxValue { get => uint.MaxValue; }
        internal override uint Length => 32;

        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            dynamic dataPointValue = BitConverter.ToUInt32(sourceData, extractionDirective.ByteOffset);
            return dataPointValue;
        }
    }
}

namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Unsigned 16–bit integer
    /// </summary>
    class UINT : DataType
    {
        internal override dynamic MinValue { get => ushort.MinValue; } //UInt16
        internal override dynamic MaxValue { get => ushort.MaxValue; }
        internal override uint Length => 16;

        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            dynamic dataPointValue = BitConverter.ToUInt16(sourceData, extractionDirective.ByteOffset);
            return dataPointValue;
        }
    }
}


namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Unsigned 64–bit integer
    /// </summary>
    class ULINT : DataType
    {
        internal override dynamic MinValue { get => ulong.MinValue; } //UInt64
        internal override dynamic MaxValue { get => ulong.MaxValue; }
        internal override uint Length => 64;

        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            dynamic dataPointValue = BitConverter.ToUInt64(sourceData, extractionDirective.ByteOffset);
            return dataPointValue;
        }
    }
}
namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Unsigned 8–bit integer
    /// </summary>
    class USINT : BYTE
    {
    }
}

namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Variable length multi bit signed int type
    /// </summary>
    class VL_REAL : REAL
    {
        //Variable
        internal override uint Length => 0;

        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            //32 bit floating point
            //This caters for datasources that are expected to be real
            //but that don't contain four bytes
            //(observed for PF755T motor side 'n'.Heatsink Fan Remaining Life)
            while (sourceData.Length < 4)
            {
                sourceData = AddByteToArray(sourceData);
            }
            dynamic dataPointValue = BitConverter.ToSingle(sourceData, extractionDirective.ByteOffset);
            return dataPointValue;
        }

        private byte[] AddByteToArray(byte[] existingByteArray)
        {
            //create an array one byte longer than the existing array
            byte[] newArray = new byte[existingByteArray.Length + 1];
            //leave the first byte of the array available for the new byte
            existingByteArray.CopyTo(newArray, 1);
            //set the first byte to 0
            newArray[0] = 0;
            return newArray;
        }
    }
}

namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Variable length multi bit unsigned int type
    /// </summary>
    class VL_UINT : UINT
    {
        //Variable
        internal override uint Length => 0;

        internal override dynamic ExtractValue(byte[] sourceData, ExtractionDirective extractionDirective)
        {
            //obtain the byte
            BitArray bitsContainingUint = new BitArray(sourceData);
            //get the first n bits
            uint temporaryValue = 0;
            uint bitMask = 1;
            int startbit = (extractionDirective.ByteOffset * 8 + extractionDirective.BitOffset);

            for (int bit = startbit; bit < startbit + extractionDirective.BitLength; bit++)
            {
                if (bitsContainingUint.Get(bit))
                {
                    temporaryValue = temporaryValue | bitMask;
                }
                bitMask <<= 1;
            }
            dynamic dataPointValue = temporaryValue;
            return dataPointValue;
        }
    }
}
namespace CGP_CS.CipDataTypes
{
    /// <summary>
    /// Double byte bit field
    /// </summary>
    class WORD : UINT
    {
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class BE_DINTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new BE_DINT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class BE_INTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new BE_INT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class BE_LINTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new BE_LINT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class BE_UDINTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new BE_UDINT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class BE_UINTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new BE_UINT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class BE_ULINTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new BE_ULINT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class BOOLFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new BOOL();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class BYTEFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new BYTE();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    abstract class DataTypeFactory
    {
        internal abstract DataType GetSourceDataType();
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class DINTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new DINT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class DWORDFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new DWORD();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class FLEX_STRINGFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new FLEX_STRING();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class INTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new INT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class LE_DINTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new LE_DINT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class LE_INTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new LE_INT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class LE_LINTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new LE_LINT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class LE_UDINTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new LE_UDINT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class LE_UINTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new LE_UINT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class LE_ULINTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new LE_ULINT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class LINTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new LINT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class LREALFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new LREAL();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class LWORDFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new LWORD();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class REALFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new REAL();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class SHORT_STRINGFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new SHORT_STRING();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class SINTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new SINT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class STRING16Factory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new STRING16();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class STRING2Factory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new STRING2();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class STRINGFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new STRING();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class UDINTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new UDINT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class UINTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new UINT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class ULINTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new ULINT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class USINTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new USINT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class VL_REALFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new VL_REAL();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class VL_UINTFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new VL_UINT();
        }
    }
}

namespace CGP_CS.CipDataTypesFactories
{
    class WORDFactory : DataTypeFactory
    {
        internal override DataType GetSourceDataType()
        {
            return new WORD();
        }
    }
}
namespace CGP_CS.Directives
{
    public class ExtractionDirective
    {
        public string CipDataType { get; set; }
        public int ByteOffset { get; set; }
        public int BitOffset { get; set; }
        public int BitLength { get; set; }
        public int ByteLength { get; set; }
    }
}

namespace CGP_CS.Enums
{
    /// <summary>
    /// Identifies the CIP data type
    /// Reference: The CIP Networks Library, Volume 1, Common Industrial Protocol
    /// </summary>
    [DataContract]
    public enum CipDataType
    {
        // Note: Use DescriptionAttr extension to obtain Description string

        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Unspecified")]
        UNSPECIFIED = 0,
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Boolean")]
        BOOL,
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Signed 8–bit integer")]
        SINT,
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Signed 16–bit integer")]
        INT,
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Signed 16–bit integer (Little Endian)")]
        LE_INT,
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Signed 16–bit integer (Big Endian)")]
        BE_INT,
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Signed 32–bit integer")]
        DINT,
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Signed 32–bit integer (Little Endian)")]
        LE_DINT,
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Signed 32–bit integer (Big Endian)")]
        BE_DINT,
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Signed 64–bit integer")]
        LINT,
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Signed 64–bit integer (Little Endian)")]
        LE_LINT,
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Signed 64–bit integer (Big Endian)")]
        BE_LINT,
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Unsigned 8–bit integer")]
        USINT,
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Unsigned 16–bit integer")]
        UINT,
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Unsigned 8–bit integer converted to HART EU string")]
        USINT_LOOKUP_HART_EU,
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Unsigned 8–bit integer converted to HART Connected string")]
        USINT_LOOKUP_HART_CONNECTED,
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Unsigned 8–bit integer converted to HART Manufacturer ID")]
        USINT_LOOKUP_MANUFACTURER_ID,
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Unsigned 16–bit integer (Little Endian)")]
        LE_UINT,
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Unsigned 16–bit integer (Big Endian)")]
        BE_UINT,
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Unsigned 32–bit integer")]
        UDINT,
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Unsigned 32–bit integer (Little Endian)")]
        LE_UDINT,
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Unsigned 32–bit integer (Big Endian)")]
        BE_UDINT,
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Unsigned 64–bit integer")]
        ULINT,
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Unsigned 64–bit integer (Little Endian)")]
        LE_ULINT,
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Unsigned 64–bit integer (Big Endian)")]
        BE_ULINT,
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Variable length multi bit unsigned int type")]
        VL_UINT,
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Variable length multi bit signed int type")]
        VL_REAL,
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Single precision 32-bit floating point (IEEE 754)")]
        REAL,
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Double precision 64-bit floating point (IEEE 754)")]
        LREAL,
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Single byte bit field")]
        BYTE,
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Double byte bit field")]
        WORD,
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Quad byte bit field")]
        DWORD,
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Octa byte bit field")]
        LWORD,
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Single byte character string")]
        STRING,
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Double byte character string")]
        STRING2,
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("Length prefixed single byte character string")]
        SHORT_STRING,
        [SuppressMessage("Microsoft.Naming", "CA1707:IdentifiersShouldNotContainUnderscores")]
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("String with optionally specified length")]
        FLEX_STRING,
        [SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly")]
        [EnumMember, Description("STRING16")]
        STRING16
    }

}


namespace DirectDriveConnectivity
{
    public enum LogType
    {
        Info,
        Warning,
        Error
    }
    public interface ILog
    {
        void Info(string message);
        void Warning(string message);
        void Error(string message);
    }

    public class LogWrapper : ILog
    {
        public void Info(string message)
        {
            Log.Info(FTOptixLog.DDCCategory, message);
        }

        public void Warning(string message)
        {
            Log.Warning(FTOptixLog.DDCCategory, message);
        }

        public void Error(string message)
        {
            Log.Error(FTOptixLog.DDCCategory, message);
        }
    }

    public class SmartLogger
    {
        public class LogItem
        {
            public LogItem(LogType logType, string message)
            {
                LogType = logType;
                Message = message;
            }

            public string Message { get; set; }
            public LogType LogType { get; set; }
        }

        public SmartLogger(ILog log)
        {
            this.log = log;
        }

        public void LogInfo(string message, bool shouldQueueMessage = true, int charsNumberToCompare = 0)
        {
            Log(LogType.Info, message, shouldQueueMessage, charsNumberToCompare);
        }

        public void LogWarning(string message, bool shouldQueueMessage = true, int charsNumberToCompare = 0)
        {
            Log(LogType.Warning, message, shouldQueueMessage, charsNumberToCompare);
        }

        public void LogError(string message, bool shouldQueueMessage = true, int charsNumberToCompare = 0)
        {
            Log(LogType.Error, message, shouldQueueMessage, charsNumberToCompare);
        }

        private void Log(LogType logType, string message, bool shouldQueueMessage, int charsNumberToCompare)
        {
            if (shouldQueueMessage)
            {
                var logMessage = charsNumberToCompare > 0 ? GetSubstring(message, charsNumberToCompare) : message;
                var logItem = new LogItem(logType, logMessage);
                if (TryQueueLogItem(logItem))
                {
                    const char dot = '.';
                    const string moreErrorsText  = "...(possibly more similar errors).";
                    LogSpecific(logType, message.TrimEnd(dot) + moreErrorsText);
                }
            }
            else
            {
                LogSpecific(logType, message);
            }
        }

        private string GetSubstring(string text, int maxLength)
        {
            if (string.IsNullOrEmpty(text))
            {
                return text;
            }

            return text[..Math.Min(text.Length, maxLength)];
        }

        private void LogSpecific(LogType logType, string message)
        {
            switch (logType)
            {
                case LogType.Info:
                    log.Info(message);
                    break;
                case LogType.Warning:
                    log.Warning(message);
                    break;
                case LogType.Error:
                    log.Error(message);
                    break;
                default:
                    throw new NotImplementedException();
            }
        }

        private bool TryQueueLogItem(LogItem newLogItem)
        {
            if (previousLogItems.Any(item => item.LogType == newLogItem.LogType && item.Message.Equals(newLogItem.Message)))
            {
                return false;
            }

            previousLogItems.Enqueue(newLogItem);

            const int MaxQueueSize = 100;
            if (previousLogItems.Count > MaxQueueSize)
            {
                _ = previousLogItems.TryDequeue(out _);
            }

            return true;
        }

        private readonly ILog log;
        private readonly ConcurrentQueue<LogItem> previousLogItems = new();
    }
}


namespace DirectDriveConnectivity
{
    struct DataPointItem
    {
#pragma warning disable 0649 // The fields are assigned to by JSON deserialization
        public string Fqn;
        public List<string> InfoAttributes;
        public string DsId;
        public string PropertyName;
        public List<PropertyItem> Properties;
#pragma warning restore 0649
    }

    struct PropertyItem
    {
#pragma warning disable 0649 // The fields are assigned to by JSON deserialization
        public string Bin;
        public string Name;
        public string Value;
        public string Type;
#pragma warning restore 0649
    }

    class DataPointsJsonReader
    {
        public DataPointsJsonReader(string path, string ipAddress)
        {
            this.path = path;
            this.ipAddress = ipAddress;
        }

        public IEnumerable<DataPointItem> Read()
        {
            try
            {
                using (StreamReader r = new(path))
                {
                    var text = r.ReadToEnd();
                    string json = text.Replace("$ip", ipAddress);
                    return JsonConvert.DeserializeObject<List<DataPointItem>>(json);
                }
            }
            catch
            {
                Log.Error(FTOptixLog.DDCCategory, string.Format("Unable to read json file: {0}.", path));
                return Enumerable.Empty<DataPointItem>();
            }
        }

        private readonly string path;
        private readonly string ipAddress;
    }
}



namespace DirectDriveConnectivity
{
    struct ExtractionDirectiveItem
    {
#pragma warning disable 0649 // The fields are assigned to by JSON deserialization
        public string CipDataType;
        public uint ByteOffset;
        public uint BitOffset;
        public uint BitLength;
        public uint ByteLength;
#pragma warning restore 0649
    }

    class DataPointsModelCreator
    {
        public void Create(IEnumerable<DataPointItem> dataPointItems, IUAObject tagsFolder)
        {
            if (tagsFolder == null)
            {
                Log.Error(FTOptixLog.DDCCategory, "Tags folder does not exist. Data points will not be created.");
                return;
            }

            foreach (DataPointItem item in dataPointItems)
            {
                var dataTypeString = GetDataPointTypeFromJSON(item);

                IUAVariable tag;
                try
                {
                    tag = InformationModel.MakeVariable(item.PropertyName,
                        DPTypeToUATypeConverter.Convert(dataTypeString));

                    // https://reference.opcfoundation.org/Core/Part8/v104/docs/6.3.2
                    const uint UncertainInitialValue = 0x40920000;
                    tag.DataValue = new DataValue(tag.Value, UncertainInitialValue, tag.SourceTimestamp);
                }
                catch
                {
                    Log.Error(FTOptixLog.DDCCategory, "Cannot create data point variable " +
                        item.PropertyName + ". Unsupported data point type " + dataTypeString + ".");
                    continue;
                }

                tagsFolder.Add(tag);

                string[] neededProperties = { "cipHealthRequest", "extractionDirective", "multiplier", "pluginName" };
                foreach (PropertyItem property in item.Properties)
                {
                    if (neededProperties.Contains(property.Name))
                    {
                        var variable = NodeUtils.MakeReadOnlyProperty(property.Name, OpcUa.DataTypes.String);
                        tag.Add(variable);
                        variable.SetValueNoPermissions(property.Value);
                    }
                }
            }
        }

        private static ExtractionDirectiveItem? ReadExtractionDirectiveFromJSON(string json)
        {
            try
            {
                return JsonConvert.DeserializeObject<ExtractionDirectiveItem>(json);
            }
            catch
            {
                return null;
            }
        }

        private string GetDataPointTypeFromJSON(DataPointItem item)
        {
            var dataTypeString = "";
            foreach (var prop in item.Properties)
            {
                if (prop.Name == "extractionDirective")
                {
                    var extractionDirective = ReadExtractionDirectiveFromJSON(prop.Value);
                    dataTypeString = extractionDirective?.CipDataType;
                }
            }

            return dataTypeString;
        }
    }
}


namespace DirectDriveConnectivity
{
    static class DPTypeToUATypeConverter
    {
        public static NodeId Convert(string dtString)
        {
            switch (dtString)
            {
                case "BOOL":
                    return OpcUa.DataTypes.Boolean;
                case "SINT":
                    return OpcUa.DataTypes.SByte;
                case "INT":
                case "LE_INT":
                case "BE_INT":
                    return OpcUa.DataTypes.Int16;
                case "DINT":
                case "LE_DINT":
                case "BE_DINT":
                    return OpcUa.DataTypes.Int32;
                case "LINT":
                case "LE_LINT":
                case "BE_LINT":
                    return OpcUa.DataTypes.Int64;
                case "USINT":
                    return OpcUa.DataTypes.Byte;
                case "UINT":
                case "LE_UINT":
                case "BE_UINT":
                    return OpcUa.DataTypes.UInt16;
                case "UDINT":
                case "LE_UDINT":
                case "BE_UDINT":
                    return OpcUa.DataTypes.UInt32;
                case "ULINT":
                case "LE_ULINT":
                case "BE_ULINT":
                    return OpcUa.DataTypes.UInt64;
                case "VL_UINT":
                    return OpcUa.DataTypes.UInt32;
                case "VL_REAL":
                    return OpcUa.DataTypes.Float;
                case "REAL":
                    return OpcUa.DataTypes.Float;
                case "LREAL":
                    return OpcUa.DataTypes.Double;
                case "BYTE":
                    return OpcUa.DataTypes.Byte;
                case "WORD":
                    return OpcUa.DataTypes.UInt16;
                case "DWORD":
                    return OpcUa.DataTypes.UInt32;
                case "LWORD":
                    return OpcUa.DataTypes.UInt64;
                case "STRING":
                case "STRING2":
                case "SHORT_STRING":
                case "FLEX_STRING":
                case "STRING16":
                    return OpcUa.DataTypes.String;
                default:
                    throw new WrongDataPointTypeException(string.Format("Wrong data point type: {0}", dtString));
            }
        }
    }

    public class WrongDataPointTypeException : Exception
    {
        public WrongDataPointTypeException()
        {
        }

        public WrongDataPointTypeException(string message) : base(message)
        { 
        }

        public WrongDataPointTypeException(string message, Exception inner) : base(message, inner)
        { 
        }
    }
}
namespace DirectDriveConnectivity
{
    public static class ModelConsts
    {
        public const string PF6000TString = "PF6000T";
        public const string PF7000String = "PF7000";
        public const string PF755Sting = "PF755";
        public const string PF755TMString = "PF755TM";
    }
}


namespace DirectDriveConnectivity
{
    public class ModelUtils
    {
        class PowerFlex
        {
            public enum PowerFlexType
            {
                PowerFlex6000T = 0,
                PowerFlex7000 = 1,
                PowerFlex755 = 2,
                PowerFlex755TM = 3
            }

            static readonly Dictionary<PowerFlexType, string> powerFlexFileNames = new()
            {
                {PowerFlexType.PowerFlex6000T, "pf6000t.json"},
                {PowerFlexType.PowerFlex7000, "pf7000.json"},
                {PowerFlexType.PowerFlex755, "pf755.json"},
                {PowerFlexType.PowerFlex755TM, "pf755tm.json"},
            };

            static readonly Dictionary<PowerFlexType, string> powerFlexStrings = new()
            {
                {PowerFlexType.PowerFlex6000T, ModelConsts.PF6000TString},
                {PowerFlexType.PowerFlex7000, ModelConsts.PF7000String},
                {PowerFlexType.PowerFlex755, ModelConsts.PF755Sting},
                {PowerFlexType.PowerFlex755TM, ModelConsts.PF755TMString},
            };

            public PowerFlex(uint powerFlexNumber)
            {
                powerFlexString = powerFlexStrings[(PowerFlexType)powerFlexNumber];
                powerFlexFileName = powerFlexFileNames[(PowerFlexType)powerFlexNumber];
            }

            public string GetString()
            {
                return powerFlexString;
            }

            public string GetFileName()
            {
                return powerFlexFileName;
            }

            private readonly string powerFlexString;
            private readonly string powerFlexFileName;
        }

        class UpdateRate
        {
            public enum UpdateRateType
            {
                TenSeconds = 0,
                OneMinute = 1,
                TwoMinutes = 2,
                FiveMinutes = 3
            }

            public UpdateRate(uint powerFlexNumber)
            {
                updateRateDuration = updateRateDurations[(UpdateRateType)powerFlexNumber];
                updateRateString = updateRateStrings[(UpdateRateType)powerFlexNumber];
            }

            public uint GetValueInMiliseconds()
            {
                return updateRateDuration;
            }

            public string GetString()
            {
                return updateRateString;
            }

            static readonly Dictionary<UpdateRateType, uint> updateRateDurations = new()
            {
                {UpdateRateType.TenSeconds, 10000},
                {UpdateRateType.OneMinute, 60000},
                {UpdateRateType.TwoMinutes, 120000},
                {UpdateRateType.FiveMinutes, 300000},
            };

            static readonly Dictionary<UpdateRateType, string> updateRateStrings = new()
            {
                {UpdateRateType.TenSeconds, "10sec"},
                {UpdateRateType.OneMinute, "1min"},
                {UpdateRateType.TwoMinutes, "2min"},
                {UpdateRateType.FiveMinutes, "5min"},
            };

            private readonly uint updateRateDuration;
            private readonly string updateRateString;
        }

        public static readonly string Tags = "Tags";
        public static readonly string PowerFlexType = "PowerFlexType";
        public static readonly string IPAddress = "IPAddress";
        public static readonly string UpdateRateString = "UpdateRate";

        public ModelUtils(IUAObject logicObject)
        {
            this.logicObject = logicObject;
        }

        public IUANode[] GetDataPointModelFolders()
        {
            var children = logicObject?.Owner?.Children;
            var deviceFolderTypeNodeId = GetDeviceFolderNodeId();

            bool IsDeviceFolder(IUANode node)
            {
                if (node is UAObject uaObject)
                    return uaObject.IsInstanceOf(deviceFolderTypeNodeId);

                return false;
            }

            var folders = children?.Where(child => IsDeviceFolder(child)).ToArray();
            return folders ?? Array.Empty<IUANode>();
        }

        public NodeId GetDeviceFolderNodeId()
        {
            var typesFolder = logicObject?.Owner?.Get(DDC.Constants.TypesFolderName);
            if (typesFolder == null)
                throw new InvalidTypesStructureException("The Types folder could not be found");

            var deviceFolder = typesFolder.Get(DDC.Constants.DeviceFolderName);
            if (deviceFolder == null)
                throw new InvalidTypesStructureException("The DeviceFolder could not be found within the Types folder");
           
            if (deviceFolder.NodeId == null)
                throw new InvalidTypesStructureException("Unable to retrieve the DeviceFolder NodeId");

            return deviceFolder.NodeId;
        }

        public IUAObject GetDeviceFolderNode()
        {
            var deviceFolderTypeNodeId = GetDeviceFolderNodeId();
            return InformationModel.MakeObject(GetDataPointFolderName(), deviceFolderTypeNodeId);
        }

        public IUAObject CreateDataPointModelFolders()
        {
            var dataPointModelFolder = InformationModel.MakeObject(GetDataPointFolderName(), GetDeviceFolderNodeId());
            logicObject.Owner.Add(dataPointModelFolder);

            var updateRate = dataPointModelFolder.Get(UpdateRateString);
            if (updateRate is IUAVariable updateRateVariable)
                updateRateVariable.SetValueNoPermissions(GetUpdateRate());

            var ipAddress = dataPointModelFolder.Get(IPAddress);
            if (ipAddress is IUAVariable ipAddressVariable)
                ipAddressVariable.SetValueNoPermissions(GetIPAddressString());

            var powerFlexType = dataPointModelFolder.Get(PowerFlexType);
            if (powerFlexType is IUAVariable typeVariable)
                typeVariable.SetValueNoPermissions(GetPowerFlexTypeString());

            var tags = dataPointModelFolder.Get(Tags);
            return tags is Folder tagsFolder ? tagsFolder : null;
        }

        public bool DoesDataPointFolderExists()
        {
            var children = logicObject.Owner.Children;
            var childrenWithName = children.Any(child => child.BrowseName.Equals(GetDataPointFolderName()));

            return childrenWithName;
        }

        public string GetPowerFlexFileName()
        {
            var powerFlexType = logicObject.GetVariable(PowerFlexType) ?? throw new InvalidPropertyException("Invalid PowerFlexType property");
            return new PowerFlex(powerFlexType.Value).GetFileName();
        }

        public string GetIPAddressString()
        {
            var ip = logicObject.GetVariable(IPAddress) ?? throw new InvalidPropertyException("Invalid IPAddress property");
            return ip.Value;
        }

        public uint GetUpdateRate()
        {
            var rate = logicObject.GetVariable(UpdateRateString) ?? throw new InvalidPropertyException("Invalid UpdateRate property");
            return new UpdateRate(rate.Value).GetValueInMiliseconds();
        }

        public string GetUpdateRateString()
        {
            var rate = logicObject.GetVariable(UpdateRateString) ?? throw new InvalidPropertyException("Invalid UpdateRate property");
            return new UpdateRate(rate.Value).GetString();
        }

        private string GetPowerFlexTypeString()
        {
            var powerFlexType = logicObject.GetVariable(PowerFlexType) ?? throw new InvalidPropertyException("Invalid PowerFlexType property");
            return new PowerFlex(powerFlexType.Value).GetString();
        }

        private string GetDataPointFolderName()
        {
            var powerFlexModelName = GetPowerFlexTypeString();
            var separator = "_";
            var ipAddress = GetIPAddressString().Replace(".", separator);
            var link = ipAddress.Split("/")[^1].Split(":")[^1];
            var updateRate = GetUpdateRateString();

            return powerFlexModelName + separator + link + separator + updateRate;
        }

        private readonly IUAObject logicObject;
    }

    public class InvalidPropertyException : Exception
    {
        public InvalidPropertyException()
        { 
        }

        public InvalidPropertyException(string message) : base(message)
        { 
        }

        public InvalidPropertyException(string message, Exception inner) : base(message, inner)
        { 
        }
    }

    public class InvalidTypesStructureException : Exception
    {
        public InvalidTypesStructureException() 
        { 
        }

        public InvalidTypesStructureException(string message) : base(message) 
        { 
        }

        public InvalidTypesStructureException(string message, Exception inner) : base(message, inner) 
        { 
        }
    }
}


namespace DirectDriveConnectivity
{
    public class NodeUtils
    {
        public static IUAVariable MakeReadOnlyProperty(QualifiedName browseName, NodeId dataTypeId)
        {
            var context = ManagedContextStore.Context;
            int namespaceIndex = GetDefaultNamespaceIndex(context);

            return context.NodeFactory.MakeVariable(NodeId.Random(namespaceIndex), browseName, dataTypeId,
                OpcUa.VariableTypes.PropertyType, true, null, WriteMaskDefaults.VariableDefault, AccessLevelMask.CurrentRead);
        }

        private static int GetDefaultNamespaceIndex(IContext context)
        {
            int namespaceIndex = context.DefaultNamespaceIndex;
            if (namespaceIndex == NodeId.InvalidNamespaceIndex)
                namespaceIndex = Project.Current.NodeId.NamespaceIndex;

            return namespaceIndex;
        }
    }
}


namespace DirectDriveConnectivity
{
    public static class DevicePathValidatorMessages
    {
        public const string PathIsEmpty = "Path is empty.";
        public const string PathNotStartWithIP = "Path does not start with IPv4 address.";
        public const string WrongNumberOfSegments = "Wrong number of path segments.";
        public const string LastSegmentNotEthernet = "Last target device path segment should represent Ethernet/IP.";
        public const string WrongPathSegment = "Wrong target device path segment.";
        public const string PathNotAllowedChars = "Not allowed characters in target device path.";
        public const string HintText = " Please enter the path in correct format" +
            " (examples: 10.72.48.124, 10.72.48.124/1:2/2:192.168.0.10).";
    }

    public class Result
    {
        public bool Test { get; set; }
        public string Message { get; set; }
       
        public Result(bool test = true, string message = "")
        {
            Test = test;
            Message = string.IsNullOrEmpty(message) ? message : message + hintText;
        }

        private const string hintText = DevicePathValidatorMessages.HintText;
    }

    public class PathSegments
    {
        public string[] Segments { get; }
        public PathSegments(string path)
        {
            Segments = path.Split(@"/");
        }

        public string GetFirstPathSegment()
        {
            return Segments[0];
        }

        public string GetLastPathSegment()
        {
            return Segments[^1];
        }

        public PathSegments GetRemainingPathSegments(int index = 0)
        {
            return new PathSegments(Segments.Where((val, idx) => idx > index).ToArray());
        }

        public bool IsEmpty()
        {
            return Segments.Length == 0;
        }

        public int Length()
        {
            return Segments.Length;
        }

        private PathSegments(string[] segments)
        {
            Segments = segments;
        }
    }

    // Target device path validator, according to:
    // http://cat.web.ra-int.com/wp-content/cat/cda/releases/1.1.1/usermanual/CipCommunication.html#Addressing_CipPathSchema_TargetDevicePath
    // Additional assumptions:
    // 1) 1 or 3 path segments,
    // 2) second segment is chassis-like (e.g 1:4),
    // 3) last segment is Ethernet/IP-like (e.g. 2:192.169.1.1)
    public class TargetDevicePathValidator
    {
        public TargetDevicePathValidator(string path)
        {
            this.path = path;
        }

        public Result Validate()
        {
            if (string.IsNullOrEmpty(path))
            {
                return new Result(false, DevicePathValidatorMessages.PathIsEmpty);
            }

            if (!CheckAllowedCharsOnly(path))
            {
                return new Result(false, DevicePathValidatorMessages.PathNotAllowedChars);
            }

            var segments = new PathSegments(path);
            if (segments.Length() == 1)
            {
                return ValidateOneSegmentPath(segments);
            }

            return ValidateThreeSegmentsPath(segments);
        }

        private static Result ValidateThreeSegmentsPath(PathSegments segments)
        {
            if (segments.Length() != 3)
            {
                return new Result(false, DevicePathValidatorMessages.WrongNumberOfSegments);
            }

            if (!IsValidIPv4(segments.GetFirstPathSegment()))
            {
                return new Result(false, DevicePathValidatorMessages.PathNotStartWithIP);
            }

            var remainingSegments = segments.GetRemainingPathSegments();

            if (!IsValidBackplanePathSegment(remainingSegments.GetFirstPathSegment()))
            {
                return new Result(false, DevicePathValidatorMessages.WrongPathSegment);
            }

            if (!IsValidEthernetIPPathSegment(remainingSegments.GetLastPathSegment()))
            {
                return new Result(false, DevicePathValidatorMessages.LastSegmentNotEthernet);
            }

            return new Result();
        }

        private static Result ValidateOneSegmentPath(PathSegments segments)
        {
            if (segments.IsEmpty())
            {
                return new Result(false, DevicePathValidatorMessages.WrongNumberOfSegments);
            }

            if (!IsValidIPv4(segments.GetFirstPathSegment()))
            {
                return new Result(false, DevicePathValidatorMessages.PathNotStartWithIP);
            }

            return new Result();
        }

        private static bool CheckAllowedCharsOnly(string value)
        {
            const string allowedCharactersPattern = @"[0-9/\.:]+";
            var regexp = new Regex(beginPattern +
                allowedCharactersPattern +
                endPattern);

            return regexp.Matches(value).Count >= 1;
        }

        private static bool IsValidIPv4(string value)
        {
            var regexp = new Regex(beginPattern +
                IPv4Pattern +
                endPattern);

            return regexp.Matches(value).Count == 1;
        }

        private static bool IsValidEthernetIPPathSegment(string value)
        {
            const string colonPattern = @"\:";
            const string portPattern = @"(2)";
            var regexp = new Regex(beginPattern +
                portPattern +
                colonPattern +
                IPv4Pattern +
                endPattern);

            return regexp.Matches(value).Count == 1;
        }

        private static bool IsValidBackplanePathSegment(string value)
        {
            const string colonPattern = @"\:";
            const string portPattern = @"(1)";
            const string linkAddressPattern = @"([0-9]|1[0-6])";

            var regexp = new Regex(beginPattern +
                portPattern +
                colonPattern +
                linkAddressPattern +
                endPattern);

            return regexp.Matches(value).Count == 1;
        }

        private static string IPv4Pattern
        {
            get
            {
                const string octetWithoutZeroPattern = @"(25[0-5]|2[0-4]?[0-9]?|1[0-9]?[0-9]?|[1-9][0-9]?)";
                const string octetWithZeroPattern = @"(25[0-5]|2[0-4]?[0-9]?|1[0-9]?[0-9]?|[1-9][0-9]?|0)";
                const string dotPattern = @"\.";

                return octetWithoutZeroPattern +
                    dotPattern +
                    octetWithZeroPattern +
                    dotPattern +
                    octetWithZeroPattern +
                    dotPattern +
                    octetWithZeroPattern;
            }
        }

        private const string beginPattern = @"^";
        private const string endPattern = @"$";
        private readonly string path;
    }
}

namespace DirectDriveConnectivity
{
    public class CdaRequest
    {
        public CdaRequest()
        { 
        }

        public string GetDataPointName(IUANode dataPointFromModel)
        {
            return dataPointFromModel.NodeId.ToString();
        }

        public List<DataPointBasic> ExtractDataPointsFromModel(IUAObject tags)
        {
            var dataPoints = new List<DataPointBasic>();
            foreach (var dataPointFromModel in tags.Children)
            {
                var dataPointName = GetDataPointName(dataPointFromModel);
                var dataPoint = new DataPointBasic();
                dataPoint.Name = dataPointName;
                dataPoint.BrowseName = dataPointFromModel.BrowseName;
                dataPoint.CipHealthRequest = dataPointFromModel.GetVariable("cipHealthRequest").Value;
                dataPoint.ExtractionDirective = dataPointFromModel.GetVariable("extractionDirective").Value;
                dataPoint.Multiplier = dataPointFromModel.GetVariable("multiplier").Value;
                if (dataPointFromModel.GetVariable("pluginName") != null)
                {
                    dataPoint.PluginName = dataPointFromModel.GetVariable("pluginName").Value;
                }
                else
                {
                    dataPoint.PluginName = "";
                }

                dataPoints.Add(dataPoint);
            }

            return dataPoints;
        }

        // to send less actions to CDA adapter we need to group data points with uniqe contextUri
        // many data points have the same contextUri, for example:
        // ra-cip://driver-cip/10.76.36.188/1:3/srv:1/cls:1/ins:1 contains many data points with fault bits
        public List<DataPointCdaAction> CreateUniqCdaActions(List<DataPointBasic> dataPoints, int updateRate)
        {
            HashSet<string> uniqCipContextUris = new HashSet<string>();
            foreach (var dataPoint in dataPoints)
            {
                uniqCipContextUris.Add(dataPoint.CipHealthRequest);
            }

            var uniqCdaActions = new List<DataPointCdaAction>();
            foreach (var contextUri in uniqCipContextUris)
            {
                DataPointCdaAction singleCdaAction = new DataPointCdaAction();
                singleCdaAction.CipHealthRequest = contextUri;
                singleCdaAction.UpdateRate = updateRate;
                singleCdaAction.DataPoints = dataPoints.Where(dataPoint => dataPoint.CipHealthRequest == contextUri).ToList();

                uniqCdaActions.Add(singleCdaAction);
            }

            return uniqCdaActions;
        }

        public string CreateIdentityRequestUri(string path, string cipDriverName = "driver-cip")
        {
            return string.Format("ra-cip://{0}/{1}/srv:1/cls:1/ins:1", cipDriverName, path);
        }

        public Ia.ActionData PrepareIdentityAction(string uri, string name)
        {
            string nameBase64;
            string warningMessage = "Not able to convert {0} to Base64 while creating identity action.";
            try
            {
                nameBase64 = Convert.ToBase64String(System.Text.Encoding.UTF8.GetBytes(name));
            }
            catch (Exception ex)
            {
                if (ex is ArgumentNullException || ex is System.Text.EncoderFallbackException)
                {
                    Log.Warning(FTOptixLog.DDCCategory, string.Format(warningMessage, name));
                    nameBase64 = rnd.Next(1, 10000).ToString();
                }
                else
                {
                    throw;
                }  
            }

            var actionData = new Ia.ActionData("");
            actionData.Add("", Ia.ActionType.Read, uri, null, Globals.IdentityTrackingIdPrefix + nameBase64);
            return actionData;
        }

        public (Ia.ActionData start, Ia.ActionData stop) PrepareCdaActions(List<DataPointCdaAction> cdaRequests)
        {
            Ia.ActionData startActionData = new Ia.ActionData("");
            Ia.ActionData stopActionData = new Ia.ActionData("");

            uint counter = 0;
            cdaRequests.ForEach(cdaRequest =>
            {
                var trackingId = "tracking_id_" + cdaRequest.CipHealthRequest + "_" + cdaRequest.UpdateRate + "_" + counter.ToString();
                counter++;

                // CDA is going to respond with these data points in response.reactionOptions. We are going to use them to decode
                // response buffer
                Dictionary<string, List<DataPointBasic>> dataPoints = new();
                dataPoints.Add("dataPoints", cdaRequest.DataPoints);

                startActionData.Add(
                    "", // it has to be empty, if not CDA reports error: Conversion failed, SegmentItem.
                    Ia.ActionType.StartMonitor, // actionType
                    cdaRequest.CipHealthRequest, // contextUri
                    null, // contextPrefix,  if you set to undefined cgp-core generates a unique prefix
                    trackingId, // trackingId, has to be unique for each action
                    cdaRequest.UpdateRate, // updateRateMs
                    Ia.UpdateType.Polling,
                    null, // groupActionOptions
                    null, // groupReactionOptions
                    null, // itemActionOptions
                    dataPoints // itemReactionOptions
                );

                stopActionData.Add(
                    "",
                    Ia.ActionType.StopMonitor,
                    cdaRequest.CipHealthRequest,
                    null,
                    trackingId,
                    cdaRequest.UpdateRate,
                    Ia.UpdateType.Polling
                );

            });

            return (startActionData, stopActionData);
        }

        private readonly Random rnd = new Random();
    }
}

namespace DirectDriveConnectivity
{
    public class NodeValuePair
    {
        public NodeValuePair(IUANode node, DataValue value)
        {
            Node = node;
            DataValue = value;
        }

        public IUANode Node { get; }
        public DataValue DataValue { get; }
    };

    public class CdaResponse
    {
        public CdaResponse(ConcurrentDictionary<string, IUANode> uaNodes)
        {
            this.uaNodes = uaNodes;
        }

        public IdentityCode ProcessIdentityResponse(CgpBaseClasses.DataItemResponse response)
        {
            var reactionOptions = JObject.FromObject(response.reactionOptions);
            var cipStatus = reactionOptions.GetValue("cipStatus");
            var generalStatus = (int)JObject.FromObject(cipStatus).GetValue("general");
            const int CIP_STATUS_SUCCESS = 0;
            if (generalStatus != CIP_STATUS_SUCCESS || response.vqts[0].q != (int)Ia.QualityCode.GOOD)
            {
                return new IdentityCode();
            }

            JArray datasource = (JArray)response.vqts[0].v;
            byte[] bufferSource = datasource.ToObject<byte[]>();
            var vendorIdExtractionDirective = new ExtractionDirective()
            {
                CipDataType = "UINT",
                ByteOffset = 0
            };
            var deviceTypeExtractionDirective = new ExtractionDirective()
            {
                CipDataType = "UINT",
                ByteOffset = 2
            };
            var productCodeExtractionDirective = new ExtractionDirective()
            {
                CipDataType = "UINT",
                ByteOffset = 4
            };

            Datapoint vendorId = new()
            {
                ExtractionDirective = vendorIdExtractionDirective
            };
            vendorId.Configure();

            Datapoint deviceType = new()
            {
                ExtractionDirective = deviceTypeExtractionDirective
            };
            deviceType.Configure();

            Datapoint productCode = new()
            {
                ExtractionDirective = productCodeExtractionDirective
            };
            productCode.Configure();

            var vendorIdValue = vendorId.GetValueFromSourceData(bufferSource);
            var deviceTypeValue = deviceType.GetValueFromSourceData(bufferSource);
            var productCodeValue = productCode.GetValueFromSourceData(bufferSource);
            return vendorIdValue != null && deviceTypeValue != null && productCodeValue != null ?
                new IdentityCode(vendorIdValue, deviceTypeValue, productCodeValue) :
                new IdentityCode();
        }

        public List<NodeValuePair> Process(CgpBaseClasses.DataItemResponse response)
        {
            var vqtTime = DateTime.Parse(response.vqts[0].t, cultureInfo).ToUniversalTime();
            var reactionOptions = JObject.FromObject(response.reactionOptions);
            var dataPoints = reactionOptions.GetValue("dataPoints").ToObject<List<DataPointBasic>>();
            if (dataPoints == null || dataPoints.Count == 0)
            {
                var baseMessage = "No data points attached to the CDA response for contextUri:";
                var message = baseMessage + " {0}.";
                Globals.Logger.LogError(string.Format(message, response.contextUri), true, baseMessage.Length);
                return new List<NodeValuePair>();
            }

            // sometimes in a CDA response may come empty buffer with GOOD quality code,
            // it means that cip device answered to the CDA but with some kind of error status (CIP VOL1 B-1 General Status Codes)
            var cipStatus = reactionOptions.GetValue("cipStatus");
            var generalStatus = (int)JObject.FromObject(cipStatus).GetValue("general");
            const int CIP_STATUS_SUCCESS = 0;
            if (generalStatus != CIP_STATUS_SUCCESS || response.vqts[0].q != (int)Ia.QualityCode.GOOD)
            {
                return SetBadQualityToDataPoints(dataPoints, vqtTime);
            }

            JArray datasource = (JArray)response.vqts[0].v;
            byte[] bufferSource = datasource.ToObject<byte[]>();
            byte[] bufferDpi = new byte[0];

            var returnContainer = new List<NodeValuePair>();

            foreach (var dataPoint in dataPoints)
            {
                var extractionDirective = JsonConvert.DeserializeObject<ExtractionDirective>(dataPoint.ExtractionDirective);
                Datapoint datapoint = new Datapoint();
                datapoint.ExtractionDirective = extractionDirective;
                datapoint.Configure();

                // in DPI parameter, incoming buffer may have values on positions that can change. We need to convert buffer to a standard
                // one and then process data points
                if (dataPoint.PluginName == "DPI" && bufferDpi.Length == 0)
                {
                    var dpiPlugin = new DpiPlugin();
                    bufferDpi = dpiPlugin.ParseBuffer(bufferSource);
                }

                var uaNode = uaNodes[dataPoint.Name];
                var bufferToProcess = (dataPoint.PluginName == "DPI") ? bufferDpi : bufferSource;

                var value = datapoint.GetValueFromSourceData(bufferToProcess);
                var dataToSendToOptix = new NodeValuePair(uaNode, new DataValue(value, (UInt32)Globals.UaQualityCode.Good, vqtTime));
                returnContainer.Add(dataToSendToOptix);
            }
            return returnContainer;
        }

        private List<NodeValuePair> SetBadQualityToDataPoints(List<DataPointBasic> dataPoints, DateTime vqtTime)
        {
            var returnContainer = new List<NodeValuePair>();
            foreach (var dataPoint in dataPoints)
            {
                var uaNode = uaNodes[dataPoint.Name];
                var defaultValue = (uaNode as IUAVariable).Value;
                returnContainer.Add(new NodeValuePair(uaNode, new DataValue(defaultValue, (UInt32)Globals.UaQualityCode.Bad, vqtTime)));
            }
            return returnContainer;
        }

        public List<NodeValuePair> SetBadQualityToDataPoints(Ia.ActionData actions)
        {
            var returnContainer = new List<NodeValuePair>();
            actions.Actions()?.ToList().ForEach(action =>
            {
                var reactionOptions = JObject.FromObject(action.reactionOptions);
                var dataPoints = reactionOptions.GetValue("dataPoints").ToObject<List<DataPointBasic>>();
                returnContainer.AddRange(SetBadQualityToDataPoints(dataPoints, DateTime.UtcNow));
            });
            return returnContainer;
        }

        private readonly ConcurrentDictionary<string, IUANode> uaNodes;
        private readonly CultureInfo cultureInfo = new CultureInfo("en-US");
    }
}

namespace DirectDriveConnectivity
{
    public class DataForOptixReadyEventArgs : EventArgs
    {
        public List<NodeValuePair> data { get; set; }
    }

    public class StopActionsAvailableEventArgs : EventArgs
    {
        public Ia.ActionData data { get; set; }
    }

    public class CgpApplication : CgpBaseClasses.Application
    {
        public event EventHandler<DataForOptixReadyEventArgs> DataForOptixReady;
        public event EventHandler<StopActionsAvailableEventArgs> StopActionsAvailable;

        public CgpApplication(ConcurrentBag<DeviceActions> devicesActions, ConcurrentDictionary<string, IUANode> uaNodes) : base()
        {
            cdaResponse = new(uaNodes);
            this.devicesActions = devicesActions;
        }

        public override void OnConsume(CgpBaseClasses.DataItemResponse[] responses)
        {
            foreach (var response in responses)
            {
                if (response.trackingId is not string)
                {
                    continue;
                }
                string responseTrackingId = response.trackingId.ToString();

                if (responseTrackingId.StartsWith(Globals.IdentityTrackingIdPrefix, StringComparison.Ordinal))
                {
                    var currentAction = FindIdentityAction(responseTrackingId);
                    if (currentAction != null)
                    {
                        var identityNotRetrievedText = "Unable to retrieve device identity from response related to tracking id: {0}.";
                        var identityNotRetrievedMessage = string.Format(identityNotRetrievedText, responseTrackingId);

                        IdentityCode identityCode;
                        try
                        {
                            identityCode = cdaResponse.ProcessIdentityResponse(response);
                        }
                        catch
                        {
                            SetBadQualityToDataPoints(identityNotRetrievedMessage, currentAction.StartActions);
                            continue;
                        }

                        if (identityCode.IsEmpty())
                        {
                            SetBadQualityToDataPoints(identityNotRetrievedMessage, currentAction.StartActions);
                            continue;
                        }

                        if (currentAction.PfCodes.Contains(identityCode))
                        {
                            OnStopActionsAvailable(currentAction.StopActions);
                            SendRequest(currentAction.StartActions);
                        }
                        else
                        {
                            var identityCodeNotFittingText = "The device identity ({0}) does not match the selected device.";
                            var identityCodeNotFittingMessage = string.Format(identityCodeNotFittingText, identityCode.ToString());
                            SetBadQualityToDataPoints(identityCodeNotFittingMessage, currentAction.StartActions);
                        }
                    }
                    else
                    {
                        var message = string.Format("No device action related to tracking id {0}.", responseTrackingId);
                        Log.Warning(FTOptixLog.DDCCategory, message);
                    }
                }
                else
                {
                    try
                    {
                        var processedResponse = cdaResponse.Process(response);
                        OnDataForOptixReady(processedResponse);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(FTOptixLog.DDCCategory, string.Format("Exception during processing CDA response: {0}.", ex.Message));
                    }
                }
            }
        }

        protected void OnDataForOptixReady(List<NodeValuePair> processedResponse)
        {
            if (processedResponse.Count > 0)
            {
                var args = new DataForOptixReadyEventArgs();
                args.data = processedResponse;
                OnDataForOptixReady(args);
            }
        }

        protected virtual void OnDataForOptixReady(DataForOptixReadyEventArgs e)
        {
            DataForOptixReady?.Invoke(this, e);
        }

        protected void OnStopActionsAvailable(Ia.ActionData stopActions)
        {
            if (stopActions != null)
            {
                var args = new StopActionsAvailableEventArgs
                {
                    data = stopActions
                };
                OnStopActionsAvailable(args);
            }
        }

        protected virtual void OnStopActionsAvailable(StopActionsAvailableEventArgs e)
        {
            StopActionsAvailable?.Invoke(this, e);
        }

        private DeviceActions FindIdentityAction(string trackingId)
        {
            foreach (var deviceActions in devicesActions)
            {
                var actions = deviceActions.IdentityAction.Actions();
                if (!actions.Any())
                {
                    continue;
                }

                // only one action expected here
                var firstAction = actions.FirstOrDefault();
                if (firstAction != null && ((string)firstAction.trackingId).Equals(trackingId))
                {
                    return deviceActions;
                }
            }

            return null;
        }

        private void SetBadQualityToDataPoints(string message, Ia.ActionData startActions)
        {
            Log.Warning(FTOptixLog.DDCCategory, message);
            var nodeValuePairs = cdaResponse.SetBadQualityToDataPoints(startActions);
            OnDataForOptixReady(nodeValuePairs);
        }

        private readonly CdaResponse cdaResponse;
        private readonly ConcurrentBag<DeviceActions> devicesActions;
    }
}

namespace DirectDriveConnectivity
{
    public class CgpProcess
    {
        public bool StartCgp()
        {
            if (cgpProcessObject is not null)
            {
                Log.Info(FTOptixLog.DDCCategory, "CgpProcess already started");
                return false;
            }

            try
            {
                string cgpFolder = Path.Combine(Project.Current.ProjectDirectory, DDC.Constants.CGPFolderName);
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = Path.Combine(cgpFolder, cgpExecutableName);
                startInfo.WorkingDirectory = cgpFolder;
                startInfo.UseShellExecute = false;
                startInfo.CreateNoWindow = true;
                cgpProcessObject = Process.Start(startInfo);
            }
            catch (Exception ex)
            {
                Log.Error(FTOptixLog.DDCCategory, $"StartCgp thrown: {ex.Message}");
                return false;
            }

            return true;
        }

        public void StopCgp()
        {
            if (cgpProcessObject is null)
            {
                return;
            }

            try
            {
                if (cgpProcessObject.HasExited)
                {
                    Log.Info(FTOptixLog.DDCCategory, "StopCgp: CGP process already exited");
                }
                else
                {
                    cgpProcessObject.Kill();
                    cgpProcessObject.WaitForExit(5000);
                }
            }
            catch (Exception ex)
            {
                Log.Error(FTOptixLog.DDCCategory, $"StopCgp thrown: {ex.Message}");
            }
            finally
            {
                cgpProcessObject.Dispose();
                cgpProcessObject = null;
            }
        }

        private Process cgpProcessObject = null;
        private const string cgpExecutableName = DDC.Constants.CGPExecutableNameWindows;
    }
}

namespace DirectDriveConnectivity
{
    public class DataPointBasic
    {
        public string Name { get; set; }
        public string BrowseName { get; set; }
        public string CipHealthRequest { get; set; }
        public string ExtractionDirective { get; set; }
        public string Multiplier { get; set; }
        public string PluginName { get; set; }
    }

    public class DataPointCdaAction
    {
        public string CipHealthRequest { get; set; }
        public int UpdateRate { get; set; }
        public List<DataPointBasic> DataPoints { get; set; }
    }
}

namespace DirectDriveConnectivity
{
    // Remark: this is only partial implementation of Dpi descriptor, for full reference go to the Dpi Object Spec:
    // "Table 3.8 – Dpi Descriptor Attribute Definition"
    [Flags]
    public enum DpiDescriptor
    {
        SignType = 0b1000,     // 0 = unsigned, 1 = signed
        ExtDataTypeBit4 = 0b0010000000000000000, // bit 16
        ExtDataTypeBit5 = 0b0100000000000000000, // bit 17
        ExtDataTypeBit6 = 0b1000000000000000000, // bit 18
    }

    // Drive Peripheral Interface (Dpi) Object Class Specification. Table 3 9 – Dpi Parameter Data Type
    public enum DpiDataType
    {
        Bool8 = 0b000,
        Bool16 = 0b001,
        Byte = 0b010,
        Word = 0b011,
        Dword = 0b100,
        Char = 0b101,   // not implemented
        Real = 0b110,
        Extended = 0b111,
        Bool32 = Extended | DpiDescriptor.ExtDataTypeBit4
    }

    public class DpiParameterObject : ICloneable
    {
        public DpiDescriptor Descriptor;
        public DpiDataType DataType;
        public dynamic Value;
        public dynamic DefaultValue;
        public dynamic Minimum;
        public dynamic Maximum;
        public uint Multiplier;
        public uint Divisor;
        public uint DpiBase;
        public short Offset;
        public string Units;
        public string Name;
        public uint DecimalPlace;
        public float Percent;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }

    public class DpiPlugin
    {
        public byte[] ParseBuffer(byte[] buffer)
        {
            if (buffer.Length == 0)
            {
                return buffer;
            }

            DpiParameterObject dpiObject = ExtractDpiObject(buffer);
            DpiParameterObject engineeringDpiObject = ConvertToEngineeringObject(dpiObject);
            return this.CreateNewBuffer(engineeringDpiObject);
        }

        private DpiParameterObject ExtractDpiObject(byte[] buffer)
        {
            DpiParameterObject dpiObject = new DpiParameterObject();
            const uint DpiDataTypeMask = 0b1110000000000000111;
            uint fullDescriptor = BitConverter.ToUInt32(buffer, (int)DpiDescriptorOffset);

            dpiObject.Descriptor = (DpiDescriptor)fullDescriptor;
            dpiObject.DataType = (DpiDataType)(fullDescriptor & DpiDataTypeMask);
            dpiObject.Value = this.DecodeInternalValue(buffer, DpiParameterValueOffset, dpiObject);
            dpiObject.Minimum = this.DecodeInternalValue(buffer, DpiOnlineMinimumOffset, dpiObject);
            dpiObject.Maximum = this.DecodeInternalValue(buffer, DpiOnlineMaximumOffset, dpiObject);
            dpiObject.DefaultValue = this.DecodeInternalValue(buffer, DpiOnlineDefaultValueOffset, dpiObject);
            dpiObject.Multiplier = BitConverter.ToUInt16(buffer, (int)DpiMultiplierOffset);
            dpiObject.Divisor = BitConverter.ToUInt16(buffer, (int)DpiDivisorOffset);
            dpiObject.DpiBase = BitConverter.ToUInt16(buffer, (int)DpiBaseOffset);
            dpiObject.Offset = BitConverter.ToInt16(buffer, (int)DpiOffsetOffset);
            dpiObject.Units = System.Text.Encoding.UTF8
                .GetString(buffer, index: (int)DpiOnlineParameterUnitsOffset, count: (int)DpiOnlineParameterUnitsLength)
                .Trim();
            dpiObject.Name = System.Text.Encoding.UTF8
                .GetString(buffer, index: (int)DpiParameterNameOffset, count: (int)DpiParameterNameLength)
                .Trim();
            dpiObject.DecimalPlace = (fullDescriptor & DpiDecimalPlaceMask) >> (int)DpiDecimalPlaceBitOffset;

            return dpiObject;
        }

        // Dpi decode internal value from CONTAINER type (32 bit) to number using Dpi descriptor.
        // Following types are decoded:
        //      8-bit unsigned and signed integer value
        //      16-bit unsigned and signed integer value
        //      32-bit unsigned and signed integer value
        //      IEEE 754 32-bit float value
        // All other types are returned as 32-bit unsigned value.
        private dynamic DecodeInternalValue(byte[] buffer, uint offset, DpiParameterObject dpiObj)
        {
            // Extract internal value from the input based on the data type.
            bool isSigned = dpiObj.Descriptor.HasFlag(DpiDescriptor.SignType);
            switch (dpiObj.DataType)
            {
                case DpiDataType.Byte:
                    return isSigned ? (sbyte)buffer[(int)offset] : buffer[(int)offset];

                case DpiDataType.Word:
                    return isSigned ? BitConverter.ToInt16(buffer, (int)offset) : BitConverter.ToUInt16(buffer, (int)offset);

                case DpiDataType.Dword:
                    return isSigned ? BitConverter.ToInt32(buffer, (int)offset) : BitConverter.ToUInt32(buffer, (int)offset);

                case DpiDataType.Real:
                    return BitConverter.ToSingle(buffer, (int)offset);

                case DpiDataType.Bool8:
                    return buffer[(int)offset];

                case DpiDataType.Bool16:
                    return BitConverter.ToUInt16(buffer, (int)offset);

                case DpiDataType.Bool32:
                    return BitConverter.ToUInt32(buffer, (int)offset);

                default:
                    var message = string.Format("Unknown dpiDataType type: {0} {1}.", dpiObj.Name, (uint)dpiObj.DataType);
                    Log.Error(FTOptixLog.DDCCategory, message);
                    return BitConverter.ToUInt32(buffer, (int)offset);
            }
        }

        private DpiParameterObject ConvertToEngineeringObject(DpiParameterObject dpiObject)
        {
            DpiParameterObject engineeringObject = (DpiParameterObject)dpiObject.Clone();

            // Dpi BOOL types contains only status bits, we don't want to perform any math operations here, just return it as it is
            if (dpiObject.DataType == DpiDataType.Bool8 ||
                dpiObject.DataType == DpiDataType.Bool16 ||
                dpiObject.DataType == DpiDataType.Bool32)
            {
                return engineeringObject;
            }

            engineeringObject.Value = this.CalculateEngineeringValue(dpiObject.Value, dpiObject);
            engineeringObject.Minimum = this.CalculateEngineeringValue(dpiObject.Minimum, dpiObject);
            engineeringObject.Maximum = this.CalculateEngineeringValue(dpiObject.Maximum, dpiObject);
            engineeringObject.DefaultValue = this.CalculateEngineeringValue(dpiObject.DefaultValue, dpiObject);
            if (dpiObject.Minimum == 0 && dpiObject.Maximum == 0)
            {
                engineeringObject.Percent = 0;
            } 
            else
            {
                engineeringObject.Percent = (100 * dpiObject.Value) / Math.Max(Math.Abs((float)dpiObject.Minimum), Math.Abs((float)dpiObject.Maximum));
            }
            engineeringObject.Percent = float.IsNaN(engineeringObject.Percent) ? 0 : engineeringObject.Percent;
            return engineeringObject;
        }

        float CalculateEngineeringValue(float value, DpiParameterObject dpiObject)
        {
            float result = 0;
            if (dpiObject.DataType == DpiDataType.Real)
            {
                result = ((value + dpiObject.Offset) * dpiObject.Multiplier * dpiObject.DpiBase) / dpiObject.Divisor;
            }
            else
            {
                result =
                    ((value + dpiObject.Offset) * dpiObject.Multiplier * dpiObject.DpiBase) /
                    (dpiObject.Divisor * (float)Math.Pow(10, dpiObject.DecimalPlace));
            }

            return result;
        }

        private byte[] CreateNewBuffer(DpiParameterObject engObject)
        {
            byte[] newBuffer = new byte[5 * 4 + 1 + engObject.Units.Length + 1 + engObject.Name.Length];

            // Write output values to result buffer.
            var bytes = BitConverter.GetBytes(engObject.Value);
            Buffer.BlockCopy(bytes, 0, newBuffer, (int)DpiDataPointValueOffset, bytes.Length);
            bytes = BitConverter.GetBytes(engObject.Percent);
            Buffer.BlockCopy(bytes, 0, newBuffer, (int)DpiDataPointPercentOffset, bytes.Length);
            bytes = BitConverter.GetBytes(engObject.Minimum);
            Buffer.BlockCopy(bytes, 0, newBuffer, (int)DpiDataPointMinimumOffset, bytes.Length);
            bytes = BitConverter.GetBytes(engObject.Maximum);
            Buffer.BlockCopy(bytes, 0, newBuffer, (int)DpiDataPointMaximumOffset, bytes.Length);
            bytes = BitConverter.GetBytes(engObject.DefaultValue);
            Buffer.BlockCopy(bytes, 0, newBuffer, (int)DpiDataPointDefaultOffset, bytes.Length);

            int offset = (int)DpiDataPointUnitsOffset;
            newBuffer[offset] = (byte)engObject.Units.Length;
            offset += 1;
            byte[] unitBytes = Encoding.UTF8.GetBytes(engObject.Units);
            Buffer.BlockCopy(unitBytes, 0, newBuffer, offset, unitBytes.Length);
            offset += engObject.Units.Length;
            newBuffer[offset] = (byte)engObject.Name.Length;
            offset += 1;
            byte[] nameBytes = Encoding.UTF8.GetBytes(engObject.Name);
            Buffer.BlockCopy(nameBytes, 0, newBuffer, offset, nameBytes.Length);

            return newBuffer;
        }

        // Dpi Parameter Data Point offsets. These are used in profiles
        private readonly uint DpiDataPointValueOffset = 0;
        private readonly uint DpiDataPointPercentOffset = 4;
        private readonly uint DpiDataPointMinimumOffset = 8;
        private readonly uint DpiDataPointMaximumOffset = 12;
        private readonly uint DpiDataPointDefaultOffset = 16;
        private readonly uint DpiDataPointUnitsOffset = 20;

        // Dpi Parameter Object, Dpi Attribute 7 (Dpi Online Read Full) offsets.
        // These offsets are used in a raw response from power flex device, we need to convert them into standard response
        // (used in profiles)
        // For more information about Attribute 7 refer to: Drive Peripheral Interface (Dpi) Object Class Specification, chapter 3.2.7
        private readonly uint DpiDescriptorOffset = 0;
        private readonly uint DpiParameterValueOffset = 4;
        private readonly uint DpiOnlineMinimumOffset = 8;
        private readonly uint DpiOnlineMaximumOffset = 12;
        private readonly uint DpiOnlineDefaultValueOffset = 16;
        private readonly uint DpiOnlineParameterUnitsOffset = 24;
        private readonly uint DpiMultiplierOffset = 28;
        private readonly uint DpiDivisorOffset = 30;
        private readonly uint DpiBaseOffset = 32;
        private readonly uint DpiOffsetOffset = 34;
        private readonly uint DpiParameterNameOffset = 40;

        // Dpi string lengths.
        private readonly uint DpiOnlineParameterUnitsLength = 4;
        private readonly uint DpiParameterNameLength = 16;

        // Dpi Decimal Place
        private readonly uint DpiDecimalPlaceMask = 0x0000f000;
        private readonly uint DpiDecimalPlaceBitOffset = 12;
    }

}

namespace DirectDriveConnectivity
{
    static class Globals
    {
        public enum UaQualityCode : UInt32
        {
            Good = 0,
            Bad = 0x80000000,
        }

        public const string IdentityTrackingIdPrefix = "tracking_id_identity_";
        public static readonly SmartLogger Logger = new SmartLogger(new LogWrapper());
    }
}

namespace DirectDriveConnectivity
{
    public class IdentityCode : IEquatable<IdentityCode>
    {
        public IdentityCode(UInt16 vendorId = 0, UInt16 productType = 0, UInt16 productCode = 0)
        {
            VendorId = vendorId;
            ProductType = productType;
            ProductCode = productCode;
        }

        public UInt16 VendorId { get; }
        public UInt16 ProductType { get; }
        public UInt16 ProductCode { get; }

        public bool IsEmpty()
        {
            return VendorId == 0 &&
                ProductType == 0 &&
                ProductCode == 0;
        }

        public bool Equals(IdentityCode other)
        {
            if (other == null)
                return false;

            return VendorId == other.VendorId &&
                ProductType == other.ProductType &&
                ProductCode == other.ProductCode;
        }

        public override bool Equals(object obj)
        {
            var code = obj as IdentityCode;
            if (code == null)
                return false;

            return Equals(code);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(VendorId, ProductType, ProductCode);
        }

        public override string ToString()
        {
            return string.Format("Vendor Id : {0}, Product Type : {1}, Product Code : {2}",
                VendorId, ProductType, ProductCode);
        }
    }
    public static class PFCodesCreator
    {
        private static void GenerateCodes()
        {
            var pf755Codes = (from type in PF755DeviceTypes
                              from code in PF755ProductCodes
                              select new IdentityCode(RAVendorId, type, code)).ToList();
            codes[ModelConsts.PF755Sting] = pf755Codes;

            var pf755tmCodes = (from type in PF755TMBusInverterDeviceTypes
                                from code in PF755TMBusInverterProductCodes
                                select new IdentityCode(RAVendorId, type, code)).ToList();
            codes[ModelConsts.PF755TMString] = pf755tmCodes;

            var pf6000TCodes = (from type in PF6000TDeviceTypes
                                from code in PF6000TProductCodes
                                select new IdentityCode(RAVendorId, type, code)).ToList();
            codes[ModelConsts.PF6000TString] = pf6000TCodes;

            var pf7000Codes = (from type in PF7000DeviceTypes
                               from code in PF7000ProductCodes
                               select new IdentityCode(RAVendorId, type, code)).ToList();
            codes[ModelConsts.PF7000String] = pf7000Codes;
        }

        public static List<IdentityCode> GetCodes(string powerFlexType)
        {
            if (!codes.Any())
            {
                GenerateCodes();
            }

            return codes.TryGetValue(powerFlexType, out var itemCode) ? itemCode : new List<IdentityCode>();
        }

        private readonly static UInt16[] PF755DeviceTypes = { 120, 123, 129, 140, 142, 143 };
        private readonly static UInt16[] PF755ProductCodes = { 2192, 3216 };

        private readonly static UInt16[] PF755TMBusInverterDeviceTypes = { 143 };
        private readonly static UInt16[] PF755TMBusInverterProductCodes = { 10128 };

        private readonly static UInt16[] PF6000TDeviceTypes = { 143 };
        private readonly static UInt16[] PF6000TProductCodes = { 10640 };

        private readonly static UInt16[] PF7000DeviceTypes = { 123, 152 };
        private readonly static UInt16[] PF7000ProductCodes = { 64, 65 };

        private readonly static UInt16 RAVendorId = 1;

        private readonly static Dictionary<string, List<IdentityCode>> codes = new();
    }
}

namespace CGP_CS.utils
{
    public static class CipHelper
    {

        public static dynamic DynamicCast(CipDataType cipType, dynamic val)
        {
            dynamic castValue = 0;
            try
            {
                switch (cipType)
                {
                    // note: no support for constructed data types
                    case CipDataType.BOOL:
                        if (val is bool)
                            castValue = (val == true);
                        else
                            castValue = (val != 0);
                        break;
                    case CipDataType.SINT:
                        castValue = (sbyte)val;
                        break;
                    case CipDataType.INT:
                    case CipDataType.LE_INT:
                    case CipDataType.BE_INT:
                        castValue = (short)val;
                        break;
                    case CipDataType.DINT:
                    case CipDataType.LE_DINT:
                    case CipDataType.BE_DINT:
                        castValue = (int)val;
                        break;
                    case CipDataType.LINT:
                    case CipDataType.LE_LINT:
                    case CipDataType.BE_LINT:
                        castValue = (long)val;
                        break;
                    case CipDataType.USINT:
                        castValue = (byte)val;
                        break;
                    case CipDataType.UINT:
                    case CipDataType.LE_UINT:
                    case CipDataType.BE_UINT:
                    case CipDataType.VL_UINT:
                        castValue = (ushort)val;
                        break;
                    case CipDataType.UDINT:
                    case CipDataType.LE_UDINT:
                    case CipDataType.BE_UDINT:
                        castValue = (uint)val;
                        break;
                    case CipDataType.ULINT:
                    case CipDataType.LE_ULINT:
                    case CipDataType.BE_ULINT:
                        castValue = (ulong)val;
                        break;
                    case CipDataType.REAL:
                    case CipDataType.VL_REAL:
                        castValue = Convert.ToSingle(val);
                        break;
                    case CipDataType.LREAL:
                        castValue = Convert.ToDouble(val);
                        break;
                    case CipDataType.BYTE:
                        castValue = (byte)val;
                        break;
                    case CipDataType.WORD:
                        castValue = (ushort)val;
                        break;
                    case CipDataType.DWORD:
                        castValue = (uint)val;
                        break;
                    case CipDataType.LWORD:
                        castValue = (ulong)val;
                        break;
                    case CipDataType.STRING:
                    case CipDataType.STRING2:
                    case CipDataType.STRING16:
                    case CipDataType.SHORT_STRING:
                    case CipDataType.FLEX_STRING:
                    case CipDataType.USINT_LOOKUP_HART_EU:
                    case CipDataType.USINT_LOOKUP_HART_CONNECTED:
                    case CipDataType.USINT_LOOKUP_MANUFACTURER_ID:
                        castValue = (string)val;
                        break;
                    default:
                        castValue = 0;
                        break;
                }
            }
            catch (InvalidCastException)
            {
            }
            catch (Exception)
            {
            }
            return castValue;
        }

    }
}

namespace CGP_CS.Enums
{
    public static class EnumUtil
    {
        public static T ParseEnum<T>(string value)
        {
            return (T)Enum.Parse(typeof(T), value, ignoreCase: true);
        }

        //Get the description string from the enum
        //Note: it shows as not referenced, but it is used for Engineering units
        public static String DescriptionAttribute<T>(this T src)
        {
            FieldInfo fieldInfo = src.GetType().GetField(src.ToString());
            if (fieldInfo != null)
            {
                DescriptionAttribute[] attr = (DescriptionAttribute[])fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), false);

                if (attr.Length > 0)
                    return attr[0].Description;
                else
                    return src.ToString();
            }
            else
            {
                return string.Empty;
            }
        }
    }
}

