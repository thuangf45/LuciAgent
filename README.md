# LuciAgent

> A universal agent layer for discovering, connecting, and communicating with nearby and remote devices.

**Status: 🌱 Idea stage, implementation underway.** The vision and architecture below are settled; the codebase is just getting started — early project structure is in place and active development has begun, but no functional milestone exists yet.

## The Problem

You're standing thirty centimeters from your own computer, holding your own phone, trying to send yourself a photo, a video, a file, or a secret key.

And somehow it's *hard*.

Small files go through a chat app, fine. But a zip file, a large video, anything past some arbitrary size — blocked by platform policy. So you end up routing a file between two devices sitting on the same desk through servers that might be thousands of kilometers away, just to have it come back down to a machine an arm's length away. Your network is weak, but if the two devices could just talk directly, the transfer could happen at gigabit speed instead of crawling through someone else's pipe. A secret key you want to hand off privately between your own two devices instead has to leave the room first.

This happens constantly, to everyone, including people who work in tech and *know* better solutions should exist. And yet almost nobody builds for this specific case: two devices, physically close, that should just be able to talk to each other directly — no app install, no group chat, no third-party relay, no ceremony.

There's also a related, forward-looking idea worth naming: today, if you see something interesting on a friend's screen, you share a link and hope they scroll to the right spot. In a more connected future, you might instead share the *screen itself* — like a lightweight TeamViewer session — and interact with it directly, as if you were holding the device in your own hands. Simple, obvious ideas that somehow nobody has made effortless yet.

**LuciAgent** starts from this itch: device-to-device communication should default to the shortest, fastest, most direct path available — and it should be a layer other applications can build on, not something every app reinvents badly.

### "Why not just build a small app that hosts a local web page and uploads the file?"

That works — for exactly one feature, built by hand, every time. It solves file transfer today, but tomorrow's need is messaging, or clipboard sync, or streaming a live sensor value, or letting one device remote-control another. Each of those becomes its own bespoke local server, its own ad-hoc discovery, its own security model. None of it composes with the others. LuciAgent instead treats file transfer, messaging, and remote control as different *applications* built on the same identity, discovery, trust, and transport primitives — so the hard part (finding the device, trusting it, opening a fast channel) is solved once, not once per feature.

### "Why not just use Bluetooth / AirDrop-style direct transfer?"

Direct radio transfer is great at the one thing it was designed for, but it doesn't generalize. Try sending a short text message *together with* an image over classic Bluetooth in a way another app can consistently receive and interpret — it's surprisingly painful, because the protocol wasn't built around composable content types, only point-to-point data blobs. Extending it to a new capability (say, "share a live clipboard" or "hand off an active session") usually means starting over. LuciAgent's aim is to make communication capabilities *composable*: file, text, presence, and control all speak through the same agent, so new features are additions, not new stacks.

### The experience this makes possible

Imagine opening a photo on your phone, swiping in one light gesture, and — in under a second — that photo is already on your computer screen. No app switch, no "share to," no waiting for an upload bar. Nothing about this requires new physics; every ingredient (fast discovery, direct transport, trust already established between your own devices) already exists somewhere. What's missing is the layer that ties them together into something instant and effortless. That's the gap LuciAgent is meant to fill: not a single new trick, but the fast, reliable orchestration of capabilities that, combined, feel like magic.

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
```

## Where This Stands Today

This project is moving from idea into early implementation. The current repository already has its initial structure in place, with active development starting on:

- `LuciAgent.Server` — background service + web host for control. Structure is in place; core functionality is being built out.
- `LuciAgent.Client.Core` — shared client logic, forming the common groundwork for future clients.
- `LuciAgent.Client.Console` — console client, used for desktop debugging while the core is developed.
- A MAUI-based cross-platform client is planned as a follow-up once the core and console client are solid.

Consider this README a living sketch of the direction — it will keep evolving in step with the codebase as implementation progresses.