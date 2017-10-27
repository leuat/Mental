using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class VolumetricTexture
{
	public Texture3D texture;
	public Vector3 size = Vector3.zero;
    public Color32[] cols;


    public VolumetricTexture ()
	{

	}

	public VolumetricTexture (Vector3 s)
	{
		size = s;
        InitializeCols();

	}


	static byte Reverse (byte b)
	{
		int rev = (b >> 4) | ((b & 0xf) << 4);
		rev = ((rev & 0xcc) >> 2) | ((rev & 0x33) << 2);
		rev = ((rev & 0xaa) >> 1) | ((rev & 0x55) << 1);
		return (byte)rev;
	}

	/*    public void fromRGBByteArray(byte[] bytes, Vector3 newScale, Vector3 orgSize)
        {
            Color c;
            c.a = 1;
            int p = 0;
            texture = new Texture3D((int)size.x, (int)size.y, (int)size.z, TextureFormat.ARGB32, true);
            for (float i = 0; i < orgSize.x;i+=newScale.x)
                for (float j = 0; j < orgSize.y; j += newScale.y)
                    for (float k = 0; k < orgSize.z; k += newScale.z)
                    {
                        int idx = (int)(i*orgSize.z*orgSize.y + j*orgSize.z + k);
                        c.r = bytes[3*idx + 0]/255.0f;
                        c.g = bytes[3 * idx + 1] / 255.0f;
                        c.b = bytes[3 * idx + 2] / 255.0f;
                        //if (idx>bytes.Length) { }
                        c.a = c.r;
                        cols[p++] = c;
                        //setColor((int)(i / newScale.x), (int)(j / newScale.y), (int)(k / newScale.z), c);
            }
            texture.SetPixels(cols);
            texture.Apply();

        }

        public void fromFloatArray(float[] val, Vector3 newScale, Vector3 orgSize, Color orgColor)
        {
            Color c;
            c.a = 1;
            int p = 0;
            texture = new Texture3D((int)size.x, (int)size.y, (int)size.z, TextureFormat.ARGB32, true);
            for (float i = 0; i < orgSize.x; i += newScale.x)
                for (float j = 0; j < orgSize.y; j += newScale.y)
                    for (float k = 0; k < orgSize.z; k += newScale.z)
                    {
                        int idx = (int)(i * orgSize.z * orgSize.y + j * orgSize.z + k);
                        float v = val[idx + 0];
                        c.r = v*orgColor.r;
                        c.g = v * orgColor.g;
                        c.b = v * orgColor.b;

                        cols[p++] = c;
                    }
            texture.SetPixels(cols);
            texture.Apply();

        }
        */

    public void InitializeCols()
    {
        cols = new Color32[(int)(size.x * size.y * size.z)];

    }

	public void CreateTexture ()
	{
		texture = new Texture3D ((int)size.x, (int)size.y, (int)size.z, TextureFormat.ARGB32, true);
        texture.wrapMode = TextureWrapMode.Clamp;
	}

	public void fromByteArray (byte[] bytes)
	{
		Color c;
		c.a = 1;
		CreateTexture ();
		for (int i = 0; i < size.x * size.y * size.z; i++) {
			c.r = bytes [3 * i + 0] / 255.0f;
			c.g = bytes [3 * i + 1] / 255.0f;
			c.b = bytes [3 * i + 2] / 255.0f;
			c.a = c.r;
			cols [i] = c;
		}
        Apply();

	}




/*	public void RandomDots (float N, float cutoff, VolumetricTexture other)
	{
		Color c = new Color ();
		c.a = 1;
		CreateTexture ();
		for (int i = 0; i < size.x; i++) {
			for (int j = 0; j < size.y; j++) {
				for (int k = 0; k < size.z; k++) {
					int x = i;
					int y = j;
					int z = k;

					Color oc = other.getColor (x, y, z);
					if (oc.r > cutoff || oc.g > cutoff || oc.b > cutoff || oc.a > cutoff) {
						//if (labels == null) 
						setColor (x, y, z, oc);
						//else 
					}
				}
			}
		}
		texture.SetPixels (cols);
		texture.Apply ();

	}
*/
	public void RandomDots (float N, float cutoff, VolumetricTexture other)
	{
		Color c = new Color ();
		c.a = 1;
		CreateTexture ();
		for (int i = 0; i < N; i++) {
			int x = Random.Range (0, (int)size.x);
			int y = Random.Range (0, (int)size.y);
			int z = Random.Range (0, (int)size.z);

			c.r = Random.value;
			c.g = Random.value;
			c.b = Random.value;

			Color oc = other.getColor (x, y, z);
			if (oc.r > cutoff || oc.g > cutoff || oc.b > cutoff || oc.a > cutoff) {
				//if (labels == null) 
				setColor (x, y, z, oc);
				//else 

			}
		}
        Apply();

	}


	public static float colLen(Color c) {
		return c.a + c.r + c.g + c.b;
	}

	public void ClipData(VolumetricTexture o, float s) {
		if (s==0)
		for (int i = 0; i < cols.Length; i++) {
			if (colLen (o.cols [i]) > 0)
				cols [i].a = cols [i].r;
			else
				cols [i].a = 0;

		}
		if (s==1)
			for (int i = 0; i < cols.Length; i++) {
				if (colLen (o.cols [i]) > 0)
					cols [i].a = 0;
				else
					cols [i].a = cols [i].r;

			}
        Apply();

	}


	public void CreateNoise (Vector3 s, float scale, float amp)
	{
		size = s;
		texture = new Texture3D ((int)size.x, (int)size.y, (int)size.z, TextureFormat.ARGB32, true);
		Vector3 cs = new Vector3 (0.856f, 1.234f, 2.3f) * scale;
        //cols = new Color[(int)(size.x * size.y * size.z)];
        InitializeCols();
		Color c = new Color (1.0f, 0.7f, 0.2f, 1.0f);
		for (int i = 0; i < size.x; i++)
			for (int j = 0; j < size.y; j++)
				for (int k = 0; k < size.z; k++) {
					Vector3 v = new Vector3 (i / (float)size.x, j / (float)size.y, k / (float)size.z);

					float val = Mathf.Clamp (Util.noise4D.raw_noise_3d (v.x * scale, v.y * scale, v.z * scale) - 0.8f, 0, 1);

					c.r = Util.noise4D.raw_noise_3d (v.x * cs.x, v.y * cs.x, v.z * cs.x);
					c.g = Util.noise4D.raw_noise_3d (v.x * cs.y, v.y * cs.y, v.z * cs.y);
					c.b = Util.noise4D.raw_noise_3d (v.x * cs.z, v.y * cs.z, v.z * cs.z);
					c.a = val;
					c.r = Mathf.Clamp (c.r, 0, 255);
					c.g = Mathf.Clamp (c.g, 0, 255);
					c.b = Mathf.Clamp (c.b, 0, 255);
					c.a = Mathf.Clamp (c.a, 0, 255);

					setColor (i, j, k, c*amp);

				}
        Apply();

	}

	public void setColor (int x, int y, int z, Color c)
	{
		if (x < 0 || x >= size.x)
			return;
		if (y < 0 || y >= size.y)
			return;
		if (z < 0 || z >= size.z)
			return;
            
		//            cols[x * size * size + (size-1-y) * size + z] = c;
		cols [(int)(x * size.z * size.y + (y) * size.x + z)] = c;
	}

    public void Apply()
    {
        texture.SetPixels32(cols);
        texture.Apply();
    }

    public Color getColor (int x, int y, int z)
	{
		if (x < 0 || x >= size.x)
			return Color.black;
		if (y < 0 || y >= size.y)
			return Color.black;;
		if (z < 0 || z >= size.z)
			return Color.black;;
		//            cols[x * size * size + (size-1-y) * size + z] = c;
		return cols [(int)(x * size.z * size.y + (y) * size.x + z)];
	}

	/*    private void singleGrass(int x, int y, int wx, int wy, float h)
        {
            Color c = new Color(0.3f, 1f, 0.3f, 1);
            float theta = Random.value * 2 * Mathf.PI;


            Vector2 dir = new Vector3(Mathf.Cos(theta), Mathf.Sin(theta));
            Vector2 pert = new Vector2(0, 0);

            for (int i = 0; i < h; i++)
            {
                for (int xx = x - wx; xx < x + wx; xx++)
                    for (int yy = y - wy; yy < y + wy; yy++)
                    {
                        float dist = 1 / (new Vector2(xx - x, yy - y).magnitude + 1);
                        Color col = c * dist;
                        col.a = 1;
                        setColor(xx + (int)pert.x, i, yy + (int)pert.y, col);
                        pert += dir * i * i / size * 0.001f;



                    }

            }


        }
        */

	/*    public void CreateGrass(int N, int Count)
        {
            size = N;
            texture = new Texture3D(size, size, size, TextureFormat.ARGB32, true);

            cols = new Color[size * size * size];
            for (int i = 0; i < cols.Length; i++)
                cols[i] = new Color(0, 0, 0, 0);

            int s = 2;

            for (int i = 0; i < Count; i++)
            {
                int x = (int)Mathf.Clamp((int)(Random.value * size), s, size - s);
                int y = (int)Mathf.Clamp((int)(Random.value * size), s, size - s);
                int h = (int)(size * 0.5f + 0.5f * (int)(Random.value * size));

                singleGrass(x, y, s, s, h);


            }

            texture.SetPixels(cols);
            texture.Apply();
        }
        */

    

}