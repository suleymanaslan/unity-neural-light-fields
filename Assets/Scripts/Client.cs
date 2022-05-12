using AsyncIO;
using NetMQ;
using NetMQ.Sockets;
using System;
using System.Collections.Concurrent;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

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
                    if (socket.TrySendFrame(client.u + "," + client.v))
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

    [SerializeField]
    private RawImage display;

    [SerializeField]
    private int resolution;

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
    }

    private void OnDestroy()
    {
        receiver?.Stop();
        NetMQConfig.Cleanup();
    }
}
