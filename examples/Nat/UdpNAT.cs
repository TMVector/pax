/*
Pax : tool support for prototyping packet processors
Jonny Shipton, Cambridge University Computer Lab, July 2016

Use of this source code is governed by the Apache 2.0 license; see LICENSE.
*/

using PacketDotNet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Net;

namespace Pax.Examples.Nat
{
  /// <summary>
  /// A class that handles UDP packets and performs Port-Address Translation.
  /// </summary>
  /// <remarks>
  /// This class is almost identical to the <see cref="TcpNAT"/> class because both protocols use 16-bit port numbers. The
  /// CreateMasqueradeNode methods *are* identical. <!--FIXME perhaps use a common NATBase<TPacket,NodeWithPort,TEncaps> ancestor?-->
  /// </remarks>
  public sealed class UdpNAT : NATBase<UdpPacket, NodeWithPort, UdpPacketEncapsulation>
  {
    /// <summary>
    /// The start of the range of UDP ports to use (inclusive).
    /// </summary>
    private readonly ushort StartPort;

    /// <summary>
    /// The end of the range of UDP ports to use (inclusive).
    /// </summary>
    private readonly ushort EndPort;

    private ushort nextPort;
    private object nextPortLock = new object();

    /// <summary>
    /// Creates a NAT for handling UDP packets.
    /// </summary>
    /// <param name="outsideFacingAddress">The public IP address of the NAT</param>
    /// <param name="nextOutsideHopMacAddress">The MAC address of the next hop on the outside-facing port.</param>
    /// <param name="inactivityTimeout">The time that should elapse before a UDP connection with no activity is removed.</param>
    /// <param name="startPort">The start of the range of UDP ports to use (inclusive).</param>
    /// <param name="endPort">The end of the range of UDP ports to use (inclusive).</param>
    public UdpNAT(IPAddress outsideFacingIPAddress, PhysicalAddress nextOutsideHopMacAddress,
      TimeSpan inactivityTimeout, ushort startPort, ushort endPort)
      : base(outsideFacingIPAddress, nextOutsideHopMacAddress, inactivityTimeout)
    {
      StartPort = startPort;
      EndPort = endPort;

      nextPort = startPort;
    }

    protected override NodeWithPort CreateMasqueradeNode(IPAddress ipAddress, int interfaceNumber, PhysicalAddress macAddress)
    {
      // Get a free port
      ushort port;
      lock (nextPortLock)
      {
        port = nextPort;

        // FIXME naive port assignment: when nextPort wraps around, could reassign ports that are already in use
        // Note that this is fairly unlikely because the key is made up of the source port, destination port, and source address.
        nextPort++;
        if (nextPort < StartPort || nextPort > EndPort)
          nextPort = StartPort;
      }

      return new NodeWithPort(ipAddress, port, interfaceNumber, macAddress);
    }

    protected override ITransportState<UdpPacket> GetInitialStateForNewConnection()
    {
      return NoTransportState<UdpPacket>.Instance;
    }
  }
}
