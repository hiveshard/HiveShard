# Components

Hiveshard has many components.
SDKs like `shard.hot` and `shard.cold` are the foundations for specialized **shards**.
These can be hosted on **runtimes**.

Both `runtime.distributed` and `runtime.standalone` can host **shards** as well as **adapters**. 
The distributed runtime can be hosted manually or distributed on managed hiveshard clusters.

**Adapters** support common use cases and can be substituted by a variety of implementations.
For example backend physics can be utilized on a managed hiveshard cluster as well as 
natively on a standalone unity host. 
Some addons are open source, others are **not**. 
However, closed source addons are provided free of charge as docker containers.

```mermaid
flowchart TD
    subgraph consumer[closed source consumer project]
        hot1[hot package 1]
        hot2[hot package 2]
        cold1[cold package 2]
        cold2[cold package 2]
        unity[unity standalone client]
    end

    subgraph generated[generated distributed backend]
        c_hot[hot runtime]
        c_cold1[cold runtime 1]
        c_cold2[cold runtime 2]
    end

    subgraph open_hiveshard[open source hiveshard]
        cold[shard.cold]
        hot[shard.hot]
        hiveshard[hiveshard]
        runtime.standalone[runtime.standalone]
        runtime.distributed[runtime.distributed]
        unity_navigation[adapters.unity.navigation]
        unity_physics[adapters.unity.physics]
    end

    subgraph closed_hiveshard[closed source hiveshard]
        distributed_navigation[adapters.distributed.navigation]
        distributed_physics[adapters.distributed.physics]
    end


    hot --> hiveshard
    cold --> hiveshard
    runtime.standalone --> hiveshard
    runtime.distributed --> hiveshard
    unity_navigation --> hiveshard
    unity_physics --> hiveshard
    distributed_navigation --> hiveshard
    distributed_physics --> hiveshard

    hot1 --> hot
    hot2 --> hot
    cold1 --> cold
    cold2 --> cold
    unity --> runtime.standalone
    unity --> hot1
    unity --> hot2
    unity --> cold1
    unity --> cold2
    unity --> unity_navigation
    unity --> unity_physics

    c_hot --> hot1
    c_hot --> hot2
    c_hot --> runtime.distributed
    c_hot --> distributed_physics
    c_cold1 --> cold1
    c_cold1 --> runtime.distributed
    c_cold1 --> distributed_navigation
    c_cold2 --> cold2
    c_cold2 --> runtime.distributed
```