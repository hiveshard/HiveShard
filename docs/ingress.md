# Ingress

Ingress is a required conponent, 
that routes regional client traffic
to a cluster.

The cluster destination might be a regional cluster,
an overseas cluster or both during chunk migration.
Cluster selection is based on state, 
that the ingress component receives asynchronously
via cold path.

## Ingress-Client communication

Ingress, beeing a hot path component,
requires highly available low latency connections
with the client.
To gurantee delivery and latency requirements,
webrtc is used as a valid socket based solution.
Messages are serialized, routed, etc -- 
by the hiveshard client sdks.

In case of an outage, 
connections need to immediately fail over
to a spare redundant instance.

## Ingress-Cluster communication

Ingress forwards client messages
to the hot path message bus (nats jetstream).
Multiple cluster enspoints might be selected delivered to
during chink switchover.
The topics where messages are delivered to
are chunk specific.
Ingress routes client messages to a topic 
based on client chunk 
(authorative chunk information is received
asynchronously via cold path)
