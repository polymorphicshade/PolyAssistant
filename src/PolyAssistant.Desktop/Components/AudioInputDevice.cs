using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NAudio.CoreAudioApi;
using NAudio.Wave;
using PolyAssistant.Desktop.Components.Interfaces;
using System.IO;

namespace PolyAssistant.Desktop.Components;

public sealed class AudioInputDevice(MMDevice rawDevice, ILogger? logger = null) : IAudioInputDevice
{
    private readonly List<byte> _recordBuffer = [];
    private readonly Lock _recordBufferLock = new();
    private readonly List<byte> _pollBuffer = [];
    private readonly Lock _pollBufferLock = new();

    // TODO: make nullable/temporary (to allow sample/channels to be specified by parameters
    private readonly WasapiCapture _wasapiCapture = new(rawDevice)
    {
        WaveFormat = new WaveFormat(44100, 2)
    };

    private bool _pollFlag;

    public MMDevice RawDevice { get; } = rawDevice;

    public ILogger Logger { get; } = logger ?? NullLogger<AudioInputDevice>.Instance;

    public string Id => RawDevice.ID;

    public string Name => RawDevice.FriendlyName;

    public bool IsListening { get; private set; }

    public bool IsRecording { get; private set; }

    public bool IsPolling { get; private set; }

    public event EventHandler<WaveInEventArgs> DataAvailable
    {
        add => _wasapiCapture.DataAvailable += value;
        remove => _wasapiCapture.DataAvailable -= value;
    }

    public event EventHandler<byte[]> DataPolled = delegate { };

    public void StartListening()
    {
        if (IsListening)
        {
            return;
        }

        _wasapiCapture.DataAvailable += OnDataAvailable;
        _wasapiCapture.RecordingStopped += OnListeningStopped;

        var count = 0;
        Exception? lastException = null;

        while (count < 3)
        {
            try
            {
                _wasapiCapture.StartRecording();
                break;
            }
            catch (Exception ex)
            {
                lastException = ex;
                Thread.Sleep(1000);
            }

            count++;
        }

        if (lastException != null)
        {
            throw lastException;
        }

        IsListening = true;

        Logger.LogInformation("Listening started for device: {name}", Name);
    }

    public byte[] StopListening()
    {
        if (!IsListening)
        {
            return [];
        }

        _wasapiCapture.StopRecording();

        try
        {
            var result = StopRecording();

            IsListening = false;

            Logger.LogInformation("Listening stopped for device: {name}", Name);

            return result;
        }
        finally
        {
            _wasapiCapture.DataAvailable -= OnDataAvailable;
            _wasapiCapture.RecordingStopped -= OnListeningStopped;
        }
    }

    public void StartRecording(bool throwOnNotListening = false)
    {
        if (IsRecording)
        {
            return;
        }

        if (!IsListening)
        {
            if (throwOnNotListening)
            {
                throw new InvalidOperationException("Recorder isn't listening, call StartListening() first");
            }

            StartListening();
        }

        ClearRecordBufferData();

        IsRecording = true;

        Logger.LogInformation("Recording started for device: {name}", Name);
    }

    public byte[] StopRecording()
    {
        if (!IsRecording)
        {
            return [];
        }

        var result = GetRecordBufferData();

        ClearRecordBufferData();

        IsRecording = false;

        Logger.LogInformation("Recording stopped for device: {name}", Name);

        return result;
    }

    public Task<byte[]> RecordAsync(TimeSpan timeSpan, bool throwOnNotListening = false, CancellationToken cancellationToken = default)
    {
        if (!IsListening)
        {
            if (throwOnNotListening)
            {
                throw new InvalidOperationException("Recorder isn't listening, call StartListening() first");
            }

            StartListening();
        }

        return Task.Run(async () =>
        {
            // start
            StartRecording();

            try
            {
                // wait
                await Task.Delay(timeSpan, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // ignored
            }

            // stop
            return StopRecording();
        }, cancellationToken);
    }

    public Task StartPolling(TimeSpan interval, bool throwOnNotListening = false, CancellationToken cancellationToken = default)
    {
        if (IsPolling)
        {
            throw new InvalidOperationException("Already polling");
        }

        if (!IsListening)
        {
            if (throwOnNotListening)
            {
                throw new InvalidOperationException("Recorder isn't listening, call StartListening() first");
            }

            StartListening();
        }

        IsPolling = true;

        var task = Task.Run(async () =>
        {
            while (IsPolling && !cancellationToken.IsCancellationRequested)
            {
                // on -> wait -> off
                _pollFlag = true;
                await Task.Delay(interval, cancellationToken);
                _pollFlag = false;

                var data = GetPollBufferData();
                ClearPollBufferData();

                // event
                DataPolled(this, data);
            }
        }, cancellationToken);

        return task;
    }

    public byte[] StopPolling()
    {
        var result = GetPollBufferData();

        ClearPollBufferData();

        IsPolling = false;

        return result;
    }

    public Task<byte[]> PollAsync(TimeSpan interval, TimeSpan timeSpan, bool throwOnNotListening = false, CancellationToken cancellationToken = default)
    {
        if (!IsListening)
        {
            if (throwOnNotListening)
            {
                throw new InvalidOperationException("Recorder isn't listening, call StartListening() first");
            }

            StartListening();
        }

        return Task.Run(async () =>
        {
            // start
            _ = StartPolling(interval, cancellationToken: cancellationToken);

            try
            {
                // wait
                await Task.Delay(timeSpan, cancellationToken);
            }
            catch (TaskCanceledException)
            {
                // ignored
            }

            // stop
            return StopPolling();
        }, cancellationToken);
    }

    public void StopAll()
    {
        StopRecording();
        StopPolling();
        StopListening();
    }

    public void Dispose()
    {
        StopRecording();
        StopPolling();
        StopListening();

        _wasapiCapture.Dispose();
    }

    private void ClearRecordBufferData()
    {
        lock (_recordBufferLock)
        {
            _recordBuffer.Clear();
        }
    }

    private byte[] GetRecordBufferData()
    {
        byte[] data;

        lock (_recordBufferLock)
        {
            data = _recordBuffer.ToArray();
        }

        using var memoryStream = new MemoryStream();
        using var writer = new WaveFileWriter(memoryStream, _wasapiCapture.WaveFormat);

        writer.Write(data, 0, data.Length);
        writer.Flush();

        return memoryStream.ToArray();
    }

    private void ClearPollBufferData()
    {
        lock (_pollBufferLock)
        {
            _pollBuffer.Clear();
        }
    }

    private byte[] GetPollBufferData()
    {
        byte[] data;

        lock (_pollBufferLock)
        {
            data = _pollBuffer.ToArray();
        }

        using var memoryStream = new MemoryStream();
        using var writer = new WaveFileWriter(memoryStream, _wasapiCapture.WaveFormat);

        writer.Write(data, 0, data.Length);
        writer.Flush();

        return memoryStream.ToArray();
    }

    private void OnDataAvailable(object? sender, WaveInEventArgs e)
    {
        // TODO: make more efficient
        var data = e.Buffer.Take(e.BytesRecorded).ToArray();

        if (IsRecording)
        {
            lock (_recordBufferLock)
            {
                _recordBuffer.AddRange(data);
            }
        }

        if (IsPolling && _pollFlag)
        {
            lock (_pollBufferLock)
            {
                _pollBuffer.AddRange(data);
            }
        }
    }

    private void OnListeningStopped(object? sender, StoppedEventArgs e)
    {
        IsListening = false;

        if (e.Exception != null)
        {
            // TODO: handle (probably with an event)
            //
            //
        }
    }
}