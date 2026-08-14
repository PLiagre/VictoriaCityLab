# City Mode asset host

Disposable Unity 6000.0.43f1 host for `M3-FH-06`. It imports only URP, Unity
Test Framework and `com.victoria.citymode.assets`. Its four scenes prove the
host-owned `common -> biome -> city` load order and reverse release without any
CityLab simulation, save, fixture or ForgeHistory source.

The setup, tests and opt-in player probe generate the build, three zoom captures
and resident-memory measurements used by `Docs/VALIDATION.md`.
