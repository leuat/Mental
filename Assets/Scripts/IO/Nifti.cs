using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;




public class Nifti
{
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
        public float intent_p2;    /*!< 2nd intent parameter. */  /* short unused10;      */
                                                                  /* short unused11;      */
        public float intent_p3;    /*!< 3rd intent parameter. */  /* short unused12;      */
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

    public int DATATYPE = 3;

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

    public void Load(string filename, int dataType)
    {
        DATATYPE = dataType;
        if (!File.Exists(filename))
        {
            Debug.Log("Cannot find file: " + filename);
            return;
        }
        FileStream fs = File.Open(filename, FileMode.Open);
        header = Nifti.HeaderFromStream(fs);

        for (int i = 0; i < 4; i++)
            Debug.Log("N" + i + " : " + header.dim[i]);
        
        Debug.Log("bitpix:" + header.pixdim);
        Debug.Log("Datatype: " + header.datatype);
        Allocate();
        Debug.Log("File size should be " + (DATATYPE * size.x * size.y * size.z) / 1024 + " mb");
        fs.Read(rawData, 0, (int)(size.x*size.y*size.z*DATATYPE));

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
                    rawData[3 * idx + 2] = (byte)(val*0.3f);

                }

    }

    public VolumetricTexture toTexture(Vector3 resizeScale)
    {
        Vector3 newScale = new Vector3(size.x / resizeScale.x, size.y / resizeScale.y, size.z / resizeScale.z);

        VolumetricTexture vt = new VolumetricTexture(newScale);
        vt.fromByteArray(rawData, resizeScale, size, DATATYPE);
/*        VolumetricTexture vt = new VolumetricTexture(size);
        vt.fromByteArray(rawData);*/
        return vt;

    }

    void Allocate()
    {
        size.x = header.dim[0];
        size.y = header.dim[1];
        size.z = header.dim[2];

        //        data = new Color[(int)(size.x*size.y*size.z)];
        rawData = new byte[(int)(size.x * size.y * size.z * DATATYPE)];
    }

    void Fill()
    {

    }

    public Nifti(string filename, int dataType)
    {
        Load(filename, dataType);
    }

    public Vector3 size = new Vector3(1, 1, 1);
    public byte[] rawData;





}
