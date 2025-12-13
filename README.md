# Hiveshard


## Consistency Model

**Atomic Total-Order Broadcast** with **Exactly-Once Semantics**

- **Global total order**
  - Enforced via lockstep tick-driven consumption across all topics.
  - Each tick aggregates completed offsets from all consumers and calculates a new authoritative order.

- **Durable sequencing**
  - Ticks and checkpoints are stored in Kafka topics.
  - Consumers only proceed once the checkpoint/tick is durably committed.

- **Exactly-once effect**
  - Each message is unique and applied deterministically.
  - Per-message type max-tick tracking ensures duplicate suppression during consumption and replay.
  - Replays use the same message sequence as the live system, preserving state determinism.

- **Replication & high availability**
  - Each consumer has 3 HA replicas.
  - Only the first responsive replica per tick is needed to progress.
  - Duplicate messages from other replicas are ignored safely.

- **Partition tolerance**
  - Kafka provides central durability and replication.
  - Only the partition able to reach Kafka can make progress; isolated partitions cannot diverge.

- **Recovery**
  - Checkpoints allow crashed replicas to recover from the last committed state.
  - Deterministic replay up to the latest tick restores full system state.

- **Atomic broadcast semantics**
  - All correct replicas observe and apply messages in the same total order.
  - Messages are never applied out of order and are not lost or duplicated in effect.
