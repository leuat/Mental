using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using UnityEngine;


namespace LemonSpawn
{

    public class ChunkReader : MonoBehaviour
    {
        public byte[] bytes;
        public bool isLoaded;
        public bool isDeflated;
        public bool failed;
        public static IEnumerator ImageChunkFromUrl(string s, ChunkReader cr)
        {
            cr.isLoaded = false;
            cr.isDeflated = false;
            WWW www = new WWW(s);
            //material = m;
            yield return www;
            cr.bytes = www.bytes;
            if (cr.bytes != null)
            {
                cr.isLoaded = true;
            }
            else
                Debug.Log("ERROR cannot load chunks: " + s);

        }

        public void getChunk(string chunk)
        {
            //"https://neuroglancer.humanbrainproject.org/precomputed/BigBrainRelease.2015/8bit/20um/0-64_0-64_0-64"
            StartCoroutine(ImageChunkFromUrl(chunk, this));
        }

        public static string getPositionString(Vector3 p, Vector3 s)
        {
            //            0 - 64_0 - 64_0 - 64
            string str = ((int)p.x).ToString() + "-";
            str += ((int)s.x).ToString() + "_";
            str += ((int)p.y).ToString() + "-";
            str += ((int)s.y).ToString() + "_";
            str += ((int)p.z).ToString() + "-";
            str += ((int)s.z).ToString();
            return str;
        }


        public static void CopyStream(Stream destination, int bufferSize = 81920)
        {
            byte[] array = new byte[bufferSize];
            int count;
            while ((count = destination.Read(array, 0, array.Length)) != 0)
            {
                destination.Write(array, 0, count);
            }
        }

        public static byte[] Decompress(byte[] data)
        {
            var compressedStream = new MemoryStream(data);
            var zipStream = new GZipStream(compressedStream, CompressionMode.Decompress);
            var resultStream = new MemoryStream();

            var buffer = new byte[4096];
            int read;
            while ((read = zipStream.Read(buffer, 0, buffer.Length)) > 0)
            {
                resultStream.Write(buffer, 0, read);
            }

            return resultStream.ToArray();
        }



        public bool Deflate()
        {
            if (isDeflated)
                return true;

            isDeflated = true;
            failed = false;
            byte[] nb = null;
            try
            {
                nb = Decompress(bytes);
            }
            catch (System.Exception e)
            {
                //failed = true;
                return false;
            }

            bytes = nb;
            return true;
        }


    }

    public class Chunk
    {
        public ChunkReader cr;
        public Vector3 position = new Vector3();
        public Vector3 localPosition = new Vector3();
        public Vector3 size = new Vector3();
        public VolumetricTexture volTex;
        public bool isApplied = false;
        public bool hasChanged = false;
        private GameObject go;
        private static byte[] zeros;

        public Chunk(VolumetricTexture vt, Vector3 s, Vector3 p, Vector3 lp, string data, string mu)
        {
            isApplied = false;
            volTex = vt;
            position = p;
            size = s;
            localPosition = lp;
            string d = data + mu + ChunkReader.getPositionString(p, p + s);
            go = new GameObject();
            cr = go.AddComponent<ChunkReader>();
            if (zeros == null)
            {
                zeros = new byte[(int)(s.x * s.y * s.z)];
                for (int i = 0; i < zeros.Length; i++)
                    zeros[i] = 255;
            }
            cr.getChunk(d);

        }

        public void Apply()
        {
            //            Debug.Log("len:"  +cr.bytes.Length);
            //            Debug.Log("Supposed to be: " + size.x * size.y * size.z);
            if (!cr.isDeflated)
            {
                Debug.Log("Is not yet deflated!");
                return;
            }
            if (cr.bytes == null)
                return;
                
            
            for (int i = 0; i < size.x; i++)
                for (int j = 0; j < size.y; j++)
                    for (int k = 0; k < size.z; k++)
                    {
                        int idx = (int)(k * size.y * size.z + j * size.z + i);
                        byte val = cr.bytes[idx];
                        float v = (Mathf.Clamp(255 - val, 0, 255)) / 255f;
                        volTex.setColor(i + (int)localPosition.x, j + (int)localPosition.y, k + (int)localPosition.z, Color.white * v);

                    }
            hasChanged = true;
            isApplied = true;
        }


        public void Update()
        {
            if (cr == null)
                return;
            if (!cr.isLoaded)
                return;

            if (cr.isDeflated == false)
            {
                if (!cr.Deflate()) 
                    cr.bytes = zeros;
            }
            
            if (!isApplied)
            {
                Apply();
            }
        }

        public void UpdateNonThreaded()
        {
            if (cr == null)
                return;
            if (cr.isLoaded)
            {
                if (go != null && cr.isDeflated)
                {
                    GameObject.DestroyImmediate(go);
                    Debug.Log("Destroying go");
                }

            }
        }

    }

    public class Chunks
    {
        public VolumetricTexture volTex;
        public Vector3 gridSize;
        public Vector3 currentPos = Vector3.zero;
        public Vector3 chunkSize;
        public Vector3 volTexSize;
        public enum StatusType { Idle, Working, Finished};
        public StatusType movingStatus = StatusType.Idle;

        public Chunk[] chunks;
        private string m_data, m_mu;

