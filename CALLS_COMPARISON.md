# Open-source stacks for group calls with multi-person screen sharing

Research date: 2026-11-16, updated 2026-11-17.

Requirements:

1. Group audio and video calls with screen sharing, where several people can share at once.
2. The call system is a separate service that the Chatney backend drives through an API, not something embedded in the backend.
3. Web clients connect over WebSocket and WebRTC. Future non-web clients (mobile and desktop apps) must be able to connect too, with media over UDP and a TCP path when UDP is blocked.

Recommendation: LiveKit, with Janus as the alternative. mediasoup does not meet the separate-service requirement out of the box, because it is a library with no network API. No independent 2026 benchmark was found, so the performance figures below come from vendors or rules of thumb.

## Projects compared

| Project | Website | Source |
|---|---|---|
| LiveKit | [livekit.com](https://livekit.com) (the company site leads with AI agents); the video SFU is documented under [WebRTC Transport](https://docs.livekit.io/transport/) | [github.com/livekit/livekit](https://github.com/livekit/livekit) |
| mediasoup | [mediasoup.org](https://mediasoup.org) | [github.com/versatica/mediasoup](https://github.com/versatica/mediasoup) |
| Janus | [janus.conf.meetecho.com](https://janus.conf.meetecho.com) | [github.com/meetecho/janus-gateway](https://github.com/meetecho/janus-gateway) |
| Jitsi Videobridge | [jitsi.org/jitsi-videobridge](https://jitsi.org/jitsi-videobridge/) | [github.com/jitsi/jitsi-videobridge](https://github.com/jitsi/jitsi-videobridge) |
| Asterisk | [asterisk.org](https://www.asterisk.org) | [github.com/asterisk/asterisk](https://github.com/asterisk/asterisk) |

## Architecture

A selective forwarding unit (SFU) over WebRTC is the only practical architecture for group audio/video with several simultaneous screen shares:

- **P2P mesh:** every participant uploads to every other one, so it breaks past about four people, sooner with several screen shares.
- **MCU (server-side mixing):** it transcodes everything, costs far more CPU, and cannot give each viewer their own layout of several screens.
- **Media over QUIC:** the IETF transport spec is still a draft (revision 18 in May 2026), and no production-grade open-source conferencing stack built on it was found.

The four dedicated candidates (LiveKit, mediasoup, Janus, Jitsi Videobridge) are WebRTC SFUs, and all four support several simultaneous screen shares. Asterisk is included for comparison: it is a telephony server that forwards video like an SFU but mixes audio on the server.

## Summary

Ratings are a judgment call on a 1–5 scale.

| | LiveKit | mediasoup | Janus | Jitsi Videobridge | Asterisk |
|---|---|---|---|---|---|
| What it is | Complete platform (SFU, signaling, TURN, SDKs) | SFU library, you build the rest | General-purpose WebRTC server with plugins | SFU inside the Jitsi stack | Telephony PBX with a video SFU mode |
| Language / license | Go / Apache-2.0 | C++ with Node or Rust API / ISC | C / GPLv3 | Kotlin/Java / Apache-2.0 | C / GPLv2 |
| Standalone service with a backend API | Yes | No, you build it | Yes | Partly | Yes |
| Web and native client connectivity | 5 | 3 | 3 | 4 | 4 |
| Maturity | 4 | 4 | 5 | 5 | 5 for telephony, 3 for video |
| Technological advance | 5 | 4 | 3 | 4 | 2 |
| Protocol support | 5 | 3 | 5 | 4 | 4 |
| Performance | 4 | 5 | 4 | 3 | 2 |
| Stability | 4 | 4 | 5 | 4 | 4 |
| Integration effort for Chatney | Low | High | Medium–high | High | Medium–high |

## Feature comparison

✅ present, 🔴 absent. "Present" means shipped by the project itself, not achievable with custom code.

| Feature | LiveKit | mediasoup | Janus | Jitsi Videobridge | Asterisk |
|---|---|---|---|---|---|
| **Media** | | | | | |
| Video forwarding (SFU) | ✅ | ✅ | ✅ | ✅ | ✅ |
| Several simultaneous screen shares | ✅ | ✅ | ✅ | ✅ | ✅ ¹ |
| VP8, VP9 and H.264 codecs | ✅ | ✅ | ✅ | ✅ | ✅ |
| Simulcast | ✅ | ✅ | ✅ | ✅ | 🔴 |
| Scalable video coding (SVC) | ✅ | ✅ | ✅ | ✅ | 🔴 |
| AV1 | ✅ | ✅ | ✅ ² | ✅ | 🔴 ³ |
| Automatic per-viewer quality switching by bandwidth | ✅ | ✅ | 🔴 ⁴ | ✅ | 🔴 |
| Viewer chooses which streams to receive | ✅ | ✅ | ✅ | ✅ | 🔴 |
| Active speaker detection | ✅ | ✅ | ✅ | ✅ | ✅ |
| End-to-end encryption shipped in the SDK | ✅ | 🔴 | 🔴 ⁵ | ✅ ⁶ | 🔴 |
| Data channels | ✅ | ✅ | ✅ | ✅ | 🔴 ³ |
| **Control plane** | | | | | |
| Built-in signaling | ✅ | 🔴 | ✅ | ✅ | ✅ |
| Backend control API over HTTP | ✅ | 🔴 | ✅ | 🔴 ²¹ | ✅ |
| Events pushed to the backend | ✅ | 🔴 | ✅ | 🔴 | ✅ |
| Backend-issued access tokens for clients | ✅ | 🔴 | ✅ | ✅ | 🔴 |
| **Room control from the backend** | | | | | |
| Create and delete rooms through the API | ✅ | 🔴 | ✅ | 🔴 ¹⁵ | ✅ |
| Rooms created automatically on first join | ✅ | 🔴 | 🔴 | ✅ | ✅ ¹⁶ |
| Empty rooms close automatically | ✅ | 🔴 | 🔴 | ✅ | ✅ ¹⁶ |
| List rooms and their participants | ✅ | 🔴 | ✅ | ✅ ¹⁷ | ✅ |
| Remove a participant | ✅ | 🔴 | ✅ | ✅ ¹⁸ | ✅ |
| Mute a participant from the server | ✅ | 🔴 | ✅ | 🔴 ¹⁸ | ✅ |
| Participant limit per room | ✅ | 🔴 | ✅ ¹⁹ | ✅ | ✅ ¹⁶ |
| Per-participant permissions (publish, subscribe, screen share) | ✅ | 🔴 | 🔴 ²⁰ | 🔴 ²⁰ | 🔴 ²⁰ |
| **Connectivity** | | | | | |
| WebSocket support (signaling) | ✅ | 🔴 | ✅ | ✅ | ✅ |
| WebRTC support (browser media) | ✅ | ✅ | ✅ | ✅ | ✅ |
| Media over UDP | ✅ | ✅ | ✅ | ✅ | ✅ |
| Direct media over TCP (ICE-TCP) | ✅ | ✅ | 🔴 ¹⁰ | 🔴 ¹¹ | 🔴 ¹¹ |
| Media over TCP/TLS through a TURN relay | ✅ | ✅ ¹² | ✅ ¹² | ✅ ¹² | ✅ ¹² |
| Plain RTP media without WebRTC | 🔴 ¹³ | ✅ | ✅ | 🔴 ¹³ | ✅ |
| **Clients** | | | | | |
| First-party web client library | ✅ | ✅ | ✅ | ✅ | 🔴 |
| First-party iOS and Android SDKs | ✅ | 🔴 | 🔴 | ✅ | 🔴 |
| First-party native desktop SDK | ✅ | ✅ | 🔴 | 🔴 ¹⁴ | 🔴 ¹⁴ |
| Ready-made call UI | ✅ | 🔴 | 🔴 | ✅ | 🔴 |
| **Operations** | | | | | |
| Built-in TURN server | ✅ | 🔴 | 🔴 | 🔴 | 🔴 |
| Recording | ✅ | 🔴 | ✅ | ✅ | ✅ ⁷ |
| SIP / telephony bridge | ✅ | 🔴 | ✅ | ✅ | ✅ |
| Built-in multi-node clustering | ✅ | 🔴 | 🔴 | ✅ | 🔴 |
| One room spanning several servers | 🔴 ⁸ | 🔴 ⁹ | 🔴 | ✅ | 🔴 |

1. Asterisk forwards every video stream to every participant; a 2022 forum report describes problems adding streams mid-conference.
2. Janus 1.2.0 (August 2023) added VP9 and AV1 simulcast and fixed AV1 SVC.
3. Asterisk's built-in video codecs are H.264, H.265, VP8 and VP9; AV1 is not among them. Asterisk has no WebRTC data channel support.
4. Janus's bandwidth-estimation work is still an open draft pull request; viewers switch layers by explicit request.
5. Janus can pass through frames the client encrypted, but the encryption itself is custom client code.
6. Jitsi's E2EE works only with VP8.
7. Audio only: ConfBridge records through MixMonitor, which handles audio only.
8. Available in LiveKit Cloud, not in the self-hosted server.
9. mediasoup provides the building block (router-to-router piping), but not the clustering.
10. Janus has an `ice_tcp` option, but its own source labels ICE-TCP "currently broken", and the sample config warns it may cause problems without ICE Lite.
11. Asterisk's RTP stack discards every non-UDP ICE candidate. Jitsi Videobridge configures ICE over UDP only; clients without UDP go through TURN.
12. Needs an external TURN server such as coturn; LiveKit ships its own.
13. LiveKit and Jitsi accept non-WebRTC clients only as audio through their SIP bridges.
14. Jitsi's desktop app is Electron on the web library. Asterisk has no SDK of its own, but any standard SIP library works.
15. Jitsi inverts this: when the first user joins, it calls a REST endpoint on your backend, which approves or rejects the room and can set a duration, participant limit and password.
16. Applies to Asterisk's ConfBridge application: it creates a conference when the first caller joins, ends it when the last one leaves, and limits size with `max_members`. Bridges created through ARI are opened, limited and closed by your own code.
17. Through the `muc_census` and `muc_size` Prosody modules shipped with Jitsi. They are unauthenticated by default and must stay on an internal network.
18. Jitsi ships a Prosody module (`muc_kick_participant`) with an authenticated HTTP endpoint for removing a participant. It has no HTTP endpoint for muting; muting is done by moderators inside the call.
19. Janus limits concurrent publishers per room, not total participants.
20. Coarser controls only: Janus allow-lists and room PINs, a Jitsi moderator role, Asterisk user profiles.
21. Jitsi has only narrow HTTP endpoints through Prosody modules (listing rooms, removing a participant); there is no HTTP call to create, delete or mute.

mediasoup has no room concept of its own: its unit is a router, and rooms, limits and moderation are code you write around it.

Checked against primary sources on the research date: mediasoup's AV1 support, the Janus bandwidth-estimation status, the absence of simulcast in Asterisk's published plans, the Asterisk 24.0.0 changelog (no video, WebRTC, simulcast or data channel changes), LiveKit's room API, the Janus VideoRoom requests, Asterisk's ARI bridge operations and the Jitsi reservation system. Re-checked on 2026-11-18 against the projects' current source code: Asterisk's codecs, data channels, ICE-TCP, recording and ConfBridge lifecycle; Janus's ICE-TCP, AV1 SVC and room lifecycle (joining an unknown room fails, and empty rooms stay until destroyed); Jitsi's ICE-TCP and its Prosody HTTP modules. The rest of the table comes from the sources listed below and general knowledge of the projects.

## Protocol openness and lock-in

Media is standard WebRTC in all five. What differs is the signaling protocol a client must speak to join a call, which decides how tied the clients are to one server:

| | Client signaling protocol | Open specification | Can a generic client join? |
|---|---|---|---|
| LiveKit | LiveKit's own protocol: protobuf messages over WebSocket | Yes, Apache-2.0 [protobuf definitions](https://github.com/livekit/protocol), but defined by LiveKit only | No, only LiveKit SDKs; publishing-only clients can use the WHIP standard |
| mediasoup | None; you define it | Your own | Your own clients only |
| Janus | Janus's own JSON API | Yes, documented, defined by Meetecho only | No, only clients written against the Janus API; WHIP/WHEP through companion projects |
| Jitsi | XMPP with Jingle and COLIBRI2 | XMPP and Jingle are IETF/XSF standards; COLIBRI2 is Jitsi's | In practice only `lib-jitsi-meet` or the Jitsi SDKs |
| Asterisk | SIP | IETF standard | Yes, any SIP client or softphone |

LiveKit's protocol is open source rather than proprietary in the licensing sense, but it is not a standard: switching servers later means replacing the client SDKs. Janus and Jitsi carry the same kind of lock-in. Only Asterisk lets standard clients join without a vendor SDK.

On the AI focus: LiveKit's company site and marketing now lead with voice and AI agents, but the agents run on top of the same WebRTC SFU. The server ([v1.13.11, October 2026](https://github.com/livekit/livekit/releases)), the web, iOS and Android SDKs and the React components are all Apache-2.0 and had commits this week. The real risk is a shift in priorities toward agent features and LiveKit Cloud, not the disappearance of the video stack.

## Separation from the backend

In all five cases media flows directly between clients and the media server and never passes through the Chatney backend. They differ in how the backend controls the media server:

| | Backend to media server | Media server to backend | How a client is authorized |
|---|---|---|---|
| LiveKit | HTTP API (Twirp, JSON or protobuf): create/delete rooms, list/remove participants, mute tracks | Signed webhooks: room started/finished, participant joined/left, track published | Backend signs a short-lived JWT per user and room with permissions inside |
| Janus | Janus API and Admin API over HTTP, WebSocket, RabbitMQ or MQTT: create/destroy rooms, list/kick participants | Event handler plugins push events over HTTP, WebSocket, RabbitMQ or MQTT | Token auth and per-room allow-lists managed by the backend |
| Jitsi | Mostly XMPP. Prosody modules add narrow HTTP endpoints (list rooms, room size, remove a participant); the bridge's REST interface covers health and statistics | No shipped webhook module; the reservation system calls the backend only when a room is created or ends | JWT room tokens validated by Prosody |
| mediasoup | No network API; it runs inside a Node.js or Rust process you write | Whatever you implement | Whatever you implement |
| Asterisk | ARI, a REST API over HTTP: create bridges, add/remove/mute channels, record. AMI over TCP as the older alternative | ARI events over WebSocket, AMI events over TCP | SIP credentials per user, which the backend must provision |

- **Asterisk** meets the requirement through ARI, but every Chatney user needs a SIP identity, and clients register over SIP-over-WebSocket with a third-party library.
- **LiveKit** fits the requirement most directly: the backend needs only an HTTP client, a JWT signer and a webhook endpoint.
- **Janus** fits too, with a lower-level API: the backend manages sessions and plugin handles rather than calling room-level endpoints.
- **Jitsi** is a separate system, but its control plane is XMPP, so the backend would need an XMPP integration or custom Prosody modules.
- **mediasoup** becomes a separate system only after you build one: a sidecar service with an API of your own design, plus signaling, in a second language.

## Client connectivity: web and native

WebRTC is not browser-only. Mobile and desktop apps embed a WebRTC library and use the same connection path as the web client: signaling over WebSocket, media over UDP, and TCP or TLS as a fallback when UDP is blocked. Media over TCP is always the fallback, because retransmission delays hurt real-time audio and video. Asterisk is the one candidate where a native client can skip WebRTC entirely and connect as a standard SIP client.

| | Signaling transports | Media transports | Path for mobile and desktop apps |
|---|---|---|---|
| LiveKit | WebSocket | WebRTC over UDP, direct TCP fallback, built-in TURN | First-party SDKs: Swift (iOS, macOS), Android, Flutter (mobile, Windows, macOS, Linux), React Native, C++, Rust, Unity |
| mediasoup | Whatever you build | WebRTC over UDP and TCP, plain RTP over UDP | First-party C++ client library; iOS, Android and Flutter wrappers are community-maintained; your signaling must be reimplemented per platform |
| Janus | HTTP, WebSocket, RabbitMQ, MQTT | WebRTC over UDP (its ICE-TCP option is labeled broken), TCP only through a TURN relay, plain RTP through plugins | No first-party SDKs: apps embed a WebRTC library and implement the Janus API themselves |
| Jitsi | XMPP over WebSocket or HTTP | WebRTC over UDP, TCP only through a TURN relay | First-party iOS, Android, React Native and Flutter SDKs that embed the full Jitsi Meet UI; desktop through Electron |
| Asterisk | SIP over UDP, TCP, TLS and WebSocket | Plain RTP or SRTP over UDP for SIP clients, WebRTC over UDP for browsers, TCP only through a TURN relay | Any standard SIP library or softphone; no first-party SDK, and the web client needs a third-party SIP library |

- **LiveKit** covers the requirement with the least work: the same room, token and API serve web, mobile and desktop, and every platform has a maintained SDK.
- **Asterisk** has the broadest transport list and works with existing SIP clients, which matters if non-WebRTC or telephony clients are expected. Its WebSocket support covers SIP signaling; the separate WebSocket media channel (available since 20.16, 22.6 and 23.0) carries audio for server-side applications, not client calls.
- **Jitsi** has mobile SDKs, but they bring the Jitsi Meet interface with them, which fits poorly with a custom app UI.
- **mediasoup and Janus** leave native clients to you. Any of the five can also be used from an Electron or Tauri desktop app through its web library.

## Maturity

- **LiveKit:** first released in 2021 and now on the 1.13.x line with frequent releases. It has a venture-backed company behind it and the largest SDK ecosystem (JS, React, Swift, Kotlin, Flutter, React Native, Rust). OpenVidu 3 is built on top of it.
- **mediasoup:** v3 has been the stable line since 2019, and 3.20.0 shipped in 2026 with a new built-in SCTP stack. It has a small core team and wide embedded use, including in OpenVidu.
- **Janus:** developed by Meetecho since 2014, with multistream support since the 1.x line. It has the longest track record but is largely driven by one company and one lead author.
- **Jitsi Videobridge:** in production since 2013, owned by 8x8, and runs meet.jit.si and their commercial service.
- **Asterisk:** the oldest project here (1999), maintained by Sangoma. The version evaluated is 24, the long-term-support line released on 2026-11-05 and supported until 2031. Its maturity is in telephony; the video SFU mode arrived in version 15 and is a side feature, and the 24.0.0 changelog contains no video or WebRTC changes.

## Technological advance

- **LiveKit:** simulcast, VP9 and AV1 SVC, automatic pausing of unwatched layers, per-subscriber quality adaptation, and end-to-end encryption. E2EE is available when self-hosted.
- **mediasoup:** simulcast and SVC with fine-grained per-consumer layer control. E2EE and any higher-level behaviour are up to your own code.
- **Janus:** simulcast and SVC for VP9 and AV1; version 1.2.0 (August 2023) added VP9/AV1 simulcast and fixed AV1 SVC. One forum report describes AV1 failing in a WHIP/WHEP setup, so test AV1 before relying on it.
- **Jitsi:** AV1 and VP9 in full SVC mode by default, with receiver-driven quality constraints. E2EE currently works only with VP8.
- **Asterisk:** forwards video but mixes audio on the server, which rules out end-to-end encryption. Simulcast and SVC were only ever a design proposal, so every viewer gets each stream at full bitrate; bandwidth feedback (REMB, transport-cc) is relayed to senders instead.

## Protocol support

All four speak standard WebRTC (ICE, DTLS-SRTP, data channels) with Opus, VP8, VP9, H.264 and AV1. They differ around the edges:

| | Signaling | TURN | Other ingress/egress |
|---|---|---|---|
| LiveKit | Own WebSocket protocol, handled by the SDKs | Built in | WHIP and RTMP ingress, SIP, recording and RTMP/HLS egress (separate services) |
| mediasoup | None, you write it | External (coturn) | Plain RTP in/out for ffmpeg or GStreamer, router-to-router piping |
| Janus | Janus API over HTTP, WebSocket, RabbitMQ or MQTT | External (coturn) | SIP, RTSP/RTP streaming, audio mixing, RTP forwarding, WHIP/WHEP via companion projects |
| Jitsi | XMPP/Jingle plus COLIBRI2 | External (coturn) | SIP via Jigasi, recording and RTMP via Jibri |
| Asterisk | SIP over WebSocket | External (coturn) | Native SIP trunks and PSTN, the widest telephony reach of the five |

## Performance

- **mediasoup:** generally regarded as the most efficient per stream. Each worker is single-threaded on one core, with a rule of thumb of roughly 500 consumers per worker; spreading a room across workers is your job.
- **LiveKit:** its own benchmark on a 16-core machine reports a 150-participant video room at 720p simulcast at about 85% CPU. That figure was only seen through a third-party write-up, not the official table. A room must fit on one node when self-hosted.
- **Janus:** in the same class as LiveKit, but scaling a room beyond one instance is manual.
- **Jitsi:** runs on the JVM with a higher memory footprint, but it has proven large conferences and can cascade bridges.
- **Asterisk:** the weakest for video. Audio mixing costs CPU per participant, and without simulcast a call's video quality is dragged toward its slowest viewer.

At chat-app sizes (2–50 people per call), any of the four dedicated SFUs has enough headroom. With several screen shares, the limit is each viewer's download bandwidth, so per-track quality and subscription control matter more than raw server throughput.

## Stability

- **Janus:** the most conservative and predictable core.
- **Jitsi:** stable at scale, but it is three cooperating services (Prosody, Jicofo, the bridge), so there are more operational failure points.
- **mediasoup:** the worker is solid; the stability of the product depends mostly on the signaling and room logic written around it.
- **LiveKit:** the fast release cadence brings occasional breaking changes (1.13.0 dropped a TURN authentication compatibility path). The company's focus is its cloud and AI agents, and some features are cloud-only, such as rooms spanning multiple servers.
- **Asterisk:** very stable as a telephony server with long-term-support releases. The video conferencing path is far less exercised; forum reports describe stream renegotiation problems and audio artifacts under packet loss.

## Fit for Chatney

- **LiveKit:**
  - **Backend:** it only issues a room token after a permission check and receives webhooks. The token is a JWT, and `System.IdentityModel.Tokens.Jwt` is already referenced.
  - **Frontend:** ready-made components exist for the React 19 app.
  - **Deployment:** one more container in the compose file.
  - **.NET SDK:** community-maintained (`Livekit.Server.Sdk.Dotnet`), not official.
- **mediasoup:** needs a Node.js or Rust sidecar service, since there is no .NET API, plus custom signaling over the existing WebSocket, coturn, and reconnection logic.
- **Janus:** the .NET backend can drive it over HTTP or WebSocket, but the client side is lower-level, and there are no first-party mobile SDKs if mobile apps are added later. GPLv3 is fine as a separate process, but check it against Chatney's own license if it is ever bundled.
- **Jitsi:** embedding it in a custom app means adopting XMPP and `lib-jitsi-meet`, the heaviest path for an app that already has its own users, permissions and signaling.
- **Asterisk:** worth choosing only if phone calls (PSTN, SIP trunks, desk phones) are a goal. For video-first group calls with several screen shares, the missing simulcast and per-viewer stream selection are the wrong trade.

## Not shortlisted

- **[OpenVidu 3](https://github.com/OpenVidu/openvidu):** a LiveKit-compatible distribution; worth a look only if its packaged deployment and monitoring are wanted.
- **[SIPSorcery](https://github.com/sipsorcery-org/sipsorcery):** the only native .NET WebRTC stack, actively maintained. It is a peer-connection library, not an SFU: it has a simple bandwidth-feedback controller for its own encoder, but no simulcast and no stream forwarding between participants.
- **[Pion](https://github.com/pion/webrtc) or [str0m](https://github.com/algesten/str0m):** libraries for building your own SFU, which is months of work to reach what LiveKit already does.
- **[Kurento](https://github.com/Kurento/kurento):** still maintained (7.3.0 in December 2025, commits this month), but it is a media-processing server built on GStreamer pipelines rather than a conferencing SFU, and OpenVidu, its former main user, moved to LiveKit.
- **[ion-sfu](https://github.com/ionorg/ion-sfu):** abandoned; last release in November 2021 and last commit in July 2023.
- **[Medooze](https://github.com/medooze/sfu):** the SFU repository was archived in 2022, and the main media-server repository is no longer public.

## Sources

- [WebRTC.ventures: open-source WebRTC media servers (June 2026)](https://webrtc.ventures/2026/06/open-source-webrtc-media-servers/)
- [Forasoft: SFU comparison](https://www.forasoft.com/learn/video-streaming/articles-streaming/sfu-comparison-mediasoup-janus-livekit-jitsi-pion)
- [LiveKit server changelog](https://releases.sh/livekit/livekit-server/changelog)
- [LiveKit self-hosting: distributed setup](https://docs.livekit.io/transport/self-hosting/distributed.md)
- [LiveKit self-hosting benchmarks](https://docs.livekit.io/transport/self-hosting/benchmark/)
- [LiveKit: cloud vs self-host](https://livekit.com/blog/livekit-cloud-vs-self-host)
- [LiveKit encryption docs](https://docs.livekit.io/transport/encryption/)
- [Livekit.Server.Sdk.Dotnet on NuGet](https://www.nuget.org/packages/livekit.server.sdk.dotnet)
- [mediasoup 3.20.0 announcement](https://mediasoup.discourse.group/t/mediasoup-3-20-0-released-and-rust-crates-with-new-built-in-sctp-stack/6974)
- [OpenVidu 3.8.0 release](https://openvidu.io/blog/2026/07/09/release-380/)
- [FOSDEM 2024: AV1/SVC in Janus](https://fosdem.org/schedule/event/fosdem-2024-3000-getting-av1-svc-to-work-in-the-janus-webrtc-server)
- [Janus forum: AV1 WHIP/WHEP report](https://janus.discourse.group/t/av1-whip-ingest-and-whep-playback/647)
- [Jitsi: how Jitsi Meet picks video codecs](https://jitsi.org/blog/av1-and-more-how-does-jitsi-meet-pick-video-codecs/)
- [mediasoup supported RTP capabilities (source)](https://raw.githubusercontent.com/versatica/mediasoup/v3/node/src/supportedRtpCapabilities.ts)
- [Janus pull request 3278: bandwidth estimation for subscribers](https://github.com/meetecho/janus-gateway/pull/3278)
- [Asterisk roadmap: Simulcast and SVC proposal](https://docs.asterisk.org/Development/Roadmap/Asterisk-15-Projects/Simulcast-and-SVC/)
- [Asterisk: transport-cc feedback](https://www.asterisk.org/transport-cc-improving-feedback-for-better-video-quality)
- [Asterisk forum: WebRTC renegotiation with ConfBridge](https://community.asterisk.org/t/webrtc-renegotiation-with-confbridge-not-working/92649)
- [Asterisk 24.0.0 release announcement](https://community.asterisk.org/t/asterisk-release-24-0-0/115045)
- [Asterisk 24.0.0 changelog](https://downloads.asterisk.org/pub/telephony/asterisk/ChangeLog-24.0.0.md)
- [Asterisk versions and support windows](https://docs.asterisk.org/About-the-Project/Asterisk-Versions/)
- [Asterisk WebSocket channel driver](https://docs.asterisk.org/Configuration/Channel-Drivers/WebSocket)
- [Asterisk issue: ICE-TCP candidates](https://issues-archive.asterisk.org/ASTERISK-24326)
- [LiveKit server APIs: room service](https://docs.livekit.io/reference/server/server-apis.md)
- [Janus VideoRoom plugin API](https://janus.conf.meetecho.com/docs/videoroom.html)
- [Asterisk ARI: Bridges REST API](https://docs.asterisk.org/Latest_API/API_Documentation/Asterisk_REST_Interface/Bridges_REST_API/)
- [Jitsi reservation system](https://jitsi.github.io/handbook/docs/devops-guide/reservation/)
- [LiveKit SDK platforms](https://docs.livekit.io/transport/sdk-platforms.md)
- [LiveKit self-hosting: ports and firewall](https://docs.livekit.io/transport/self-hosting/ports-firewall/)
- [Forasoft: Media over QUIC in 2026](https://www.forasoft.com/blog/article/media-over-quic-moq-streaming-2026)
- [CSI Magazine: MoQ at NAB 2026](https://csimagazine.com/csi/nab26-moq-update.php)
