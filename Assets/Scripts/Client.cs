using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Receiver
{
    private readonly Thread receiveThread;
    private bool running;
    private Client client;

    public Receiver(Client _client)
    {
        client = _client;
        receiveThread = new Thread((object callback) =>
        {
            TimeSpan timeout = new TimeSpan(0, 0, 5);
            bool imageReceived = false;
            byte[] rawImage;
            using (var socket = new RequestSocket())
            {
                socket.Connect("tcp://localhost:5555");

                while (running)
                {
                    //socket.SendFrameEmpty();
                    //rawImage = socket.ReceiveFrameBytes();
                    float uCoordinate = client.u + client.uOffset;
                    if (uCoordinate > 1) uCoordinate -= 2;
                    if (uCoordinate < -1) uCoordinate += 2;
                    float vCoordinate = client.v + client.vOffset;
                    if (vCoordinate > 1) vCoordinate -= 2;
                    if (vCoordinate < -1) vCoordinate += 2;
                    if (socket.TrySendFrame(uCoordinate + "," + -vCoordinate))
                    {
                        imageReceived = socket.TryReceiveFrameBytes(timeout, out rawImage);
                        if (imageReceived)
                        {
                            ((Action<byte[]>)callback)(rawImage);
                        }
                    }
                }
            }
        });
    }

    public void Start(Action<byte[]> callback)
    {
        running = true;
        receiveThread.Start(callback);
    }

    public void Stop()
    {
        running = false;
        receiveThread.Join();
    }
}

public class Client : MonoBehaviour
{
    private readonly ConcurrentQueue<Action> runOnMainThread = new ConcurrentQueue<Action>();
    private Receiver receiver;
    private Texture2D tex;

    [SerializeField, Range(-1f, 1f)]
    public float u, v;

    [HideInInspector]
    public float uOffset, vOffset;

    [SerializeField]
    private RawImage display;

    [SerializeField]
    private int resolution;

    [SerializeField]
    TextMeshPro textMeshPro; 

    public void Start()
    {
        tex = new Texture2D(resolution, resolution, TextureFormat.RGB24, mipChain: false);
        display.texture = tex;

        ForceDotNet.Force();  // If you have multiple sockets in the following threads
        receiver = new Receiver(this);
        receiver.Start((byte[] rawImage) => runOnMainThread.Enqueue(() =>
        {
            tex.LoadRawTextureData(rawImage);
            tex.Apply(updateMipmaps: false);
        }
        ));
    }

    public void Update()
    {
        if (!runOnMainThread.IsEmpty)
        {
            Action action;
            while (runOnMainThread.TryDequeue(out action))
            {
                action.Invoke();
            }
        }
        textMeshPro.text = u.ToString("F1") + "-" + v.ToString("F1");
    }

    private void OnDestroy()
    {
        receiver?.Stop();
        NetMQConfig.Cleanup();
    }
}
