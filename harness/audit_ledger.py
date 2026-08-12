"""Journal append-only et machine-verifiable de la boucle d'audit CityLab."""

from __future__ import annotations

import argparse
import datetime as dt
import json
from pathlib import Path
from typing import Any, Iterable


TRANSITIONS: dict[str | None, set[str]] = {
    None: {"AUDIT_PROPOSED"},
    "AUDIT_PROPOSED": {"AUDIT_CHALLENGED", "AUDIT_STALE"},
    "AUDIT_CHALLENGED": {"AUDIT_APPROVED", "AUDIT_REJECTED"},
    "AUDIT_APPROVED": {"AUDIT_CONVERTED"},
    "AUDIT_CONVERTED": {"AUDIT_IMPLEMENTED"},
    "AUDIT_IMPLEMENTED": {"AUDIT_VERIFIED"},
    "AUDIT_VERIFIED": {"AUDIT_ARCHIVED"},
    "AUDIT_REJECTED": {"AUDIT_ARCHIVED"},
    "AUDIT_STALE": {"AUDIT_ARCHIVED"},
    "AUDIT_ARCHIVED": set(),
}


class LedgerError(RuntimeError):
    pass


def read_events(path: Path) -> list[dict[str, Any]]:
    if not path.exists():
        return []
    events: list[dict[str, Any]] = []
    for number, line in enumerate(path.read_text(encoding="utf-8").splitlines(), 1):
        if not line.strip():
            continue
        try:
            event = json.loads(line)
        except json.JSONDecodeError as exc:
            raise LedgerError(f"ligne {number} invalide: {exc}") from exc
        if not isinstance(event, dict):
            raise LedgerError(f"ligne {number}: objet JSON attendu")
        events.append(event)
    return events


def current_status(events: Iterable[dict[str, Any]], audit_id: str) -> str | None:
    status: str | None = None
    for event in events:
        if event.get("audit_id") == audit_id:
            status = str(event.get("event"))
    return status


def validate_events(events: Iterable[dict[str, Any]]) -> None:
    states: dict[str, str | None] = {}
    seen_ids: set[str] = set()
    for index, event in enumerate(events, 1):
        required = {"event_id", "audit_id", "event", "actor", "timestamp"}
        missing = sorted(required - event.keys())
        if missing:
            raise LedgerError(f"evenement {index}: champs absents: {', '.join(missing)}")
        event_id = str(event["event_id"])
        if event_id in seen_ids:
            raise LedgerError(f"event_id duplique: {event_id}")
        seen_ids.add(event_id)
        audit_id = str(event["audit_id"])
        previous = states.get(audit_id)
        next_event = str(event["event"])
        if next_event not in TRANSITIONS.get(previous, set()):
            raise LedgerError(
                f"transition interdite pour {audit_id}: {previous!r} -> {next_event!r}"
            )
        states[audit_id] = next_event


def append_event(
    path: Path,
    *,
    audit_id: str,
    event: str,
    actor: str,
    payload: dict[str, Any] | None = None,
) -> dict[str, Any]:
    events = read_events(path)
    previous = current_status(events, audit_id)
    if event not in TRANSITIONS.get(previous, set()):
        raise LedgerError(f"transition interdite: {previous!r} -> {event!r}")
    timestamp = dt.datetime.now(dt.UTC).isoformat().replace("+00:00", "Z")
    record = {
        "event_id": f"{audit_id}:{len(events) + 1:04d}:{event}",
        "audit_id": audit_id,
        "event": event,
        "actor": actor,
        "timestamp": timestamp,
        "payload": payload or {},
    }
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("a", encoding="utf-8", newline="\n") as stream:
        stream.write(json.dumps(record, ensure_ascii=False, sort_keys=True) + "\n")
    validate_events(read_events(path))
    return record


def main() -> int:
    parser = argparse.ArgumentParser()
    parser.add_argument("ledger", type=Path)
    parser.add_argument("--validate", action="store_true")
    args = parser.parse_args()
    if args.validate:
        events = read_events(args.ledger)
        validate_events(events)
        print(f"CITYLAB_AUDIT_LEDGER_OK events={len(events)}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())

