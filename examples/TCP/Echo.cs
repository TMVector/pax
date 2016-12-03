/*
Echo TCP server
Nik Sultana, Cambridge University Computer Lab, December 2016

Use of this source code is governed by the Apache 2.0 license; see LICENSE.
*/


using System;
using System.Net;
using PacketDotNet;
using Pax;
using Pax_TCP;

public class Echo_Server {
  TCPuny tcp = new TCPuny (100, 1024);
  SockID my_sock;
  SockAddr_In my_addr;

  // FIXME use static echo buffer, rather than keep reallocating.
  public Echo_Server (uint port, IPAddress address) {
    my_sock = tcp.socket(Internet_Domain.AF_Inet, Internet_Type.Sock_Stream, Internet_Protocol.TCP).value_exc();
    my_addr = new SockAddr_In(port, address);
  }

  public void start () {
    // NOTE we call "value_exc()" to force the check to see if there's an error result.
    tcp.bind(my_sock, my_addr).value_exc();
    tcp.listen(my_sock).value_exc();

    //FIXME make this multithreaded, using a static thread pool.
    while (true) {
      SockAddr_In client_addr;
      SockID client_sock = tcp.accept(my_sock, out client_addr).value_exc();

      bool ended = false;
      byte[] buf;
      while (!ended) {
        tcp.read (client_sock, out buf, 10/*FIXME const*/); // FIXME check 'read' value to see if connection's been broken.
        tcp.write (client_sock, buf);
        ended = true; // FIXME check 'buf' to see if there's an EOF.
      }

      tcp.close(client_sock).value_exc();
    }
  }
}

// FIXME can run this directly? I think with the current setup it needs to be
//       loaded via Pax?
public class Test_Echo_Server {
  public static void Main() {
    var server = new Echo_Server(7, IPAddress.Parse("192.168.100.100"));
  }
}
