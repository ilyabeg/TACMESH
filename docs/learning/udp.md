# Learning: UDP

**UDP - User Datagram Protocol** is a fast *connectionless* communication protocol used to send data packets called 'Datagrams' with minimal delay. It is widely used for things that need and prefer fast communication over a safe one.
Key features of UDP:

- **Connectionless**: It sends data directly to a target device without establishing a connection or performing a handshake first (Unlike TCP which needs a safe and valid connection to be established before starting the communication).

- **Doesn't Guarantee Arrival**: It does not track packets, verify arrival, guarentee delivery or re-send lost data (matter of fact, it does not know if data was lost to begin with).

- **Very Fast and has Light Weight Header**: Skips connection setup and error checking recovery methods (Gets straight to the point), and uses a fixed 8 byte header of the *source port*, *destination port*, *packet length* and a *checksum* (used to detect data corruption, such as bit flips, happened on the way to the destination).

The down side of UDP is that it is not reliable as TCP, meaning apps need to handle error checking and re-sending themselves.

## Why Choose UDP Over TCP?

In my project, I use UDP for my network communication because establishing a connection (using TCP) between one user and all the others in a mobile Ad-Hoc Network where all of the connections break and reconnect constantly will be next to impossible. I use UDP for fast communication and handle the reliability myself by having the ground forces hold messages that can't yet reach their destination in a buffer, until a suitable connection was established.

## what happens to a datagram sent to a port nobody listens on?

When a datagram is sent to a port that has no active listener, the destination's operating system takes over:

* The OS drops the packet because it realizes that no one is bound to that specific port.
* Then, the OS creates an ICMP (Internet Control Message Protocol) packet that says "Port Unreacheable" and sends it back to the source IP address.

## the difference between bind and send

You can think of the difference between *Bindind* and *Sending* in terms of UDP communication sort of as **claiming a specific mailing address** and **dropping a letter into a mailbox**.

* **Bind** - Means to assign a local port and IP address to a socket. When binding a UDP Socket, the Operating System allocates it with a random address that is made up of an IP and Port. Bind tells the OS "*Listen for incoming packets pn this specific local IP and Port number*" (This is your address so you can receive mail in the mailbox).

* **Send** - Means to transmit data packets to a destination IP and port. When *sending* you have to tell where to transmit the data to (What address is the mailbox that you wand to send your mail?).