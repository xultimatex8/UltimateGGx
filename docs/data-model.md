# Data Model — UltimateGGx

This document describes the core entities of UltimateGGx and how they relate to each other.

## Diagram

```mermaid
---
config:
  layout: elk
---
classDiagram
direction TB
  class Summoner {
    puuid: string
    username: string
    tag: string
    level: int
    profileIconId: int
  }

  class Queue {
    tier: string
    rank: string
    points: int
    wins: int
    losses: int
    type: QueueType
  }

  class Champion {
    key: int
    name: string
    roles: string[]
  }

  class SummonerSpell {
    key: int
    name: string
  }

  class Match {
    matchId: string
    endOfGameResult: string
    gameDuration: long
    gameEndTimestamp: long
    queueType: QueueType
  }

  class Team {
    win: boolean
    teamId: int
  }

  class Participant {
    assists: int
    championLevel: int
    deaths: int
    gold: int
    items: int[]
    kills: int
    lane: string
    primaryRune: int
    secondaryTree: int
    damageToChampions: int
  }

  class ParticipantFrame {
    gold: int
    level: int
    timestamp: long
    positionX: int
    positionY: int
  }

  class Event {
    timestamp: long
    bounty: int
    shutdownBounty: int
    monsterType: string
    monsterSubType: string
    buildingType: string
    laneType: string
    towertype: string
    type: EventType
  }

  class QueueType {
    DRAFT_PICK
    RANKED_SOLO
    RANKED_FLEX
  }

  class EventType {
    CHAMPION_KILL
    CHAMPION_SPECIAL_KILL
    ELITE_MONSTER_KILL
    BUILDING_KILL
  }

  <<Enum>> QueueType
  <<Enum>> EventType

  Summoner "1" --> "0..*" Queue : has ranked stats in
  Summoner "1" --> "0..*" Participant : plays as
  Match "1" --> "2" Team : is composed of
  Team "1" --> "5" Participant : is made up of
  Participant "0..*" --> "1" Champion : is
  Participant "0..*" --> "2" SummonerSpell : has
  Participant "1" --> "0..*" ParticipantFrame : has
  Match "1" --> "0..*" Event : contains
  Event "0..*" --> "0..1" Participant : killed by
  Event "0..*" --> "0..1" Participant : victim is
  Event "0..*" --> "0..*" Participant : assisted by
```

## Entities

**Summoner** — a Riot account, identified by its `puuid`.

**Queue** — a summoner's ranked standing in a specific queue type. A summoner can have one `Queue` entry per `QueueType` they've played.

**Champion / SummonerSpell** — reference catalogs, not tied to a specific match.

**Match** — a single game, identified by Riot's `matchId`. Holds match-level metadata (duration, end result, queue type).

**Team** — one of the two sides in a match (blue/red).

**Participant** — a summoner's performance within one specific match (kills, gold, items, etc.). Distinct from `Summoner` because the same person plays many matches, each producing its own `Participant` record.

**ParticipantFrame** — a snapshot of a participant's state at a point in time (gold, level, position).

**Event** — a discrete moment in the match (a kill, an objective taken, a tower falling).

## Design notes

- **`Event.killer` / `Event.victim`** are both optional relations to `Participant`, because not every event has a clear killer or victim (e.g. a tower destroyed by minions has neither).

- Fields like `monsterType`, `buildingType`, `laneType` are only populated depending on `Event.type` — for example, `monsterType` is only relevant for `ELITE_MONSTER_KILL` events.