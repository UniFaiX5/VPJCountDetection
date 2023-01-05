using RtspClientSharpCore.RawFrames;
using System;

namespace RtspClientSharpCore.MediaParsers
{
    interface IMediaPayloadParser
    {
        Action<RawFrame> FrameGenerated { get; set; }

        void Parse(TimeSpan timeOffset, ArraySegment<byte> byteSegment, bool markerBit);

        void ResetState();
    }
}