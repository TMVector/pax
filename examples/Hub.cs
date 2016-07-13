/*
Pax : tool support for prototyping packet processors
Nik Sultana, Cambridge University Computer Lab, June 2016

Use of this source code is governed by the Apache 2.0 license; see LICENSE.
*/

using Pax;

#if !LITE
using PacketDotNet;

public partial class Hub : MultiInterface_SimplePacketProcessor {
  override public ForwardingDecision process_packet (int in_port, ref Packet packet)
  {
    // Extract the bytes and call the instance of IAbstract_ByteProcessor
    byte[] bs = packet.Bytes;
    return (process_packet (in_port, ref bs));
  }
}
#endif

public partial class Hub : IAbstract_ByteProcessor {
  public ForwardingDecision process_packet (int in_port, ref byte[] packet)
  {
    return (ForwardingDecision.broadcast(in_port));
  }
}
