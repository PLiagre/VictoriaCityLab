"""Budget, retry et plateau pour les invocations non supervisees."""

from __future__ import annotations

from dataclasses import dataclass, field


@dataclass
class Supervisor:
    max_iterations: int = 3
    plateau_limit: int = 2
    attempts: int = 0
    fingerprints: list[str] = field(default_factory=list)

    def register(self, fingerprint: str) -> str:
        self.attempts += 1
        self.fingerprints.append(fingerprint)
        if self.attempts >= self.max_iterations:
            return "STOP_BUDGET"
        if len(self.fingerprints) >= self.plateau_limit and len(set(self.fingerprints[-self.plateau_limit:])) == 1:
            return "STOP_PLATEAU"
        return "CONTINUE"

