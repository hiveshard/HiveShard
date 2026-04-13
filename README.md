# HiveShard

[![codecov](https://codecov.io/gh/hiveshard/HiveShard/graph/badge.svg?token=KOI7AGCISI)](https://codecov.io/gh/hiveshard/HiveShard)

HiveShard is a distributed runtime for deterministic simulations that require partitioning of 2D space.
The goal is to support 100M human actors simultaneously (connected via client endpoints)
AND billions of simulated entities in ONE consistent 2D world.

HiveShard SDK provides a domain focused, event driven API for specifying logic components (HiveShards)
that are then orchestrated onto every single partition (chunk) by the runtime.
Each HiveShard instance holds its own state in memory and Information is exchanged between HiveShards via propagation.

![HiveShard instances (classes A, B and C) distributed in a grid of 2D space](./media/shardgrid.svg)

HiveShard clients are only connected to nearby chunks via multiple stateful TCP connections.
Server-side client endpoints (called Edges) can be scaled up horizontally as they are separated from the core simulation.

## FAQs
Can a single client stall the simulation for others?
No. Edge components send server-authoritative state to the simulation and control what client input goes into that state.

Can a single faulty simulation component stall the entire simulation region-wide?
No. Each component is replicated about 5 times. The simulation waits for 3 of these to be in consensus about correctness before continuing. If any instance stalls because of physical abnormalities, any set of 3 other replicas is going to subsidize the faulty one.

Can the simulation break down due total fault of an entire (hiveshard,chunk) logic component (all replicas)
In such rare cases, the simulation would stall globally until a chorum of instances has rebooted from checkpoints and caught up with the stream.