        public Chunks(VolumetricTexture vt, Vector3 cs, Vector3 vts, Vector3 localPos, string data, string mu)
        {
            volTex = vt;
            chunkSize = cs;
            volTexSize = vts;
            gridSize.x = volTexSize.x / chunkSize.x;
            gridSize.y = volTexSize.y / chunkSize.y;
            gridSize.z = volTexSize.z / chunkSize.z;
//            gridSize += Vector3.one * 2; // Borders
            currentPos = localPos;
            m_data = data;
            m_mu = mu;
            Initialize(data, mu);

        }


        public int getIndex(int x, int y, int z)
        {
            return (int)(x * gridSize.z * gridSize.y + y * gridSize.z + z);
        }

        private Thread thread;

        private void Initialize(string data, string mu)
        {
            volTex.size = volTexSize;
            volTex.InitializeCols();
            volTex.CreateTexture();
            chunks = new Chunk[(int)(gridSize.x * gridSize.y * gridSize.z)];
            Vector3 chunkPos = chunkSize;
            chunkPos.x *= currentPos.x;
            chunkPos.y *= currentPos.y;
            chunkPos.z *= currentPos.z;


            int pos = 0;
            for (int i = 0; i < gridSize.x; i++)
                for (int j = 0; j < gridSize.y; j++)
                    for (int k = 0; k < gridSize.z; k++)
                    {
                        Vector3 localPos = new Vector3((i - 0) * chunkSize.x, (j - 0) * chunkSize.y, (k - 0) * chunkSize.z);

                        chunks[pos] = new Chunk(volTex, chunkSize, localPos + chunkPos, localPos, data, mu);
                        pos++;
                    }


            thread = new Thread(ThreadUpdate);
            thread.IsBackground = true;
            thread.Start();
        }


        public Chunk getChunk(int i, int j, int k)
        {
            if (i < 0 || i >= gridSize.x)
                return null;
            if (j < 0 || j >= gridSize.y)
                return null;
            if (k < 0 || k >= gridSize.z)
                return null;

            return chunks[getIndex(i, j, k)];
        }


        private void MoveChunkThread(System.Object o)
        {
            if (movingStatus != StatusType.Idle)
                return;

            movingStatus = StatusType.Working;
            Vector3 dir = (Vector3)o;
            currentPos += dir;
            Vector3 chunkPos = chunkSize;
            chunkPos.x *= currentPos.x;
            chunkPos.y *= currentPos.y;
            chunkPos.z *= currentPos.z;


            Chunk[] newChunks = new Chunk[chunks.Length];

            for (int i = 0; i < gridSize.x; i++)
                for (int j = 0; j < gridSize.y; j++)
                    for (int k = 0; k < gridSize.z; k++)
                    {
                        Chunk c = getChunk((int)(i + dir.x), (int)(j + dir.y), (int)(k + dir.z));
                        int idx = getIndex(i, j, k);
                        Vector3 localPos = new Vector3((i - 0) * chunkSize.x, (j - 0) * chunkSize.y, (k - 0) * chunkSize.z);
                        if (c != null)
                        {
                            newChunks[idx] = c;
                            c.localPosition = localPos;
                            c.isApplied = false;
                            c.hasChanged = true;
                            //c.Apply();


                        }
                        else
                        {
                            newChunks[idx] = new Chunk(volTex, chunkSize, localPos + chunkPos, localPos, m_data, m_mu);
                        }

                    }
            chunks = newChunks;
            movingStatus = StatusType.Finished;

        }

        private void MoveChunksThreadApply()
        {
            for (int i = 0; i < gridSize.x; i++)
                for (int j = 0; j < gridSize.y; j++)
                    for (int k = 0; k < gridSize.z; k++)
                    {
                        Chunk c = getChunk(i,j,k);
                        if (!c.isApplied)
                            c.Apply();
                    }

        }

        public void MoveChunks(Vector3 dir)
        {
            MoveChunkThread(dir);
            Thread moveThread = new Thread(MoveChunksThreadApply);
            moveThread.Start();
        }


        bool changed = false;
        public bool threadAbort = false;


        public void DestroyThreads()
        {
            threadAbort = true;
            if (thread != null)
                thread.Abort();
        }

        public void ThreadLoop()
        {
            if (chunks != null)
            {
                for (int i = 0; i < gridSize.x - 0; i++)
                    for (int j = 0; j < gridSize.y - 0; j++)
                        for (int k = 0; k < gridSize.z - 0; k++)
                        {
                            int idx = getIndex(i, j, k);
                            if (chunks[idx] != null)
                            {
                                chunks[idx].Update();
                            }
                        }
            }

        }

        public void ThreadUpdate()
        {
            while (!threadAbort)
            {
                ThreadLoop();
                Thread.Sleep(250);
            }
        }

        public bool Update()
        {
//            ThreadLoop();
            changed = false;
            int cnt = 0;
            for (int i = 0; i < chunks.Length; i++)
            {
                if (chunks[i] != null)
                {
                    chunks[i].UpdateNonThreaded();
                    if (chunks[i].hasChanged)
                    {
                        changed = true;
                        chunks[i].hasChanged = false;
                        
                    }
                    if (chunks[i].isApplied)
                        cnt++;
                }
            }

            //if (changed)
            //float l = (gridSize.x - 2) * (gridSize.y - 2) * (gridSize.z - 2);
            //Debug.Log(cnt + " , " + l);
            if (changed)
            {
                volTex.Apply();
                Debug.Log("Apply");
                return true;
            }

            if (movingStatus == StatusType.Finished)
            {
                movingStatus = StatusType.Idle;
                volTex.Apply();
                Debug.Log("Apply Move");
                return true;

            }


            return false;

        }


    }

}