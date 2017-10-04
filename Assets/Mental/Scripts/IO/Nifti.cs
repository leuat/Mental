using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;




public class Nifti
{

    public class Label
    {
        public string name;
        public int index;
        public Vector3 color;
        public Vector3 extra;
        public Label(int i, string n, Vector3 c, Vector3 e)
        {
            extra = e;
            color = c;
            name = n;
            index = i;
        }
        public Label(string[] s)
        {
            index = int.Parse(s[0]);
            color.x = float.Parse(s[1]);
            color.y = float.Parse(s[2]);
            color.z = float.Parse(s[3]);

            extra.x = float.Parse(s[4]);
            extra.y = float.Parse(s[5]);
            extra.z = float.Parse(s[6]);

            name = s[7];
        }
    }

    public struct NiftiHeader
    {

        public int sizeof_hdr;    /*!< MUST be 348           */  /* int sizeof_hdr;      */
        //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 10)]
        public char[] data_type; /*!< ++UNUSED++            */  /* char data_type[10];  */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 18)]
        public char[] db_name;   /*!< ++UNUSED++            */  /* char db_name[18];    */
        public int extents;       /*!< ++UNUSED++            */  /* int extents;         */
        public short session_error; /*!< ++UNUSED++            */  /* short session_error; */
        public char regular;       /*!< ++UNUSED++            */  /* char regular;        */
        public char dim_info;      /*!< MRI slice ordering.   */  /* char hkey_un0;       */

        /*--- was image_dimension substruct ---*/
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public short[] dim;        /*!< Data array dimensions.*/  /* short dim[8];        */
        public float intent_p1;    /*!< 1st intent parameter. */  /* short unused8;       */
                                                                  /* short unused9;       */
        public char intent_p2;    /*!< 2nd intent parameter. */  /* short unused10;      */
                                                                 /* short unused11;      */
        public char intent_p3;    /*!< 3rd intent parameter. */  /* short unused12;      */
                                                                 /* short unused13;      */
        public short intent_code;  /*!< NIFTI_INTENT_* code.  */  /* short unused14;      */
        public short datatype;      /*!< Defines data type!    */  /* short datatype;      */
        public short bitpix;        /*!< Number bits/voxel.    */  /* short bitpix;        */
        public short slice_start;   /*!< First slice index.    */  /* short dim_un0;       */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public float[] pixdim;     /*!< Grid spacings.        */  /* float pixdim[8];     */
        public float vox_offset;    /*!< Offset into .nii file */  /* float vox_offset;    */
        public float scl_slope;    /*!< Data scaling: slope.  */  /* float funused1;      */
        public float scl_inter;    /*!< Data scaling: offset. */  /* float funused2;      */
        public short slice_end;     /*!< Last slice index.     */  /* float funused3;      */
        public char slice_code;   /*!< Slice timing order.   */
        public char xyzt_units;   /*!< Units of pixdim[1..4] */
        public float cal_max;       /*!< Max display intensity */  /* float cal_max;       */
        public float cal_min;       /*!< Min display intensity */  /* float cal_min;       */
        public float slice_duration;/*!< Time for 1 slice.     */  /* float compressed;    */
        public float toffset;       /*!< Time axis shift.      */  /* float verified;      */
        public int glmax;         /*!< ++UNUSED++            */  /* int glmax;           */
        public int glmin;         /*!< ++UNUSED++            */  /* int glmin;           */

        /*--- was data_history substruct ---*/
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 80)]
        public char[] descrip; /*!< any text you like.    */  /* char descrip[80];    */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public char[] aux_file;  /*!< auxiliary filename.   */  /* char aux_file[24];   */

        public short qform_code;   /*!< NIFTI_XFORM_* code.   */  /*-- all ANALYZE 7.5 ---*/
        public short sform_code;   /*!< NIFTI_XFORM_* code.   */  /*   fields below here  */
                                                                  /*   are replaced       */
        public float quatern_b;    /*!< Quaternion b param.   */
        public float quatern_c;    /*!< Quaternion c param.   */
        public float quatern_d;    /*!< Quaternion d param.   */
        public float qoffset_x;    /*!< Quaternion x shift.   */
        public float qoffset_y;    /*!< Quaternion y shift.   */
        public float qoffset_z;    /*!< Quaternion z shift.   */

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] srow_x;    /*!< 1st row affine transform.   */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] srow_y;    /*!< 2nd row affine transform.   */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public float[] srow_z;    /*!< 3rd row affine transform.   */

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]

        public char[] intent_name;/*!< 'name' or meaning of data.  */
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public char[] magic;      /*!< MUST be "ni1\0" or "n+1\0". */

    };                   /**** 348 bytes total ****/

    /*    public static NiftiHeader ByteArrayToObject(byte[] arrBytes)
        {
            using (var memStream = new MemoryStream())
            {
                var binForm = new BinaryFormatter();
                memStream.Write(arrBytes, 0, arrBytes.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                NiftiHeader obj = (NiftiHeader)binForm.Deserialize(memStream);
                return obj ;
            }
        }*/


    public enum DataType
    {
        DT_NONE, DT_BINARY, DT_UNSIGNED_CHAR, DT_SIGNED_SHORT, DT_SIGNED_INT, DT_FLOAT, DT_COMPLEX,
        DT_DOUBLE, DT_RGB, DT_INT8, DT_UINT16, DT_UINT32, DT_INT64, DT_UINT64, DT_FLOAT128
    };


    public DataType dataType = DataType.DT_NONE;
    public int BytesPerPixel = 0;
    public Dictionary<int, Label> indexedColors = new Dictionary<int, Label>();

    public static NiftiHeader HeaderFromStream(FileStream fs)
    {
        byte[] b = new byte[348];
        fs.Read(b, 0, 348);
        var handle = GCHandle.Alloc(b, GCHandleType.Pinned);
        NiftiHeader h = (NiftiHeader)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(NiftiHeader));
        handle.Free();
        return h;
    }

    NiftiHeader header;

    public static DataType getDataType(int type)
    {
        type = Mathf.Abs(type);
        if (type == 1)
            return DataType.DT_BINARY;
        if (type == 2)
            return DataType.DT_UNSIGNED_CHAR;
        if (type == 4)
            return DataType.DT_SIGNED_SHORT;
        if (type == 8)
            return DataType.DT_SIGNED_INT;
        if (type == 16)
            return DataType.DT_FLOAT;
        if (type == 32)
            return DataType.DT_DOUBLE;
        if (type == 128)
            return DataType.DT_RGB;
        if (type == 512)
            return DataType.DT_UINT16;
        if (type == 768)
            return DataType.DT_UINT32;
        if (type == 1024)
            return DataType.DT_INT64;
        if (type == 1280)
            return DataType.DT_UINT64;
        if (type == 1536)
            return DataType.DT_FLOAT128;


        return DataType.DT_NONE;
    }
    

    private void LoadLabelTextFile(string fileName)
    {
        StreamReader theReader = new StreamReader(fileName);
        string line;
        using (theReader)
        {
            do
            {
                line = theReader.ReadLine();
                if (line != null && line[0]!='#')
                {
                    string[] s = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    Label l = new Label(s);
                    indexedColors[l.index] = l;
                }
            }
            while (line != null);
            theReader.Close();
        }
    }


    public void LoadLabel(string filename)
    {
        LoadLabelTextFile(filename);

    }



    public void Load(string filename)
    {
        if (!File.Exists(filename))
        {
            Debug.Log("Cannot find file: " + filename);
            return;
        }
        FileStream fs = File.Open(filename, FileMode.Open);
        header = Nifti.HeaderFromStream(fs);

        /*        for (int i = 0; i < 4; i++)
                    Debug.Log("N" + i + " : " + header.dim[i]);
                    */

        dataType = getDataType((int)header.datatype);
        BytesPerPixel = header.bitpix / 8;

        Debug.Log("bpix:" + header.bitpix);
        Debug.Log("datatype:" + header.datatype);
        if (VerifyFeature())
        {
            Allocate();
            Debug.Log("File size should be " + (BytesPerPixel * size.x * size.y * size.z) / 1024 + " mb");
            fs.Read(rawData, 0, (int)(size.x * size.y * size.z * BytesPerPixel));
        }
        else
            Debug.Log("ERROR: File type " + dataType + " not yet supported aargh");

        fs.Close();

    }


    private void TestBall()
    {
        header.dim[0] = 128;
        header.dim[1] = 128;
        header.dim[2] = 128;
        //Debug.Log(header.bitpix);
        Allocate();

        for (int i = 0; i < size.x; i++)
            for (int j = 0; j < size.y; j++)
                for (int k = 0; k < size.z; k++)
                {
                    Vector3 p = (new Vector3(i, j, k) - new Vector3(size.x, size.y, size.z) / 2);
                    p.x /= size.x;
                    p.y /= size.y;
                    p.z /= size.z;
                    float d = p.magnitude;

                    float val = Mathf.Clamp(Mathf.Exp(-d * d / 0.02f) * 256, 0, 255);
                    int idx = (int)(i * size.y * size.z + j * size.z + k);
                    rawData[3 * idx + 0] = (byte)val;
                    rawData[3 * idx + 1] = (byte)val;
                    rawData[3 * idx + 2] = (byte)(val * 0.3f);
                }
    }

    public static void Normalize(float[] array)
    {
        float min = 1E30f, max = -1E30f;
        for (int i = 0; i < array.Length; i++)
        {
            min = Mathf.Min(array[i], min);
            max = Mathf.Min(array[i], max);
        }
        for (int i = 0; i < array.Length; i++)
            array[i] /= max * 255f;


    }

    public void downsampleToRGB(Vector3 newScale)
    {
        int p = 0;
        Vector3 newSize = new Vector3(size.x / newScale.x, size.y / newScale.y, size.z / newScale.z);
        byte[] data = new byte[(int)(3 * newSize.x* newSize.y* newSize.z)];
        byte[] floatArray = new byte[4];
        byte c1=0, c2=0, c3=0;
        for (float i = 0; i < size.x; i += newScale.x)
            for (float j = 0; j < size.y; j += newScale.y)
                for (float k = 0; k < size.z; k += newScale.z)
                {
                    int idx = (int)(i * size.z * size.y + j * size.z + k);
                    if (dataType == DataType.DT_RGB)
                    {
                        data[3 * p + 0] = rawData[3 * idx + 0];
                        data[3 * p + 1] = rawData[3 * idx + 1];
                        data[3 * p + 2] = rawData[3 * idx + 2];
                    }
                    if (dataType == DataType.DT_UINT16)
                    {
                        ushort val = (ushort)(((rawData[2 * idx + 1]) << 8) | (rawData[2 * idx + 0]));
                        if (!hasIndexing || !indexedColors.ContainsKey(val))
                        {
                            c1 = (byte)(val / 256);
                            c2 = (byte)(val / 256);
                            c3 = (byte)(val / 256);
                        }
                        else
                        {
                            Label l = indexedColors[val];
                            c1 = (byte)l.color.x;
                            c2 = (byte)l.color.y;
                            c3 = (byte)l.color.z;
                        }
                        data[3 * p + 0] = c1;
                        data[3 * p + 1] = c2;
                        data[3 * p + 2] = c3;
                    }
                    if (dataType == DataType.DT_FLOAT)
                    {
                        for (int l = 0; l < 4; l++)
                            floatArray[l] = rawData[4 * idx + l];

                        float val = BitConverter.ToSingle(floatArray, 0);
                        data[3 * p + 0] = (byte)(val / 256);
                        data[3 * p + 1] = (byte)(val / 256);
                        data[3 * p + 2] = (byte)(val / 256);
                    }

                    p++;
                }
        size = newSize;
        rawData = data;
    }

    public bool VerifyFeature()
    {
        if (dataType == DataType.DT_RGB || dataType == DataType.DT_UINT16 || dataType == DataType.DT_FLOAT)
            return true;

        return false;

    }


    public VolumetricTexture toTexture(Vector3 resizeScale)
    {
        if (!VerifyFeature())
        {
            Debug.Log("Error: feature not implemented (" + dataType + ")");
            return null;
        }

        downsampleToRGB(resizeScale);
        VolumetricTexture vt = new VolumetricTexture(size);
        vt.fromByteArray(rawData);
        return vt;

    }

    void Allocate()
    {
        size.x = header.dim[0];
        size.y = header.dim[1];
        size.z = header.dim[2];
        rawData = new byte[(int)(size.x * size.y * size.z * BytesPerPixel)];
    }

    void Fill()
    {

    }

    private bool hasIndexing = false;

    public Nifti(string filename)
    {
        string[] split = filename.Split('.');
 
        if (split[1] == "nii")
        {
            Load(Application.dataPath + "/../data/" + filename);
            return;
        }
        if (split[1] == "label")
        {
            LoadLabel(Application.dataPath + "/../data/" + filename);
            hasIndexing = true;
            Load(Application.dataPath + "/../data/" + split[0] + ".nii");

            return;
        }
        Debug.Log("Could not recognize file format");

    }

    public Vector3 size = new Vector3(1, 1, 1);
    public byte[] rawData;


    public Vector3 findNewResolutionScale(int forceValue)
    {
        Vector3 val;// = new Vector3(size.x, size.y, size.z);

        val.x = Mathf.Max((int)(size.x / forceValue), 1);
        val.y = Mathf.Max((int)(size.y / forceValue), 1);
        val.z = Mathf.Max((int)(size.z / forceValue), 1);

        return val;
    }




}
