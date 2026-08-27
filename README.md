# LuciAgent

> A universal agent layer for discovering, connecting, and communicating with nearby and remote devices.

## The Vision

Modern devices are highly connected, yet device-to-device communication remains fragmented across operating systems, ecosystems, and single-purpose applications.

Sending a file, sharing a message, synchronizing state, or interacting with another nearby device often requires users to install a specific application, create a group, or route data through a third-party service — even when the devices are physically close.

**LuciAgent** aims to provide a universal communication layer between devices.

Instead of every application implementing its own discovery, identity, trust, and transport mechanisms, LuciAgent provides these primitives once and exposes them through an API.

Applications can then build higher-level experiences on top of the agent.

## Multi-Tier Communication

LuciAgent uses a prioritized communication strategy:

1. **Direct P2P** — communicate directly whenever the platform and network allow it.
2. **LAN** — use local network connectivity for high-speed communication.
3. **Internet Coordination / Relay** — fall back to an Internet-based coordinator or relay when direct connectivity is unavailable.

The goal is simple:

> **Use the closest and most direct path available.**

## Four Fundamental Questions

Every LuciAgent node should be able to answer four questions:

### 1. Who am I?

Device identity, capabilities, state, and presence.

### 2. Who can I see?

Discoverable devices across available communication layers.

### 3. Who can see me?

Visibility and presence policies determine what the node exposes.

### 4. Who can communicate with me?

Trust, authorization, and secure session establishment determine who may interact with the node.

These four primitives form the foundation on which higher-level services can be built.

## Architecture

### Agent as Infrastructure

LuciAgent is intentionally designed as an infrastructure layer rather than a single-purpose application.

The agent provides:

- Device identity
- Multi-tier discovery
- Secure handshaking
- Trust and authorization
- Peer-to-peer communication
- LAN communication
- Internet fallback
- Data transfer
- Local policy enforcement
- Temporary data and cache management

Applications can build features such as:

- File sharing
- Messaging
- Media sharing
- Remote control
- Screen sharing
- Video communication
- Device synchronization
- IoT interaction

without implementing the underlying device-to-device communication stack themselves.

## Extensible by Design

LuciAgent defines the communication primitives.

Applications define the experience.

```text
Application
     |
     v
LuciAgent API
     |
     v
+--------------------------+
|        LuciAgent         |
|                          |
| Identity                 |
| Discovery                |
| Trust                    |
| Communication            |
| Policy                   |
+------------+-------------+
             |
             v
      Other Luci Agents